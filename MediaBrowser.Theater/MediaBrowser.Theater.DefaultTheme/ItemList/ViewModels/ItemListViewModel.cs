using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using MediaBrowser.Model.ApiClient;
using MediaBrowser.Model.Dto;
using MediaBrowser.Model.Querying;
using MediaBrowser.Theater.Api.Navigation;
using MediaBrowser.Theater.Api.UserInterface;
using MediaBrowser.Theater.DefaultTheme.Core.ViewModels;
using MediaBrowser.Theater.Playback;
using MediaBrowser.Theater.Presentation;
using MediaBrowser.Theater.Presentation.ViewModels;

namespace MediaBrowser.Theater.DefaultTheme.ItemList.ViewModels
{
    public class ItemNameSortMode
        : IItemListSortMode
    {
        private static readonly Regex NumberRegex = new Regex(@"^\d+");

        public string DisplayName { get { return "MediaBrowser.Theater.DefaultTheme:Strings:Sorting_ByTitle".Localize(); } }

        public object GetSortKey(BaseItemDto item)
        {
            var index = GetIndexKey(item);

            string name = item.SortName ?? item.Name ?? string.Empty;
            if (name.Length > 0) {
                var match = NumberRegex.Match(name);
                if (match.Success) {
                    int number = int.Parse(match.Value);
                    name = number.ToString("D5") + name.Substring(match.Index + match.Length);
                }
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

    public class IndexSortMode
        : IItemListSortMode
    {
        private readonly ItemNameSortMode _byName = new ItemNameSortMode();

        public string DisplayName { get { return "MediaBrowser.Theater.DefaultTheme:Strings:Sorting_ByNaturalOrder".Localize(); } }

        public object GetSortKey(BaseItemDto item)
        {
            if (item.IndexNumber != null) {
                return item.IndexNumber;
            }

            return _byName.GetSortKey(item);
        }

        public object GetIndexKey(BaseItemDto item)
        {
            if (item.IndexNumber != null) {
                return (item.IndexNumber/5)*5;
            }

            return _byName.GetIndexKey(item);
        }
    }

    public class ItemYearSortMode
        : IItemListSortMode
    {
        public string DisplayName { get { return "MediaBrowser.Theater.DefaultTheme:Strings:Sorting_ByYear".Localize(); } }

        public object GetSortKey(BaseItemDto item)
        {
            return GetDate(item) ?? DateTime.MaxValue;
        }

        public object GetIndexKey(BaseItemDto item)
        {
            var date = GetDate(item);
            if (date != null) {
                return (date.Value.Year/5)*5;
            }

            return "N/A";
        }

        private DateTime? GetDate(BaseItemDto item)
        {
            if (item.PremiereDate != null && item.IsType("episode")) {
                return item.PremiereDate.Value;
            }

            if (item.ProductionYear != null) {
                return new DateTime(item.ProductionYear.Value, 1, 1);
            }

            return null;
        }
    }

    public class ItemCommunityReviewSortMode
        : IItemListSortMode
    {
        public string DisplayName { get { return "MediaBrowser.Theater.DefaultTheme:Strings:Sorting_ByCommunityReview".Localize(); } }

        public object GetSortKey(BaseItemDto item)
        {
            if (item.CommunityRating != null) {
                return item.CommunityRating.Value;
            }

            return float.NegativeInfinity;
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
        : BaseViewModel, IHasRootPresentationOptions, IHasItemSortModes
    {
        public const double ItemHeight = 500;

        private readonly ItemListParameters _parameters;
        private readonly IConnectionManager _connectionManager;
        private readonly IImageManager _imageManager;
        private readonly Task<ItemsResult> _items;
        private readonly INavigator _navigator;
        private readonly IPlaybackManager _playbackManager;

        private string _itemType;
        private IItemListSortMode _sortMode;
        private ItemTileViewModel _selectedItem;
        private ItemInfoViewModel _selectedItemDetails;
        private IEnumerable<IItemListSortMode> _availableSortModes;
        private SortDirection _sortDirection;

        public ItemListViewModel(ItemListParameters parameters, IConnectionManager connectionManager, IImageManager imageManager, INavigator navigator, IPlaybackManager playbackManager)
        {
            _items = parameters.Items;
            _parameters = parameters;
            _connectionManager = connectionManager;
            _imageManager = imageManager;
            _navigator = navigator;
            _playbackManager = playbackManager;
            _availableSortModes = new IItemListSortMode[] { new IndexSortMode(), new ItemNameSortMode(), new ItemYearSortMode(), new ItemCommunityReviewSortMode() };

            Items = new RangeObservableCollection<ItemTileViewModel>();
            Title = parameters.Title;

            PresentationOptions = new RootPresentationOptions {
                ShowMediaBrowserLogo = false
            };
        }

        public string Title { get; private set; }

        public SortDirection SortDirection
        {
            get { return _sortDirection; }
            set
            {
                if (value == _sortDirection) {
                    return;
                }

                _sortDirection = value;
                SaveSortPreference();

                OnPropertyChanged();
                RefreshSorting();
            }
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
            get { return _sortMode ?? _availableSortModes.First(); }
            set
            {
                if (Equals(_sortMode, value)) {
                    return;
                }

                _sortMode = value;
                SaveSortPreference();

                OnPropertyChanged();
                RefreshSorting();
            }
        }

        private void SaveSortPreference()
        {
            if (!string.IsNullOrEmpty(_itemType) && _sortMode != null) {
                Theme.Instance.Configuration.SaveSortMode(_itemType, _sortMode, _sortDirection);
            }
        }
        
        private void RefreshSorting()
        {
            var sorted = SortItems(Items);

            Items.Clear();
            Items.AddRange(sorted);
        }

        private IEnumerable<ItemTileViewModel> SortItems(IEnumerable<ItemTileViewModel> items)
        {
            return (SortDirection == SortDirection.Ascending) ?
                       items.OrderBy(vm => SortMode.GetSortKey(vm.Item)).ToList() :
                       items.OrderByDescending(vm => SortMode.GetSortKey(vm.Item)).ToList();
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
                    return SortMode.GetIndexKey(viewModel.Item);
                };
            }
        }

        public RootPresentationOptions PresentationOptions { get; private set; }

        public override async Task Initialize()
        {
            await LoadItems(_items);
            await base.Initialize();
        }

        private ItemTileViewModel CreateItemViewModel(BaseItemDto dto)
        {
            return new ItemTileViewModel(_connectionManager, _imageManager, _navigator, _playbackManager, dto);
        }

        private async Task LoadItems(Task<ItemsResult> itemsTask)
        {
            ItemsResult result = await itemsTask;
            IEnumerable<ItemTileViewModel> viewModels = result.Items.Select(dto => {
                var vm = (_parameters.ViewModelSelector ?? CreateItemViewModel)(dto);
                vm.DesiredImageHeight = ItemHeight;

                if (_parameters.ForceShowItemNames) {
                    vm.ShowCaptionBar = true;
                }

                return vm;
            });

            if (result.Items.Length > 0 && _sortMode == null) {
                _itemType = result.Items[0].Type;

                var sortModePreference = Theme.Instance.Configuration.FindSortMode(_itemType);
                if (sortModePreference != null) {
                    _sortDirection = sortModePreference.SortDirection;
                    _sortMode = _availableSortModes.FirstOrDefault(sm => sm.GetType().FullName == sortModePreference.SortModeType);
                }
            }

            Items.AddRange(SortItems(viewModels));
        }
    }
}