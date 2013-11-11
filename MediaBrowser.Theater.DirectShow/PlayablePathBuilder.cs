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
            // Check the mounted path first
            if (isoMount != null)
            {
                if (item.IsoType.HasValue && item.IsoType.Value == IsoType.BluRay)
                {
                    return GetBlurayPath(isoMount.MountedPath);
                }

                return isoMount.MountedPath;
            }

            // Stream remote items through the api
            if (item.LocationType == LocationType.Remote)
            {
                return GetStreamedPath(item, apiClient);
            }

            // Stream if we can't access the file system
            if (!File.Exists(item.Path) && !Directory.Exists(item.Path))
            {
                return GetStreamedPath(item, apiClient);
            }

            if (item.VideoType.HasValue && item.VideoType.Value == VideoType.BluRay)
            {
                return GetBlurayPath(item.Path);
            }

            return item.Path;
        }

        private static string GetStreamedPath(BaseItemDto item, IApiClient apiClient)
        {
            if (item.VideoType.HasValue)
            {
                if (item.VideoType.Value != VideoType.VideoFile)
                {
                    return apiClient.GetVideoStreamUrl(new VideoStreamOptions
                    {
                        ItemId = item.Id,
                        OutputFileExtension = "mp4",
                        VideoCodec = VideoCodecs.H264,
                        AudioCodec = AudioCodecs.Aac
                    });
                }
            }

            var extension = item.LocationType == LocationType.Remote ? null : Path.GetExtension(item.Path);

            if (item.IsAudio)
            {
                return apiClient.GetAudioStreamUrl(new VideoStreamOptions
                {
                    Static = true,
                    ItemId = item.Id,
                    OutputFileExtension = extension
                });
            }

            return apiClient.GetVideoStreamUrl(new VideoStreamOptions
            {
                Static = true,
                ItemId = item.Id,
                OutputFileExtension = extension
            });
        }

        private static string GetBlurayPath(string root)
        {
            var file = new DirectoryInfo(root)
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

            return root;
        }
    }
}
