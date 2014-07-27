using MediaBrowser.Common.Events;
using MediaBrowser.Model.ApiClient;
using MediaBrowser.Model.Dto;
using MediaBrowser.Model.Logging;
using MediaBrowser.Model.Net;
using MediaBrowser.Theater.Implementations.Networking;
using MediaBrowser.Theater.Interfaces.Configuration;
using MediaBrowser.Theater.Interfaces.Navigation;
using MediaBrowser.Theater.Interfaces.Playback;
using MediaBrowser.Theater.Interfaces.Session;
using MediaBrowser.Theater.Interfaces.Theming;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
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

            await _apiClient.Logout();

            var previous = CurrentUser;

            CurrentUser = null;

            if (previous != null)
            {
                EventHelper.FireEventIfNotNull(UserLoggedOut, this, EventArgs.Empty, _logger);
            }

            // Clear auto login info
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
                return Version.Parse("3.0.5115.35703");
            }
        }

        public async Task Login(string username, string password, bool rememberCredentials)
        {
            //Check just in case
            if (password == null) { password = string.Empty; }

            //Compute hash then pass to main login routine
            var hash = ComputeHash(password);
            await InternalLogin(username, hash, rememberCredentials);
        }

        protected byte[] ComputeHash(string data)
        {
            using (var provider = SHA1.Create())
            {
                var hash = provider.ComputeHash(Encoding.UTF8.GetBytes(data ?? string.Empty));
                return hash;
            }
        }

        protected async Task InternalLogin(string username, byte[] hash, bool rememberLogin)
        {
            var systemInfo = await _apiClient.GetSystemInfoAsync(CancellationToken.None);

            if (Version.Parse(systemInfo.Version) < RequiredServerVersion)
            {
                throw new ApplicationException(string.Format("Media Browser Server is out of date. Please upgrade to {0} or greater.", RequiredServerVersion));
            }

            try
            {
                var result = await _apiClient.AuthenticateUserAsync(username, hash);

                CurrentUser = result.User;
                _apiClient.SetAuthenticationInfo(result.AccessToken, result.User.Id);

                UpdateSavedLogin(result.AccessToken, result.User.Id, result.ServerId, rememberLogin);
            }
            catch (HttpException ex)
            {
                throw new UnauthorizedAccessException("Invalid username or password. Please try again.");
            }

            await AfterLogin();
        }

        private void UpdateSavedLogin(string accessToken, string userId, string serverId, bool remember)
        {
            //Save details if we need to 
            if (remember)
            {
                _config.Configuration.AutoLoginConfiguration.UserId = userId;
                _config.Configuration.AutoLoginConfiguration.AccessToken = accessToken;
                _config.Configuration.AutoLoginConfiguration.ServerId = serverId;
                _config.SaveConfiguration();
            }
            else
            {
                _config.Configuration.AutoLoginConfiguration = new AutoLoginConfiguration();
                _config.SaveConfiguration();
            }
        }

        public async Task ValidateSavedLogin(AutoLoginConfiguration configuration)
        {
            if (string.IsNullOrEmpty(configuration.AccessToken) ||
                string.IsNullOrEmpty(configuration.ServerId) ||
                string.IsNullOrEmpty(configuration.UserId))
            {
                _apiClient.ClearAuthenticationInfo();

                throw new UnauthorizedAccessException();
            }

            _apiClient.SetAuthenticationInfo(configuration.AccessToken, configuration.UserId);

            try
            {
                CurrentUser = await _apiClient.GetUserAsync(configuration.UserId);
            }
            catch (Exception ex)
            {
                _apiClient.ClearAuthenticationInfo();

                throw;
            }

            await AfterLogin();
        }

        private async Task AfterLogin()
        {
            EventHelper.FireEventIfNotNull(UserLoggedIn, this, EventArgs.Empty, _logger);

            await UpdateWolInfo();
            await _navService.NavigateToHomePage();

            _navService.ClearHistory();
        }

        private async Task UpdateWolInfo()
        {
            var systemInfo = await _apiClient.GetSystemInfoAsync(CancellationToken.None);
            
            //Check WOL config
            if (_config.Configuration.WolConfiguration == null)
            {
                _config.Configuration.WolConfiguration = new WolConfiguration
                {
                    Port = 9,
                    HostMacAddresses = new List<string>(),
                    HostIpAddresses = new List<string>(),
                    WakeAttempts = 1
                };
                _config.SaveConfiguration();
            }

            var wolConfig = _config.Configuration.WolConfiguration;

            try
            {
                var uri = new Uri(_apiClient.ServerAddress);
                var host = uri.Host;

                var currentIpAddresses = await NetworkUtils.ResolveIpAddressesForHostName(host);

                var hasChanged = currentIpAddresses.Any(currentIpAddress => wolConfig.HostIpAddresses.All(x => x != currentIpAddress));

                if (!hasChanged)
                    hasChanged = wolConfig.HostIpAddresses.Any(hostIpAddress => currentIpAddresses.All(x => x != hostIpAddress));

                if (hasChanged)
                {
                    wolConfig.HostMacAddresses =
                        await NetworkUtils.ResolveMacAddressesForHostName(host);

                    wolConfig.HostIpAddresses = currentIpAddresses;

                    //Always add system info MAC address in case we are in a WAN setting
                    if (!wolConfig.HostMacAddresses.Contains(systemInfo.MacAddress))
                        wolConfig.HostMacAddresses.Add(systemInfo.MacAddress);

                    _config.SaveConfiguration();
                }
            }
            catch (Exception ex)
            {
                _logger.ErrorException("Error attempting to configure WOL.", ex);
            }
        }
    }
}
