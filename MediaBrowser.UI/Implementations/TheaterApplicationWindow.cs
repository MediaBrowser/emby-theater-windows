using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;
using MediaBrowser.Common.Events;
using MediaBrowser.Model.ApiClient;
using MediaBrowser.Model.Dto;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.Logging;
using MediaBrowser.Theater.Core.Loading;
using MediaBrowser.Theater.Core.Modals;
using MediaBrowser.Theater.Interfaces;
using MediaBrowser.Theater.Interfaces.Presentation;
using MediaBrowser.Theater.Interfaces.Session;
using MediaBrowser.Theater.Interfaces.Theming;
using MediaBrowser.Theater.Interfaces.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media.Imaging;

namespace MediaBrowser.UI.Implementations
{
    /// <summary>
    /// Class TheaterApplicationWindow
    /// </summary>
    internal class TheaterApplicationWindow : IPresentationManager
    {
        /// <summary>
        /// The _logger
        /// </summary>
        private readonly ILogger _logger;
        private readonly IThemeManager _themeManager;
        private readonly IApiClient _apiClient;
        private readonly Func<ISessionManager> _sessionFactory;

        /// <summary>
        /// Initializes a new instance of the <see cref="TheaterApplicationWindow" /> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="themeManager">The theme manager.</param>
        public TheaterApplicationWindow(ILogger logger, IThemeManager themeManager, IApiClient apiClient, Func<ISessionManager> sessionFactory)
        {
            _logger = logger;
            _themeManager = themeManager;
            _apiClient = apiClient;
            _sessionFactory = sessionFactory;

            _themeManager.ThemeUnloaded += _themeManager_ThemeUnloaded;
            _themeManager.ThemeLoaded += _themeManager_ThemeLoaded;
        }

        void _themeManager_ThemeLoaded(object sender, ItemEventArgs<ITheme> e)
        {
            if (App.Instance.ApplicationWindow != null)
            {
                App.Instance.ApplicationWindow.PageContent.Content = e.Argument.CreatePageContentDataContext();
            }
        }

        void _themeManager_ThemeUnloaded(object sender, ItemEventArgs<ITheme> e)
        {
            App.Instance.ApplicationWindow.PageContent.Content = null;
        }

        public IEnumerable<IHomePage> HomePages { get; private set; }

        /// <summary>
        /// Gets the window.
        /// </summary>
        /// <value>The window.</value>
        public Window Window
        {
            get { return App.Instance.ApplicationWindow; }
        }

        /// <summary>
        /// Clears the backdrops.
        /// </summary>
        public void ClearBackdrops()
        {
            App.Instance.ApplicationWindow.RotatingBackdrops.ClearBackdrops();
        }

        /// <summary>
        /// Sets the backdrops.
        /// </summary>
        /// <param name="item">The item.</param>
        public void SetBackdrops(BaseItemDto item)
        {
            App.Instance.ApplicationWindow.RotatingBackdrops.SetBackdrops(item);
        }

        /// <summary>
        /// Sets the backdrops.
        /// </summary>
        /// <param name="paths">The paths.</param>
        public void SetBackdrops(IEnumerable<string> paths)
        {
            App.Instance.ApplicationWindow.RotatingBackdrops.SetBackdrops(paths.ToArray());
        }

        /// <summary>
        /// Called when [window loaded].
        /// </summary>
        internal void OnWindowLoaded()
        {
            App.Instance.ApplicationWindow.PageContent.Content = _themeManager.CurrentTheme.CreatePageContentDataContext();
            EventHelper.FireEventIfNotNull(WindowLoaded, null, EventArgs.Empty, _logger);
        }

        /// <summary>
        /// Occurs when [window loaded].
        /// </summary>
        public event EventHandler<EventArgs> WindowLoaded;


        /// <summary>
        /// Gets the backdrop container.
        /// </summary>
        /// <value>The backdrop container.</value>
        public FrameworkElement BackdropContainer
        {
            get
            {
                return App.Instance.ApplicationWindow.BackdropContainer;
            }
        }

        /// <summary>
        /// Gets the window overlay.
        /// </summary>
        /// <value>The window overlay.</value>
        public FrameworkElement WindowOverlay
        {
            get
            {
                return App.Instance.ApplicationWindow.WindowBackgroundContent;
            }
        }


        /// <summary>
        /// Gets the apps.
        /// </summary>
        /// <value>The apps.</value>
        private IEnumerable<IAppFactory> AppFactories { get; set; }

        /// <summary>
        /// Gets the settings pages.
        /// </summary>
        /// <value>The settings pages.</value>
        public IEnumerable<ISettingsPage> SettingsPages { get; private set; }

        /// <summary>
        /// Adds the parts.
        /// </summary>
        /// <param name="apps">The apps.</param>
        /// <param name="settingsPages">The settings pages.</param>
        /// <param name="homePages">The home pages.</param>
        public void AddParts(IEnumerable<IAppFactory> apps, IEnumerable<ISettingsPage> settingsPages, IEnumerable<IHomePage> homePages)
        {
            AppFactories = apps;
            SettingsPages = settingsPages;
            HomePages = homePages;
        }

        /// <summary>
        /// Shows the default error message.
        /// </summary>
        /// <exception cref="System.NotImplementedException"></exception>
        public void ShowDefaultErrorMessage()
        {
            ShowMessage(new MessageBoxInfo
            {
                Caption = "Error",
                Text = "There was an error processing the request.",
                Icon = MessageBoxIcon.Error,
                Button = MessageBoxButton.OK
            });
        }

        /// <summary>
        /// Shows the message.
        /// </summary>
        /// <param name="options">The options.</param>
        /// <returns>MessageBoxResult.</returns>
        public MessageBoxResult ShowMessage(MessageBoxInfo options)
        {
            return ShowMessage(options, Window);
        }

        /// <summary>
        /// Shows the message.
        /// </summary>
        /// <param name="options">The options.</param>
        /// <param name="parentWindow">The parent window.</param>
        /// <returns>MessageBoxResult.</returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public MessageBoxResult ShowMessage(MessageBoxInfo options, Window parentWindow)
        {
            return parentWindow.Dispatcher.Invoke(() =>
            {
                var win = new MessageBoxWindow(options);

                win.ShowModal(parentWindow);

                return win.MessageBoxResult;
            });
        }

        public void ShowNotification(string caption, string text, BitmapImage icon)
        {
        }

        public void SetPageTitle(string title)
        {
            var viewModel = App.Instance.ApplicationWindow.PageContent.Content as PageContentViewModel;

            if (viewModel != null)
            {
                viewModel.PageTitle = title;
                viewModel.ShowDefaultPageTitle = false;
            }
        }

        public void SetDefaultPageTitle()
        {
            var viewModel = App.Instance.ApplicationWindow.PageContent.Content as PageContentViewModel;

            if (viewModel != null)
            {
                viewModel.ShowDefaultPageTitle = true;
            }
        }

        public IEnumerable<IApp> GetApps(UserDto user)
        {
            return AppFactories
                .SelectMany(i => i.GetApps());
        }

        public void AddResourceDictionary(ResourceDictionary resource)
        {
            _logger.Info("Adding resource {0}", resource.GetType().Name);

            Application.Current.Resources.MergedDictionaries.Add(resource);
        }

        public void RemoveResourceDictionary(ResourceDictionary resource)
        {
            _logger.Info("Removing resource {0}", resource.GetType().Name);

            Application.Current.Resources.MergedDictionaries.Remove(resource);
        }

        public void SetGlobalThemeContentVisibility(bool visible)
        {
            var viewModel = App.Instance.ApplicationWindow.PageContent.Content as PageContentViewModel;

            if (viewModel != null)
            {
                viewModel.ShowGlobalContent = visible;
            }
        }

        public Task<DisplayPreferences> GetDisplayPreferences(string displayPreferencesId, CancellationToken cancellationToken)
        {
            return _apiClient.GetDisplayPreferencesAsync(displayPreferencesId, _sessionFactory().CurrentUser.Id, "MBT-" + _themeManager.CurrentTheme.Name, cancellationToken);
        }

        public Task UpdateDisplayPreferences(DisplayPreferences displayPreferences, CancellationToken cancellationToken)
        {
            return _apiClient.UpdateDisplayPreferencesAsync(displayPreferences, _sessionFactory().CurrentUser.Id, "MBT-" + _themeManager.CurrentTheme.Name, cancellationToken);
        }

        private LoadingWindow _loadingWindow;
        private int _loadingCount;
        private readonly object _loadingSyncLock = new object();

        public void ShowLoadingAnimation()
        {
            lock (_loadingSyncLock)
            {
                _loadingCount++;

                if (_loadingCount == 1)
                {
                    Window.Dispatcher.InvokeAsync(() =>
                    {
                        if (_loadingWindow == null)
                        {
                            _loadingWindow = new LoadingWindow();
                        }

                        if (!_loadingWindow.IsActive)
                        {
                            _loadingWindow.Show(Window);
                        }
                    });
                }
            }
        }

        public void HideLoadingAnimation()
        {
            lock (_loadingSyncLock)
            {
                if (_loadingCount > 0)
                {
                    _loadingCount--;
                }

                if (_loadingCount == 0)
                {
                    Window.Dispatcher.InvokeAsync(() => _loadingWindow.Hide());
                }
            }
        }
    }
}
