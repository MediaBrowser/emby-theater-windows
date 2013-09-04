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
    public class TvViewModel : BaseViewModel
    {
        private readonly IApiClient _apiClient;
        private readonly ISessionManager _sessionManager;

        public ItemListViewModel NextUpViewModel { get; private set; }
        public ItemListViewModel ResumeViewModel { get; private set; }

        public TvViewModel(IPresentationManager presentation, IImageManager imageManager, IApiClient apiClient, ISessionManager session, INavigationService nav, IPlaybackManager playback, ILogger logger)
        {
            _apiClient = apiClient;
            _sessionManager = session;

            NextUpViewModel = new ItemListViewModel(GetNextUpAsync, presentation, imageManager, apiClient, session, nav, playback, logger)
            {
                ImageDisplayWidth = HomePageViewModel.TileWidth,
                ImageDisplayHeightGenerator = v => HomePageViewModel.TileHeight,
                DisplayNameGenerator = MultiItemTile.GetDisplayName
            };

            ResumeViewModel = new ItemListViewModel(GetResumeablesAsync, presentation, imageManager, apiClient, session, nav, playback, logger)
            {
                ImageDisplayWidth = HomePageViewModel.TileWidth,
                ImageDisplayHeightGenerator = v => HomePageViewModel.TileHeight,
                DisplayNameGenerator = MultiItemTile.GetDisplayName
            };
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

                Limit = 15
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

                Limit = 6,

                Recursive = true
            };

            return _apiClient.GetItemsAsync(query);
        }
    }
}
