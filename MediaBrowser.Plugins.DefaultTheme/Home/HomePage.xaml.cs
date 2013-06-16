using MediaBrowser.Model.ApiClient;
using MediaBrowser.Model.Dto;
using MediaBrowser.Theater.Interfaces.Navigation;
using MediaBrowser.Theater.Interfaces.Presentation;
using MediaBrowser.Theater.Interfaces.Session;
using MediaBrowser.Theater.Interfaces.Theming;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using MediaBrowser.Theater.Presentation.Controls;

namespace MediaBrowser.Plugins.DefaultTheme.Home
{
    /// <summary>
    /// Interaction logic for HomePage.xaml
    /// </summary>
    public partial class HomePage : Page
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
        protected IApplicationWindow ApplicationWindow { get; private set; }
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

        public HomePage(BaseItemDto parent, string displayPreferencesId, IApiClient apiClient, IImageManager imageManager, ISessionManager sessionManager, IApplicationWindow applicationWindow, INavigationService navigationManager, IThemeManager themeManager)
        {
            InitializeComponent();

            NavigationManager = navigationManager;
            ApplicationWindow = applicationWindow;
            SessionManager = sessionManager;
            ImageManager = imageManager;
            ApiClient = apiClient;
            DisplayPreferencesId = displayPreferencesId;

            ParentItem = parent;
            ThemeManager = themeManager;
        }

        protected override void OnInitialized(EventArgs e)
        {
            Loaded += HomePage_Loaded;

            MenuList.SelectionChanged += MenuList_SelectionChanged;
            new ListFocuser(MenuList).FocusAfterContainersGenerated(0);

            MenuList.ItemsSource = CollectionViewSource.GetDefaultView(new[] { "movies", "tv", "music", "games", "apps", "folders" });

            base.OnInitialized(e);
        }

        void MenuList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var item = MenuList.SelectedItem as string;

            if (string.Equals(item, "movies"))
            {
                PageContent.Content = new Movies();
            }
            else if (string.Equals(item, "tv"))
            {
                PageContent.Content = new TV();
            }
            else if (string.Equals(item, "music"))
            {
                PageContent.Content = new Movies();
            }
            else if (string.Equals(item, "games"))
            {
                PageContent.Content = new Movies();
            }
            else if (string.Equals(item, "apps"))
            {
                PageContent.Content = new Movies();
            }
            if (string.Equals(item, "folders"))
            {
                PageContent.Content = new Folders(ParentItem, DisplayPreferencesId, ApiClient, ImageManager, SessionManager, ApplicationWindow, NavigationManager, ThemeManager);
            }
        }

        void HomePage_Loaded(object sender, RoutedEventArgs e)
        {
            ThemeManager.CurrentTheme.SetDefaultPageTitle();

            var parent = ParentItem;

            if (parent == null)
            {
                ApplicationWindow.ClearBackdrops();
            }
            else
            {
                ApplicationWindow.SetBackdrops(parent);
            }
        }
    }
}
