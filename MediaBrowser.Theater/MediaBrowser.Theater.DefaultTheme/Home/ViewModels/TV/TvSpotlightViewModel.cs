using System;
using System.Linq;
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
using MediaBrowser.Theater.Presentation.ViewModels;

namespace MediaBrowser.Theater.DefaultTheme.Home.ViewModels.TV
{
    public class TvSpotlightViewModel
        : BaseViewModel, IPanoramaPage
    {
        private readonly IApiClient _apiClient;
        private readonly ILogger _logger;
        private readonly ISessionManager _sessionManager;
        private CancellationTokenSource _mainViewCancellationTokenSource;
        private TvView _tvView;
        
        public TvSpotlightViewModel(IImageManager imageManager, INavigator navigator, IApiClient apiClient, ISessionManager sessionManager, ILogManager logManager)
        {
            _apiClient = apiClient;
            _sessionManager = sessionManager;
            _logger = logManager.GetLogger("TV Spotlight");
            SpotlightHeight = HomeViewModel.TileHeight * 2 + HomeViewModel.TileMargin * 2;
            SpotlightWidth = 16 * (SpotlightHeight / 9) + 100;

            LowerSpotlightWidth = SpotlightWidth/3 - HomeViewModel.TileMargin*1.5;
            LowerSpotlightHeight = SpotlightHeight/2;

            SpotlightViewModel = new ItemSpotlightViewModel(imageManager, apiClient) {
                ImageType = ImageType.Backdrop,
                ItemSelectedAction = i => navigator.Navigate(Go.To.Item(i))
            };

            AllShowsImagesViewModel = new ImageSlideshowViewModel(imageManager, Enumerable.Empty<string>()) {
                ImageStretch = Stretch.UniformToFill
            };

            LoadViewModels();
        }

        public double SpotlightWidth { get; private set; }
        public double SpotlightHeight { get; private set; }

        public double LowerSpotlightWidth { get; private set; }
        public double LowerSpotlightHeight { get; private set; }

        public ItemSpotlightViewModel SpotlightViewModel { get; private set; }
        public ImageSlideshowViewModel AllShowsImagesViewModel { get; private set; }
        public ICommand AllShowsCommand { get; private set; }
        public ICommand GenresCommand { get; private set; }
        public ICommand UpcommingCommand { get; private set; } 

        public string DisplayName
        {
            get { return "TV Spotlight"; }
        }

        public bool IsTitlePage
        {
            get { return true; }
        }

        private void DisposeMainViewCancellationTokenSource(bool cancel)
        {
            if (_mainViewCancellationTokenSource != null) {
                if (cancel) {
                    try {
                        _mainViewCancellationTokenSource.Cancel();
                    }
                    catch (ObjectDisposedException) { }
                }
                _mainViewCancellationTokenSource.Dispose();
                _mainViewCancellationTokenSource = null;
            }
        }

        private async void LoadViewModels()
        {
            CancellationTokenSource cancellationSource = _mainViewCancellationTokenSource = new CancellationTokenSource();

            try {
                TvView view = await _apiClient.GetTvView(_sessionManager.CurrentUser.Id, cancellationSource.Token);

                _tvView = view;

                cancellationSource.Token.ThrowIfCancellationRequested();

                LoadSpotlightViewModel(view);
                LoadAllShowsViewModel(view);
//                LoadMiniSpotlightsViewModel(view);
//                LoadNextUpViewModel(view);
//                LoadLatestEpisodesViewModel(view);
//                LoadResumableViewModel(view);
            }
            catch (Exception ex) {
                _logger.ErrorException("Error getting tv view", ex);
            }
            finally {
                DisposeMainViewCancellationTokenSource(false);
            }
        }

        private void LoadSpotlightViewModel(TvView view)
        {
            SpotlightViewModel.Items = view.SpotlightItems;
        }

        private void LoadAllShowsViewModel(TvView view)
        {
            var images = view.ShowsItems.Take(1).Select(i => _apiClient.GetImageUrl(i.Id, new ImageOptions
            {
                ImageType = i.ImageType,
                Tag = i.ImageTag,
                Height = Convert.ToInt32(HomeViewModel.TileWidth * 2),
                EnableImageEnhancers = false
            }));

            AllShowsImagesViewModel.Images.AddRange(images);
            AllShowsImagesViewModel.StartRotating();
        }
    }
}