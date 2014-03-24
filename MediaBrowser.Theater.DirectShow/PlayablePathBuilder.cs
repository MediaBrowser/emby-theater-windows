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
        /// <param name="startTimeTicks">The start time ticks.</param>
        /// <returns>System.String.</returns>
        public static string GetPlayablePath(BaseItemDto item, IIsoMount isoMount, IApiClient apiClient, long? startTimeTicks)
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
                return GetStreamedPath(item, apiClient, startTimeTicks);
            }

            // Stream if we can't access the file system
            if (!File.Exists(item.Path) && !Directory.Exists(item.Path))
            {
                return GetStreamedPath(item, apiClient, startTimeTicks);
            }

            if (item.VideoType.HasValue && item.VideoType.Value == VideoType.BluRay)
            {
                return GetBlurayPath(item.Path);
            }

            return item.Path;
        }

        private static string GetStreamedPath(BaseItemDto item, IApiClient apiClient, long? startTimeTicks)
        {
            var extension = item.LocationType == LocationType.Remote ? null : Path.GetExtension(item.Path);

            if (item.IsAudio)
            {
                if (item.LocationType == LocationType.Remote)
                {
                    return apiClient.GetAudioStreamUrl(new StreamOptions
                    {
                        ItemId = item.Id,
                        OutputFileExtension = ".aac",
                        AudioCodec = "aac",
                        StartTimeTicks = startTimeTicks
                    });
                }

                return apiClient.GetAudioStreamUrl(new StreamOptions
                {
                    Static = true,
                    ItemId = item.Id,
                    OutputFileExtension = extension,
                    AudioCodec = "copy",
                    StartTimeTicks = startTimeTicks
                });
            }

            if (item.LocationType == LocationType.Remote)
            {
                return apiClient.GetVideoStreamUrl(new VideoStreamOptions
                {
                    ItemId = item.Id,
                    OutputFileExtension = "ts",
                    VideoCodec = "h264",
                    AudioCodec = "aac"
                });
            }

            // Folder rips
            if (item.VideoType.HasValue && item.VideoType.Value != VideoType.VideoFile)
            {
                return apiClient.GetVideoStreamUrl(new VideoStreamOptions
                {
                    ItemId = item.Id,
                    OutputFileExtension = "ts",
                    VideoCodec = "h264",
                    AudioCodec = "aac"
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
