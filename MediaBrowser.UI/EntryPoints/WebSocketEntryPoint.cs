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

        private volatile Timer _webSocketTimer;
        private readonly object _timerLock = new object();

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
        }

        void _session_UserLoggedIn(object sender, EventArgs e)
        {
            EnsureWebSocket();
        }

        private async void EnsureWebSocket()
        {
            if (_apiWebSocket == null || !_apiWebSocket.IsOpen)
            {
                try
                {
                    var systemInfo = await _apiClient.GetSystemInfoAsync().ConfigureAwait(false);

                    var socket = new ApiWebSocket(ClientWebSocketFactory.CreateWebSocket(), _logger, _json);

                    await socket.ConnectAsync(_apiClient.ServerHostName, systemInfo.WebSocketPortNumber, _apiClient.ClientName, _apiClient.DeviceId, _appHost.ApplicationVersion.ToString(), CancellationToken.None).ConfigureAwait(false);

                    _apiWebSocket = socket;

                    ((ApiClient)_apiClient).WebSocketConnection = _apiWebSocket;

                    OnWebSocketInitialized();

                    StartWebSocketTimer();

                    _logger.Info("Web socket connection established.");
                }
                catch (Exception ex)
                {
                    _logger.ErrorException("Error establishing web socket connection", ex);
                }
            }
        }

        private void OnWebSocketInitialized()
        {
            _apiWebSocket.BrowseCommand += _apiWebSocket_BrowseCommand;
            _apiWebSocket.UserDeleted += _apiWebSocket_UserDeleted;
            _apiWebSocket.UserUpdated += _apiWebSocket_UserUpdated;
            _apiWebSocket.PlaystateCommand += _apiWebSocket_PlaystateCommand;
            _apiWebSocket.PlayCommand += _apiWebSocket_PlayCommand;
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

            DisposeTimerLock();
        }

        private void StartWebSocketTimer()
        {
            if (_webSocketTimer == null)
            {
                lock (_timerLock)
                {
                    if (_webSocketTimer == null)
                    {
                        _webSocketTimer = new Timer(OnWebSocketTimerFired, null, Timeout.Infinite, 30000);
                    }
                }
            }
        }

        private void OnWebSocketTimerFired(object state)
        {
            EnsureWebSocket();
        }

        private void DisposeTimerLock()
        {
            lock (_timerLock)
            {
                if (_webSocketTimer != null)
                {
                    _webSocketTimer.Dispose();
                    _webSocketTimer = null;
                }
            }
        }

    }
}
