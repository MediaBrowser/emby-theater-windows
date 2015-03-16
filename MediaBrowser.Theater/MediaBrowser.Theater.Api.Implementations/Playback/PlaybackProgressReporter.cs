using System;
using System.Collections.Generic;
using System.Linq;
using MediaBrowser.Model.ApiClient;
using MediaBrowser.Model.Dto;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.Logging;
using MediaBrowser.Model.Session;
using MediaBrowser.Theater.Playback;

namespace MediaBrowser.Theater.Api.Playback
{
    public class PlaybackProgressReporter
    {
        private readonly IConnectionManager _connectionManager;
        private readonly ILogger _logger;
        private readonly IPlaybackManager _playback;

        public PlaybackProgressReporter(IConnectionManager connectionManager, ILogger logger, IPlaybackManager playback)
        {
            _connectionManager = connectionManager;
            _logger = logger;
            _playback = playback;

            playback.Events.Subscribe(status => {
                if (status.StatusType == PlaybackStatusType.Started) {
                    ReportMediaStarted(status);
                } else if (status.StatusType == PlaybackStatusType.Playing) {
                    ReportMediaProgress(status);
                } else if (!status.StatusType.IsActiveState()) {
                    ReportMediaCompleted(status);
                }
            });
        }

        private async void ReportMediaProgress(PlaybackStatus status)
        {
            BaseItemDto item = status.PlayableMedia.Media.Item;

            if (item == null) {
                return;
            }

            await _playback.AccessSession(async s => {
                var info = new PlaybackProgressInfo {
                    ItemId = item.Id,
                    IsMuted = _playback.GlobalSettings.Audio.IsMuted,
                    IsPaused = status.StatusType == PlaybackStatusType.Paused,
                    PositionTicks = status.Progress,
                    CanSeek = s.Capabilities.CanSeek,
                    AudioStreamIndex = status.GetActiveStreamIndex(MediaStreamType.Audio),
                    SubtitleStreamIndex = status.GetActiveStreamIndex(MediaStreamType.Subtitle),
                    VolumeLevel = (int) (_playback.GlobalSettings.Audio.Volume*100),
                    PlayMethod = PlayMethod.DirectPlay, // todo remove hard coding
                };

                try {
                    IApiClient apiClient = _connectionManager.GetApiClient(item);
                    await apiClient.ReportPlaybackProgressAsync(info);
                }
                catch (Exception ex) {
                    _logger.ErrorException("Error sending playback progress checking for {0}", ex, item.Name);
                }
            });
        }

        private async void ReportMediaCompleted(PlaybackStatus status)
        {
            BaseItemDto item = status.PlayableMedia.Media.Item;

            var info = new PlaybackStopInfo {
                ItemId = item.Id,
                PositionTicks = status.Progress
            };

            try {
                IApiClient apiClient = _connectionManager.GetApiClient(item);
                await apiClient.ReportPlaybackStoppedAsync(info);
            }
            catch (Exception ex) {
                _logger.ErrorException("Error sending playback stopped checking for {0}", ex, item.Name);
            }
        }

        private async void ReportMediaStarted(PlaybackStatus status)
        {
            BaseItemDto item = status.PlayableMedia.Media.Item;

            if (item.Id == null) {
                // Item is local media to the client (i.e playing local dvd)
                // todo - fix up local media progress reporting
                return;
            }

            try {
                // TODO: should probably be all media types
                var queueTypes = new List<string> { item.MediaType };

                var info = new PlaybackStartInfo {
                    ItemId = item.Id,
                    QueueableMediaTypes = queueTypes.ToList(),

                    // TODO: Remove this hardcoding
                    PlayMethod = PlayMethod.DirectPlay
                };

                IApiClient apiClient = _connectionManager.GetApiClient(item);
                await apiClient.ReportPlaybackStartAsync(info);
            }
            catch (Exception ex) {
                _logger.ErrorException("Error sending playback start checking for {0}", ex, item.Name);
                throw;
            }
        }
    }
}