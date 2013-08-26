using MediaBrowser.Model.ApiClient;
using MediaBrowser.Model.Dto;
using MediaBrowser.Model.Entities;
using MediaBrowser.Theater.Interfaces.Navigation;
using MediaBrowser.Theater.Interfaces.Presentation;
using MediaBrowser.Theater.Interfaces.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;

namespace MediaBrowser.Theater.Presentation.ViewModels
{
    public class GalleryViewModel : BaseViewModel, IDisposable
    {
        private readonly IApiClient _apiClient;
        private readonly IImageManager _imageManager;
        private readonly INavigationService _navigation;

        public ICommand OpenImageViewerCommand { get; private set; }

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
                var changed = _item != value;
                _item = value;

                if (changed)
                {
                    OnPropertyChanged("Item");
                }
            }
        }

        private int? _imageWidth;
        public int? ImageWidth
        {
            get { return _imageWidth; }

            set
            {
                var changed = _imageWidth != value;
                _imageWidth = value;

                if (changed)
                {
                    OnPropertyChanged("ImageWidth");
                }
            }
        }

        private int? _imageHeight;
        public int? ImageHeight
        {
            get { return _imageHeight; }

            set
            {
                var changed = _imageHeight != value;
                _imageHeight = value;

                if (changed)
                {
                    OnPropertyChanged("ImageHeight");
                }
            }
        }

        private readonly RangeObservableCollection<GalleryImageViewModel> _listItems = new RangeObservableCollection<GalleryImageViewModel>();

        private ListCollectionView _listCollectionView;

        public GalleryViewModel(IApiClient apiClient, IImageManager imageManager, INavigationService navigation)
        {
            _apiClient = apiClient;
            _imageManager = imageManager;
            _navigation = navigation;

            OpenImageViewerCommand = new RelayCommand(OpenImageViewer);
        }

        public ListCollectionView ListCollectionView
        {
            get
            {
                if (_listCollectionView == null)
                {
                    _listCollectionView = new ListCollectionView(_listItems);
                    ReloadList();
                }

                return _listCollectionView;
            }

            set
            {
                var changed = _listCollectionView != value;
                _listCollectionView = value;

                if (changed)
                {
                    OnPropertyChanged("ListCollectionView");
                }
            }
        }

        private async void ReloadList()
        {
            var urls = GetImages(Item, _apiClient, ImageWidth, ImageHeight);

            _listItems.AddRange(urls.Select(i => new GalleryImageViewModel(_imageManager) { ImageUrl = i }));
        }

        /// <summary>
        /// Gets the images.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <param name="apiClient">The API client.</param>
        /// <param name="imageWidth">Width of the image.</param>
        /// <param name="imageHeight">Height of the image.</param>
        /// <returns>List{System.String}.</returns>
        public static List<string> GetImages(BaseItemDto item, IApiClient apiClient, int? imageWidth, int? imageHeight)
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

        private async void OpenImageViewer(object commandParameter)
        {
            var image = (GalleryImageViewModel)commandParameter;

            var images = GetImages(Item, _apiClient, Convert.ToInt32(SystemParameters.VirtualScreenWidth), null);

            var selectedIndex = ListCollectionView.IndexOf(image);

            images = images.Skip(selectedIndex).Concat(images.Take(selectedIndex)).ToList();

            var initialImages = images.Select(i => new ImageViewerImage { Url = i, Caption = Item.Name });

            var vm = new ImageViewerViewModel(_imageManager, initialImages);

            await _navigation.NavigateToImageViewer(vm);
        }

        public void Dispose()
        {
            foreach (var item in _listItems.ToList())
            {
                item.Dispose();
            }
        }
    }
}
