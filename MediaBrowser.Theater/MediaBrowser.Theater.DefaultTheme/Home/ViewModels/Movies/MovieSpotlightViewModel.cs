using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media;
using MediaBrowser.Model.ApiClient;
using MediaBrowser.Model.Dto;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.Logging;
using MediaBrowser.Theater.Api.Navigation;
using MediaBrowser.Theater.Api.Session;
using MediaBrowser.Theater.Api.UserInterface;
using MediaBrowser.Theater.DefaultTheme.Core.ViewModels;
using MediaBrowser.Theater.Presentation;
using MediaBrowser.Theater.Presentation.ViewModels;

namespace MediaBrowser.Theater.DefaultTheme.Home.ViewModels.Movies
{
    public class MovieSpotlightViewModel
        : BaseViewModel, IPanoramaPage
    {
        private readonly IApiClient _apiClient;
        private readonly IImageManager _imageManager;
        private readonly ILogger _logger;
        private readonly INavigator _navigator;
        //private readonly IPlaybackManager _playbackManager;
        private readonly IServerEvents _serverEvents;
        private readonly ISessionManager _sessionManager;
        private CancellationTokenSource _mainViewCancellationTokenSource;

        public MovieSpotlightViewModel(Task<MoviesView> moviesViewTask, IImageManager imageManager, INavigator navigator, IApiClient apiClient, IServerEvents serverEvents,
                                    /*IPlaybackManager playbackManager,*/ ISessionManager sessionManager, ILogManager logManager)
        {
            _imageManager = imageManager;
            _navigator = navigator;
            _apiClient = apiClient;
            _serverEvents = serverEvents;
            //_playbackManager = playbackManager;
            _sessionManager = sessionManager;
            _logger = logManager.GetLogger("Movies Spotlight");
            SpotlightHeight = HomeViewModel.TileHeight*2 + HomeViewModel.TileMargin*2;
            SpotlightWidth = 16*(SpotlightHeight/9) + 100;

            LowerSpotlightWidth = SpotlightWidth/2 - HomeViewModel.TileMargin;
            LowerSpotlightHeight = HomeViewModel.TileHeight;

            SpotlightViewModel = new ItemSpotlightViewModel(imageManager, apiClient) {
                ImageType = ImageType.Backdrop,
                ItemSelectedAction = i => navigator.Navigate(Go.To.Item(i))
            };

            AllMoviesImagesViewModel = new ImageSlideshowViewModel(imageManager, Enumerable.Empty<string>()) {
                ImageStretch = Stretch.UniformToFill
            };

            MiniSpotlightItems = new RangeObservableCollection<ItemTileViewModel>() { 
                CreateMiniSpotlightItem(),
                CreateMiniSpotlightItem(),
                CreateMiniSpotlightItem(),
            };

            LoadViewModels(moviesViewTask);
        }

        public double SpotlightWidth { get; private set; }
        public double SpotlightHeight { get; private set; }

        public double LowerSpotlightWidth { get; private set; }
        public double LowerSpotlightHeight { get; private set; }

        public ItemSpotlightViewModel SpotlightViewModel { get; private set; }
        public RangeObservableCollection<ItemTileViewModel> MiniSpotlightItems { get; private set; }
        public ImageSlideshowViewModel AllMoviesImagesViewModel { get; private set; }
        public ICommand AllMoviesCommand { get; private set; }
        public ICommand TrailersCommand { get; private set; }

        public string DisplayName
        {
            get { return "MediaBrowser.Theater.DefaultTheme:Strings:Home_MovieSpotlight_Title".Localize(); }
        }

        public bool IsTitlePage
        {
            get { return true; }
        }

        public bool IsVisible
        {
            get { return true; }
        }

        private void DisposeMainViewCancellationTokenSource(bool cancel)
        {
            if (_mainViewCancellationTokenSource != null) {
                if (cancel) {
                    try {
                        _mainViewCancellationTokenSource.Cancel();
                    } catch (ObjectDisposedException) { }
                }
                _mainViewCancellationTokenSource.Dispose();
                _mainViewCancellationTokenSource = null;
            }
        }

        private async void LoadViewModels(Task<MoviesView> moviesViewTask)
        {
            CancellationTokenSource cancellationSource = _mainViewCancellationTokenSource = new CancellationTokenSource();

            try {
                MoviesView view = await moviesViewTask;
                
                cancellationSource.Token.ThrowIfCancellationRequested();

                LoadSpotlightViewModel(view);
                LoadAllMoviesViewModel(view);
                LoadMiniSpotlightsViewModel(view);
            } catch (Exception ex) {
                _logger.ErrorException("Error getting tv view", ex);
            } finally {
                DisposeMainViewCancellationTokenSource(false);
            }
        }

        private ItemTileViewModel CreateMiniSpotlightItem()
        {
            return new ItemTileViewModel(_apiClient, _imageManager, _serverEvents, _navigator, /*_playbackManager,*/ null) {
                ImageWidth = HomeViewModel.TileWidth + (HomeViewModel.TileMargin/4) - 1,
                ImageHeight = HomeViewModel.TileHeight,
                PreferredImageTypes = new[] { ImageType.Backdrop, ImageType.Thumb },
                DownloadImagesAtExactSize = true
            };
        }

        private void LoadMiniSpotlightsViewModel(MoviesView view)
        {
            BaseItemDto[] items = view.MiniSpotlights.Take(3).ToArray();

            for (int i = 0; i < items.Length; i++) {
                if (MiniSpotlightItems.Count > i) {
                    MiniSpotlightItems[i].Item = items[i];
                } else {
                    var vm = CreateMiniSpotlightItem();
                    vm.Item = items[i];

                    MiniSpotlightItems.Add(vm);
                }
            }

            if (MiniSpotlightItems.Count > items.Length) {
                var toRemove = MiniSpotlightItems.Skip(items.Length).ToList();
                MiniSpotlightItems.RemoveRange(toRemove);
            }
        }

        private void LoadSpotlightViewModel(MoviesView view)
        {
            SpotlightViewModel.Items = view.SpotlightItems;
        }

        private void LoadAllMoviesViewModel(MoviesView view)
        {
            IEnumerable<string> images = view.MovieItems.Take(1).Select(i => _apiClient.GetImageUrl(i.Id, new ImageOptions {
                ImageType = i.ImageType,
                Tag = i.ImageTag,
                Height = Convert.ToInt32(HomeViewModel.TileWidth*2),
                EnableImageEnhancers = false
            }));

            AllMoviesImagesViewModel.Images.AddRange(images);
            AllMoviesImagesViewModel.StartRotating();
        }
    }
}
