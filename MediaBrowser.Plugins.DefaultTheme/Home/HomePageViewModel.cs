using System.Threading;
using MediaBrowser.Model.ApiClient;
using MediaBrowser.Model.Channels;
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
    public class HomePageViewModel : TabbedViewModel, IAcceptsPlayCommand
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
            var userViews = await _apiClient.GetUserViews(_sessionManager.CurrentUser.Id, CancellationToken.None);

            var views = userViews.Items.Select(i =>
            {
                // Mixed folder type.
                if (string.IsNullOrEmpty(i.CollectionType))
                {
                    return null;
                }

                var supportedViews = new List<string>
                {
                    CollectionType.Movies,
                    "Folders",
                    CollectionType.Channels,
                    CollectionType.Games,
                    CollectionType.TvShows,
                    CollectionType.Playlists,
                    CollectionType.LiveTv
                };

                if (!supportedViews.Contains(i.CollectionType, StringComparer.OrdinalIgnoreCase))
                {
                    return null;
                }

                return new TabItem
                {
                    DisplayName = i.Name,
                    Name = i.CollectionType,
                    Item = i
                };

            }).Where(i => i != null).ToList();

            if (_presentationManager.GetApps(_sessionManager.CurrentUser).Any())
            {
                views.Add(new TabItem
                {
                    Name = "apps",
                    DisplayName = "Apps"
                });
            }

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

            var tab = (TabItem)Sections.CurrentItem;
            
            if (string.Equals(section, "apps", StringComparison.OrdinalIgnoreCase))
            {
                return new AppListViewModel(_presentationManager, _sessionManager, _logger);
            }
            if (string.Equals(section, "playlists", StringComparison.OrdinalIgnoreCase))
            {
                // Eventually when people have enough playlists we'll need to do something different
                var vm = new ItemListViewModel(i => GetFolderItems(i, tab.Item.Id), _presentationManager, _imageManager, _apiClient, _nav, _playbackManager, _logger, _serverEvents)
                {
                    ImageDisplayWidth = 330,
                    ImageDisplayHeightGenerator = v => 330,
                    DisplayNameGenerator = GetDisplayName,

                    OnItemCreated = v =>
                    {
                        v.DisplayNameVisibility = Visibility.Visible;
                    }
                };

                return vm;

            }
            if (string.Equals(section, "folders", StringComparison.OrdinalIgnoreCase))
            {
                var vm = new ItemListViewModel(i => GetFolderItems(i, tab.Item.Id), _presentationManager, _imageManager, _apiClient, _nav, _playbackManager, _logger, _serverEvents)
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
            if (string.Equals(section, CollectionType.LiveTv, StringComparison.OrdinalIgnoreCase))
            {
                var vm = new ItemListViewModel(i => GetFolderItems(i, tab.Item.Id), _presentationManager, _imageManager, _apiClient, _nav, _playbackManager, _logger, _serverEvents)
                {
                    ImageDisplayWidth = 270,
                    ImageDisplayHeightGenerator = v => 270,
                    DisplayNameGenerator = GetDisplayName,

                    OnItemCreated = v =>
                    {
                        v.DisplayNameVisibility = Visibility.Visible;
                    }
                };

                return vm;

            }
            if (string.Equals(section, CollectionType.Games, StringComparison.OrdinalIgnoreCase))
            {
                return new GamesViewModel(_presentationManager, _imageManager, _apiClient, _sessionManager, _nav,
                                       _playbackManager, _logger, TileWidth, TileHeight, _serverEvents, tab.Item.Id);
            }
            if (string.Equals(section, CollectionType.TvShows, StringComparison.OrdinalIgnoreCase))
            {
                var tvViewModel = GetTvViewModel(tab.Item.Id);
                tvViewModel.CurrentItemChanged += SectionViewModel_CurrentItemChanged;
                return tvViewModel;
            }
            if (string.Equals(section, CollectionType.Movies, StringComparison.OrdinalIgnoreCase))
            {
                var moviesViewModel = GetMoviesViewModel(tab.Item.Id);
                moviesViewModel.CurrentItemChanged += SectionViewModel_CurrentItemChanged;
                return moviesViewModel;
            }
            if (string.Equals(section, CollectionType.Channels, StringComparison.OrdinalIgnoreCase))
            {
                var vm = new ItemListViewModel(GetChannelsAsync, _presentationManager, _imageManager, _apiClient, _nav, _playbackManager, _logger, _serverEvents)
                {
                    ImageDisplayWidth = 480,
                    ImageDisplayHeightGenerator = v => 270,
                    DisplayNameGenerator = GetDisplayName,

                    OnItemCreated = v =>
                    {
                        v.DisplayNameVisibility = Visibility.Visible;
                    },

                    PreferredImageTypesGenerator = i => new[] { ImageType.Thumb, ImageType.Backdrop, ImageType.Primary }
                };

                return vm;

            }

            return null;
        }

        private TvViewModel GetTvViewModel(string parentId)
        {
            return new TvViewModel(_presentationManager, _imageManager, _apiClient, _sessionManager, _nav, _playbackManager, _logger, TileWidth, TileHeight, _serverEvents, parentId);
        }

        private MoviesViewModel GetMoviesViewModel(string parentId)
        {
            return new MoviesViewModel(_presentationManager, _imageManager, _apiClient, _sessionManager, _nav,
                                       _playbackManager, _logger, TileWidth, TileHeight, _serverEvents, parentId);
        }

        private async Task<ItemsResult> GetChannelsAsync(ItemListViewModel viewModel)
        {
            var query = new ChannelQuery
            {
                UserId = _sessionManager.CurrentUser.Id
            };

            var result = await _apiClient.GetChannels(query, CancellationToken.None);

            return new ItemsResult
            {
                Items = result.Items,
                TotalRecordCount = result.TotalRecordCount
            };
        }

        private Task<ItemsResult> GetFolderItems(ItemListViewModel viewModel,string parentId)
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

                SortOrder = SortOrder.Ascending,

                ParentId = parentId
            };

            return _apiClient.GetItemsAsync(query, CancellationToken.None);
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
                else if (string.Equals(tab.Name, "tvshows"))
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
                var tab = (TabItem)Sections.CurrentItem;
                vm = GetTvViewModel(tab.Item.Id);
            }

            return vm.NavigateToAllShows();
        }

        private Task NavigateToAllMoviesInternal()
        {
            var vm = ContentViewModel as MoviesViewModel;

            if (vm == null)
            {
                var tab = (TabItem)Sections.CurrentItem;
                vm = GetMoviesViewModel(tab.Item.Id);
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

        private void Resume(object commandParameter)
        {
            var item = commandParameter as ItemViewModel;

            if (item != null)
            {
                item.Resume();
            }
        }

        // IAcceptsPlayCommand.HandlePlayCommand
        //
        // If we  play the media direct from the home page
        // resume the item if possible, not this is differnet on
        // the detail page where we have a choice between play and resume
        //
        public void HandlePlayCommand()
        {
            Resume(CurrentItem);
        }
    }
}
