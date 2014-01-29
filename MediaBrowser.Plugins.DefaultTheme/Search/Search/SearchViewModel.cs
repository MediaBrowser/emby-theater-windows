using System.Windows;
using System.Windows.Threading;
using MediaBrowser.Model.ApiClient;
using MediaBrowser.Model.Dto;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.Logging;
using MediaBrowser.Model.Querying;
using MediaBrowser.Model.Search;
using MediaBrowser.Plugins.DefaultTheme.ListPage;
using MediaBrowser.Theater.Interfaces.Navigation;
using MediaBrowser.Theater.Interfaces.Playback;
using MediaBrowser.Theater.Interfaces.Presentation;
using MediaBrowser.Theater.Interfaces.Session;
using MediaBrowser.Theater.Interfaces.ViewModels;
using MediaBrowser.Theater.Presentation.ViewModels;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace MediaBrowser.Plugins.DefaultTheme.Search
{
    public class SearchViewModel : BaseViewModel, IDisposable
    {
   
        private readonly IPresentationManager _presentationManager;
        private readonly IImageManager _imageManager;
        private readonly IApiClient _apiClient;
        private readonly ISessionManager _sessionManager;
        private readonly INavigationService _navService;
        private readonly IPlaybackManager _playbackManager;
        private readonly ILogger _logger;
        private readonly IServerEvents _serverEvents;
   
        private readonly ImageType[] _preferredImageTypes = new[] { ImageType.Backdrop, ImageType.Primary, ImageType.Thumb };
        private readonly ItemsResult _emptyItemsResult = new ItemsResult { TotalRecordCount = 0, Items = new BaseItemDto[0] };
        private ItemsResult _searchedItemsResult = new ItemsResult { TotalRecordCount = 0, Items = new BaseItemDto[0] };
        private ItemsResult _searchedPeopleItemsResult = new ItemsResult { TotalRecordCount = 0, Items = new BaseItemDto[0] };
        private Timer _updateSearchTimer;
        private readonly Dispatcher _dispatcher;

        public ItemListViewModel MatchedItemsViewModel { get; private set; }
        public ItemListViewModel MatchedPeopleViewModel { get; private set; }
        public AlphaInputViewModel AlphaInputViewModel { get; private set; }

        public SearchViewModel(IPresentationManager presentationManager, IImageManager imageManager, IApiClient apiClient, ISessionManager sessionManager, INavigationService navService, IPlaybackManager playbackManager, ILogger logger, IServerEvents serverEvents)
        {
           
            _presentationManager = presentationManager;
            _imageManager = imageManager;
            _apiClient = apiClient;
            _sessionManager = sessionManager;
            _navService = navService;
            _playbackManager = playbackManager;
            _logger = logger;
            _serverEvents = serverEvents;
            _dispatcher = Dispatcher.CurrentDispatcher;

            LoadMatchedItemsViewModel();
            LoadMatchedPeopleViewModel();

            AlphaInputViewModel = new AlphaInputViewModel(UpdateSearchText, presentationManager, _imageManager, _apiClient, _navService, _playbackManager, _logger, _serverEvents);
          
        }

        // 3x3 grid 1080 wide, ItemDisplayWidth = TileWidth, ItemContainerWidth = ItemDisplayWidth + 20, ditto for height
        private const double TileWidth = 351;
        private const double TileHeight = TileWidth * 9 / 16;
     
        private void LoadMatchedItemsViewModel()
        {
            MatchedItemsViewModel = new ItemListViewModel(s => Task.FromResult(_searchedItemsResult), _presentationManager, _imageManager, _apiClient, _navService, _playbackManager, _logger, _serverEvents)
            {
               ImageDisplayWidth = TileWidth,
               ImageDisplayHeightGenerator = vm => TileHeight,
               DisplayNameGenerator = GetDisplayName,
               PreferredImageTypesGenerator = vm => new[] { ImageType.Thumb, ImageType.Primary }, //   vm => _preferredImageTypes,
               ScrollDirection = ScrollDirection.Horizontal,
               EnableBackdropsForCurrentItem = false,
               ShowSidebar = false,
               AutoSelectFirstItem = false,
               ShowLoadingAnimation = true,
               IsVirtualizationRequired= false,
            };

            MatchedItemsViewModel.ItemContainerWidth = TileWidth + 5;
            MatchedItemsViewModel.ItemContainerHeight = TileHeight + 5;
            OnPropertyChanged("MatchedItemsViewModel");
        }

        private void LoadMatchedPeopleViewModel()
        {
            MatchedPeopleViewModel = new ItemListViewModel(s => Task.FromResult(_searchedPeopleItemsResult), _presentationManager, _imageManager, _apiClient, _navService, _playbackManager, _logger, _serverEvents)
            {
                ImageDisplayWidth = (TileWidth + 20) / 4,
                ImageDisplayHeightGenerator = vm =>89,
                DisplayNameGenerator = GetDisplayName,
                PreferredImageTypesGenerator = vm => new[] { ImageType.Primary, ImageType.Thumb },
                ScrollDirection = ScrollDirection.Horizontal,
                EnableBackdropsForCurrentItem = false,
                AutoSelectFirstItem = false,
                ShowLoadingAnimation = false,
                IsVirtualizationRequired = false,
              
            };

            MatchedPeopleViewModel.ItemContainerWidth = MatchedPeopleViewModel.ImageDisplayWidth + 5;
            MatchedPeopleViewModel.ItemContainerHeight = MatchedPeopleViewModel.DefaultImageDisplayHeight + 5;
            OnPropertyChanged("MatchedPeopleViewModel");
        }
        
        private async Task<Boolean> GeSearchItemsAsync()
        {
            SearchHintResult searchHintResult = await _apiClient.GetSearchHintsAsync(new SearchQuery { UserId = _apiClient.CurrentUserId, SearchTerm = CurrentSearch, IncludePeople = false, IncludeStudios = false, Limit = 9 });
           
            var ids = (searchHintResult.TotalRecordCount > 0) ? searchHintResult.SearchHints.Select(s => s.ItemId).ToArray() : null;
            if (ids != null && ids.Length > 0)
            {
                var query = new ItemQuery
                {
                    Ids = ids,
                    UserId = _apiClient.CurrentUserId,
                    Fields = FolderPage.QueryFields
                };

                _searchedItemsResult = await _apiClient.GetItemsAsync(query);
            }
            else
            {
                _searchedItemsResult = _emptyItemsResult;
            }

            return true;
        }

        private async Task<Boolean> GeSearchPeopleAsync()
        {
            SearchHintResult searchHintResult = await _apiClient.GetSearchHintsAsync(new SearchQuery {UserId = _apiClient.CurrentUserId, SearchTerm = CurrentSearch, IncludePeople = true, IncludeArtists = false, IncludeGenres = false, IncludeMedia = false, IncludeStudios = false, Limit = 9});

            var ids = (searchHintResult.TotalRecordCount > 0) ? searchHintResult.SearchHints.Select(s => s.ItemId).ToArray() : null;
            if (ids != null && ids.Length > 0)
            {
                var query = new ItemQuery
                {
                    Ids = ids,
                    UserId = _apiClient.CurrentUserId,
                    Fields = FolderPage.QueryFields
                };

                _searchedPeopleItemsResult = await _apiClient.GetItemsAsync(query);
            }
            else
            {
                _searchedPeopleItemsResult = _emptyItemsResult;
            }

            return true;
        }

        private async void UpdateSearch()
        {
           MatchedPeopleVisibility = String.IsNullOrEmpty(CurrentSearch) ? Visibility.Hidden : Visibility.Visible;
           MatchedItemsVisibility = String.IsNullOrEmpty(CurrentSearch) ? Visibility.Hidden : Visibility.Visible;


           if (!String.IsNullOrEmpty(CurrentSearch))
            {
                var searchTasks = new Task[2];

               
                searchTasks[0] = Task.Run(() => GeSearchItemsAsync());
                searchTasks[1] = Task.Run(() => GeSearchPeopleAsync());
               
                Task.WaitAll(searchTasks); 
            }
            else
            {
                _searchedItemsResult = _emptyItemsResult;
                _searchedPeopleItemsResult = _emptyItemsResult;
            }

           await MatchedItemsViewModel.ReloadItems(true); 
           await MatchedPeopleViewModel.ReloadItems(true);
        }

        // Add a new char to current search, causes searc to fire
        private  Task<Boolean> UpdateSearchText(AlphaInputViewModel viewModel)
        {
           var currentSearch = CurrentSearch;

            var indexOption = viewModel.CurrentIndexOption == null ? string.Empty : viewModel.CurrentIndexOption.Name;

            if (string.Compare(indexOption, "_") == 0)
                indexOption = " ";

            if (string.Compare(indexOption, "back") == 0)
            {
                if (CurrentSearch.Length > 0)
                    CurrentSearch = CurrentSearch.Substring(0, CurrentSearch.Length - 1);
            }
            else
                CurrentSearch = CurrentSearch + indexOption;

            return Task.FromResult(true);
        }

     
        private Visibility _matchedItemsVisibility = Visibility.Hidden;
        public Visibility MatchedItemsVisibility
        {
            get { return _matchedItemsVisibility; }

            set
            {
                var changed = _matchedItemsVisibility != value;

                _matchedItemsVisibility = value;

                if (changed)
                {
                    OnPropertyChanged("MatchedItemsVisibility");
                }
            }
        }

        private Visibility _matchedPeopleVisibility = Visibility.Hidden;
        public Visibility MatchedPeopleVisibility
        {
            get { return _matchedPeopleVisibility; }

            set
            {
                var changed = _matchedPeopleVisibility != value;

                _matchedPeopleVisibility = value;

                if (changed)
                {
                    OnPropertyChanged("MatchedPeopleVisibility");
                }
            }
        }


        private void UpdateSearchInvokeAsync(object state)
        {
            _dispatcher.InvokeAsync(UpdateSearch);
        }

        
        private readonly object _searchSyncLock = new object();

        private string _currentSearch;
        public string CurrentSearch
        {
            get { return _currentSearch;  }

            set {
                var changed = _currentSearch != value;

                _currentSearch = value;

                if (changed)
                {
                      lock (_searchSyncLock)
                      {
                          // effectivly wait for 500 MS for another key stroke before we fire off a search - nagal algorithm
                            if (_updateSearchTimer == null)
                            {
                                _updateSearchTimer = new Timer(UpdateSearchInvokeAsync, null, 500, Timeout.Infinite);
                            }
                            else
                            {
                                _updateSearchTimer.Change(500, Timeout.Infinite);
                            }
                      }
                     OnPropertyChanged("CurrentSearch");
                }
            }
        }

        private ItemViewModel _currentItem;
        public ItemViewModel CurrentItem
        {
            get { return _currentItem; }

            set
            {
                var changed = _currentItem != value;

                _currentItem = value;

                if (changed)
                {
                    OnPropertyChanged("CurrentItem");
                }
            }
        }


        public static string GetDisplayName(BaseItemDto item)
        {
            var name = item.Name;

            if (item.IndexNumber.HasValue && !item.IsType("season"))
            {
                name = item.IndexNumber + " - " + name;
            }

            if (item.ParentIndexNumber.HasValue && item.IsAudio)
            {
                name = item.ParentIndexNumber + "." + name;
            }

            return name;
        }

      
        public void Dispose()
        {
            lock (_searchSyncLock)
            {
                if (_updateSearchTimer != null)
                {
                    _updateSearchTimer.Dispose();
                    _updateSearchTimer = null;
                }
            }

            if (MatchedItemsViewModel != null)
            {
                MatchedItemsViewModel.Dispose();
            }
            if (MatchedPeopleViewModel != null)
            {
                MatchedPeopleViewModel.Dispose();
            }

            if (AlphaInputViewModel != null)
            {
                AlphaInputViewModel.Dispose();
            }
        }
    }
}
