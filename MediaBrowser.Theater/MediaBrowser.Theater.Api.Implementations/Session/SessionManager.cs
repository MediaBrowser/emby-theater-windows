using System;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using MediaBrowser.Common.Events;
using MediaBrowser.Model.ApiClient;
using MediaBrowser.Model.Dto;
using MediaBrowser.Model.Logging;
using MediaBrowser.Model.Net;
using MediaBrowser.Model.System;
using MediaBrowser.Model.Users;
using MediaBrowser.Theater.Api.Configuration;
using MediaBrowser.Theater.Api.Navigation;
using MediaBrowser.Theater.Api.Playback;

namespace MediaBrowser.Theater.Api.Session
{
    public class SessionManager : ISessionManager
    {
        private readonly IApiClient _apiClient;
        private readonly ITheaterConfigurationManager _config;
        private readonly ILogger _logger;
        private readonly INavigator _navService;
//        private readonly IPlaybackManager _playback;

        public SessionManager(INavigator navService, IApiClient apiClient, ILogger logger, ITheaterConfigurationManager config)//, IPlaybackManager playback)
        {
            _navService = navService;
            _apiClient = apiClient;
            _logger = logger;
            _config = config;
//            _playback = playback;
        }

        /// <summary>
        ///     TODO: Create an ITheaterAppHost and add this property to it
        /// </summary>
        private Version RequiredServerVersion
        {
            get { return Version.Parse("3.0.5115.35703"); }
        }

        public event EventHandler<EventArgs> UserLoggedIn;

        public event EventHandler<EventArgs> UserLoggedOut;

        public UserDto CurrentUser { get; private set; }

        public async Task Logout()
        {
//            _playback.StopAllPlayback();

            _apiClient.CurrentUserId = null;

            UserDto previous = CurrentUser;

            CurrentUser = null;

            if (previous != null) {
                EventHelper.FireEventIfNotNull(UserLoggedOut, this, EventArgs.Empty, _logger);
            }

            //Clear auto login info
            _config.Configuration.AutoLoginConfiguration = new AutoLoginConfiguration();
            _config.SaveConfiguration();

            await _navService.Navigate(Go.To.Login());
            _navService.ClearNavigationHistory();
        }

        public async Task Login(string username, string password, bool rememberCredentials)
        {
            //Check just in case
            if (password == null) {
                password = string.Empty;
            }

            //Compute hash then pass to main login routine
            byte[] hash = ComputeHash(password);
            await InternalLogin(username, hash);

            //Save details if we need to 
            if (rememberCredentials) {
                _config.Configuration.AutoLoginConfiguration.UserName = username;
                if (String.IsNullOrEmpty(password)) {
                    _config.Configuration.AutoLoginConfiguration.UserPasswordHash = null;
                } else {
                    _config.Configuration.AutoLoginConfiguration.UserPasswordHash = Convert.ToBase64String(hash);
                }
                _config.SaveConfiguration();
            } else {
                _config.Configuration.AutoLoginConfiguration = new AutoLoginConfiguration();
                _config.SaveConfiguration();
            }
        }

        public async Task LoginWithHash(string username, string passwordHash, bool rememberCredentials)
        {
            byte[] hash = Convert.FromBase64String(passwordHash);
            await InternalLogin(username, hash);

            //Save details if we need to 
            if (rememberCredentials) {
                _config.Configuration.AutoLoginConfiguration.UserName = username;
                _config.Configuration.AutoLoginConfiguration.UserPasswordHash = passwordHash;
                _config.SaveConfiguration();
            } else {
                _config.Configuration.AutoLoginConfiguration = new AutoLoginConfiguration();
                _config.SaveConfiguration();
            }
        }

        protected byte[] ComputeHash(string data)
        {
            using (SHA1 provider = SHA1.Create()) {
                byte[] hash = provider.ComputeHash(Encoding.UTF8.GetBytes(data ?? string.Empty));
                return hash;
            }
        }

        protected async Task InternalLogin(string username, byte[] hash)
        {
            SystemInfo systemInfo = await _apiClient.GetSystemInfoAsync();

            if (Version.Parse(systemInfo.Version) < RequiredServerVersion) {
                throw new ApplicationException(string.Format("Media Browser Server is out of date. Please upgrade to {0} or greater.", RequiredServerVersion));
            }

            try {
                AuthenticationResult result = await _apiClient.AuthenticateUserAsync(username, hash);

                CurrentUser = result.User;
                _apiClient.CurrentUserId = CurrentUser.Id;
            }
            catch (HttpException ex) {
                throw new UnauthorizedAccessException("Invalid username or password. Please try again.");
            }

            EventHelper.FireEventIfNotNull(UserLoggedIn, this, EventArgs.Empty, _logger);

            await _navService.Navigate(Go.To.Home());
            _navService.ClearNavigationHistory();
        }
    }
}