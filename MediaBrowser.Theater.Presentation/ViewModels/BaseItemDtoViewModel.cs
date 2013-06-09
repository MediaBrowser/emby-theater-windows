using MediaBrowser.Model.ApiClient;
using MediaBrowser.Model.Dto;
using MediaBrowser.Model.Entities;
using MediaBrowser.Theater.Interfaces.Presentation;
using System;
using System.Collections.Generic;
using System.Linq;

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

        /// <summary>
        /// The _average primary image aspect ratio
        /// </summary>
        private double _averagePrimaryImageAspectRatio;
        /// <summary>
        /// Gets the aspect ratio that should be used if displaying the primary image
        /// </summary>
        /// <value>The average primary image aspect ratio.</value>
        public double AveragePrimaryImageAspectRatio
        {
            get { return _averagePrimaryImageAspectRatio; }

            set
            {
                _averagePrimaryImageAspectRatio = value;
                OnPropertyChanged("AveragePrimaryImageAspectRatio");
            }
        }

        /// <summary>
        /// The _image width
        /// </summary>
        private double _imageWidth;
        /// <summary>
        /// Gets or sets the width of the image.
        /// </summary>
        /// <value>The width of the image.</value>
        public double ImageWidth
        {
            get { return _imageWidth; }
            set
            {
                _imageWidth = value;
                OnPropertyChanged("ImageWidth");
            }
        }

        public double ImageHeight
        {
            get
            {
                if (AveragePrimaryImageAspectRatio.Equals(0))
                {
                    return 1;
                }

                return ImageWidth / AveragePrimaryImageAspectRatio;
            }
        }

        /// <summary>
        /// The _image type
        /// </summary>
        private string _viewType;
        /// <summary>
        /// Gets or sets the type of the image.
        /// </summary>
        /// <value>The type of the image.</value>
        public string ViewType
        {
            get { return _viewType; }
            set
            {
                _viewType = value;
                OnPropertyChanged("ViewType");
            }
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
            var width = ImageWidth;

            var averageAspectRatio = GetAspectRatio(imageType, AveragePrimaryImageAspectRatio);

            var height = width / averageAspectRatio;

            var imageOptions = new ImageOptions
            {
                ImageType = imageType,
                ImageIndex = imageIndex,
                Width = Convert.ToInt32(width)
            };

            var currentAspectRatio = imageType == ImageType.Primary ? Item.PrimaryImageAspectRatio ?? width / height : width / height;

            // Preserve the exact AR if it deviates from the average significantly
            var preserveExactAspectRatio = Math.Abs(currentAspectRatio - averageAspectRatio) > .25;

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

        private double GetAspectRatio(ImageType imageType, double averagePrimaryImageAspectRatio)
        {
            switch (imageType)
            {
                case ImageType.Art:
                    return 1.777777777777778;
                case ImageType.Backdrop:
                    return 1.777777777777778;
                case ImageType.Banner:
                    return 5.414285714285714;
                case ImageType.Disc:
                    return 1;
                case ImageType.Logo:
                    return 1.777777777777778;
                case ImageType.Primary:
                    return averagePrimaryImageAspectRatio;
                case ImageType.Thumb:
                    return 1.777777777777778;
                default:
                    return 1;
            }
        }

        /// <summary>
        /// Gets the average primary image aspect ratio.
        /// </summary>
        /// <param name="items">The items.</param>
        /// <returns>System.Double.</returns>
        /// <exception cref="System.ArgumentNullException">items</exception>
        public static double GetAveragePrimaryImageAspectRatio(IEnumerable<BaseItemDto> items)
        {
            if (items == null)
            {
                throw new ArgumentNullException("items");
            }

            var values = items.Select(i => i.PrimaryImageAspectRatio ?? 0).Where(i => i > 0).ToList();

            if (values.Count > 0)
            {
                return values.Median();
            }

            return 0;
        }
    }

    public static class MedianExtension
    {
        public static double Median(this IEnumerable<double> source)
        {
            var list = source.ToList();

            if (list.Count != 0)
            {
                var midpoint = (list.Count - 1) / 2;
                var sorted = list.OrderBy(n => n);
                var median = sorted.ElementAt(midpoint);

                if (list.Count % 2 == 0)
                {
                    median = (median + sorted.ElementAt(midpoint + 1)) / 2;
                }

                return median;
            }

            throw new InvalidOperationException("Sequence contains no elements");
        }
    }
}
