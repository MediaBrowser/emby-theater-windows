using MediaBrowser.Model.ApiClient;
using MediaBrowser.Model.Dlna;
using MediaBrowser.Model.Dto;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.IO;
using MediaBrowser.Theater.DirectShow.Streaming;
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
        public static string GetPlayablePath(BaseItemDto item, IIsoMount isoMount, IApiClient apiClient, long? startTimeTicks, int? maxBitrate)
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

            if (item.LocationType == LocationType.FileSystem)
            {
                if (File.Exists(item.Path) || Directory.Exists(item.Path))
                {
                    if (item.VideoType.HasValue && item.VideoType.Value == VideoType.BluRay)
                    {
                        return GetBlurayPath(item.Path);
                    }

                    return item.Path;
                }
            }

            return GetStreamedPath(item, apiClient, startTimeTicks, maxBitrate);
        }

        private static string GetStreamedPath(BaseItemDto item, IApiClient apiClient, long? startTimeTicks, int? maxBitrate)
        {
            var profile = new MediaBrowserTheaterProfile();

            StreamInfo info;

            if (item.IsAudio)
            {
                var options = new AudioOptions
                {
                    Context = EncodingContext.Streaming,
                    DeviceId = apiClient.DeviceId,
                    ItemId = item.Id,

                    // TODO: Reduce to 2 is user only has stereo speakers
                    MaxAudioChannels = 6,

                    MaxBitrate = maxBitrate,
                    MediaSources = item.MediaSources,

                    Profile = profile
                };

                info = new StreamBuilder().BuildAudioItem(options);
            }
            else
            {
                var options = new VideoOptions
                {
                    Context = EncodingContext.Streaming,
                    DeviceId = apiClient.DeviceId,
                    ItemId = item.Id,

                    // TODO: Reduce to 2 is user only has stereo speakers
                    MaxAudioChannels = 6,

                    MaxBitrate = maxBitrate,
                    MediaSources = item.MediaSources,

                    Profile = profile
                };

                info = new StreamBuilder().BuildVideoItem(options);
            }

            info.StartPositionTicks = startTimeTicks ?? 0;

            return info.ToUrl(apiClient.ServerAddress + "/mediabrowser");
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
