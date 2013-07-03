using MediaBrowser.Model.ApiClient;
using MediaBrowser.Model.Dto;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.Net;
using MediaBrowser.Plugins.DefaultTheme.Controls.Details;
using MediaBrowser.Plugins.DefaultTheme.Resources;
using MediaBrowser.Theater.Interfaces.Playback;
using MediaBrowser.Theater.Interfaces.Presentation;
using MediaBrowser.Theater.Interfaces.Session;
using MediaBrowser.Theater.Interfaces.Theming;
using System;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using MediaBrowser.Theater.Presentation.Pages;

namespace MediaBrowser.Plugins.DefaultTheme.Pages
{
    /// <summary>
    /// Interaction logic for DetailPage.xaml
    /// </summary>
    public partial class DetailPage : BasePage, INotifyPropertyChanged
    {
        /// <summary>
        /// Gets the API client.
        /// </summary>
        /// <value>The API client.</value>
        protected IApiClient ApiClient { get; private set; }
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
        /// Gets the theme manager.
        /// </summary>
        /// <value>The theme manager.</value>
        protected IThemeManager ThemeManager { get; private set; }

        /// <summary>
        /// Occurs when a property value changes.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;
        
        /// <summary>
        /// The _image manager
        /// </summary>
        private readonly IImageManager _imageManager;

        /// <summary>
        /// The playback manager
        /// </summary>
        private readonly IPlaybackManager _playbackManager;

        /// <summary>
        /// Initializes a new instance of the <see cref="DetailPage" /> class.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <param name="imageManager">The image manager.</param>
        /// <param name="playbackManager">The playback manager.</param>
        /// <param name="apiClient">The API client.</param>
        /// <param name="sessionManager">The session manager.</param>
        /// <param name="applicationWindow">The application window.</param>
        /// <param name="themeManager">The theme manager.</param>
        public DetailPage(BaseItemDto item, IImageManager imageManager, IPlaybackManager playbackManager, IApiClient apiClient, ISessionManager sessionManager, IPresentationManager applicationWindow, IThemeManager themeManager)
        {
            if (item == null)
            {
                throw new ArgumentNullException("item");
            }

            _item = item;
            _imageManager = imageManager;
            _playbackManager = playbackManager;
            ThemeManager = themeManager;
            PresentationManager = applicationWindow;
            SessionManager = sessionManager;
            ApiClient = apiClient;
            InitializeComponent();

            BtnOverview.Click += BtnOverview_Click;
            BtnChapters.Click += BtnChapters_Click;
            BtnMediaInfo.Click += BtnDetails_Click;
            BtnPerformers.Click += BtnPerformers_Click;
            BtnTrailers.Click += BtnTrailers_Click;
            BtnSpecialFeatures.Click += BtnSpecialFeatures_Click;
            BtnGallery.Click += BtnGallery_Click;

            Loaded += DetailPage_Loaded;

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
                OnPropertyChanged("Item");
            }
        }

        /// <summary>
        /// Called when [property changed].
        /// </summary>
        /// <param name="name">The name.</param>
        public void OnPropertyChanged(string name)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(name));
            }

            if (string.Equals(name, "Item"))
            {
                OnItemChanged();
            }
        }

        /// <summary>
        /// Raises the <see cref="E:System.Windows.FrameworkElement.Initialized" /> event. This method is invoked whenever <see cref="P:System.Windows.FrameworkElement.IsInitialized" /> is set to true internally.
        /// </summary>
        /// <param name="e">The <see cref="T:System.Windows.RoutedEventArgs" /> that contains the event data.</param>
        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);

            DataContext = this;

            OnPropertyChanged("Item");
        }
        
        /// <summary>
        /// Handles the Loaded event of the DetailPage control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs" /> instance containing the event data.</param>
        async void DetailPage_Loaded(object sender, RoutedEventArgs e)
        {
            if (Item != null)
            {
                await AppResources.Instance.SetPageTitle(Item);
            }
        }

        /// <summary>
        /// Handles the Click event of the BtnGallery control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs" /> instance containing the event data.</param>
        void BtnGallery_Click(object sender, RoutedEventArgs e)
        {
            PrimaryImageGrid.Visibility = Visibility.Collapsed;
            ShowDetailControl(BtnGallery, new ItemGallery(ApiClient, _imageManager) { });
        }

        /// <summary>
        /// Handles the Click event of the BtnSpecialFeatures control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs" /> instance containing the event data.</param>
        void BtnSpecialFeatures_Click(object sender, RoutedEventArgs e)
        {
            PrimaryImageGrid.Visibility = Visibility.Collapsed;
            ShowDetailControl(BtnSpecialFeatures, new ItemSpecialFeatures(ApiClient, _imageManager, SessionManager, PresentationManager) { });
        }

        /// <summary>
        /// Handles the Click event of the BtnTrailers control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs" /> instance containing the event data.</param>
        void BtnTrailers_Click(object sender, RoutedEventArgs e)
        {
            PrimaryImageGrid.Visibility = Visibility.Collapsed;
            ShowDetailControl(BtnTrailers, new ItemTrailers(ApiClient, _imageManager, SessionManager, PresentationManager) { });
        }

        /// <summary>
        /// Handles the Click event of the BtnDetails control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs" /> instance containing the event data.</param>
        void BtnDetails_Click(object sender, RoutedEventArgs e)
        {
            PrimaryImageGrid.Visibility = Visibility.Visible;
            ShowDetailControl(BtnMediaInfo, new ItemMediaInfo { });
        }

        /// <summary>
        /// Handles the Click event of the BtnChapters control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs" /> instance containing the event data.</param>
        void BtnChapters_Click(object sender, RoutedEventArgs e)
        {
            PrimaryImageGrid.Visibility = Visibility.Collapsed;
            ShowDetailControl(BtnChapters, new ItemChapters(ApiClient, _imageManager) { });
        }

        /// <summary>
        /// Handles the Click event of the BtnOverview control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs" /> instance containing the event data.</param>
        void BtnOverview_Click(object sender, RoutedEventArgs e)
        {
            PrimaryImageGrid.Visibility = Visibility.Visible;
            ShowDetailControl(BtnOverview, new ItemOverview(_playbackManager));
        }

        /// <summary>
        /// Handles the Click event of the BtnPerformers control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs" /> instance containing the event data.</param>
        void BtnPerformers_Click(object sender, RoutedEventArgs e)
        {
            PrimaryImageGrid.Visibility = Visibility.Collapsed;
            ShowDetailControl(BtnPerformers, new ItemPerformers(ApiClient, _imageManager, SessionManager) { });
        }

        /// <summary>
        /// Called when [item changed].
        /// </summary>
        protected async void OnItemChanged()
        {
            PresentationManager.SetBackdrops(Item);

            var pageTitleTask = AppResources.Instance.SetPageTitle(Item);

            BtnOverview_Click(null, null);

            RenderItem();

            ItemInfoFooter.Item = Item;

            await UpdateLogo(Item);

            await pageTitleTask;
        }

        /// <summary>
        /// Updates the logo.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <returns>Task.</returns>
        private async Task UpdateLogo(BaseItemDto item)
        {
            if (!item.HasArtImage && item.IsType("episode"))
            {
                item = await ApiClient.GetItemAsync(item.SeriesId, SessionManager.CurrentUser.Id);
            }

            // Hide it for movies, for now. It looks really tacky being right under the movie poster
            // Creating extra spacing shrinks the poster too much
            if (item.HasArtImage && !item.IsType("movie"))
            {
                ImgLogo.Visibility = Visibility.Visible;

                ImgLogo.Source =
                    await _imageManager.GetRemoteBitmapAsync(ApiClient.GetImageUrl(item, new ImageOptions
                    {
                        MaxHeight = 200,
                        ImageType = ImageType.Art
                    }));
            }
            else
            {
                ImgLogo.Visibility = Visibility.Collapsed;
            }
        }

        /// <summary>
        /// Renders the item.
        /// </summary>
        private async void RenderItem()
        {
            Task<BitmapImage> primaryImageTask = null;

            if (Item.HasPrimaryImage)
            {
                PrimaryImage.Visibility = Visibility.Visible;

                primaryImageTask = _imageManager.GetRemoteBitmapAsync(ApiClient.GetImageUrl(Item, new ImageOptions
                {
                    ImageType = ImageType.Primary
                }));
            }
            else
            {
                SetDefaultImage();
            }

            if (Item.IsType("movie") || Item.IsType("trailer"))
            {
                TxtName.Visibility = Visibility.Collapsed;
            }
            else
            {
                var name = Item.Name;

                if (Item.IndexNumber.HasValue)
                {
                    name = Item.IndexNumber.Value + " - " + name;

                    if (Item.ParentIndexNumber.HasValue)
                    {
                        name = Item.ParentIndexNumber.Value + "." + name;
                    }
                }
                TxtName.Text = name;

                TxtName.Visibility = Visibility.Visible;
            }

            if (Item.Taglines != null && Item.Taglines.Count > 0)
            {
                Tagline.Visibility = Visibility.Visible;

                Tagline.Text = Item.Taglines[0];
            }
            else
            {
                Tagline.Visibility = Visibility.Collapsed;
            }

            BtnGallery.Visibility = ItemGallery.GetImages(Item, ApiClient).Count > 0 ? Visibility.Visible : Visibility.Collapsed;
            BtnTrailers.Visibility = Item.LocalTrailerCount > 0 ? Visibility.Visible : Visibility.Collapsed;
            BtnSpecialFeatures.Visibility = Item.SpecialFeatureCount > 0 ? Visibility.Visible : Visibility.Collapsed;
            BtnPerformers.Visibility = Item.People != null && Item.People.Length > 0 ? Visibility.Visible : Visibility.Collapsed;
            BtnChapters.Visibility = Item.Chapters != null && Item.Chapters.Count > 0 ? Visibility.Visible : Visibility.Collapsed;

            if (primaryImageTask != null)
            {
                try
                {
                    PrimaryImage.Source = await primaryImageTask;
                }
                catch (HttpException)
                {
                    SetDefaultImage();
                }
            }
        }

        /// <summary>
        /// Sets the default image.
        /// </summary>
        private void SetDefaultImage()
        {
            PrimaryImage.Visibility = Visibility.Collapsed;
        }

        /// <summary>
        /// Shows the detail control.
        /// </summary>
        /// <param name="button">The button.</param>
        /// <param name="element">The element.</param>
        private void ShowDetailControl(Button button, BaseDetailsControl element)
        {
            DetailContent.Content = element;
            element.Item = Item;
        }
    }
}
