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
    public class GamesViewModel : BaseHomePageSectionViewModel
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
                EnableBackdropsForCurrentItem = false
            };

            const int tileScaleFactor = 12;

            RecentlyPlayedViewModel = new ItemListViewModel(GetRecentlyPlayedAsync, presentation, imageManager, apiClient, nav, playback, logger, _serverEvents)
            {
                ImageDisplayWidth = TileWidth * tileScaleFactor / 16,
                ImageDisplayHeightGenerator = v => TileHeight,
                DisplayNameGenerator = HomePageViewModel.GetDisplayName,
                EnableBackdropsForCurrentItem = false,
                ImageStretch = Stretch.UniformToFill
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

        private void LoadGenresViewModel(GamesView view)
        {
        }

        private void LoadYearsViewModel(GamesView view)
        {
        }

        private void LoadMultiPlayerViewModel(GamesView view)
        {
            ShowMultiPlayer = view.MultiPlayerItems.Length > 0;

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
            var item = await ApiClient.GetRootFolderAsync(_sessionManager.CurrentUser.Id);

            var displayPreferences = await PresentationManager.GetDisplayPreferences("MultiPlayerGames", CancellationToken.None);

            var playerIndex = await ApiClient.GetGamePlayerIndex(_sessionManager.CurrentUser.Id, CancellationToken.None);

            var indexOptions = playerIndex.Where(i => !string.IsNullOrEmpty(i.Name) && int.Parse(i.Name) > 1).Select(i => new TabItem
            {
                Name = i.Name,
                DisplayName = i.Name + " Player (" + i.ItemCount + ")"
            });

            var page = new FolderPage(item, displayPreferences, ApiClient, _imageManager, _sessionManager,
                                      PresentationManager, _navService, _playbackManager, _logger, indexOptions, _serverEvents);

            //page.SortOptions = GetSeriesSortOptions();
            page.CustomPageTitle = "Games | Multi-Player";

            page.ViewType = ViewType.Games;
            page.CustomItemQuery = GetMultiPlayerGames;

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
            var item = await ApiClient.GetRootFolderAsync(_sessionManager.CurrentUser.Id);

            var displayPreferences = await PresentationManager.GetDisplayPreferences("GameYears", CancellationToken.None);

            var yearIndex = await ApiClient.GetYearIndex(_sessionManager.CurrentUser.Id, new[] { "Game" }, CancellationToken.None);

            var indexOptions = yearIndex.Where(i => !string.IsNullOrEmpty(i.Name)).Select(i => new TabItem
            {
                Name = i.Name,
                DisplayName = i.Name + " (" + i.ItemCount + ")"
            });

            var page = new FolderPage(item, displayPreferences, ApiClient, _imageManager, _sessionManager,
                                      PresentationManager, _navService, _playbackManager, _logger, indexOptions, _serverEvents);

            //page.SortOptions = GetSeriesSortOptions();
            page.CustomPageTitle = "Games | Timeline";

            page.ViewType = ViewType.Games;
            page.CustomItemQuery = GetGamesByYear;

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
            var item = await ApiClient.GetRootFolderAsync(_sessionManager.CurrentUser.Id);

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
                DisplayName = i.Name + " (" + i.GameCount + ")"
            });

            var page = new FolderPage(item, displayPreferences, ApiClient, _imageManager, _sessionManager,
                                      PresentationManager, _navService, _playbackManager, _logger, indexOptions, _serverEvents);

            var sortOptions = new Dictionary<string, string>();

            page.SortOptions = sortOptions;
            page.CustomPageTitle = "Games | Genres";

            page.ViewType = ViewType.Games;
            page.CustomItemQuery = GetGamesByGenre;

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
            DisposeMainViewCancellationTokenSource(true);
        }
    }
}
