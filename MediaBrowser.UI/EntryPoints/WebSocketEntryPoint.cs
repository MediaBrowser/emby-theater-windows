using MediaBrowser.ApiInteraction;
using MediaBrowser.ApiInteraction.WebSocket;
using MediaBrowser.Common;
using MediaBrowser.Model.ApiClient;
using MediaBrowser.Model.Logging;
using MediaBrowser.Model.Serialization;
using MediaBrowser.Theater.Interfaces.Presentation;
using MediaBrowser.Theater.Interfaces.Session;
using System;
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

        private volatile Timer _webSocketTimer;
        private readonly object _timerLock = new object();

        private ApiWebSocket _apiWebSocket;

        public WebSocketEntryPoint(ISessionManager session, IApiClient apiClient, IJsonSerializer json, ILogger logger, IApplicationHost appHost)
        {
            _session = session;
            _apiClient = apiClient;
            _json = json;
            _logger = logger;
            _appHost = appHost;
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

                    StartWebSocketTimer();

                    _logger.Info("Web socket connection established.");
                }
                catch (Exception ex)
                {
                    _logger.ErrorException("Error establishing web socket connection", ex);
                }
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
