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

        public IItemDetailSection GetSection(BaseItemDto item)
        {
            return new ItemChildrenViewModel(item, _apiClient, _imageManager, _serverEvents, _navigator, _sessionManager);
        }
    }
}
