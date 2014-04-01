using MediaBrowser.Common.Events;
using MediaBrowser.Model.ApiClient;
using MediaBrowser.Model.Dto;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.IO;
using MediaBrowser.Model.Logging;
using MediaBrowser.Theater.Interfaces.Configuration;
using MediaBrowser.Theater.Interfaces.Playback;
using MediaBrowser.Theater.Interfaces.Presentation;
using MediaBrowser.Theater.Presentation.Playback;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Management;

namespace MediaBrowser.Theater.DirectShow
{
    public class InternalDirectShowPlayer : IInternalMediaPlayer, IVideoPlayer
    {
        private DirectShowPlayer _mediaPlayer;

        private readonly ILogger _logger;
        private readonly IHiddenWindow _hiddenWindow;
        private readonly IPresentationManager _presentation;
        private readonly IApiClient _apiClient;
        private readonly IPlaybackManager _playbackManager;
        private readonly ITheaterConfigurationManager _config;
        private readonly IIsoManager _isoManager;

        public event EventHandler<MediaChangeEventArgs> MediaChanged;

        public event EventHandler<PlaybackStopEventArgs> PlaybackCompleted;

        private List<BaseItemDto> _playlist = new List<BaseItemDto>();

        public InternalDirectShowPlayer(ILogManager logManager, IHiddenWindow hiddenWindow, IPresentationManager presentation, IApiClient apiClient, IPlaybackManager playbackManager, ITheaterConfigurationManager config, IIsoManager isoManager)
        {
            _logger = logManager.GetLogger("InternalDirectShowPlayer");
            _hiddenWindow = hiddenWindow;
            _presentation = presentation;
            _apiClient = apiClient;
            _playbackManager = playbackManager;
            _config = config;
            _isoManager = isoManager;
        }

        public IReadOnlyList<BaseItemDto> Playlist
        {
            get
            {
                return _playlist;
            }
        }

        public int CurrentPlaylistIndex { get; private set; }

        public PlayOptions CurrentPlayOptions { get; private set; }

        public BaseItemDto CurrentMedia
        {
            get
            {
                return _playlist.Count > 0 ? _playlist[CurrentPlaylistIndex] : null;
            }
        }

        public bool CanSeek
        {
            get { return true; }
        }

        public bool CanPause
        {
            get { return true; }
        }

        public bool CanQueue
        {
            get { return true; }
        }

        public bool CanTrackProgress
        {
            get { return true; }
        }

        public string Name
        {
            get { return "Internal Player"; }
        }

        public bool SupportsMultiFilePlayback
        {
            get { return true; }
        }

        /// <summary>
        /// Gets a value indicating whether this instance can select audio track.
        /// </summary>
        /// <value><c>true</c> if this instance can select audio track; otherwise, <c>false</c>.</value>
        public virtual bool CanSelectAudioTrack
        {
            get { return true; }
        }

        /// <summary>
        /// Gets a value indicating whether this instance can select subtitle track.
        /// </summary>
        /// <value><c>true</c> if this instance can select subtitle track; otherwise, <c>false</c>.</value>
        public virtual bool CanSelectSubtitleTrack
        {
            get { return true; }
        }

        public PlayState PlayState
        {
            get
            {
                if (_mediaPlayer == null)
                {
                    return PlayState.Idle;
                }

                return _mediaPlayer.PlayState;
            }
        }

        public long? CurrentPositionTicks
        {
            get
            {
                if (_mediaPlayer != null)
                {
                    return _mediaPlayer.CurrentPositionTicks;
                }

                return null;
            }
        }

        public long? CurrentDurationTicks
        {
            get
            {
                if (_mediaPlayer != null)
                {
                    return _mediaPlayer.CurrentDurationTicks;
                }

                return null;
            }
        }

        public bool CanPlayByDefault(BaseItemDto item)
        {
            return item.IsVideo || item.IsAudio;
        }

        public bool CanPlayMediaType(string mediaType)
        {
            return new[] { MediaType.Video, MediaType.Audio }.Contains(mediaType, StringComparer.OrdinalIgnoreCase);
        }

        public async Task Play(PlayOptions options)
        {
            CurrentPlaylistIndex = 0;
            CurrentPlayOptions = options;

            _playlist = options.Items.ToList();

            try
            {
                InvokeOnPlayerThread(() =>
                {
                    _mediaPlayer = new DirectShowPlayer(_logger, _hiddenWindow, this, _presentation.WindowHandle);

                    //HideCursor();
                });

                await PlayTrack(0, options.StartPositionTicks);
            }
            catch (Exception ex)
            {
                _logger.ErrorException("Error beginning playback", ex);

                DisposePlayer();

                throw;
            }
        }

        private async Task PlayTrack(int index, long? startPositionTicks)
        {
            var previousMedia = CurrentMedia;
            var previousIndex = CurrentPlaylistIndex;
            var endingTicks = CurrentPositionTicks;

            var options = CurrentPlayOptions;

            var playableItem = await GetPlayableItem(options.Items[index], startPositionTicks, CancellationToken.None);

            try
            {
                var enableMadVr = EnableMadvr(options);
                var enableReclock = EnableReclock(options);

                InvokeOnPlayerThread(() => _mediaPlayer.Play(playableItem, enableReclock, enableMadVr, false, _config.Configuration.InternalPlayerConfiguration.EnableXySubFilter, _config.Configuration.InternalPlayerConfiguration.VideoConfig, _config.Configuration.InternalPlayerConfiguration.AudioConfig));
            }
            catch
            {
                DisposeMount(playableItem);

                throw;
            }

            CurrentPlaylistIndex = index;

            if (startPositionTicks.HasValue && startPositionTicks.Value > 0)
            {
                InvokeOnPlayerThread(() => _mediaPlayer.Seek(startPositionTicks.Value));
            }

            if (previousMedia != null)
            {
                var args = new MediaChangeEventArgs
                {
                    Player = this,
                    NewPlaylistIndex = index,
                    NewMedia = CurrentMedia,
                    PreviousMedia = previousMedia,
                    PreviousPlaylistIndex = previousIndex,
                    EndingPositionTicks = endingTicks
                };
                // can't InvokeOnPlayerThread because InvokeRequired returns false
                 _presentation.Window.Dispatcher.Invoke
                (
                    () => EventHelper.FireEventIfNotNull(MediaChanged, this, args, _logger)
                );
            }
        }

        private async Task<PlayableItem> GetPlayableItem(BaseItemDto item, long? startTimeTicks, CancellationToken cancellationToken)
        {
            IIsoMount mountedIso = null;

            if (item.VideoType.HasValue && item.VideoType.Value == VideoType.Iso && item.IsoType.HasValue && _isoManager.CanMount(item.Path))
            {
                try
                {
                    mountedIso = await _isoManager.Mount(item.Path, cancellationToken);
                }
                catch (Exception ex)
                {
                    _logger.ErrorException("Error mounting iso {0}", ex, item.Path);
                }
            }

            return new PlayableItem
            {
                OriginalItem = item,
                PlayablePath = PlayablePathBuilder.GetPlayablePath(item, mountedIso, _apiClient, startTimeTicks),
                IsoMount = mountedIso
            };
        }

        /// <summary>
        /// Enables the madvr.
        /// </summary>
        /// <param name="options">The options.</param>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise</returns>
        private bool EnableMadvr(PlayOptions options)
        {
            var video = options.Items.First();

            if (!video.IsVideo)
            {
                return false;
            }

            if (!_config.Configuration.InternalPlayerConfiguration.EnableMadvr)
            {
                return false;
            }

            if (!options.GoFullScreen)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Enables the madvr.
        /// </summary>
        /// <param name="options">The options.</param>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise</returns>
        private bool EnableReclock(PlayOptions options)
        {
            var video = options.Items.First();

            if (!video.IsVideo)
            {
                return false;
            }

            if (!_config.Configuration.InternalPlayerConfiguration.EnableReclock)
            {
                return false;
            }

            if (!options.GoFullScreen)
            {
                return false;
            }
            
            return true;
        }

        private void DisposePlayer()
        {
            if (_mediaPlayer != null)
            {
                InvokeOnPlayerThread(() => {
                    _mediaPlayer.Dispose();
                    _mediaPlayer = null; //force the object to get cleaned up
                });
            }
        }

        public void Pause()
        {
            lock (_commandLock)
            {
                if (_mediaPlayer != null)
                {
                    InvokeOnPlayerThread(_mediaPlayer.Pause);
                }
            }
        }

        public void UnPause()
        {
            lock (_commandLock)
            {
                if (_mediaPlayer != null)
                {
                    InvokeOnPlayerThread(_mediaPlayer.Unpause);
                }
            }
        }

        private readonly object _commandLock = new object();

        public void Stop()
        {
            lock (_commandLock)
            {
                if (_mediaPlayer != null)
                {
                    InvokeOnPlayerThread(() => _mediaPlayer.Stop(TrackCompletionReason.Stop, null));
                }
            }
        }

        public void Seek(long positionTicks)
        {
            lock (_commandLock)
            {
                if (_mediaPlayer != null)
                {
                    InvokeOnPlayerThread(() => _mediaPlayer.Seek(positionTicks));
                }
            }
        }

        public void SetRate(double rate)
        {
            lock (_commandLock)
            {
                if (_mediaPlayer != null)
                {
                    InvokeOnPlayerThread(() => _mediaPlayer.SetRate(rate));
                }
            }
        }

        /// <summary>
        /// Occurs when [play state changed].
        /// </summary>
        public event EventHandler PlayStateChanged;
        internal void OnPlayStateChanged()
        {
            EventHelper.FireEventIfNotNull(PlayStateChanged, this, EventArgs.Empty, _logger);
        }

        private void DisposeMount(PlayableItem media)
        {
            if (media.IsoMount != null)
            {
                try
                {
                    media.IsoMount.Dispose();
                    media.IsoMount = null;
                }
                catch (Exception ex)
                {
                    _logger.ErrorException("Error unmounting iso {0}", ex, media.IsoMount.MountedPath);
                }
            }
        }

        internal async void OnPlaybackStopped(PlayableItem media, long? positionTicks, TrackCompletionReason reason, int? newTrackIndex)
        {
            DisposeMount(media);

            if (reason == TrackCompletionReason.Ended || reason == TrackCompletionReason.ChangeTrack)
            {
                var nextIndex = newTrackIndex ?? (CurrentPlaylistIndex + 1);

                if (nextIndex < CurrentPlayOptions.Items.Count)
                {
                    await PlayTrack(nextIndex, null);
                    return;
                }
            }

            DisposePlayer();

            var args = new PlaybackStopEventArgs
            {
                Player = this,
                Playlist = _playlist,
                EndingMedia = media.OriginalItem,
                EndingPositionTicks = positionTicks

            };

            EventHelper.FireEventIfNotNull(PlaybackCompleted, this, args, _logger);

            _playbackManager.ReportPlaybackCompleted(args);
        }

        public void ChangeTrack(int newIndex)
        {
            _mediaPlayer.Stop(TrackCompletionReason.ChangeTrack, newIndex);
            InvokeOnPlayerThread(() => _mediaPlayer.Stop(TrackCompletionReason.ChangeTrack, newIndex));
        }

        public IReadOnlyList<SelectableMediaStream> SelectableStreams
        {
            get { return _mediaPlayer.GetSelectableStreams(); }
        }

        public void ChangeAudioStream(SelectableMediaStream track)
        {
            InvokeOnPlayerThread(() => _mediaPlayer.SetAudioTrack(track));
        }

        public void ChangeSubtitleStream(SelectableMediaStream track)
        {
            InvokeOnPlayerThread(() => _mediaPlayer.SetSubtitleTrack(track));
        }

        public void RemoveSubtitles()
        {

        }

        private void InvokeOnPlayerThread(Action action)
        {
            if (_hiddenWindow.Form.InvokeRequired)
            {
                _hiddenWindow.Form.Invoke(action);
            }
            else
            {
                action();
            }
        }
    }

    public enum TrackCompletionReason
    {
        Stop,
        Ended,
        ChangeTrack
    }
}
