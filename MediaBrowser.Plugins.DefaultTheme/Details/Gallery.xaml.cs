using MediaBrowser.Model.ApiClient;
using MediaBrowser.Model.Dto;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.Net;
using MediaBrowser.Theater.Interfaces.Navigation;
using MediaBrowser.Theater.Interfaces.Presentation;
using MediaBrowser.Theater.Interfaces.Theming;
using MediaBrowser.Theater.Interfaces.ViewModels;
using MediaBrowser.Theater.Presentation.Controls;
using MediaBrowser.Theater.Presentation.Pages;
using MediaBrowser.Theater.Presentation.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace MediaBrowser.Plugins.DefaultTheme.Details
{
    /// <summary>
    /// Interaction logic for Gallery.xaml
    /// </summary>
    public partial class Gallery : UserControl
    {
        private readonly IImageManager _imageManager;
        private readonly IThemeManager _themeManager;
        private readonly INavigationService _nav;

        private readonly IApiClient _apiClient;

        public Gallery(IImageManager imageManager, IApiClient apiClient, INavigationService nav, IThemeManager themeManager)
        {
            _imageManager = imageManager;
            _apiClient = apiClient;
            _nav = nav;
            _themeManager = themeManager;
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

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);

            LstItems.ItemInvoked += LstItems_ItemInvoked;
        }

        async void LstItems_ItemInvoked(object sender, ItemEventArgs<object> e)
        {
            var images = GetImages(Item, _apiClient, Convert.ToInt32(SystemParameters.VirtualScreenWidth), null);

            var selectedIndex = LstItems.SelectedIndex;

            images = images.Skip(selectedIndex).Concat(images.Take(selectedIndex)).ToList();

            var initialImages = images.Select(i => new ImageViewerImage { Url = i, Caption = Item.Name });

            var vm = new ImageViewerViewModel(Dispatcher, _imageManager, initialImages);

            await _nav.NavigateToImageViewer(vm);
        }

        /// <summary>
        /// Called when [item changed].
        /// </summary>
        protected async void OnItemChanged()
        {
            var urls = GetImages(Item, _apiClient, null, 640);

            var tasks = urls.Select(GetImage);

            var results = await Task.WhenAll(tasks);

            var images = results.Where(i => i != null).ToList();

            LstItems.ItemsSource = CollectionViewSource.GetDefaultView(images);
        }

        /// <summary>
        /// Gets the image.
        /// </summary>
        /// <param name="url">The URL.</param>
        /// <returns>Task{BitmapImage}.</returns>
        private async Task<BitmapImage> GetImage(string url)
        {
            try
            {
                return await _imageManager.GetRemoteBitmapAsync(url);
            }
            catch (HttpException)
            {
                return null;
            }
        }

        /// <summary>
        /// Gets the images.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <returns>List{System.String}.</returns>
        internal static List<string> GetImages(BaseItemDto item, IApiClient apiClient, int? imageWidth, int? imageHeight)
        {
            var images = new List<string> { };

            if (item.BackdropCount > 0)
            {
                for (var i = 0; i < item.BackdropCount; i++)
                {
                    images.Add(apiClient.GetImageUrl(item, new ImageOptions
                    {
                        ImageType = ImageType.Backdrop,
                        ImageIndex = i,
                        Width = imageWidth,
                        Height = imageHeight
                    }));
                }
            }

            if (item.HasThumb)
            {
                images.Add(apiClient.GetImageUrl(item, new ImageOptions
                {
                    ImageType = ImageType.Thumb,
                    Width = imageWidth,
                    Height = imageHeight
                }));
            }

            if (item.HasArtImage)
            {
                images.Add(apiClient.GetImageUrl(item, new ImageOptions
                {
                    ImageType = ImageType.Art,
                    Width = imageWidth,
                    Height = imageHeight
                }));
            }

            if (item.HasDiscImage)
            {
                images.Add(apiClient.GetImageUrl(item, new ImageOptions
                {
                    ImageType = ImageType.Disc,
                    Width = imageWidth,
                    Height = imageHeight
                }));
            }

            if (item.HasMenuImage)
            {
                images.Add(apiClient.GetImageUrl(item, new ImageOptions
                {
                    ImageType = ImageType.Menu,
                    Width = imageWidth,
                    Height = imageHeight
                }));
            }

            if (item.HasBoxImage)
            {
                images.Add(apiClient.GetImageUrl(item, new ImageOptions
                {
                    ImageType = ImageType.Box,
                    Width = imageWidth,
                    Height = imageHeight
                }));
            }

            if (item.HasBoxImage)
            {
                images.Add(apiClient.GetImageUrl(item, new ImageOptions
                {
                    ImageType = ImageType.Box,
                    Width = imageWidth,
                    Height = imageHeight
                }));
            }

            if (item.HasBoxRearImage)
            {
                images.Add(apiClient.GetImageUrl(item, new ImageOptions
                {
                    ImageType = ImageType.BoxRear,
                    Width = imageWidth,
                    Height = imageHeight
                }));
            }

            return images;
        }
    }
}
