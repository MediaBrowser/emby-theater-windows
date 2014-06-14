using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using MediaBrowser.Model.ApiClient;
using MediaBrowser.Model.Dto;
using MediaBrowser.Model.Querying;
using MediaBrowser.Theater.Api.Navigation;
using MediaBrowser.Theater.Api.Playback;
using MediaBrowser.Theater.Api.Session;
using MediaBrowser.Theater.Api.UserInterface;
using MediaBrowser.Theater.DefaultTheme.Core.ViewModels;
using MediaBrowser.Theater.Presentation.ViewModels;

namespace MediaBrowser.Theater.DefaultTheme.ItemList.ViewModels
{
    public interface IItemListSortMode
    {
        string DisplayName { get; }
        object GetSortKey(BaseItemDto item);
        object GetIndexKey(BaseItemDto item);
    }

    public class ItemNameSortMode
        : IItemListSortMode
    {
        public string DisplayName { get { return "Name_LOCALIZE_THIS"; } }

        public object GetSortKey(BaseItemDto item)
        {
            var index = GetIndexKey(item);

            string name = item.SortName ?? item.Name ?? string.Empty;
            if (name.Length > 0) {
                return index + name.ToUpper(CultureInfo.CurrentUICulture);
            }

            return index + "#";
        }

        public object GetIndexKey(BaseItemDto item)
        {
            string name = (item.SortName ?? item.Name ?? string.Empty).Trim();
            if (name.Length > 0) {
                var key = name.First().ToString(CultureInfo.CurrentUICulture).ToUpper(CultureInfo.CurrentUICulture);

                if (char.IsLetter(key, 0)) {
                    return key;
                }
                
                if (char.IsNumber(key, 0)) {
                    return "#";
                }
            }

            return "_";
        }
    }

    public class ItemYearSortMode
        : IItemListSortMode
    {
        public string DisplayName { get { return "Year_LOCALIZE_THIS"; } }

        public object GetSortKey(BaseItemDto item)
        {
            if (item.PremiereDate != null) {
                return item.PremiereDate.Value.Year;
            }

            return int.MaxValue;
        }

        public object GetIndexKey(BaseItemDto item)
        {
            if (item.PremiereDate != null) {
                return (item.PremiereDate.Value.Year / 5) * 5;
            }

            return "N/A";
        }
    }

    public class ItemCommunityReviewSortMode
        : IItemListSortMode
    {
        public string DisplayName { get { return "CommunityReview_LOCALIZE_THIS"; } }

        public object GetSortKey(BaseItemDto item)
        {
            if (item.CommunityRating != null) {
                return item.CommunityRating.Value;
            }

            return float.PositiveInfinity;
        }

        public object GetIndexKey(BaseItemDto item)
        {
            if (item.CommunityRating != null)
            {
                return Math.Round(item.CommunityRating.Value);
            }

            return "N/A";
        }
    }

    public class ItemListViewModel
        : BaseViewModel, IHasRootPresentationOptions
    {
        public const double ItemHeight = 500;

        private readonly IApiClient _apiClient;
        private readonly IImageManager _imageManager;
        private readonly Task<ItemsResult> _items;
        private readonly INavigator _navigator;
        private readonly IPlaybackManager _playbackManager;
        private readonly IServerEvents _serverEvents;
        private readonly ISessionManager _sessionManager;
        
        private IItemListSortMode _sortMode;

        private ItemTileViewModel _selectedItem;
        private ItemInfoViewModel _selectedItemDetails;
        private IEnumerable<IItemListSortMode> _availableSortModes;

        public ItemListViewModel(Task<ItemsResult> items, string title, IApiClient apiClient, IImageManager imageManager, IServerEvents serverEvents, INavigator navigator, ISessionManager sessionManager, IPlaybackManager playbackManager)
        {
            _items = items;
            _apiClient = apiClient;
            _imageManager = imageManager;
            _serverEvents = serverEvents;
            _navigator = navigator;
            _sessionManager = sessionManager;
            _playbackManager = playbackManager;
            _sortMode = new ItemNameSortMode();
            _availableSortModes = new[] { _sortMode, new ItemYearSortMode(), new ItemCommunityReviewSortMode() };
            Items = new RangeObservableCollection<ItemTileViewModel>();

            PresentationOptions = new RootPresentationOptions {
                ShowMediaBrowserLogo = false,
                Title = title
            };
        }

        public IEnumerable<IItemListSortMode> AvailableSortModes
        {
            get { return _availableSortModes; }
            set
            {
                _availableSortModes = value;
                OnPropertyChanged();
            }
        }

        public IItemListSortMode SortMode
        {
            get { return _sortMode; }
            set
            {
                if (Equals(_sortMode, value)) {
                    return;
                }

                _sortMode = value;

                OnPropertyChanged();

                var sorted = Items.OrderBy(vm => _sortMode.GetSortKey(vm.Item));

                Items.Clear();
                Items.AddRange(sorted);

                OnPropertyChanged("IndexSelector");
            }
        }

        public RangeObservableCollection<ItemTileViewModel> Items { get; private set; }

        public ItemTileViewModel SelectedItem
        {
            get { return _selectedItem; }
            set
            {
                if (Equals(value, _selectedItem)) {
                    return;
                }
                _selectedItem = value;
                OnPropertyChanged();

                if (_selectedItem != null) {
                    SelectedItemDetails = new ItemInfoViewModel(_selectedItem.Item);
                } else {
                    SelectedItemDetails = null;
                }
            }
        }

        public bool HasSelectedItemDetails
        {
            get { return SelectedItemDetails != null; }
        }

        public ItemInfoViewModel SelectedItemDetails
        {
            get { return _selectedItemDetails; }
            private set
            {
                if (Equals(value, _selectedItemDetails)) {
                    return;
                }
                _selectedItemDetails = value;
                OnPropertyChanged();
                OnPropertyChanged("HasSelectedItemDetails");
            }
        }

        public Func<object, object> IndexSelector
        {
            get
            {
                return item => {
                    var viewModel = (ItemTileViewModel) item;
                    return _sortMode.GetIndexKey(viewModel.Item);
                };
            }
        }

        public RootPresentationOptions PresentationOptions { get; private set; }

        public override async Task Initialize()
        {
            await LoadItems(_items);
            await base.Initialize();
        }

        private async Task LoadItems(Task<ItemsResult> itemsTask)
        {
            ItemsResult result = await itemsTask;
            IEnumerable<ItemTileViewModel> viewModels = result.Items.Select(dto => new ItemTileViewModel(_apiClient, _imageManager, _serverEvents, _navigator, _playbackManager, dto) {
                DesiredImageHeight = ItemHeight
            });

            Items.AddRange(viewModels.OrderBy(vm => _sortMode.GetSortKey(vm.Item)));
        }
    }
}