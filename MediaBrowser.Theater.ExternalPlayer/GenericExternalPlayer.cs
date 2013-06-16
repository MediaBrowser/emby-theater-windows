using MediaBrowser.Common.Events;
using MediaBrowser.Model.Dto;
using MediaBrowser.Model.Logging;
using MediaBrowser.Theater.Interfaces.Configuration;
using MediaBrowser.Theater.Interfaces.Playback;
using MediaBrowser.Theater.Interfaces.UserInput;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MediaBrowser.Theater.ExternalPlayer
{
    /// <summary>
    /// Class GenericExternalPlayer
    /// </summary>
    public class GenericExternalPlayer : IExternalMediaPlayer
    {
        private readonly IUserInputManager _userInput;
        
        /// <summary>
        /// The _logger
        /// </summary>
        private readonly ILogger _logger;
        /// <summary>
        /// The _playback manager
        /// </summary>
        private readonly IPlaybackManager _playbackManager;

        /// <summary>
        /// The _current process
        /// </summary>
        private Process _currentProcess;

        /// <summary>
        /// Occurs when [volume changed].
        /// </summary>
        public event EventHandler VolumeChanged;

        /// <summary>
        /// Occurs when [play state changed].
        /// </summary>
        public event EventHandler PlayStateChanged;

        /// <summary>
        /// Occurs when [media changed].
        /// </summary>
        public event EventHandler<MediaChangeEventArgs> MediaChanged;

        /// <summary>
        /// Occurs when [playback completed].
        /// </summary>
        public event EventHandler<PlaybackStopEventArgs> PlaybackCompleted;

        /// <summary>
        /// The _playlist
        /// </summary>
        private List<BaseItemDto> _playlist = new List<BaseItemDto>();
        /// <summary>
        /// Gets the playlist.
        /// </summary>
        /// <value>The playlist.</value>
        public IReadOnlyList<BaseItemDto> Playlist
        {
            get
            {
                return _playlist;
            }
        }

        /// <summary>
        /// Gets the index of the current playlist.
        /// </summary>
        /// <value>The index of the current playlist.</value>
        public int CurrentPlaylistIndex { get; private set; }

        /// <summary>
        /// Gets the current play options.
        /// </summary>
        /// <value>The current play options.</value>
        public PlayOptions CurrentPlayOptions { get; private set; }

        /// <summary>
        /// Gets the current media.
        /// </summary>
        /// <value>The current media.</value>
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
            get { return false; }
        }

        /// <summary>
        /// Gets a value indicating whether this instance can pause.
        /// </summary>
        /// <value><c>true</c> if this instance can pause; otherwise, <c>false</c>.</value>
        public bool CanPause
        {
            get { return false; }
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
            get { return false; }
        }

        /// <summary>
        /// Gets a value indicating whether this instance can track progress.
        /// </summary>
        /// <value><c>true</c> if this instance can track progress; otherwise, <c>false</c>.</value>
        public bool CanTrackProgress
        {
            get { return false; }
        }

        /// <summary>
        /// Gets the name.
        /// </summary>
        /// <value>The name.</value>
        public string Name
        {
            get { return "Generic Player"; }
        }

        /// <summary>
        /// Gets a value indicating whether [supports multi file playback].
        /// </summary>
        /// <value><c>true</c> if [supports multi file playback]; otherwise, <c>false</c>.</value>
        public bool SupportsMultiFilePlayback
        {
            get { return false; }
        }

        /// <summary>
        /// Gets the state of the play.
        /// </summary>
        /// <value>The state of the play.</value>
        public PlayState PlayState
        {
            get
            {
                return _currentProcess != null ? PlayState.Playing : PlayState.Idle;
            }
        }

        /// <summary>
        /// Gets the current position ticks.
        /// </summary>
        /// <value>The current position ticks.</value>
        public long? CurrentPositionTicks
        {
            get { return null; }
        }

        /// <summary>
        /// Determines whether this instance can play the specified item.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <returns><c>true</c> if this instance can play the specified item; otherwise, <c>false</c>.</returns>
        public bool CanPlay(BaseItemDto item)
        {
            // Can only play if configured to do so
            return false;
        }

        /// <summary>
        /// The _null task result
        /// </summary>
        private readonly Task _nullTaskResult = Task.FromResult(true);

        /// <summary>
        /// Initializes a new instance of the <see cref="GenericExternalPlayer" /> class.
        /// </summary>
        /// <param name="playbackManager">The playback manager.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="userInput">The user input.</param>
        public GenericExternalPlayer(IPlaybackManager playbackManager, ILogger logger, IUserInputManager userInput)
        {
            _playbackManager = playbackManager;
            _logger = logger;
            _userInput = userInput;
        }

        /// <summary>
        /// Stops this instance.
        /// </summary>
        /// <returns>Task.</returns>
        public Task Stop()
        {
            if (_currentProcess == null)
            {
                return _nullTaskResult;
            }

            return Task.Run(() =>
            {
                if (!_currentProcess.CloseMainWindow())
                {
                    _currentProcess.Kill();
                }
            });
        }

        /// <summary>
        /// Plays the specified options.
        /// </summary>
        /// <param name="options">The options.</param>
        /// <returns>Task.</returns>
        public Task Play(PlayOptions options)
        {
            return Task.Run(() =>
            {
                CurrentPlaylistIndex = 0;
                CurrentPlayOptions = options;

                _playlist = options.Items.ToList();
                
                var process = new Process
                {
                    EnableRaisingEvents = true,
                    StartInfo = GetProcessStartInfo(options.Items, options.Configuration)
                };

                _logger.Info("{0} {1}", _currentProcess.StartInfo.FileName, _currentProcess.StartInfo.Arguments);

                try
                {
                    process.Start();
                }
                catch (Exception ex)
                {
                    _logger.ErrorException("Error staring player", ex);

                    _playlist.Clear();

                    throw;
                }

                if (options.Configuration.CloseOnStopButton)
                {
                    _userInput.KeyDown += KeyboardListener_KeyDown;
                }

                process.Exited += CurrentProcess_Exited;

                _currentProcess = process;
            });
        }

        /// <summary>
        /// Handles the KeyDown event of the KeyboardListener control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="KeyEventArgs" /> instance containing the event data.</param>
        async void KeyboardListener_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.MediaStop)
            {
                var playstate = PlayState;

                if (playstate == PlayState.Paused || playstate == PlayState.Playing)
                {
                    await Stop();
                }
            }
        }

        /// <summary>
        /// Handles the Exited event of the CurrentProcess control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        void CurrentProcess_Exited(object sender, EventArgs e)
        {
            _userInput.KeyDown -= KeyboardListener_KeyDown;
            
            var process = (Process)sender;

            var playlist = _playlist.ToList();
            var index = CurrentPlaylistIndex;
            var ticks = CurrentPositionTicks;

            process.Dispose();

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
        /// Gets a value indicating whether this instance is muted.
        /// </summary>
        /// <value><c>true</c> if this instance is muted; otherwise, <c>false</c>.</value>
        public bool IsMuted
        {
            get { return false; }
        }

        /// <summary>
        /// Gets the volume.
        /// </summary>
        /// <value>The volume.</value>
        public int Volume
        {
            get { return 100; }
        }

        /// <summary>
        /// Sets the volume.
        /// </summary>
        /// <param name="volume">The volume.</param>
        /// <returns>Task.</returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public Task SetVolume(int volume)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Mutes this instance.
        /// </summary>
        /// <returns>Task.</returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public Task Mute()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Uns the mute.
        /// </summary>
        /// <returns>Task.</returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public Task UnMute()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Pauses this instance.
        /// </summary>
        /// <returns>Task.</returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public Task Pause()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Uns the pause.
        /// </summary>
        /// <returns>Task.</returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public Task UnPause()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Seeks the specified position ticks.
        /// </summary>
        /// <param name="positionTicks">The position ticks.</param>
        /// <returns>Task.</returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public Task Seek(long positionTicks)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Gets the process start info.
        /// </summary>
        /// <param name="items">The items.</param>
        /// <param name="playerConfiguration">The player configuration.</param>
        /// <returns>ProcessStartInfo.</returns>
        private ProcessStartInfo GetProcessStartInfo(IEnumerable<BaseItemDto> items, PlayerConfiguration playerConfiguration)
        {
            return new ProcessStartInfo
            {
                FileName = playerConfiguration.Command,
                Arguments = GetCommandArguments(items, playerConfiguration)
            };
        }

        /// <summary>
        /// Gets the command arguments.
        /// </summary>
        /// <param name="items">The items.</param>
        /// <param name="playerConfiguration">The player configuration.</param>
        /// <returns>System.String.</returns>
        private string GetCommandArguments(IEnumerable<BaseItemDto> items, PlayerConfiguration playerConfiguration)
        {
            var args = playerConfiguration.Args;

            if (string.IsNullOrEmpty(args))
            {
                return string.Empty;
            }

            var paths = items.Take(1).Select(i => "\"" + GetPathForCommandLine(i) + "\"");

            return string.Format(args, string.Join(" ", paths.ToArray()));
        }

        /// <summary>
        /// Gets the path for command line.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <returns>System.String.</returns>
        private string GetPathForCommandLine(BaseItemDto item)
        {
            return item.Path;
        }
    }
}
