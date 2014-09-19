using MediaBrowser.Common.Events;
using MediaBrowser.Model.ApiClient;
using MediaBrowser.Model.Dto;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.Logging;
using MediaBrowser.Theater.Interfaces.Configuration;
using MediaBrowser.Theater.Interfaces.Playback;
using MediaBrowser.Theater.Interfaces.Presentation;
using MediaBrowser.Theater.Interfaces.UserInput;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using Vlc.DotNet.Core;
using Vlc.DotNet.Core.Interops.Signatures.LibVlc.MediaListPlayer;
using Vlc.DotNet.Core.Medias;
using Vlc.DotNet.Forms;
using ILogger = MediaBrowser.Model.Logging.ILogger;

namespace MediaBrowser.Theater.Vlc
{
    /// <summary>
    /// Class NVlcPlayer
    /// </summary>
    public class NVlcPlayer : IInternalMediaPlayer, MediaBrowser.Theater.Interfaces.Playback.IVideoPlayer, IDisposable
    {
        public event EventHandler<MediaChangeEventArgs> MediaChanged;

        public event EventHandler<PlaybackStopEventArgs> PlaybackCompleted;

        /// <summary>
        /// The _hidden window
        /// </summary>
        private readonly IHiddenWindow _hiddenWindow;
        private readonly ILogger _logger;
        private readonly IPlaybackManager _playbackManager;
        private readonly ITheaterConfigurationManager _config;
        private readonly IUserInputManager _userInput;
        private readonly IApiClient _apiClient;
        private readonly IPresentationManager _presentation;

        private VlcControl _vlcControl;

        /// <summary>
        /// The _task result
        /// </summary>
        private readonly Task _taskResult = Task.FromResult(true);

        /// <summary>
        /// Initializes a new instance of the <see cref="NVlcPlayer" /> class.
        /// </summary>
        /// <param name="hiddenWindow">The hidden window.</param>
        /// <param name="logManager">The log manager.</param>
        /// <param name="playbackManager">The playback manager.</param>
        public NVlcPlayer(IHiddenWindow hiddenWindow, ILogManager logManager, IPlaybackManager playbackManager, ITheaterConfigurationManager config, IUserInputManager userInput, IApiClient apiClient, IPresentationManager presentation)
        {
            _hiddenWindow = hiddenWindow;
            _playbackManager = playbackManager;
            _config = config;
            _userInput = userInput;
            _apiClient = apiClient;
            _presentation = presentation;

            _logger = logManager.GetLogger(Name);
        }

        private List<BaseItemDto> _playlist = new List<BaseItemDto>();
        public IReadOnlyList<BaseItemDto> Playlist
        {
            get
            {
                return _playlist;
            }
        }

        public bool RequiresGlobalMouseHook
        {
            get
            {
                return true;
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

        /// <summary>
        /// Gets a value indicating whether this instance can seek.
        /// </summary>
        /// <value><c>true</c> if this instance can seek; otherwise, <c>false</c>.</value>
        public bool CanSeek
        {
            get
            {
                return _vlcControl != null && CurrentMedia != null & _vlcControl.IsSeekable;
            }
        }

        /// <summary>
        /// Gets a value indicating whether this instance can track progress.
        /// </summary>
        /// <value><c>true</c> if this instance can track progress; otherwise, <c>false</c>.</value>
        public bool CanTrackProgress
        {
            get { return true; }
        }

        /// <summary>
        /// Gets a value indicating whether this instance can pause.
        /// </summary>
        /// <value><c>true</c> if this instance can pause; otherwise, <c>false</c>.</value>
        public bool CanPause
        {
            get { return true; }
        }

        /// <summary>
        /// Gets a value indicating whether this instance can queue.
        /// </summary>
        /// <value><c>true</c> if this instance can queue; otherwise, <c>false</c>.</value>
        public bool CanQueue
        {
            get { return false; }
        }

        /// <summary>
        /// Gets a value indicating whether this instance can control volume.
        /// </summary>
        /// <value><c>true</c> if this instance can control volume; otherwise, <c>false</c>.</value>
        public bool CanControlVolume
        {
            get { return true; }
        }

        /// <summary>
        /// Gets the name.
        /// </summary>
        /// <value>The name.</value>
        public string Name
        {
            get { return "Internal Vlc Player"; }
        }

        /// <summary>
        /// Gets a value indicating whether [supports multi file playback].
        /// </summary>
        /// <value><c>true</c> if [supports multi file playback]; otherwise, <c>false</c>.</value>
        public bool SupportsMultiFilePlayback
        {
            get { return true; }
        }

        private bool _isPaused;
        /// <summary>
        /// Gets the state of the play.
        /// </summary>
        /// <value>The state of the play.</value>
        public PlayState PlayState
        {
            get
            {
                if (_playlist == null || _playlist.Count == 0)
                {
                    return PlayState.Idle;
                }

                if (_isPaused)
                {
                    return PlayState.Paused;
                }

                return PlayState.Playing;
            }
        }

        private bool _initialized;
        /// <summary>
        /// Ensures the media player created.
        /// </summary>
        private void EnsureMediaPlayerCreated()
        {
            if (_initialized)
            {
                return;
            }

            //VlcContext.LibVlcDllsPath = @"D:\\Development\\MediaBrowser.Theater\\MediaBrowser.UI\\bin\\x86\\Debug";
            //VlcContext.LibVlcDllsPath = @"C:\\Program Files (x86)\\VideoLAN\\VLC";

            // Set the vlc plugins directory path
            //VlcContext.LibVlcPluginsPath = @"C:\\Program Files (x86)\\VideoLAN\\VLC\\plugins";

            /* Setting up the configuration of the VLC instance.
                     * You can use any available command-line option using the AddOption function (see last two options). 
                     * A list of options is available at 
                     *     http://wiki.videolan.org/VLC_command-line_help
                     * for example. */

            // Ignore the VLC configuration file
            VlcContext.StartupOptions.IgnoreConfig = true;

            // Enable file based logging
            VlcContext.StartupOptions.LogOptions.LogInFile = false;

            // Shows the VLC log console (in addition to the applications window)
            VlcContext.StartupOptions.LogOptions.ShowLoggerConsole = false;

            // Set the log level for the VLC instance
            VlcContext.StartupOptions.LogOptions.Verbosity = VlcLogVerbosities.Debug;

            var configStrings = new List<string>
                {
                "-I", 
                "dummy",  
		        "--ignore-config", 
                "--no-osd",
                "--disable-screensaver",
                "--no-video-title-show"
                };

            if (_config.Configuration.VlcConfiguration.EnableGpuAcceleration)
            {
                configStrings.Add("--ffmpeg-hw");
            }

            foreach (var opt in configStrings)
            {
                VlcContext.StartupOptions.AddOption(opt);
            }

            // Initialize the VlcContext
            VlcContext.Initialize();

            _initialized = true;
        }

        /// <summary>
        /// Plays the specified options.
        /// </summary>
        /// <param name="options">The options.</param>
        /// <returns>Task.</returns>
        public async Task Play(PlayOptions options)
        {
            InvokeOnPlayerThread(() => PlayInternal(options));
        }

        private void PlayInternal(PlayOptions options)
        {
            EnsureMediaPlayerCreated();

            CurrentPlaylistIndex = 0;
            CurrentPlayOptions = options;
            _duration = null;

            _playlist = options.Items.ToList();
            _isPaused = false;

            try
            {
                //var media = new PathMedia(@"D:\\Video\\TV\\30 Rock\\Season 1\\30 Rock - 1x02 - The Aftermath.mkv");
                var media = new LocationMedia(options.Items.First().Path);
                //media.StateChanged +=
                //    delegate(MediaBase s, VlcEventArgs<States> args)
                //    {
                //        if (args.Data == States.Ended)
                //        {
                //            var subItems = media.SubItems;
                //            if (subItems.Count > 0)
                //            {
                //                _vlcControl.Play(subItems[0]);
                //            }
                //        }
                //    };
                //media.MediaSubItemAdded +=
                //    delegate(MediaBase s, VlcEventArgs<MediaBase> args)
                //    {
                //        _vlcControl.Media = args.Data;
                //        _vlcControl.Play();
                //    };

                _vlcControl = new VlcControl();

                _vlcControl.Media = media;
                _vlcControl.PlaybackMode = PlaybackModes.Loop;
                _vlcControl.Stopped += _vlcControl_Stopped;
                _vlcControl.Paused += _vlcControl_Paused;
                _vlcControl.Playing += _vlcControl_Playing;
                _vlcControl.LengthChanged += _vlcControl_LengthChanged;
                _vlcControl.Play();

                _vlcControl.SetHandle(_hiddenWindow.Form.Handle);

                _userInput.GlobalKeyDown += _userInput_KeyDown;
            }
            catch (Exception ex)
            {
                _logger.ErrorException("Error beginning playback", ex);

                DisposePlayer();
            }
        }

        void _vlcControl_LengthChanged(VlcControl sender, VlcEventArgs<long> e)
        {
            _duration = _duration ?? e.Data;
        }

        void _vlcControl_Playing(VlcControl sender, VlcEventArgs<EventArgs> e)
        {
            OnPlayStateChanged();
            _isPaused = false ;
        }

        void _vlcControl_Paused(VlcControl sender, VlcEventArgs<EventArgs> e)
        {
            OnPlayStateChanged();
            _isPaused = true;
        }

        void _vlcControl_Stopped(VlcControl sender, VlcEventArgs<EventArgs> e)
        {
            var playlist = _playlist.ToList();
            var index = CurrentPlaylistIndex;
            var ticks = CurrentPositionTicks;

            DisposePlayer();

            var media = index != -1 && playlist.Count > 0 ? playlist[index] : null;

            var args = new PlaybackStopEventArgs
            {
                Playlist = playlist,
                Player = this,
                EndingPlaylistIndex = index,
                EndingPositionTicks = ticks,
                EndingMedia = media
            };

            EventHelper.QueueEventIfNotNull(PlaybackCompleted, this, args, _logger);

            _playbackManager.ReportPlaybackCompleted(args);
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

        void _userInput_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Pause:
                    Pause();
                    break;
                case Keys.VolumeDown:
                    _vlcControl.AudioProperties.Volume -= 5;
                    break;
                case Keys.VolumeUp:
                    _vlcControl.AudioProperties.Volume += 5;
                    _vlcControl.AudioProperties.IsMute = false;
                    break;
                case Keys.VolumeMute:
                    _vlcControl.AudioProperties.IsMute = !_vlcControl.AudioProperties.IsMute;
                    break;
                case Keys.MediaNextTrack:
                    break;
                case Keys.MediaPlayPause:
                    Pause();
                    break;
                case Keys.MediaPreviousTrack:
                    break;
                case Keys.MediaStop:
                    Stop();
                    break;
                default:
                    return;
            }
        }

        /// <summary>
        /// Determines whether this instance can play the specified item.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <returns><c>true</c> if this instance can play the specified item; otherwise, <c>false</c>.</returns>
        public bool CanPlayByDefault(BaseItemDto item)
        {
            return false;
        }

        public bool CanPlayMediaType(string mediaType)
        {
            return new[] { MediaType.Video, MediaType.Audio }.Contains(mediaType, StringComparer.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Stops this instance.
        /// </summary>
        /// <returns>Task.</returns>
        public void Stop()
        {
            InvokeOnPlayerThread(() => _vlcControl.Stop());
        }

        /// <summary>
        /// Sets the volume.
        /// </summary>
        /// <param name="volume">The volume.</param>
        /// <returns>Task.</returns>
        public Task SetVolume(int volume)
        {
            if (volume > 0 && IsMuted)
            {
                _vlcControl.AudioProperties.IsMute = false;
            }

            _vlcControl.AudioProperties.Volume = volume;

            OnVolumeChanged();

            return _taskResult;
        }

        /// <summary>
        /// Mutes this instance.
        /// </summary>
        /// <returns>Task.</returns>
        public Task Mute()
        {
            _vlcControl.AudioProperties.IsMute = true;

            OnVolumeChanged();

            return _taskResult;
        }

        /// <summary>
        /// Uns the mute.
        /// </summary>
        /// <returns>Task.</returns>
        public Task UnMute()
        {
            _vlcControl.AudioProperties.IsMute = false;

            OnVolumeChanged();

            return _taskResult;
        }

        /// <summary>
        /// Pauses this instance.
        /// </summary>
        /// <returns>Task.</returns>
        public void Pause()
        {
            _vlcControl.Pause();
        }

        /// <summary>
        /// Uns the pause.
        /// </summary>
        /// <returns>Task.</returns>
        public void UnPause()
        {
            _vlcControl.Play();
        }

        /// <summary>
        /// Seeks the specified position ticks.
        /// </summary>
        /// <param name="positionTicks">The position ticks.</param>
        /// <returns>Task.</returns>
        public void Seek(long positionTicks)
        {
            if (_vlcControl != null)
            {
                _vlcControl.Time = TimeSpan.FromTicks(positionTicks);
            }
        }

        /// <summary>
        /// Gets the current position ticks.
        /// </summary>
        /// <value>The current position ticks.</value>
        public long? CurrentPositionTicks
        {
            get { return _vlcControl == null ? (long?)null : _vlcControl.Time.Ticks; }
        }

        /// <summary>
        /// Gets a value indicating whether this instance is muted.
        /// </summary>
        /// <value><c>true</c> if this instance is muted; otherwise, <c>false</c>.</value>
        public bool IsMuted
        {
            get { return _vlcControl.AudioProperties.IsMute; }
        }

        /// <summary>
        /// Gets the volume.
        /// </summary>
        /// <value>The volume.</value>
        public int Volume
        {
            get { return _vlcControl.AudioProperties.Volume; }
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
                DisposePlayer();

                VlcContext.CloseAll();
            }
        }

        /// <summary>
        /// Disposes the player.
        /// </summary>
        private void DisposePlayer()
        {
            _userInput.GlobalKeyDown -= _userInput_KeyDown;

            CurrentPlayOptions = null;
            CurrentPlaylistIndex = 0;
            _duration = null;

            _playlist = new List<BaseItemDto>();
        }

        #region VolumeChanged
        /// <summary>
        /// Occurs when [volume changed].
        /// </summary>
        public event EventHandler VolumeChanged;
        protected void OnVolumeChanged()
        {
            EventHelper.FireEventIfNotNull(VolumeChanged, this, EventArgs.Empty, _logger);
        }
        #endregion

        #region PlayStateChanged
        /// <summary>
        /// Occurs when [play state changed].
        /// </summary>
        public event EventHandler PlayStateChanged;
        protected void OnPlayStateChanged()
        {
            EventHelper.FireEventIfNotNull(PlayStateChanged, this, EventArgs.Empty, _logger);
        }
        #endregion


        public bool CanSetAudioStreamIndex
        {
            get { return false; }
        }

        public bool CanSetSubtitleStreamIndex
        {
            get { return false; }
        }

        public bool CanAcceptNavigationCommands
        {
            get { return false; }
        }

        private long? _duration;
        public long? CurrentDurationTicks
        {
            get
            {
                return _duration;
            }
        }

        public int? CurrentSubtitleStreamIndex
        {
            get { return null; }
        }

        public int? CurrentAudioStreamIndex
        {
            get { return null; }
        }

        public void ChangeTrack(int newIndex)
        {
        }

        public void NextTrack()
        {
        }

        public void PreviousTrack()
        {
        }

        public void SetRate(double rate)
        {
        }

        public void SetSubtitleStreamIndex(int subtitleStreamIndex)
        {
        }

        public void NextSubtitleStream()
        {
        }

        public void SetAudioStreamIndex(int audioStreamIndex)
        {
        }

        public void NextAudioStream()
        {
        }

        public bool CanSelectAudioTrack
        {
            get { return false; }
        }

        public bool CanSelectSubtitleTrack
        {
            get { return false; }
        }

        public IReadOnlyList<SelectableMediaStream> SelectableStreams
        {
            get { return new List<SelectableMediaStream>(); }
        }

        public void ChangeAudioStream(SelectableMediaStream track)
        {
        }

        public void ChangeSubtitleStream(SelectableMediaStream track)
        {
        }

        public void RemoveSubtitles()
        {
        }
    }
}
