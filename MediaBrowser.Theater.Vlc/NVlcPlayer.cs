using Declarations;
using Declarations.Events;
using Declarations.Media;
using Declarations.Players;
using Implementation;
using MediaBrowser.Common.Events;
using MediaBrowser.Model.Dto;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.Logging;
using MediaBrowser.Theater.Interfaces.Playback;
using MediaBrowser.Theater.Interfaces.Presentation;
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
    public class NVlcPlayer : IInternalMediaPlayer, IDisposable
    {
        /// <summary>
        /// The _windows forms panel
        /// </summary>
        private Panel _windowsFormsPanel;
        /// <summary>
        /// The _media player factory
        /// </summary>
        private MediaPlayerFactory _mediaPlayerFactory;
        /// <summary>
        /// The _media list player
        /// </summary>
        private IMediaListPlayer _mediaListPlayer;
        /// <summary>
        /// The _media list
        /// </summary>
        private IMediaList _mediaList;

        /// <summary>
        /// The _hidden window
        /// </summary>
        private readonly IHiddenWindow _hiddenWindow;
        private readonly ILogger _logger;

        /// <summary>
        /// The _task result
        /// </summary>
        private readonly Task _taskResult = Task.FromResult(true);

        /// <summary>
        /// Initializes a new instance of the <see cref="NVlcPlayer" /> class.
        /// </summary>
        /// <param name="hiddenWindow">The hidden window.</param>
        /// <param name="logManager">The log manager.</param>
        public NVlcPlayer(IHiddenWindow hiddenWindow, ILogManager logManager)
        {
            _hiddenWindow = hiddenWindow;

            _logger = logManager.GetLogger(Name);
        }

        private readonly List<BaseItemDto> _playlist = new List<BaseItemDto>();
        public IReadOnlyList<BaseItemDto> Playlist
        {
            get { return _playlist; }
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
            get { return "Vlc"; }
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
                if (_mediaListPlayer == null)
                {
                    return PlayState.Idle;
                }

                if (_mediaListPlayer.PlayerState == MediaState.Paused)
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

            _windowsFormsPanel = new Panel
            {
                BackColor = Color.Black
            };

            _hiddenWindow.WindowsFormsHost.Child = _windowsFormsPanel;

            _mediaPlayerFactory = new MediaPlayerFactory(new[] 
             {
                "-I", 
                "dummy",  
		        "--ignore-config", 
                "--no-osd",
                "--disable-screensaver",
                //"--ffmpeg-hw",
		        "--plugin-path=./plugins"
             });
        }

        /// <summary>
        /// Plays the specified options.
        /// </summary>
        /// <param name="options">The options.</param>
        /// <returns>Task.</returns>
        public Task Play(PlayOptions options)
        {
            EnsureMediaPlayerCreated();

            CurrentPlaylistIndex = 0;
            CurrentPlayOptions = options;

            lock (_playlist)
            {
                _playlist.Clear();
                _playlist.AddRange(options.Items);
            }

            try
            {
                _mediaList = _mediaPlayerFactory.CreateMediaList<IMediaList>(options.Items.Select(GetPlayablePath));
                _mediaListPlayer = _mediaPlayerFactory.CreateMediaListPlayer<IMediaListPlayer>(_mediaList);

                _mediaListPlayer.InnerPlayer.WindowHandle = _windowsFormsPanel.Handle;

                _mediaListPlayer.InnerPlayer.Events.PlayerStopped += Events_PlayerStopped;
                _mediaListPlayer.Play();

                var position = options.StartPositionTicks;

                if (position > 0)
                {
                    _mediaListPlayer.Time = Convert.ToInt64(TimeSpan.FromTicks(position).TotalMilliseconds);
                }

                _mediaListPlayer.MediaListPlayerEvents.MediaListPlayerNextItemSet += MediaListPlayerEvents_MediaListPlayerNextItemSet;
            }
            catch (Exception ex)
            {
                _logger.ErrorException("Error beginning playback", ex);

                DisposePlayer();
            }

            return _taskResult;
        }

        /// <summary>
        /// Handles the PlayerStopped event of the Events control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        void Events_PlayerStopped(object sender, EventArgs e)
        {
        }

        /// <summary>
        /// Medias the list player events_ media list player next item set.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        void MediaListPlayerEvents_MediaListPlayerNextItemSet(object sender, MediaListPlayerNextItemSet e)
        {
            //var current = MediaList.FirstOrDefault(i => i.Tag == e.NewMedia.Tag);

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

        /// <summary>
        /// Determines whether this instance can play the specified item.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <returns><c>true</c> if this instance can play the specified item; otherwise, <c>false</c>.</returns>
        public bool CanPlay(BaseItemDto item)
        {
            return item.IsVideo || item.IsAudio;
        }

        /// <summary>
        /// Stops this instance.
        /// </summary>
        /// <returns>Task.</returns>
        public Task Stop()
        {
            _mediaListPlayer.Stop();

            return _taskResult;
        }

        /// <summary>
        /// Sets the volume.
        /// </summary>
        /// <param name="volume">The volume.</param>
        /// <returns>Task.</returns>
        public Task SetVolume(int volume)
        {
            if (volume > 0 && _mediaListPlayer.InnerPlayer.Mute)
            {
                _mediaListPlayer.InnerPlayer.Mute = false;
            }

            _mediaListPlayer.InnerPlayer.Volume = volume;

            OnVolumeChanged();
            
            return _taskResult;
        }

        /// <summary>
        /// Mutes this instance.
        /// </summary>
        /// <returns>Task.</returns>
        public Task Mute()
        {
            _mediaListPlayer.InnerPlayer.Mute = true;

            OnVolumeChanged();
            
            return _taskResult;
        }

        /// <summary>
        /// Uns the mute.
        /// </summary>
        /// <returns>Task.</returns>
        public Task UnMute()
        {
            _mediaListPlayer.InnerPlayer.Mute = false;

            OnVolumeChanged();

            return _taskResult;
        }

        /// <summary>
        /// Pauses this instance.
        /// </summary>
        /// <returns>Task.</returns>
        public Task Pause()
        {
            _mediaListPlayer.Pause();

            return _taskResult;
        }

        /// <summary>
        /// Uns the pause.
        /// </summary>
        /// <returns>Task.</returns>
        public Task UnPause()
        {
            _mediaListPlayer.Pause();

            return _taskResult;
        }

        /// <summary>
        /// Seeks the specified position ticks.
        /// </summary>
        /// <param name="positionTicks">The position ticks.</param>
        /// <returns>Task.</returns>
        public Task Seek(long positionTicks)
        {
            _mediaListPlayer.Time = Convert.ToInt64(TimeSpan.FromTicks(positionTicks).TotalMilliseconds);

            return _taskResult;
        }

        /// <summary>
        /// Gets the current position ticks.
        /// </summary>
        /// <value>The current position ticks.</value>
        public long? CurrentPositionTicks
        {
            get
            {
                if (_mediaListPlayer != null)
                {
                    return TimeSpan.FromMilliseconds(_mediaListPlayer.Time).Ticks;
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
            get { return _mediaListPlayer.InnerPlayer.Mute; }
        }

        /// <summary>
        /// Gets the volume.
        /// </summary>
        /// <value>The volume.</value>
        public int Volume
        {
            get { return _mediaListPlayer.InnerPlayer.Volume; }
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
            if (_mediaListPlayer != null)
            {
                _mediaListPlayer.MediaListPlayerEvents.MediaListPlayerNextItemSet -= MediaListPlayerEvents_MediaListPlayerNextItemSet;
            }

            if (_mediaList != null)
            {
                _mediaList.Dispose();
            }

            if (_mediaListPlayer != null && _mediaListPlayer.InnerPlayer != null)
            {
                _mediaListPlayer.InnerPlayer.Events.PlayerStopped -= Events_PlayerStopped;
                _mediaListPlayer.InnerPlayer.Dispose();
            }

            _mediaListPlayer = null;

            CurrentPlayOptions = null;
            CurrentPlaylistIndex = 0;

            lock (_playlist)
            {
                _playlist.Clear();
            }
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
    }
}
