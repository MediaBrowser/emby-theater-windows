using System;
using MediaBrowser.Model.ApiClient;
using MediaBrowser.Model.Dto;
using MediaBrowser.Model.Entities;
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
        /// <param name="apiClient">The API client.</param>
        /// <returns>System.String.</returns>
        public static string GetPlayablePath(BaseItemDto item, IApiClient apiClient)
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

            if (item.VideoType.HasValue && item.VideoType.Value == VideoType.BluRay && !string.IsNullOrEmpty(item.MainFeaturePlaylistName))
            {
                var file = new DirectoryInfo(item.Path)
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

            return item.Path;
        }
    }
}
