using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using MediaBrowser.Model.ApiClient;
using MediaBrowser.Model.Dto;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.Querying;
using MediaBrowser.Theater.Api.Navigation;
using MediaBrowser.Theater.Api.Session;
using MediaBrowser.Theater.Api.UserInterface;
using MediaBrowser.Theater.DefaultTheme.Core.ViewModels;
using MediaBrowser.Theater.DefaultTheme.Home.ViewModels;
using MediaBrowser.Theater.Presentation;
using MediaBrowser.Theater.Presentation.Controls;
using MediaBrowser.Theater.Presentation.ViewModels;

namespace MediaBrowser.Theater.DefaultTheme.ItemDetails.ViewModels
{
    public class ItemsListViewModel
        : BaseViewModel, IItemDetailSection, IKnownSize
    {
        private readonly ItemsResult _itemsResult;
        private readonly IApiClient _apiClient;
        private readonly IImageManager _imageManager;
        private readonly INavigator _navigator;

        private bool _isVisible;

        public int SortOrder 
        {
            get { return 2; }
        }

        public Size Size
        {
            get
            {
                if (Items.Count == 0) {
                    return new Size();
                }

                return new Size(800, 700);
            }
        }

        public string Title { get; set; }

        public RangeObservableCollection<IViewModel> Items { get; private set; }

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

        public ItemsListViewModel(ItemsResult itemsResult, IApiClient apiClient, IImageManager imageManager, INavigator navigator)
        {
            _itemsResult = itemsResult;
            _apiClient = apiClient;
            _imageManager = imageManager;
            _navigator = navigator;

            Title = SelectHeader(itemsResult.Items.Length > 0 ? itemsResult.Items.First().Type : null);
            Items = new RangeObservableCollection<IViewModel>();
            LoadItems();
        }

        private void LoadItems()
        {
            IEnumerable<IViewModel> items = _itemsResult.Items.Select(i => new ListItemViewModel(i, _apiClient, _imageManager, _navigator));

            Items.Clear();
            Items.AddRange(items);

            IsVisible = Items.Count > 0;
            OnPropertyChanged("Size");
        }

        internal static string SelectHeader(string itemType)
        {
            switch (itemType)
            {
                case "Series":
                    return "MediaBrowser.Theater.DefaultTheme:Strings:DetailSection_SeriesHeader".Localize();
                case "Season":
                    return "MediaBrowser.Theater.DefaultTheme:Strings:DetailSection_SeasonsHeader".Localize();
                case "Episode":
                    return "MediaBrowser.Theater.DefaultTheme:Strings:DetailSection_EpisodesHeader".Localize();
                case "Artist":
                    return "MediaBrowser.Theater.DefaultTheme:Strings:DetailSection_AlbumsHeader".Localize();
                case "Album":
                    return "MediaBrowser.Theater.DefaultTheme:Strings:DetailSection_TracksHeader".Localize();
                case "Movie":
                    return "MediaBrowser.Theater.DefaultTheme:Strings:DetailSection_MoviesHeader".Localize();
                default:
                    return "MediaBrowser.Theater.DefaultTheme:Strings:DetailSection_ItemsHeader".Localize();
            }
        }
    }

    public class ItemsGridViewModel
        : BaseViewModel, IItemDetailSection, IKnownSize
    {
        private const double PosterHeight = 350 - HomeViewModel.TileMargin * 0.5;

        private readonly ItemsResult _itemsResult;
        private readonly IApiClient _apiClient;
        private readonly IImageManager _imageManager;
        private readonly IServerEvents _serverEvents;
        private readonly INavigator _navigator;
        private readonly ImageType[] _preferredImageTypes;

        private bool _isVisible;

        public int SortOrder { get { return 2; } }

        public RangeObservableCollection<ItemTileViewModel> Items { get; private set; }

        public ItemsGridViewModel(ItemsResult itemsResult, IApiClient apiClient, IImageManager imageManager, IServerEvents serverEvents, INavigator navigator)
        {
            _itemsResult = itemsResult;
            _apiClient = apiClient;
            _imageManager = imageManager;
            _serverEvents = serverEvents;
            _navigator = navigator;

            var itemType = itemsResult.Items.Length > 0 ? itemsResult.Items.First().Type : null;

            if (itemType == "Episode") {
                _preferredImageTypes = new[] { ImageType.Screenshot, ImageType.Thumb, ImageType.Art, ImageType.Primary };
            } else {
                _preferredImageTypes = new[] { ImageType.Primary, ImageType.Backdrop, ImageType.Thumb };
            }

            Title = ItemsListViewModel.SelectHeader(itemType);
            Items = new RangeObservableCollection<ItemTileViewModel>();

            LoadItems();
        }

        public string Title { get; set; }

        public Size Size
        {
            get
            {
                if (Items.Count == 0)
                {
                    return new Size();
                }

                var width = (int)Math.Ceiling(Items.Count / 2.0);

                return new Size(width * (Items.First().Size.Width + 2 * HomeViewModel.TileMargin) + 20, 900);
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
            return new ItemTileViewModel(_apiClient, _imageManager, _serverEvents, _navigator, /*_playbackManager,*/ null)
            {
                DesiredImageHeight = PosterHeight,
                PreferredImageTypes = _preferredImageTypes
            };
        }
    }

    public abstract class BaseItemsListSectionGenerator
        : IItemDetailSectionGenerator
    {
        private readonly IApiClient _apiClient;
        private readonly ISessionManager _sessionManager;
        private readonly IImageManager _imageManager;
        private readonly INavigator _navigator;
        private readonly IServerEvents _serverEvents;

        protected BaseItemsListSectionGenerator(IApiClient apiClient, ISessionManager sessionManager, IImageManager imageManager, INavigator navigator, IServerEvents serverEvents)
        {
            _apiClient = apiClient;
            _sessionManager = sessionManager;
            _imageManager = imageManager;
            _navigator = navigator;
            _serverEvents = serverEvents;

            ListThreshold = 8;
        }

        public int ListThreshold { get; set; }

        public virtual bool HasSection(BaseItemDto item)
        {
            return true;
        }

        public virtual Task<IEnumerable<IItemDetailSection>> GetSections(BaseItemDto item)
        {
            return Task.FromResult(Enumerable.Empty<IItemDetailSection>());
        }

        protected async Task<IItemDetailSection> GetItemsSection(ItemsResult itemsResult, bool expandSingleItem = true)
        {
            return await GetItemsSection(itemsResult, result => (result.Items.Length > 0 && result.Items[0].Type == "Episode") || result.Items.Length > ListThreshold, expandSingleItem);
        }

        protected async Task<IItemDetailSection> GetItemsSection(ItemsResult itemsResult, Func<ItemsResult, bool> listCondition, bool expandSingleItem = true)
        {
            if (itemsResult.Items.Length == 1 && expandSingleItem && itemsResult.Items[0].IsFolder) {
                var query = new ItemQuery { ParentId = itemsResult.Items[0].Id, UserId = _sessionManager.CurrentUser.Id };
                return await GetItemsSection(await _apiClient.GetItemsAsync(query), listCondition);
            }

            if (listCondition(itemsResult)) {
                return new ItemsListViewModel(itemsResult, _apiClient, _imageManager, _navigator);
            }

            return new ItemsGridViewModel(itemsResult, _apiClient, _imageManager, _serverEvents, _navigator);
        }
    }
}
