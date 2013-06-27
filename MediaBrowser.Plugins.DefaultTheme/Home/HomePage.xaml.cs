using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MediaBrowser.Model.ApiClient;
using MediaBrowser.Model.Dto;
using MediaBrowser.Model.Net;
using MediaBrowser.Theater.Interfaces.Navigation;
using MediaBrowser.Theater.Interfaces.Presentation;
using MediaBrowser.Theater.Interfaces.Session;
using MediaBrowser.Theater.Interfaces.Theming;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using MediaBrowser.Theater.Presentation.Controls;
using MediaBrowser.Theater.Presentation.Pages;

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
        /// <summary>
        /// Gets the theme manager.
        /// </summary>
        /// <value>The theme manager.</value>
        protected IThemeManager ThemeManager { get; private set; }

        public BaseItemDto ParentItem { get; set; }
        public string DisplayPreferencesId { get; set; }

        public HomePage(BaseItemDto parent, string displayPreferencesId, IApiClient apiClient, IImageManager imageManager, ISessionManager sessionManager, IPresentationManager applicationWindow, INavigationService navigationManager, IThemeManager themeManager)
        {
            NavigationManager = navigationManager;
            PresentationManager = applicationWindow;
            SessionManager = sessionManager;
            ImageManager = imageManager;
            ApiClient = apiClient;
            DisplayPreferencesId = displayPreferencesId;

            ParentItem = parent;
            ThemeManager = themeManager;

            InitializeComponent();
        }

        protected override async void OnInitialized(EventArgs e)
        {
            Loaded += HomePage_Loaded;

            MenuList.SelectionChanged += MenuList_SelectionChanged;
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
                ThemeManager.CurrentTheme.ShowDefaultErrorMessage();
            }

            if (PresentationManager.Apps.Any())
            {
                views.Add("apps");
            }

            views.Add("media collections");

            MenuList.ItemsSource = CollectionViewSource.GetDefaultView(views);

            base.OnInitialized(e);
        }

        void MenuList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var item = MenuList.SelectedItem as string;

            ScrollingPanel.SetHorizontalOffset(0);

            if (string.Equals(item, "movies"))
            {
                PageContent.Content = new Movies.Movies(ParentItem, ApiClient, ImageManager, SessionManager, NavigationManager, ThemeManager, PresentationManager, ScrollingPanel);
            }
            else if (string.Equals(item, "tv"))
            {
                PageContent.Content = new TV.TV(ParentItem, ApiClient, ImageManager, SessionManager, NavigationManager, ThemeManager, PresentationManager, ScrollingPanel);
            }
            else if (string.Equals(item, "music"))
            {
                PageContent.Content = new Music.Music(ParentItem, ApiClient, ImageManager, SessionManager, NavigationManager, ThemeManager, PresentationManager, ScrollingPanel);
            }
            else if (string.Equals(item, "games"))
            {
                PageContent.Content = new TextBlock();
            }
            else if (string.Equals(item, "apps"))
            {
                PageContent.Content = new TextBlock();
            }
            if (string.Equals(item, "media collections"))
            {
                PageContent.Content = new Folders(ParentItem, new Model.Entities.DisplayPreferences
                {
                    PrimaryImageHeight = 225,
                    PrimaryImageWidth = 400

                }, ApiClient, ImageManager, SessionManager, PresentationManager, NavigationManager, ThemeManager);
            }
        }

        void HomePage_Loaded(object sender, RoutedEventArgs e)
        {
            ThemeManager.CurrentTheme.SetDefaultPageTitle();

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
    }
}
