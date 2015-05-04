using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediaBrowser.ApiInteraction.Data;
using MediaBrowser.ApiInteraction.Net;
using MediaBrowser.ApiInteraction.Playback;
using MediaBrowser.Model.ApiClient;
using MediaBrowser.Model.Dlna;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.IO;
using MediaBrowser.Model.Logging;
using MediaBrowser.Theater.Api.Configuration;

namespace MediaBrowser.Theater.Playback
{
    public interface IPlayableMediaBuilder 
    {
        Task<PlayableMedia> GetPlayableMedia(Media media, DeviceProfile profile, bool canPlayMountedIso, CancellationToken cancellationToken);
    }

    public class PlayableMediaBuilder : IPlayableMediaBuilder
    {
        private readonly ILogger _log;
        private readonly ITheaterConfigurationManager _config;
        private readonly IConnectionManager _connectionManager;
        private readonly IIsoManager _isoManager;
        private readonly ApiInteraction.Playback.IPlaybackManager _apiPlaybackManager;

        public PlayableMediaBuilder(IConnectionManager connectionManager, ILogManager logManager, IIsoManager isoManager, ITheaterConfigurationManager config)
        {
            _log = logManager.GetLogger(typeof (PlayableMediaBuilder).Name);
            _connectionManager = connectionManager;
            _isoManager = isoManager;
            _config = config;

            var localPlayer = new LocalPlayer(new NetworkConnection(_log), new HttpWebRequestClient(_log, new HttpWebRequestFactory()));
            _apiPlaybackManager = new ApiInteraction.Playback.PlaybackManager(new NullAssetManager(), _connectionManager.Device, _log, localPlayer);
        }

        public Task<StreamInfo> GetAudioStreamInfo(string serverId, AudioOptions options)
        {
            var apiClient = _connectionManager.GetApiClient(serverId);
            options.DeviceId = _connectionManager.Device.DeviceId;

            return _apiPlaybackManager.GetAudioStreamInfo(serverId, options, false, apiClient);
        }

        public Task<StreamInfo> GetVideoStreamInfo(string serverId, VideoOptions options)
        {
            var apiClient = _connectionManager.GetApiClient(serverId);
            options.DeviceId = _connectionManager.Device.DeviceId;

            return _apiPlaybackManager.GetVideoStreamInfo(serverId, options, false, apiClient);
        }

        public async Task<PlayableMedia> GetPlayableMedia(Media media, DeviceProfile profile, bool canPlayMountedIso, CancellationToken cancellationToken)
        {
            var item = media.Item;

            // Handle iso mounting, since StreamBuilder does not support this on it's own

            IIsoMount mountedIso = null;
            if (canPlayMountedIso && item.VideoType == VideoType.Iso && item.IsoType != null && _isoManager.CanMount(item.Path)) {
                try {
                    mountedIso = await _isoManager.Mount(item.Path, cancellationToken).ConfigureAwait(false);
                }
                catch (Exception e) {
                    _log.ErrorException("Error mounting ISO {0}", e, item.Path);
                }
            }

            if (mountedIso != null) {
                if (item.IsoType == IsoType.BluRay) {
                    return new PlayableMedia {
                        Media = media,
                        Path = GetBlurayPath(mountedIso.MountedPath),
                        IsoMount = mountedIso,
                        Source = item.MediaSources.First()
                    };
                }

                return new PlayableMedia {
                    Media = media,
                    Path = mountedIso.MountedPath,
                    IsoMount = mountedIso,
                    Source = item.MediaSources.First()
                };
            }

            // Handle direct play of folder rips, since StreamBuilder doesn't support this on it's own

            if (canPlayMountedIso && item.LocationType == LocationType.FileSystem && item.VideoType.HasValue) {
                if (item.VideoType == VideoType.BluRay || item.VideoType == VideoType.Dvd || item.VideoType == VideoType.HdDvd) {
                    if (Directory.Exists(item.Path)) {
                        return new PlayableMedia {
                            Media = media,
                            Path = GetFolderRipPath(item.VideoType.Value, item.Path),
                            Source = item.MediaSources.First()
                        };
                    }
                }
            }

            return await GetPlayableMediaInternal(media, profile);
        }

        private async Task<PlayableMedia> GetPlayableMediaInternal(Media media, DeviceProfile profile)
        {
            var item = media.Item;

            var options = new VideoOptions {
                Context = EncodingContext.Streaming,
                ItemId = item.Id,

                // TODO: Set to 2 if user only has stereo speakers
                MaxAudioChannels = 6,

                MaxBitrate = _config.Configuration.MaxStreamingBitrate,
                MediaSources = item.MediaSources,

                Profile = profile
            };

            var streamInfo = item.IsAudio ?
                                 await GetAudioStreamInfo(item.ServerId, options) :
                                 await GetVideoStreamInfo(item.ServerId, options);

            streamInfo.StartPositionTicks = media.Options.StartPositionTicks ?? 0;

            var apiClient = _connectionManager.GetApiClient(item);

            return new PlayableMedia {
                Media = media,
                Path = streamInfo.ToUrl(apiClient.ServerAddress + "/mediabrowser", apiClient.AccessToken),
                Source = streamInfo.MediaSource,
                StreamInfo = streamInfo
            };
        }

        private static string GetFolderRipPath(VideoType videoType, string root)
        {
            if (videoType == VideoType.BluRay) {
                return GetBlurayPath(root);
            }

            return root;
        }

        private static string GetBlurayPath(string root)
        {
            var file = new DirectoryInfo(root)
                .EnumerateFiles("index.bdmv", SearchOption.AllDirectories)
                .FirstOrDefault();

            if (file != null) {
                Uri uri;

                if (Uri.TryCreate(file.FullName, UriKind.RelativeOrAbsolute, out uri)) {
                    return uri.OriginalString;
                }

                return file.FullName;
            }

            return root;
        }
    }
}