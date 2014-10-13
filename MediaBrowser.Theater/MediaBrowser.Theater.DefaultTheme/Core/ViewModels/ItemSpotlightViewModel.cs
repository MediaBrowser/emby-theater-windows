using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using System.Windows.Media;
using MediaBrowser.Model.ApiClient;
using MediaBrowser.Model.Dto;
using MediaBrowser.Model.Entities;
using MediaBrowser.Theater.Api.UserInterface;
using MediaBrowser.Theater.DefaultTheme.Home.ViewModels;
using MediaBrowser.Theater.Presentation.ViewModels;

namespace MediaBrowser.Theater.DefaultTheme.Core.ViewModels
{
    public class ItemSpotlightViewModel
        : BaseViewModel
    {
        private readonly IConnectionManager _connectionManager;
        private string _currentCaption;
        private ImageType _imageType;
        private IEnumerable<BaseItemDto> _items;
        private Dictionary<string, BaseItemDto> _urlsToItems;

        public ItemSpotlightViewModel(IImageManager imageManager, IConnectionManager connectionManager)
        {
            _connectionManager = connectionManager;
            _urlsToItems = new Dictionary<string, BaseItemDto>();
            Images = new ImageSlideshowViewModel(imageManager, Enumerable.Empty<string>()) {
                ImageStretch = Stretch.UniformToFill
            };

            Images.PropertyChanged += (s, e) => {
                if (e.PropertyName == "CurrentImageUrl") {
                    BaseItemDto item = CurrentItem;
                    CurrentCaption = item != null ? item.Name : null;
                    OnPropertyChanged("CurrentItem");
                }
            };

            ItemSelectedCommand = new RelayCommand(o => {
                BaseItemDto item = CurrentItem;
                Action<BaseItemDto> action = ItemSelectedAction;

                if (action != null && item != null) {
                    action(item);
                }
            });
        }

        public ImageSlideshowViewModel Images { get; private set; }

        public string CurrentCaption
        {
            get { return _currentCaption; }
            private set
            {
                if (Equals(_currentCaption, value)) {
                    return;
                }

                _currentCaption = value;
                OnPropertyChanged();
                OnPropertyChanged("HasCaption");
            }
        }

        public bool HasCaption
        {
            get { return !string.IsNullOrEmpty(CurrentCaption); }
        }

        public Action<BaseItemDto> ItemSelectedAction { get; set; }
        public ICommand ItemSelectedCommand { get; private set; }

        public IEnumerable<BaseItemDto> Items
        {
            get { return _items; }
            set
            {
                if (Equals(_items, value)) {
                    return;
                }

                _items = value;
                OnPropertyChanged();
                LoadItems();
            }
        }

        public ImageType ImageType
        {
            get { return _imageType; }
            set
            {
                if (Equals(_imageType, value)) {
                    return;
                }

                _imageType = value;
                OnPropertyChanged();
                LoadItems();
            }
        }

        public BaseItemDto CurrentItem
        {
            get
            {
                BaseItemDto item;
                if (_urlsToItems.TryGetValue(Images.CurrentImageUrl, out item)) {
                    return item;
                }

                return null;
            }
        }
        
        private void LoadItems()
        {
            if (Items == null) {
                return;
            }

            const double tileWidth = HomeViewModel.TileWidth*2 + HomeViewModel.TileMargin;
            const double tileHeight = tileWidth*9/16;

            var imageOptions = new ImageOptions {
                Width = Convert.ToInt32(tileWidth),
                Height = Convert.ToInt32(tileHeight),
                ImageType = ImageType
            };
            
            var apiClient = _connectionManager.GetApiClient(Items.First());
            _urlsToItems = Items.ToDictionary(i => apiClient.GetImageUrl(i, imageOptions));

            Images.Images.Clear();
            Images.Images.AddRange(_urlsToItems.Keys);
            Images.StartRotating(10000);
        }
    }
}