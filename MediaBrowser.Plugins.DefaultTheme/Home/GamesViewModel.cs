using System.Windows;
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
        public GalleryViewModel GenresViewModel { get; private set; }
        public GalleryViewModel YearsViewModel { get; private set; }
        public GalleryViewModel MultiPlayerViewModel { get; private set; }
        public ItemListViewModel RecentlyPlayedViewModel { get; private set; }

        private GamesView _gamesView;

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

            var spotlightTileHeight = TileHeight * 2 + TileMargin * 2;
            var spotlightTileWidth = 16 * (spotlightTileHeight / 9) + 100;

            var lowerSpotlightWidth = spotlightTileWidth / 3 - (TileMargin * 1.5);

            SpotlightViewModel = new ImageViewerViewModel(_imageManager, new List<ImageViewerImage>())
            {
                Height = spotlightTileHeight,
                Width = spotlightTileWidth,
                CustomCommandAction = i => _navService.NavigateToItem(i.Item, ViewType.Games),
                ImageStretch = Stretch.UniformToFill
            };

            GenresViewModel = new GalleryViewModel(ApiClient, _imageManager, _navService)
            {
                GalleryHeight = TileHeight,
                GalleryWidth = lowerSpotlightWidth,
                CustomCommandAction = () => NavigateWithLoading(NavigateToGenresInternal)
            };

            YearsViewModel = new GalleryViewModel(ApiClient, _imageManager, _navService)
            {
                GalleryHeight = TileHeight,
                GalleryWidth = lowerSpotlightWidth,
                CustomCommandAction = () => NavigateWithLoading(NavigateToYearsInternal)
            };

            MultiPlayerViewModel = new GalleryViewModel(ApiClient, _imageManager, _navService)
            {
                GalleryHeight = TileHeight,
                GalleryWidth = lowerSpotlightWidth,
                CustomCommandAction = () => NavigateWithLoading(NavigateToMultiPlayerGamesInternal)
            };

            LoadViewModels();
        }

        public const int PosterWidth = 250;
        public const int PosterStripWidth = 320;
        public const int ThumbstripWidth = 500;

        public static void SetDefaults(ListPageConfig config, string gameSystem)
        {
            config.DefaultViewType = ListViewTypes.PosterStrip;
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

                _gamesView = view;

                LoadSpotlightViewModel(view);
                LoadGameSystemsViewModel(view);
                LoadMultiPlayerViewModel(view);
                LoadRecentlyPlayedViewModel(view);
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

        private Visibility _multiPlayerVisibility = Visibility.Collapsed;
        public Visibility MultiPlayerVisibility
        {
            get { return _multiPlayerVisibility; }

            set
            {
                var changed = _multiPlayerVisibility != value;

                _multiPlayerVisibility = value;

                if (changed)
                {
                    OnPropertyChanged("MultiPlayerVisibility");
                }
            }
        }

        private Visibility _recentlyPlayedVisibility = Visibility.Collapsed;
        public Visibility RecentlyPlayedVisibility
        {
            get { return _recentlyPlayedVisibility; }

            set
            {
                var changed = _recentlyPlayedVisibility != value;

                _recentlyPlayedVisibility = value;

                if (changed)
                {
                    OnPropertyChanged("RecentlyPlayedVisibility");
                }
            }
        }

        private void LoadSpotlightViewModel(GamesView view)
        {
            const ImageType imageType = ImageType.Backdrop;

            var tileWidth = TileWidth * 2 + TileMargin;
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

        private void LoadGameSystemsViewModel(GamesView view)
        {
            GameSystemsViewModel = new ItemListViewModel(GetGameSystems, PresentationManager, _imageManager, ApiClient, _navService, _playbackManager, _logger, _serverEvents)
            {
                ImageDisplayWidth = TileWidth,
                ImageDisplayHeightGenerator = v => TileHeight,
                DisplayNameGenerator = HomePageViewModel.GetDisplayName,
                EnableBackdropsForCurrentItem = false,
                Context = ViewType.Games,
                EnableServerImageEnhancers = false,

                OnItemCreated = vm =>
                {
                    vm.DisplayNameVisibility = Visibility.Visible;
                }
            };

            OnPropertyChanged("GameSystemsViewModel");
        }

        private void LoadRecentlyPlayedViewModel(GamesView view)
        {
            RecentlyPlayedViewModel = new ItemListViewModel(GetRecentlyPlayedAsync, PresentationManager, _imageManager, ApiClient, _navService, _playbackManager, _logger, _serverEvents)
            {
                ImageDisplayWidth = TileWidth * 13 / 16,
                ImageDisplayHeightGenerator = v => TileHeight,
                DisplayNameGenerator = HomePageViewModel.GetDisplayName,
                EnableBackdropsForCurrentItem = false,
                ImageStretch = Stretch.UniformToFill,
                Context = ViewType.Games,
                EnableServerImageEnhancers = false,

                OnItemCreated = vm =>
                {
                    vm.DisplayNameVisibility = Visibility.Visible;
                }
            };

            OnPropertyChanged("RecentlyPlayedViewModel");
        }

        private void LoadMultiPlayerViewModel(GamesView view)
        {
            MultiPlayerVisibility = view.MultiPlayerItems.Count > 0 ? Visibility.Visible : Visibility.Collapsed;

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

            var indexOptions = yearIndex.Where(i => !string.IsNullOrEmpty(i.Name)).OrderByDescending(i => i.Name).Select(i => new TabItem
            {
                Name = i.Name,
                DisplayName = i.Name
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
            var result = new ItemsResult
            {
                Items = _gamesView.GameSystems.ToArray(),
                TotalRecordCount = _gamesView.GameSystems.Count
            };

            return Task.FromResult(result);
        }

        private Task<ItemsResult> GetRecentlyPlayedAsync(ItemListViewModel viewModel)
        {
            var result = new ItemsResult
            {
                Items = _gamesView.RecentlyPlayedGames.ToArray(),
                TotalRecordCount = _gamesView.RecentlyPlayedGames.Count
            };

            RecentlyPlayedVisibility = result.Items.Length > 0 ? Visibility.Visible : Visibility.Collapsed;

            return Task.FromResult(result);
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
