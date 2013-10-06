using MediaBrowser.Model.ApiClient;
using MediaBrowser.Model.Dto;
using MediaBrowser.Model.Entities;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace MediaBrowser.Plugins.DefaultTheme.Home
{
    public class ItemStub
    {
        public string Name { get; set; }
        public string Id { get; set; }
        public Guid ImageTag { get; set; }
        public ImageType ImageType { get; set; }
    }

    public class MoviesView
    {
        public BaseItemDto[] SpotlightItems { get; set; }
        public ItemStub[] MovieItems { get; set; }
        public ItemStub[] PeopleItems { get; set; }

        public ItemStub[] BoxSetItems { get; set; }
        public ItemStub[] TrailerItems { get; set; }
        public ItemStub[] HDItems { get; set; }
        public ItemStub[] ThreeDItems { get; set; }

        public ItemStub[] FamilyMovies { get; set; }

        public ItemStub[] RomanceItems { get; set; }
        public ItemStub[] ComedyItems { get; set; }

        public double FamilyMoviePercentage { get; set; }

        public double HDMoviePercentage { get; set; }
    }

    public class TvView
    {
        public BaseItemDto[] SpotlightItems { get; set; }
        public ItemStub[] ShowsItems { get; set; }
        public ItemStub[] ActorItems { get; set; }

        public ItemStub[] RomanceItems { get; set; }
        public ItemStub[] ComedyItems { get; set; }
    }

    public class GamesView
    {
        public BaseItemDto[] SpotlightItems { get; set; }
        public ItemStub[] MultiPlayerItems { get; set; }
    }
    
    public class HomeView
    {
        public BaseItemDto[] SpotlightItems { get; set; }
    }

    public static class ApiClientExtensions
    {
        public const string ComedyGenre = "comedy";
        public const string RomanceGenre = "romance";
        public const string FamilyGenre = "family";

        public static Task<TvView> GetTvView(this IApiClient apiClient, string userId, CancellationToken cancellationToken)
        {
            var url = apiClient.GetApiUrl("MBT/DefaultTheme/TV?userId=" + userId + "&ComedyGenre=" + ComedyGenre + "&RomanceGenre=" + RomanceGenre);

            return apiClient.GetAsync<TvView>(url, cancellationToken);
        }

        public static Task<MoviesView> GetMovieView(this IApiClient apiClient, string userId, CancellationToken cancellationToken)
        {
            var url = apiClient.GetApiUrl("MBT/DefaultTheme/Movies?familyrating=pg&userId=" + userId + "&ComedyGenre=" + ComedyGenre + "&RomanceGenre=" + RomanceGenre + "&FamilyGenre=" + FamilyGenre);

            return apiClient.GetAsync<MoviesView>(url, cancellationToken);
        }

        public static Task<HomeView> GetHomeView(this IApiClient apiClient, string userId, CancellationToken cancellationToken)
        {
            var url = apiClient.GetApiUrl("MBT/DefaultTheme/Home?userId=" + userId);

            return apiClient.GetAsync<HomeView>(url, cancellationToken);
        }

        public static Task<GamesView> GetGamesView(this IApiClient apiClient, string userId, CancellationToken cancellationToken)
        {
            var url = apiClient.GetApiUrl("MBT/DefaultTheme/Games?userId=" + userId);

            return apiClient.GetAsync<GamesView>(url, cancellationToken);
        }
    }
}
