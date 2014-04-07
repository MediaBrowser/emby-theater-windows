using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using MediaBrowser.Model.ApiClient;
using MediaBrowser.Model.Dto;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.Querying;
using MediaBrowser.Theater.Api.Navigation;
using MediaBrowser.Theater.Api.Session;
using MediaBrowser.Theater.Api.UserInterface;
using MediaBrowser.Theater.DefaultTheme.Core.ViewModels;
using MediaBrowser.Theater.DefaultTheme.Home.ViewModels;
using MediaBrowser.Theater.Presentation.Controls;
using MediaBrowser.Theater.Presentation.ViewModels;

namespace MediaBrowser.Theater.DefaultTheme.ItemDetails.ViewModels
{
    public class ItemChildrenViewModel
        : BaseViewModel, IItemDetailSection, IKnownSize
    {
        private const double PosterHeight = 350 - HomeViewModel.TileMargin * 0.5;

        private readonly BaseItemDto _item;
        private readonly IApiClient _apiClient;
        private readonly IImageManager _imageManager;
        private readonly IServerEvents _serverEvents;
        private readonly INavigator _navigator;
        private readonly ISessionManager _sessionManager;

        private bool _isVisible;
        private ImageType[] _preferredImageTypes;

        public int SortOrder { get { return 2; } }

        public RangeObservableCollection<ItemTileViewModel> Items { get; private set; }

        public ItemChildrenViewModel(BaseItemDto item, IApiClient apiClient, IImageManager imageManager, IServerEvents serverEvents, INavigator navigator, ISessionManager sessionManager)
        {
            _item = item;
            _apiClient = apiClient;
            _imageManager = imageManager;
            _serverEvents = serverEvents;
            _navigator = navigator;
            _sessionManager = sessionManager;

            if (item.Type == "Season")
                _preferredImageTypes = new[] { ImageType.Screenshot, ImageType.Thumb, ImageType.Art, ImageType.Primary };
            else
                _preferredImageTypes = new[] { ImageType.Primary, ImageType.Backdrop, ImageType.Thumb };

            Items = new RangeObservableCollection<ItemTileViewModel>();
            for (int i = 0; i < 4; i++)
            {
                Items.Add(CreateItem());
            }

            IsVisible = true;
            LoadItems();
        }

        public string Title
        {
            get
            {
                switch (_item.Type) {
                    case "Series":
                        return "Seasons";
                    case "Season":
                        return "Episodes";
                    case "Artist":
                        return "Albums";
                    case "Album":
                        return "Tracks";
                    default:
                        return "Items";
                }
            }
        }

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

        private async void LoadItems()
        {
            var query = new ItemQuery { ParentId = _item.Id, UserId = _sessionManager.CurrentUser.Id, Limit = 8 };
            LoadItems(await _apiClient.GetItemsAsync(query));
        }

        private async void LoadItems(ItemsResult result)
        {
            BaseItemDto[] items = result.Items;

            for (int i = 0; i < items.Length; i++)
            {
                if (Items.Count > i)
                {
                    Items[i].Item = items[i];
                }
                else
                {
                    ItemTileViewModel vm = CreateItem();
                    vm.Item = items[i];

                    Items.Add(vm);
                }

                Items[i].PropertyChanged += (s, e) => {
                    if (e.PropertyName == "Size")
                        OnPropertyChanged("Size");
                };
            }

            if (Items.Count > items.Length)
            {
                List<ItemTileViewModel> toRemove = Items.Skip(items.Length).ToList();
                Items.RemoveRange(toRemove);
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

        public static Task<ItemsResult> Query(BaseItemDto item, IApiClient apiClient, ISessionManager sessionManager)
        {
            var query = new ItemQuery { ParentId = item.Id, UserId = sessionManager.CurrentUser.Id };
            return apiClient.GetItemsAsync(query);
        }
    }

    public class ItemChildrenListViewModel 
        : BaseViewModel, IItemDetailSection, IKnownSize
    {
        private readonly BaseItemDto _item;
        private readonly IApiClient _apiClient;
        private readonly ISessionManager _sessionManager;
        private readonly IImageManager _imageManager;
        private readonly INavigator _navigator;

        public int SortOrder 
        {
            get { return 2; }
        }

        public Size Size
        {
            get { return new Size(800, 700); }
        }

        public string Title
        {
            get
            {
                switch (_item.Type) {
                    case "Series":
                        return "Seasons";
                    case "Season":
                        return "Episodes";
                    case "Artist":
                        return "Albums";
                    case "Album":
                        return "Tracks";
                    default:
                        return "Items";
                }
            }
        }

        public RangeObservableCollection<IViewModel> Items { get; private set; }

        public ItemChildrenListViewModel(BaseItemDto item, IApiClient apiClient, ISessionManager sessionManager, IImageManager imageManager, INavigator navigator)
        {
            _item = item;
            _apiClient = apiClient;
            _sessionManager = sessionManager;
            _imageManager = imageManager;
            _navigator = navigator;

            Items = new RangeObservableCollection<IViewModel>();
            LoadItems();
        }

        private async void LoadItems()
        {
            var result = await ItemChildrenViewModel.Query(_item, _apiClient, _sessionManager);
            IEnumerable<IViewModel> items = result.Items.Select(i => new ListItemViewModel(i, _apiClient, _imageManager, _navigator));

            Items.Clear();
            Items.AddRange(items);
        }
    }

    public class ListItemViewModel
        : BaseViewModel
    {
        private readonly BaseItemDto _item;

        public ItemArtworkViewModel Artwork { get; private set; }

        public ICommand NavigateCommand { get; private set; }

        public string Name {
            get { return _item.Name; }
        }

        public string Detail
        {
            get
            {
                if (_item.Type == "Episode") {
                    if (_item.IndexNumber == null) {
                        return null;
                    }

                    if (_item.IndexNumberEnd != null) {
                        return string.Format("S{0}, E{1}-{2}", _item.ParentIndexNumber, _item.IndexNumber, _item.IndexNumberEnd);
                    }

                    return string.Format("S{0}, E{1}", _item.ParentIndexNumber, _item.IndexNumber);
                }

                if (_item.ProductionYear != null) {
                    return _item.ProductionYear.Value.ToString(CultureInfo.InvariantCulture);
                }

                return null;
            }
        }

        public float Rating
        {
            get { return _item.CommunityRating ?? 0; }
        }

        public bool HasRating
        {
            get { return _item.CommunityRating != null; }
        }

        public ListItemViewModel(BaseItemDto item, IApiClient apiClient, IImageManager imageManager, INavigator navigator)
        {
            _item = item;

            Artwork = new ItemArtworkViewModel(item, apiClient, imageManager) { 
                DesiredImageHeight = 100,
                DesiredImageWidth = 178
            };

            NavigateCommand = new RelayCommand(arg => navigator.Navigate(Go.To.Item(item)));
        }
    }

    public class ItemChildrenSectionGenerator
        : IItemDetailSectionGenerator
    {
        private readonly IApiClient _apiClient;
        private readonly IImageManager _imageManager;
        private readonly IServerEvents _serverEvents;
        private readonly INavigator _navigator;
        private readonly ISessionManager _sessionManager;

        public ItemChildrenSectionGenerator(IApiClient apiClient, IImageManager imageManager, IServerEvents serverEvents, INavigator navigator, ISessionManager sessionManager)
        {
            _apiClient = apiClient;
            _imageManager = imageManager;
            _serverEvents = serverEvents;
            _navigator = navigator;
            _sessionManager = sessionManager;
        }

        public bool HasSection(BaseItemDto item)
        {
            return item != null && item.IsFolder;
        }

        public async Task<IEnumerable<IItemDetailSection>> GetSections(BaseItemDto item)
        {
            var result = await ItemChildrenViewModel.Query(item, _apiClient, _sessionManager);
            if (result.Items.Length == 1) {
                return await GetSections(result.Items[0]);
            }

            if (item.Type == "Season" || result.Items.Length > 8) {
                return new[] { new ItemChildrenListViewModel(item, _apiClient, _sessionManager, _imageManager, _navigator) };
            }

            return new[] { new ItemChildrenViewModel(item, _apiClient, _imageManager, _serverEvents, _navigator, _sessionManager) };
        }
    }
}

