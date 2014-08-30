using MediaBrowser.Common.Events;
using MediaBrowser.Common.Net;
using MediaBrowser.Model.ApiClient;
using MediaBrowser.Model.Dto;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.IO;
using MediaBrowser.Model.Logging;
using MediaBrowser.Theater.Interfaces.Configuration;
using MediaBrowser.Theater.Interfaces.Playback;
using MediaBrowser.Theater.Interfaces.Presentation;
using MediaBrowser.Theater.Interfaces.Session;
using MediaBrowser.Theater.Interfaces.UserInput;
using MediaBrowser.Theater.Presentation.Playback;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace MediaBrowser.Theater.DirectShow
{
    public class InternalDirectShowPlayer : IInternalMediaPlayer, IVideoPlayer, IDisposable
    {
        private DirectShowPlayer _mediaPlayer;

        private readonly ILogger _logger;
        private readonly IHiddenWindow _hiddenWindow;
        private readonly IPresentationManager _presentation;
        private readonly ISessionManager _sessionManager;
        private readonly IApiClient _apiClient;
        private readonly IPlaybackManager _playbackManager;
        private readonly ITheaterConfigurationManager _config;
        private readonly IIsoManager _isoManager;
        private readonly IUserInputManager _inputManager;
        private readonly IHttpClient _httpClient;
        private readonly IZipClient _zipClient;
        private URCOMLoader _privateCom = null;

        public event EventHandler<MediaChangeEventArgs> MediaChanged;

        public event EventHandler<PlaybackStopEventArgs> PlaybackCompleted;

        public URCOMLoader PrivateCom
        {
            get
            {
                return _privateCom;
            }
        }

        private List<BaseItemDto> _playlist = new List<BaseItemDto>();

        public InternalDirectShowPlayer(ILogManager logManager, IHiddenWindow hiddenWindow, IPresentationManager presentation, ISessionManager sessionManager, IApiClient apiClient, IPlaybackManager playbackManager, ITheaterConfigurationManager config, IIsoManager isoManager, IUserInputManager inputManager, IZipClient zipClient, IHttpClient httpClient)
        {
            _logger = logManager.GetLogger("InternalDirectShowPlayer");
            _hiddenWindow = hiddenWindow;
            _presentation = presentation;
            _sessionManager = sessionManager;
            _apiClient = apiClient;
            _httpClient = httpClient;
            _playbackManager = playbackManager;
            _config = config;
            _isoManager = isoManager;
            _inputManager = inputManager;
            _zipClient = zipClient;

            _config.Configuration.InternalPlayerConfiguration.VideoConfig.SetDefaults();
            _config.Configuration.InternalPlayerConfiguration.AudioConfig.SetDefaults();
            _config.Configuration.InternalPlayerConfiguration.SubtitleConfig.SetDefaults();
            _config.Configuration.InternalPlayerConfiguration.COMConfig.SetDefaults();

            //use a static object so we keep the libraries in the same place. Doesn't usually matter, but the EVR Presenter does some COM hooking that has problems if we change the lib address.
            if (_privateCom == null)
                _privateCom = new URCOMLoader(_config, zipClient);
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


        public bool CanSetAudioStreamIndex
        {
            get { return true; }
        }


        public bool CanSetSubtitleStreamIndex
        {
            get { return true; }
        }

        public bool CanAcceptNavigationCommands
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

        /// <summary>
        /// Get the current subtitle index.
        /// </summary>
        /// <value>The current subtitle index.</value>
        public int? CurrentSubtitleStreamIndex
        {
            get
            {
                if (_mediaPlayer != null)
                {
                    return _mediaPlayer.CurrentSubtitleStreamIndex;
                }

                return null;
            }
        }

        /// <summary>
        /// Get the current audio index.
        /// </summary>
        /// <value>The current audio index.</value>
        public int? CurrentAudioStreamIndex
        {
            get
            {
                if (_mediaPlayer != null)
                {
                    return _mediaPlayer.CurrentAudioStreamIndex;
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
                //var enableReclock = EnableReclock(options);

                InvokeOnPlayerThread(() =>
                {
                    //create a fresh DS Player everytime we want one
                    DisposePlayer();
                    _mediaPlayer = new DirectShowPlayer(_logger, _hiddenWindow, this, _presentation.WindowHandle, _sessionManager, _config, _inputManager, _apiClient, _zipClient, _httpClient);
                    _mediaPlayer.Play(playableItem, enableMadVr, false);

                }, true);
            }
            catch
            {
                OnPlaybackStopped(playableItem, null, TrackCompletionReason.Failure, null);

                throw;
            }

            CurrentPlaylistIndex = index;

            if (startPositionTicks.HasValue && startPositionTicks.Value > 0)
            {
                InvokeOnPlayerThread(() => _mediaPlayer.Seek(startPositionTicks.Value));
            }

            if (previousMedia != null && MediaChanged != null)
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

                _presentation.Window.Dispatcher.Invoke
                (
                    () => MediaChanged(this, args)
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
                PlayablePath = PlayablePathBuilder.GetPlayablePath(item, mountedIso, _apiClient, startTimeTicks, _config.Configuration.MaxStreamingBitrate),
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

            if (!_config.Configuration.InternalPlayerConfiguration.VideoConfig.EnableMadvr)
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
        //private bool EnableReclock(PlayOptions options)
        //{
        //    var video = options.Items.First();

        //    if (!video.IsVideo)
        //    {
        //        return false;
        //    }

        //    if (!_config.Configuration.InternalPlayerConfiguration.EnableReclock)
        //    {
        //        return false;
        //    }

        //    if (!options.GoFullScreen)
        //    {
        //        return false;
        //    }

        //    return true;
        //}

        private void DisposePlayer()
        {
            if (_mediaPlayer != null)
            {
                InvokeOnPlayerThread(() =>
                {
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

            StopTranscoding(media);

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

        private async void StopTranscoding(PlayableItem media)
        {
            // If streaming video, stop the transcoder
            if (media.IsVideo && media.PlayablePath.IndexOf("://", StringComparison.OrdinalIgnoreCase) != -1)
            {
                try
                {
                    await _apiClient.StopTranscodingProcesses(_apiClient.DeviceId);
                }
                catch
                {

                }
            }
        }

        public void ChangeTrack(int newIndex)
        {
            _mediaPlayer.Stop(TrackCompletionReason.ChangeTrack, newIndex); // don't invoke on player thread
        }

        public void NextTrack()
        {
            var nextIndex = CurrentPlaylistIndex + 1;
            if (nextIndex < CurrentPlayOptions.Items.Count)
            {
                ChangeTrack(nextIndex);
            }
        }

        public void PreviousTrack()
        {
            var previousIndex = CurrentPlaylistIndex - 1;
            if (previousIndex >= 0 && previousIndex <= CurrentPlayOptions.Items.Count)
            {
                ChangeTrack(previousIndex);
            }
        }

        public IReadOnlyList<SelectableMediaStream> SelectableStreams
        {
            get { return _mediaPlayer.GetSelectableStreams(); }
        }

        public void ChangeAudioStream(SelectableMediaStream track)
        {
            InvokeOnPlayerThread(() => _mediaPlayer.SetAudioTrack(track));
        }


        public void SetSubtitleStreamIndex(int subtitleStreamIndex)
        {
            InvokeOnPlayerThread(() => _mediaPlayer.SetSubtitleStreamIndex(subtitleStreamIndex));
        }

        public void NextSubtitleStream()
        {
            InvokeOnPlayerThread(() => _mediaPlayer.NextSubtitleStream());
        }

        public void SetAudioStreamIndex(int audioStreamIndex)
        {
            InvokeOnPlayerThread(() => _mediaPlayer.SetAudioStreamIndex(audioStreamIndex));
        }

        public void NextAudioStream()
        {
            InvokeOnPlayerThread(() => _mediaPlayer.NextAudioStream());
        }

        public void ChangeSubtitleStream(SelectableMediaStream track)
        {
            InvokeOnPlayerThread(() => _mediaPlayer.SetSubtitleStream(track));
        }

        public void RemoveSubtitles()
        {

        }

        private void InvokeOnPlayerThread(Action action, bool throwOnError = false)
        {
            try
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
            catch (Exception ex)
            {
                _logger.ErrorException("InvokeOnPlayerThread", ex);

                if (throwOnError) throw ex;
            }
        }

        #region IDisposable

        private bool _disposed = false;
        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (!_disposed)
                {
                    if (_privateCom != null)
                        _privateCom.Dispose();
                    _privateCom = null;

                    _disposed = true;
                }
            }
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion
    }

    public enum TrackCompletionReason
    {
        Stop,
        Ended,
        ChangeTrack,
        Failure
    }
}
