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

        private string _name;
        public string Name
        {
            get { return _name; }

            set
            {
                var changed = !string.Equals(_name, value);
                _name = value;

                if (changed)
                {
                    OnPropertyChanged("Name");
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

        private double? _galleryWidth;
        public double? GalleryWidth
        {
            get { return _galleryWidth; }

            set
            {
                var changed = _galleryWidth != value;
                _galleryWidth = value;

                if (changed)
                {
                    OnPropertyChanged("GalleryWidth");
                }
            }
        }

        private double? _galleryHeight;
        public double? GalleryHeight
        {
            get { return _galleryHeight; }

            set
            {
                var changed = _galleryHeight != value;
                _galleryHeight = value;

                if (changed)
                {
                    OnPropertyChanged("GalleryHeight");
                }
            }
        }

        public Action CustomCommandAction { get; set; }
        public Action FocusedCommandAction { get; set; }

        public ICommand CustomCommand { get; private set; }
        public ICommand FocusedCommand { get; private set; }
        
        public RangeObservableCollection<GalleryImageViewModel> ListItems { get; private set; }

        public GalleryViewModel(IApiClient apiClient, IImageManager imageManager, INavigationService navigation)
        {
            ListItems = new RangeObservableCollection<GalleryImageViewModel>();

            ListCollectionView = new ListCollectionView(ListItems);

            _apiClient = apiClient;
            _imageManager = imageManager;
            _navigation = navigation;

            OpenImageViewerCommand = new RelayCommand(OpenImageViewer);

            CustomCommand = new RelayCommand(o => CustomCommandAction());
            FocusedCommand = new RelayCommand(o => FocusedCommandAction());
        }

        public ListCollectionView ListCollectionView { get; private set; }

        public void AddImages(IEnumerable<string> urls)
        {
            ListItems.AddRange(urls.Select(i => new GalleryImageViewModel(_imageManager) { ImageUrl = i }));
        }

        /// <summary>
        /// Gets the images.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <param name="apiClient">The API client.</param>
        /// <param name="imageWidth">Width of the image.</param>
        /// <param name="imageHeight">Height of the image.</param>
        /// <param name="includeNonBackdrops">if set to <c>true</c> [include non backdrops].</param>
        /// <returns>List{System.String}.</returns>
        public static List<string> GetImages(BaseItemDto item, IApiClient apiClient, int? imageWidth, int? imageHeight, bool includeNonBackdrops)
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

            if (includeNonBackdrops)
            {
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
            }

            return images;
        }

        private async void OpenImageViewer(object commandParameter)
        {
            var image = (GalleryImageViewModel)commandParameter;

            var isBackdrop = image.ImageUrl.IndexOf("backdrop", StringComparison.OrdinalIgnoreCase) != -1;

            var images = GetImages(Item, _apiClient, Convert.ToInt32(SystemParameters.VirtualScreenWidth), null, !isBackdrop);

            var selectedIndex = ListCollectionView.IndexOf(image);

            images = images.Skip(selectedIndex).Concat(images.Take(selectedIndex)).ToList();

            var initialImages = images.Select(i => new ImageViewerImage { Url = i, Caption = Item.Name });

            var vm = new ImageViewerViewModel(_imageManager, initialImages);

            await _navigation.NavigateToImageViewer(vm);
        }

        public void Dispose()
        {
            foreach (var item in ListItems.ToList())
            {
                item.Dispose();
            }
        }
    }
}
