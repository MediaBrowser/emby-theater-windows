using MediaBrowser.Model.ApiClient;
using MediaBrowser.Model.Logging;
using MediaBrowser.Model.Net;
using MediaBrowser.Theater.Interfaces.Playback;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace MediaBrowser.Theater.Implementations.Playback
{
    public class PlaybackProgressReporter : IDisposable
    {
        private readonly IApiClient _apiClient;
        private readonly IMediaPlayer _mediaPlayer;
        private readonly ILogger _logger;

        private Timer _timer;

        public PlaybackProgressReporter(IApiClient apiClient, IMediaPlayer mediaPlayer, ILogger logger)
        {
            _apiClient = apiClient;
            _mediaPlayer = mediaPlayer;
            _logger = logger;
        }

        /// <summary>
        /// Starts this instance.
        /// </summary>
        /// <returns>Task.</returns>
        /// <exception cref="System.InvalidOperationException">Nothing is currently playing</exception>
        public async Task Start()
        {
            var item = _mediaPlayer.CurrentMedia;

            if (item == null)
            {
                throw new InvalidOperationException("Nothing is currently playing");
            }

            _logger.Info("Sending playback start checkin for {0}", item.Name);

            try
            {
                await _apiClient.ReportPlaybackStartAsync(item.Id, _apiClient.CurrentUserId);

                if (_mediaPlayer.CanTrackProgress)
                {
                    _timer = new Timer(TimerCallback, null, 5000, 5000);
                }

                _mediaPlayer.MediaChanged += _mediaPlayer_MediaChanged;
                _mediaPlayer.PlaybackCompleted += _mediaPlayer_PlaybackCompleted;
            }
            catch (HttpException ex)
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
            }

            if (e.EndingMedia != null)
            {
                try
                {
                    await _apiClient.ReportPlaybackStoppedAsync(e.EndingMedia.Id, _apiClient.CurrentUserId, e.EndingPositionTicks);
                }
                catch (HttpException ex)
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
                catch (HttpException ex)
                {
                    _logger.ErrorException("Error sending playback stopped checking for {0}", ex, e.PreviousMedia.Name);
                }
            }

            if (e.NewMedia != null)
            {
                try
                {
                    await _apiClient.ReportPlaybackStartAsync(e.NewMedia.Id, _apiClient.CurrentUserId);
                }
                catch (HttpException ex)
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

            _logger.Info("Sending playback progress checkin for {0}", item.Name);

            try
            {
                await _apiClient.ReportPlaybackProgressAsync(item.Id, _apiClient.CurrentUserId, _mediaPlayer.CurrentPositionTicks, _mediaPlayer.PlayState == PlayState.Paused);
            }
            catch (HttpException ex)
            {
                _logger.ErrorException("Error sending playback progress checking for {0}", ex, item.Name);

                throw;
            }
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources.
        /// </summary>
        /// <param name="dispose"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        protected virtual void Dispose(bool dispose)
        {
            if (dispose)
            {
                _mediaPlayer.MediaChanged -= _mediaPlayer_MediaChanged;
                _mediaPlayer.PlaybackCompleted -= _mediaPlayer_PlaybackCompleted;

                if (_timer != null)
                {
                    _timer.Dispose();
                }
            }
        }
    }
}
