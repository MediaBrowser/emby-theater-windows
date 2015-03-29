using MediaBrowser.Model.ApiClient;
using MediaBrowser.Model.Dlna;
using MediaBrowser.Model.Logging;
using MediaBrowser.Model.Session;
using MediaBrowser.Theater.Interfaces.Playback;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace MediaBrowser.Theater.Implementations.Playback
{
    public class PlaybackProgressReporter : IDisposable
    {
        private readonly IConnectionManager _connectionManager;
        private readonly IMediaPlayer _mediaPlayer;
        private readonly ILogger _logger;
        private readonly IPlaybackManager _internalPlaybackManager;
        private readonly ApiInteraction.Playback.IPlaybackManager _apiPlaybackManager;

        private Timer _timer;

        public PlaybackProgressReporter(IConnectionManager connectionManager, IMediaPlayer mediaPlayer, ILogger logger, IPlaybackManager internalPlaybackManager, ApiInteraction.Playback.IPlaybackManager apiPlaybackManager)
        {
            _connectionManager = connectionManager;
            _mediaPlayer = mediaPlayer;
            _logger = logger;
            _internalPlaybackManager = internalPlaybackManager;
            _apiPlaybackManager = apiPlaybackManager;
        }

        /// <summary>
        /// Starts this instance.
        /// </summary>
        /// <returns>Task.</returns>
        public async Task Start()
        {
            var item = _mediaPlayer.CurrentMedia;

            if (item == null)
            {
                return;
            }

            if (item.Id == null)
            {
                // Item is local media to the client (i.e playing local dvd)
                // todo - fix up local media progress reporting
                return;
            }

            try
            {
                var queueTypes = _mediaPlayer.CanQueue
                                     ? new List<string> { item.MediaType }
                                     : new List<string> { };

                var info = new PlaybackStartInfo
                {
                    ItemId = item.Id,
                    CanSeek = _mediaPlayer.CanSeek,
                    QueueableMediaTypes = queueTypes.ToList(),

                    // TODO: Remove this hardcoding
                    PlayMethod = PlayMethod.DirectPlay
                };

                var apiClient = _connectionManager.GetApiClient(item);

                await apiClient.ReportPlaybackStartAsync(info);

                if (_mediaPlayer.CanTrackProgress)
                {
                    _timer = new Timer(TimerCallback, null, 100, 900);
                }

                _mediaPlayer.MediaChanged += _mediaPlayer_MediaChanged;
                _mediaPlayer.PlaybackCompleted += _mediaPlayer_PlaybackCompleted;
            }
            catch (Exception ex)
            {
                _logger.ErrorException("Error sending internalPlaybackManager start checking for {0}", ex, item.Name);

                throw;
            }
        }

        /// <summary>
        /// Handles the PlaybackCompleted event of the _mediaPlayer control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="eventArgs">The <see cref="PlaybackStopEventArgs"/> instance containing the event data.</param>
        async void _mediaPlayer_PlaybackCompleted(object sender, PlaybackStopEventArgs eventArgs)
        {
            _mediaPlayer.MediaChanged -= _mediaPlayer_MediaChanged;
            _mediaPlayer.PlaybackCompleted -= _mediaPlayer_PlaybackCompleted;

            var item = eventArgs.EndingMedia;
            if (item != null)
            {
                var apiClient = _connectionManager.GetApiClient(item);

                var stopInfo = new PlaybackStopInfo
                {
                    ItemId = item.Id,
                    PositionTicks = eventArgs.EndingPositionTicks
                };

                // Have to test this for null because external players are currently not supplying this
                // Also some players will play in contexts not currently supported by common playback managers, e.g. direct play of folder rips, and iso-mounted media
                // Remove when implemented
                if (eventArgs.StreamInfo != null)
                {
                    await _apiPlaybackManager.ReportPlaybackStopped(stopInfo, eventArgs.StreamInfo, item.ServerId, apiClient.CurrentUserId, false, apiClient);
                }
                else
                {
                    await apiClient.ReportPlaybackStoppedAsync(stopInfo);
                }
            }
            
            if (_timer != null)
            {
                _timer.Dispose();
                _timer = null;
            }
        }

        /// <summary>
        /// Handles the MediaChanged event of the _mediaPlayer control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="eventArgs">The <see cref="MediaChangeEventArgs"/> instance containing the event data.</param>
        async void _mediaPlayer_MediaChanged(object sender, MediaChangeEventArgs eventArgs)
        {
            if (eventArgs.PreviousMedia != null)
            {
                var apiClient = _connectionManager.GetApiClient(eventArgs.PreviousMedia);

                var stopInfo = new PlaybackStopInfo
                {
                    ItemId = eventArgs.PreviousMedia.Id,
                    PositionTicks = eventArgs.EndingPositionTicks
                };

                // Have to test this for null because external players are currently not supplying this
                // Also some players will play in contexts not currently supported by common playback managers, e.g. direct play of folder rips, and iso-mounted media
                // Remove when implemented
                if (eventArgs.PreviousStreamInfo != null)
                {
                    await _apiPlaybackManager.ReportPlaybackStopped(stopInfo, eventArgs.PreviousStreamInfo, eventArgs.PreviousMedia.ServerId, apiClient.CurrentUserId, false, apiClient);
                }
                else
                {
                    await apiClient.ReportPlaybackStoppedAsync(stopInfo);
                }
            }

            if (eventArgs.NewMedia != null)
            {
                try
                {
                    var queueTypes = _mediaPlayer.CanQueue
                                ? new List<string> { eventArgs.NewMedia.MediaType }
                                : new List<string> { };

                    var info = new PlaybackStartInfo
                    {
                        ItemId = eventArgs.NewMedia.Id,

                        CanSeek = _mediaPlayer.CanSeek,
                        QueueableMediaTypes = queueTypes.ToList(),

                        // TODO: Remove this hardcoding
                        PlayMethod = PlayMethod.DirectPlay
                    };

                    var apiClient = _connectionManager.GetApiClient(eventArgs.NewMedia);

                    await apiClient.ReportPlaybackStartAsync(info);
                }
                catch (Exception ex)
                {
                    _logger.ErrorException("Error sending internalPlaybackManager start checking for {0}", ex, eventArgs.NewMedia.Name);
                }
            }
        }

        /// <summary>
        /// Timers the callback.
        /// </summary>
        /// <param name="state">The state.</param>
        private async void TimerCallback(object state)
        {
            var item = _mediaPlayer.CurrentMedia;

            if (item == null)
            {
                return;
            }

            var currentStreamInfo = _mediaPlayer.CurrentStreamInfo;

            var info = new PlaybackProgressInfo
            {
                ItemId = item.Id,
                IsMuted = _internalPlaybackManager.IsMuted,
                IsPaused = _mediaPlayer.PlayState == PlayState.Paused,
                PositionTicks = _mediaPlayer.CurrentPositionTicks,
                CanSeek = _mediaPlayer.CanSeek,
                AudioStreamIndex = _mediaPlayer.CurrentAudioStreamIndex,
                SubtitleStreamIndex = _mediaPlayer.CurrentSubtitleStreamIndex,
                VolumeLevel = (_mediaPlayer.PlayState != PlayState.Idle) ? (int?) _internalPlaybackManager.Volume : null,
                PlayMethod = currentStreamInfo.PlayMethod
            };

            var apiClient = _connectionManager.GetApiClient(item);

            try
            {
                // Have to test this for null because external players are currently not supplying this
                // Also some players will play in contexts not currently supported by common playback managers, e.g. direct play of folder rips, and iso-mounted media
                // Remove when implemented
                if (currentStreamInfo != null)
                {
                    info.MediaSourceId = currentStreamInfo.MediaSourceId;

                    await _apiPlaybackManager.ReportPlaybackProgress(info, currentStreamInfo, false, apiClient);
                }
                else
                {
                    await apiClient.ReportPlaybackProgressAsync(info);
                }
            }
            catch (Exception ex)
            {
                _logger.ErrorException("Error sending internalPlaybackManager progress checking for {0}", ex, item.Name);
            }
        }

        public void Dispose()
        {
            _mediaPlayer.MediaChanged -= _mediaPlayer_MediaChanged;
            _mediaPlayer.PlaybackCompleted -= _mediaPlayer_PlaybackCompleted;

            if (_timer != null)
            {
                _timer.Dispose();
                _timer = null;
            }
        }
    }
}
