using MediaBrowser.Model.ApiClient;
using MediaBrowser.Model.Dto;
using MediaBrowser.Model.Entities;
using System;
using System.Collections.Generic;
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

    public class MoviesView : BaseView
    {
        public List<ItemStub> MovieItems { get; set; }
        public List<ItemStub> PeopleItems { get; set; }

        public List<ItemStub> BoxSetItems { get; set; }
        public List<ItemStub> TrailerItems { get; set; }
        public List<ItemStub> HDItems { get; set; }
        public List<ItemStub> ThreeDItems { get; set; }

        public List<ItemStub> FamilyMovies { get; set; }

        public List<ItemStub> RomanceItems { get; set; }
        public List<ItemStub> ComedyItems { get; set; }

        public double FamilyMoviePercentage { get; set; }

        public double HDMoviePercentage { get; set; }
    }

    public class TvView : BaseView
    {
        public List<ItemStub> ShowsItems { get; set; }
        public List<ItemStub> ActorItems { get; set; }

        public List<ItemStub> RomanceItems { get; set; }
        public List<ItemStub> ComedyItems { get; set; }
    }

    public class GamesView : BaseView
    {
        public List<ItemStub> MultiPlayerItems { get; set; }

    }

    public class BaseView
    {
        public List<BaseItemDto> BackdropItems { get; set; }
        public List<BaseItemDto> SpotlightItems { get; set; }
        public List<BaseItemDto> MiniSpotlights { get; set; }
    }

    public class HomeView
    {
        public List<BaseItemDto> SpotlightItems { get; set; }
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
