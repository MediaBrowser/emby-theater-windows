using System;
using System.Collections.Generic;
using System.Linq;
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
using MediaBrowser.Theater.Playback;
using MediaBrowser.Theater.Presentation;
using MediaBrowser.Theater.Presentation.Controls;
using MediaBrowser.Theater.Presentation.ViewModels;

namespace MediaBrowser.Theater.DefaultTheme.Home.ViewModels.Movies
{
    public class LatestMoviesViewModel
        : BaseViewModel, IKnownSize, IHomePage
    {
        private const double PosterHeight = (HomeViewModel.TileHeight*1.5) + HomeViewModel.TileMargin;
        private const double PosterWidth = PosterHeight*2/3.0;

        private readonly IConnectionManager _connectionManager;
        private readonly IImageManager _imageManager;
        private readonly INavigator _navigator;
        private readonly ISessionManager _sessionManager;
        private readonly IPlaybackManager _playbackManager;
        private readonly IApiClient _apiClient;

        private bool _isVisible;

        public LatestMoviesViewModel(BaseItemDto movieFolder, IConnectionManager connectionManager, IImageManager imageManager, INavigator navigator, ISessionManager sessionManager, IPlaybackManager playbackManager)
        {
            _apiClient = connectionManager.GetApiClient(movieFolder);
            _connectionManager = connectionManager;
            _imageManager = imageManager;
            _navigator = navigator;
            _sessionManager = sessionManager;
            _playbackManager = playbackManager;

            SectionTitle = movieFolder.Name;
            Title = "MediaBrowser.Theater.DefaultTheme:Strings:Home_LatestMovies_Title".Localize();

            Movies = new RangeObservableCollection<ItemTileViewModel>();
            for (int i = 0; i < 8; i++) {
                Movies.Add(CreateMovieItem());
            }

            IsVisible = true;
            LoadItems(movieFolder);
        }

        public RangeObservableCollection<ItemTileViewModel> Movies { get; private set; }

        public string Title { get; set; }

        public bool IsVisible
        {
            get { return _isVisible; }
            private set
            {
                if (Equals(_isVisible, value)) {
                    return;
                }

                _isVisible = value;
                OnPropertyChanged();
            }
        }

        public string SectionTitle { get; set; }

        public int Index { get; set; }

        public Size Size
        {
            get
            {
                if (Movies.Count == 0) {
                    return new Size();
                }

                var width = (int) Math.Ceiling(Movies.Count/2.0);

                return new Size(width*(PosterWidth + 2*HomeViewModel.TileMargin) + HomeViewModel.SectionSpacing,
                                2*(PosterWidth + 2*HomeViewModel.TileMargin));
            }
        }

        private async void LoadItems(BaseItemDto movieFolder)
        {
            var result = await _apiClient.GetItemsAsync(new ItemQuery {
                UserId = _sessionManager.CurrentUser.Id,
                ParentId = movieFolder.Id,
                IncludeItemTypes = new[] { "Movie" },
                SortBy = new[] { ItemSortBy.DateCreated },
                SortOrder = SortOrder.Descending,
                Filters = new[] { ItemFilter.IsRecentlyAdded },
                Limit = 8,
                Recursive = true
            });

            var items = result.Items;

            for (int i = 0; i < items.Length; i++) {
                if (Movies.Count > i) {
                    Movies[i].Item = items[i];
                } else {
                    ItemTileViewModel vm = CreateMovieItem();
                    vm.Item = items[i];

                    Movies.Add(vm);
                }
            }

            if (Movies.Count > items.Length) {
                List<ItemTileViewModel> toRemove = Movies.Skip(items.Length).ToList();
                Movies.RemoveRange(toRemove);
            }

            IsVisible = Movies.Count > 0;
            OnPropertyChanged("Size");
        }

        private ItemTileViewModel CreateMovieItem()
        {
            return new ItemTileViewModel(_connectionManager, _imageManager, _navigator, _playbackManager, _sessionManager, null) {
                DesiredImageWidth = PosterWidth,
                DesiredImageHeight = PosterHeight,
                ShowCaptionBar = false,
                PreferredImageTypes = new[] { ImageType.Primary, ImageType.Backdrop, ImageType.Thumb }
            };
        }
    }
}