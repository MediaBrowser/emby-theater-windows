using MediaBrowser.Model.ApiClient;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.Logging;
using MediaBrowser.Model.Querying;
using MediaBrowser.Plugins.DefaultTheme.Controls;
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

        public HomePageViewModel(IPresentationManager presentationManager, IApiClient apiClient, ISessionManager sessionManager, ILogger logger, IImageManager imageManager, INavigationService nav, IPlaybackManager playbackManager)
        {
            _presentationManager = presentationManager;
            _apiClient = apiClient;
            _sessionManager = sessionManager;
            _logger = logger;
            _imageManager = imageManager;
            _nav = nav;
            _playbackManager = playbackManager;
        }

        protected override async Task<IEnumerable<string>> GetSectionNames()
        {
            var views = new List<string>();

            try
            {
                var itemCounts = await _apiClient.GetItemCountsAsync(_sessionManager.CurrentUser.Id);

                if (itemCounts.MovieCount > 0 || itemCounts.TrailerCount > 0)
                {
                    views.Add("movies");
                }

                if (itemCounts.SeriesCount > 0 || itemCounts.EpisodeCount > 0)
                {
                    views.Add("tv");
                }

                //if (itemCounts.SongCount > 0)
                //{
                //    views.Add("music");
                //}
                //if (itemCounts.GameCount > 0)
                //{
                //    views.Add("games");
                //}
            }
            catch (Exception ex)
            {
                _logger.ErrorException("Error getting item counts", ex);
            }

            if (_presentationManager.GetApps(_sessionManager.CurrentUser).Any())
            {
                views.Add("apps");
            }

            views.Add("media collections");

            return views;
        }

        protected override BaseViewModel GetContentViewModel(string section)
        {
            if (string.Equals(section, "apps"))
            {
                return new AppListViewModel(_presentationManager, _sessionManager, _logger);
            }
            if (string.Equals(section, "media collections"))
            {
                var vm = new ItemListViewModel(GetMediaCollectionsAsync, _presentationManager, _imageManager, _apiClient, _sessionManager, _nav, _playbackManager, _logger)
                {
                    ImageDisplayWidth = 400,
                    ImageDisplayHeightGenerator = v => 225,
                    DisplayNameGenerator = MultiItemTile.GetDisplayName
                };

                return vm;

            }
            if (string.Equals(section, "games"))
            {
                return new GamesViewModel();
            }

            return null;
        }

        private Task<ItemsResult> GetMediaCollectionsAsync(DisplayPreferences displayPreferences)
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
    }
}
