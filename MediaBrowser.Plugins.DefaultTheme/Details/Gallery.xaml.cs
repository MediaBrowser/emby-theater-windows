using MediaBrowser.Model.ApiClient;
using MediaBrowser.Model.Dto;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.Net;
using MediaBrowser.Theater.Interfaces.Presentation;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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

        private readonly IApiClient _apiClient;

        public Gallery(IImageManager imageManager, IApiClient apiClient)
        {
            _imageManager = imageManager;
            _apiClient = apiClient;
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
        /// Called when [item changed].
        /// </summary>
        protected async void OnItemChanged()
        {
            var urls = GetImages(Item, _apiClient);

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
        internal static List<string> GetImages(BaseItemDto item, IApiClient apiClient)
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
                        MaxHeight = 150
                    }));
                }
            }

            if (item.HasThumb)
            {
                images.Add(apiClient.GetImageUrl(item, new ImageOptions
                {
                    ImageType = ImageType.Thumb,
                    MaxHeight = 150
                }));
            }

            if (item.HasArtImage)
            {
                images.Add(apiClient.GetImageUrl(item, new ImageOptions
                {
                    ImageType = ImageType.Art,
                    MaxHeight = 150
                }));
            }

            if (item.HasDiscImage)
            {
                images.Add(apiClient.GetImageUrl(item, new ImageOptions
                {
                    ImageType = ImageType.Disc,
                    MaxHeight = 150
                }));
            }

            if (item.HasMenuImage)
            {
                images.Add(apiClient.GetImageUrl(item, new ImageOptions
                {
                    ImageType = ImageType.Menu,
                    MaxHeight = 150
                }));
            }

            if (item.HasBoxImage)
            {
                images.Add(apiClient.GetImageUrl(item, new ImageOptions
                {
                    ImageType = ImageType.Box,
                    MaxHeight = 150
                }));
            }

            if (item.HasBoxImage)
            {
                images.Add(apiClient.GetImageUrl(item, new ImageOptions
                {
                    ImageType = ImageType.Box,
                    MaxHeight = 150
                }));
            }

            if (item.HasBoxRearImage)
            {
                images.Add(apiClient.GetImageUrl(item, new ImageOptions
                {
                    ImageType = ImageType.BoxRear,
                    MaxHeight = 150
                }));
            }

            return images;
        }
    }
}
