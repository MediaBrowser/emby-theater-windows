using MediaBrowser.Common.Events;
using MediaBrowser.Model.ApiClient;
using MediaBrowser.Model.Dto;
using MediaBrowser.Model.Logging;
using MediaBrowser.Model.Net;
using MediaBrowser.Theater.Interfaces.Configuration;
using MediaBrowser.Theater.Interfaces.Navigation;
using MediaBrowser.Theater.Interfaces.Playback;
using MediaBrowser.Theater.Interfaces.Session;
using MediaBrowser.Theater.Interfaces.Theming;
using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace MediaBrowser.Theater.Implementations.Session
{
    public class SessionManager : ISessionManager
    {
        public event EventHandler<EventArgs> UserLoggedIn;

        public event EventHandler<EventArgs> UserLoggedOut;

        private readonly IApiClient _apiClient;
        private readonly INavigationService _navService;
        private readonly ILogger _logger;
        private readonly IThemeManager _themeManager;
        private readonly ITheaterConfigurationManager _config;
        private readonly IPlaybackManager _playback;

        public SessionManager(INavigationService navService, IApiClient apiClient, ILogger logger, IThemeManager themeManager, ITheaterConfigurationManager config, IPlaybackManager playback)
        {
            _navService = navService;
            _apiClient = apiClient;
            _logger = logger;
            _themeManager = themeManager;
            _config = config;
            _playback = playback;
        }

        public UserDto CurrentUser { get; private set; }

        public async Task Logout()
        {
            _playback.StopAllPlayback();

            _apiClient.CurrentUserId = null;

            var previous = CurrentUser;

            CurrentUser = null;

            if (previous != null)
            {
                EventHelper.FireEventIfNotNull(UserLoggedOut, this, EventArgs.Empty, _logger);
            }

            //Clear auto login info
            _config.Configuration.AutoLoginConfiguration = new AutoLoginConfiguration();
            _config.SaveConfiguration();

            await _navService.NavigateToLoginPage();

            _navService.ClearHistory();
        }

        /// <summary>
        /// TODO: Create an ITheaterAppHost and add this property to it
        /// </summary>
        private Version RequiredServerVersion
        {
            get
            {
                return Version.Parse("3.0.5076.20161");
            }
        }

        public async Task Login(string username, string password, bool rememberCredentials)
        {
            //Check just in case
            if (password == null) { password = string.Empty; }

            //Compute hash then pass to main login routine
            var hash = ComputeHash(password);
            await InternalLogin(username, hash);

            //Save details if we need to 
            if (rememberCredentials)
            {
                _config.Configuration.AutoLoginConfiguration.UserName = username;
                if(String.IsNullOrEmpty(password)) { _config.Configuration.AutoLoginConfiguration.UserPasswordHash = null;}
                else {_config.Configuration.AutoLoginConfiguration.UserPasswordHash = Convert.ToBase64String(hash); }
                _config.SaveConfiguration();
            }
            else
            {
                _config.Configuration.AutoLoginConfiguration = new AutoLoginConfiguration();
                _config.SaveConfiguration();
            }
        }

        public async Task LoginWithHash(string username, string passwordHash, bool rememberCredentials)
        {
            byte[] hash = Convert.FromBase64String(passwordHash);
            await InternalLogin(username, hash);

            //Save details if we need to 
            if (rememberCredentials)
            {
                _config.Configuration.AutoLoginConfiguration.UserName = username;
                _config.Configuration.AutoLoginConfiguration.UserPasswordHash = passwordHash;
                _config.SaveConfiguration();
            }
            else
            {
                _config.Configuration.AutoLoginConfiguration = new AutoLoginConfiguration();
                _config.SaveConfiguration();
            }
        }

        protected byte[] ComputeHash(string data)
        {
            using (var provider = SHA1.Create())
            {
                var hash = provider.ComputeHash(Encoding.UTF8.GetBytes(data ?? string.Empty));
                return hash;
            }
        }

        protected async Task InternalLogin(string username, byte[] hash)
        {
            var systemInfo = await _apiClient.GetSystemInfoAsync();

            if (Version.Parse(systemInfo.Version) < RequiredServerVersion)
            {
                throw new ApplicationException(string.Format("Media Browser Server is out of date. Please upgrade to {0} or greater.", RequiredServerVersion));
            }

            try
            {
                var result = await _apiClient.AuthenticateUserAsync(username, hash);

                CurrentUser = result.User;
                _apiClient.CurrentUserId = CurrentUser.Id;
            }
            catch (HttpException ex)
            {
                throw new UnauthorizedAccessException("Invalid username or password. Please try again.");
            }

            EventHelper.FireEventIfNotNull(UserLoggedIn, this, EventArgs.Empty, _logger);

            await _navService.NavigateToHomePage();

            _navService.ClearHistory();
        }
    }
}
