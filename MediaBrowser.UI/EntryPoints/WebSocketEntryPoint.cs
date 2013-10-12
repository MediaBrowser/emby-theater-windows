using MediaBrowser.ApiInteraction;
using MediaBrowser.ApiInteraction.WebSocket;
using MediaBrowser.Common;
using MediaBrowser.Model.ApiClient;
using MediaBrowser.Model.Dto;
using MediaBrowser.Model.Logging;
using MediaBrowser.Model.Querying;
using MediaBrowser.Model.Serialization;
using MediaBrowser.Model.Session;
using MediaBrowser.Theater.Interfaces.Navigation;
using MediaBrowser.Theater.Interfaces.Playback;
using MediaBrowser.Theater.Interfaces.Presentation;
using MediaBrowser.Theater.Interfaces.Session;
using MediaBrowser.Theater.Interfaces.Theming;
using MediaBrowser.Theater.Presentation.Playback;
using System;
using System.Linq;
using System.Threading;
using System.Windows;

namespace MediaBrowser.UI.EntryPoints
{
    public class WebSocketEntryPoint : IStartupEntryPoint, IDisposable
    {
        private readonly ISessionManager _session;
        private readonly IApiClient _apiClient;
        private readonly ILogger _logger;
        private readonly IJsonSerializer _json;
        private readonly ApplicationHost _appHost;
        private readonly INavigationService _nav;
        private readonly IPlaybackManager _playbackManager;
        private readonly IPresentationManager _presentation;
        private readonly ILogManager _logManager;

        private bool _isDisposed;

        private ApiWebSocket ApiWebSocket
        {
            get { return _appHost.ApiWebSocket; }
            set
            {
                _appHost.ApiWebSocket = value;
            }
        }

        public WebSocketEntryPoint(ISessionManager session, IApiClient apiClient, IJsonSerializer json, ILogManager logManager, IApplicationHost appHost, INavigationService nav, IPlaybackManager playbackManager, IPresentationManager presentation)
        {
            _session = session;
            _apiClient = apiClient;
            _json = json;
            _logger = logManager.GetLogger(GetType().Name);
            _appHost = (ApplicationHost)appHost;
            _nav = nav;
            _playbackManager = playbackManager;
            _presentation = presentation;
            _logManager = logManager;
        }

        public void Run()
        {
            _session.UserLoggedIn += _session_UserLoggedIn;
            _apiClient.ServerLocationChanged += _apiClient_ServerLocationChanged;
            _nav.Navigated += _nav_Navigated;
            EnsureWebSocket();
        }

        void _apiClient_ServerLocationChanged(object sender, EventArgs e)
        {
            ReloadWebSocket();
        }

        void _session_UserLoggedIn(object sender, EventArgs e)
        {
            if (_isDisposed)
            {
                return;
            }

            EnsureWebSocket();
        }

        private void EnsureWebSocket()
        {
            var socket = ApiWebSocket;

            if (socket == null)
            {
                ReloadWebSocket();
            }
            else
            {
                socket.EnsureConnectionAsync(CancellationToken.None);
            }
        }

        private async void ReloadWebSocket()
        {
            DisposeSocket();
            
            try
            {
                var systemInfo = await _apiClient.GetSystemInfoAsync().ConfigureAwait(false);

                var socketLogger = _logManager.GetLogger(typeof (ApiWebSocket).Name);

                var socket = new ApiWebSocket(socketLogger, _json, _apiClient.ServerHostName, systemInfo.WebSocketPortNumber,
                                              _apiClient.DeviceId, _appHost.ApplicationVersion.ToString(),
                                              _apiClient.ClientName, _apiClient.DeviceName, () => ClientWebSocketFactory.CreateWebSocket(socketLogger));

                OnWebSocketInitialized(socket);

                await socket.ConnectAsync(CancellationToken.None).ConfigureAwait(false);

                _logger.Info("Web socket connection established.");
            }
            catch (Exception ex)
            {
                _logger.ErrorException("Error establishing web socket connection", ex);
            }
        }

        private void OnWebSocketInitialized(ApiWebSocket socket)
        {
            ApiWebSocket = socket;

            ((ApiClient)_apiClient).WebSocketConnection = socket;

            socket.BrowseCommand += _apiWebSocket_BrowseCommand;
            socket.UserDeleted += _apiWebSocket_UserDeleted;
            socket.UserUpdated += _apiWebSocket_UserUpdated;
            socket.PlaystateCommand += _apiWebSocket_PlaystateCommand;
            socket.SystemCommand += socket_SystemCommand;
            socket.MessageCommand += socket_MessageCommand;
            socket.PlayCommand += _apiWebSocket_PlayCommand;
            socket.Closed += socket_Closed;

            socket.StartEnsureConnectionTimer(5000);
        }

        void socket_Closed(object sender, EventArgs e)
        {
            EnsureWebSocket();
        }

        void socket_MessageCommand(object sender, MessageCommandEventArgs e)
        {
            _presentation.ShowMessage(new MessageBoxInfo
            {
                Button = MessageBoxButton.OK,
                Caption = e.Request.Header,
                Text = e.Request.Text,
                TimeoutMs = Convert.ToInt32(e.Request.TimeoutMs ?? 0)
            });
        }

        void socket_SystemCommand(object sender, SystemCommandEventArgs e)
        {
            switch (e.Command)
            {
                case SystemCommand.GoHome:
                    _nav.NavigateToHomePage();
                    break;
                case SystemCommand.GoToSettings:
                    _nav.NavigateToSettingsPage();
                    break;
                case SystemCommand.Mute:
                    _playbackManager.Mute();
                    break;
                case SystemCommand.Unmute:
                    _playbackManager.UnMute();
                    break;
                case SystemCommand.ToggleMute:
                    if (_playbackManager.IsMuted)
                    {
                        _playbackManager.UnMute();
                    }
                    else
                    {
                        _playbackManager.Mute();
                    }
                    break;
                case SystemCommand.VolumeDown:
                    _playbackManager.VolumeStepDown();
                    break;
                case SystemCommand.VolumeUp:
                    _playbackManager.VolumeStepUp();
                    break;
            }
        }

        async void _apiWebSocket_PlayCommand(object sender, PlayRequestEventArgs e)
        {
            if (_session.CurrentUser == null)
            {
                OnAnonymousRemoteControlCommand();
                return;
            }

            try
            {
                var result = await _apiClient.GetItemsAsync(new ItemQuery
                {
                    Ids = e.Request.ItemIds,
                    UserId = _session.CurrentUser.Id,

                    Fields = new[] { ItemFields.Chapters, ItemFields.MediaStreams, ItemFields.Path }
                });

                await _playbackManager.Play(new PlayOptions
                {
                    StartPositionTicks = e.Request.StartPositionTicks ?? 0,
                    GoFullScreen = true,
                    Items = result.Items.ToList()
                });
            }
            catch (Exception ex)
            {
                _logger.ErrorException("Error processing play command", ex);
            }
        }

        void _apiWebSocket_PlaystateCommand(object sender, PlaystateRequestEventArgs e)
        {
            if (_session.CurrentUser == null)
            {
                OnAnonymousRemoteControlCommand();
                return;
            }

            var player = _playbackManager.MediaPlayers
              .FirstOrDefault(i => i.PlayState != PlayState.Idle);

            if (player == null)
            {
                return;
            }

            var request = e.Request;

            switch (request.Command)
            {
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
                    player.Seek(e.Request.SeekPositionTicks ?? 0);
                    break;
                case PlaystateCommand.PreviousTrack:
                    {
                        player.GoToPreviousTrack();
                        break;
                    }
                case PlaystateCommand.NextTrack:
                    {
                        player.GoToNextTrack();
                        break;
                    }
            }
        }

        void _apiWebSocket_UserUpdated(object sender, UserUpdatedEventArgs e)
        {
        }

        async void _apiWebSocket_UserDeleted(object sender, UserDeletedEventArgs e)
        {
            if (_session.CurrentUser != null && string.Equals(e.Id, _session.CurrentUser.Id))
            {
                await _session.Logout();
            }
        }

        async void _apiWebSocket_BrowseCommand(object sender, BrowseRequestEventArgs e)
        {
            if (_session.CurrentUser == null)
            {
                OnAnonymousRemoteControlCommand();
                return;
            }

            try
            {
                var dto = new BaseItemDto
                {
                    Name = e.Request.ItemName,
                    Type = e.Request.ItemType,
                    Id = e.Request.ItemId
                };

                var viewType = ViewType.Folders;

                if (!string.IsNullOrEmpty(e.Request.Context))
                {
                    Enum.TryParse(e.Request.Context, true, out viewType);
                }
                await _nav.NavigateToItem(dto, viewType);
            }
            catch (Exception ex)
            {
                _logger.ErrorException("Error processing browse command", ex);
            }
        }

        private void OnAnonymousRemoteControlCommand()
        {
            _logger.Error("Cannot process remote control command without a logged in user.");
            _presentation.ShowMessage(new MessageBoxInfo
            {
                Button = MessageBoxButton.OK,
                Caption = "Error",
                Icon = MessageBoxIcon.Error,
                Text = "Please sign in before attempting to use remote control"
            });
        }

        async void _nav_Navigated(object sender, NavigationEventArgs e)
        {
            var itemPage = e.NewPage as IItemPage;

            if (itemPage != null)
            {
                var item = itemPage.PageItem;

                try
                {
                    await ApiWebSocket.SendContextMessageAsync(item.Type, item.Id, item.Name, itemPage.ViewType.ToString(), CancellationToken.None).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    _logger.ErrorException("Error sending context message", ex);
                }
            }
        }

        public void Dispose()
        {
            _isDisposed = true;
            _session.UserLoggedIn -= _session_UserLoggedIn;
            DisposeSocket();
        }

        private void DisposeSocket()
        {
            var socket = ApiWebSocket;

            if (socket != null)
            {
                socket.BrowseCommand -= _apiWebSocket_BrowseCommand;
                socket.UserDeleted -= _apiWebSocket_UserDeleted;
                socket.UserUpdated -= _apiWebSocket_UserUpdated;
                socket.PlaystateCommand -= _apiWebSocket_PlaystateCommand;
                socket.SystemCommand -= socket_SystemCommand;
                socket.MessageCommand -= socket_MessageCommand;
                socket.PlayCommand -= _apiWebSocket_PlayCommand;
                socket.Closed -= socket_Closed;

                socket.Dispose();
            }
        }
    }
}
