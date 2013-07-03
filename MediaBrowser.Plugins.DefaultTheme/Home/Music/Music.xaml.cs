using MediaBrowser.Model.ApiClient;
using MediaBrowser.Model.Drawing;
using MediaBrowser.Model.Dto;
using MediaBrowser.Theater.Interfaces.Navigation;
using MediaBrowser.Theater.Interfaces.Presentation;
using MediaBrowser.Theater.Interfaces.Session;
using MediaBrowser.Theater.Interfaces.Theming;
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace MediaBrowser.Plugins.DefaultTheme.Home.Music
{
    /// <summary>
    /// Interaction logic for Music.xaml
    /// </summary>
    public partial class Music : UserControl
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
        protected IScrollInfo ScrollingPanel { get; private set; }

        public Music(BaseItemDto parentItem, IApiClient apiClient, IImageManager imageManager, ISessionManager sessionManager, INavigationService navigationManager, IPresentationManager applicationWindow, IScrollInfo scrollingPanel)
        {
            ScrollingPanel = scrollingPanel;
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
                Width = 207,
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

            //var spotlight = new Spotlight(ApiClient, ImageManager, NavigationManager, PresentationManager);
            //spotlight.ContentLoaded += spotlight_ContentLoaded;

            //GridSpotlight.Children.Add(spotlight);
            GridViews.Children.Add(new LatestAlbums(ParentItem, displayPreferences, ApiClient, ImageManager, SessionManager, NavigationManager, PresentationManager));
            GridLatestAlbums.Children.Add(new LatestAlbums(ParentItem, displayPreferences, ApiClient, ImageManager, SessionManager, NavigationManager, PresentationManager));
            GridLatestSongs.Children.Add(new LatestSongs(ParentItem, displayPreferences, ApiClient, ImageManager, SessionManager, NavigationManager, PresentationManager));
            MainGrid.Visibility = Visibility.Visible;

            var itemCounts = await ApiClient.GetItemCountsAsync(ApiClient.CurrentUserId);

            if (itemCounts.MusicVideoCount > 0)
            {
                LblMusicVideos.Visibility = Visibility.Visible;
                GridLatestMusicVideos.Visibility = Visibility.Visible;
                GridLatestMusicVideos.Children.Add(new LatestMusicVideos(ParentItem, displayPreferences, ApiClient, ImageManager, SessionManager, NavigationManager, PresentationManager));
            }
            else
            {
                LblMusicVideos.Visibility = Visibility.Collapsed;
                GridLatestMusicVideos.Visibility = Visibility.Collapsed;
            }
        }

        void spotlight_ContentLoaded(object sender, EventArgs e)
        {
            MainGrid.Visibility = Visibility.Visible;
            ScrollingPanel.SetHorizontalOffset(750);

            Dispatcher.InvokeAsync(async () =>
            {
                await Task.Delay(50);
                ScrollingPanel.SetHorizontalOffset(750);
            });
        }
    }
}
