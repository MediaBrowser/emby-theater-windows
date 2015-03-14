using MediaBrowser.ApiInteraction.Playback;
using MediaBrowser.Model.ApiClient;
using MediaBrowser.Model.Dlna;
using MediaBrowser.Model.Dto;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.IO;
using MediaBrowser.Model.MediaInfo;
using MediaBrowser.Theater.DirectShow.Streaming;
using MediaBrowser.Theater.Presentation.Playback;
using System;
using System.Collections.Generic;
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
        public static PlayableItem GetPlayableItem(BaseItemDto item, List<MediaSourceInfo> mediaSources, IIsoMount isoMount, IApiClient apiClient, StreamBuilder streamBuilder, long? startTimeTicks, int? maxBitrate)
        {
            // Check the mounted path first
            if (isoMount != null)
            {
                if (item.IsoType.HasValue && item.IsoType.Value == IsoType.BluRay)
                {
                    return new PlayableItem
                    {
                        OriginalItem = item,
                        PlayablePath = GetBlurayPath(isoMount.MountedPath),
                        IsoMount = isoMount,
                        MediaSource = item.MediaSources.First()
                    };
                }

                return new PlayableItem
                {
                    OriginalItem = item,
                    PlayablePath = isoMount.MountedPath,
                    IsoMount = isoMount,
                    MediaSource = item.MediaSources.First()
                };
            }

            if (item.LocationType == LocationType.FileSystem)
            {
                if (File.Exists(item.Path) || Directory.Exists(item.Path))
                {
                    if (item.VideoType.HasValue && item.VideoType.Value == VideoType.BluRay)
                    {
                        return new PlayableItem
                        {
                            OriginalItem = item,
                            PlayablePath = GetBlurayPath(item.Path),
                            MediaSource = item.MediaSources.First()
                        };
                    }

                    if (item.VideoType.HasValue && item.VideoType.Value == VideoType.Dvd)
                    {
                        return new PlayableItem
                        {
                            OriginalItem = item,
                            PlayablePath = item.Path,
                            MediaSource = item.MediaSources.First()
                        };
                    }

                    if (item.VideoType.HasValue && item.VideoType.Value == VideoType.HdDvd)
                    {
                        return new PlayableItem
                        {
                            OriginalItem = item,
                            PlayablePath = item.Path,
                            MediaSource = item.MediaSources.First()
                        };
                    }

                    //return item.Path;
                }
            }

            return GetStreamedItem(item, mediaSources, apiClient, streamBuilder, startTimeTicks, maxBitrate);
        }

        private static PlayableItem GetStreamedItem(BaseItemDto item, List<MediaSourceInfo> mediaSources, IApiClient apiClient, StreamBuilder streamBuilder, long? startTimeTicks, int? maxBitrate)
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
                    MediaSources = mediaSources,

                    Profile = profile
                };

                info = streamBuilder.BuildAudioItem(options);
                info.StartPositionTicks = startTimeTicks ?? 0;

                if (info.MediaSource.Protocol == MediaProtocol.File && File.Exists(info.MediaSource.Path))
                {
                    return new PlayableItem
                    {
                        OriginalItem = item,
                        PlayablePath = info.MediaSource.Path,
                        MediaSource = info.MediaSource,
                        StreamInfo = info
                    };
                }

                return new PlayableItem
                {
                    OriginalItem = item,
                    PlayablePath = info.ToUrl(apiClient.ServerAddress + "/mediabrowser", apiClient.AccessToken),
                    MediaSource = info.MediaSource,
                    StreamInfo = info
                };
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
                    MediaSources = mediaSources,

                    Profile = profile
                };

                info = streamBuilder.BuildVideoItem(options);
                info.StartPositionTicks = startTimeTicks ?? 0;

                if (info.MediaSource.Protocol == MediaProtocol.File && File.Exists(info.MediaSource.Path))
                {
                    return new PlayableItem
                    {
                        OriginalItem = item,
                        PlayablePath = info.MediaSource.Path,
                        MediaSource = info.MediaSource,
                        StreamInfo = info
                    };
                }

                //info.Container = "ts";
                //info.VideoCodec = "copy";
                //info.AudioCodec = "copy";
                //info.Protocol = "http";

                //if (item.IsType("tvchannel"))
                //{
                //    info.VideoCodec = "copy";
                //}

                var playable = new PlayableItem
                {
                    OriginalItem = item,
                    PlayablePath = info.ToUrl(apiClient.ServerAddress + "/mediabrowser", apiClient.AccessToken),
                    MediaSource = info.MediaSource,
                    StreamInfo = info
                };

                if (!info.IsDirectStream)
                {
                    playable.PlayablePath += "&EnableAdaptiveBitrateStreaming=false";
                }

                return playable;
            }
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
