using MediaBrowser.Model.ApiClient;
using MediaBrowser.Model.Dto;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace MediaBrowser.Plugins.DefaultTheme.Home
{
    public class TvView
    {
        public BaseItemDto[] SpotlightItems { get; set; }
        public ItemStub[] ShowsItems { get; set; }
        public ItemStub[] ActorItems { get; set; }
    }

    public class MovieView
    {
        public BaseItemDto[] SpotlightItems { get; set; }
        public ItemStub[] MovieItems { get; set; }
        public ItemStub[] PeopleItems { get; set; }

        public ItemStub[] BoxSetItems { get; set; }
        public ItemStub[] TrailerItems { get; set; }
        public ItemStub[] HDItems { get; set; }
        public ItemStub[] ThreeDItems { get; set; }
        public double HDMoviePercentage { get; set; }

        public ItemStub[] FamilyMovies { get; set; }

        public double FamilyMoviePercentage { get; set; }
    }
    
    public class ItemStub
    {
        public string Name { get; set; }
        public string Id { get; set; }
        public Guid ImageTag { get; set; }
    }

    public static class ApiClientExtensions
    {
        public static Task<TvView> GetTvView(this IApiClient apiClient, string userId, CancellationToken cancellationToken)
        {
            return apiClient.GetAsync<TvView>(apiClient.GetApiUrl("MBT/DefaultTheme/TV?userId=" + userId), cancellationToken);
        }

        public static Task<MovieView> GetMovieView(this IApiClient apiClient, string userId, CancellationToken cancellationToken)
        {
            return apiClient.GetAsync<MovieView>(apiClient.GetApiUrl("MBT/DefaultTheme/Movies?familyrating=pg&userId=" + userId), cancellationToken);
        }
    }
}
