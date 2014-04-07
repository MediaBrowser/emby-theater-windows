using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediaBrowser.Model.ApiClient;
using MediaBrowser.Model.Dto;
using MediaBrowser.Model.Querying;
using MediaBrowser.Theater.Api.Navigation;
using MediaBrowser.Theater.Api.Session;
using MediaBrowser.Theater.Api.UserInterface;

namespace MediaBrowser.Theater.DefaultTheme.ItemDetails.ViewModels
{
    public class PersonWorkSectionGenerator
        : BaseItemsListSectionGenerator
    {
        private readonly IApiClient _apiClient;
        private readonly ISessionManager _sessionManager;

        public PersonWorkSectionGenerator(IApiClient apiClient, ISessionManager sessionManager, IImageManager imageManager, INavigator navigator, IServerEvents serverEvents) 
            : base(apiClient, sessionManager, imageManager, navigator, serverEvents)
        {
            _apiClient = apiClient;
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
            return await GetItemsSection(await _apiClient.GetItemsAsync(query));
        }

        private async Task<IItemDetailSection> GetSeries(BaseItemDto person)
        {
            var query = new ItemQuery { Person = person.Name, IncludeItemTypes = new[] { "Series" }, UserId = _sessionManager.CurrentUser.Id, Recursive = true };
            return await GetItemsSection(await _apiClient.GetItemsAsync(query));
        }
    }
}
