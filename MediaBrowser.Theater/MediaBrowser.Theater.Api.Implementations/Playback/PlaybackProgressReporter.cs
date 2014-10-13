using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediaBrowser.Model.ApiClient;
using MediaBrowser.Model.Dto;
using MediaBrowser.Model.Logging;
using MediaBrowser.Model.Session;

namespace MediaBrowser.Theater.Api.Playback
{
    public class PlaybackProgressReporter : IDisposable
    {
        private readonly IConnectionManager _connectionManager;
        private readonly ILogger _logger;
        private readonly IMediaPlayer _mediaPlayer;
        private readonly IPlaybackManager _playback;

        private Timer _timer;

        public PlaybackProgressReporter(IConnectionManager connectionManager, IMediaPlayer mediaPlayer, ILogger logger, IPlaybackManager playback)
        {
            _connectionManager = connectionManager;
            _mediaPlayer = mediaPlayer;
            _logger = logger;
            _playback = playback;
        }

        public void Dispose()
        {
            _mediaPlayer.MediaChanged -= _mediaPlayer_MediaChanged;
            _mediaPlayer.PlaybackCompleted -= _mediaPlayer_PlaybackCompleted;

            if (_timer != null) {
                _timer.Dispose();
                _timer = null;
            }
        }

        /// <summary>
        ///     Starts this instance.
        /// </summary>
        /// <returns>Task.</returns>
        public async Task Start()
        {
            BaseItemDto item = _mediaPlayer.CurrentMedia;

            if (item == null) {
                throw new InvalidOperationException("Nothing is currently playing");
            }

            if (item.Id == null) {
                // Item is local media to the client (i.e playing local dvd)
                // todo - fix up local media progress reporting
                return;
            }

            try {
                List<string> queueTypes = _mediaPlayer.CanQueue
                                              ? new List<string> { item.MediaType }
                                              : new List<string>();

                var info = new PlaybackStartInfo {
                    ItemId = item.Id,
                    CanSeek = _mediaPlayer.CanSeek,
                    QueueableMediaTypes = queueTypes.ToList(),

                    // TODO: Remove this hardcoding
                    PlayMethod = PlayMethod.DirectPlay
                };

                IApiClient apiClient = _connectionManager.GetApiClient(item);
                await apiClient.ReportPlaybackStartAsync(info);

                if (_mediaPlayer.CanTrackProgress) {
                    _timer = new Timer(TimerCallback, null, 100, 900);
                }

                _mediaPlayer.MediaChanged += _mediaPlayer_MediaChanged;
                _mediaPlayer.PlaybackCompleted += _mediaPlayer_PlaybackCompleted;
            }
            catch (Exception ex) {
                _logger.ErrorException("Error sending playback start checking for {0}", ex, item.Name);

                throw;
            }
        }

        /// <summary>
        ///     Handles the PlaybackCompleted event of the _mediaPlayer control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="PlaybackStopEventArgs" /> instance containing the event data.</param>
        private async void _mediaPlayer_PlaybackCompleted(object sender, PlaybackStopEventArgs e)
        {
            _mediaPlayer.MediaChanged -= _mediaPlayer_MediaChanged;
            _mediaPlayer.PlaybackCompleted -= _mediaPlayer_PlaybackCompleted;

            if (_timer != null) {
                _timer.Dispose();
                _timer = null;
            }

            if (e.EndingMedia != null) {
                var info = new PlaybackStopInfo {
                    ItemId = e.EndingMedia.Id,
                    PositionTicks = e.EndingPositionTicks
                };

                try {
                    IApiClient apiClient = _connectionManager.GetApiClient(e.EndingMedia);
                    await apiClient.ReportPlaybackStoppedAsync(info);
                }
                catch (Exception ex) {
                    _logger.ErrorException("Error sending playback stopped checking for {0}", ex, e.EndingMedia.Name);
                }
            }
        }

        /// <summary>
        ///     Handles the MediaChanged event of the _mediaPlayer control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="MediaChangeEventArgs" /> instance containing the event data.</param>
        private async void _mediaPlayer_MediaChanged(object sender, MediaChangeEventArgs e)
        {
            if (e.PreviousMedia != null) {
                var info = new PlaybackStopInfo {
                    ItemId = e.PreviousMedia.Id,
                    PositionTicks = e.EndingPositionTicks
                };

                try {
                    IApiClient apiClient = _connectionManager.GetApiClient(e.PreviousMedia);
                    await apiClient.ReportPlaybackStoppedAsync(info);
                }
                catch (Exception ex) {
                    _logger.ErrorException("Error sending playback stopped checking for {0}", ex, e.PreviousMedia.Name);
                }
            }

            if (e.NewMedia != null) {
                try {
                    List<string> queueTypes = _mediaPlayer.CanQueue
                                                  ? new List<string> { e.NewMedia.MediaType }
                                                  : new List<string>();

                    var info = new PlaybackStartInfo {
                        ItemId = e.NewMedia.Id,
                        CanSeek = _mediaPlayer.CanSeek,
                        QueueableMediaTypes = queueTypes.ToList(),

                        // TODO: Remove this hardcoding
                        PlayMethod = PlayMethod.DirectPlay
                    };

                    IApiClient apiClient = _connectionManager.GetApiClient(e.NewMedia);
                    await apiClient.ReportPlaybackStartAsync(info);
                }
                catch (Exception ex) {
                    _logger.ErrorException("Error sending playback start checking for {0}", ex, e.NewMedia.Name);
                }
            }
        }

        /// <summary>
        ///     Timers the callback.
        /// </summary>
        /// <param name="state">The state.</param>
        private async void TimerCallback(object state)
        {
            BaseItemDto item = _mediaPlayer.CurrentMedia;

            if (item == null) {
                return;
            }

            var info = new PlaybackProgressInfo {
                //SessionId = _sessionManager
                //Item = item,
                ItemId = item.Id,
                //MediaSourceId = string.Empty,
                IsMuted = _playback.IsMuted,
                IsPaused = _mediaPlayer.PlayState == PlayState.Paused,
                PositionTicks = _mediaPlayer.CurrentPositionTicks,
                CanSeek = _mediaPlayer.CanSeek,
                AudioStreamIndex = _mediaPlayer.CurrentAudioStreamIndex,
                SubtitleStreamIndex = _mediaPlayer.CurrentSubtitleStreamIndex,
                VolumeLevel = (_mediaPlayer.PlayState != PlayState.Idle) ? (int?) _playback.Volume : null,
                PlayMethod = PlayMethod.DirectPlay, // todo remove hard coding
            };

            try {
                IApiClient apiClient = _connectionManager.GetApiClient(item);
                await apiClient.ReportPlaybackProgressAsync(info);
            }
            catch (Exception ex) {
                _logger.ErrorException("Error sending playback progress checking for {0}", ex, item.Name);
            }
        }
    }
}