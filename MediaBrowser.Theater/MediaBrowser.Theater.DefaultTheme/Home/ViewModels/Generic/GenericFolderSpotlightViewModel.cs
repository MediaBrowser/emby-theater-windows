using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using MediaBrowser.Model.ApiClient;
using MediaBrowser.Model.Dto;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.Logging;
using MediaBrowser.Model.Querying;
using MediaBrowser.Theater.Api.Navigation;
using MediaBrowser.Theater.Api.Playback;
using MediaBrowser.Theater.Api.Session;
using MediaBrowser.Theater.Api.UserInterface;
using MediaBrowser.Theater.DefaultTheme.Core.ViewModels;
using MediaBrowser.Theater.DefaultTheme.Home.ViewModels.Movies;
using MediaBrowser.Theater.DefaultTheme.ItemList;
using MediaBrowser.Theater.Presentation;
using MediaBrowser.Theater.Presentation.Controls;
using MediaBrowser.Theater.Presentation.ViewModels;

namespace MediaBrowser.Theater.DefaultTheme.Home.ViewModels.Generic
{
    public class GenericFolderSpotlightViewModel
        : BaseViewModel, IKnownSize, IHomePage
    {
        private readonly IApiClient _apiClient;
        private readonly IImageManager _imageManager;
        private readonly ILogger _logger;
        private readonly double _miniSpotlightWidth;
        private readonly INavigator _navigator;
        private readonly IPlaybackManager _playbackManager;
        private readonly IServerEvents _serverEvents;
        private readonly ISessionManager _sessionManager;
        private CancellationTokenSource _mainViewCancellationTokenSource;

        public GenericFolderSpotlightViewModel(BaseItemDto folder, IImageManager imageManager, INavigator navigator, IApiClient apiClient, IServerEvents serverEvents, ISessionManager sessionManager, ILogManager logManager, IPlaybackManager playbackManager)
        {
            _imageManager = imageManager;
            _navigator = navigator;
            _apiClient = apiClient;
            _serverEvents = serverEvents;
            _playbackManager = playbackManager;
            _sessionManager = sessionManager;
            _logger = logManager.GetLogger(folder.Name + " Spotlight");
            SpotlightHeight = HomeViewModel.TileHeight*2 + HomeViewModel.TileMargin*2;
            SpotlightWidth = 16*(SpotlightHeight/9) + 100;
            _miniSpotlightWidth = HomeViewModel.TileWidth + (HomeViewModel.TileMargin/4) - 1;

            LowerSpotlightWidth = SpotlightWidth/3 - HomeViewModel.TileMargin*1.5;
            LowerSpotlightHeight = HomeViewModel.TileHeight;

            BrowseItemsCommand = new RelayCommand(arg => {
                var itemParams = new ItemListParameters {
                    Items = GetChildren(folder),
                    Title = "Browse All" //todo localize
                };

                navigator.Navigate(Go.To.ItemList(itemParams));
            });
            
            AllShowsImagesViewModel = new ImageSlideshowViewModel(imageManager, Enumerable.Empty<string>()) {
                ImageStretch = Stretch.UniformToFill
            };

            MiniSpotlightItemsSide = new RangeObservableCollection<ItemTileViewModel> {
                CreateMiniSpotlightItem(),
                CreateMiniSpotlightItem(),
                CreateMiniSpotlightItem(),
            };

            MiniSpotlightItemsBottom = new RangeObservableCollection<ItemTileViewModel> {
                CreateMiniSpotlightItem(),
                CreateMiniSpotlightItem(),
            };

            Title = SectionTitle = folder.Name;

            LoadViewModels(folder);
        }
        
        public double SpotlightWidth { get; private set; }
        public double SpotlightHeight { get; private set; }

        public double LowerSpotlightWidth { get; private set; }
        public double LowerSpotlightHeight { get; private set; }

        public ImageSlideshowViewModel AllShowsImagesViewModel { get; private set; }
        public RangeObservableCollection<ItemTileViewModel> MiniSpotlightItemsSide { get; private set; }
        public RangeObservableCollection<ItemTileViewModel> MiniSpotlightItemsBottom { get; private set; }

        public ICommand BrowseItemsCommand { get; private set; }

        public string Title { get; set; }

        public string SectionTitle { get; set; }

        public int Index { get; set; }

        public Size Size
        {
            get
            {
                return new Size(SpotlightWidth + _miniSpotlightWidth + 4*HomeViewModel.TileMargin + HomeViewModel.SectionSpacing,
                                SpotlightHeight + HomeViewModel.TileHeight + 4*HomeViewModel.TileMargin);
            }
        }

        private void DisposeMainViewCancellationTokenSource(bool cancel)
        {
            if (_mainViewCancellationTokenSource != null) {
                if (cancel) {
                    try {
                        _mainViewCancellationTokenSource.Cancel();
                    }
                    catch (ObjectDisposedException) { }
                }
                _mainViewCancellationTokenSource.Dispose();
                _mainViewCancellationTokenSource = null;
            }
        }

        public static string GetDisplayName(BaseItemDto item)
        {
            string name = item.Name;

            if (item.IsType("Episode")) {
                name = item.SeriesName;

                if (item.IndexNumber.HasValue && item.ParentIndexNumber.HasValue) {
                    name = name + " " + string.Format("S{0}, E{1}", item.ParentIndexNumber.Value, item.IndexNumber.Value);
                }
            }

            return name;
        }

        private Task<ItemsResult> GetChildren(BaseItemDto item)
        {
            var query = new ItemQuery
            {
                UserId = _sessionManager.CurrentUser.Id,
                ParentId = item.Id,
                SortBy = new[] { ItemSortBy.SortName },
                Fields = MovieSpotlightViewModel.QueryFields
            };

            return _apiClient.GetItemsAsync(query);
        }

        private async void LoadViewModels(BaseItemDto tvFolder)
        {
            CancellationTokenSource cancellationSource = _mainViewCancellationTokenSource = new CancellationTokenSource();

            try {
                cancellationSource.Token.ThrowIfCancellationRequested();
                                
                var spotlight = await _apiClient.GetItemsAsync(new ItemQuery
                {
                    UserId = _sessionManager.CurrentUser.Id,
                    ParentId = tvFolder.Id,
                    SortBy = new[] { ItemSortBy.CommunityRating },
                    SortOrder = SortOrder.Descending,
                    Fields = MovieSpotlightViewModel.QueryFields,
                    Filters = new[] { ItemFilter.IsRecentlyAdded },
                    Limit = 10,
                    Recursive = true
                });

                if (spotlight.TotalRecordCount < 2) {
                    spotlight = await _apiClient.GetItemsAsync(new ItemQuery {
                        UserId = _sessionManager.CurrentUser.Id,
                        ParentId = tvFolder.Id,
                        SortBy = new[] { ItemSortBy.CommunityRating },
                        SortOrder = SortOrder.Descending,
                        Fields = MovieSpotlightViewModel.QueryFields,
                        Limit = 10,
                        Recursive = true
                    });
                }

                await LoadAllShowsViewModel(tvFolder);
                LoadMiniSpotlightsViewModel(spotlight);
            }
            catch (Exception ex) {
                _logger.ErrorException("Error getting folder view", ex);
            }
            finally {
                DisposeMainViewCancellationTokenSource(false);
            }
        }

        private ItemTileViewModel CreateMiniSpotlightItem()
        {
            return new ItemTileViewModel(_apiClient, _imageManager, _serverEvents, _navigator, _playbackManager, null) {
                DesiredImageWidth = _miniSpotlightWidth,
                DesiredImageHeight = HomeViewModel.TileHeight,
                PreferredImageTypes = new[] { ImageType.Backdrop, ImageType.Thumb },
                DisplayNameGenerator = GetDisplayName
            };
        }

        private void LoadMiniSpotlightsViewModel(ItemsResult tvItems)
        {
            var rnd = new Random();
            var items = tvItems.Items.OrderBy(i => rnd.Next()).Take(5).ToList();

            AddItemsToMiniSpotlight(MiniSpotlightItemsBottom, items.Take(2).ToList());
            AddItemsToMiniSpotlight(MiniSpotlightItemsSide, items.Skip(2).Take(3).ToList());
        }

        private void AddItemsToMiniSpotlight(RangeObservableCollection<ItemTileViewModel> spotlight, IList<BaseItemDto> items) 
        {
            for (int i = 0; i < items.Count; i++) {
                if (spotlight.Count > i) {
                    spotlight[i].Item = items[i];
                } else {
                    ItemTileViewModel vm = CreateMiniSpotlightItem();
                    vm.Item = items[i];

                    spotlight.Add(vm);
                }
            }

            if (spotlight.Count > items.Count) {
                List<ItemTileViewModel> toRemove = spotlight.Skip(items.Count).ToList();
                spotlight.RemoveRange(toRemove);
            }
        }

        private async Task LoadAllShowsViewModel(BaseItemDto folder)
        {
            var folderImage = FindFolderImage(folder);
            if (folderImage != null) {
                AllShowsImagesViewModel.Images.Add(folderImage);
            } else {
                var items = await GetChildren(folder);

                IEnumerable<string> images = items.Items
                                                  .Where(i => i.BackdropImageTags.Any())
                                                  .Select(i => _apiClient.GetImageUrl(i.Id, new ImageOptions {
                                                      ImageType = ImageType.Backdrop,
                                                      Tag = i.BackdropImageTags.First(),
                                                      Width = (int) SpotlightWidth,
                                                      EnableImageEnhancers = false
                                                  }));

                AllShowsImagesViewModel.Images.AddRange(images);
            }

            AllShowsImagesViewModel.StartRotating();
        }

        private string FindFolderImage(BaseItemDto folder)
        {
            var preferredTypes = new[] { ImageType.Backdrop, ImageType.Art, ImageType.Primary };

            return preferredTypes.Select(imageType => _apiClient.GetImageUrl(folder.Id, new ImageOptions { ImageType = imageType, Width = (int) SpotlightWidth, EnableImageEnhancers = false }))
                                 .FirstOrDefault(folderImage => folderImage != null);
        }
    }
}
