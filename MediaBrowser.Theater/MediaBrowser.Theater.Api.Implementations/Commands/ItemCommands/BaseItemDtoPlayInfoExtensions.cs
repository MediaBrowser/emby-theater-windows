using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MediaBrowser.Model.ApiClient;
using MediaBrowser.Model.Dto;
using MediaBrowser.Model.Entities;
using MediaBrowser.Theater.Api.Library;
using MediaBrowser.Theater.Api.Session;
using MediaBrowser.Theater.Playback;

namespace MediaBrowser.Theater.Api.Commands.ItemCommands
{
    public static class BaseItemDtoPlayInfoExtensions
    {
        public static double GetPlayedPercent(this BaseItemDto item)
        {
            if (item == null)
            {
                return 0;
            }

            if (item.IsFolder)
            {
                return item.UserData.PlayedPercentage ?? 0;
            }

            if (item.RunTimeTicks.HasValue)
            {
                if (item.UserData != null && item.UserData.PlaybackPositionTicks > 0)
                {
                    if (item.UserData.PlaybackPositionTicks == item.RunTimeTicks)
                    {
                        return 100;
                    }

                    double percent = item.UserData.PlaybackPositionTicks / (double)item.RunTimeTicks.Value;
                    return percent * 100;
                }
            }

            return 0;
        }

        public static async Task<IList<BaseItemDto>> GetSmartPlayItems(this BaseItemDto item, IConnectionManager connectionManager, ISessionManager sessionManager)
        {
            var queryParams = new ChildrenQueryParams
            {
                Recursive = true,
                IncludeItemTypes = new[] { "Movie", "Episode", "Audio" },
                SortOrder = SortOrder.Ascending,
                SortBy = new[] { "SortName" }
            };

            if (item.IsType("Series") || item.IsType("Season") || item.IsType("BoxSet"))
            {
                var response = (await ItemChildren.Get(connectionManager, sessionManager, item, queryParams));
                var children = response.Items;

                int lastWatched = -1;
                for (int i = 0; i < children.Length; i++)
                {
                    var percent = children[i].GetPlayedPercent();
                    if (percent >= 100 || children[i].UserData.Played)
                    {
                        lastWatched = i;
                    }
                    else if (percent > 0)
                    {
                        lastWatched = i - 1;
                    }
                }

                var start = lastWatched + 1;
                if (start > 0 && start < children.Length - 1)
                {
                    children = children.Skip(start).ToArray();
                }

                return children;
            }

            if (item.IsFolder || item.IsGenre || item.IsPerson || item.IsStudio)
            {
                return (await ItemChildren.Get(connectionManager, sessionManager, item, queryParams)).Items;
            }

            return new List<BaseItemDto> { item };
        }

        public static async Task<IList<Media>> GetSmartPlayMedia(this BaseItemDto item, IConnectionManager connectionManager, ISessionManager sessionManager)
        {
            var items = await item.GetSmartPlayItems(connectionManager, sessionManager);
            return items.Take(1).Select(Media.Resume).Concat(items.Skip(1).Select(Media.Create)).ToList();
        }

        public static bool IsPlayable(this BaseItemDto item)
        {
            return !item.IsFolder && !item.IsGenre && !item.IsPerson && !item.IsStudio;
        }
    }
}