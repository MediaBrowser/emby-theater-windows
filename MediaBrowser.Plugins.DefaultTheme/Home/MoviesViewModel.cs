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
using System.Threading.Tasks;

namespace MediaBrowser.Plugins.DefaultTheme.Home
{
    public class MoviesViewModel : BaseViewModel
    {
        private readonly IApiClient _apiClient;
        private readonly ISessionManager _sessionManager;

        public ItemListViewModel ResumeViewModel { get; private set; }
        public ItemListViewModel TrailersViewModel { get; private set; }

        public MoviesViewModel(IPresentationManager presentation, IImageManager imageManager, IApiClient apiClient, ISessionManager session, INavigationService nav, IPlaybackManager playback, ILogger logger)
        {
            _apiClient = apiClient;
            _sessionManager = session;

            ResumeViewModel = new ItemListViewModel(GetResumeablesAsync, presentation, imageManager, apiClient, session, nav, playback, logger)
            {
                ImageDisplayWidth = HomePageViewModel.TileWidth,
                ImageDisplayHeightGenerator = v => HomePageViewModel.TileHeight,
                DisplayNameGenerator = MultiItemTile.GetDisplayName,
                PreferredImageTypesGenerator = vm => new[] { ImageType.Backdrop, ImageType.Thumb, ImageType.Primary }
            };

            TrailersViewModel = new ItemListViewModel(GetTrailersAsync, presentation, imageManager, apiClient, session, nav, playback, logger)
            {
                ImageDisplayWidth = HomePageViewModel.TileHeight * 2 / 3,
                ImageDisplayHeightGenerator = v => HomePageViewModel.TileHeight,
                DisplayNameGenerator = MultiItemTile.GetDisplayName,
                PreferredImageTypesGenerator = vm => new[] { ImageType.Primary }
            };
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

                IncludeItemTypes = new[] { "Movie" },

                Filters = new[] { ItemFilter.IsResumable },

                Limit = 6,

                Recursive = true
            };

            return _apiClient.GetItemsAsync(query);
        }

        private Task<ItemsResult> GetTrailersAsync()
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

                IncludeItemTypes = new[] { "Trailer" },

                Filters = new[] { ItemFilter.IsUnplayed },

                Limit = 9,

                Recursive = true
            };

            return _apiClient.GetItemsAsync(query);
        }
    }
}
