using System.Linq;
using System.Windows.Controls;
using MediaBrowser.Model.ApiClient;
using MediaBrowser.Model.Dto;
using MediaBrowser.Model.Entities;
using MediaBrowser.Plugins.DefaultTheme.Header;
using MediaBrowser.Theater.Interfaces.Navigation;
using MediaBrowser.Theater.Interfaces.Playback;
using MediaBrowser.Theater.Interfaces.Presentation;
using MediaBrowser.Theater.Interfaces.Session;
using MediaBrowser.Theater.Presentation.Pages;
using System;
using System.Threading.Tasks;
using System.Windows;

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
        public PanoramaDetailPage(BaseItemDto item, IApiClient apiClient, ISessionManager sessionManager, IPresentationManager presentationManager, IImageManager imageManager, INavigationService nav)
        {
            _presentationManager = presentationManager;
            _imageManager = imageManager;
            _nav = nav;
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

            Loaded += DetailPage_Loaded;
        }

        /// <summary>
        /// Handles the Loaded event of the DetailPage control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs" /> instance containing the event data.</param>
        void DetailPage_Loaded(object sender, RoutedEventArgs e)
        {
            if (Item != null)
            {
                OnItemChanged();
            }
        }

        /// <summary>
        /// Called when [item changed].
        /// </summary>
        protected async void OnItemChanged()
        {
            _presentationManager.SetBackdrops(Item);

            var pageTitleTask = PageTitlePanel.Current.SetPageTitle(Item);

            RenderItem(Item);

            ItemInfoFooter.Item = Item;

            RenderDetailControls(Item);

            await UpdateLogo(Item);

            await pageTitleTask;
        }

        private void RenderDetailControls(BaseItemDto item)
        {
            DetailPanels.Children.Clear();
            DetailPanels.ColumnDefinitions.Clear();

            if (item.LocalTrailerCount > 0)
            {
                DetailPanels.Children.Add(new Trailers(new Model.Entities.DisplayPreferences
                {
                    PrimaryImageWidth = 384

                }, _apiClient, _imageManager, _sessionManager, _nav, _presentationManager, _item));
            }

            if (item.People.Length > 0)
            {
                DetailPanels.Children.Add(new People(new Model.Entities.DisplayPreferences
                {
                    PrimaryImageWidth = 200

                }, _apiClient, _imageManager, _sessionManager, _nav, _presentationManager, _item));
            }

            if (item.Chapters.Count > 0)
            {
                DetailPanels.Children.Add(new Scenes(item, _apiClient, _imageManager));
            }

            if (item.SpecialFeatureCount > 0)
            {
                DetailPanels.Children.Add(new SpecialFeatures(new Model.Entities.DisplayPreferences
                {
                    PrimaryImageWidth = 384

                }, _apiClient, _imageManager, _sessionManager, _nav, _presentationManager, _item));
            }

            if (Gallery.GetImages(item, _apiClient).Any())
            {
                DetailPanels.Children.Add(new Gallery(_imageManager, _apiClient)
                {
                    Item = item
                });
            }

            var columnIndex = 0;

            foreach (var child in DetailPanels.Children.OfType<UIElement>())
            {
                DetailPanels.ColumnDefinitions.Add(new ColumnDefinition());
                
                Grid.SetColumn(child, columnIndex++);
            }
        }

        /// <summary>
        /// Updates the logo.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <returns>Task.</returns>
        private async Task UpdateLogo(BaseItemDto item)
        {
            if (item != null && (item.HasArtImage || item.ParentArtImageTag.HasValue))
            {
                ImgLogo.Source = await _imageManager.GetRemoteBitmapAsync(_apiClient.GetArtImageUrl(item, new ImageOptions
                {
                    MaxHeight = 80,
                    ImageType = ImageType.Art
                }));

                ImgLogo.Visibility = Visibility.Visible;
            }
            else if (item != null && item.HasDiscImage)
            {
                ImgLogo.Source = await _imageManager.GetRemoteBitmapAsync(_apiClient.GetImageUrl(item, new ImageOptions
                {
                    MaxHeight = 80,
                    ImageType = ImageType.Disc
                }));

                ImgLogo.Visibility = Visibility.Visible;
            }
            else if (item != null && item.HasBoxImage)
            {
                ImgLogo.Source = await _imageManager.GetRemoteBitmapAsync(_apiClient.GetImageUrl(item, new ImageOptions
                {
                    MaxHeight = 80,
                    ImageType = ImageType.Box
                }));

                ImgLogo.Visibility = Visibility.Visible;
            }
            else if (item != null && item.HasBoxImage)
            {
                ImgLogo.Source = await _imageManager.GetRemoteBitmapAsync(_apiClient.GetImageUrl(item, new ImageOptions
                {
                    MaxHeight = 80,
                    ImageType = ImageType.BoxRear
                }));

                ImgLogo.Visibility = Visibility.Visible;
            }
            else
            {
                // Just hide it so that it still takes up the same amount of space
                ImgLogo.Visibility = Visibility.Hidden;
            }
        }

        /// <summary>
        /// Renders the item.
        /// </summary>
        private async void RenderItem(BaseItemDto item)
        {
            //Task<BitmapImage> primaryImageTask = null;

            //if (Item.HasPrimaryImage)
            //{
            //    PrimaryImage.Visibility = Visibility.Visible;

            //    primaryImageTask = _imageManager.GetRemoteBitmapAsync(ApiClient.GetImageUrl(Item, new ImageOptions
            //    {
            //        ImageType = ImageType.Primary
            //    }));
            //}
            //else
            //{
            //    SetDefaultImage();
            //}

            //if (Item.IsType("movie") || Item.IsType("trailer"))
            //{
            //    TxtName.Visibility = Visibility.Collapsed;
            //}
            //else
            //{
            //    var name = Item.Name;

            //    if (Item.IndexNumber.HasValue)
            //    {
            //        name = Item.IndexNumber.Value + " - " + name;

            //        if (Item.ParentIndexNumber.HasValue)
            //        {
            //            name = Item.ParentIndexNumber.Value + "." + name;
            //        }
            //    }
            //    TxtName.Text = name;

            //    TxtName.Visibility = Visibility.Visible;
            //}

            //if (item.Taglines.Count > 0)
            //{
            //    Tagline.Visibility = Visibility.Visible;

            //    Tagline.Text = Item.Taglines[0];
            //}
            //else
            //{
            //    Tagline.Visibility = Visibility.Collapsed;
            //}

            //BtnGallery.Visibility = ItemGallery.GetImages(Item, ApiClient).Count > 0 ? Visibility.Visible : Visibility.Collapsed;
            //BtnTrailers.Visibility = Item.LocalTrailerCount > 0 ? Visibility.Visible : Visibility.Collapsed;
            //BtnSpecialFeatures.Visibility = Item.SpecialFeatureCount > 0 ? Visibility.Visible : Visibility.Collapsed;
            //BtnPerformers.Visibility = Item.People != null && Item.People.Length > 0 ? Visibility.Visible : Visibility.Collapsed;
            //BtnChapters.Visibility = Item.Chapters != null && Item.Chapters.Count > 0 ? Visibility.Visible : Visibility.Collapsed;

            //if (primaryImageTask != null)
            //{
            //    try
            //    {
            //        PrimaryImage.Source = await primaryImageTask;
            //    }
            //    catch (HttpException)
            //    {
            //        SetDefaultImage();
            //    }
            //}
        }

        /// <summary>
        /// Sets the default image.
        /// </summary>
        private void SetDefaultImage()
        {
            //PrimaryImage.Visibility = Visibility.Collapsed;
        }
    }
}
