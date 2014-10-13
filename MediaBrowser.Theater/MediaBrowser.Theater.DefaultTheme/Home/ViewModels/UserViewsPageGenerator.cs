using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MediaBrowser.Model.ApiClient;
using MediaBrowser.Model.Channels;
using MediaBrowser.Model.Dto;
using MediaBrowser.Model.Querying;
using MediaBrowser.Theater.Api;
using MediaBrowser.Theater.Api.Session;

namespace MediaBrowser.Theater.DefaultTheme.Home.ViewModels
{
    public interface IUserViewHomePageGenerator
    {
        string CollectionType { get; }

        Task<IEnumerable<IHomePage>> GetHomePages(BaseItemDto view);
    }

    public class UserViewsPageGenerator
        : IHomePageGenerator
    {
        private readonly ISessionManager _sessionManager;
        private readonly IEnumerable<IUserViewHomePageGenerator> _generators;

        public UserViewsPageGenerator(ISessionManager sessionManager, ITheaterApplicationHost appHost)
        {
            _sessionManager = sessionManager;
            _generators = appHost.GetExports<IUserViewHomePageGenerator>();
        }

        public async Task<IEnumerable<IHomePage>> GetHomePages()
        {
            var apiClient = _sessionManager.ActiveApiClient; // todo enumerate over all connections
            var views = await apiClient.GetUserViews(_sessionManager.CurrentUser.Id, CancellationToken.None);

            var tasks = new List<Task<IEnumerable<IHomePage>>>();

            foreach (var view in views.Items) {
                var generators = _generators.Where(g => string.Equals(view.CollectionType, g.CollectionType, StringComparison.OrdinalIgnoreCase)).ToList();

                if (generators.Count == 0) {
                    generators = _generators.Where(g => g.CollectionType == null).ToList();
                }

                tasks.AddRange(generators.Select(g => g.GetHomePages(view)));
            }

            await Task.WhenAll(tasks);
            return tasks.SelectMany(t => t.Result);
        }
    }
}
