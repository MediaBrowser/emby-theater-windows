using System;
using System.Linq;
using System.Threading.Tasks;
using MediaBrowser.Common.Events;
using MediaBrowser.Model.ApiClient;
using MediaBrowser.Model.Configuration;
using MediaBrowser.Model.Connect;
using MediaBrowser.Model.Dto;
using MediaBrowser.Model.Logging;
using MediaBrowser.Model.Net;
using MediaBrowser.Theater.Api.Configuration;
using MediaBrowser.Theater.Api.Navigation;
using MediaBrowser.Theater.Api.Playback;

namespace MediaBrowser.Theater.Api.Session
{
    public class SessionManager : ISessionManager
    {
        public event EventHandler<EventArgs> UserLoggedIn;

        public event EventHandler<EventArgs> UserLoggedOut;

        private readonly INavigator _navService;
        private readonly ILogger _logger;
        private readonly ITheaterConfigurationManager _config;
        private readonly IPlaybackManager _playback;
        private readonly IConnectionManager _connectionManager;
        private UserDto _currentUser;

        public SessionManager(INavigator navService, ILogger logger, ITheaterConfigurationManager config, IPlaybackManager playback, IConnectionManager connectionManager)
        {
            _navService = navService;
            _logger = logger;
            _config = config;
            _playback = playback;
            _connectionManager = connectionManager;

            _connectionManager.LocalUserSignIn += (s, e) => CurrentUser = e.Argument;
            _connectionManager.LocalUserSignOut += (s, e) => CurrentUser = null;
            _connectionManager.ConnectUserSignIn += (s, e) => ConnectUser = e.Argument;
            _connectionManager.ConnectUserSignOut += (s, e) => ConnectUser = null;
            _connectionManager.RemoteLoggedOut += (s, e) => playback.StopAllPlayback();
        }
        
        public UserDto CurrentUser
        {
            get { return _currentUser; }
            private set
            {
                if (Equals(_currentUser, value)) {
                    return;
                }

                var previous = _currentUser;
                _currentUser = value;

                if (previous != null) {
                    EventHelper.FireEventIfNotNull(UserLoggedOut, this, EventArgs.Empty, _logger);
                }

                if (_currentUser != null) {
                    EventHelper.FireEventIfNotNull(UserLoggedIn, this, EventArgs.Empty, _logger);
                }
            }
        }

        public ConnectUser ConnectUser { get; private set; }

        public IApiClient ActiveApiClient
        {
            get
            {
                return _connectionManager.CurrentApiClient;
            }
        }

        public async Task Logout()
        {
            _playback.StopAllPlayback();
            await _connectionManager.Logout();
        }

        public async Task LoginToServer(string username, string password, bool rememberCredentials)
        {
            var apiClient = ActiveApiClient;

            //Check just in case
            if (password == null)
            {
                password = string.Empty;
            }
            
            _connectionManager.SaveLocalCredentials = rememberCredentials;

            try
            {
                var result = await apiClient.AuthenticateUserAsync(username, password);
                CurrentUser = result.User;
            }
            catch (HttpException ex)
            {
                throw new UnauthorizedAccessException("Invalid username or password. Please try again.");
            }

            await AfterLogin();
        }
        
        private async Task AfterLogin()
        {
            await _navService.Navigate(Go.To.Home());
            _navService.ClearNavigationHistory();
        }

        public string LocalUserId
        {
            get { return CurrentUser == null ? null : CurrentUser.Id; }
        }

        public string ConnectUserId
        {
            get { return ConnectUser == null ? null : ConnectUser.Id; }
        }

        public string UserName
        {
            get
            {
                return CurrentUser == null ?
                    (ConnectUser == null ? null : ConnectUser.Name) :
                    CurrentUser.Name;
            }
        }

        public string UserImageUrl
        {
            get
            {
                return CurrentUser == null ?
                    (ConnectUser == null ? null : ConnectUser.ImageUrl) :
                    GetLocalUserImageUrl();
            }
        }

        public UserConfiguration UserConfiguration
        {
            get
            {
                return CurrentUser == null ?
                    (ConnectUser == null ? null : new UserConfiguration()) :
                    CurrentUser.Configuration;
            }
        }

        public bool IsUserSignedIn
        {
            get { return CurrentUser != null || ConnectUser != null; }
        }

        private string GetLocalUserImageUrl()
        {
            if (CurrentUser != null && CurrentUser.HasPrimaryImage)
            {
                return ActiveApiClient.GetUserImageUrl(CurrentUser, new ImageOptions
                {

                });
            }

            return null;
        }
    }
}