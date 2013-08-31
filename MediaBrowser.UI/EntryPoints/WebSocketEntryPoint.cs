using MediaBrowser.ApiInteraction;
using MediaBrowser.ApiInteraction.WebSocket;
using MediaBrowser.Common;
using MediaBrowser.Model.ApiClient;
using MediaBrowser.Model.Dto;
using MediaBrowser.Model.Logging;
using MediaBrowser.Model.Net;
using MediaBrowser.Model.Querying;
using MediaBrowser.Model.Serialization;
using MediaBrowser.Theater.Interfaces.Navigation;
using MediaBrowser.Theater.Interfaces.Playback;
using MediaBrowser.Theater.Interfaces.Presentation;
using MediaBrowser.Theater.Interfaces.Session;
using System;
using System.Linq;
using System.Threading;

namespace MediaBrowser.UI.EntryPoints
{
    public class WebSocketEntryPoint : IStartupEntryPoint, IDisposable
    {
        private readonly ISessionManager _session;
        private readonly IApiClient _apiClient;
        private readonly ILogger _logger;
        private readonly IJsonSerializer _json;
        private readonly IApplicationHost _appHost;
        private readonly INavigationService _nav;
        private readonly IPlaybackManager _playbackManager;

        private ApiWebSocket _apiWebSocket;

        public WebSocketEntryPoint(ISessionManager session, IApiClient apiClient, IJsonSerializer json, ILogger logger, IApplicationHost appHost, INavigationService nav, IPlaybackManager playbackManager)
        {
            _session = session;
            _apiClient = apiClient;
            _json = json;
            _logger = logger;
            _appHost = appHost;
            _nav = nav;
            _playbackManager = playbackManager;
        }

        public void Run()
        {
            _session.UserLoggedIn += _session_UserLoggedIn;
            _apiClient.ServerLocationChanged += _apiClient_ServerLocationChanged;
            EnsureWebSocket();
        }

        void _apiClient_ServerLocationChanged(object sender, EventArgs e)
        {
            ReloadWebSocket();
        }

        void _session_UserLoggedIn(object sender, EventArgs e)
        {
            EnsureWebSocket();
        }

        private void EnsureWebSocket()
        {
            if (_apiWebSocket == null || !_apiWebSocket.IsOpen)
            {
                ReloadWebSocket();
            }
        }

        private async void ReloadWebSocket()
        {
            try
            {
                var systemInfo = await _apiClient.GetSystemInfoAsync().ConfigureAwait(false);

                var socket = new ApiWebSocket(_logger, _json, _apiClient.ServerHostName, systemInfo.WebSocketPortNumber,
                                              _apiClient.DeviceId, _appHost.ApplicationVersion.ToString(),
                                              _apiClient.ClientName, ClientWebSocketFactory.CreateWebSocket);

                await socket.ConnectAsync(CancellationToken.None).ConfigureAwait(false);

                OnWebSocketInitialized(socket);

                _logger.Info("Web socket connection established.");
            }
            catch (Exception ex)
            {
                _logger.ErrorException("Error establishing web socket connection", ex);
            }
        }

        private void OnWebSocketInitialized(ApiWebSocket socket)
        {
            var previousSocket = _apiWebSocket;

            _apiWebSocket = socket;

            ((ApiClient)_apiClient).WebSocketConnection = socket;

            socket.BrowseCommand += _apiWebSocket_BrowseCommand;
            socket.UserDeleted += _apiWebSocket_UserDeleted;
            socket.UserUpdated += _apiWebSocket_UserUpdated;
            socket.PlaystateCommand += _apiWebSocket_PlaystateCommand;
            socket.PlayCommand += _apiWebSocket_PlayCommand;

            socket.StartEnsureConnectionTimer(5000);

            if (previousSocket != null)
            {
                previousSocket.Dispose();
            }
        }

        async void _apiWebSocket_PlayCommand(object sender, PlayRequestEventArgs e)
        {
            if (_session.CurrentUser == null)
            {
                _logger.Error("Cannot process remote control command without a logged in user.");
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
                _logger.Error("Cannot process remote control command without a logged in user.");
                return;
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
                _logger.Error("Cannot process remote control command without a logged in user.");
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

                await _nav.NavigateToItem(dto, e.Request.Context);
            }
            catch (HttpException ex)
            {
                _logger.ErrorException("Error processing browse command", ex);
            }
        }

        public void Dispose()
        {
            _session.UserLoggedIn -= _session_UserLoggedIn;
        }
    }
}
