using MediaBrowser.Model.ApiClient;
using MediaBrowser.Model.Dto;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.Logging;
using MediaBrowser.Model.Querying;
using MediaBrowser.Plugins.DefaultTheme.ListPage;
using MediaBrowser.Theater.Interfaces.Navigation;
using MediaBrowser.Theater.Interfaces.Playback;
using MediaBrowser.Theater.Interfaces.Presentation;
using MediaBrowser.Theater.Interfaces.Session;
using MediaBrowser.Theater.Interfaces.ViewModels;
using MediaBrowser.Theater.Presentation.ViewModels;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace MediaBrowser.Plugins.DefaultTheme.Home
{
    public class TvViewModel : BaseHomePageSectionViewModel, IDisposable
    {
        private readonly ISessionManager _sessionManager;
        private readonly IPlaybackManager _playbackManager;
        private readonly IImageManager _imageManager;
        private readonly INavigationService _navService;
        private readonly ILogger _logger;

        public ItemListViewModel NextUpViewModel { get; private set; }
        public ItemListViewModel ResumeViewModel { get; private set; }

        public GalleryViewModel AllShowsViewModel { get; private set; }
        public GalleryViewModel ActorsViewModel { get; private set; }

        public ImageViewerViewModel SpotlightViewModel { get; private set; }
        public GalleryViewModel RomanticSeriesViewModel { get; private set; }

        public TvViewModel(IPresentationManager presentation, IImageManager imageManager, IApiClient apiClient, ISessionManager session, INavigationService nav, IPlaybackManager playback, ILogger logger, double tileWidth, double tileHeight)
            : base(presentation, apiClient)
        {
            _sessionManager = session;
            _playbackManager = playback;
            _imageManager = imageManager;
            _navService = nav;
            _logger = logger;

            TileWidth = tileWidth;
            TileHeight = tileHeight;

            NextUpViewModel = new ItemListViewModel(GetNextUpAsync, presentation, imageManager, apiClient, session, nav, playback, logger)
            {
                ImageDisplayWidth = TileWidth,
                ImageDisplayHeightGenerator = v => TileHeight,
                DisplayNameGenerator = HomePageViewModel.GetDisplayName,
                EnableBackdropsForCurrentItem = false
            };
            NextUpViewModel.PropertyChanged += NextUpViewModel_PropertyChanged;

            ResumeViewModel = new ItemListViewModel(GetResumeablesAsync, presentation, imageManager, apiClient, session, nav, playback, logger)
            {
                ImageDisplayWidth = TileWidth,
                ImageDisplayHeightGenerator = v => TileHeight,
                DisplayNameGenerator = HomePageViewModel.GetDisplayName,
                EnableBackdropsForCurrentItem = false
            };
            ResumeViewModel.PropertyChanged += ResumeViewModel_PropertyChanged;

            ActorsViewModel = new GalleryViewModel(ApiClient, _imageManager, _navService)
            {
                GalleryHeight = TileHeight,
                GalleryWidth = TileWidth * 10 / 16,
                CustomCommandAction = NavigateToActors
            };

            AllShowsViewModel = new GalleryViewModel(ApiClient, _imageManager, _navService)
            {
                GalleryHeight = TileHeight,
                GalleryWidth = TileWidth * 10 / 16,
                CustomCommandAction = NavigateToAllShows
            };

            RomanticSeriesViewModel = new GalleryViewModel(ApiClient, _imageManager, _navService)
            {
                GalleryHeight = TileHeight,
                GalleryWidth = TileWidth * 10 / 16,
                CustomCommandAction = () => NavigateWithLoading(NavigateToRomanticTvInternal)
            };

            var spotlightTileWidth = TileWidth * 2 + TilePadding;
            var spotlightTileHeight = spotlightTileWidth * 9 / 16;

            SpotlightViewModel = new ImageViewerViewModel(_imageManager, new List<ImageViewerImage>())
            {
                Height = spotlightTileHeight,
                Width = spotlightTileWidth,
                CustomCommandAction = i => _navService.NavigateToItem(i.Item, ViewType.Tv)
            };

            LoadViewModels();
        }

        private CancellationTokenSource _mainViewCancellationTokenSource;
        private void DisposeMainViewCancellationTokenSource(bool cancel)
        {
            if (_mainViewCancellationTokenSource != null)
            {
                if (cancel)
                {
                    try
                    {
                        _mainViewCancellationTokenSource.Cancel();
                    }
                    catch (ObjectDisposedException)
                    {
                        
                    }
                }
                _mainViewCancellationTokenSource.Dispose();
                _mainViewCancellationTokenSource = null;
            }
        }

        private async void LoadViewModels()
        {
            PresentationManager.ShowLoadingAnimation();

            var cancellationSource = _mainViewCancellationTokenSource = new CancellationTokenSource();

            try
            {
                var view = await ApiClient.GetTvView(_sessionManager.CurrentUser.Id, cancellationSource.Token);

                cancellationSource.Token.ThrowIfCancellationRequested();

                LoadSpotlightViewModel(view);
                LoadAllShowsViewModel(view);
                LoadRomanticSeriesViewModel(view);
                LoadActorsViewModel(view);
            }
            catch (Exception ex)
            {
                _logger.ErrorException("Error getting tv view", ex);
                PresentationManager.ShowDefaultErrorMessage();
            }
            finally
            {
                PresentationManager.HideLoadingAnimation();
                DisposeMainViewCancellationTokenSource(false);
            }
        }

        void ResumeViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            ShowResume = ResumeViewModel.ItemCount > 0;
        }

        void NextUpViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            ShowNextUp = NextUpViewModel.ItemCount > 0;
        }

        private bool _showNextUp;
        public bool ShowNextUp
        {
            get { return _showNextUp; }

            set
            {
                var changed = _showNextUp != value;

                _showNextUp = value;

                if (changed)
                {
                    OnPropertyChanged("ShowNextUp");
                }
            }
        }

        private bool _showResume;
        public bool ShowResume
        {
            get { return _showResume; }

            set
            {
                var changed = _showResume != value;

                _showResume = value;

                if (changed)
                {
                    OnPropertyChanged("ShowResume");
                }
            }
        }

        private bool _showRomanticSeries;
        public bool ShowRomanticSeries
        {
            get { return _showRomanticSeries; }

            set
            {
                var changed = _showRomanticSeries != value;

                _showRomanticSeries = value;

                if (changed)
                {
                    OnPropertyChanged("ShowRomanticSeries");
                }
            }
        }

        private void LoadSpotlightViewModel(TvView view)
        {
            const ImageType imageType = ImageType.Backdrop;

            var tileWidth = TileWidth * 2 + TilePadding;
            var tileHeight = tileWidth * 9 / 16;

            BackdropItems = view.SpotlightItems.OrderBy(i => Guid.NewGuid()).ToArray();

            var images = view.SpotlightItems.Select(i => new ImageViewerImage
            {
                Url = ApiClient.GetImageUrl(i, new ImageOptions
                {
                    Height = Convert.ToInt32(tileHeight),
                    Width = Convert.ToInt32(tileWidth),
                    ImageType = imageType

                }),

                Caption = i.Name,
                Item = i

            }).ToList();

            SpotlightViewModel.Images.AddRange(images);
            SpotlightViewModel.StartRotating(8000);
        }

        private void LoadActorsViewModel(TvView view)
        {
            var images = view.ActorItems.Take(1).Select(i => ApiClient.GetPersonImageUrl(i.Name, new ImageOptions
            {
                ImageType = i.ImageType,
                Tag = i.ImageTag,
                Height = Convert.ToInt32(TileWidth * 2),
                EnableImageEnhancers = false
            }));

            ActorsViewModel.AddImages(images);
        }

        private void LoadRomanticSeriesViewModel(TvView view)
        {
            var now = DateTime.Now;

            if (now.DayOfWeek == DayOfWeek.Friday)
            {
                ShowRomanticSeries = view.RomanceItems.Length > 0 && now.Hour >= 15;
            }
            else if (now.DayOfWeek == DayOfWeek.Saturday)
            {
                ShowRomanticSeries = view.RomanceItems.Length > 0 && (now.Hour < 3 || now.Hour >= 15);
            }
            else if (now.DayOfWeek == DayOfWeek.Sunday)
            {
                ShowRomanticSeries = view.RomanceItems.Length > 0 && now.Hour < 3;
            }
            else
            {
                ShowRomanticSeries = false;
            }

            var images = view.RomanceItems.Take(1).Select(i => ApiClient.GetImageUrl(i.Id, new ImageOptions
            {
                ImageType = i.ImageType,
                Tag = i.ImageTag,
                Width = Convert.ToInt32(TileWidth * 2),
                EnableImageEnhancers = false
            }));

            RomanticSeriesViewModel.AddImages(images);
        }

        private async void NavigateToActors()
        {
            PresentationManager.ShowLoadingAnimation();

            try
            {
                await NavigateToActorsInternal();
            }
            finally
            {
                PresentationManager.HideLoadingAnimation();
            }
        }

        private async Task NavigateToActorsInternal()
        {
            var item = await ApiClient.GetRootFolderAsync(_sessionManager.CurrentUser.Id);

            var displayPreferences = await PresentationManager.GetDisplayPreferences("TVActors", CancellationToken.None);

            var page = new FolderPage(item, displayPreferences, ApiClient, _imageManager, _sessionManager,
                                      PresentationManager, _navService, _playbackManager, _logger);

            page.CustomPageTitle = "TV | Actors";

            page.ViewType = ViewType.Tv;
            page.CustomItemQuery = GetAllActors;

            await _navService.Navigate(page);
        }

        private void LoadAllShowsViewModel(TvView view)
        {
            var images = view.ShowsItems.Take(1).Select(i => ApiClient.GetImageUrl(i.Id, new ImageOptions
            {
                ImageType = i.ImageType,
                Tag = i.ImageTag,
                Height = Convert.ToInt32(TileWidth * 2),
                EnableImageEnhancers = false
            }));

            AllShowsViewModel.AddImages(images);
        }

        private async void NavigateToAllShows()
        {
            PresentationManager.ShowLoadingAnimation();

            try
            {
                await NavigateToAllShowsInternal();
            }
            finally
            {
                PresentationManager.HideLoadingAnimation();
            }
        }

        private async Task NavigateToAllShowsInternal()
        {
            var item = await ApiClient.GetRootFolderAsync(_sessionManager.CurrentUser.Id);

            var displayPreferences = await PresentationManager.GetDisplayPreferences("AllShows", CancellationToken.None);

            var page = new FolderPage(item, displayPreferences, ApiClient, _imageManager, _sessionManager,
                                      PresentationManager, _navService, _playbackManager, _logger);

            page.CustomPageTitle = "TV Shows";

            page.ViewType = ViewType.Tv;
            page.CustomItemQuery = GetAllShows;

            await _navService.Navigate(page);
        }

        private Task<ItemsResult> GetAllShows(DisplayPreferences displayPreferences)
        {
            var query = new ItemQuery
            {
                Fields = FolderPage.QueryFields,

                UserId = _sessionManager.CurrentUser.Id,

                IncludeItemTypes = new[] { "Series" },

                SortBy = !String.IsNullOrEmpty(displayPreferences.SortBy)
                             ? new[] { displayPreferences.SortBy }
                             : new[] { ItemSortBy.SortName },

                SortOrder = displayPreferences.SortOrder,

                Recursive = true
            };

            return ApiClient.GetItemsAsync(query);
        }

        private Task<ItemsResult> GetAllActors(DisplayPreferences displayPreferences)
        {
            var fields = FolderPage.QueryFields.ToList();
            fields.Remove(ItemFields.ItemCounts);
            fields.Remove(ItemFields.Overview);
            fields.Remove(ItemFields.DisplayPreferencesId);
            fields.Remove(ItemFields.DateCreated);

            var query = new PersonsQuery
            {
                Fields = fields.ToArray(),

                IncludeItemTypes = new[] { "Series", "Episode" },

                SortBy = !String.IsNullOrEmpty(displayPreferences.SortBy)
                             ? new[] { displayPreferences.SortBy }
                             : new[] { ItemSortBy.SortName },

                SortOrder = displayPreferences.SortOrder,

                Recursive = true,

                PersonTypes = new[] { PersonType.Actor, PersonType.GuestStar }
            };

            return ApiClient.GetPeopleAsync(query);
        }

        private async Task NavigateToRomanticTvInternal()
        {
            var item = await ApiClient.GetRootFolderAsync(_sessionManager.CurrentUser.Id);

            var displayPreferences = await PresentationManager.GetDisplayPreferences("RomanticTv", CancellationToken.None);

            var page = new FolderPage(item, displayPreferences, ApiClient, _imageManager, _sessionManager,
                                      PresentationManager, _navService, _playbackManager, _logger);

            page.CustomPageTitle = "Date Night";

            page.ViewType = ViewType.Tv;
            page.CustomItemQuery = GetRomanticSeries;

            await _navService.Navigate(page);
        }

        private Task<ItemsResult> GetRomanticSeries(DisplayPreferences displayPreferences)
        {
            var query = new ItemQuery
            {
                Fields = FolderPage.QueryFields,

                UserId = _sessionManager.CurrentUser.Id,

                IncludeItemTypes = new[] { "Series" },

                Genres = new[] { ApiClientExtensions.RomanceGenre },

                SortBy = !String.IsNullOrEmpty(displayPreferences.SortBy)
                             ? new[] { displayPreferences.SortBy }
                             : new[] { ItemSortBy.SortName },

                SortOrder = displayPreferences.SortOrder,

                Recursive = true
            };

            return ApiClient.GetItemsAsync(query);
        }

        private Task<ItemsResult> GetNextUpAsync()
        {
            var query = new NextUpQuery
            {
                Fields = new[]
                        {
                            ItemFields.PrimaryImageAspectRatio,
                            ItemFields.DateCreated,
                            ItemFields.DisplayPreferencesId
                        },

                UserId = _sessionManager.CurrentUser.Id,

                Limit = 30
            };

            return ApiClient.GetNextUpAsync(query);
        }

        private Task<ItemsResult> GetResumeablesAsync()
        {
            var query = new ItemQuery
            {
                Fields = new[]
                        {
                            ItemFields.PrimaryImageAspectRatio,
                            ItemFields.DateCreated,
                            ItemFields.DisplayPreferencesId
                        },

                UserId = _sessionManager.CurrentUser.Id,

                SortBy = new[] { ItemSortBy.DatePlayed },

                SortOrder = SortOrder.Descending,

                IncludeItemTypes = new[] { "Episode" },

                Filters = new[] { ItemFilter.IsResumable },

                Limit = 4,

                Recursive = true
            };

            return ApiClient.GetItemsAsync(query);
        }

        public void Dispose()
        {
            if (SpotlightViewModel != null)
            {
                SpotlightViewModel.Dispose();
            }
            if (ActorsViewModel != null)
            {
                ActorsViewModel.Dispose();
            }
            if (AllShowsViewModel != null)
            {
                AllShowsViewModel.Dispose();
            }
            if (ResumeViewModel != null)
            {
                ResumeViewModel.Dispose();
            }
            if (NextUpViewModel != null)
            {
                NextUpViewModel.Dispose();
            }
            DisposeMainViewCancellationTokenSource(true);
        }
    }
}
