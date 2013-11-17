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
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media;

namespace MediaBrowser.Plugins.DefaultTheme.Home
{
    public class MoviesViewModel : BaseHomePageSectionViewModel, IDisposable, IHasActivePresentation
    {
        private readonly ISessionManager _sessionManager;
        private readonly IImageManager _imageManager;
        private readonly INavigationService _navService;
        private readonly IPlaybackManager _playbackManager;
        private readonly ILogger _logger;
        private readonly IServerEvents _serverEvents;

        public ItemListViewModel LatestTrailersViewModel { get; private set; }
        public ItemListViewModel LatestMoviesViewModel { get; private set; }
        public ItemListViewModel MiniSpotlightsViewModel { get; private set; }

        public ImageViewerViewModel SpotlightViewModel { get; private set; }

        public GalleryViewModel GenresViewModel { get; private set; }
        public GalleryViewModel AllMoviesViewModel { get; private set; }
        public GalleryViewModel YearsViewModel { get; private set; }
        public GalleryViewModel TrailersViewModel { get; private set; }

        private readonly double _posterTileHeight;
        private readonly double _posterTileWidth;

        private MoviesView _moviesView;

        public MoviesViewModel(IPresentationManager presentation, IImageManager imageManager, IApiClient apiClient, ISessionManager session, INavigationService nav, IPlaybackManager playback, ILogger logger, double tileWidth, double tileHeight, IServerEvents serverEvents)
            : base(presentation, apiClient)
        {
            _sessionManager = session;
            _imageManager = imageManager;
            _navService = nav;
            _playbackManager = playback;
            _logger = logger;
            _serverEvents = serverEvents;

            TileWidth = tileWidth;
            TileHeight = tileHeight;

            _posterTileHeight = (TileHeight * 1.48) + TilePadding / 2;
            _posterTileWidth = _posterTileHeight * 2 / 3;

            var spotlightTileHeight = TileHeight * 2 + TilePadding / 2;
            var spotlightTileWidth = 16 * (spotlightTileHeight / 9) + 70;

            var lowerSpotlightWidth = ((spotlightTileWidth - (TilePadding)) / 4) - 3.15;

            GenresViewModel = new GalleryViewModel(ApiClient, _imageManager, _navService)
            {
                GalleryHeight = TileHeight,
                GalleryWidth = lowerSpotlightWidth,
                CustomCommandAction = () => NavigateWithLoading(NavigateToGenresInternal)
            };

            TrailersViewModel = new GalleryViewModel(ApiClient, _imageManager, _navService)
            {
                GalleryHeight = TileHeight,
                GalleryWidth = lowerSpotlightWidth,
                CustomCommandAction = () => NavigateWithLoading(() => NavigateToMoviesInternal("Trailers"))
            };
            
            YearsViewModel = new GalleryViewModel(ApiClient, _imageManager, _navService)
            {
                GalleryHeight = TileHeight,
                GalleryWidth = lowerSpotlightWidth,
                CustomCommandAction = () => NavigateWithLoading(NavigateToYearsInternal)
            };

            AllMoviesViewModel = new GalleryViewModel(ApiClient, _imageManager, _navService)
            {
                GalleryHeight = TileHeight,
                GalleryWidth = lowerSpotlightWidth,
                CustomCommandAction = () => NavigateWithLoading(() => NavigateToMoviesInternal("AllMovies"))
            };

            SpotlightViewModel = new ImageViewerViewModel(_imageManager, new List<ImageViewerImage>())
            {
                Height = spotlightTileHeight,
                Width = spotlightTileWidth,
                CustomCommandAction = i => _navService.NavigateToItem(i.Item, ViewType.Movies),
                ImageStretch = Stretch.UniformToFill
            };

            LoadViewModels();
        }

        public const int PosterWidth = 214;
        public const int ThumbstripWidth = 600;
        public const int ListImageWidth = 160;
        public const int PosterStripWidth = 290;

        public static void SetDefaults(ListPageConfig config)
        {
            config.DefaultViewType = ListViewTypes.Poster;
            config.PosterImageWidth = PosterWidth;
            config.ThumbImageWidth = ThumbstripWidth;
            config.ListImageWidth = ListImageWidth;
            config.PosterStripImageWidth = PosterStripWidth;
        }

        private async void LoadViewModels()
        {
            PresentationManager.ShowLoadingAnimation();

            var cancellationSource = _mainViewCancellationTokenSource = new CancellationTokenSource();

            try
            {
                var view = await ApiClient.GetMovieView(_sessionManager.CurrentUser.Id, cancellationSource.Token);

                _moviesView = view;

                LoadSpotlightViewModel(view);
                LoadAllMoviesViewModel(view);
                LoadMiniSpotlightsViewModel(view);
                LoadLatestMoviesViewModel(view);
                LoadLatestTrailersViewModel(view);
            }
            catch (Exception ex)
            {
                _logger.ErrorException("Error getting movie view", ex);
                PresentationManager.ShowDefaultErrorMessage();
            }
            finally
            {
                PresentationManager.HideLoadingAnimation();
                DisposeMainViewCancellationTokenSource(false);
            }
        }

        private void LoadLatestMoviesViewModel(MoviesView view)
        {
            LatestMoviesViewModel = new ItemListViewModel(GetLatestMoviesAsync, PresentationManager, _imageManager, ApiClient, _navService, _playbackManager, _logger, _serverEvents)
            {
                ImageDisplayWidth = _posterTileWidth,
                ImageDisplayHeightGenerator = v => _posterTileHeight,
                DisplayNameGenerator = HomePageViewModel.GetDisplayName,
                PreferredImageTypesGenerator = vm => new[] { ImageType.Primary, ImageType.Backdrop, ImageType.Thumb, },
                EnableBackdropsForCurrentItem = false,
                ListType = "LatestMovies"
            };

            OnPropertyChanged("LatestMoviesViewModel");

            ShowLatestMovies = view.LatestMovies.Count > 0;
        }

        private void LoadLatestTrailersViewModel(MoviesView view)
        {
            LatestTrailersViewModel = new ItemListViewModel(GetLatestTrailersAsync, PresentationManager, _imageManager, ApiClient, _navService, _playbackManager, _logger, _serverEvents)
            {
                ImageDisplayWidth = _posterTileWidth,
                ImageDisplayHeightGenerator = v => _posterTileHeight,
                DisplayNameGenerator = HomePageViewModel.GetDisplayName,
                PreferredImageTypesGenerator = vm => new[] { ImageType.Primary },
                EnableBackdropsForCurrentItem = false
            };

            OnPropertyChanged("LatestTrailersViewModel");

            ShowLatestTrailers = view.LatestTrailers.Count > 0;
        }

        private Task<ItemsResult> GetLatestTrailersAsync(ItemListViewModel viewModel)
        {
            var result = new ItemsResult
            {
                Items = _moviesView.LatestTrailers.ToArray(),
                TotalRecordCount = _moviesView.LatestTrailers.Count
            };

            return Task.FromResult(result);
        }

        private Task<ItemsResult> GetLatestMoviesAsync(ItemListViewModel viewModel)
        {
            var result = new ItemsResult
            {
                Items = _moviesView.LatestMovies.ToArray(),
                TotalRecordCount = _moviesView.LatestMovies.Count
            };

            return Task.FromResult(result);
        }

        private void LoadMiniSpotlightsViewModel(MoviesView view)
        {
            Func<ItemListViewModel, Task<ItemsResult>> getItems = vm =>
            {
                var items = view.MiniSpotlights.Take(3).ToArray();

                return Task.FromResult(new ItemsResult
                {
                    TotalRecordCount = items.Length,
                    Items = items
                });
            };

            MiniSpotlightsViewModel = new ItemListViewModel(getItems, PresentationManager, _imageManager, ApiClient, _navService, _playbackManager, _logger, _serverEvents)
            {
                ImageDisplayWidth = TileWidth + (TilePadding / 4) - 1,
                ImageDisplayHeightGenerator = v => TileHeight,
                DisplayNameGenerator = HomePageViewModel.GetDisplayName,
                EnableBackdropsForCurrentItem = false,
                ImageStretch = Stretch.UniformToFill,
                PreferredImageTypesGenerator = vm => new[] { ImageType.Backdrop },
                DownloadImageAtExactSize = true
            };

            OnPropertyChanged("MiniSpotlightsViewModel");
        }

        private bool _showLatestMovies;
        public bool ShowLatestMovies
        {
            get { return _showLatestMovies; }

            set
            {
                var changed = _showLatestMovies != value;

                _showLatestMovies = value;

                if (changed)
                {
                    OnPropertyChanged("ShowLatestMovies");
                }
            }
        }

        private bool _showLatestTrailers;
        public bool ShowLatestTrailers
        {
            get { return _showLatestTrailers; }

            set
            {
                var changed = _showLatestTrailers != value;

                _showLatestTrailers = value;

                if (changed)
                {
                    OnPropertyChanged("ShowLatestTrailers");
                }
            }
        }

        private void LoadSpotlightViewModel(MoviesView view)
        {
            const ImageType imageType = ImageType.Backdrop;

            var tileWidth = TileWidth * 2 + TilePadding;
            var tileHeight = tileWidth * 9 / 16;

            BackdropItems = view.BackdropItems.ToArray();

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
            SpotlightViewModel.StartRotating(10000);
        }

        public void EnableActivePresentation()
        {
            SpotlightViewModel.StartRotating(10000);
        }
        public void DisableActivePresentation()
        {
            SpotlightViewModel.StopRotating();
        }

        private async Task NavigateToGenresInternal()
        {
            var item = await GetRootFolder();

            var displayPreferences = await PresentationManager.GetDisplayPreferences("MovieGenres", CancellationToken.None);

            var genres = await ApiClient.GetGenresAsync(new ItemsByNameQuery
            {
                IncludeItemTypes = new[] { "Movie" },
                SortBy = new[] { ItemSortBy.SortName },
                Recursive = true,
                UserId = _sessionManager.CurrentUser.Id
            });

            var indexOptions = genres.Items.Select(i => new TabItem
            {
                Name = i.Name,
                DisplayName = i.Name
            });

            var options = new ListPageConfig
            {
                IndexOptions = indexOptions.ToList(),
                PageTitle = "Movies",
                CustomItemQuery = GetMoviesByGenre
            };

            SetDefaults(options);

            var page = new FolderPage(item, displayPreferences, ApiClient, _imageManager, PresentationManager, _navService, _playbackManager, _logger, _serverEvents, options)
            {
                ViewType = ViewType.Movies
            };

            await _navService.Navigate(page);
        }

        private async Task NavigateToYearsInternal()
        {
            var item = await GetRootFolder();

            var displayPreferences = await PresentationManager.GetDisplayPreferences("MovieYears", CancellationToken.None);

            var yearIndex = await ApiClient.GetYearIndex(_sessionManager.CurrentUser.Id, new[] { "Movie" }, CancellationToken.None);

            var indexOptions = yearIndex.Where(i => !string.IsNullOrEmpty(i.Name)).OrderByDescending(i => i.Name).Select(i => new TabItem
            {
                Name = i.Name,
                DisplayName = i.Name
            });

            var options = new ListPageConfig
            {
                IndexOptions = indexOptions.ToList(),
                PageTitle = "Movies",
                CustomItemQuery = GetMoviesByYear
            };

            SetDefaults(options);

            options.DefaultViewType = ListViewTypes.PosterStrip;

            var page = new FolderPage(item, displayPreferences, ApiClient, _imageManager, PresentationManager, _navService, _playbackManager, _logger, _serverEvents, options)
            {
                ViewType = ViewType.Movies
            };

            await _navService.Navigate(page);
        }

        private void LoadAllMoviesViewModel(MoviesView view)
        {
            var images = view.MovieItems.Take(1).Select(i => ApiClient.GetImageUrl(i.Id, new ImageOptions
            {
                ImageType = i.ImageType,
                Tag = i.ImageTag,
                Width = Convert.ToInt32(TileWidth * 2),
                EnableImageEnhancers = false
            }));

            AllMoviesViewModel.AddImages(images);
        }

        public Task NavigateToMovies()
        {
            return NavigateToMoviesInternal("AllMovies");
        }

        private async Task NavigateToMoviesInternal(string indexValue)
        {
            var item = await GetRootFolder();

            var displayPreferences = await PresentationManager.GetDisplayPreferences("Movies", CancellationToken.None);

            var view = _moviesView ?? await ApiClient.GetMovieView(_sessionManager.CurrentUser.Id, CancellationToken.None);

            var tabs = new List<TabItem>();

            tabs.Add(new TabItem
            {
                DisplayName = "All Movies",
                Name = "AllMovies"
            });

            tabs.Add(new TabItem
            {
                DisplayName = "New Releases",
                Name = "NewReleases",
            });

            if (ShowComedy(view))
            {
                tabs.Add(new TabItem
                {
                    DisplayName = GetComedyViewName(),
                    Name = "Comedy",
                });
            }

            if (ShouldShowRomance(view))
            {
                tabs.Add(new TabItem
                {
                    DisplayName = "Date Night",
                    Name = "Romance",
                });
            }

            if (view.TrailerItems.Count > 0)
            {
                tabs.Add(new TabItem
                {
                    DisplayName = "Trailers",
                    Name = "Trailers",
                });
            }

            tabs.Add(new TabItem
            {
                DisplayName = "Favorites",
                Name = "FavoriteMovies"
            });

            if (view.BoxSetItems.Count > 0)
            {
                tabs.Add(new TabItem
                {
                    DisplayName = "Box Sets",
                    Name = "BoxSets",
                });
            }

            if (view.FamilyMovies.Count > 0)
            {
                tabs.Add(new TabItem
                {
                    DisplayName = "Family",
                    Name = "Family",
                });
            }

            tabs.Add(new TabItem
            {
                DisplayName = "Popular",
                Name = "TopRated",
            });

            tabs.Add(new TabItem
            {
                DisplayName = "Critically Acclaimed",
                Name = "TopCriticRated",
            });

            if (view.HDItems.Count > 0)
            {
                tabs.Add(new TabItem
                {
                    DisplayName = "HD Movies",
                    Name = "HDMovies",
                });
            }

            if (view.ThreeDItems.Count > 0)
            {
                tabs.Add(new TabItem
                {
                    DisplayName = "3D Movies",
                    Name = "3DMovies",
                });
            }

            var options = new ListPageConfig
            {
                PageTitle = "Movies",
                CustomItemQuery = GetMovies,
                IndexOptions = tabs,
                IndexValue = indexValue
            };

            SetDefaults(options);

            var page = new FolderPage(item, displayPreferences, ApiClient, _imageManager, PresentationManager, _navService, _playbackManager, _logger, _serverEvents, options)
            {
                ViewType = ViewType.Movies
            };

            await _navService.Navigate(page);
        }

        private Task<ItemsResult> GetMovies(ItemListViewModel viewModel, DisplayPreferences displayPreferences)
        {
            var query = new ItemQuery
            {
                Fields = FolderPage.QueryFields,

                UserId = _sessionManager.CurrentUser.Id,

                IncludeItemTypes = new[] { "Movie" },

                SortBy = !String.IsNullOrEmpty(displayPreferences.SortBy)
                             ? new[] { displayPreferences.SortBy }
                             : new[] { ItemSortBy.SortName },

                SortOrder = displayPreferences.SortOrder,

                Recursive = true
            };

            var indexOption = viewModel.CurrentIndexOption == null ? string.Empty : viewModel.CurrentIndexOption.Name;

            if (string.Equals(indexOption, "TopRated"))
            {
                query.MinCommunityRating = ApiClientExtensions.TopMovieCommunityRating;

                query.SortBy = new[] { ItemSortBy.SortName };
                query.SortOrder = SortOrder.Ascending;
            }
            else if (string.Equals(indexOption, "NewReleases"))
            {
                query.SortBy = new[] { ItemSortBy.PremiereDate };
                query.SortOrder = SortOrder.Descending;
                query.Limit = 100;
            }
            else if (string.Equals(indexOption, "HDMovies"))
            {
                query.IsHD = true;

                query.SortBy = new[] { ItemSortBy.SortName };
                query.SortOrder = SortOrder.Ascending;
            }
            else if (string.Equals(indexOption, "3DMovies"))
            {
                query.Is3D = true;

                query.SortBy = new[] { ItemSortBy.SortName };
                query.SortOrder = SortOrder.Ascending;
            }
            else if (string.Equals(indexOption, "Trailers"))
            {
                query.IncludeItemTypes = new[] { "Trailer" };

                query.SortBy = new[] { ItemSortBy.SortName };
                query.SortOrder = SortOrder.Ascending;
            }
            else if (string.Equals(indexOption, "BoxSets"))
            {
                query.IncludeItemTypes = new[] { "BoxSet" };

                query.SortBy = new[] { ItemSortBy.SortName };
                query.SortOrder = SortOrder.Ascending;
            }
            else if (string.Equals(indexOption, "FavoriteMovies"))
            {
                query.Filters = new[] { ItemFilter.IsFavorite };

                query.SortBy = new[] { ItemSortBy.SortName };
                query.SortOrder = SortOrder.Ascending;
            }
            else if (string.Equals(indexOption, "TopCriticRated"))
            {
                query.MinCriticRating = 95;

                query.SortBy = new[] { ItemSortBy.SortName };
                query.SortOrder = SortOrder.Ascending;
            }
            else if (string.Equals(indexOption, "Family"))
            {
                query.Genres = new[] { "Family" };

                query.SortBy = new[] { ItemSortBy.SortName };
                query.SortOrder = SortOrder.Ascending;
            }
            else if (string.Equals(indexOption, "Comedy"))
            {
                query.Genres = new[] { ApiClientExtensions.ComedyGenre };

                query.SortBy = new[] { ItemSortBy.SortName };
                query.SortOrder = SortOrder.Ascending;
            }
            else if (string.Equals(indexOption, "Romance"))
            {
                query.Genres = new[] { ApiClientExtensions.RomanceGenre };

                query.SortBy = new[] { ItemSortBy.SortName };
                query.SortOrder = SortOrder.Ascending;
            }
            //else if (string.Equals(indexOption, "ShowsInProgress"))
            //{
            //    query.Ids = viewModel.CurrentIndexOption.TabType.Split(',');

            //    query.SortBy = new[] { ItemSortBy.SortName };
            //    query.SortOrder = SortOrder.Ascending;
            //}
            else if (indexOption.StartsWith("Genre:"))
            {
                query.Genres = new[] { indexOption.Split(':').Last() };

                query.SortBy = new[] { ItemSortBy.SortName };
                query.SortOrder = SortOrder.Ascending;
            }
            else if (indexOption.StartsWith("Studio:"))
            {
                query.Studios = new[] { indexOption.Split(':').Last() };

                query.SortBy = new[] { ItemSortBy.SortName };
                query.SortOrder = SortOrder.Ascending;
            }

            return ApiClient.GetItemsAsync(query);
        }

        public bool ShouldShowRomance(MoviesView view)
        {
            var now = DateTime.Now;

            if (now.DayOfWeek == DayOfWeek.Friday)
            {
                return view.RomanceItems.Count > 0 && now.Hour >= 15;
            }
            if (now.DayOfWeek == DayOfWeek.Saturday)
            {
                return view.RomanceItems.Count > 0 && (now.Hour < 3 || now.Hour >= 15);
            }
            if (now.DayOfWeek == DayOfWeek.Sunday)
            {
                return view.RomanceItems.Count > 0 && now.Hour < 3;
            }
            return false;
        }

        private string GetComedyViewName()
        {
            var now = DateTime.Now;

            if (now.DayOfWeek == DayOfWeek.Thursday)
            {
                return "Comedy Night";
            }
            if (now.DayOfWeek == DayOfWeek.Sunday)
            {
                return "Sunday Funnies";
            }

            return "Comedy";
        }

        private bool ShowComedy(MoviesView view)
        {
            if (view.ComedyItems.Count == 0)
            {
                return false;
            }

            var now = DateTime.Now;

            if (now.DayOfWeek == DayOfWeek.Thursday)
            {
                return now.Hour >= 12;
            }
            if (now.DayOfWeek == DayOfWeek.Sunday)
            {
                return now.Hour >= 6;
            }
            return false;
        }

        private Task<ItemsResult> GetMoviesByGenre(ItemListViewModel viewModel, DisplayPreferences displayPreferences)
        {
            var query = new ItemQuery
            {
                Fields = FolderPage.QueryFields,

                UserId = _sessionManager.CurrentUser.Id,

                IncludeItemTypes = new[] { "Movie" },

                SortBy = !String.IsNullOrEmpty(displayPreferences.SortBy)
                             ? new[] { displayPreferences.SortBy }
                             : new[] { ItemSortBy.SortName },

                SortOrder = displayPreferences.SortOrder,

                Recursive = true
            };

            var indexOption = viewModel.CurrentIndexOption;

            if (indexOption != null)
            {
                query.Genres = new[] { indexOption.Name };
            }

            return ApiClient.GetItemsAsync(query);
        }

        private Task<ItemsResult> GetMoviesByYear(ItemListViewModel viewModel, DisplayPreferences displayPreferences)
        {
            var query = new ItemQuery
            {
                Fields = FolderPage.QueryFields,

                UserId = _sessionManager.CurrentUser.Id,

                IncludeItemTypes = new[] { "Movie" },

                SortBy = !String.IsNullOrEmpty(displayPreferences.SortBy)
                             ? new[] { displayPreferences.SortBy }
                             : new[] { ItemSortBy.SortName },

                SortOrder = displayPreferences.SortOrder,

                Recursive = true
            };

            var indexOption = viewModel.CurrentIndexOption;

            if (indexOption != null)
            {
                query.Years = new[] { int.Parse(indexOption.Name) };
            }

            return ApiClient.GetItemsAsync(query);
        }

        internal static Dictionary<string, string> GetMovieSortOptions()
        {
            var sortOptions = new Dictionary<string, string>();
            sortOptions["Name"] = ItemSortBy.SortName;

            sortOptions["Critic Rating"] = ItemSortBy.CriticRating;
            sortOptions["Date Added"] = ItemSortBy.DateCreated;
            sortOptions["IMDb Rating"] = ItemSortBy.CommunityRating;
            sortOptions["Parental Rating"] = ItemSortBy.OfficialRating;
            sortOptions["Release Date"] = ItemSortBy.PremiereDate;
            sortOptions["Runtime"] = ItemSortBy.Runtime;

            return sortOptions;
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

        public void Dispose()
        {
            if (LatestTrailersViewModel != null)
            {
                LatestTrailersViewModel.Dispose();
            }
            if (LatestMoviesViewModel != null)
            {
                LatestMoviesViewModel.Dispose();
            }
            if (MiniSpotlightsViewModel != null)
            {
                MiniSpotlightsViewModel.Dispose();
            }
            if (SpotlightViewModel != null)
            {
                SpotlightViewModel.Dispose();
            }
            if (GenresViewModel != null)
            {
                GenresViewModel.Dispose();
            }
            if (TrailersViewModel != null)
            {
                TrailersViewModel.Dispose();
            }
            if (AllMoviesViewModel != null)
            {
                AllMoviesViewModel.Dispose();
            }
            if (YearsViewModel != null)
            {
                YearsViewModel.Dispose();
            }
            DisposeMainViewCancellationTokenSource(true);
        }
    }
}
