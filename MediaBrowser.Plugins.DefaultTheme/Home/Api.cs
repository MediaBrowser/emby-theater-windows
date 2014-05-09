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
        public string ImageTag { get; set; }
        public ImageType ImageType { get; set; }
    }

    public class MoviesView : BaseView
    {
        public List<ItemStub> MovieItems { get; set; }

        public List<ItemStub> BoxSetItems { get; set; }
        public List<ItemStub> TrailerItems { get; set; }
        public List<ItemStub> HDItems { get; set; }
        public List<ItemStub> ThreeDItems { get; set; }

        public List<ItemStub> FamilyMovies { get; set; }

        public List<ItemStub> RomanceItems { get; set; }
        public List<ItemStub> ComedyItems { get; set; }

        public double FamilyMoviePercentage { get; set; }

        public double HDMoviePercentage { get; set; }

        public List<BaseItemDto> LatestTrailers { get; set; }
        public List<BaseItemDto> LatestMovies { get; set; }
    }

    public class TvView : BaseView
    {
        public List<ItemStub> ShowsItems { get; set; }

        public List<ItemStub> RomanceItems { get; set; }
        public List<ItemStub> ComedyItems { get; set; }

        public List<string> SeriesIdsInProgress { get; set; }

        public List<BaseItemDto> LatestEpisodes { get; set; }
        public List<BaseItemDto> NextUpEpisodes { get; set; }
        public List<BaseItemDto> ResumableEpisodes { get; set; }
    }

    public class ItemByNameInfo
    {
        public string Name { get; set; }
        public int ItemCount { get; set; }
    }

    public class GamesView : BaseView
    {
        public List<ItemStub> MultiPlayerItems { get; set; }
        public List<BaseItemDto> GameSystems { get; set; }
        public List<BaseItemDto> RecentlyPlayedGames { get; set; }
    }

    public class BaseView
    {
        public List<BaseItemDto> BackdropItems { get; set; }
        public List<BaseItemDto> SpotlightItems { get; set; }
        public List<BaseItemDto> MiniSpotlights { get; set; }
    }

    public class FavoritesView : BaseView
    {
        public List<BaseItemDto> Artists { get; set; }
        public List<BaseItemDto> Movies { get; set; }
        public List<BaseItemDto> Series { get; set; }
        public List<BaseItemDto> Episodes { get; set; }
        public List<BaseItemDto> Games { get; set; }
        public List<BaseItemDto> Books { get; set; }
        public List<BaseItemDto> Albums { get; set; }
        public List<BaseItemDto> Songs { get; set; }
    }

    public static class ApiClientExtensions
    {
        public const string ComedyGenre = "comedy";
        public const string RomanceGenre = "romance";
        public const string FamilyGenre = "family";

        public const double TopTvCommunityRating = 8.5;
        public const double TopMovieCommunityRating = 8.2;

        public static Task<TvView> GetTvView(this IApiClient apiClient, string userId, CancellationToken cancellationToken)
        {
            var url = apiClient.GetApiUrl("MBT/DefaultTheme/TV?userId=" + userId + "&ComedyGenre=" + ComedyGenre + "&RomanceGenre=" + RomanceGenre + "&TopCommunityRating=" + TopTvCommunityRating + "&NextUpEpisodeLimit=15&LatestEpisodeLimit=9&ResumableEpisodeLimit=3");

            return apiClient.GetAsync<TvView>(url, cancellationToken);
        }

        public static Task<MoviesView> GetMovieView(this IApiClient apiClient, string userId, CancellationToken cancellationToken)
        {
            var url = apiClient.GetApiUrl("MBT/DefaultTheme/Movies?familyrating=pg&userId=" + userId + "&ComedyGenre=" + ComedyGenre + "&RomanceGenre=" + RomanceGenre + "&FamilyGenre=" + FamilyGenre + "&LatestMoviesLimit=16&LatestTrailersLimit=6");

            return apiClient.GetAsync<MoviesView>(url, cancellationToken);
        }

        public static Task<FavoritesView> GetFavoritesView(this IApiClient apiClient, string userId, CancellationToken cancellationToken)
        {
            var url = apiClient.GetApiUrl("MBT/DefaultTheme/Favorites?userId=" + userId);

            return apiClient.GetAsync<FavoritesView>(url, cancellationToken);
        }

        public static Task<GamesView> GetGamesView(this IApiClient apiClient, string userId, CancellationToken cancellationToken)
        {
            var url = apiClient.GetApiUrl("MBT/DefaultTheme/Games?userId=" + userId + "&RecentlyPlayedGamesLimit=3");

            return apiClient.GetAsync<GamesView>(url, cancellationToken);
        }
    }
}
