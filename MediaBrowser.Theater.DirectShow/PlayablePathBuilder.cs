using MediaBrowser.Model.ApiClient;
using MediaBrowser.Model.Dto;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.IO;
using System;
using System.IO;
using System.Linq;

namespace MediaBrowser.Theater.DirectShow
{
    /// <summary>
    /// Class PlayablePathBuilder
    /// </summary>
    public static class PlayablePathBuilder
    {
        /// <summary>
        /// Gets the playable path.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <param name="isoMount">The iso mount.</param>
        /// <param name="apiClient">The API client.</param>
        /// <returns>System.String.</returns>
        public static string GetPlayablePath(BaseItemDto item, IIsoMount isoMount, IApiClient apiClient)
        {
            if (item.LocationType == LocationType.Remote)
            {
                return apiClient.GetVideoStreamUrl(new VideoStreamOptions
                {
                    ItemId = item.Id,
                    AudioCodec = AudioCodecs.Copy,
                    VideoCodec = VideoCodecs.Copy,
                    OutputFileExtension = ".mp4"
                });
            }

            if (!File.Exists(item.Path) && !Directory.Exists(item.Path))
            {
                return apiClient.GetVideoStreamUrl(new VideoStreamOptions
                {
                    Static = true,
                    ItemId = item.Id
                });
            }

            var itemPath = isoMount == null ? item.Path : isoMount.MountedPath;

            if (item.VideoType.HasValue && item.VideoType.Value == VideoType.BluRay)
            {
                var file = new DirectoryInfo(itemPath)
                    .EnumerateFiles("index.bdmv", SearchOption.AllDirectories)
                    .FirstOrDefault();

                if (file != null)
                {
                    Uri uri;

                    if (Uri.TryCreate(file.FullName, UriKind.RelativeOrAbsolute, out uri))
                    {
                        return uri.OriginalString;
                    }

                    return file.FullName;
                }
            }

            return itemPath;
        }
    }
}
