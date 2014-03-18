using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
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
using MediaBrowser.Theater.Presentation.Controls;
using MediaBrowser.Theater.Presentation.ViewModels;

namespace MediaBrowser.Theater.DefaultTheme.Home.ViewModels.TV
{
    public class TvSpotlightViewModel
        : BaseViewModel, IKnownSize, IHomePage
    {
        private readonly IApiClient _apiClient;
        private readonly IImageManager _imageManager;
        private readonly ILogger _logger;
        private readonly double _miniSpotlightWidth;
        private readonly INavigator _navigator;
        //private readonly IPlaybackManager _playbackManager;
        private readonly IServerEvents _serverEvents;
        private readonly ISessionManager _sessionManager;
        private CancellationTokenSource _mainViewCancellationTokenSource;

        public TvSpotlightViewModel(Task<TvView> tvViewTask, IImageManager imageManager, INavigator navigator, IApiClient apiClient, IServerEvents serverEvents,
                                    /*IPlaybackManager playbackManager,*/ ISessionManager sessionManager, ILogManager logManager)
        {
            _imageManager = imageManager;
            _navigator = navigator;
            _apiClient = apiClient;
            _serverEvents = serverEvents;
            //_playbackManager = playbackManager;
            _sessionManager = sessionManager;
            _logger = logManager.GetLogger("TV Spotlight");
            SpotlightHeight = HomeViewModel.TileHeight*2 + HomeViewModel.TileMargin*2;
            SpotlightWidth = 16*(SpotlightHeight/9) + 100;
            _miniSpotlightWidth = HomeViewModel.TileWidth + (HomeViewModel.TileMargin/4) - 1;

            LowerSpotlightWidth = SpotlightWidth/3 - HomeViewModel.TileMargin*1.5;
            LowerSpotlightHeight = HomeViewModel.TileHeight;

            SpotlightViewModel = new ItemSpotlightViewModel(imageManager, apiClient) {
                ImageType = ImageType.Backdrop,
                ItemSelectedAction = i => navigator.Navigate(Go.To.Item(i))
            };

            AllShowsImagesViewModel = new ImageSlideshowViewModel(imageManager, Enumerable.Empty<string>()) {
                ImageStretch = Stretch.UniformToFill
            };

            MiniSpotlightItems = new RangeObservableCollection<ItemTileViewModel> {
                CreateMiniSpotlightItem(),
                CreateMiniSpotlightItem(),
                CreateMiniSpotlightItem(),
            };

            LoadViewModels(tvViewTask);
        }

        public double SpotlightWidth { get; private set; }
        public double SpotlightHeight { get; private set; }

        public double LowerSpotlightWidth { get; private set; }
        public double LowerSpotlightHeight { get; private set; }

        public ItemSpotlightViewModel SpotlightViewModel { get; private set; }
        public RangeObservableCollection<ItemTileViewModel> MiniSpotlightItems { get; private set; }
        public ImageSlideshowViewModel AllShowsImagesViewModel { get; private set; }
        public ICommand AllShowsCommand { get; private set; }
        public ICommand GenresCommand { get; private set; }
        public ICommand UpcomingCommand { get; private set; }

        public string Title
        {
            get { return "MediaBrowser.Theater.DefaultTheme:Strings:Home_TvSpotlight_Title".Localize(); }
        }

        public string SectionTitle
        {
            get { return "MediaBrowser.Theater.DefaultTheme:Strings:Home_TVSectionTitle".Localize(); }
        }

        public Size Size
        {
            get
            {
                return new Size(SpotlightWidth + _miniSpotlightWidth + 4*HomeViewModel.TileMargin + HomeViewModel.SectionSpacing,
                                SpotlightHeight + HomeViewModel.TileHeight + 4*HomeViewModel.TileMargin);
            }
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

        public static string GetDisplayName(BaseItemDto item)
        {
            string name = item.Name;

            if (item.IsType("Episode")) {
                name = item.SeriesName;

                if (item.IndexNumber.HasValue && item.ParentIndexNumber.HasValue) {
                    name = name + " " + string.Format("S{0}, E{1}", item.ParentIndexNumber.Value, item.IndexNumber.Value);
                }
            }

            return name;
        }

        private async void LoadViewModels(Task<TvView> tvViewTask)
        {
            CancellationTokenSource cancellationSource = _mainViewCancellationTokenSource = new CancellationTokenSource();

            try {
                TvView view = await tvViewTask;

                cancellationSource.Token.ThrowIfCancellationRequested();

                LoadSpotlightViewModel(view);
                LoadAllShowsViewModel(view);
                LoadMiniSpotlightsViewModel(view);
            }
            catch (Exception ex) {
                _logger.ErrorException("Error getting tv view", ex);
            }
            finally {
                DisposeMainViewCancellationTokenSource(false);
            }
        }

        private ItemTileViewModel CreateMiniSpotlightItem()
        {
            return new ItemTileViewModel(_apiClient, _imageManager, _serverEvents, _navigator, /*_playbackManager,*/ null) {
                DesiredImageWidth = _miniSpotlightWidth,
                DesiredImageHeight = HomeViewModel.TileHeight,
                PreferredImageTypes = new[] { ImageType.Backdrop, ImageType.Thumb },
                DisplayNameGenerator = GetDisplayName,
                DownloadImagesAtExactSize = true
            };
        }

        private void LoadMiniSpotlightsViewModel(TvView view)
        {
            BaseItemDto[] items = view.MiniSpotlights.Take(3).ToArray();

            for (int i = 0; i < items.Length; i++) {
                if (MiniSpotlightItems.Count > i) {
                    MiniSpotlightItems[i].Item = items[i];
                } else {
                    ItemTileViewModel vm = CreateMiniSpotlightItem();
                    vm.Item = items[i];

                    MiniSpotlightItems.Add(vm);
                }
            }

            if (MiniSpotlightItems.Count > items.Length) {
                List<ItemTileViewModel> toRemove = MiniSpotlightItems.Skip(items.Length).ToList();
                MiniSpotlightItems.RemoveRange(toRemove);
            }
        }

        private void LoadSpotlightViewModel(TvView view)
        {
            SpotlightViewModel.Items = view.SpotlightItems;
        }

        private void LoadAllShowsViewModel(TvView view)
        {
            IEnumerable<string> images = view.ShowsItems.Take(1).Select(i => _apiClient.GetImageUrl(i.Id, new ImageOptions {
                ImageType = i.ImageType,
                Tag = i.ImageTag,
                Height = Convert.ToInt32(HomeViewModel.TileWidth*2),
                EnableImageEnhancers = false
            }));

            AllShowsImagesViewModel.Images.AddRange(images);
            AllShowsImagesViewModel.StartRotating();
        }
    }
}