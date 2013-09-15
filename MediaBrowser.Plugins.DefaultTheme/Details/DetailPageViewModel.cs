using MediaBrowser.Model.ApiClient;
using MediaBrowser.Model.Dto;
using MediaBrowser.Model.Logging;
using MediaBrowser.Model.Querying;
using MediaBrowser.Theater.Interfaces.Navigation;
using MediaBrowser.Theater.Interfaces.Playback;
using MediaBrowser.Theater.Interfaces.Presentation;
using MediaBrowser.Theater.Interfaces.Session;
using MediaBrowser.Theater.Interfaces.ViewModels;
using MediaBrowser.Theater.Presentation.ViewModels;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace MediaBrowser.Plugins.DefaultTheme.Details
{
    public class DetailPageViewModel : TabbedViewModel
    {
        private readonly IApiClient _apiClient;
        private readonly ISessionManager _sessionManager;
        private readonly IImageManager _imageManager;
        private readonly IPresentationManager _presentationManager;
        private readonly IPlaybackManager _playback;
        private readonly INavigationService _navigation;
        private readonly ILogger _logger;

        private ItemViewModel _itemViewModel;
        public ItemViewModel ItemViewModel
        {
            get
            {
                return _itemViewModel;
            }
            set
            {
                _itemViewModel = value;
                OnPropertyChanged("ItemViewModel");
            }
        }

        public DetailPageViewModel(ItemViewModel item, IApiClient apiClient, ISessionManager sessionManager, IImageManager imageManager, IPresentationManager presentationManager, IPlaybackManager playback, INavigationService navigation, ILogger logger)
        {
            _apiClient = apiClient;
            _sessionManager = sessionManager;
            _imageManager = imageManager;
            _presentationManager = presentationManager;
            _playback = playback;
            _navigation = navigation;
            _logger = logger;
            ItemViewModel = item;
        }

        protected override async Task<IEnumerable<string>> GetSectionNames()
        {
            var item = ItemViewModel;

            var themeMediaTask = GetThemeMedia(item.Item);
            var criticReviewsTask = GetCriticReviews(item.Item);

            await Task.WhenAll(themeMediaTask, criticReviewsTask);

            return GetMenuList(item.Item, themeMediaTask.Result, criticReviewsTask.Result);
        }

        private IEnumerable<string> GetMenuList(BaseItemDto item, AllThemeMediaResult themeMediaResult, ItemReviewsResult reviewsResult)
        {
            var views = new List<string>
                {
                    "overview"
                };

            if (item.ChildCount > 0)
            {
                if (item.IsType("series"))
                {
                    views.Add("seasons");
                }
                else if (item.IsType("season"))
                {
                    views.Add("episodes");
                }
                else if (item.IsType("musicalbum"))
                {
                    views.Add("songs");
                }
            }

            if (item.People.Length > 0)
            {
                views.Add("cast");
            }

            if (item.LocalTrailerCount > 1)
            {
                views.Add("trailers");
            }

            if (item.Chapters != null && item.Chapters.Count > 0)
            {
                views.Add("scenes");
            }

            if (item.SpecialFeatureCount > 0)
            {
                views.Add("special features");
            }

            if (item.IsType("movie") || item.IsType("trailer") || item.IsType("series") || item.IsType("musicalbum") || item.IsGame)
            {
                views.Add("similar");
            }

            if (reviewsResult.TotalRecordCount > 0 || !string.IsNullOrEmpty(item.CriticRatingSummary))
            {
                views.Add("reviews");
            }

            if (item.SoundtrackIds != null)
            {
                if (item.SoundtrackIds.Length > 1)
                {
                    views.Add("soundtracks");
                }
                else if (item.SoundtrackIds.Length > 0)
                {
                    views.Add("soundtrack");
                }
            }

            if (themeMediaResult.ThemeVideosResult.TotalRecordCount > 0 || themeMediaResult.ThemeSongsResult.TotalRecordCount > 0)
            {
                views.Add("themes");
            }

            if (GalleryViewModel.GetImages(item, _apiClient, null, null, true).Any())
            {
                views.Add("gallery");
            }

            return views;
        }

        protected override BaseViewModel GetContentViewModel(string section)
        {
            if (string.Equals(section, "overview"))
            {
                return _itemViewModel;
            }
            if (string.Equals(section, "reviews"))
            {
                return new CriticReviewListViewModel(_presentationManager, _apiClient, _imageManager, _itemViewModel.Item.Id);
            }
            if (string.Equals(section, "scenes"))
            {
                return new ChapterInfoListViewModel(_apiClient, _imageManager, _playback, _presentationManager)
                {
                    Item = _itemViewModel.Item,
                    ImageWidth = 380
                };
            }
            if (string.Equals(section, "cast"))
            {
                return new ItemPersonListViewModel(_apiClient, _imageManager, _presentationManager, _navigation)
                {
                    Item = _itemViewModel.Item,
                    ImageWidth = 300
                };
            }
            if (string.Equals(section, "gallery"))
            {
                const int imageHeight = 640;

                var vm = new GalleryViewModel(_apiClient, _imageManager, _navigation)
                {
                    ImageHeight = imageHeight,
                    Item = _itemViewModel.Item
                };

                var imageUrls = GalleryViewModel.GetImages(_itemViewModel.Item, _apiClient, null, imageHeight, true);

                vm.AddImages(imageUrls);

                return vm;
            }
            if (string.Equals(section, "similar"))
            {
                return new ItemListViewModel(GetSimilarItemsAsync, _presentationManager, _imageManager, _apiClient, _sessionManager, _navigation, _playback, _logger)
                {
                    ImageDisplayWidth = 300,
                    EnableBackdropsForCurrentItem = false
                };
            }
            if (string.Equals(section, "special features"))
            {
                return new ItemListViewModel(GetSpecialFeatures, _presentationManager, _imageManager, _apiClient, _sessionManager, _navigation, _playback, _logger)
                {
                    ImageDisplayWidth = 576,
                    EnableBackdropsForCurrentItem = false
                };
            }
            if (string.Equals(section, "themes"))
            {
                return new ItemListViewModel(GetConvertedThemeMediaResult, _presentationManager, _imageManager, _apiClient, _sessionManager, _navigation, _playback, _logger)
                {
                    ImageDisplayWidth = 576,
                    EnableBackdropsForCurrentItem = false
                };
            }
            if (string.Equals(section, "soundtrack") || string.Equals(section, "soundtracks"))
            {
                return new ItemListViewModel(GetSoundtracks, _presentationManager, _imageManager, _apiClient, _sessionManager, _navigation, _playback, _logger)
                {
                    ImageDisplayWidth = 400,
                    EnableBackdropsForCurrentItem = false
                };
            }
            if (string.Equals(section, "seasons") || string.Equals(section, "episodes") || string.Equals(section, "songs"))
            {
                return new ItemListViewModel(GetChildren, _presentationManager, _imageManager, _apiClient, _sessionManager, _navigation, _playback, _logger)
                {
                    ImageDisplayWidth = 300,
                    EnableBackdropsForCurrentItem = false
                };
            }
            if (string.Equals(section, "trailers"))
            {
                return new ItemListViewModel(GetTrailers, _presentationManager, _imageManager, _apiClient, _sessionManager, _navigation, _playback, _logger)
                {
                    ImageDisplayWidth = 384,
                    EnableBackdropsForCurrentItem = false
                };
            }

            return null;
        }

        private async Task<ItemReviewsResult> GetCriticReviews(BaseItemDto item)
        {
            if (item.IsPerson || item.IsStudio || item.IsArtist || item.IsGameGenre || item.IsMusicGenre || item.IsGenre)
            {
                return new ItemReviewsResult();
            }

            try
            {
                return await _apiClient.GetCriticReviews(item.Id, CancellationToken.None);
            }
            catch
            {
                // Logged at lower levels
                return new ItemReviewsResult();
            }
        }

        private async Task<AllThemeMediaResult> GetThemeMedia(BaseItemDto item)
        {
            if (item.IsPerson || item.IsStudio || item.IsArtist || item.IsGameGenre || item.IsMusicGenre || item.IsGenre)
            {
                return new AllThemeMediaResult();
            }

            try
            {
                return await _apiClient.GetAllThemeMediaAsync(_sessionManager.CurrentUser.Id, item.Id, false, CancellationToken.None);
            }
            catch
            {
                // Logged at lower levels
                return new AllThemeMediaResult();
            }
        }

        private async Task<ItemsResult> GetConvertedThemeMediaResult()
        {
            var allThemeMedia = await GetThemeMedia(ItemViewModel.Item);

            return new ItemsResult
            {
                TotalRecordCount = allThemeMedia.ThemeSongsResult.TotalRecordCount + allThemeMedia.ThemeVideosResult.TotalRecordCount,
                Items = allThemeMedia.ThemeVideosResult.Items.Concat(allThemeMedia.ThemeSongsResult.Items).ToArray()
            };
        }

        private Task<ItemsResult> GetSimilarItemsAsync()
        {
            var item = ItemViewModel.Item;

            var query = new SimilarItemsQuery
            {
                UserId = _sessionManager.CurrentUser.Id,
                Limit = item.IsGame || item.IsType("musicalbum") ? 6 : 12,
                Fields = new[]
                        {
                                 ItemFields.PrimaryImageAspectRatio,
                                 ItemFields.DateCreated
                        },
                Id = item.Id
            };

            if (item.IsType("trailer"))
            {
                return _apiClient.GetSimilarTrailersAsync(query);
            }
            if (item.IsGame)
            {
                return _apiClient.GetSimilarGamesAsync(query);
            }
            if (item.IsType("musicalbum"))
            {
                return _apiClient.GetSimilarAlbumsAsync(query);
            }
            if (item.IsType("series"))
            {
                return _apiClient.GetSimilarSeriesAsync(query);
            }

            return _apiClient.GetSimilarMoviesAsync(query);
        }

        private Task<ItemsResult> GetSoundtracks()
        {
            var item = ItemViewModel.Item;
     
            var query = new ItemQuery
            {
                UserId = _sessionManager.CurrentUser.Id,
                Fields = new[]
                        {
                                 ItemFields.PrimaryImageAspectRatio,
                                 ItemFields.DateCreated
                        },
                Ids = item.SoundtrackIds,
                SortBy = new[] { ItemSortBy.SortName }
            };

            return _apiClient.GetItemsAsync(query);
        }

        private async Task<ItemsResult> GetTrailers()
        {
            var item = ItemViewModel.Item;

            var items = await _apiClient.GetLocalTrailersAsync(_sessionManager.CurrentUser.Id, item.Id);

            return new ItemsResult
            {
                Items = items,
                TotalRecordCount = items.Length
            };
        }

        private Task<ItemsResult> GetChildren()
        {
            var item = ItemViewModel.Item;

            var query = new ItemQuery
            {
                UserId = _sessionManager.CurrentUser.Id,
                Fields = new[]
                        {
                                 ItemFields.PrimaryImageAspectRatio,
                                 ItemFields.DateCreated
                        },
                ParentId = item.Id,
                SortBy = new[] { ItemSortBy.SortName },

                MinIndexNumber = item.IsType("Series") ? 1 : (int?)null
            };

            return _apiClient.GetItemsAsync(query);
        }

        private async Task<ItemsResult> GetSpecialFeatures()
        {
            var item = ItemViewModel.Item;

            var items = await _apiClient.GetSpecialFeaturesAsync(_sessionManager.CurrentUser.Id, item.Id);

            return new ItemsResult
            {
                TotalRecordCount = items.Length,
                Items = items
            };
        }
    }
}
