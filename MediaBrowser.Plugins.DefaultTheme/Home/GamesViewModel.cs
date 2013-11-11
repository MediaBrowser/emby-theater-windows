using MediaBrowser.Model.ApiClient;
using MediaBrowser.Model.Dto;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.Games;
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
    public class GamesViewModel : BaseHomePageSectionViewModel, IHasActivePresentation
    {
        private readonly ISessionManager _sessionManager;
        private readonly IPlaybackManager _playbackManager;
        private readonly IImageManager _imageManager;
        private readonly INavigationService _navService;
        private readonly ILogger _logger;
        private readonly IServerEvents _serverEvents;

        public ImageViewerViewModel SpotlightViewModel { get; private set; }

        public ItemListViewModel GameSystemsViewModel { get; private set; }
        public ItemListViewModel MiniSpotlightsViewModel { get; private set; }
        public ItemListViewModel MiniSpotlightsViewModel2 { get; private set; }
        public GalleryViewModel GenresViewModel { get; private set; }
        public GalleryViewModel YearsViewModel { get; private set; }
        public GalleryViewModel MultiPlayerViewModel { get; private set; }
        public ItemListViewModel RecentlyPlayedViewModel { get; private set; }

        public GamesViewModel(IPresentationManager presentation, IImageManager imageManager, IApiClient apiClient, ISessionManager session, INavigationService nav, IPlaybackManager playback, ILogger logger, double tileWidth, double tileHeight, IServerEvents serverEvents)
            : base(presentation, apiClient)
        {
            _sessionManager = session;
            _playbackManager = playback;
            _imageManager = imageManager;
            _navService = nav;
            _logger = logger;
            _serverEvents = serverEvents;

            TileWidth = tileWidth;
            TileHeight = tileHeight;

            var spotlightTileWidth = TileWidth * 2 + TilePadding;
            var spotlightTileHeight = spotlightTileWidth * 9 / 16;

            SpotlightViewModel = new ImageViewerViewModel(_imageManager, new List<ImageViewerImage>())
            {
                Height = spotlightTileHeight,
                Width = spotlightTileWidth,
                CustomCommandAction = i => _navService.NavigateToItem(i.Item, ViewType.Games),
                ImageStretch = Stretch.UniformToFill
            };

            GameSystemsViewModel = new ItemListViewModel(GetGameSystems, presentation, imageManager, apiClient, nav, playback, logger, _serverEvents)
            {
                ImageDisplayWidth = TileWidth,
                ImageDisplayHeightGenerator = v => TileHeight,
                DisplayNameGenerator = HomePageViewModel.GetDisplayName,
                EnableBackdropsForCurrentItem = false,
                Context = ViewType.Games
            };

            const double tileScaleFactor = 12;

            RecentlyPlayedViewModel = new ItemListViewModel(GetRecentlyPlayedAsync, presentation, imageManager, apiClient, nav, playback, logger, _serverEvents)
            {
                ImageDisplayWidth = TileWidth * tileScaleFactor / 16,
                ImageDisplayHeightGenerator = v => TileHeight,
                DisplayNameGenerator = HomePageViewModel.GetDisplayName,
                EnableBackdropsForCurrentItem = false,
                ImageStretch = Stretch.UniformToFill,
                Context = ViewType.Games
            };

            GenresViewModel = new GalleryViewModel(ApiClient, _imageManager, _navService)
            {
                GalleryHeight = TileHeight,
                GalleryWidth = TileWidth * tileScaleFactor / 16,
                CustomCommandAction = () => NavigateWithLoading(NavigateToGenresInternal)
            };

            YearsViewModel = new GalleryViewModel(ApiClient, _imageManager, _navService)
            {
                GalleryHeight = TileHeight,
                GalleryWidth = TileWidth * tileScaleFactor / 16,
                CustomCommandAction = () => NavigateWithLoading(NavigateToYearsInternal)
            };

            MultiPlayerViewModel = new GalleryViewModel(ApiClient, _imageManager, _navService)
            {
                GalleryHeight = TileHeight,
                GalleryWidth = TileWidth * tileScaleFactor / 16,
                CustomCommandAction = () => NavigateWithLoading(NavigateToMultiPlayerGamesInternal)
            };

            LoadViewModels();
        }

        public const int PosterWidth = 250;
        public const int PosterStripWidth = 320;
        public const int ThumbstripWidth = 600;

        public static void SetDefaults(ListPageConfig config, string gameSystem)
        {
            config.DefaultViewType = ListViewTypes.Poster;
            config.PosterImageWidth = PosterWidth;
            config.ThumbImageWidth = ThumbstripWidth;
            config.ListImageWidth = 160;
            config.PosterStripImageWidth = PosterStripWidth;

            if (string.Equals(gameSystem, GameSystem.Nintendo64, StringComparison.OrdinalIgnoreCase))
            {
                config.PosterImageWidth = 400;
                config.PosterStripImageWidth = 480;
            }
            else if (string.Equals(gameSystem, GameSystem.SuperNintendo, StringComparison.OrdinalIgnoreCase))
            {
                config.PosterImageWidth = 400;
                config.PosterStripImageWidth = 480;
            }
            else if (string.Equals(gameSystem, GameSystem.SegaSaturn, StringComparison.OrdinalIgnoreCase))
            {
                config.PosterImageWidth = 200;
                config.PosterStripImageWidth = 250;
            }
            else if (string.Equals(gameSystem, GameSystem.SegaCD, StringComparison.OrdinalIgnoreCase))
            {
                config.PosterImageWidth = 200;
                config.PosterStripImageWidth = 250;
            }
            else if (string.Equals(gameSystem, GameSystem.SonyPlaystation, StringComparison.OrdinalIgnoreCase))
            {
                config.PosterImageWidth = 300;
                config.PosterStripImageWidth = 380;
            }
            else if (string.Equals(gameSystem, GameSystem.SegaDreamcast, StringComparison.OrdinalIgnoreCase))
            {
                config.PosterImageWidth = 300;
                config.PosterStripImageWidth = 380;
            }
        }
        
        private async void LoadViewModels()
        {
            PresentationManager.ShowLoadingAnimation();

            var cancellationSource = _mainViewCancellationTokenSource = new CancellationTokenSource();

            try
            {
                var view = await ApiClient.GetGamesView(_sessionManager.CurrentUser.Id, cancellationSource.Token);

                LoadSpotlightViewModel(view);
                LoadGenresViewModel(view);
                LoadYearsViewModel(view);
                LoadMultiPlayerViewModel(view);

                LoadMiniSpotlightsViewModel(view);
                LoadMiniSpotlightsViewModel2(view);
            }
            catch (Exception ex)
            {
                _logger.ErrorException("Error getting games view", ex);
                PresentationManager.ShowDefaultErrorMessage();
            }
            finally
            {
                PresentationManager.HideLoadingAnimation();
                DisposeMainViewCancellationTokenSource(false);
            }
        }

        private void LoadMiniSpotlightsViewModel(GamesView view)
        {
            Func<ItemListViewModel, Task<ItemsResult>> getItems = vm =>
            {
                var items = view.MiniSpotlights.Take(2).ToArray();

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

        private void LoadMiniSpotlightsViewModel2(GamesView view)
        {
            Func<ItemListViewModel, Task<ItemsResult>> getItems = vm =>
            {
                var items = view.MiniSpotlights.Skip(2).Take(3).ToArray();

                return Task.FromResult(new ItemsResult
                {
                    TotalRecordCount = items.Length,
                    Items = items
                });
            };

            MiniSpotlightsViewModel2 = new ItemListViewModel(getItems, PresentationManager, _imageManager, ApiClient, _navService, _playbackManager, _logger, _serverEvents)
            {
                ImageDisplayWidth = TileWidth,
                ImageDisplayHeightGenerator = v => TileHeight,
                DisplayNameGenerator = HomePageViewModel.GetDisplayName,
                EnableBackdropsForCurrentItem = false,
                ImageStretch = Stretch.UniformToFill,
                PreferredImageTypesGenerator = vm => new[] { ImageType.Backdrop },
                DownloadImageAtExactSize = true
            };

            OnPropertyChanged("MiniSpotlightsViewModel2");
        }

        private bool _showMultiPlayer;
        public bool ShowMultiPlayer
        {
            get { return _showMultiPlayer; }

            set
            {
                var changed = _showMultiPlayer != value;

                _showMultiPlayer = value;

                if (changed)
                {
                    OnPropertyChanged("ShowMultiPlayer");
                }
            }
        }

        private bool _showRecentlyPlayed;
        public bool ShowRecentlyPlayed
        {
            get { return _showRecentlyPlayed; }

            set
            {
                var changed = _showRecentlyPlayed != value;

                _showRecentlyPlayed = value;

                if (changed)
                {
                    OnPropertyChanged("ShowRecentlyPlayed");
                }
            }
        }

        private void LoadSpotlightViewModel(GamesView view)
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

        private void LoadGenresViewModel(GamesView view)
        {
        }

        private void LoadYearsViewModel(GamesView view)
        {
        }

        private void LoadMultiPlayerViewModel(GamesView view)
        {
            ShowMultiPlayer = view.MultiPlayerItems.Count > 0;

            var images = view.MultiPlayerItems.Take(1).Select(i => ApiClient.GetImageUrl(i.Id, new ImageOptions
            {
                ImageType = i.ImageType,
                Tag = i.ImageTag,
                Width = Convert.ToInt32(TileWidth * 2),
                EnableImageEnhancers = false
            }));

            MultiPlayerViewModel.AddImages(images);
        }

        private async Task NavigateToMultiPlayerGamesInternal()
        {
            var item = await GetRootFolder();

            var displayPreferences = await PresentationManager.GetDisplayPreferences("MultiPlayerGames", CancellationToken.None);

            var playerIndex = await ApiClient.GetGamePlayerIndex(_sessionManager.CurrentUser.Id, CancellationToken.None);

            var indexOptions = playerIndex.Where(i => !string.IsNullOrEmpty(i.Name) && int.Parse(i.Name) > 1).Select(i => new TabItem
            {
                Name = i.Name,
                DisplayName = i.Name + " Player (" + i.ItemCount + ")"
            });

            var options = new ListPageConfig
            {
                IndexOptions = indexOptions.ToList(),
                PageTitle = "Games",
                CustomItemQuery = GetMultiPlayerGames
            };

            SetDefaults(options, null);

            var page = new FolderPage(item, displayPreferences, ApiClient, _imageManager, PresentationManager, _navService, _playbackManager, _logger, _serverEvents, options)
            {
                ViewType = ViewType.Games
            };

            await _navService.Navigate(page);
        }

        private Task<ItemsResult> GetMultiPlayerGames(ItemListViewModel viewModel, DisplayPreferences displayPreferences)
        {
            var query = new ItemQuery
            {
                Fields = FolderPage.QueryFields,

                UserId = _sessionManager.CurrentUser.Id,

                IncludeItemTypes = new[] { "Game" },

                SortBy = !String.IsNullOrEmpty(displayPreferences.SortBy)
                             ? new[] { displayPreferences.SortBy }
                             : new[] { ItemSortBy.SortName },

                SortOrder = displayPreferences.SortOrder,

                MinPlayers = 2,

                Recursive = true
            };

            var indexOption = viewModel.CurrentIndexOption;

            if (indexOption != null)
            {
                query.MinPlayers = query.MaxPlayers = int.Parse(indexOption.Name);
            }

            return ApiClient.GetItemsAsync(query);
        }

        private async Task NavigateToYearsInternal()
        {
            var item = await GetRootFolder();

            var displayPreferences = await PresentationManager.GetDisplayPreferences("GameYears", CancellationToken.None);

            var yearIndex = await ApiClient.GetYearIndex(_sessionManager.CurrentUser.Id, new[] { "Game" }, CancellationToken.None);

            var indexOptions = yearIndex.Where(i => !string.IsNullOrEmpty(i.Name)).Select(i => new TabItem
            {
                Name = i.Name,
                DisplayName = i.Name + " (" + i.ItemCount + ")"
            });

            var options = new ListPageConfig
            {
                IndexOptions = indexOptions.ToList(),
                PageTitle = "Games",
                CustomItemQuery = GetGamesByYear
            };

            SetDefaults(options, null);

            options.DefaultViewType = ListViewTypes.PosterStrip;

            var page = new FolderPage(item, displayPreferences, ApiClient, _imageManager, PresentationManager, _navService, _playbackManager, _logger, _serverEvents, options)
            {
                ViewType = ViewType.Games
            };

            await _navService.Navigate(page);
        }

        private Task<ItemsResult> GetGamesByYear(ItemListViewModel viewModel, DisplayPreferences displayPreferences)
        {
            var query = new ItemQuery
            {
                Fields = FolderPage.QueryFields,

                UserId = _sessionManager.CurrentUser.Id,

                IncludeItemTypes = new[] { "Game" },

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

        private Task<ItemsResult> GetGameSystems(ItemListViewModel viewModel)
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

                SortBy = new[] { ItemSortBy.SortName },

                IncludeItemTypes = new[] { "GameSystem" },

                Recursive = true
            };

            return ApiClient.GetItemsAsync(query);
        }

        private async Task<ItemsResult> GetRecentlyPlayedAsync(ItemListViewModel viewModel)
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

                Filters = new[] { ItemFilter.IsPlayed },

                IncludeItemTypes = new[] { "Game" },

                Recursive = true,

                Limit = 6
            };

            var result = await ApiClient.GetItemsAsync(query);

            ShowRecentlyPlayed = result.Items.Length > 0;

            return result;
        }

        private async Task NavigateToGenresInternal()
        {
            var item = await GetRootFolder();

            var displayPreferences = await PresentationManager.GetDisplayPreferences("GameGenres", CancellationToken.None);

            var genres = await ApiClient.GetGameGenresAsync(new ItemsByNameQuery
            {
                IncludeItemTypes = new[] { "Game" },
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
                PageTitle = "Games",
                CustomItemQuery = GetGamesByGenre
            };

            SetDefaults(options, null);

            var page = new FolderPage(item, displayPreferences, ApiClient, _imageManager, PresentationManager, _navService, _playbackManager, _logger, _serverEvents, options)
            {
                ViewType = ViewType.Games
            };

            await _navService.Navigate(page);
        }

        private Task<ItemsResult> GetGamesByGenre(ItemListViewModel viewModel, DisplayPreferences displayPreferences)
        {
            var query = new ItemQuery
            {
                Fields = FolderPage.QueryFields,

                UserId = _sessionManager.CurrentUser.Id,

                IncludeItemTypes = new[] { "Game" },

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
            if (SpotlightViewModel != null)
            {
                SpotlightViewModel.Dispose();
            }
            if (GameSystemsViewModel != null)
            {
                GameSystemsViewModel.Dispose();
            }
            if (MiniSpotlightsViewModel != null)
            {
                MiniSpotlightsViewModel.Dispose();
            }
            if (MiniSpotlightsViewModel2 != null)
            {
                MiniSpotlightsViewModel2.Dispose();
            }
            if (GenresViewModel != null)
            {
                GenresViewModel.Dispose();
            }
            if (YearsViewModel != null)
            {
                YearsViewModel.Dispose();
            }
            if (MultiPlayerViewModel != null)
            {
                MultiPlayerViewModel.Dispose();
            }
            if (RecentlyPlayedViewModel != null)
            {
                RecentlyPlayedViewModel.Dispose();
            }
            DisposeMainViewCancellationTokenSource(true);
        }
    }
}
