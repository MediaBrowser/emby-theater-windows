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
using System.Windows.Media;

namespace MediaBrowser.Plugins.DefaultTheme.Home
{
    public class MoviesViewModel : BaseHomePageSectionViewModel, IDisposable
    {
        private readonly ISessionManager _sessionManager;
        private readonly IImageManager _imageManager;
        private readonly INavigationService _navService;
        private readonly IPlaybackManager _playbackManager;
        private readonly ILogger _logger;


        public ItemListViewModel ResumeViewModel { get; private set; }
        public ItemListViewModel LatestTrailersViewModel { get; private set; }
        public ItemListViewModel LatestMoviesViewModel { get; private set; }

        public ImageViewerViewModel SpotlightViewModel { get; private set; }

        public GalleryViewModel AllMoviesViewModel { get; private set; }
        public GalleryViewModel ActorsViewModel { get; private set; }
        public GalleryViewModel BoxsetsViewModel { get; private set; }
        public GalleryViewModel TrailersViewModel { get; private set; }
        public GalleryViewModel HDMoviesViewModel { get; private set; }
        public GalleryViewModel ThreeDMoviesViewModel { get; private set; }
        public GalleryViewModel FamilyMoviesViewModel { get; private set; }

        public GalleryViewModel RomanticMoviesViewModel { get; private set; }

        public MoviesViewModel(IPresentationManager presentation, IImageManager imageManager, IApiClient apiClient, ISessionManager session, INavigationService nav, IPlaybackManager playback, ILogger logger, double tileWidth, double tileHeight)
            : base(presentation, apiClient)
        {
            _sessionManager = session;
            _imageManager = imageManager;
            _navService = nav;
            _playbackManager = playback;
            _logger = logger;

            TileWidth = tileWidth;
            TileHeight = tileHeight;

            ResumeViewModel = new ItemListViewModel(GetResumeablesAsync, presentation, imageManager, apiClient, session, nav, playback, logger)
            {
                ImageDisplayWidth = TileWidth,
                ImageDisplayHeightGenerator = v => TileHeight,
                DisplayNameGenerator = HomePageViewModel.GetDisplayName,
                PreferredImageTypesGenerator = vm => new[] { ImageType.Backdrop, ImageType.Thumb, ImageType.Primary },
                EnableBackdropsForCurrentItem = false
            };
            ResumeViewModel.PropertyChanged += ResumeViewModel_PropertyChanged;

            var trailerTileHeight = (TileHeight * 2) + TilePadding / 2;
            var trailerTileWidth = trailerTileHeight * 2 / 3;

            LatestTrailersViewModel = new ItemListViewModel(GetLatestTrailersAsync, presentation, imageManager, apiClient, session, nav, playback, logger)
            {
                ImageDisplayWidth = trailerTileWidth,
                ImageDisplayHeightGenerator = v => trailerTileHeight,
                DisplayNameGenerator = HomePageViewModel.GetDisplayName,
                PreferredImageTypesGenerator = vm => new[] { ImageType.Primary },
                EnableBackdropsForCurrentItem = false
            };
            LatestTrailersViewModel.PropertyChanged += TrailersViewModel_PropertyChanged;

            LatestMoviesViewModel = new ItemListViewModel(GetLatestMoviesAsync, presentation, imageManager, apiClient, session, nav, playback, logger)
            {
                ImageDisplayWidth = TileWidth,
                ImageDisplayHeightGenerator = v => TileHeight,
                DisplayNameGenerator = HomePageViewModel.GetDisplayName,
                PreferredImageTypesGenerator = vm => new[] { ImageType.Backdrop, ImageType.Thumb, ImageType.Primary },
                EnableBackdropsForCurrentItem = false
            };
            LatestMoviesViewModel.PropertyChanged += LatestMoviesViewModel_PropertyChanged;

            ActorsViewModel = new GalleryViewModel(ApiClient, _imageManager, _navService)
            {
                GalleryHeight = TileHeight,
                GalleryWidth = TileWidth * 10 / 16,
                CustomCommandAction = () => NavigateWithLoading(NavigateToActorsInternal)
            };

            AllMoviesViewModel = new GalleryViewModel(ApiClient, _imageManager, _navService)
            {
                GalleryHeight = TileHeight,
                GalleryWidth = TileWidth * 10 / 16,
                CustomCommandAction = () => NavigateWithLoading(NavigateToAllMoviesInternal)
            };

            BoxsetsViewModel = new GalleryViewModel(ApiClient, _imageManager, _navService)
            {
                GalleryHeight = TileHeight,
                GalleryWidth = TileWidth * 10 / 16,
                CustomCommandAction = () => NavigateWithLoading(NavigateToBoxsetsInternal)
            };

            TrailersViewModel = new GalleryViewModel(ApiClient, _imageManager, _navService)
            {
                GalleryHeight = TileHeight,
                GalleryWidth = TileWidth * 10 / 16,
                CustomCommandAction = () => NavigateWithLoading(NavigateToTrailersInternal)
            };

            HDMoviesViewModel = new GalleryViewModel(ApiClient, _imageManager, _navService)
            {
                GalleryHeight = TileHeight,
                GalleryWidth = TileWidth * 10 / 16,
                CustomCommandAction = () => NavigateWithLoading(NavigateToHDMoviesInternal)
            };

            FamilyMoviesViewModel = new GalleryViewModel(ApiClient, _imageManager, _navService)
            {
                GalleryHeight = TileHeight,
                GalleryWidth = TileWidth * 10 / 16,
                CustomCommandAction = () => NavigateWithLoading(NavigateToFamilyMoviesInternal)
            };

            ThreeDMoviesViewModel = new GalleryViewModel(ApiClient, _imageManager, _navService)
            {
                GalleryHeight = TileHeight,
                GalleryWidth = TileWidth * 10 / 16,
                CustomCommandAction = () => NavigateWithLoading(NavigateTo3DMoviesInternal)
            };

            RomanticMoviesViewModel = new GalleryViewModel(ApiClient, _imageManager, _navService)
            {
                GalleryHeight = TileHeight,
                GalleryWidth = TileWidth * 10 / 16,
                CustomCommandAction = () => NavigateWithLoading(NavigateToRomanticMoviesInternal)
            };

            var spotlightTileWidth = TileWidth * 2 + TilePadding;
            var spotlightTileHeight = spotlightTileWidth * 9 / 16;

            SpotlightViewModel = new ImageViewerViewModel(_imageManager, new List<ImageViewerImage>())
            {
                Height = spotlightTileHeight,
                Width = spotlightTileWidth,
                CustomCommandAction = i => _navService.NavigateToItem(i.Item, ViewType.Movies)
            };

            LoadViewModels();
        }

        private async void LoadViewModels()
        {
            PresentationManager.ShowLoadingAnimation();

            try
            {
                var view = await ApiClient.GetMovieView(_sessionManager.CurrentUser.Id, CancellationToken.None);

                LoadSpotlightViewModel(view);
                LoadBoxsetsViewModel(view);
                LoadTrailersViewModel(view);
                LoadAllMoviesViewModel(view);
                LoadHDMoviesViewModel(view);
                LoadFamilyMoviesViewModel(view);
                Load3DMoviesViewModel(view);
                LoadRomanticMoviesViewModel(view);
                LoadActorsViewModel(view);
            }
            catch (Exception ex)
            {
                _logger.ErrorException("Error getting movie view", ex);
                PresentationManager.ShowDefaultErrorMessage();
            }
            finally
            {
                PresentationManager.HideLoadingAnimation();
            }
        }

        void LatestMoviesViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            ShowLatestMovies = LatestMoviesViewModel.ItemCount > 0;
        }

        void TrailersViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            ShowLatestTrailers = LatestTrailersViewModel.ItemCount > 0;
        }

        void ResumeViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            ShowResume = ResumeViewModel.ItemCount > 0;
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

        private bool _showTrailers;
        public bool ShowTrailers
        {
            get { return _showTrailers; }

            set
            {
                var changed = _showTrailers != value;

                _showTrailers = value;

                if (changed)
                {
                    OnPropertyChanged("ShowTrailers");
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

        private bool _showBoxSets;
        public bool ShowBoxSets
        {
            get { return _showBoxSets; }

            set
            {
                var changed = _showBoxSets != value;

                _showBoxSets = value;

                if (changed)
                {
                    OnPropertyChanged("ShowBoxSets");
                }
            }
        }

        private bool _show3DMovies;
        public bool Show3DMovies
        {
            get { return _show3DMovies; }

            set
            {
                var changed = _show3DMovies != value;

                _show3DMovies = value;

                if (changed)
                {
                    OnPropertyChanged("Show3DMovies");
                }
            }
        }

        private bool _showRomanticMovies;
        public bool ShowRomanticMovies
        {
            get { return _showRomanticMovies; }

            set
            {
                var changed = _showRomanticMovies != value;

                _showRomanticMovies = value;

                if (changed)
                {
                    OnPropertyChanged("ShowRomanticMovies");
                }
            }
        }

        private bool _showHdMovies;
        public bool ShowHDMovies
        {
            get { return _showHdMovies; }

            set
            {
                var changed = _showHdMovies != value;

                _showHdMovies = value;

                if (changed)
                {
                    OnPropertyChanged("ShowHDMovies");
                }
            }
        }

        private bool _showFamilyMovies;
        public bool ShowFamilyMovies
        {
            get { return _showFamilyMovies; }

            set
            {
                var changed = _showFamilyMovies != value;

                _showFamilyMovies = value;

                if (changed)
                {
                    OnPropertyChanged("ShowFamilyMovies");
                }
            }
        }

        private void LoadSpotlightViewModel(MoviesView view)
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

                IncludeItemTypes = new[] { "Movie" },

                Filters = new[] { ItemFilter.IsResumable },

                Limit = 6,

                Recursive = true
            };

            return ApiClient.GetItemsAsync(query);
        }

        private Task<ItemsResult> GetLatestTrailersAsync()
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

                SortBy = new[] { ItemSortBy.DateCreated },

                SortOrder = SortOrder.Descending,

                IncludeItemTypes = new[] { "Trailer" },

                Filters = new[] { ItemFilter.IsUnplayed },

                Limit = 4,

                Recursive = true
            };

            return ApiClient.GetItemsAsync(query);
        }

        private Task<ItemsResult> GetLatestMoviesAsync()
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

                SortBy = new[] { ItemSortBy.DateCreated },

                SortOrder = SortOrder.Descending,

                IncludeItemTypes = new[] { "Movie" },

                Filters = new[] { ItemFilter.IsUnplayed },

                Limit = 4,

                Recursive = true
            };

            return ApiClient.GetItemsAsync(query);
        }

        private void LoadActorsViewModel(MoviesView view)
        {
            var images = view.PeopleItems.Take(1).Select(i => ApiClient.GetPersonImageUrl(i.Name, new ImageOptions
            {
                ImageType = i.ImageType,
                Tag = i.ImageTag,
                Height = Convert.ToInt32(TileWidth * 2),
                EnableImageEnhancers = false
            }));

            ActorsViewModel.AddImages(images);
        }

        private async Task NavigateToActorsInternal()
        {
            var item = await ApiClient.GetRootFolderAsync(_sessionManager.CurrentUser.Id);

            var displayPreferences = await PresentationManager.GetDisplayPreferences("TVActors", CancellationToken.None);

            var page = new FolderPage(item, displayPreferences, ApiClient, _imageManager, _sessionManager,
                                      PresentationManager, _navService, _playbackManager, _logger);

            page.CustomPageTitle = "TV | Actors";

            page.ViewType = ViewType.Movies;
            page.CustomItemQuery = GetAllActors;

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

        private void LoadHDMoviesViewModel(MoviesView view)
        {
            ShowHDMovies = view.HDItems.Length > 0 && view.HDMoviePercentage > 10 && view.HDMoviePercentage < 90;

            var images = view.HDItems.Take(1).Select(i => ApiClient.GetImageUrl(i.Id, new ImageOptions
            {
                ImageType = i.ImageType,
                Tag = i.ImageTag,
                Width = Convert.ToInt32(TileWidth * 2),
                EnableImageEnhancers = false
            }));

            HDMoviesViewModel.AddImages(images);
        }

        private void LoadRomanticMoviesViewModel(MoviesView view)
        {
            var now = DateTime.Now;

            if (now.DayOfWeek == DayOfWeek.Friday)
            {
                ShowRomanticMovies = view.RomanceItems.Length > 0 && now.Hour >= 15;
            }
            else if (now.DayOfWeek == DayOfWeek.Saturday)
            {
                ShowRomanticMovies = view.RomanceItems.Length > 0 && (now.Hour < 3 || now.Hour >= 15);
            }
            else if (now.DayOfWeek == DayOfWeek.Sunday)
            {
                ShowRomanticMovies = view.RomanceItems.Length > 0 && now.Hour < 3;
            }
            else
            {
                ShowRomanticMovies = false;
            }

            var images = view.RomanceItems.Take(1).Select(i => ApiClient.GetImageUrl(i.Id, new ImageOptions
            {
                ImageType = i.ImageType,
                Tag = i.ImageTag,
                Width = Convert.ToInt32(TileWidth * 2),
                EnableImageEnhancers = false
            }));

            RomanticMoviesViewModel.AddImages(images);
        }

        private void LoadFamilyMoviesViewModel(MoviesView view)
        {
            ShowFamilyMovies = view.FamilyMovies.Length > 0 && view.FamilyMoviePercentage > 10 && view.FamilyMoviePercentage < 90;

            var images = view.FamilyMovies.Take(1).Select(i => ApiClient.GetImageUrl(i.Id, new ImageOptions
            {
                ImageType = i.ImageType,
                Tag = i.ImageTag,
                Width = Convert.ToInt32(TileWidth * 2),
                EnableImageEnhancers = false
            }));

            FamilyMoviesViewModel.AddImages(images);
        }

        private void Load3DMoviesViewModel(MoviesView view)
        {
            Show3DMovies = view.ThreeDItems.Length > 0;

            var images = view.ThreeDItems.Take(1).Select(i => ApiClient.GetImageUrl(i.Id, new ImageOptions
            {
                ImageType = i.ImageType,
                Tag = i.ImageTag,
                Width = Convert.ToInt32(TileWidth * 2),
                EnableImageEnhancers = false
            }));

            ThreeDMoviesViewModel.AddImages(images);
        }

        private void LoadBoxsetsViewModel(MoviesView view)
        {
            ShowBoxSets = view.BoxSetItems.Length > 0;

            var images = view.BoxSetItems.Take(1).Select(i => ApiClient.GetImageUrl(i.Id, new ImageOptions
            {
                ImageType = i.ImageType,
                Tag = i.ImageTag,
                Width = Convert.ToInt32(TileWidth * 2),
                EnableImageEnhancers = false
            }));

            BoxsetsViewModel.AddImages(images);
        }

        private void LoadTrailersViewModel(MoviesView view)
        {
            ShowTrailers = view.TrailerItems.Length > 0;

            var images = view.TrailerItems.Take(1).Select(i => ApiClient.GetImageUrl(i.Id, new ImageOptions
            {
                ImageType = i.ImageType,
                Tag = i.ImageTag,
                Width = Convert.ToInt32(TileWidth * 2),
                EnableImageEnhancers = false
            }));

            TrailersViewModel.AddImages(images);
        }

        private async Task NavigateToTrailersInternal()
        {
            var item = await ApiClient.GetRootFolderAsync(_sessionManager.CurrentUser.Id);

            var displayPreferences = await PresentationManager.GetDisplayPreferences("Trailers", CancellationToken.None);

            var page = new FolderPage(item, displayPreferences, ApiClient, _imageManager, _sessionManager,
                                      PresentationManager, _navService, _playbackManager, _logger);

            page.CustomPageTitle = "Trailers";

            page.ViewType = ViewType.Movies;
            page.CustomItemQuery = GetTrailers;

            await _navService.Navigate(page);
        }

        private Task<ItemsResult> GetTrailers(DisplayPreferences displayPreferences)
        {
            var query = new ItemQuery
            {
                Fields = FolderPage.QueryFields,

                UserId = _sessionManager.CurrentUser.Id,

                IncludeItemTypes = new[] { "Trailer" },

                SortBy = !String.IsNullOrEmpty(displayPreferences.SortBy)
                             ? new[] { displayPreferences.SortBy }
                             : new[] { ItemSortBy.SortName },

                SortOrder = displayPreferences.SortOrder,

                Recursive = true
            };

            return ApiClient.GetItemsAsync(query);
        }

        private async Task NavigateToBoxsetsInternal()
        {
            var item = await ApiClient.GetRootFolderAsync(_sessionManager.CurrentUser.Id);

            var displayPreferences = await PresentationManager.GetDisplayPreferences("Boxsets", CancellationToken.None);

            var page = new FolderPage(item, displayPreferences, ApiClient, _imageManager, _sessionManager,
                                      PresentationManager, _navService, _playbackManager, _logger);

            page.CustomPageTitle = "Box Sets";

            page.ViewType = ViewType.Movies;
            page.CustomItemQuery = GetBoxSets;

            await _navService.Navigate(page);
        }

        private Task<ItemsResult> GetBoxSets(DisplayPreferences displayPreferences)
        {
            var query = new ItemQuery
            {
                Fields = FolderPage.QueryFields,

                UserId = _sessionManager.CurrentUser.Id,

                IncludeItemTypes = new[] { "BoxSet" },

                SortBy = !String.IsNullOrEmpty(displayPreferences.SortBy)
                             ? new[] { displayPreferences.SortBy }
                             : new[] { ItemSortBy.SortName },

                SortOrder = displayPreferences.SortOrder,

                Recursive = true
            };

            return ApiClient.GetItemsAsync(query);
        }

        private async Task NavigateToAllMoviesInternal()
        {
            var item = await ApiClient.GetRootFolderAsync(_sessionManager.CurrentUser.Id);

            var displayPreferences = await PresentationManager.GetDisplayPreferences("AllMovies", CancellationToken.None);

            var page = new FolderPage(item, displayPreferences, ApiClient, _imageManager, _sessionManager,
                                      PresentationManager, _navService, _playbackManager, _logger);

            page.CustomPageTitle = "Movies";

            page.ViewType = ViewType.Movies;
            page.CustomItemQuery = GetAllMovies;

            await _navService.Navigate(page);
        }

        private async Task NavigateToRomanticMoviesInternal()
        {
            var item = await ApiClient.GetRootFolderAsync(_sessionManager.CurrentUser.Id);

            var displayPreferences = await PresentationManager.GetDisplayPreferences("RomanticMovies", CancellationToken.None);

            var page = new FolderPage(item, displayPreferences, ApiClient, _imageManager, _sessionManager,
                                      PresentationManager, _navService, _playbackManager, _logger);

            page.CustomPageTitle = "Date Night";

            page.ViewType = ViewType.Movies;
            page.CustomItemQuery = GetRomanticMovies;

            await _navService.Navigate(page);
        }

        private Task<ItemsResult> GetRomanticMovies(DisplayPreferences displayPreferences)
        {
            var query = new ItemQuery
            {
                Fields = FolderPage.QueryFields,

                UserId = _sessionManager.CurrentUser.Id,

                IncludeItemTypes = new[] { "Movie" },

                Genres = new[] { ApiClientExtensions.RomanceGenre },

                SortBy = !String.IsNullOrEmpty(displayPreferences.SortBy)
                             ? new[] { displayPreferences.SortBy }
                             : new[] { ItemSortBy.SortName },

                SortOrder = displayPreferences.SortOrder,

                Recursive = true
            };

            return ApiClient.GetItemsAsync(query);
        }

        private Task<ItemsResult> GetAllMovies(DisplayPreferences displayPreferences)
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

            return ApiClient.GetItemsAsync(query);
        }

        private async Task NavigateToHDMoviesInternal()
        {
            var item = await ApiClient.GetRootFolderAsync(_sessionManager.CurrentUser.Id);

            var displayPreferences = await PresentationManager.GetDisplayPreferences("HDMovies", CancellationToken.None);

            var page = new FolderPage(item, displayPreferences, ApiClient, _imageManager, _sessionManager,
                                      PresentationManager, _navService, _playbackManager, _logger);

            page.CustomPageTitle = "HD Movies";

            page.ViewType = ViewType.Movies;
            page.CustomItemQuery = GetHDMovies;

            await _navService.Navigate(page);
        }

        private Task<ItemsResult> GetHDMovies(DisplayPreferences displayPreferences)
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

                IsHD = true,

                Recursive = true
            };

            return ApiClient.GetItemsAsync(query);
        }

        private async Task NavigateToFamilyMoviesInternal()
        {
            var item = await ApiClient.GetRootFolderAsync(_sessionManager.CurrentUser.Id);

            var displayPreferences = await PresentationManager.GetDisplayPreferences("FamilyMovies", CancellationToken.None);

            var page = new FolderPage(item, displayPreferences, ApiClient, _imageManager, _sessionManager,
                                      PresentationManager, _navService, _playbackManager, _logger);

            page.CustomPageTitle = "Family Movies";

            page.ViewType = ViewType.Movies;
            page.CustomItemQuery = GetFamilyMovies;

            await _navService.Navigate(page);
        }

        private Task<ItemsResult> GetFamilyMovies(DisplayPreferences displayPreferences)
        {
            var query = new ItemQuery
            {
                Fields = FolderPage.QueryFields,

                UserId = _sessionManager.CurrentUser.Id,

                IncludeItemTypes = new[] { "Movie" },

                Genres = new[] { ApiClientExtensions.FamilyGenre },

                SortBy = !String.IsNullOrEmpty(displayPreferences.SortBy)
                             ? new[] { displayPreferences.SortBy }
                             : new[] { ItemSortBy.SortName },

                SortOrder = displayPreferences.SortOrder,

                Recursive = true
            };

            return ApiClient.GetItemsAsync(query);
        }

        private async Task NavigateTo3DMoviesInternal()
        {
            var item = await ApiClient.GetRootFolderAsync(_sessionManager.CurrentUser.Id);

            var displayPreferences = await PresentationManager.GetDisplayPreferences("3DMovies", CancellationToken.None);

            var page = new FolderPage(item, displayPreferences, ApiClient, _imageManager, _sessionManager,
                                      PresentationManager, _navService, _playbackManager, _logger);

            page.CustomPageTitle = "3D Movies";

            page.ViewType = ViewType.Movies;
            page.CustomItemQuery = Get3DMovies;

            await _navService.Navigate(page);
        }

        private Task<ItemsResult> Get3DMovies(DisplayPreferences displayPreferences)
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

                Recursive = true,

                Is3D = true
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

                IncludeItemTypes = new[] { "Movie", "Trailer" },

                SortBy = !String.IsNullOrEmpty(displayPreferences.SortBy)
                             ? new[] { displayPreferences.SortBy }
                             : new[] { ItemSortBy.SortName },

                SortOrder = displayPreferences.SortOrder,

                Recursive = true
            };

            return ApiClient.GetPeopleAsync(query);
        }

        public void Dispose()
        {
            if (LatestTrailersViewModel != null)
            {
                LatestTrailersViewModel.Dispose();
            }
            if (ResumeViewModel != null)
            {
                ResumeViewModel.Dispose();
            }
        }
    }
}
