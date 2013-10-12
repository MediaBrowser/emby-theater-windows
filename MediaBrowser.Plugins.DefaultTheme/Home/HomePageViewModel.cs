using System.Threading;
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
using System.Threading.Tasks;

namespace MediaBrowser.Plugins.DefaultTheme.Home
{
    public class HomePageViewModel : TabbedViewModel
    {
        private readonly IPresentationManager _presentationManager;
        private readonly IApiClient _apiClient;
        private readonly ISessionManager _sessionManager;
        private readonly ILogger _logger;
        private readonly IImageManager _imageManager;
        private readonly INavigationService _nav;
        private readonly IPlaybackManager _playbackManager;
        private readonly IServerEvents _serverEvents;

        private const double TileWidth = 368;
        private const double TileHeight = TileWidth * 9 / 16;

        public HomePageViewModel(IPresentationManager presentationManager, IApiClient apiClient, ISessionManager sessionManager, ILogger logger, IImageManager imageManager, INavigationService nav, IPlaybackManager playbackManager, IServerEvents serverEvents)
        {
            _presentationManager = presentationManager;
            _apiClient = apiClient;
            _sessionManager = sessionManager;
            _logger = logger;
            _imageManager = imageManager;
            _nav = nav;
            _playbackManager = playbackManager;
            _serverEvents = serverEvents;
        }

        protected override async Task<IEnumerable<TabItem>> GetSections()
        {
            var views = new List<TabItem>
                {
                    //_sessionManager.CurrentUser.Name.ToLower()
                };

            //views.Add(new TabItem
            //{
            //    Name = "start",
            //    DisplayName = "start"
            //});
            
            try
            {
                var itemCounts = await _apiClient.GetItemCountsAsync(_sessionManager.CurrentUser.Id);

                if (itemCounts.MovieCount > 0 || itemCounts.TrailerCount > 0)
                {
                    views.Add(new TabItem
                    {
                        Name = "movies",
                        DisplayName = "movies"
                    });
                }

                if (itemCounts.SeriesCount > 0 || itemCounts.EpisodeCount > 0)
                {
                    views.Add(new TabItem
                    {
                        Name = "tv",
                        DisplayName = "tv"
                    });
                }

                //if (itemCounts.SongCount > 0)
                //{
                //    views.Add("music");
                //}
                if (itemCounts.GameCount > 0)
                {
                    views.Add(new TabItem
                    {
                        Name = "games",
                        DisplayName = "games"
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.ErrorException("Error getting item counts", ex);
            }

            if (_presentationManager.GetApps(_sessionManager.CurrentUser).Any())
            {
                views.Add(new TabItem
                {
                    Name = "apps",
                    DisplayName = "apps"
                });
            }

            views.Add(new TabItem
            {
                Name = "media collections",
                DisplayName = "collections"
            });

            return views;
        }

        internal static string GetDisplayName(BaseItemDto item)
        {
            var name = item.Name;

            if (item.IsType("Episode"))
            {
                name = item.SeriesName;

                if (item.IndexNumber.HasValue && item.ParentIndexNumber.HasValue)
                {
                    name = name + ": " + string.Format("S{0}, Ep. {1}", item.ParentIndexNumber.Value, item.IndexNumber.Value);
                }

            }

            return name;
        }

        protected override BaseViewModel GetContentViewModel(string section)
        {
            if (string.Equals(section, "apps"))
            {
                return new AppListViewModel(_presentationManager, _sessionManager, _logger);
            }
            if (string.Equals(section, "media collections"))
            {
                var vm = new ItemListViewModel(GetMediaCollectionsAsync, _presentationManager, _imageManager, _apiClient, _nav, _playbackManager, _logger, _serverEvents)
                {
                    ImageDisplayWidth = TileWidth,
                    ImageDisplayHeightGenerator = v => TileHeight,
                    DisplayNameGenerator = GetDisplayName
                };

                return vm;

            }
            if (string.Equals(section, "games"))
            {
                return new GamesViewModel(_presentationManager, _imageManager, _apiClient, _sessionManager, _nav,
                                       _playbackManager, _logger, TileWidth, TileHeight, _serverEvents);
            }
            if (string.Equals(section, "tv"))
            {
                return new TvViewModel(_presentationManager, _imageManager, _apiClient, _sessionManager, _nav, _playbackManager, _logger, TileWidth, TileHeight, _serverEvents);
            }
            if (string.Equals(section, "movies"))
            {
                return new MoviesViewModel(_presentationManager, _imageManager, _apiClient, _sessionManager, _nav,
                                           _playbackManager, _logger, TileWidth, TileHeight, _serverEvents);
            }

            return new UserTabViewModel(_presentationManager, _imageManager, _apiClient, _sessionManager, _nav, _playbackManager, _logger, TileWidth, TileHeight, _serverEvents);
        }

        private Task<ItemsResult> GetMediaCollectionsAsync(ItemListViewModel viewModel)
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

                SortOrder = SortOrder.Ascending
            };

            return _apiClient.GetItemsAsync(query);
        }

        public void SetBackdrops()
        {
            var vm = ContentViewModel as BaseHomePageSectionViewModel;

            if (vm != null)
            {
                vm.SetBackdrops();
            }
            else
            {
                _presentationManager.ClearBackdrops();
            }
        }

        protected override void OnTabCommmand(TabItem tab)
        {
            if (tab != null)
            {
                if (string.Equals(tab.Name, "movies"))
                {
                    NavigateToAllMoviesInternal();
                }
                else if (string.Equals(tab.Name, "tv"))
                {
                    NavigateToAllShowsInternal();
                }
            }
        }

        private async Task NavigateToAllShowsInternal()
        {
            var item = await _apiClient.GetRootFolderAsync(_sessionManager.CurrentUser.Id);

            var displayPreferences = await _presentationManager.GetDisplayPreferences("Shows", CancellationToken.None);

            var page = new FolderPage(item, displayPreferences, _apiClient, _imageManager, _sessionManager,
                                      _presentationManager, _nav, _playbackManager, _logger, _serverEvents);

            page.SortOptions = TvViewModel.GetSeriesSortOptions();
            page.CustomPageTitle = "TV Shows";

            page.ViewType = ViewType.Tv;
            page.CustomItemQuery = GetAllShows;

            await _nav.Navigate(page);
        }

        private Task<ItemsResult> GetAllShows(ItemListViewModel viewModel, DisplayPreferences displayPreferences)
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

            return _apiClient.GetItemsAsync(query);
        }

        private async Task NavigateToAllMoviesInternal()
        {
            var item = await _apiClient.GetRootFolderAsync(_sessionManager.CurrentUser.Id);

            var displayPreferences = await _presentationManager.GetDisplayPreferences("Movies", CancellationToken.None);

            var page = new FolderPage(item, displayPreferences, _apiClient, _imageManager, _sessionManager,
                                      _presentationManager, _nav, _playbackManager, _logger, _serverEvents);

            page.SortOptions = MoviesViewModel.GetMovieSortOptions();
            page.CustomPageTitle = "Movies";

            page.ViewType = ViewType.Movies;
            page.CustomItemQuery = GetAllMovies;

            await _nav.Navigate(page);
        }

        private Task<ItemsResult> GetAllMovies(ItemListViewModel viewModel, DisplayPreferences displayPreferences)
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

            return _apiClient.GetItemsAsync(query);
        }
    }
}
