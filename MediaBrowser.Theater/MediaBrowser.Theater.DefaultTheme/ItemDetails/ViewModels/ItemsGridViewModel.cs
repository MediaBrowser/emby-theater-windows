using System;
using System.Linq;
using System.Windows;
using MediaBrowser.Model.ApiClient;
using MediaBrowser.Model.Dto;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.Querying;
using MediaBrowser.Theater.Api.Navigation;
using MediaBrowser.Theater.Api.Playback;
using MediaBrowser.Theater.Api.Session;
using MediaBrowser.Theater.Api.UserInterface;
using MediaBrowser.Theater.DefaultTheme.Core.ViewModels;
using MediaBrowser.Theater.DefaultTheme.Home.ViewModels;
using MediaBrowser.Theater.Playback;
using MediaBrowser.Theater.Presentation.Controls;
using MediaBrowser.Theater.Presentation.ViewModels;

namespace MediaBrowser.Theater.DefaultTheme.ItemDetails.ViewModels
{
    public class ItemsGridViewModel
        : BaseViewModel, IItemDetailSection, IKnownSize
    {
        private readonly ItemsResult _itemsResult;
        private readonly IConnectionManager _connectionManager;
        private readonly IImageManager _imageManager;
        private readonly INavigator _navigator;
        private readonly IPlaybackManager _playbackManager;
        private readonly ISessionManager _sessionManager;
        private readonly ImageType[] _preferredImageTypes;

        private bool _isVisible;

        public int SortOrder { get { return 2; } }

        public string SectionTitle { get; private set; }

        public RangeObservableCollection<ItemTileViewModel> Items { get; private set; }

        public ItemsGridViewModel(ItemsResult itemsResult, IConnectionManager connectionManager, IImageManager imageManager, INavigator navigator, IPlaybackManager playbackManager, ISessionManager sessionManager)
        {
            _itemsResult = itemsResult;
            _connectionManager = connectionManager;
            _imageManager = imageManager;
            _navigator = navigator;
            _playbackManager = playbackManager;
            _sessionManager = sessionManager;

            var itemType = itemsResult.Items.Length > 0 ? itemsResult.Items.First().Type : null;

            if (itemType == "Episode") {
                _preferredImageTypes = new[] { ImageType.Screenshot, ImageType.Thumb, ImageType.Art, ImageType.Primary };
            } else {
                _preferredImageTypes = new[] { ImageType.Primary, ImageType.Backdrop, ImageType.Thumb };
            }

            SectionTitle = Title = ItemsListViewModel.SelectHeader(itemType);
            Items = new RangeObservableCollection<ItemTileViewModel>();

            LoadItems();
        }

        public string Title { get; set; }

        public Size Size
        {
            get
            {
                if (Items.Count == 0) {
                    return new Size(0, 0);
                }

                var width = (int)Math.Ceiling(Items.Count / 2.0);
                var itemSize = Items.First().Size;

                return new Size(width*(itemSize.Width + 2*HomeViewModel.TileMargin) + 20, 2*itemSize.Height + 4*HomeViewModel.TileMargin + 20);
            }
        }

        public bool IsVisible
        {
            get { return _isVisible; }
            private set
            {
                if (Equals(_isVisible, value))
                {
                    return;
                }

                _isVisible = value;
                OnPropertyChanged();
            }
        }

        private void LoadItems()
        {
            BaseItemDto[] items = _itemsResult.Items;

            for (int i = 0; i < items.Length; i++) {
                ItemTileViewModel vm = CreateItem();
                vm.Item = items[i];

                Items.Add(vm);

                Items[i].PropertyChanged += (s, e) => {
                    if (e.PropertyName == "Size")
                        OnPropertyChanged("Size");
                };
            }

            IsVisible = Items.Count > 0;
            OnPropertyChanged("Size");
        }

        private ItemTileViewModel CreateItem()
        {
            return new ItemTileViewModel(_connectionManager, _imageManager, _navigator, _playbackManager, _sessionManager, null)
            {
                DesiredImageHeight = PersonListItemViewModel.Height,
                PreferredImageTypes = _preferredImageTypes
            };
        }
    }
}