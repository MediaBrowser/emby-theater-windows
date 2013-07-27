using MediaBrowser.Model.ApiClient;
using MediaBrowser.Model.Dto;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.Querying;
using MediaBrowser.Plugins.DefaultTheme.Header;
using MediaBrowser.Theater.Interfaces.Navigation;
using MediaBrowser.Theater.Interfaces.Playback;
using MediaBrowser.Theater.Interfaces.Presentation;
using MediaBrowser.Theater.Interfaces.Session;
using MediaBrowser.Theater.Interfaces.Theming;
using MediaBrowser.Theater.Presentation.Controls;
using MediaBrowser.Theater.Presentation.Pages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace MediaBrowser.Plugins.DefaultTheme.Details
{
    /// <summary>
    /// Interaction logic for PanoramaDetailPage.xaml
    /// </summary>
    public partial class PanoramaDetailPage : BasePage
    {
        /// <summary>
        /// Gets the API client.
        /// </summary>
        /// <value>The API client.</value>
        private readonly IApiClient _apiClient;

        /// <summary>
        /// Gets the session manager.
        /// </summary>
        /// <value>The session manager.</value>
        private readonly ISessionManager _sessionManager;

        /// <summary>
        /// The _image manager
        /// </summary>
        private readonly IImageManager _imageManager;

        /// <summary>
        /// The playback manager
        /// </summary>
        private readonly IPlaybackManager _playbackManager;

        private readonly INavigationService _nav;
        private readonly IThemeManager _themeManager;

        /// <summary>
        /// Gets the application window.
        /// </summary>
        /// <value>The application window.</value>
        private readonly IPresentationManager _presentationManager;

        /// <summary>
        /// Initializes a new instance of the <see cref="PanoramaDetailPage"/> class.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <param name="apiClient">The API client.</param>
        /// <param name="sessionManager">The session manager.</param>
        /// <param name="presentationManager">The presentation manager.</param>
        /// <param name="imageManager">The image manager.</param>
        public PanoramaDetailPage(BaseItemDto item, IApiClient apiClient, ISessionManager sessionManager, IPresentationManager presentationManager, IImageManager imageManager, INavigationService nav, IPlaybackManager playbackManager, IThemeManager themeManager)
        {
            _presentationManager = presentationManager;
            _imageManager = imageManager;
            _nav = nav;
            _playbackManager = playbackManager;
            _themeManager = themeManager;
            _sessionManager = sessionManager;
            _apiClient = apiClient;

            _item = item;

            InitializeComponent();
        }

        /// <summary>
        /// The _item
        /// </summary>
        private BaseItemDto _item;
        /// <summary>
        /// Gets or sets the item.
        /// </summary>
        /// <value>The item.</value>
        public BaseItemDto Item
        {
            get { return _item; }

            set
            {
                _item = value;
                LoadItem();
            }
        }

        /// <summary>
        /// Raises the <see cref="E:System.Windows.FrameworkElement.Initialized" /> event. This method is invoked whenever <see cref="P:System.Windows.FrameworkElement.IsInitialized" /> is set to true internally.
        /// </summary>
        /// <param name="e">The <see cref="T:System.Windows.RoutedEventArgs" /> that contains the event data.</param>
        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);

            MenuList.SelectionChanged += MenuList_SelectionChanged;
            Loaded += PanoramaDetailPage_Loaded;

            LoadItem();
        }

        async void PanoramaDetailPage_Loaded(object sender, RoutedEventArgs e)
        {
            await PageTitlePanel.Current.SetPageTitle(Item);

            _presentationManager.SetBackdrops(Item);
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

            if (string.Equals(item, "overview"))
            {
                SetScrollDirection(null);

                PageContent.Content = new Overview(_item, _imageManager, _apiClient, _playbackManager, _presentationManager);
            }
            else if (string.Equals(item, "trailers"))
            {
                SetScrollDirection(ScrollDirection.Horizontal);

                PageContent.Content = new Trailers(new Model.Entities.DisplayPreferences
                {
                    PrimaryImageWidth = 384

                }, _apiClient, _imageManager, _sessionManager, _nav, _presentationManager, _item, _playbackManager);
            }
            else if (string.Equals(item, "gallery"))
            {
                SetScrollDirection(ScrollDirection.Horizontal);

                PageContent.Content = new Gallery(_imageManager, _apiClient, _nav, _themeManager)
                {
                    Item = Item
                };
            }
            else if (string.Equals(item, "special features"))
            {
                SetScrollDirection(ScrollDirection.Horizontal);

                PageContent.Content = new SpecialFeatures(new Model.Entities.DisplayPreferences
                {
                    PrimaryImageWidth = 576

                }, _apiClient, _imageManager, _sessionManager, _nav, _presentationManager, _item, _playbackManager);
            }
            else if (string.Equals(item, "scenes"))
            {
                SetScrollDirection(ScrollDirection.Horizontal);

                PageContent.Content = new Scenes(Item, _apiClient, _imageManager, _playbackManager);
            }
            else if (string.Equals(item, "similar"))
            {
                SetScrollDirection(ScrollDirection.Horizontal);

                PageContent.Content = new SimilarItems(new Model.Entities.DisplayPreferences
                {
                    PrimaryImageWidth = 400

                }, _apiClient, _imageManager, _sessionManager, _nav, _presentationManager, _item);
            }
            else if (string.Equals(item, "people"))
            {
                SetScrollDirection(ScrollDirection.Horizontal);

                PageContent.Content = new People(new Model.Entities.DisplayPreferences
                {
                    PrimaryImageWidth = 300

                }, _apiClient, _imageManager, _sessionManager, _nav, _presentationManager, _item);
            }
            else if (string.Equals(item, "songs") || string.Equals(item, "seasons") || string.Equals(item, "episodes"))
            {
                SetScrollDirection(ScrollDirection.Horizontal);

                PageContent.Content = new Children(new Model.Entities.DisplayPreferences
                {
                    PrimaryImageWidth = 400

                }, _apiClient, _imageManager, _sessionManager, _nav, _presentationManager, _item);
            }
            else if (string.Equals(item, "themes"))
            {
                SetScrollDirection(ScrollDirection.Horizontal);

                PageContent.Content = new ThemeVideos(new Model.Entities.DisplayPreferences
                {
                    PrimaryImageWidth = 576

                }, _apiClient, _imageManager, _sessionManager, _nav, _presentationManager, _playbackManager, Item);
            }
            else if (string.Equals(item, "soundtrack") || string.Equals(item, "soundtracks"))
            {
                SetScrollDirection(ScrollDirection.Horizontal);

                PageContent.Content = new Soundtracks(new Model.Entities.DisplayPreferences
                {
                    PrimaryImageWidth = 400

                }, _apiClient, _imageManager, _sessionManager, _nav, _presentationManager, _item);
            }
            else if (string.Equals(item, "reviews"))
            {
                SetScrollDirection(ScrollDirection.Horizontal);

                PageContent.Content = new Reviews(_item, _apiClient, _presentationManager);
            }
        }

        private void SetScrollDirection(ScrollDirection? direction)
        {
            if (direction == null)
            {
                System.Windows.Controls.ScrollViewer.SetHorizontalScrollBarVisibility(ScrollViewer, ScrollBarVisibility.Disabled);
                System.Windows.Controls.ScrollViewer.SetVerticalScrollBarVisibility(ScrollViewer, ScrollBarVisibility.Disabled);
                ScrollingPanel.CanHorizontallyScroll = false;
                ScrollingPanel.CanVerticallyScroll = false;
            }
            else if (direction == ScrollDirection.Horizontal)
            {
                System.Windows.Controls.ScrollViewer.SetHorizontalScrollBarVisibility(ScrollViewer, ScrollBarVisibility.Hidden);
                System.Windows.Controls.ScrollViewer.SetVerticalScrollBarVisibility(ScrollViewer, ScrollBarVisibility.Disabled);
                ScrollingPanel.CanHorizontallyScroll = true;
                ScrollingPanel.CanVerticallyScroll = false;
            }
            else
            {
                System.Windows.Controls.ScrollViewer.SetHorizontalScrollBarVisibility(ScrollViewer, ScrollBarVisibility.Disabled);
                System.Windows.Controls.ScrollViewer.SetVerticalScrollBarVisibility(ScrollViewer, ScrollBarVisibility.Hidden);
                ScrollingPanel.CanHorizontallyScroll = false;
                ScrollingPanel.CanVerticallyScroll = true;
            }
        }

        protected override void FocusOnFirstLoad()
        {
        }

        /// <summary>
        /// Called when [item changed].
        /// </summary>
        protected async void LoadItem()
        {
            ItemInfoFooter.Item = Item;

            SetTitle(Item);
            RenderDetailControls(Item);
        }

        private void SetTitle(BaseItemDto item)
        {
            if (item.Taglines.Count > 0)
            {
                TxtName.Text = item.Taglines[0];
                TxtName.Visibility = Visibility.Visible;
            }
            else if (item.IsType("episode"))
            {
                TxtName.Text = GetEpisodeTitle(item);
                TxtName.Visibility = Visibility.Visible;
            }
            else if (item.IsType("audio"))
            {
                TxtName.Text = GetSongTitle(item);
                TxtName.Visibility = Visibility.Visible;
            }
            else
            {
                TxtName.Visibility = Visibility.Collapsed;
            }
        }

        internal static string GetEpisodeTitle(BaseItemDto item)
        {
            var title = item.Name;

            if (item.IndexNumber.HasValue)
            {
                title = "Ep. " + item.IndexNumber.Value + ": " + title;
            }

            if (item.ParentIndexNumber.HasValue)
            {
                title = "Season " + item.ParentIndexNumber.Value + ", " + title;
            }

            return title;
        }

        private string GetSongTitle(BaseItemDto item)
        {
            return item.Name;
        }

        private async void RenderDetailControls(BaseItemDto item)
        {
            var themeMediaTask = _apiClient.GetAllThemeMediaAsync(_sessionManager.CurrentUser.Id, item.Id, false);
            var criticReviewsTask = _apiClient.GetCriticReviews(item.Id);

            try
            {
                await Task.WhenAll(themeMediaTask, criticReviewsTask);
            }
            catch (Exception ex)
            {
                // Logged at lower levels
            }

            var allThemeMedia = themeMediaTask.IsCompleted ? themeMediaTask.Result : new AllThemeMediaResult();
            var criticReviews = criticReviewsTask.IsCompleted ? criticReviewsTask.Result : new ItemReviewsResult();

            LoadMenuList(item, allThemeMedia, criticReviews);
        }

        private void LoadMenuList(BaseItemDto item, AllThemeMediaResult themeMediaResult, ItemReviewsResult reviewsResult)
        {
            new ListFocuser(MenuList).FocusAfterContainersGenerated(0);

            var views = new List<string>();

            views.Add("overview");

            if (item.ChildCount > 0)
            {
                if (item.IsType("series"))
                {
                    views.Add("seasons");
                }
                else if (item.IsType("season"))
                {
                    views.Add("episodes");
                }
                else if (item.IsType("musicalbum"))
                {
                    views.Add("songs");
                }
            }

            if (item.People.Length > 0)
            {
                views.Add("people");
            }

            if (item.LocalTrailerCount > 1)
            {
                views.Add("trailers");
            }

            if (item.Chapters != null && item.Chapters.Count > 0)
            {
                views.Add("scenes");
            }

            if (item.SpecialFeatureCount > 0)
            {
                views.Add("special features");
            }

            if (item.IsType("movie") || item.IsType("trailer") || item.IsType("series") || item.IsType("musicalbum") || item.IsGame)
            {
                views.Add("similar");
            }

            if (reviewsResult.TotalRecordCount > 0 || !string.IsNullOrEmpty(item.CriticRatingSummary))
            {
                views.Add("reviews");
            }

            if (item.SoundtrackIds != null)
            {
                if (item.SoundtrackIds.Length > 1)
                {
                    views.Add("soundtracks");
                }
                else if (item.SoundtrackIds.Length > 0)
                {
                    views.Add("soundtrack");
                }
            }

            if (themeMediaResult.ThemeVideosResult.TotalRecordCount > 0 || themeMediaResult.ThemeSongsResult.TotalRecordCount > 0)
            {
                views.Add("themes");
            }

            if (Gallery.GetImages(item, _apiClient, null, null).Any())
            {
                views.Add("gallery");
            }

            MenuList.ItemsSource = CollectionViewSource.GetDefaultView(views);
        }
    }
}
