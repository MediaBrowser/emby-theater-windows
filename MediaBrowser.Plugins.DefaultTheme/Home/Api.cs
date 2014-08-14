using MediaBrowser.Model.ApiClient;
using MediaBrowser.Model.Dto;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.Querying;
using System;
using System.Collections.Generic;
using System.Linq;
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

        public List<BaseItemDto> LatestTrailers { get; set; }
        public List<BaseItemDto> LatestMovies { get; set; }
    }

    public class TvView : BaseView
    {
        public List<ItemStub> ShowsItems { get; set; }

        public List<BaseItemDto> LatestEpisodes { get; set; }
        public List<BaseItemDto> NextUpEpisodes { get; set; }
        public List<BaseItemDto> ResumableEpisodes { get; set; }
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

    public static class ApiClientExtensions
    {
        public const string ComedyGenre = "comedy";
        public const string RomanceGenre = "romance";
        public const string FamilyGenre = "family";

        public const double TopTvCommunityRating = 8.5;
        public const double TopMovieCommunityRating = 8.2;

        public static async Task<TvView> GetTvView(this IApiClient apiClient, string userId, string parentId, CancellationToken cancellationToken)
        {
            var allShowsItemsTask = apiClient.GetItemsAsync(new ItemQuery
            {
                IncludeItemTypes = new[] { "Series" },
                ImageTypes = new[] { ImageType.Backdrop },
                SortBy = new[] { ItemSortBy.Random },
                UserId = userId,
                Recursive = true,
                Limit = 1,
                ParentId = parentId

            }, cancellationToken);

            var nextUpItemsTask = apiClient.GetNextUpEpisodesAsync(new NextUpQuery
            {
                UserId = userId,
                Limit = 15,
                ParentId = parentId

            }, cancellationToken);

            var latestEpisodesTask = apiClient.GetItemsAsync(new ItemQuery
            {
                IncludeItemTypes = new[] { "Episode" },
                ImageTypes = new[] { ImageType.Primary },
                SortBy = new[] { ItemSortBy.DateCreated },
                SortOrder = SortOrder.Descending,
                IsPlayed = false,
                UserId = userId,
                Limit = 9,
                Recursive = true,
                IsUnaired = false,
                IsMissing = false,
                ParentId = parentId

            }, cancellationToken);

            var resumableEpisodesTask = apiClient.GetItemsAsync(new ItemQuery
            {
                IncludeItemTypes = new[] { "Episode" },
                ImageTypes = new[] { ImageType.Primary },
                SortBy = new[] { ItemSortBy.DatePlayed },
                SortOrder = SortOrder.Descending,
                Filters = new[] { ItemFilter.IsResumable },
                UserId = userId,
                Limit = 3,
                Recursive = true,
                ParentId = parentId

            }, cancellationToken);

            var backdropItemsTask = apiClient.GetItemsAsync(new ItemQuery
            {
                IncludeItemTypes = new[] { "Series" },
                ImageTypes = new[] { ImageType.Backdrop },
                SortBy = new[] { ItemSortBy.Random },
                UserId = userId,
                Limit = 60,
                Recursive = true,
                ParentId = parentId

            }, cancellationToken);

            await Task.WhenAll(allShowsItemsTask, nextUpItemsTask, latestEpisodesTask, resumableEpisodesTask, backdropItemsTask);

            return new TvView
            {
                ShowsItems = allShowsItemsTask.Result.Items.Select(GetStub).ToList(),
                NextUpEpisodes = nextUpItemsTask.Result.Items.ToList(),
                LatestEpisodes = latestEpisodesTask.Result.Items.ToList(),
                ResumableEpisodes = resumableEpisodesTask.Result.Items.ToList(),
                BackdropItems = backdropItemsTask.Result.Items.ToList(),
                MiniSpotlights = backdropItemsTask.Result.Items.OrderBy(i => Guid.NewGuid()).ToList(),
                SpotlightItems = backdropItemsTask.Result.Items.OrderBy(i => Guid.NewGuid()).ToList()
            };
        }

        public static async Task<GamesView> GetGamesView(this IApiClient apiClient, string userId, string parentId, CancellationToken cancellationToken)
        {
            var multiPlayerItemsTask = apiClient.GetItemsAsync(new ItemQuery
            {
                MinPlayers = 2,
                MediaTypes = new[] { MediaType.Game },
                ImageTypes = new[] { ImageType.Primary },
                SortBy = new[] { ItemSortBy.Random },
                UserId = userId,
                Recursive = true,
                Limit = 1,
                ParentId = parentId

            }, cancellationToken);

            var gameSystemTask = apiClient.GetItemsAsync(new ItemQuery
            {
                IncludeItemTypes = new[] { "GameSystem" },
                SortBy = new[] { ItemSortBy.SortName },
                UserId = userId,
                Recursive = true,
                ParentId = parentId

            }, cancellationToken);

            var recentlyPlayedGamesTask = apiClient.GetItemsAsync(new ItemQuery
            {
                MediaTypes = new[] { MediaType.Game },
                ImageTypes = new[] { ImageType.Primary },
                SortBy = new[] { ItemSortBy.DatePlayed },
                SortOrder = SortOrder.Descending,
                IsPlayed = true,
                UserId = userId,
                Limit = 3,
                Recursive = true,
                ParentId = parentId

            }, cancellationToken);

            var backdropItemsTask = apiClient.GetItemsAsync(new ItemQuery
            {
                MediaTypes = new[] { MediaType.Game },
                ImageTypes = new[] { ImageType.Backdrop },
                SortBy = new[] { ItemSortBy.Random },
                UserId = userId,
                Limit = 60,
                Recursive = true,
                ParentId = parentId

            }, cancellationToken);

            await Task.WhenAll(multiPlayerItemsTask, gameSystemTask, recentlyPlayedGamesTask, backdropItemsTask);

            return new GamesView
            {
                GameSystems = gameSystemTask.Result.Items.ToList(),
                RecentlyPlayedGames = recentlyPlayedGamesTask.Result.Items.ToList(),
                MultiPlayerItems = multiPlayerItemsTask.Result.Items.Select(GetStub).ToList(),

                BackdropItems = backdropItemsTask.Result.Items.ToList(),

                MiniSpotlights = backdropItemsTask.Result.Items.OrderBy(i => Guid.NewGuid()).ToList(),

                SpotlightItems = backdropItemsTask.Result.Items.OrderBy(i => Guid.NewGuid()).ToList()
            };
        }

        public static async Task<MoviesView> GetMovieView(this IApiClient apiClient, string userId, string parentId, CancellationToken cancellationToken)
        {
            var url = apiClient.GetApiUrl("MBT/DefaultTheme/Movies?familyrating=pg&userId=" + userId + "&ComedyGenre=" + ComedyGenre + "&RomanceGenre=" + RomanceGenre + "&FamilyGenre=" + FamilyGenre + "&LatestMoviesLimit=16&LatestTrailersLimit=6");

            var threeDItemsTask = apiClient.GetItemsAsync(new ItemQuery
            {
                IncludeItemTypes = new[] { "Movie" },
                SortBy = new[] { ItemSortBy.Random },
                UserId = userId,
                Limit = 1,
                Recursive = true,
                Is3D = true,
                ParentId = parentId

            }, cancellationToken);

            var familyItemsTask = apiClient.GetItemsAsync(new ItemQuery
            {
                IncludeItemTypes = new[] { "Movie" },
                SortBy = new[] { ItemSortBy.Random },
                UserId = userId,
                Limit = 1,
                Recursive = true,
                Genres = new[] { "Family" },
                ParentId = parentId

            }, cancellationToken);

            var hdItemsTask = apiClient.GetItemsAsync(new ItemQuery
            {
                IncludeItemTypes = new[] { "Movie" },
                SortBy = new[] { ItemSortBy.Random },
                UserId = userId,
                Limit = 1,
                Recursive = true,
                IsHD = true,
                ParentId = parentId

            }, cancellationToken);

            var movieItemsTask = apiClient.GetItemsAsync(new ItemQuery
            {
                IncludeItemTypes = new[] { "Movie" },
                SortBy = new[] { ItemSortBy.Random },
                UserId = userId,
                Limit = 1,
                Recursive = true,
                ParentId = parentId

            }, cancellationToken);

            var boxsetItemsTask = apiClient.GetItemsAsync(new ItemQuery
            {
                IncludeItemTypes = new[] { "BoxSet" },
                SortBy = new[] { ItemSortBy.Random },
                UserId = userId,
                Limit = 1,
                Recursive = true,
                ParentId = parentId

            }, cancellationToken);

            var latestMoviesTask = apiClient.GetItemsAsync(new ItemQuery
            {
                IncludeItemTypes = new[] { "Movie" },
                SortBy = new[] { ItemSortBy.DateCreated },
                SortOrder = SortOrder.Descending,
                IsPlayed = false,
                UserId = userId,
                Limit = 16,
                Recursive = true,
                ParentId = parentId

            }, cancellationToken);

            var backdropItemsTask = apiClient.GetItemsAsync(new ItemQuery
            {
                IncludeItemTypes = new[] { "Movie" },
                ImageTypes = new[] { ImageType.Backdrop },
                SortBy = new[] { ItemSortBy.Random },
                UserId = userId,
                Limit = 60,
                Recursive = true,
                ParentId = parentId

            }, cancellationToken);

            var resumableTask = apiClient.GetItemsAsync(new ItemQuery
            {
                IncludeItemTypes = new[] { "Movie" },
                ImageTypes = new[] { ImageType.Primary },
                SortBy = new[] { ItemSortBy.DatePlayed },
                SortOrder = SortOrder.Descending,
                Filters = new[] { ItemFilter.IsResumable },
                UserId = userId,
                Limit = 3,
                Recursive = true,
                ParentId = parentId

            }, cancellationToken);

            await Task.WhenAll(threeDItemsTask, familyItemsTask, movieItemsTask, hdItemsTask, boxsetItemsTask, latestMoviesTask, backdropItemsTask, resumableTask);

            return new MoviesView
            {
                BoxSetItems = boxsetItemsTask.Result.Items.Select(GetStub).ToList(),
                FamilyMovies = familyItemsTask.Result.Items.Select(GetStub).ToList(),
                ThreeDItems = threeDItemsTask.Result.Items.Select(GetStub).ToList(),
                MovieItems = movieItemsTask.Result.Items.Select(GetStub).ToList(),
                HDItems = hdItemsTask.Result.Items.Select(GetStub).ToList(),
                LatestMovies = latestMoviesTask.Result.Items.ToList(),
                BackdropItems = backdropItemsTask.Result.Items.ToList(),
                MiniSpotlights = resumableTask.Result.Items.OrderBy(i => Guid.NewGuid()).ToList(),
                SpotlightItems = backdropItemsTask.Result.Items.OrderBy(i => Guid.NewGuid()).ToList(),
                TrailerItems = new List<ItemStub>(),
                LatestTrailers = new List<BaseItemDto>()
            };
        }

        private static ItemStub GetStub(BaseItemDto item)
        {
            var stub = new ItemStub();

            stub.Id = item.Id;
            stub.Name = item.Name;
            stub.ImageType = ImageType.Primary;
            stub.ImageTag = item.ImageTags.ContainsKey(ImageType.Primary) ? item.ImageTags[ImageType.Primary] : null;

            return stub;
        }
    }
}
