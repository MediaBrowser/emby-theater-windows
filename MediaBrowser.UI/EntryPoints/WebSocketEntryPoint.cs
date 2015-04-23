using MediaBrowser.Model.ApiClient;
using MediaBrowser.Model.Events;
using MediaBrowser.Model.Logging;
using MediaBrowser.Theater.Interfaces.Commands;
using MediaBrowser.Theater.Interfaces.Navigation;
using MediaBrowser.Theater.Interfaces.Playback;
using MediaBrowser.Theater.Interfaces.Presentation;
using MediaBrowser.Theater.Interfaces.Session;
using MediaBrowser.Theater.Interfaces.UserInput;
using System;
using System.Collections.Generic;

namespace MediaBrowser.UI.EntryPoints
{
    public class WebSocketEntryPoint : IStartupEntryPoint
    {
        private readonly ISessionManager _sessionManager;
        private readonly ILogger _logger;
        private readonly INavigationService _navigationService;
        private readonly IPlaybackManager _playbackManager;
        private readonly IPresentationManager _presentationManager;
        private readonly ICommandManager _commandManager;
        private readonly IUserInputManager _userInputManager;
        private readonly IConnectionManager _connectionManager;

        private readonly Dictionary<string, SocketListener> _listeners = new Dictionary<string, SocketListener>(StringComparer.OrdinalIgnoreCase);

        public WebSocketEntryPoint(ISessionManager sessionManager, ILogManager logManager, INavigationService navigationService, IPlaybackManager playbackManager, IPresentationManager presentationManager, ICommandManager commandManager, IUserInputManager userInputManager, IConnectionManager connectionManager)
        {
            _sessionManager = sessionManager;
            _logger = logManager.GetLogger(GetType().Name);
            _navigationService = navigationService;
            _playbackManager = playbackManager;
            _presentationManager = presentationManager;
            _commandManager = commandManager;
            _userInputManager = userInputManager;
            _connectionManager = connectionManager;
        }

        public void Run()
        {
            _connectionManager.Connected += _connectionManager_Connected;

            var user = _sessionManager.CurrentUser;
            if (user != null)
            {
                EnsureListener(user.ServerId);
            }
        }

        private void EnsureListener(string serverId)
        {
            GetListener(serverId);
        }

        private SocketListener GetListener(string serverId)
        {
            SocketListener listener;
            if (_listeners.TryGetValue(serverId, out listener))
            {
                return listener;
            }

            var apiClient = _connectionManager.GetApiClient(serverId);

            listener = new SocketListener(serverId, apiClient, _playbackManager, _userInputManager, _presentationManager,
                _sessionManager, _logger, _navigationService, _commandManager, _connectionManager);

            _listeners[serverId] = listener;

            return listener;
        }

        void _connectionManager_Connected(object sender, GenericEventArgs<ConnectionResult> e)
        {
            EnsureListener(e.Argument.Servers[0].Id);
        }
    }
}
