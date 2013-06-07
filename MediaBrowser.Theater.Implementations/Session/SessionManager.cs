using MediaBrowser.Common.Events;
using MediaBrowser.Model.ApiClient;
using MediaBrowser.Model.Dto;
using MediaBrowser.Model.Logging;
using MediaBrowser.Theater.Interfaces;
using MediaBrowser.Theater.Interfaces.Navigation;
using System;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;
using MediaBrowser.Theater.Interfaces.Session;

namespace MediaBrowser.Theater.Implementations.Session
{
    public class SessionManager : ISessionManager
    {
        public event EventHandler<EventArgs> UserLoggedIn;

        public event EventHandler<EventArgs> UserLoggedOut;

        private readonly IApiClient _apiClient;
        private readonly INavigationService _navService;
        private readonly ILogger _logger;

        public SessionManager(INavigationService navService, IApiClient apiClient, ILogger logger)
        {
            _navService = navService;
            _apiClient = apiClient;
            _logger = logger;
        }

        public UserDto CurrentUser { get; private set; }

        public DispatcherOperation Logout()
        {
            _apiClient.CurrentUserId = null;

            var previous = CurrentUser;

            CurrentUser = null;

            if (previous != null)
            {
                EventHelper.FireEventIfNotNull(UserLoggedOut, this, EventArgs.Empty, _logger);
            }
            
            return _navService.NavigateToLoginPage();
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

            var root = await _apiClient.GetRootFolderAsync(user.Id);

            await _navService.NavigateToHomePage(root);
        }
    }
}
