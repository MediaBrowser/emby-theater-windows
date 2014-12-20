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

        public SessionManager(INavigator navService, ILogger logger, ITheaterConfigurationManager config, IPlaybackManager playback, IConnectionManager connectionManager)
        {
            _navService = navService;
            _logger = logger;
            _config = config;
            _playback = playback;
            _connectionManager = connectionManager;

            _connectionManager.RemoteLoggedOut += _connectionManager_RemoteLoggedOut;
        }

        async void _connectionManager_RemoteLoggedOut(object sender, EventArgs e)
        {
            if (CurrentUser != null)
            {
                await Logout();
            }
        }

        public UserDto CurrentUser { get; private set; }
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

            var previous = CurrentUser;

            CurrentUser = null;

            if (previous != null)
            {
                EventHelper.FireEventIfNotNull(UserLoggedOut, this, EventArgs.Empty, _logger);
            }

            await _navService.Navigate(Go.To.Login());

            _navService.ClearNavigationHistory();
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

                _config.Configuration.RememberLogin = rememberCredentials;
                _config.SaveConfiguration();
            }
            catch (HttpException ex)
            {
                throw new UnauthorizedAccessException("Invalid username or password. Please try again.");
            }

            await AfterLogin();
        }

        public async Task ValidateSavedLogin(ConnectionResult result)
        {
            CurrentUser = await result.ApiClient.GetUserAsync(result.ApiClient.CurrentUserId);

            // TODO: Switch to check for ConnectUser
            if (result.Servers.Any(i => !string.IsNullOrEmpty(i.ExchangeToken)))
            {
                _config.Configuration.RememberLogin = true;
                _config.SaveConfiguration();
            }

            await AfterLogin();
        }

        private async Task AfterLogin()
        {
            EventHelper.FireEventIfNotNull(UserLoggedIn, this, EventArgs.Empty, _logger);

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