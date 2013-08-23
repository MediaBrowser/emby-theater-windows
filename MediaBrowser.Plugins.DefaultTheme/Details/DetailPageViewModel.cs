using MediaBrowser.Model.ApiClient;
using MediaBrowser.Model.Dto;
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

        public DetailPageViewModel(ItemViewModel item, IApiClient apiClient, ISessionManager sessionManager, IImageManager imageManager, IPresentationManager presentationManager, IPlaybackManager playback, INavigationService navigation)
        {
            _apiClient = apiClient;
            _sessionManager = sessionManager;
            _imageManager = imageManager;
            _presentationManager = presentationManager;
            _playback = playback;
            _navigation = navigation;
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

            if (GalleryViewModel.GetImages(item, _apiClient, null, null).Any())
            {
                views.Add("gallery");
            }

            return views;
        }

        protected override BaseViewModel GetContentViewModel(string section)
        {
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
                return new GalleryViewModel(_apiClient, _imageManager, _navigation)
                {
                    ImageHeight = 640,
                    Item = _itemViewModel.Item
                };
            }
            //if (string.Equals(section, "similar"))
            //{
            //    return new ItemListViewModel(_apiClient, _imageManager, _navigation)
            //    {
            //        ImageHeight = 640,
            //        Item = _itemViewModel.Item
            //    };
            //}
            if (string.Equals(section, "overview"))
            {
                return _itemViewModel;
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
    }
}
