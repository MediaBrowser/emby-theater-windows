using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediaBrowser.Model.ApiClient;
using MediaBrowser.Model.Dto;
using MediaBrowser.Model.Querying;
using MediaBrowser.Theater.Api.Navigation;
using MediaBrowser.Theater.Api.Playback;
using MediaBrowser.Theater.Api.Session;
using MediaBrowser.Theater.Api.UserInterface;

namespace MediaBrowser.Theater.DefaultTheme.ItemDetails.ViewModels
{
    public class PersonWorkSectionGenerator
        : BaseItemsListSectionGenerator
    {
        private readonly IConnectionManager _connectionManager;
        private readonly ISessionManager _sessionManager;

        public PersonWorkSectionGenerator(IConnectionManager connectionManager, ISessionManager sessionManager, IImageManager imageManager, INavigator navigator, IPlaybackManager playbackManager) 
            : base(connectionManager, sessionManager, imageManager, navigator, playbackManager)
        {
            _connectionManager = connectionManager;
            _sessionManager = sessionManager;
        }

        public override bool HasSection(BaseItemDto item)
        {
            return item.Type == "Person";
        }

        public override async Task<IEnumerable<IItemDetailSection>> GetSections(BaseItemDto item)
        {
            return new[] {
                await GetMovies(item),
                await GetSeries(item)
            };
        }

        private async Task<IItemDetailSection> GetMovies(BaseItemDto person)
        {
            var query = new ItemQuery { Person = person.Name, IncludeItemTypes = new[] { "Movie" }, UserId = _sessionManager.CurrentUser.Id, Recursive = true };
            var apiClient = _connectionManager.GetApiClient(person);
            return await GetItemsSection(await apiClient.GetItemsAsync(query));
        }

        private async Task<IItemDetailSection> GetSeries(BaseItemDto person)
        {
            var query = new ItemQuery { Person = person.Name, IncludeItemTypes = new[] { "Series" }, UserId = _sessionManager.CurrentUser.Id, Recursive = true };
            var apiClient = _connectionManager.GetApiClient(person);
            return await GetItemsSection(await apiClient.GetItemsAsync(query));
        }
    }
}
