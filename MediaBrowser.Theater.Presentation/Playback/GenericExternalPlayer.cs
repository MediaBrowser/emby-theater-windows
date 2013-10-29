using MediaBrowser.Common.Events;
using MediaBrowser.Model.Dto;
using MediaBrowser.Model.Logging;
using MediaBrowser.Theater.Interfaces.Playback;
using MediaBrowser.Theater.Interfaces.UserInput;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MediaBrowser.Theater.Presentation.Playback
{
    /// <summary>
    /// Class GenericExternalPlayer
    /// </summary>
    public class GenericExternalPlayer : IExternalMediaPlayer, IConfigurableMediaPlayer
    {
        private readonly IUserInputManager _userInput;

        /// <summary>
        /// The _logger
        /// </summary>
        protected readonly ILogger Logger;

        /// <summary>
        /// The _playback manager
        /// </summary>
        private readonly IPlaybackManager _playbackManager;

        /// <summary>
        /// The _current process
        /// </summary>
        private Process _currentProcess;

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
        public int CurrentPlaylistIndex { get; protected set; }

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
                var index = CurrentPlaylistIndex;

                return _playlist.Count > 0 && index >= 0 ? _playlist[index] : null;
            }
        }

        /// <summary>
        /// Gets a value indicating whether this instance can close automatically on stop button.
        /// </summary>
        /// <value><c>true</c> if this instance can close automatically on stop button; otherwise, <c>false</c>.</value>
        public virtual bool CanCloseAutomaticallyOnStopButton
        {
            get { return false; }
        }

        /// <summary>
        /// Gets a value indicating whether this instance can seek.
        /// </summary>
        /// <value><c>true</c> if this instance can seek; otherwise, <c>false</c>.</value>
        public virtual bool CanSeek
        {
            get { return false; }
        }

        /// <summary>
        /// Gets a value indicating whether this instance can pause.
        /// </summary>
        /// <value><c>true</c> if this instance can pause; otherwise, <c>false</c>.</value>
        public virtual bool CanPause
        {
            get { return false; }
        }

        /// <summary>
        /// Gets a value indicating whether this instance can queue.
        /// </summary>
        /// <value><c>true</c> if this instance can queue; otherwise, <c>false</c>.</value>
        public virtual bool CanQueue
        {
            get { return false; }
        }

        /// <summary>
        /// Gets a value indicating whether this instance can track progress.
        /// </summary>
        /// <value><c>true</c> if this instance can track progress; otherwise, <c>false</c>.</value>
        public virtual bool CanTrackProgress
        {
            get { return false; }
        }

        /// <summary>
        /// Gets the name.
        /// </summary>
        /// <value>The name.</value>
        public virtual string Name
        {
            get { return "External Player"; }
        }

        /// <summary>
        /// Gets a value indicating whether [supports multi file playback].
        /// </summary>
        /// <value><c>true</c> if [supports multi file playback]; otherwise, <c>false</c>.</value>
        public virtual bool SupportsMultiFilePlayback
        {
            get { return false; }
        }

        /// <summary>
        /// Gets the state of the play.
        /// </summary>
        /// <value>The state of the play.</value>
        public virtual PlayState PlayState
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
        public long? CurrentPositionTicks { get; protected set; }

        public long? CurrentDurationTicks { get; protected set; }
        
        /// <summary>
        /// Determines whether this instance can play the specified item.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <returns><c>true</c> if this instance can play the specified item; otherwise, <c>false</c>.</returns>
        public virtual bool CanPlayByDefault(BaseItemDto item)
        {
            // Can only play if configured to do so
            return false;
        }

        public virtual bool CanPlayMediaType(string mediaType)
        {
            // We don't know
            return true;
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
            Logger = logger;
            _userInput = userInput;
        }

        /// <summary>
        /// Stops this instance.
        /// </summary>
        /// <returns>Task.</returns>
        public virtual void Stop()
        {
            if (_currentProcess != null)
            {
                var closed = false;

                try
                {
                    closed = _currentProcess.CloseMainWindow();
                }
                catch (InvalidOperationException)
                {
                    // Will throw this if there's no main window
                }

                if (!closed)
                {
                    _currentProcess.Kill();
                }
            }
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
                    StartInfo = GetProcessStartInfo(options.Items, options)
                };

                Logger.Info("{0} {1}", process.StartInfo.FileName, process.StartInfo.Arguments);

                try
                {
                    process.Start();
                }
                catch (Exception ex)
                {
                    Logger.ErrorException("Error starting player", ex);

                    _playlist.Clear();

                    throw;
                }

                if (options.Configuration.CloseOnStopButton && !CanCloseAutomaticallyOnStopButton)
                {
                    _userInput.KeyDown += KeyboardListener_KeyDown;
                }

                process.Exited += CurrentProcess_Exited;

                _currentProcess = process;

                OnPlayerLaunched();
            });
        }

        protected virtual void OnPlayerLaunched()
        {

        }

        /// <summary>
        /// Handles the KeyDown event of the KeyboardListener control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="KeyEventArgs" /> instance containing the event data.</param>
        void KeyboardListener_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.MediaStop)
            {
                var playstate = PlayState;

                if (playstate == PlayState.Paused || playstate == PlayState.Playing)
                {
                    Stop();
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

            EventHelper.QueueEventIfNotNull(PlaybackCompleted, this, args, Logger);

            _playbackManager.ReportPlaybackCompleted(args);

            OnPlayerExited();
        }

        protected virtual void OnPlayerExited()
        {

        }

        /// <summary>
        /// Pauses this instance.
        /// </summary>
        /// <returns>Task.</returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public virtual void Pause()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Uns the pause.
        /// </summary>
        /// <returns>Task.</returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public virtual void UnPause()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Seeks the specified position ticks.
        /// </summary>
        /// <param name="positionTicks">The position ticks.</param>
        /// <returns>Task.</returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public virtual void Seek(long positionTicks)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Changes the track.
        /// </summary>
        /// <param name="newIndex">The new index.</param>
        /// <exception cref="System.NotImplementedException"></exception>
        public virtual void ChangeTrack(int newIndex)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Gets the process start info.
        /// </summary>
        /// <param name="items">The items.</param>
        /// <param name="options">The options.</param>
        /// <returns>ProcessStartInfo.</returns>
        protected virtual ProcessStartInfo GetProcessStartInfo(IEnumerable<BaseItemDto> items, PlayOptions options)
        {
            return new ProcessStartInfo
            {
                FileName = options.Configuration.Command,
                Arguments = GetCommandArguments(items, options)
            };
        }

        /// <summary>
        /// Gets the command arguments.
        /// </summary>
        /// <param name="items">The items.</param>
        /// <param name="options">The options.</param>
        /// <returns>System.String.</returns>
        protected virtual string GetCommandArguments(IEnumerable<BaseItemDto> items, PlayOptions options)
        {
            return GetCommandArguments(items, options.Configuration.Args);
        }

        /// <summary>
        /// Gets the command arguments.
        /// </summary>
        /// <param name="items">The items.</param>
        /// <param name="formatString">The format string.</param>
        /// <returns>System.String.</returns>
        protected string GetCommandArguments(IEnumerable<BaseItemDto> items, string formatString)
        {
            var list = items.ToList();

            var paths = list.Select(i => "\"" + GetPathForCommandLine(i) + "\"");

            if (!SupportsMultiFilePlayback)
            {
                paths = paths.Take(1);
            }

            return formatString.Replace("{PATH}", string.Join(" ", paths.ToArray()));
        }

        /// <summary>
        /// Gets the path for command line.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <returns>System.String.</returns>
        protected virtual string GetPathForCommandLine(BaseItemDto item)
        {
            return item.Path;
        }

        /// <summary>
        /// Gets a value indicating whether [requires configured path].
        /// </summary>
        /// <value><c>true</c> if [requires configured path]; otherwise, <c>false</c>.</value>
        public virtual bool RequiresConfiguredPath
        {
            get { return true; }
        }

        /// <summary>
        /// Gets a value indicating whether [requires configured arguments].
        /// </summary>
        /// <value><c>true</c> if [requires configured arguments]; otherwise, <c>false</c>.</value>
        public virtual bool RequiresConfiguredArguments
        {
            get { return true; }
        }
    }
}
