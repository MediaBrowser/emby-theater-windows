using MediaBrowser.Model.ApiClient;
using MediaBrowser.Model.Logging;
using MediaBrowser.Theater.Interfaces.Playback;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace MediaBrowser.Theater.Implementations.Playback
{
    public class PlaybackProgressReporter
    {
        private readonly IApiClient _apiClient;
        private readonly IMediaPlayer _mediaPlayer;
        private readonly ILogger _logger;
        private readonly IPlaybackManager _playback;

        private Timer _timer;

        public PlaybackProgressReporter(IApiClient apiClient, IMediaPlayer mediaPlayer, ILogger logger, IPlaybackManager playback)
        {
            _apiClient = apiClient;
            _mediaPlayer = mediaPlayer;
            _logger = logger;
            _playback = playback;
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
                throw new InvalidOperationException("Nothing is currently playing");
            }

            try
            {
                var queueTypes = _mediaPlayer.CanQueue
                                     ? new List<string> { item.MediaType }
                                     : new List<string> { };

                await _apiClient.ReportPlaybackStartAsync(item.Id, _apiClient.CurrentUserId, _mediaPlayer.CanSeek, queueTypes);

                if (_mediaPlayer.CanTrackProgress)
                {
                    _timer = new Timer(TimerCallback, null, 1000, 1000);
                }

                _mediaPlayer.MediaChanged += _mediaPlayer_MediaChanged;
                _mediaPlayer.PlaybackCompleted += _mediaPlayer_PlaybackCompleted;
            }
            catch (Exception ex)
            {
                _logger.ErrorException("Error sending playback start checking for {0}", ex, item.Name);

                throw;
            }
        }

        /// <summary>
        /// Handles the PlaybackCompleted event of the _mediaPlayer control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="PlaybackStopEventArgs"/> instance containing the event data.</param>
        async void _mediaPlayer_PlaybackCompleted(object sender, PlaybackStopEventArgs e)
        {
            _mediaPlayer.MediaChanged -= _mediaPlayer_MediaChanged;
            _mediaPlayer.PlaybackCompleted -= _mediaPlayer_PlaybackCompleted;

            if (_timer != null)
            {
                _timer.Dispose();
                _timer = null;
            }

            if (e.EndingMedia != null)
            {
                try
                {
                    await _apiClient.ReportPlaybackStoppedAsync(e.EndingMedia.Id, _apiClient.CurrentUserId, e.EndingPositionTicks);
                }
                catch (Exception ex)
                {
                    _logger.ErrorException("Error sending playback stopped checking for {0}", ex, e.EndingMedia.Name);
                }
            }
        }

        /// <summary>
        /// Handles the MediaChanged event of the _mediaPlayer control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="MediaChangeEventArgs"/> instance containing the event data.</param>
        async void _mediaPlayer_MediaChanged(object sender, MediaChangeEventArgs e)
        {
            if (e.PreviousMedia != null)
            {
                try
                {
                    await _apiClient.ReportPlaybackStoppedAsync(e.PreviousMedia.Id, _apiClient.CurrentUserId, e.EndingPositionTicks);
                }
                catch (Exception ex)
                {
                    _logger.ErrorException("Error sending playback stopped checking for {0}", ex, e.PreviousMedia.Name);
                }
            }

            if (e.NewMedia != null)
            {
                try
                {
                    var queueTypes = _mediaPlayer.CanQueue
                                ? new List<string> { e.NewMedia.MediaType }
                                : new List<string> { };

                    await _apiClient.ReportPlaybackStartAsync(e.NewMedia.Id, _apiClient.CurrentUserId, _mediaPlayer.CanSeek, queueTypes);
                }
                catch (Exception ex)
                {
                    _logger.ErrorException("Error sending playback start checking for {0}", ex, e.NewMedia.Name);
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

            try
            {
                await _apiClient.ReportPlaybackProgressAsync(item.Id, _apiClient.CurrentUserId, _mediaPlayer.CurrentPositionTicks, _mediaPlayer.PlayState == PlayState.Paused, _playback.IsMuted);
            }
            catch (Exception ex)
            {
                _logger.ErrorException("Error sending playback progress checking for {0}", ex, item.Name);

                throw;
            }
        }
    }
}
