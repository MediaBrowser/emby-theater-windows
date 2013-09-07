using System;
using System.Threading;
using MediaBrowser.Model.ApiClient;
using MediaBrowser.Model.Dto;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.Logging;
using MediaBrowser.Model.Querying;
using MediaBrowser.Plugins.DefaultTheme.Controls;
using MediaBrowser.Plugins.DefaultTheme.ListPage;
using MediaBrowser.Theater.Interfaces.Navigation;
using MediaBrowser.Theater.Interfaces.Playback;
using MediaBrowser.Theater.Interfaces.Presentation;
using MediaBrowser.Theater.Interfaces.Session;
using MediaBrowser.Theater.Interfaces.ViewModels;
using MediaBrowser.Theater.Presentation.ViewModels;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MediaBrowser.Plugins.DefaultTheme.Home
{
    public class TvViewModel : BaseViewModel
    {
        private readonly IApiClient _apiClient;
        private readonly ISessionManager _sessionManager;
        private readonly IPresentationManager _presentationManager;
        private readonly IPlaybackManager _playbackManager;
        private readonly IImageManager _imageManager;
        private readonly INavigationService _navService;
        private readonly ILogger _logger;

        public ItemListViewModel NextUpViewModel { get; private set; }
        public ItemListViewModel ResumeViewModel { get; private set; }
        public ItemListViewModel LatestEpisodesViewModel { get; private set; }

        public GalleryViewModel AllShowsViewModel { get; private set; }
        public GalleryViewModel ActorsViewModel { get; private set; }

        public ImageViewerViewModel SpotlightViewModel { get; private set; }

        public TvViewModel(IPresentationManager presentation, IImageManager imageManager, IApiClient apiClient, ISessionManager session, INavigationService nav, IPlaybackManager playback, ILogger logger)
        {
            _apiClient = apiClient;
            _sessionManager = session;
            _presentationManager = presentation;
            _playbackManager = playback;
            _imageManager = imageManager;
            _navService = nav;
            _logger = logger;

            LatestEpisodesViewModel = new ItemListViewModel(GetLatestEpisodesAsync, presentation, imageManager, apiClient, session, nav, playback, logger)
            {
                ImageDisplayWidth = HomePageViewModel.TileWidth,
                ImageDisplayHeightGenerator = v => HomePageViewModel.TileHeight,
                DisplayNameGenerator = MultiItemTile.GetDisplayName,
                EnableBackdropsForCurrentItem = false
            };

            NextUpViewModel = new ItemListViewModel(GetNextUpAsync, presentation, imageManager, apiClient, session, nav, playback, logger)
            {
                ImageDisplayWidth = HomePageViewModel.TileWidth,
                ImageDisplayHeightGenerator = v => HomePageViewModel.TileHeight,
                DisplayNameGenerator = MultiItemTile.GetDisplayName,
                EnableBackdropsForCurrentItem = false
            };

            ResumeViewModel = new ItemListViewModel(GetResumeablesAsync, presentation, imageManager, apiClient, session, nav, playback, logger)
            {
                ImageDisplayWidth = HomePageViewModel.TileWidth,
                ImageDisplayHeightGenerator = v => HomePageViewModel.TileHeight,
                DisplayNameGenerator = MultiItemTile.GetDisplayName,
                EnableBackdropsForCurrentItem = false
            };

            LoadSpotlightViewModel();
            LoadAllShowsViewModel();
            LoadActorsViewModel();
        }

        private async void LoadSpotlightViewModel()
        {
            const ImageType imageType = ImageType.Backdrop;

            const int tileWidth = HomePageViewModel.TileWidth * 2 + 36;
            const int tileHeight = tileWidth * 9 / 16;

            SpotlightViewModel = new ImageViewerViewModel(_imageManager, new List<ImageViewerImage>())
            {
                Height = tileHeight,
                Width = tileWidth
            };

            var itemsResult = await _apiClient.GetItemsAsync(new ItemQuery
            {
                UserId = _sessionManager.CurrentUser.Id,

                SortBy = new[] { ItemSortBy.Random },

                IncludeItemTypes = new[] { "Series" },

                ImageTypes = new[] { imageType },

                Limit = 10,

                Recursive = true
            });

            var images = itemsResult.Items.Select(i => new ImageViewerImage
            {
                Url = _apiClient.GetImageUrl(i, new ImageOptions
                {
                    Height = tileHeight,
                    ImageType = imageType

                }),

                Caption = i.Name,
                Item = i

            }).ToList();

            SpotlightViewModel.CustomCommandAction = i => _navService.NavigateToItem(i.Item, ViewType.Tv);

            SpotlightViewModel.Images.AddRange(images);
            SpotlightViewModel.StartRotating(8000);
        }

        private async void LoadActorsViewModel()
        {
            ActorsViewModel = new GalleryViewModel(_apiClient, _imageManager, _navService)
            {
                GalleryHeight = HomePageViewModel.TileHeight,
                GalleryWidth = HomePageViewModel.TileWidth
            };

            ActorsViewModel.CustomCommandAction = NavigateToActors;

            var actorsResult = await _apiClient.GetPeopleAsync(new PersonsQuery
            {
                IncludeItemTypes = new[] { "Series" },
                SortBy = new[] { ItemSortBy.Random },
                Recursive = true,
                Limit = 3,
                PersonTypes = new[] { PersonType.Actor },
                ImageTypes = new[] { ImageType.Primary }
            });

            var images = actorsResult.Items.Select(i => _apiClient.GetImageUrl(i, new ImageOptions
            {
                Height = HomePageViewModel.TileHeight

            }));

            ActorsViewModel.AddImages(images);
        }

        private async void NavigateToActors()
        {
            var item = await _apiClient.GetRootFolderAsync(_sessionManager.CurrentUser.Id);

            var displayPreferences = await _presentationManager.GetDisplayPreferences("TVActors", CancellationToken.None);

            var page = new FolderPage(item, displayPreferences, _apiClient, _imageManager, _sessionManager,
                                      _presentationManager, _navService, _playbackManager, _logger);

            page.CustomPageTitle = "TV | Actors";

            page.ViewType = ViewType.Tv;
            page.CustomItemQuery = GetAllActors;

            await _navService.Navigate(page);
        }

        private async void LoadAllShowsViewModel()
        {
            AllShowsViewModel = new GalleryViewModel(_apiClient, _imageManager, _navService)
            {
                GalleryHeight = HomePageViewModel.TileHeight,
                GalleryWidth = HomePageViewModel.TileWidth
            };

            AllShowsViewModel.CustomCommandAction = NavigateToAllShows;

            const ImageType imageType = ImageType.Primary;

            var allSeriesResult = await _apiClient.GetItemsAsync(new ItemQuery
            {
                UserId = _sessionManager.CurrentUser.Id,

                SortBy = new[] { ItemSortBy.Random },

                IncludeItemTypes = new[] { "Series" },

                ImageTypes = new[] { imageType },

                Limit = 3,

                Recursive = true
            });

            var images = allSeriesResult.Items.Select(i => _apiClient.GetImageUrl(i, new ImageOptions
            {
                Height = HomePageViewModel.TileHeight,
                ImageType = imageType

            }));

            AllShowsViewModel.AddImages(images);
        }

        private async void NavigateToAllShows()
        {
            var item = await _apiClient.GetRootFolderAsync(_sessionManager.CurrentUser.Id);

            var displayPreferences = await _presentationManager.GetDisplayPreferences("AllShows", CancellationToken.None);

            var page = new FolderPage(item, displayPreferences, _apiClient, _imageManager, _sessionManager,
                                      _presentationManager, _navService, _playbackManager, _logger);

            page.CustomPageTitle = "TV Shows";

            page.ViewType = ViewType.Tv;
            page.CustomItemQuery = GetAllShows;

            await _navService.Navigate(page);
        }

        private Task<ItemsResult> GetAllShows(DisplayPreferences displayPreferences)
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

        private Task<ItemsResult> GetAllActors(DisplayPreferences displayPreferences)
        {
            var fields = FolderPage.QueryFields.ToList();
            fields.Remove(ItemFields.ItemCounts);
            fields.Remove(ItemFields.Overview);
            fields.Remove(ItemFields.DisplayPreferencesId);
            fields.Remove(ItemFields.DateCreated);

            var query = new PersonsQuery
            {
                Fields = fields.ToArray(),

                IncludeItemTypes = new[] { "Series", "Episode" },

                SortBy = !String.IsNullOrEmpty(displayPreferences.SortBy)
                             ? new[] { displayPreferences.SortBy }
                             : new[] { ItemSortBy.SortName },

                SortOrder = displayPreferences.SortOrder,

                Recursive = true,

                PersonTypes = new[] { PersonType.Actor, PersonType.GuestStar }
            };

            return _apiClient.GetPeopleAsync(query);
        }

        private Task<ItemsResult> GetNextUpAsync()
        {
            var query = new NextUpQuery
            {
                Fields = new[]
                        {
                            ItemFields.PrimaryImageAspectRatio,
                            ItemFields.DateCreated,
                            ItemFields.DisplayPreferencesId
                        },

                UserId = _sessionManager.CurrentUser.Id,

                Limit = 20
            };

            return _apiClient.GetNextUpAsync(query);
        }

        private Task<ItemsResult> GetResumeablesAsync()
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

                IncludeItemTypes = new[] { "Episode" },

                Filters = new[] { ItemFilter.IsResumable },

                Limit = 4,

                Recursive = true
            };

            return _apiClient.GetItemsAsync(query);
        }

        private Task<ItemsResult> GetLatestEpisodesAsync()
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

                SortBy = new[] { ItemSortBy.DateCreated },

                SortOrder = SortOrder.Descending,

                IncludeItemTypes = new[] { "Episode" },

                Filters = new[] { ItemFilter.IsUnplayed },

                Limit = 6,

                Recursive = true
            };

            return _apiClient.GetItemsAsync(query);
        }
    }
}
