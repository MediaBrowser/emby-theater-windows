using MediaBrowser.Model.ApiClient;
using MediaBrowser.Model.Dto;
using MediaBrowser.Model.Logging;
using MediaBrowser.Model.Net;
using MediaBrowser.Plugins.DefaultTheme.Home.Apps;
using MediaBrowser.Theater.Interfaces.Navigation;
using MediaBrowser.Theater.Interfaces.Presentation;
using MediaBrowser.Theater.Interfaces.Session;
using MediaBrowser.Theater.Presentation.Controls;
using MediaBrowser.Theater.Presentation.Pages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Threading;

namespace MediaBrowser.Plugins.DefaultTheme.Home
{
    /// <summary>
    /// Interaction logic for HomePage.xaml
    /// </summary>
    public partial class HomePage : BasePage, ISupportsItemBackdrops
    {
        /// <summary>
        /// Gets the API client.
        /// </summary>
        /// <value>The API client.</value>
        private readonly IApiClient _apiClient;

        /// <summary>
        /// Gets the image manager.
        /// </summary>
        /// <value>The image manager.</value>
        private readonly IImageManager _imageManager;

        /// <summary>
        /// Gets the session manager.
        /// </summary>
        /// <value>The session manager.</value>
        private readonly ISessionManager _sessionManager;

        /// <summary>
        /// Gets the application window.
        /// </summary>
        /// <value>The application window.</value>
        private readonly IPresentationManager _presentationManager;

        /// <summary>
        /// Gets the navigation manager.
        /// </summary>
        /// <value>The navigation manager.</value>
        private readonly INavigationService _navigationManager;

        private readonly BaseItemDto _parentItem;

        public string DisplayPreferencesId { get; set; }

        private readonly ILogger _logger;

        public HomePage(BaseItemDto parentItem, IApiClient apiClient, IImageManager imageManager, ISessionManager sessionManager, IPresentationManager applicationWindow, INavigationService navigationManager, ILogger logger)
        {
            _navigationManager = navigationManager;
            _presentationManager = applicationWindow;
            _sessionManager = sessionManager;
            _imageManager = imageManager;
            _apiClient = apiClient;
            _logger = logger;
            _parentItem = parentItem;

            InitializeComponent();
        }

        protected override async void OnInitialized(EventArgs e)
        {
            Loaded += HomePage_Loaded;
            Unloaded += HomePage_Unloaded;

            MenuList.SelectionChanged += MenuList_SelectionChanged;

            DisplayPreferencesId = _parentItem.DisplayPreferencesId;

            new ListFocuser(MenuList).FocusAfterContainersGenerated(0);

            var views = new List<string>();

            try
            {
                var itemCounts = await _apiClient.GetItemCountsAsync(_sessionManager.CurrentUser.Id);

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
                _presentationManager.ShowDefaultErrorMessage();
            }

            if (_presentationManager.GetApps(_sessionManager.CurrentUser).Any())
            {
                views.Add("apps");
            }

            views.Add("media collections");

            MenuList.ItemsSource = CollectionViewSource.GetDefaultView(views);

            base.OnInitialized(e);
        }

        private Timer _selectionChangeTimer;
        private readonly object _syncLock = new object();

        void MenuList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            lock (_syncLock)
            {
                if (_selectionChangeTimer == null)
                {
                    _selectionChangeTimer = new Timer(OnSelectionTimerFired, null, 500, Timeout.Infinite);
                }
                else
                {
                    _selectionChangeTimer.Change(500, Timeout.Infinite);
                }
            }
        }

        private void OnSelectionTimerFired(object state)
        {
            Dispatcher.InvokeAsync(UpdatePageContent);
        }

        private void UpdatePageContent()
        {
            var item = MenuList.SelectedItem as string;

            if (string.Equals(item, "movies"))
            {
                PageContent.Content = new Movies.Movies(_parentItem, _apiClient, _imageManager, _sessionManager, _navigationManager, _presentationManager);
            }
            else if (string.Equals(item, "tv"))
            {
                PageContent.Content = new TV.TV(_parentItem, _apiClient, _imageManager, _sessionManager, _navigationManager, _presentationManager);
            }
            else if (string.Equals(item, "music"))
            {
                PageContent.Content = new Music.Music(_parentItem, _apiClient, _imageManager, _sessionManager, _navigationManager, _presentationManager);
            }
            else if (string.Equals(item, "games"))
            {
                PageContent.Content = new TextBlock();
            }
            else if (string.Equals(item, "apps"))
            {
                PageContent.Content = new AppsControl(_presentationManager, _sessionManager, _logger);
            }
            if (string.Equals(item, "media collections"))
            {
                PageContent.Content = new Folders(_parentItem, new Model.Entities.DisplayPreferences
                {
                    PrimaryImageHeight = 225,
                    PrimaryImageWidth = 400

                }, _apiClient, _imageManager, _sessionManager, _navigationManager, _presentationManager);
            }
        }

        void HomePage_Loaded(object sender, RoutedEventArgs e)
        {
            _presentationManager.SetDefaultPageTitle();
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
                    _selectionChangeTimer.Dispose();
                    _selectionChangeTimer = null;
                }
            }
        }

        public BaseItemDto BackdropItem
        {
            get { return _parentItem; }
        }
    }
}
