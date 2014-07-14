using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MediaBrowser.Model.ApiClient;
using MediaBrowser.Model.Dto;
using MediaBrowser.Model.Querying;
using MediaBrowser.Theater.Api;
using MediaBrowser.Theater.Api.Session;

namespace MediaBrowser.Theater.DefaultTheme.Home.ViewModels
{
    public interface IHomePageMediaFolderGenerator
    {
        Task<IEnumerable<IHomePage>> GetHomePages(BaseItemDto mediaFolder);
    }

    public class MediaFolderHomePages
        : IHomePageGenerator
    {
        private readonly IApiClient _apiClient;
        private readonly IEnumerable<IHomePageMediaFolderGenerator> _generators;
        private readonly ISessionManager _sessionManager;

        public MediaFolderHomePages(ITheaterApplicationHost appHost, IApiClient apiClient, ISessionManager sessionManager)
        {
            _apiClient = apiClient;
            _sessionManager = sessionManager;
            _generators = appHost.GetExports<IHomePageMediaFolderGenerator>();
        }

        public async Task<IEnumerable<IHomePage>> GetHomePages()
        {
            BaseItemDto root = await _apiClient.GetRootFolderAsync(_sessionManager.CurrentUser.Id);
            if (root.ChildCount == 0) {
                return Enumerable.Empty<IHomePage>();
            }

            ItemsResult mediaFolders = await _apiClient.GetItemsAsync(new ItemQuery {
                UserId = _sessionManager.CurrentUser.Id,
                ParentId = root.Id
            });

            var tasks = new List<Task<IEnumerable<IHomePage>>>();
            foreach (BaseItemDto folder in mediaFolders.Items.OrderBy(f => f.CollectionType)) {
                tasks.AddRange(_generators.Select(g => g.GetHomePages(folder)));
            }

            await Task.WhenAll(tasks);

            return tasks.SelectMany(t => t.Result);
        }
    }
}