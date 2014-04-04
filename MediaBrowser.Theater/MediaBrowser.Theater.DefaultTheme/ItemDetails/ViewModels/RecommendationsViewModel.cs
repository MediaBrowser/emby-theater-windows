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
    public class RecommendationsViewModel
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

        public int SortOrder { get { return 2; } }

        public RangeObservableCollection<ItemTileViewModel> Items { get; private set; }

        public RecommendationsViewModel(BaseItemDto item, IApiClient apiClient, IImageManager imageManager, IServerEvents serverEvents, INavigator navigator, ISessionManager sessionManager)
        {
            _item = item;
            _apiClient = apiClient;
            _imageManager = imageManager;
            _serverEvents = serverEvents;
            _navigator = navigator;
            _sessionManager = sessionManager;

            Items = new RangeObservableCollection<ItemTileViewModel>();
            for (int i = 0; i < 6; i++) {
                Items.Add(CreateItem());
            }

            IsVisible = true;
            LoadItems();
        }

        public string Title
        {
            get { return "Similar"; }
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
            var query = new SimilarItemsQuery { Id = _item.Id, UserId = _sessionManager.CurrentUser.Id, Limit = 6 };

            switch (_item.Type) {
                case "Movie":
                    LoadItems(await _apiClient.GetSimilarMoviesAsync(query));
                    break;
                case "Series":
                case "Season":
                case "Episode":
                    LoadItems(await _apiClient.GetSimilarSeriesAsync(query));
                    break;
                case "Game":
                    LoadItems(await _apiClient.GetSimilarGamesAsync(query));
                    break;
                case "Album":
                    LoadItems(await _apiClient.GetSimilarAlbumsAsync(query));
                    break;
                default:
                    IsVisible = false;
                    break;
            }
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
                PreferredImageTypes = new[] { ImageType.Primary, ImageType.Backdrop, ImageType.Thumb }
            };
        }
    }

    public class RecommendationsSectionGenerator
        : IItemDetailSectionGenerator
    {
        private readonly IApiClient _apiClient;
        private readonly IImageManager _imageManager;
        private readonly IServerEvents _serverEvents;
        private readonly INavigator _navigator;
        private readonly ISessionManager _sessionManager;

        public RecommendationsSectionGenerator(IApiClient apiClient, IImageManager imageManager, IServerEvents serverEvents, INavigator navigator, ISessionManager sessionManager)
        {
            _apiClient = apiClient;
            _imageManager = imageManager;
            _serverEvents = serverEvents;
            _navigator = navigator;
            _sessionManager = sessionManager;
        }

        public bool HasSection(BaseItemDto item)
        {
            return item != null;
        }

        public Task<IItemDetailSection> GetSection(BaseItemDto item)
        {
            IItemDetailSection section = new RecommendationsViewModel(item, _apiClient, _imageManager, _serverEvents, _navigator, _sessionManager);
            return Task.FromResult(section);
        }
    }
}
