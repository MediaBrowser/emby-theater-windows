using System.Threading;
using System.Windows.Threading;
using MediaBrowser.Model.ApiClient;
using MediaBrowser.Model.Dto;
using MediaBrowser.Model.Logging;
using MediaBrowser.Model.Net;
using MediaBrowser.Plugins.DefaultTheme.Home.Apps;
using MediaBrowser.Theater.Interfaces.Navigation;
using MediaBrowser.Theater.Interfaces.Presentation;
using MediaBrowser.Theater.Interfaces.Session;
using MediaBrowser.Theater.Interfaces.Theming;
using MediaBrowser.Theater.Presentation.Controls;
using MediaBrowser.Theater.Presentation.Pages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace MediaBrowser.Plugins.DefaultTheme.Home
{
    /// <summary>
    /// Interaction logic for HomePage.xaml
    /// </summary>
    public partial class HomePage : BasePage
    {
        /// <summary>
        /// Gets the API client.
        /// </summary>
        /// <value>The API client.</value>
        protected IApiClient ApiClient { get; private set; }
        /// <summary>
        /// Gets the image manager.
        /// </summary>
        /// <value>The image manager.</value>
        protected IImageManager ImageManager { get; private set; }
        /// <summary>
        /// Gets the session manager.
        /// </summary>
        /// <value>The session manager.</value>
        protected ISessionManager SessionManager { get; private set; }
        /// <summary>
        /// Gets the application window.
        /// </summary>
        /// <value>The application window.</value>
        protected IPresentationManager PresentationManager { get; private set; }
        /// <summary>
        /// Gets the navigation manager.
        /// </summary>
        /// <value>The navigation manager.</value>
        protected INavigationService NavigationManager { get; private set; }

        public BaseItemDto ParentItem { get; set; }
        public string DisplayPreferencesId { get; set; }

        private readonly ILogger _logger;

        public HomePage(IApiClient apiClient, IImageManager imageManager, ISessionManager sessionManager, IPresentationManager applicationWindow, INavigationService navigationManager, ILogger logger)
        {
            NavigationManager = navigationManager;
            PresentationManager = applicationWindow;
            SessionManager = sessionManager;
            ImageManager = imageManager;
            ApiClient = apiClient;
            _logger = logger;

            InitializeComponent();
        }

        protected override async void OnInitialized(EventArgs e)
        {
            Loaded += HomePage_Loaded;
            Unloaded += HomePage_Unloaded;

            MenuList.SelectionChanged += MenuList_SelectionChanged;

            ParentItem = await ApiClient.GetRootFolderAsync(SessionManager.CurrentUser.Id);

            DisplayPreferencesId = ParentItem.DisplayPreferencesId;

            new ListFocuser(MenuList).FocusAfterContainersGenerated(0);

            var views = new List<string>();

            try
            {
                var itemCounts = await ApiClient.GetItemCountsAsync(SessionManager.CurrentUser.Id);

                if (itemCounts.MovieCount > 0 || itemCounts.TrailerCount > 0)
                {
                    views.Add("movies");
                }

                if (itemCounts.SeriesCount > 0 || itemCounts.EpisodeCount > 0)
                {
                    views.Add("tv");
                }

                if (itemCounts.SongCount > 0)
                {
                    views.Add("music");
                }
                if (itemCounts.GameCount > 0)
                {
                    views.Add("games");
                }
            }
            catch (HttpException)
            {
                PresentationManager.ShowDefaultErrorMessage();
            }

            if (PresentationManager.GetApps(SessionManager.CurrentUser).Any())
            {
                views.Add("apps");
            }

            views.Add("media collections");

            MenuList.ItemsSource = CollectionViewSource.GetDefaultView(views);

            base.OnInitialized(e);
        }

        private DispatcherTimer _selectionChangeTimer;
        private readonly object _syncLock = new object();

        void MenuList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            lock (_syncLock)
            {
                if (_selectionChangeTimer == null)
                {
                    _selectionChangeTimer = new DispatcherTimer(TimeSpan.FromMilliseconds(400), DispatcherPriority.Normal, UpdatePageContent, Dispatcher);
                }
                else
                {
                    _selectionChangeTimer.Stop();
                    _selectionChangeTimer.Start();
                }
            }
        }

        private void UpdatePageContent(object sender, EventArgs args)
        {
            DisposeTimer();

            var item = MenuList.SelectedItem as string;

            if (string.Equals(item, "movies"))
            {
                PageContent.Content = new Movies.Movies(ParentItem, ApiClient, ImageManager, SessionManager, NavigationManager, PresentationManager);
            }
            else if (string.Equals(item, "tv"))
            {
                PageContent.Content = new TV.TV(ParentItem, ApiClient, ImageManager, SessionManager, NavigationManager, PresentationManager);
            }
            else if (string.Equals(item, "music"))
            {
                PageContent.Content = new Music.Music(ParentItem, ApiClient, ImageManager, SessionManager, NavigationManager, PresentationManager);
            }
            else if (string.Equals(item, "games"))
            {
                PageContent.Content = new TextBlock();
            }
            else if (string.Equals(item, "apps"))
            {
                PageContent.Content = new AppsControl(PresentationManager, SessionManager, _logger);
            }
            if (string.Equals(item, "media collections"))
            {
                PageContent.Content = new Folders(ParentItem, new Model.Entities.DisplayPreferences
                {
                    PrimaryImageHeight = 225,
                    PrimaryImageWidth = 400

                }, ApiClient, ImageManager, SessionManager, NavigationManager, PresentationManager);
            }
        }

        void HomePage_Loaded(object sender, RoutedEventArgs e)
        {
            PresentationManager.SetDefaultPageTitle();

            var parent = ParentItem;

            if (parent == null)
            {
                PresentationManager.ClearBackdrops();
            }
            else
            {
                PresentationManager.SetBackdrops(parent);
            }
        }

        void HomePage_Unloaded(object sender, RoutedEventArgs e)
        {
            DisposeTimer();
        }

        private void DisposeTimer()
        {
            lock (_syncLock)
            {
                if (_selectionChangeTimer != null)
                {
                    _selectionChangeTimer.Stop();
                }
            }
        }
    }
}
