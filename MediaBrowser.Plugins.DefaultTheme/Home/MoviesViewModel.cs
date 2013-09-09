using MediaBrowser.Model.ApiClient;
using MediaBrowser.Model.Dto;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.Logging;
using MediaBrowser.Model.Querying;
using MediaBrowser.Plugins.DefaultTheme.Controls;
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
using System.Threading.Tasks;

namespace MediaBrowser.Plugins.DefaultTheme.Home
{
    public class MoviesViewModel : BaseHomePageSectionViewModel, IDisposable
    {
        private readonly ISessionManager _sessionManager;
        private readonly IImageManager _imageManager;
        private readonly INavigationService _navService;

        public ItemListViewModel ResumeViewModel { get; private set; }
        public ItemListViewModel TrailersViewModel { get; private set; }
        public ItemListViewModel LatestMoviesViewModel { get; private set; }

        public ImageViewerViewModel SpotlightViewModel { get; private set; }

        public MoviesViewModel(IPresentationManager presentation, IImageManager imageManager, IApiClient apiClient, ISessionManager session, INavigationService nav, IPlaybackManager playback, ILogger logger, double tileWidth, double tileHeight)
            : base(presentation, apiClient)
        {
            _sessionManager = session;
            _imageManager = imageManager;
            _navService = nav;

            TileWidth = tileWidth;
            TileHeight = tileHeight;

            ResumeViewModel = new ItemListViewModel(GetResumeablesAsync, presentation, imageManager, apiClient, session, nav, playback, logger)
            {
                ImageDisplayWidth = TileWidth,
                ImageDisplayHeightGenerator = v => TileHeight,
                DisplayNameGenerator = MultiItemTile.GetDisplayName,
                PreferredImageTypesGenerator = vm => new[] { ImageType.Backdrop, ImageType.Thumb, ImageType.Primary },
                EnableBackdropsForCurrentItem = false
            };
            ResumeViewModel.PropertyChanged += ResumeViewModel_PropertyChanged;

            var trailerTileHeight = (TileHeight * 2) + TilePadding / 2;
            var trailerTileWidth = trailerTileHeight * 2 / 3;

            TrailersViewModel = new ItemListViewModel(GetLatestTrailersAsync, presentation, imageManager, apiClient, session, nav, playback, logger)
            {
                ImageDisplayWidth = trailerTileWidth,
                ImageDisplayHeightGenerator = v => trailerTileHeight,
                DisplayNameGenerator = MultiItemTile.GetDisplayName,
                PreferredImageTypesGenerator = vm => new[] { ImageType.Primary },
                EnableBackdropsForCurrentItem = false
            };
            TrailersViewModel.PropertyChanged += TrailersViewModel_PropertyChanged;

            LatestMoviesViewModel = new ItemListViewModel(GetLatestMoviesAsync, presentation, imageManager, apiClient, session, nav, playback, logger)
            {
                ImageDisplayWidth = TileWidth,
                ImageDisplayHeightGenerator = v => TileHeight,
                DisplayNameGenerator = MultiItemTile.GetDisplayName,
                PreferredImageTypesGenerator = vm => new[] { ImageType.Backdrop, ImageType.Thumb, ImageType.Primary },
                EnableBackdropsForCurrentItem = false
            };
            LatestMoviesViewModel.PropertyChanged += LatestMoviesViewModel_PropertyChanged;
            
            LoadSpotlightViewModel();
        }

        void LatestMoviesViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            ShowLatestMovies = LatestMoviesViewModel.ItemCount > 0;
        }

        void TrailersViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            ShowTrailers = TrailersViewModel.ItemCount > 0;
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

        private async void LoadSpotlightViewModel()
        {
            const ImageType imageType = ImageType.Backdrop;

            var tileWidth = TileWidth * 2 + TilePadding;
            var tileHeight = tileWidth * 9 / 16;

            SpotlightViewModel = new ImageViewerViewModel(_imageManager, new List<ImageViewerImage>())
            {
                Height = tileHeight,
                Width = tileWidth
            };

            var itemsResult = await ApiClient.GetItemsAsync(new ItemQuery
            {
                UserId = _sessionManager.CurrentUser.Id,

                SortBy = new[] { ItemSortBy.Random },

                IncludeItemTypes = new[] { "Movie" },

                ImageTypes = new[] { imageType },

                Limit = 30,

                Recursive = true
            });

            BackdropItems = itemsResult.Items;

            var images = itemsResult.Items.OrderBy(i => Guid.NewGuid()).Select(i => new ImageViewerImage
            {
                Url = ApiClient.GetImageUrl(i, new ImageOptions
                {
                    Height = Convert.ToInt32(tileHeight),
                    ImageType = imageType

                }),

                Caption = i.Name,
                Item = i

            }).ToList();

            SpotlightViewModel.CustomCommandAction = i => _navService.NavigateToItem(i.Item, ViewType.Movies);

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

                Limit = 6,

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

                Limit = 6,

                Recursive = true
            };

            return ApiClient.GetItemsAsync(query);
        }

        public void Dispose()
        {
            if (TrailersViewModel != null)
            {
                TrailersViewModel.Dispose();
            }
            if (ResumeViewModel != null)
            {
                ResumeViewModel.Dispose();
            }
        }
    }
}
