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
using System.Windows.Input;
using System.Windows.Media;

namespace MediaBrowser.Plugins.DefaultTheme.Home
{
    public class FavoritesViewModel : BaseHomePageSectionViewModel, IDisposable
    {
        private readonly ISessionManager _sessionManager;
        private readonly IPlaybackManager _playbackManager;
        private readonly IImageManager _imageManager;
        private readonly INavigationService _navService;
        private readonly ILogger _logger;
        private readonly IServerEvents _serverEvents;

        public ItemListViewModel MoviesViewModel { get; private set; }
        public ItemListViewModel SeriesViewModel { get; private set; }
        public ItemListViewModel EpisodesViewModel { get; private set; }
        public ItemListViewModel GamesViewModel { get; private set; }
        public ItemListViewModel AlbumsViewModel { get; private set; }
        public ItemListViewModel SongsViewModel { get; private set; }
        public ItemListViewModel ArtistsViewModel { get; private set; }
        public ItemListViewModel MiniSpotlightsViewModel { get; private set; }
        public ItemListViewModel MiniSpotlightsViewModel2 { get; private set; }

        public ImageViewerViewModel SpotlightViewModel { get; private set; }

        public ICommand NavigateToFavoriteMoviesCommand { get; set; }

        public FavoritesViewModel(IPresentationManager presentation, IImageManager imageManager, IApiClient apiClient, ISessionManager session, INavigationService nav, IPlaybackManager playback, ILogger logger, double tileWidth, double tileHeight, IServerEvents serverEvents)
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
                CustomCommandAction = i => _navService.NavigateToItem(i.Item, ViewType.Tv)
            };

            LoadViewModels();

            NavigateToFavoriteMoviesCommand = new RelayCommand(o => NavigateToFavorites("Movie"));
        }

        private async void LoadViewModels()
        {
            PresentationManager.ShowLoadingAnimation();

            try
            {
                var view = await ApiClient.GetFavoritesView(_sessionManager.CurrentUser.Id, CancellationToken.None);

                LoadSpotlightViewModel(view);
                LoadMoviesViewModel(view);
                LoadSeriesViewModel(view);
                LoadEpisodesViewModel(view);
                LoadGamesViewModel(view);
                LoadArtistsViewModel(view);
                LoadAlbumsViewModel(view);
                LoadSongsViewModel(view);
                LoadMiniSpotlightsViewModel(view);
                LoadMiniSpotlightsViewModel2(view);
            }
            catch (Exception ex)
            {
                _logger.ErrorException("Error getting home view", ex);
                PresentationManager.ShowDefaultErrorMessage();
            }
            finally
            {
                PresentationManager.HideLoadingAnimation();
            }
        }

        private void LoadMiniSpotlightsViewModel(FavoritesView view)
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

        private void LoadMiniSpotlightsViewModel2(FavoritesView view)
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

        private bool _showMovies;
        public bool ShowMovies
        {
            get { return _showMovies; }

            set
            {
                var changed = _showMovies != value;

                _showMovies = value;

                if (changed)
                {
                    OnPropertyChanged("ShowMovies");
                }
            }
        }

        private bool _showSeries;
        public bool ShowSeries
        {
            get { return _showSeries; }

            set
            {
                var changed = _showSeries != value;

                _showSeries = value;

                if (changed)
                {
                    OnPropertyChanged("ShowSeries");
                }
            }
        }

        private bool _showEpisodes;
        public bool ShowEpisodes
        {
            get { return _showEpisodes; }

            set
            {
                var changed = _showEpisodes != value;

                _showEpisodes = value;

                if (changed)
                {
                    OnPropertyChanged("ShowEpisodes");
                }
            }
        }

        private bool _showGames;
        public bool ShowGames
        {
            get { return _showGames; }

            set
            {
                var changed = _showGames != value;

                _showGames = value;

                if (changed)
                {
                    OnPropertyChanged("ShowGames");
                }
            }
        }

        private bool _showAlbums;
        public bool ShowAlbums
        {
            get { return _showAlbums; }

            set
            {
                var changed = _showAlbums != value;

                _showAlbums = value;

                if (changed)
                {
                    OnPropertyChanged("ShowAlbums");
                }
            }
        }

        private bool _showSongs;
        public bool ShowSongs
        {
            get { return _showSongs; }

            set
            {
                var changed = _showSongs != value;

                _showSongs = value;

                if (changed)
                {
                    OnPropertyChanged("ShowSongs");
                }
            }
        }

        private bool _showArtists;
        public bool ShowArtists
        {
            get { return _showArtists; }

            set
            {
                var changed = _showArtists != value;

                _showArtists = value;

                if (changed)
                {
                    OnPropertyChanged("ShowArtists");
                }
            }
        }

        private void LoadSpotlightViewModel(FavoritesView view)
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

        private void LoadMoviesViewModel(FavoritesView view)
        {
            var tileHeight = (TileHeight * 1.46) + TilePadding / 2;
            var tileWidth = tileHeight * 2 / 3;

            MoviesViewModel = new ItemListViewModel(vm => Task.FromResult(new ItemsResult
            {
                Items = view.Movies.ToArray(),
                TotalRecordCount = view.Movies.Count

            }), PresentationManager, _imageManager, ApiClient, _navService, _playbackManager, _logger, _serverEvents)
            {
                ImageDisplayWidth = tileWidth,
                ImageDisplayHeightGenerator = v => tileHeight,
                DisplayNameGenerator = HomePageViewModel.GetDisplayName,
                EnableBackdropsForCurrentItem = false,
                PreferredImageTypesGenerator = vm => new[] { ImageType.Primary },
                ShowTitle = false
            };

            OnPropertyChanged("MoviesViewModel");
            ShowMovies = view.Movies.Count > 0;
        }

        private void LoadSeriesViewModel(FavoritesView view)
        {
            var tileHeight = (TileHeight * 1.46) + TilePadding / 2;
            var tileWidth = tileHeight * 2 / 3;

            SeriesViewModel = new ItemListViewModel(vm => Task.FromResult(new ItemsResult
            {
                Items = view.Series.ToArray(),
                TotalRecordCount = view.Series.Count

            }), PresentationManager, _imageManager, ApiClient, _navService, _playbackManager, _logger, _serverEvents)
            {
                ImageDisplayWidth = tileWidth,
                ImageDisplayHeightGenerator = v => tileHeight,
                DisplayNameGenerator = HomePageViewModel.GetDisplayName,
                EnableBackdropsForCurrentItem = false,
                PreferredImageTypesGenerator = vm => new[] { ImageType.Primary },
                ShowTitle = false
            };

            OnPropertyChanged("SeriesViewModel");
            ShowSeries = view.Series.Count > 0;
        }

        private void LoadEpisodesViewModel(FavoritesView view)
        {
            EpisodesViewModel = new ItemListViewModel(vm => Task.FromResult(new ItemsResult
            {
                Items = view.Episodes.ToArray(),
                TotalRecordCount = view.Episodes.Count

            }), PresentationManager, _imageManager, ApiClient, _navService, _playbackManager, _logger, _serverEvents)
            {
                ImageDisplayWidth = TileWidth,
                ImageDisplayHeightGenerator = v => TileHeight,
                DisplayNameGenerator = HomePageViewModel.GetDisplayName,
                EnableBackdropsForCurrentItem = false,
                PreferredImageTypesGenerator = vm => new[] { ImageType.Primary }
            };

            OnPropertyChanged("EpisodesViewModel");
            ShowEpisodes = view.Episodes.Count > 0;
        }

        private void LoadGamesViewModel(FavoritesView view)
        {
            var tileHeight = (TileHeight * 1.46) + TilePadding / 2;
            var tileWidth = tileHeight * 2 / 3;

            GamesViewModel = new ItemListViewModel(vm => Task.FromResult(new ItemsResult
            {
                Items = view.Games.ToArray(),
                TotalRecordCount = view.Games.Count

            }), PresentationManager, _imageManager, ApiClient, _navService, _playbackManager, _logger, _serverEvents)
            {
                ImageDisplayWidth = tileWidth,
                ImageDisplayHeightGenerator = v => tileHeight,
                DisplayNameGenerator = HomePageViewModel.GetDisplayName,
                EnableBackdropsForCurrentItem = false,
                PreferredImageTypesGenerator = vm => new[] { ImageType.Primary },
                ImageStretch = Stretch.UniformToFill,
                DownloadImageAtExactSize = false,
                ShowTitle = false
            };

            OnPropertyChanged("GamesViewModel");
            ShowGames = view.Games.Count > 0;
        }

        private void LoadAlbumsViewModel(FavoritesView view)
        {
            var tileHeight = (TileHeight * 1.46) + TilePadding / 2;
            var tileWidth = tileHeight;

            AlbumsViewModel = new ItemListViewModel(vm => Task.FromResult(new ItemsResult
            {
                Items = view.Albums.ToArray(),
                TotalRecordCount = view.Albums.Count

            }), PresentationManager, _imageManager, ApiClient, _navService, _playbackManager, _logger, _serverEvents)
            {
                ImageDisplayWidth = tileWidth,
                ImageDisplayHeightGenerator = v => tileHeight,
                DisplayNameGenerator = HomePageViewModel.GetDisplayName,
                EnableBackdropsForCurrentItem = false,
                PreferredImageTypesGenerator = vm => new[] { ImageType.Primary },
                ImageStretch = Stretch.UniformToFill
            };

            OnPropertyChanged("AlbumsViewModel");
            ShowAlbums = view.Albums.Count > 0;
        }

        private void LoadArtistsViewModel(FavoritesView view)
        {
            var tileHeight = (TileHeight * 1.46) + TilePadding / 2;
            var tileWidth = tileHeight;

            ArtistsViewModel = new ItemListViewModel(vm => Task.FromResult(new ItemsResult
            {
                Items = view.Artists.ToArray(),
                TotalRecordCount = view.Artists.Count

            }), PresentationManager, _imageManager, ApiClient, _navService, _playbackManager, _logger, _serverEvents)
            {
                ImageDisplayWidth = tileWidth,
                ImageDisplayHeightGenerator = v => tileHeight,
                DisplayNameGenerator = HomePageViewModel.GetDisplayName,
                EnableBackdropsForCurrentItem = false,
                PreferredImageTypesGenerator = vm => new[] { ImageType.Primary },
                ImageStretch = Stretch.UniformToFill
            };

            OnPropertyChanged("ArtistsViewModel");
            ShowArtists = view.Artists.Count > 0;
        }

        private void LoadSongsViewModel(FavoritesView view)
        {
            var tileHeight = (TileHeight * 1.46) + TilePadding / 2;
            var tileWidth = tileHeight;

            SongsViewModel = new ItemListViewModel(vm => Task.FromResult(new ItemsResult
            {
                Items = view.Songs.ToArray(),
                TotalRecordCount = view.Songs.Count

            }), PresentationManager, _imageManager, ApiClient, _navService, _playbackManager, _logger, _serverEvents)
            {
                ImageDisplayWidth = tileWidth,
                ImageDisplayHeightGenerator = v => tileHeight,
                DisplayNameGenerator = HomePageViewModel.GetDisplayName,
                EnableBackdropsForCurrentItem = false,
                PreferredImageTypesGenerator = vm => new[] { ImageType.Primary },
                ImageStretch = Stretch.UniformToFill
            };

            OnPropertyChanged("SongsViewModel");
            ShowSongs = view.Songs.Count > 0;
        }

        private async void NavigateToFavorites(string type)
        {
            try
            {
                var itemCounts = await ApiClient.GetItemCountsAsync(new ItemCountsQuery
                {
                    UserId = _sessionManager.CurrentUser.Id,
                    IsFavorite = true
                });

                var item = await ApiClient.GetRootFolderAsync(_sessionManager.CurrentUser.Id);

                var displayPreferences = await PresentationManager.GetDisplayPreferences("Favorites", CancellationToken.None);

                var indexOptions = GetFavoriteTabs(itemCounts);

                var page = new FolderPage(item, displayPreferences, ApiClient, _imageManager, _sessionManager,
                                          PresentationManager, _navService, _playbackManager, _logger, indexOptions, _serverEvents);

                var sortOptions = new Dictionary<string, string>();

                page.SortOptions = sortOptions;
                page.CustomPageTitle = "Favorites";

                page.ViewType = ViewType.Folders;
                page.CustomItemQuery = GetFavoriteItems;

                await _navService.Navigate(page);
            }
            catch (Exception ex)
            {
                _logger.Error("Error navigating to favorites", ex);

                PresentationManager.ShowDefaultErrorMessage();
            }
        }

        private IEnumerable<TabItem> GetFavoriteTabs(ItemCounts counts)
        {
            var list = new List<TabItem>();

            if (counts.MovieCount > 0)
            {
                list.Add(new TabItem
                {
                    DisplayName = "Movies (" + counts.MovieCount + ")",
                    Name = "Movie"
                });
            }

            if (counts.BoxSetCount > 0)
            {
                list.Add(new TabItem
                {
                    DisplayName = "Box Sets (" + counts.BoxSetCount + ")",
                    Name = "BoxSet"
                });
            }

            if (counts.SeriesCount > 0)
            {
                list.Add(new TabItem
                {
                    DisplayName = "TV Shows (" + counts.SeriesCount + ")",
                    Name = "Series"
                });
            }

            if (counts.EpisodeCount > 0)
            {
                list.Add(new TabItem
                {
                    DisplayName = "Episodes (" + counts.EpisodeCount + ")",
                    Name = "Episode"
                });
            }

            if (counts.PersonCount > 0)
            {
                list.Add(new TabItem
                {
                    DisplayName = "People (" + counts.PersonCount + ")",
                    Name = "Person"
                });
            }

            if (counts.ArtistCount > 0)
            {
                list.Add(new TabItem
                {
                    DisplayName = "Artists (" + counts.ArtistCount + ")",
                    Name = "Artist"
                });
            }

            if (counts.AlbumCount > 0)
            {
                list.Add(new TabItem
                {
                    DisplayName = "Albums (" + counts.AlbumCount + ")",
                    Name = "MusicAlbum"
                });
            }

            if (counts.SongCount > 0)
            {
                list.Add(new TabItem
                {
                    DisplayName = "Songs (" + counts.SongCount + ")",
                    Name = "Audio"
                });
            }

            if (counts.MusicVideoCount > 0)
            {
                list.Add(new TabItem
                {
                    DisplayName = "Music Videos (" + counts.MusicVideoCount + ")",
                    Name = "MusicVideo"
                });
            }

            if (counts.GameCount > 0)
            {
                list.Add(new TabItem
                {
                    DisplayName = "Games (" + counts.GameCount + ")",
                    Name = "Game"
                });
            }

            if (counts.BookCount > 0)
            {
                list.Add(new TabItem
                {
                    DisplayName = "Books (" + counts.BookCount + ")",
                    Name = "Book"
                });
            }

            if (counts.AdultVideoCount > 0)
            {
                list.Add(new TabItem
                {
                    DisplayName = "Adult Videos (" + counts.AdultVideoCount + ")",
                    Name = "AdultVideo"
                });
            }
            
            return list;
        }

        private Task<ItemsResult> GetFavoriteItems(ItemListViewModel viewModel, DisplayPreferences displayPreferences)
        {
            var indexOption = viewModel.CurrentIndexOption;

            if (indexOption != null)
            {
                if (string.Equals(indexOption.Name, "Artist", StringComparison.OrdinalIgnoreCase))
                {
                    return GetFavoriteArtists(viewModel, displayPreferences);
                }
                if (string.Equals(indexOption.Name, "Person", StringComparison.OrdinalIgnoreCase))
                {
                    return GetFavoritePeople(viewModel, displayPreferences);
                }
            }

            var query = new ItemQuery
            {
                Fields = FolderPage.QueryFields,

                UserId = _sessionManager.CurrentUser.Id,

                SortBy = !String.IsNullOrEmpty(displayPreferences.SortBy)
                             ? new[] { displayPreferences.SortBy }
                             : new[] { ItemSortBy.SortName },

                SortOrder = displayPreferences.SortOrder,

                Recursive = true,

                Filters = new[] { ItemFilter.IsFavorite }
            };

            if (indexOption != null)
            {
                query.IncludeItemTypes = new[] { indexOption.Name };
            }

            return ApiClient.GetItemsAsync(query);
        }

        private Task<ItemsResult> GetFavoritePeople(ItemListViewModel viewModel, DisplayPreferences displayPreferences)
        {
            var query = new PersonsQuery
            {
                Fields = FolderPage.QueryFields,

                UserId = _sessionManager.CurrentUser.Id,

                SortBy = !String.IsNullOrEmpty(displayPreferences.SortBy)
                             ? new[] { displayPreferences.SortBy }
                             : new[] { ItemSortBy.SortName },

                SortOrder = displayPreferences.SortOrder,

                Recursive = true,

                Filters = new[] { ItemFilter.IsFavorite }
            };

            return ApiClient.GetPeopleAsync(query);
        }

        private Task<ItemsResult> GetFavoriteArtists(ItemListViewModel viewModel, DisplayPreferences displayPreferences)
        {
            var query = new ArtistsQuery
            {
                Fields = FolderPage.QueryFields,

                UserId = _sessionManager.CurrentUser.Id,

                SortBy = !String.IsNullOrEmpty(displayPreferences.SortBy)
                             ? new[] { displayPreferences.SortBy }
                             : new[] { ItemSortBy.SortName },

                SortOrder = displayPreferences.SortOrder,

                Recursive = true,

                Filters = new[] { ItemFilter.IsFavorite }
            };

            return ApiClient.GetArtistsAsync(query);
        }

        public void Dispose()
        {
            if (SpotlightViewModel != null)
            {
                SpotlightViewModel.Dispose();
            }
            if (MoviesViewModel != null)
            {
                MoviesViewModel.Dispose();
            }
            if (SeriesViewModel != null)
            {
                SeriesViewModel.Dispose();
            }
            if (EpisodesViewModel != null)
            {
                EpisodesViewModel.Dispose();
            }
            if (GamesViewModel != null)
            {
                GamesViewModel.Dispose();
            }
            if (AlbumsViewModel != null)
            {
                AlbumsViewModel.Dispose();
            }
            if (SongsViewModel != null)
            {
                SongsViewModel.Dispose();
            }
            if (ArtistsViewModel != null)
            {
                ArtistsViewModel.Dispose();
            }
            if (MiniSpotlightsViewModel != null)
            {
                MiniSpotlightsViewModel.Dispose();
            }
            if (MiniSpotlightsViewModel2 != null)
            {
                MiniSpotlightsViewModel2.Dispose();
            }
        }
    }
}
