using MediaBrowser.Model.ApiClient;
using MediaBrowser.Model.Dto;
using MediaBrowser.Model.Net;
using MediaBrowser.Theater.Interfaces.Navigation;
using MediaBrowser.Theater.Interfaces.Presentation;
using MediaBrowser.Theater.Interfaces.Session;
using MediaBrowser.Theater.Interfaces.Theming;
using MediaBrowser.Theater.Presentation.Controls;
using System;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

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

        protected IApplicationWindow ApplicationWindow { get; private set; }
        protected IScrollInfo ScrollingPanel { get; private set; }

        public TV(BaseItemDto parentItem, IApiClient apiClient, IImageManager imageManager, ISessionManager sessionManager, INavigationService navigationManager, IThemeManager themeManager, IApplicationWindow applicationWindow, IScrollInfo scrollingPanel)
        {
            ScrollingPanel = scrollingPanel;
            ApplicationWindow = applicationWindow;
            ThemeManager = themeManager;
            NavigationManager = navigationManager;
            SessionManager = sessionManager;
            ImageManager = imageManager;
            ApiClient = apiClient;
            ParentItem = parentItem;
            InitializeComponent();
        }

        protected override async void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);

            Model.Entities.DisplayPreferences displayPreferences;

            try
            {
                displayPreferences = await ApiClient.GetDisplayPreferencesAsync(ParentItem.DisplayPreferencesId);
            }
            catch (HttpException)
            {
                ThemeManager.CurrentTheme.ShowDefaultErrorMessage();
                return;
            }

            var spotlight = new Spotlight(ApiClient, ImageManager, NavigationManager, ApplicationWindow);
            spotlight.ContentLoaded += spotlight_ContentLoaded;

            GridSpotlight.Children.Add(spotlight);
            GridResume.Children.Add(new ResumableEpisodes(ParentItem, displayPreferences, ApiClient, ImageManager, SessionManager, NavigationManager, ThemeManager, ApplicationWindow));
            GridViews.Children.Add(new ResumableEpisodes(ParentItem, displayPreferences, ApiClient, ImageManager, SessionManager, NavigationManager, ThemeManager, ApplicationWindow));
            GridNextUp.Children.Add(new NextUp(ParentItem, displayPreferences, ApiClient, ImageManager, SessionManager, NavigationManager, ThemeManager, ApplicationWindow));

        }

        void spotlight_ContentLoaded(object sender, EventArgs e)
        {
            MainGrid.Visibility = System.Windows.Visibility.Visible;
            ScrollingPanel.SetHorizontalOffset(750);

            Dispatcher.InvokeAsync(async () =>
            {
                await Task.Delay(50);
                ScrollingPanel.SetHorizontalOffset(750);
            });
        }
    }
}
