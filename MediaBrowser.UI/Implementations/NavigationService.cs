using System.Threading;
using MediaBrowser.Common;
using MediaBrowser.Common.Updates;
using MediaBrowser.Model.ApiClient;
using MediaBrowser.Model.Configuration;
using MediaBrowser.Model.Dto;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.Logging;
using MediaBrowser.Theater.Core.FullscreenVideo;
using MediaBrowser.Theater.Core.ImageViewer;
using MediaBrowser.Theater.Core.Login;
using MediaBrowser.Theater.Core.Settings;
using MediaBrowser.Theater.Interfaces.Configuration;
using MediaBrowser.Theater.Interfaces.Navigation;
using MediaBrowser.Theater.Interfaces.Playback;
using MediaBrowser.Theater.Interfaces.Presentation;
using MediaBrowser.Theater.Interfaces.Session;
using MediaBrowser.Theater.Interfaces.Theming;
using MediaBrowser.Theater.Interfaces.UserInput;
using MediaBrowser.Theater.Interfaces.ViewModels;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace MediaBrowser.UI.Implementations
{
    /// <summary>
    /// Class NavigationService
    /// </summary>
    internal class NavigationService : INavigationService
    {
        public event EventHandler<NavigationEventArgs> Navigated
        {
            add
            {
                App.Instance.ApplicationWindow.Navigated += value;
            }
            remove
            {
                App.Instance.ApplicationWindow.Navigated -= value;
            }
        }

        /// <summary>
        /// The _theme manager
        /// </summary>
        private readonly IThemeManager _themeManager;
        /// <summary>
        /// The _playback manager
        /// </summary>
        private readonly Func<IPlaybackManager> _playbackManagerFactory;

        private readonly IApiClient _apiClient;

        private readonly IPresentationManager _presentationManager;

        private readonly ITheaterConfigurationManager _config;
        private readonly Func<ISessionManager> _sessionFactory;
        private readonly IInstallationManager _installationManager;

        private readonly IApplicationHost _appHost;

        private readonly IImageManager _imageManager;
        private readonly ILogger _logger;

        private readonly IUserInputManager _userInputManager;

        /// <summary>
        /// Initializes a new instance of the <see cref="NavigationService" /> class.
        /// </summary>
        /// <param name="themeManager">The theme manager.</param>
        /// <param name="playbackManagerFactory">The playback manager factory.</param>
        /// <param name="apiClient">The API client.</param>
        /// <param name="presentationManager">The presentation manager.</param>
        public NavigationService(IThemeManager themeManager, Func<IPlaybackManager> playbackManagerFactory, IApiClient apiClient, IPresentationManager presentationManager, ITheaterConfigurationManager config, Func<ISessionManager> sessionFactory, IApplicationHost appHost, IInstallationManager installationManager, IImageManager imageManager, ILogger logger, IUserInputManager userInputManager)
        {
            _themeManager = themeManager;
            _playbackManagerFactory = playbackManagerFactory;
            _apiClient = apiClient;
            _presentationManager = presentationManager;
            _config = config;
            _sessionFactory = sessionFactory;
            _appHost = appHost;
            _installationManager = installationManager;
            _imageManager = imageManager;
            _logger = logger;
            _userInputManager = userInputManager;
        }

        /// <summary>
        /// Navigates the specified page.
        /// </summary>
        /// <param name="page">The page.</param>
        /// <returns>DispatcherOperation.</returns>
        public Task Navigate(Page page)
        {
            return App.Instance.ApplicationWindow.Navigate(page);
        }

        /// <summary>
        /// Navigates to settings page.
        /// </summary>
        /// <returns>DispatcherOperation.</returns>
        public Task NavigateToSettingsPage()
        {
            return Navigate(new SettingsPage(_presentationManager, this, _sessionFactory(), _appHost, _installationManager));
        }

        /// <summary>
        /// Navigates to login page.
        /// </summary>
        /// <returns>DispatcherOperation.</returns>
        public async Task NavigateToLoginPage()
        {
            var systemConfig = await _apiClient.GetServerConfigurationAsync();

            if (systemConfig.ManualLoginClients.Contains(ManualLoginCategory.MediaBrowserTheater))
            {
                await NavigateToManualLoginPage();
            }
            else
            {
                await NavigateToVisualLoginPage();
            }
        }

        private Task NavigateToVisualLoginPage()
        {
            var task = new TaskCompletionSource<bool>();

            App.Instance.ApplicationWindow.Dispatcher.InvokeAsync(async () =>
            {

                await Navigate(new LoginPage(_apiClient, _imageManager, this, _sessionFactory(), _presentationManager));

                task.TrySetResult(true);

            });

            return task.Task;
        }

        private Task NavigateToManualLoginPage()
        {
            var task = new TaskCompletionSource<bool>();

            App.Instance.ApplicationWindow.Dispatcher.InvokeAsync(async () =>
            {

                await Navigate(new ManualLoginPage(string.Empty, _sessionFactory(), _presentationManager));

                task.TrySetResult(true);

            });

            return task.Task;
        }

        /// <summary>
        /// Navigates to internal player page.
        /// </summary>
        /// <returns>DispatcherOperation.</returns>
        public Task NavigateToInternalPlayerPage()
        {
            var page = new FullscreenVideoPage(_userInputManager, _playbackManagerFactory(), this, _themeManager, _apiClient, _imageManager);

            new InternalPlayerPageBehavior(page).AdjustPresentationForPlayback();

            return Navigate(page);
        }

        /// <summary>
        /// Navigates to home page.
        /// </summary>
        /// <returns>DispatcherOperation.</returns>
        public async Task NavigateToHomePage()
        {
            var userId = _sessionFactory().CurrentUser.Id;

            var userConfig = await _config.GetUserTheaterConfiguration(userId);

            var homePages = _presentationManager.HomePages.ToList();

            var homePage = homePages.FirstOrDefault(i => string.Equals(i.Name, userConfig.HomePage)) ??
                homePages.FirstOrDefault(i => string.Equals(i.Name, "Default")) ??
                homePages.First();

            var rootItem = await _apiClient.GetRootFolderAsync(userId);

            await Navigate(homePage.GetHomePage(rootItem));
        }

        /// <summary>
        /// Navigates to item.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <param name="context">The context.</param>
        /// <returns>DispatcherOperation.</returns>
        public async Task NavigateToItem(BaseItemDto item, string context = null)
        {
            // Grab it fresh from the server to make sure we have the full record
            item = await _apiClient.GetItemAsync(item.Id, _apiClient.CurrentUserId);

            if (item.IsFolder)
            {
                DisplayPreferences displayPreferences;

                try
                {
                    displayPreferences = await _apiClient.GetDisplayPreferencesAsync(item.DisplayPreferencesId, _sessionFactory().CurrentUser.Id, "DefaultTheme", CancellationToken.None);
                }
                catch
                {
                    // Already logged at lower levels
                    displayPreferences = new DisplayPreferences() { Id = new Guid(item.DisplayPreferencesId) };
                }

                await App.Instance.ApplicationWindow.Dispatcher.InvokeAsync(async () => await Navigate(_themeManager.CurrentTheme.GetFolderPage(item, displayPreferences, context)));
            }
            else
            {
                await App.Instance.ApplicationWindow.Dispatcher.InvokeAsync(async () => await Navigate(_themeManager.CurrentTheme.GetItemPage(item, context)));
            }
        }

        /// <summary>
        /// Navigates to person.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="context">The context.</param>
        /// <returns>Task.</returns>
        public async Task NavigateToPerson(string name, string context = null)
        {
            var item = await _apiClient.GetPersonAsync(name, _sessionFactory().CurrentUser.Id);

            await App.Instance.ApplicationWindow.Dispatcher.InvokeAsync(async () => await Navigate(_themeManager.CurrentTheme.GetPersonPage(item, context)));
        }

        /// <summary>
        /// Navigates the back.
        /// </summary>
        /// <returns>DispatcherOperation.</returns>
        public Task NavigateBack()
        {
            return App.Instance.ApplicationWindow.NavigateBack();
        }

        /// <summary>
        /// Navigates the forward.
        /// </summary>
        /// <returns>DispatcherOperation.</returns>
        public Task NavigateForward()
        {
            return App.Instance.ApplicationWindow.NavigateForward();
        }

        /// <summary>
        /// Clears the history.
        /// </summary>
        public void ClearHistory()
        {
            App.Instance.ApplicationWindow.ClearNavigationHistory();
        }

        /// <summary>
        /// Removes the pages from history.
        /// </summary>
        /// <param name="count">The count.</param>
        public void RemovePagesFromHistory(int count)
        {
            App.Instance.ApplicationWindow.RemovePagesFromHistory(count);
        }

        /// <summary>
        /// Gets the current page.
        /// </summary>
        /// <value>The current page.</value>
        public Page CurrentPage
        {
            get { return App.Instance.ApplicationWindow.PageFrame.Content as Page; }
        }

        public Task NavigateToImageViewer(ImageViewerViewModel viewModel)
        {
            return Navigate(new ImageViewerPage(_themeManager, viewModel));
        }
    }
}
