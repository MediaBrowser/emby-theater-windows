using MediaBrowser.Model.ApiClient;
using MediaBrowser.Model.Dto;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.Logging;
using MediaBrowser.Model.Querying;
using MediaBrowser.Theater.Interfaces.Navigation;
using MediaBrowser.Theater.Interfaces.Playback;
using MediaBrowser.Theater.Interfaces.Presentation;
using MediaBrowser.Theater.Interfaces.Session;
using MediaBrowser.Theater.Presentation.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

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

        private const double TileWidth = 336;
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

            try
            {
                var itemCounts = await _apiClient.GetItemCountsAsync(new ItemCountsQuery
                {
                    UserId = _sessionManager.CurrentUser.Id
                });

                if (itemCounts.MovieCount > 0)
                {
                    views.Add(new TabItem
                    {
                        Name = "movies",
                        DisplayName = "Movies"
                    });
                }

                if (itemCounts.SeriesCount > 0 || itemCounts.EpisodeCount > 0)
                {
                    views.Add(new TabItem
                    {
                        Name = "tv",
                        DisplayName = "TV"
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
                        DisplayName = "Games"
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.ErrorException("Error getting item counts", ex);
            }

            //views.Add(new TabItem
            //{
            //    Name = "favorites",
            //    DisplayName = "Favorites"
            //});

            if (_presentationManager.GetApps(_sessionManager.CurrentUser).Any())
            {
                views.Add(new TabItem
                {
                    Name = "apps",
                    DisplayName = "Apps"
                });
            }

            views.Add(new TabItem
            {
                Name = "media collections",
                DisplayName = "Folders"
            });

            return views;
        }

        public void EnableActivePresentation()
        {
            var content = ContentViewModel as IHasActivePresentation;

            if (content != null)
            {
                content.EnableActivePresentation();
            }
        }
        public void DisableActivePresentation()
        {
            var content = ContentViewModel as IHasActivePresentation;

            if (content != null)
            {
                content.DisableActivePresentation();
            }
        }

        internal static string GetDisplayName(BaseItemDto item)
        {
            var name = item.Name;

            if (item.IsType("Episode"))
            {
                name = item.SeriesName;

                if (item.IndexNumber.HasValue && item.ParentIndexNumber.HasValue)
                {
                    name = name + " " + string.Format("S{0}, E{1}", item.ParentIndexNumber.Value, item.IndexNumber.Value);
                }

            }

            return name;
        }

        protected override object GetContentViewModel(string section)
        {
            CurrentItem = null;

            if (string.Equals(section, "apps"))
            {
                return new AppListViewModel(_presentationManager, _sessionManager, _logger);
            }
            if (string.Equals(section, "media collections"))
            {
                var vm = new ItemListViewModel(GetMediaCollectionsAsync, _presentationManager, _imageManager, _apiClient, _nav, _playbackManager, _logger, _serverEvents)
                {
                    ImageDisplayWidth = 480,
                    ImageDisplayHeightGenerator = v => 270,
                    DisplayNameGenerator = GetDisplayName,

                    OnItemCreated = v =>
                    {
                        v.DisplayNameVisibility = Visibility.Visible;
                    }
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
                var tvViewModel = GetTvViewModel();
                tvViewModel.CurrentItemChanged += SectionViewModel_CurrentItemChanged;
                return tvViewModel;
            }
            if (string.Equals(section, "movies"))
            {
                var moviesViewModel = GetMoviesViewModel();
                moviesViewModel.CurrentItemChanged += SectionViewModel_CurrentItemChanged;
                return moviesViewModel;
            }

            return null;
        }

        private TvViewModel GetTvViewModel()
        {
            return new TvViewModel(_presentationManager, _imageManager, _apiClient, _sessionManager, _nav, _playbackManager, _logger, TileWidth, TileHeight, _serverEvents);
        }

        private MoviesViewModel GetMoviesViewModel()
        {
            return new MoviesViewModel(_presentationManager, _imageManager, _apiClient, _sessionManager, _nav,
                                       _playbackManager, _logger, TileWidth, TileHeight, _serverEvents);
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

        private Task NavigateToAllShowsInternal()
        {
            var vm = ContentViewModel as TvViewModel;

            if (vm == null)
            {
                vm = GetTvViewModel();
            }

            return vm.NavigateToAllShows();
        }

        private Task NavigateToAllMoviesInternal()
        {
            var vm = ContentViewModel as MoviesViewModel;

            if (vm == null)
            {
                vm = GetMoviesViewModel();
            }

            return vm.NavigateToMovies();
        }

        private ItemViewModel _currentItem;
        public ItemViewModel CurrentItem
        {
            get { return _currentItem; }
            private set
            {
                _currentItem = value;
                OnPropertyChanged("CurrentItem");
            }
        }
        private void SectionViewModel_CurrentItemChanged(BaseHomePageSectionViewModel sender, EventArgs e)
        {
            CurrentItem = sender.CurrentItem;
        }
    }
}
