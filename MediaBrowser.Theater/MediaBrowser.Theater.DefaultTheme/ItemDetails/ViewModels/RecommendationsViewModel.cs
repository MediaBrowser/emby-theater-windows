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
    public class RecommendationsViewModel
        : BaseViewModel, IItemDetailSection, IKnownSize
    {
        private static readonly double PosterHeight = PersonListItemViewModel.Height;

        private readonly BaseItemDto _item;
        private readonly IConnectionManager _connectionManager;
        private readonly IImageManager _imageManager;
        private readonly INavigator _navigator;
        private readonly ISessionManager _sessionManager;
        private readonly IPlaybackManager _playbackManager;

        private bool _isVisible;

        public int SortOrder { get { return 4; } }

        public RangeObservableCollection<ItemTileViewModel> Items { get; private set; }

        public RecommendationsViewModel(BaseItemDto item, IConnectionManager connectionManager, IImageManager imageManager, INavigator navigator, ISessionManager sessionManager, IPlaybackManager playbackManager)
        {
            _item = item;
            _connectionManager = connectionManager;
            _imageManager = imageManager;
            _navigator = navigator;
            _sessionManager = sessionManager;
            _playbackManager = playbackManager;

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
                var itemSize = Items.First().Size;

                return new Size(width * (itemSize.Width + 2 * HomeViewModel.TileMargin) + 20, 2 * itemSize.Height + 4 * HomeViewModel.TileMargin);
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
            var apiClient = _connectionManager.GetApiClient(_item);

            switch (_item.Type) {
                case "Movie":
                    LoadItems(await apiClient.GetSimilarMoviesAsync(query));
                    break;
                case "Series":
                case "Season":
                case "Episode":
                    LoadItems(await apiClient.GetSimilarSeriesAsync(query));
                    break;
                case "Game":
                    LoadItems(await apiClient.GetSimilarGamesAsync(query));
                    break;
                case "Album":
                    LoadItems(await apiClient.GetSimilarAlbumsAsync(query));
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
            return new ItemTileViewModel(_connectionManager, _imageManager, _navigator, _playbackManager, _sessionManager, null)
            {
                DesiredImageHeight = PosterHeight,
                PreferredImageTypes = new[] { ImageType.Primary, ImageType.Backdrop, ImageType.Thumb }
            };
        }
    }

    public class RecommendationsSectionGenerator
        : IItemDetailSectionGenerator
    {
        private readonly IConnectionManager _connectionManager;
        private readonly IImageManager _imageManager;
        private readonly INavigator _navigator;
        private readonly ISessionManager _sessionManager;
        private readonly IPlaybackManager _playbackManager;

        public RecommendationsSectionGenerator(IConnectionManager connectionManager, IImageManager imageManager, INavigator navigator, ISessionManager sessionManager, IPlaybackManager playbackManager)
        {
            _connectionManager = connectionManager;
            _imageManager = imageManager;
            _navigator = navigator;
            _sessionManager = sessionManager;
            _playbackManager = playbackManager;
        }

        public bool HasSection(BaseItemDto item)
        {
            return item != null && item.Type != "Person";
        }

        public Task<IEnumerable<IItemDetailSection>> GetSections(BaseItemDto item)
        {
            IItemDetailSection section = new RecommendationsViewModel(item, _connectionManager, _imageManager, _navigator, _sessionManager, _playbackManager);
            return Task.FromResult<IEnumerable<IItemDetailSection>>(new[] { section });
        }
    }
}
