using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using MediaBrowser.Model.ApiClient;
using MediaBrowser.Model.Querying;
using MediaBrowser.Theater.Api.Navigation;
using MediaBrowser.Theater.Api.Playback;
using MediaBrowser.Theater.Api.Session;
using MediaBrowser.Theater.Api.UserInterface;
using MediaBrowser.Theater.DefaultTheme.Core.ViewModels;
using MediaBrowser.Theater.Presentation.ViewModels;

namespace MediaBrowser.Theater.DefaultTheme.ItemList.ViewModels
{
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
        private ItemTileViewModel _selectedItem;
        private ItemInfoViewModel _selectedItemDetails;

        public ItemListViewModel(Task<ItemsResult> items, string title, IApiClient apiClient, IImageManager imageManager, IServerEvents serverEvents, INavigator navigator, ISessionManager sessionManager, IPlaybackManager playbackManager)
        {
            _items = items;
            _apiClient = apiClient;
            _imageManager = imageManager;
            _serverEvents = serverEvents;
            _navigator = navigator;
            _sessionManager = sessionManager;
            _playbackManager = playbackManager;
            Items = new RangeObservableCollection<ItemTileViewModel>();

            PresentationOptions = new RootPresentationOptions {
                ShowMediaBrowserLogo = false,
                Title = title
            };
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
                    return GetSortKey(viewModel);
                };
            }
        }

        public RootPresentationOptions PresentationOptions { get; private set; }

        public override async Task Initialize()
        {
            await LoadItems(_items);
            await base.Initialize();
        }

        private static object GetSortKey(ItemTileViewModel viewModel)
        {
            string name = viewModel.Item.SortName ?? viewModel.Item.Name;
            if (name.Length > 0) {
                return name.First().ToString(CultureInfo.CurrentUICulture).ToUpper(CultureInfo.CurrentUICulture);
            }
            return string.Empty;
        }

        private async Task LoadItems(Task<ItemsResult> itemsTask)
        {
            ItemsResult result = await itemsTask;
            IEnumerable<ItemTileViewModel> viewModels = result.Items.Select(dto => new ItemTileViewModel(_apiClient, _imageManager, _serverEvents, _navigator, _playbackManager, dto) {
                DesiredImageHeight = ItemHeight
            });

            Items.AddRange(viewModels.OrderBy(GetSortKey));
        }
    }
}