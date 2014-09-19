using Declarations;
using Declarations.Enums;
using Declarations.Events;
using Declarations.Media;
using Declarations.Players;
using Implementation;
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
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
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
        /// The _media player factory
        /// </summary>
        private MediaPlayerFactory _mediaPlayerFactory;

        private Declarations.Players.IVideoPlayer _videoPlayer;
        private IMedia _media;

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
            get { return true; }
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
            get { return true; }
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

        /// <summary>
        /// Gets the state of the play.
        /// </summary>
        /// <value>The state of the play.</value>
        public PlayState PlayState
        {
            get
            {
                if (_videoPlayer == null)
                {
                    return PlayState.Idle;
                }

                if (_videoPlayer.PlaybackRate.Equals(0))
                {
                    return PlayState.Paused;
                }

                return PlayState.Playing;
            }
        }

        /// <summary>
        /// Ensures the media player created.
        /// </summary>
        private void EnsureMediaPlayerCreated()
        {
            if (_mediaPlayerFactory != null)
            {
                return;
            }

            //_windowsFormsPanel = new Panel
            //{
            //    BackColor = Color.Black
            //};

            //_hiddenWindow.Form.Controls.Clear();
            //_hiddenWindow.Form.Controls.Add(_windowsFormsPanel);

            var configStrings = new List<string>
                {
                "-I", 
                "dummy",  
		        "--ignore-config", 
                "--no-osd",
                "--disable-screensaver",
		        "--plugin-path=./plugins"
                };

            if (_config.Configuration.VlcConfiguration.EnableGpuAcceleration)
            {
                configStrings.Add("--ffmpeg-hw");
            }

            _mediaPlayerFactory = new MediaPlayerFactory(configStrings.ToArray());
        }

        /// <summary>
        /// Plays the specified options.
        /// </summary>
        /// <param name="options">The options.</param>
        /// <returns>Task.</returns>
        public async Task Play(PlayOptions options)
        {
            InvokeOnPlayerThread(async () => await PlayInternal(options));
        }

        private bool _initialEndEventFired = false;
        private Task PlayInternal(PlayOptions options)
        {
            EnsureMediaPlayerCreated();

            CurrentPlaylistIndex = 0;
            CurrentPlayOptions = options;

            _playlist = options.Items.ToList();

            try
            {
                //_media = _mediaPlayerFactory.CreateMedia<IMedia>(@"D:\\Video\\TV\\30 Rock\\Season 1\\30 Rock - 1x02 - The Aftermath.mkv");
                _media = _mediaPlayerFactory.CreateMedia<IMedia>(options.Items[0].Path);
                _videoPlayer = _mediaPlayerFactory.CreatePlayer<Declarations.Players.IVideoPlayer>();
                _videoPlayer.Open(_media);
                _videoPlayer.Play();

                _videoPlayer.WindowHandle = _hiddenWindow.Form.Handle;
                _videoPlayer.DeviceType = GetAudioDeviceType();
                _videoPlayer.Events.PlayerPaused += Events_PlayerPaused;
                _videoPlayer.Events.PlayerPlaying += Events_PlayerPaused;
                _videoPlayer.Events.PlayerStopped += Events_PlayerStopped;
                _videoPlayer.Events.MediaEnded += Events_MediaEnded;

                _videoPlayer.Play();

                var position = options.StartPositionTicks;

                if (position > 0)
                {
                    _videoPlayer.Time = Convert.ToInt64(TimeSpan.FromTicks(position).TotalMilliseconds);
                }

                //_mediaListPlayer.MediaListPlayerEvents.MediaListPlayerNextItemSet += MediaListPlayerEvents_MediaListPlayerNextItemSet;

                _userInput.GlobalKeyDown += _userInput_KeyDown;
            }
            catch (Exception ex)
            {
                _logger.ErrorException("Error beginning playback", ex);

                DisposePlayer();
            }

            return _taskResult;
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

        private void PlaySubItem()
        {
            InvokeOnPlayerThread(async () =>
            {
                var subItems = _media.SubItems;

                if (subItems != null)
                {
                    var subItem = subItems.FirstOrDefault();

                    if (subItem != null)
                    {
                        _videoPlayer.Open(subItem);
                        _videoPlayer.Play();
                    }
                }
            });
        }

        void Events_MediaEnded(object sender, EventArgs e)
        {
            if (!_initialEndEventFired)
            {
                _initialEndEventFired = true;
                PlaySubItem();
                return;
            }

            Stop();
        }

        void Events_PlayerPaused(object sender, EventArgs e)
        {
            //PlaySubItem();
            OnPlayStateChanged();
        }

        void _userInput_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Pause:
                    _videoPlayer.Pause();
                    break;
                case Keys.VolumeDown:
                    _videoPlayer.Volume -= 5;
                    break;
                case Keys.VolumeUp:
                    _videoPlayer.Volume += 5;
                    _videoPlayer.Mute = false;
                    break;
                case Keys.VolumeMute:
                    _videoPlayer.ToggleMute();
                    break;
                case Keys.MediaNextTrack:
                    break;
                case Keys.MediaPlayPause:
                    _videoPlayer.Pause();
                    break;
                case Keys.MediaPreviousTrack:
                    break;
                case Keys.MediaStop:
                    _videoPlayer.Stop();
                    break;
                default:
                    return;
            }
        }

        private AudioOutputDeviceType GetAudioDeviceType()
        {
            switch (_config.Configuration.VlcConfiguration.AudioLayout)
            {
                case AudioLayout.Five1:
                    return AudioOutputDeviceType.AudioOutputDevice_5_1;
                case AudioLayout.Six1:
                    return AudioOutputDeviceType.AudioOutputDevice_6_1;
                case AudioLayout.Seven1:
                    return AudioOutputDeviceType.AudioOutputDevice_7_1;
                case AudioLayout.Mono:
                    return AudioOutputDeviceType.AudioOutputDevice_Mono;
                case AudioLayout.Spdif:
                    return AudioOutputDeviceType.AudioOutputDevice_SPDIF;
                default:
                    return AudioOutputDeviceType.AudioOutputDevice_Stereo;
            }
        }

        /// <summary>
        /// Handles the PlayerStopped event of the Events control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        void Events_PlayerStopped(object sender, EventArgs e)
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

        /// <summary>
        /// Medias the list player events_ media list player next item set.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        void MediaListPlayerEvents_MediaListPlayerNextItemSet(object sender, MediaListPlayerNextItemSet e)
        {
            //var current = _mediaList.FirstOrDefault(i => i.Tag == e.Item.Tag);

            //var newIndex = current != null ? MediaList.IndexOf(current) : -1;

            //var currentIndex = _currentPlaylistIndex;

            //if (newIndex != currentIndex)
            //{
            //    OnMediaChanged(currentIndex, null, newIndex);
            //}

            //_currentPlaylistIndex = newIndex;
        }

        /// <summary>
        /// Gets the playable path.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <returns>System.String.</returns>
        private string GetPlayablePath(BaseItemDto item)
        {
            if (item.LocationType == LocationType.Remote)
            {
                return GetStreamingUrl(item);
            }
            if (!File.Exists(item.Path) && !Directory.Exists(item.Path))
            {
                return GetStreamingUrl(item);
            }

            if (item.VideoType.HasValue && item.VideoType.Value == VideoType.BluRay)
            {
                var file = new DirectoryInfo(item.Path)
                    .EnumerateFiles("*.m2ts", SearchOption.AllDirectories)
                    .OrderByDescending(f => f.Length)
                    .FirstOrDefault();

                if (file != null)
                {
                    return file.FullName;
                }
            }

            return item.Path;
        }

        private string GetStreamingUrl(BaseItemDto item)
        {
            // TODO: Add non-static url's for dvd + bluray

            return _apiClient.GetVideoStreamUrl(new VideoStreamOptions
            {
                Static = true,
                ItemId = item.Id
            });
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
            _videoPlayer.Stop();
        }

        /// <summary>
        /// Sets the volume.
        /// </summary>
        /// <param name="volume">The volume.</param>
        /// <returns>Task.</returns>
        public Task SetVolume(int volume)
        {
            if (volume > 0 && _videoPlayer.Mute)
            {
                _videoPlayer.Mute = false;
            }

            _videoPlayer.Volume = volume;

            OnVolumeChanged();

            return _taskResult;
        }

        /// <summary>
        /// Mutes this instance.
        /// </summary>
        /// <returns>Task.</returns>
        public Task Mute()
        {
            _videoPlayer.Mute = true;

            OnVolumeChanged();

            return _taskResult;
        }

        /// <summary>
        /// Uns the mute.
        /// </summary>
        /// <returns>Task.</returns>
        public Task UnMute()
        {
            _videoPlayer.Mute = false;

            OnVolumeChanged();

            return _taskResult;
        }

        /// <summary>
        /// Pauses this instance.
        /// </summary>
        /// <returns>Task.</returns>
        public void Pause()
        {
            _videoPlayer.Pause();
        }

        /// <summary>
        /// Uns the pause.
        /// </summary>
        /// <returns>Task.</returns>
        public void UnPause()
        {
            _videoPlayer.Pause();
        }

        /// <summary>
        /// Seeks the specified position ticks.
        /// </summary>
        /// <param name="positionTicks">The position ticks.</param>
        /// <returns>Task.</returns>
        public void Seek(long positionTicks)
        {
            _videoPlayer.Time = Convert.ToInt64(TimeSpan.FromTicks(positionTicks).TotalMilliseconds);
        }

        /// <summary>
        /// Gets the current position ticks.
        /// </summary>
        /// <value>The current position ticks.</value>
        public long? CurrentPositionTicks
        {
            get
            {
                if (_videoPlayer != null)
                {
                    return TimeSpan.FromMilliseconds(_videoPlayer.Time).Ticks;
                }

                return null;
            }
        }

        /// <summary>
        /// Gets a value indicating whether this instance is muted.
        /// </summary>
        /// <value><c>true</c> if this instance is muted; otherwise, <c>false</c>.</value>
        public bool IsMuted
        {
            get { return _videoPlayer.Mute; }
        }

        /// <summary>
        /// Gets the volume.
        /// </summary>
        /// <value>The volume.</value>
        public int Volume
        {
            get { return _videoPlayer.Volume; }
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

                if (_mediaPlayerFactory != null)
                {
                    _mediaPlayerFactory.Dispose();
                }
            }
        }

        /// <summary>
        /// Disposes the player.
        /// </summary>
        private void DisposePlayer()
        {
            _userInput.GlobalKeyDown -= _userInput_KeyDown;

            if (_videoPlayer != null)
            {
                //_mediaListPlayer.MediaListPlayerEvents.MediaListPlayerNextItemSet -= MediaListPlayerEvents_MediaListPlayerNextItemSet;
            }

            if (_videoPlayer != null)
            {
                _videoPlayer.Events.PlayerPaused -= Events_PlayerPaused;
                _videoPlayer.Events.PlayerPlaying -= Events_PlayerPaused;
                _videoPlayer.Events.PlayerStopped -= Events_PlayerStopped;
                _videoPlayer.Events.MediaEnded -= Events_MediaEnded;
                _videoPlayer.Dispose();
            }

            if (_media != null)
            {
                _media.Dispose();
                _media = null;
            }

            _videoPlayer = null;

            CurrentPlayOptions = null;
            CurrentPlaylistIndex = 0;

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

        public long? CurrentDurationTicks
        {
            get
            {

                if (_videoPlayer != null)
                {
                    return TimeSpan.FromMilliseconds(_videoPlayer.Length).Ticks;
                }
                return null;
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
