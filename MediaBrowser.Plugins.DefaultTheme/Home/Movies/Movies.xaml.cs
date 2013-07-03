using MediaBrowser.Model.ApiClient;
using MediaBrowser.Model.Drawing;
using MediaBrowser.Model.Dto;
using MediaBrowser.Theater.Interfaces.Navigation;
using MediaBrowser.Theater.Interfaces.Presentation;
using MediaBrowser.Theater.Interfaces.Session;
using System;
using System.Windows;
using System.Windows.Controls;

namespace MediaBrowser.Plugins.DefaultTheme.Home.Movies
{
    /// <summary>
    /// Interaction logic for Movies.xaml
    /// </summary>
    public partial class Movies : UserControl
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

        protected IPresentationManager PresentationManager { get; private set; }

        public Movies(BaseItemDto parentItem, IApiClient apiClient, IImageManager imageManager, ISessionManager sessionManager, INavigationService navigationManager, IPresentationManager applicationWindow)
        {
            PresentationManager = applicationWindow;
            NavigationManager = navigationManager;
            SessionManager = sessionManager;
            ImageManager = imageManager;
            ApiClient = apiClient;
            ParentItem = parentItem;
            InitializeComponent();
        }

        internal static ImageSize GetImageSize()
        {
            return new ImageSize
            {
                Width = 368,
                Height = 207
            };
        }

        protected override async void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);

            var size = GetImageSize();

            var displayPreferences = new Model.Entities.DisplayPreferences
            {
                PrimaryImageWidth = Convert.ToInt32(size.Width),
                PrimaryImageHeight = Convert.ToInt32(size.Height)
            };

            var spotlight = new MovieSpotlight(ApiClient, ImageManager, NavigationManager, PresentationManager);
            spotlight.ContentLoaded += spotlight_ContentLoaded;

            GridSpotlight.Children.Add(spotlight);

            GridResume.Children.Add(new ResumableMovies(ParentItem, displayPreferences, ApiClient, ImageManager, SessionManager, NavigationManager, PresentationManager));
            GridViews.Children.Add(new ResumableMovies(ParentItem, displayPreferences, ApiClient, ImageManager, SessionManager, NavigationManager, PresentationManager));

            GridTrailers.Children.Add(new RecentTrailers(ParentItem, new Model.Entities.DisplayPreferences
            {
                PrimaryImageWidth = 138,
                PrimaryImageHeight = Convert.ToInt32(size.Height)

            }, ApiClient, ImageManager, SessionManager, NavigationManager, PresentationManager));
        }

        void spotlight_ContentLoaded(object sender, EventArgs e)
        {
            MainGrid.Visibility = Visibility.Visible;
        }
    }
}
