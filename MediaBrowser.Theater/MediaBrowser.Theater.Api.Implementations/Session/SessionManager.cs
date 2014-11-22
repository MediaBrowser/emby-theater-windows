using System;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using MediaBrowser.Common.Events;
using MediaBrowser.Model.ApiClient;
using MediaBrowser.Model.Dto;
using MediaBrowser.Model.Logging;
using MediaBrowser.Model.Net;
using MediaBrowser.Model.Users;
using MediaBrowser.Theater.Api.Configuration;
using MediaBrowser.Theater.Api.Navigation;
using MediaBrowser.Theater.Api.Playback;

namespace MediaBrowser.Theater.Api.Session
{
    public class SessionManager : ISessionManager
    {
        private readonly ITheaterConfigurationManager _config;
        private readonly IConnectionManager _connectionManager;
        private readonly ILogger _logger;
        private readonly INavigator _navService;
        private readonly IPlaybackManager _playback;

        public SessionManager(INavigator navService, ILogger logger, ITheaterConfigurationManager config, IPlaybackManager playback, IConnectionManager connectionManager)
        {
            _navService = navService;
            _logger = logger;
            _config = config;
            _playback = playback;
            _connectionManager = connectionManager;

            _connectionManager.RemoteLoggedOut += _connectionManager_RemoteLoggedOut;
        }

        public event EventHandler<EventArgs> UserLoggedIn;

        public event EventHandler<EventArgs> UserLoggedOut;

        public UserDto CurrentUser { get; private set; }

        public IApiClient ActiveApiClient
        {
            get { return _connectionManager.GetApiClient(new BaseItemDto()); }
        }

        public async Task Logout()
        {
            _playback.StopAllPlayback();

            await _connectionManager.Logout();

            UserDto previous = CurrentUser;

            CurrentUser = null;

            if (previous != null) {
                EventHelper.FireEventIfNotNull(UserLoggedOut, this, EventArgs.Empty, _logger);
            }

            await _navService.Navigate(Go.To.Login());

            _navService.ClearNavigationHistory();
        }

        public async Task LoginToServer(string username, string password, bool rememberCredentials)
        {
            IApiClient apiClient = ActiveApiClient;

            //Check just in case
            if (password == null) {
                password = string.Empty;
            }
            
            try {
                AuthenticationResult result = await apiClient.AuthenticateUserAsync(username, password);

                CurrentUser = result.User;

                _config.Configuration.RememberLogin = rememberCredentials;
                _config.SaveConfiguration();
            }
            catch (HttpException ex) {
                throw new UnauthorizedAccessException("Invalid username or password. Please try again.");
            }

            await AfterLogin();
        }

        public async Task ValidateSavedLogin(ConnectionResult result)
        {
            CurrentUser = await result.ApiClient.GetUserAsync(result.ApiClient.CurrentUserId);

            await AfterLogin();
        }

        private async void _connectionManager_RemoteLoggedOut(object sender, EventArgs e)
        {
            if (CurrentUser != null) {
                await Logout();
            }
        }

        private async Task AfterLogin()
        {
            EventHelper.FireEventIfNotNull(UserLoggedIn, this, EventArgs.Empty, _logger);

            await _navService.Navigate(Go.To.Home());

            _navService.ClearNavigationHistory();
        }
    }
}