using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows;
using MediaBrowser.ApiInteraction.WebSocket;
using MediaBrowser.Common;
using MediaBrowser.Model.ApiClient;
using MediaBrowser.Model.Dto;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.Events;
using MediaBrowser.Model.Logging;
using MediaBrowser.Model.Querying;
using MediaBrowser.Model.Serialization;
using MediaBrowser.Model.Session;
using MediaBrowser.Model.System;
using MediaBrowser.Theater.Api.Events;
using MediaBrowser.Theater.Api.Navigation;
using MediaBrowser.Theater.Api.Playback;
using MediaBrowser.Theater.Api.Session;
using MediaBrowser.Theater.Api.System;
using MediaBrowser.Theater.Api.UserInterface;

namespace MediaBrowser.Theater.EntryPoints
{
    public class WebSocketEntryPoint : IStartupEntryPoint, IDisposable
    {
        private readonly IApiClient _apiClient;
        private readonly ApplicationHost _appHost;
        private readonly IJsonSerializer _json;
        private readonly ILogger _logger;
        private readonly INavigator _nav;
        private readonly IPlaybackManager _playbackManager;
        private readonly IPresenter _presentation;
        private readonly IEventAggregator _events;
        private readonly ISessionManager _session;

        private bool _isDisposed;

        public WebSocketEntryPoint(ISessionManager session, IApiClient apiClient, IJsonSerializer json, ILogManager logManager, IApplicationHost appHost, INavigator nav, IPlaybackManager playbackManager, IPresenter presentation, IEventAggregator events)
        {
            _session = session;
            _apiClient = apiClient;
            _json = json;
            _logger = logManager.GetLogger(GetType().Name);
            _appHost = (ApplicationHost) appHost;
            _nav = nav;
            _playbackManager = playbackManager;
            _presentation = presentation;
            _events = events;
        }

        private ApiWebSocket ApiWebSocket
        {
            get { return _appHost.ApiWebSocket; }
        }

        public void Dispose()
        {
            _isDisposed = true;
            _session.UserLoggedIn -= _session_UserLoggedIn;
            DisposeSocket();
        }

        public void Run()
        {
            _session.UserLoggedIn += _session_UserLoggedIn;
            _apiClient.ServerLocationChanged += _apiClient_ServerLocationChanged;
            
            _events.Get<PageLoadedEvent>().Subscribe(PageLoaded);

            ApiWebSocket socket = ApiWebSocket;

            socket.BrowseCommand += _apiWebSocket_BrowseCommand;
            socket.UserDeleted += _apiWebSocket_UserDeleted;
            socket.UserUpdated += _apiWebSocket_UserUpdated;
            socket.PlaystateCommand += _apiWebSocket_PlaystateCommand;
            socket.GeneralCommand += socket_GeneralCommand;
            socket.MessageCommand += socket_MessageCommand;
            socket.PlayCommand += _apiWebSocket_PlayCommand;
            socket.Closed += socket_Closed;
            socket.Connected += socket_Connected;

            UpdateServerLocation();
        }

        private void socket_Connected(object sender, EventArgs e)
        {
            ReportCapabilities(CancellationToken.None);
        }

        private async void ReportCapabilities(CancellationToken cancellationToken)
        {
            try {
                await _apiClient.ReportCapabilities(new ClientCapabilities {
                    PlayableMediaTypes = new List<string> {
                        MediaType.Audio,
                        MediaType.Video,
                        MediaType.Game,
                        MediaType.Photo,
                        MediaType.Book
                    },

                    // MBT should be able to implement them all
                    SupportedCommands = Enum.GetNames(typeof (GeneralCommandType)).ToList()
                }, cancellationToken);
            }
            catch (Exception ex) {
                _logger.ErrorException("Error reporting capabilities", ex);
            }
        }

        private void _apiClient_ServerLocationChanged(object sender, EventArgs e)
        {
            UpdateServerLocation();
        }

        private async void UpdateServerLocation()
        {
            try {
                SystemInfo systemInfo = await _apiClient.GetSystemInfoAsync(CancellationToken.None).ConfigureAwait(false);

                ApiWebSocket socket = ApiWebSocket;

                await socket.ChangeServerLocation(_apiClient.ServerHostName, systemInfo.WebSocketPortNumber, CancellationToken.None).ConfigureAwait(false);

                socket.StartEnsureConnectionTimer(5000);
            }
            catch (Exception ex) {
                _logger.ErrorException("Error establishing web socket connection", ex);
            }
        }

        private void _session_UserLoggedIn(object sender, EventArgs e)
        {
            if (_isDisposed) {
                return;
            }

            EnsureWebSocket();
        }

        private void EnsureWebSocket()
        {
            ApiWebSocket.EnsureConnectionAsync(CancellationToken.None);
        }

        private void socket_Closed(object sender, EventArgs e)
        {
            EnsureWebSocket();
        }

        private void socket_MessageCommand(object sender, GenericEventArgs<MessageCommand> e)
        {
            _presentation.ShowMessage(new MessageBoxInfo {
                Button = MessageBoxButton.OK,
                Caption = e.Argument.Header,
                Text = e.Argument.Text,
                TimeoutMs = Convert.ToInt32(e.Argument.TimeoutMs ?? 0)
            });
        }

        private void socket_GeneralCommand(object sender, GeneralCommandEventArgs e)
        {
            if (e.KnownCommandType.HasValue) {
                switch (e.KnownCommandType.Value) {
                    case GeneralCommandType.GoHome:
                        _nav.Navigate(Go.To.Home());
                        break;
                    case GeneralCommandType.GoToSettings:
                        _nav.Navigate(Go.To.Settings());
                        break;
                    case GeneralCommandType.Mute:
                        _playbackManager.Mute();
                        break;
                    case GeneralCommandType.Unmute:
                        _playbackManager.UnMute();
                        break;
                    case GeneralCommandType.ToggleMute:
                        if (_playbackManager.IsMuted) {
                            _playbackManager.UnMute();
                        } else {
                            _playbackManager.Mute();
                        }
                        break;
                    case GeneralCommandType.VolumeDown:
                        _playbackManager.VolumeStepDown();
                        break;
                    case GeneralCommandType.VolumeUp:
                        _playbackManager.VolumeStepUp();
                        break;
                    default:
                        _logger.Warn("Unrecognized command: " + e.KnownCommandType.Value);
                        break;
                }
            }
        }

        private async void _apiWebSocket_PlayCommand(object sender, GenericEventArgs<PlayRequest> e)
        {
            if (_session.CurrentUser == null) {
                OnAnonymousRemoteControlCommand();
                return;
            }

            try {
                ItemsResult result = await _apiClient.GetItemsAsync(new ItemQuery {
                    Ids = e.Argument.ItemIds,
                    UserId = _session.CurrentUser.Id,
                    Fields = new[] {
                        ItemFields.Chapters,
                        ItemFields.MediaStreams,
                        ItemFields.Overview,
                        ItemFields.Path,
                        ItemFields.People
                    }
                });

                await _playbackManager.Play(new PlayOptions {
                    StartPositionTicks = e.Argument.StartPositionTicks ?? 0,
                    GoFullScreen = true,
                    Items = result.Items.ToList()
                });
            }
            catch (Exception ex) {
                _logger.ErrorException("Error processing play command", ex);
            }
        }

        private void _apiWebSocket_PlaystateCommand(object sender, GenericEventArgs<PlaystateRequest> e)
        {
            if (_session.CurrentUser == null) {
                OnAnonymousRemoteControlCommand();
                return;
            }

            IMediaPlayer player = _playbackManager.MediaPlayers
                                                  .FirstOrDefault(i => i.PlayState != PlayState.Idle);

            if (player == null) {
                return;
            }

            PlaystateRequest request = e.Argument;

            switch (request.Command) {
                case PlaystateCommand.Pause:
                    player.Pause();
                    break;
                case PlaystateCommand.Stop:
                    player.Stop();
                    break;
                case PlaystateCommand.Unpause:
                    player.UnPause();
                    break;
                case PlaystateCommand.Seek:
                    player.Seek(e.Argument.SeekPositionTicks ?? 0);
                    break;
                case PlaystateCommand.PreviousTrack: {
                    player.GoToPreviousTrack();
                    break;
                }
                case PlaystateCommand.NextTrack: {
                    player.GoToNextTrack();
                    break;
                }
            }
        }

        private void _apiWebSocket_UserUpdated(object sender, GenericEventArgs<UserDto> e) { }

        private async void _apiWebSocket_UserDeleted(object sender, GenericEventArgs<string> e)
        {
            if (_session.CurrentUser != null && string.Equals(e.Argument, _session.CurrentUser.Id)) {
                await _session.Logout();
            }
        }

        private async void _apiWebSocket_BrowseCommand(object sender, GenericEventArgs<BrowseRequest> e)
        {
            if (_session.CurrentUser == null) {
                OnAnonymousRemoteControlCommand();
                return;
            }

            try {
                BaseItemDto dto = await _apiClient.GetItemAsync(e.Argument.ItemId, _session.CurrentUser.Id);
                await _nav.Navigate(Go.To.Item(dto));
            }
            catch (Exception ex) {
                _logger.ErrorException("Error processing browse command", ex);
            }
        }

        private void OnAnonymousRemoteControlCommand()
        {
            _logger.Error("Cannot process remote control command without a logged in user.");
            _presentation.ShowMessage(new MessageBoxInfo {
                Button = MessageBoxButton.OK,
                Caption = "Error",
                Icon = MessageBoxIcon.Error,
                Text = "Please sign in before attempting to use remote control"
            });
        }

        async void PageLoaded(PageLoadedEvent e)
        {
            var itemPage = e.ViewModel as IItemDetailsViewModel;

            if (itemPage != null)
            {
                var item = itemPage.Item;

                try
                {
                    await ApiWebSocket.SendContextMessageAsync(item.Type, item.Id, item.Name, "Folder", CancellationToken.None).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    _logger.ErrorException("Error sending context message", ex);
                }
            }
        }

        private void DisposeSocket()
        {
            ApiWebSocket socket = ApiWebSocket;

            if (socket != null) {
                socket.BrowseCommand -= _apiWebSocket_BrowseCommand;
                socket.UserDeleted -= _apiWebSocket_UserDeleted;
                socket.UserUpdated -= _apiWebSocket_UserUpdated;
                socket.PlaystateCommand -= _apiWebSocket_PlaystateCommand;
                socket.GeneralCommand -= socket_GeneralCommand;
                socket.MessageCommand -= socket_MessageCommand;
                socket.PlayCommand -= _apiWebSocket_PlayCommand;
                socket.Closed -= socket_Closed;
                socket.Connected -= socket_Connected;

                socket.Dispose();
            }
        }
    }
}