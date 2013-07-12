using MediaBrowser.Model.ApiClient;
using MediaBrowser.Model.Dto;
using MediaBrowser.Model.Entities;
using MediaBrowser.Theater.Interfaces.Presentation;
using MediaBrowser.Theater.Presentation.Extensions;
using System;

namespace MediaBrowser.Theater.Presentation.ViewModels
{
    /// <summary>
    /// Class DtoBaseItemViewModel
    /// </summary>
    public class BaseItemDtoViewModel : BaseViewModel
    {
        /// <summary>
        /// Gets the API client.
        /// </summary>
        /// <value>The API client.</value>
        public IApiClient ApiClient { get; private set; }
        public IImageManager ImageManager { get; private set; }

        public bool IsSpecialFeature { get; set; }
        public bool IsLocalTrailer { get; set; }

        /// <summary>
        /// The _average primary image aspect ratio
        /// </summary>
        private double _medianPrimaryImageAspectRatio;
        /// <summary>
        /// Gets the aspect ratio that should be used if displaying the primary image
        /// </summary>
        /// <value>The average primary image aspect ratio.</value>
        public double MedianPrimaryImageAspectRatio
        {
            get { return _medianPrimaryImageAspectRatio; }

            set
            {
                _medianPrimaryImageAspectRatio = value;
                OnPropertyChanged("MedianPrimaryImageAspectRatio");
            }
        }

        /// <summary>
        /// The _image width
        /// </summary>
        private ImageType _imageType;
        /// <summary>
        /// Gets or sets the width of the image.
        /// </summary>
        /// <value>The width of the image.</value>
        public ImageType ImageType
        {
            get { return _imageType; }
            set
            {
                _imageType = value;
                OnPropertyChanged("ImageType");
            }
        }
        

        /// <summary>
        /// Gets or sets the width of the image.
        /// </summary>
        /// <value>The width of the image.</value>
        public double ImageDisplayWidth { get; set; }

        public double ImageDisplayHeight { get; set; }
        
        public string ViewType { get; set; }

        public string PersonRole { get; set; }

        public void NotifyDisplayPreferencesChanged()
        {
            OnPropertyChanged("ViewType");
        }

        /// <summary>
        /// The _item
        /// </summary>
        private BaseItemDto _item;

        public BaseItemDtoViewModel(IApiClient apiClient, IImageManager imageManager)
        {
            ImageManager = imageManager;
            ApiClient = apiClient;
        }

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
        /// Gets an image url that can be used to download an image from the api
        /// </summary>
        /// <param name="imageType">The type of image requested</param>
        /// <param name="imageIndex">The image index, if there are multiple. Currently only applies to backdrops. Supply null or 0 for first backdrop.</param>
        /// <returns>System.String.</returns>
        public string GetImageUrl(ImageType imageType, int? imageIndex = null)
        {
            var width = ImageDisplayWidth;
            var height = ImageDisplayHeight;

            var imageOptions = new ImageOptions
            {
                ImageType = imageType,
                ImageIndex = imageIndex,
                Width = Convert.ToInt32(width)
            };

            var medianAspectRatio = AspectRatioHelper.GetAspectRatio(imageType, MedianPrimaryImageAspectRatio);

            var currentAspectRatio = imageType == ImageType.Primary ? Item.PrimaryImageAspectRatio ?? width / height : width / height;

            // Preserve the exact AR if it deviates from the median significantly
            var preserveExactAspectRatio = Math.Abs(currentAspectRatio - medianAspectRatio) > .25;

            if (!preserveExactAspectRatio)
            {
                imageOptions.Height = Convert.ToInt32(height);
            }

            if (Item.IsType("Person"))
            {
                return ApiClient.GetPersonImageUrl(Item, imageOptions);
            }

            return ApiClient.GetImageUrl(Item, imageOptions);
        }


    }
}
