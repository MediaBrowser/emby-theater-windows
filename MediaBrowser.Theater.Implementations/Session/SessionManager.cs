using MediaBrowser.Common.Events;
using MediaBrowser.Model.ApiClient;
using MediaBrowser.Model.Dto;
using MediaBrowser.Model.Logging;
using MediaBrowser.Theater.Interfaces.Configuration;
using MediaBrowser.Theater.Interfaces.Navigation;
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

        public SessionManager(INavigationService navService, IApiClient apiClient, ILogger logger, IThemeManager themeManager, ITheaterConfigurationManager config)
        {
            _navService = navService;
            _apiClient = apiClient;
            _logger = logger;
            _themeManager = themeManager;
            _config = config;
        }

        public UserDto CurrentUser { get; private set; }

        public async Task Logout()
        {
            _apiClient.CurrentUserId = null;

            var previous = CurrentUser;

            CurrentUser = null;

            if (previous != null)
            {
                EventHelper.FireEventIfNotNull(UserLoggedOut, this, EventArgs.Empty, _logger);
            }

            await _navService.NavigateToLoginPage();
            _navService.ClearHistory();
        }

        public async Task Login(UserDto user, string password)
        {
            if (CurrentUser != null && !string.Equals(CurrentUser.Id, user.Id))
            {
                await Logout();
            }

            using (var provider = SHA1.Create())
            {
                var hash = provider.ComputeHash(Encoding.UTF8.GetBytes(password ?? string.Empty));

                await _apiClient.AuthenticateUserAsync(user.Id, hash);
            }

            CurrentUser = user;
            _apiClient.CurrentUserId = user.Id;

            EventHelper.FireEventIfNotNull(UserLoggedIn, this, EventArgs.Empty, _logger);

            var userConfig = await _config.GetUserTheaterConfiguration(user.Id);

            var theme = _themeManager.Themes.FirstOrDefault(i => string.Equals(i.Name, userConfig.Theme)) ?? _themeManager.DefaultTheme;

            await _themeManager.LoadTheme(theme);

            await _navService.NavigateToHomePage(user.Id);

            _navService.ClearHistory();
        }
    }
}
