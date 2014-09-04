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

            LowerSpotlightWidth = SpotlightWidth/2 - HomeViewModel.TileMargin;//*1.5;
            LowerSpotlightHeight = HomeViewModel.TileHeight;

            BrowseItemsCommand = new RelayCommand(arg => {
                var itemParams = new ItemListParameters {
                    Items = GetChildren(folder),
                    Title = "Browse All" //todo localize
                };

                navigator.Navigate(Go.To.ItemList(itemParams));
            });
            
            AllItemsImagesViewModel = new ImageSlideshowViewModel(imageManager, Enumerable.Empty<string>()) {
                ImageStretch = Stretch.UniformToFill
            };

            MiniSpotlightItemsSide = new RangeObservableCollection<ItemTileViewModel> {
                CreateMiniSpotlightItem(_miniSpotlightWidth, HomeViewModel.TileHeight),
                CreateMiniSpotlightItem(_miniSpotlightWidth, HomeViewModel.TileHeight),
                CreateMiniSpotlightItem(_miniSpotlightWidth, HomeViewModel.TileHeight),
            };

            MiniSpotlightItemsBottom = new RangeObservableCollection<ItemTileViewModel> {
                CreateMiniSpotlightItem(LowerSpotlightWidth, LowerSpotlightHeight),
                CreateMiniSpotlightItem(LowerSpotlightWidth, LowerSpotlightHeight),
            };

            Title = SectionTitle = folder.Name;

            LoadViewModels(folder);
        }
        
        public double SpotlightWidth { get; private set; }
        public double SpotlightHeight { get; private set; }

        public double LowerSpotlightWidth { get; private set; }
        public double LowerSpotlightHeight { get; private set; }

        public ImageSlideshowViewModel AllItemsImagesViewModel { get; private set; }
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

        private async void LoadViewModels(BaseItemDto folder)
        {
            CancellationTokenSource cancellationSource = _mainViewCancellationTokenSource = new CancellationTokenSource();

            try {
                var spotlight = await _apiClient.GetItemsAsync(new ItemQuery
                {
                    UserId = _sessionManager.CurrentUser.Id,
                    ParentId = folder.Id,
                    SortBy = new[] { ItemSortBy.CommunityRating },
                    SortOrder = SortOrder.Descending,
                    Fields = MovieSpotlightViewModel.QueryFields,
                    Filters = new[] { ItemFilter.IsRecentlyAdded },
                    Limit = 10,
                    Recursive = true
                }, cancellationSource.Token);

                if (spotlight.TotalRecordCount < 2) {
                    spotlight = await _apiClient.GetItemsAsync(new ItemQuery {
                        UserId = _sessionManager.CurrentUser.Id,
                        ParentId = folder.Id,
                        SortBy = new[] { ItemSortBy.CommunityRating },
                        SortOrder = SortOrder.Descending,
                        Fields = MovieSpotlightViewModel.QueryFields,
                        Limit = 10,
                        Recursive = true
                    }, cancellationSource.Token);
                }

                await LoadAllItemsViewModel(folder);
                LoadMiniSpotlightsViewModel(spotlight);
            }
            catch (Exception ex) {
                _logger.ErrorException("Error getting folder view", ex);
            }
            finally {
                DisposeMainViewCancellationTokenSource(false);
            }
        }

        private ItemTileViewModel CreateMiniSpotlightItem(double width, double height)
        {
            return new ItemTileViewModel(_apiClient, _imageManager, _serverEvents, _navigator, _playbackManager, null) {
                DesiredImageWidth = width,
                DesiredImageHeight = height,
                PreferredImageTypes = new[] { ImageType.Backdrop, ImageType.Thumb },
                DisplayNameGenerator = GetDisplayName
            };
        }

        private void LoadMiniSpotlightsViewModel(ItemsResult itemResult)
        {
            var rnd = new Random();
            var items = itemResult.Items.OrderBy(i => rnd.Next()).Take(5).ToList();

            AddItemsToMiniSpotlight(MiniSpotlightItemsBottom, items.Take(2).ToList(), LowerSpotlightWidth, LowerSpotlightHeight);
            AddItemsToMiniSpotlight(MiniSpotlightItemsSide, items.Skip(2).Take(3).ToList(), _miniSpotlightWidth, HomeViewModel.TileHeight);
        }

        private void AddItemsToMiniSpotlight(RangeObservableCollection<ItemTileViewModel> spotlight, IList<BaseItemDto> items, double itemWidth, double itemHeight) 
        {
            for (int i = 0; i < items.Count; i++) {
                if (spotlight.Count > i) {
                    spotlight[i].Item = items[i];
                } else {
                    ItemTileViewModel vm = CreateMiniSpotlightItem(itemWidth, itemHeight);
                    vm.Item = items[i];

                    spotlight.Add(vm);
                }
            }

            if (spotlight.Count > items.Count) {
                List<ItemTileViewModel> toRemove = spotlight.Skip(items.Count).ToList();
                spotlight.RemoveRange(toRemove);
            }
        }

        private async Task LoadAllItemsViewModel(BaseItemDto folder)
        {
            var folderImage = DownloadImage(folder);
            if (folderImage != null) {
                AllItemsImagesViewModel.Images.Add(folderImage);
            } else {
                var items = await GetChildren(folder);

                IEnumerable<string> images = items.Items.Select(DownloadImage)
                                                  .Where(img => img != null);

                AllItemsImagesViewModel.Images.AddRange(images);
            }

            AllItemsImagesViewModel.StartRotating();
        }
        
        /// <summary>
        ///     Gets an image url that can be used to download an image from the api
        /// </summary>
        /// <param name="imageType">The type of image requested</param>
        /// <param name="imageIndex">
        ///     The image index, if there are multiple. Currently only applies to backdrops. Supply null or 0
        ///     for first backdrop.
        /// </param>
        /// <returns>System.String.</returns>
        private string GetImageUrl(BaseItemDto item, ImageType imageType, int? imageIndex = null)
        {
            var imageOptions = new ImageOptions
            {
                ImageType = imageType,
                ImageIndex = imageIndex,
                Width = (int)SpotlightWidth,
                EnableImageEnhancers = false
            };
            
            return _apiClient.GetImageUrl(item, imageOptions);
        }

        public string DownloadImage(BaseItemDto item)
        {
            var preferredImageTypes = new[] { ImageType.Backdrop, ImageType.Art, ImageType.Primary };
            
            if (item != null)
            {
                foreach (ImageType imageType in preferredImageTypes)
                {
                    if (imageType == ImageType.Backdrop)
                    {
                        if (item.BackdropCount == 0)
                        {
                            continue;
                        }
                    }
                    else
                    {
                        if (!item.ImageTags.ContainsKey(imageType))
                        {
                            continue;
                        }
                    }

                    return GetImageUrl(item, imageType);
                }
            }

            return null;
        }
    }
}
