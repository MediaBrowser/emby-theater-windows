using MediaBrowser.Model.ApiClient;
using MediaBrowser.Model.Dto;
using MediaBrowser.Theater.Interfaces.Navigation;
using MediaBrowser.Theater.Interfaces.Presentation;
using MediaBrowser.Theater.Interfaces.Session;
using MediaBrowser.Theater.Interfaces.Theming;
using System;
using System.Windows.Controls;

namespace MediaBrowser.Plugins.DefaultTheme.Home.TV
{
    /// <summary>
    /// Interaction logic for TV.xaml
    /// </summary>
    public partial class TV : UserControl
    {
        private BaseItemDto ParentItem { get; set; }

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
        /// Gets the navigation manager.
        /// </summary>
        /// <value>The navigation manager.</value>
        protected INavigationService NavigationManager { get; private set; }
        /// <summary>
        /// Gets the theme manager.
        /// </summary>
        /// <value>The theme manager.</value>
        protected IThemeManager ThemeManager { get; private set; }
        
        public TV(BaseItemDto parentItem, IApiClient apiClient, IImageManager imageManager, ISessionManager sessionManager, INavigationService navigationManager, IThemeManager themeManager)
        {
            ThemeManager = themeManager;
            NavigationManager = navigationManager;
            SessionManager = sessionManager;
            ImageManager = imageManager;
            ApiClient = apiClient;
            ParentItem = parentItem;
            InitializeComponent();
        }

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);

            GridSpotlight.Children.Add(new ResumableEpisodes(ParentItem, ParentItem.DisplayPreferencesId, ApiClient, ImageManager, SessionManager, NavigationManager, ThemeManager));
            GridResume.Children.Add(new ResumableEpisodes(ParentItem, ParentItem.DisplayPreferencesId, ApiClient, ImageManager, SessionManager, NavigationManager, ThemeManager));
            GridViews.Children.Add(new ResumableEpisodes(ParentItem, ParentItem.DisplayPreferencesId, ApiClient, ImageManager, SessionManager, NavigationManager, ThemeManager));
            GridNextUp.Children.Add(new NextUp(ParentItem, ParentItem.DisplayPreferencesId, ApiClient, ImageManager, SessionManager, NavigationManager, ThemeManager));
        }
    }
}
