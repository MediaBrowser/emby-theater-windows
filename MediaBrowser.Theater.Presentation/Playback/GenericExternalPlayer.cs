using MediaBrowser.Common.Events;
using MediaBrowser.Model.Dto;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.IO;
using MediaBrowser.Model.Logging;
using MediaBrowser.Theater.Interfaces.Configuration;
using MediaBrowser.Theater.Interfaces.Playback;
using MediaBrowser.Theater.Interfaces.UserInput;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
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

        private readonly IIsoManager _isoManager;

        private IIsoMount _currentIsoMount;

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
        /// Gets a value indicating whether this instance can set which audio stream to play
        /// </summary>
        /// <value><c>true</c> if this instance can set the audio stream index; otherwise, <c>false</c>.</value>
        public virtual bool CanSetAudioStreamIndex
        {
            get { return false; }
        }

        /// <summary>
        /// Gets a value indicating whether this instance can set which subtitle stream to play
        /// </summary>
        /// <value><c>true</c> if this instance can set the subtitle stream index; otherwise, <c>false</c>.</value>
        public virtual bool CanSetSubtitleStreamIndex
        {
            get { return false; }
        }

        /// <summary>
        /// Gets a value indicating whether this instance accepts navigation commands 
        /// Up, down, left, right, pageup, pagedown, goHome, goSettings
        /// </summary>
        /// <value><c>true</c> if this instance can set the subtitle stream index; otherwise, <c>false</c>.</value>
        public virtual bool CanAcceptNavigationCommands
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
        /// Initializes a new instance of the <see cref="GenericExternalPlayer" /> class.
        /// </summary>
        /// <param name="playbackManager">The playback manager.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="userInput">The user input.</param>
        public GenericExternalPlayer(IPlaybackManager playbackManager, ILogger logger, IUserInputManager userInput, IIsoManager isoManager)
        {
            _playbackManager = playbackManager;
            Logger = logger;
            _userInput = userInput;
            _isoManager = isoManager;
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
        public async Task Play(PlayOptions options)
        {
            _currentIsoMount = options.Items.Count == 1 && options.Configuration.IsoMethod == IsoConfiguration.Mount ?
                await GetIsoMount(options.Items[0], CancellationToken.None) :
                null;

            CurrentPlaylistIndex = 0;
            CurrentPlayOptions = options;

            _playlist = options.Items.ToList();

            var process = new Process
            {
                EnableRaisingEvents = true,
                StartInfo = GetProcessStartInfo(options.Items, options, _currentIsoMount)
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
                _userInput.GlobalKeyDown += KeyboardListener_KeyDown;
            }

            process.Exited += CurrentProcess_Exited;

            _currentProcess = process;

            OnPlayerLaunched();
        }

        private async Task<IIsoMount> GetIsoMount(BaseItemDto item, CancellationToken cancellationToken)
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
                    Logger.ErrorException("Error mounting iso {0}", ex, item.Path);
                }
            }

            return mountedIso;
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
            _currentProcess = null;

            _userInput.GlobalKeyDown -= KeyboardListener_KeyDown;

            var process = (Process)sender;

            var playlist = _playlist.ToList();
            var index = CurrentPlaylistIndex;
            var ticks = CurrentPositionTicks;

            process.Dispose();

            if (_currentIsoMount != null)
            {
                _currentIsoMount.Dispose();
                _currentIsoMount = null;
            }

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

        // <summary>
        // Changes to the next track.
        // <summary>
        public void NextTrack()
        {
            throw new NotImplementedException();
        }

        // <summary>
        // Changes to the previous track.
        // </summary>
        public void PreviousTrack()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Sets the playback speed
        /// </summary>
        /// <param name="rate">The playback speed</param>
        /// <exception cref="System.NotImplementedException"></exception>
        public virtual void SetRate(double rate)
        {
            throw new NotImplementedException();
        }

        public void SetSubtitleStreamIndex(int subtitleStreamIndex)
        {
            throw new NotImplementedException();
        }

        public void NextSubtitleStream()
        {
            throw new NotImplementedException(); ;
        }


        public void SetAudioStreamIndex(int subtitleStreamIndex)
        {
            throw new NotImplementedException();
        }
       
        public void NextAudioStream()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Gets the process start info.
        /// </summary>
        /// <param name="items">The items.</param>
        /// <param name="options">The options.</param>
        /// <param name="isoMount">The iso mount.</param>
        /// <returns>ProcessStartInfo.</returns>
        protected virtual ProcessStartInfo GetProcessStartInfo(IEnumerable<BaseItemDto> items, PlayOptions options, IIsoMount isoMount)
        {
            return new ProcessStartInfo
            {
                FileName = options.Configuration.Command,
                Arguments = GetCommandArguments(items, options, isoMount)
            };
        }

        /// <summary>
        /// Gets the command arguments.
        /// </summary>
        /// <param name="items">The items.</param>
        /// <param name="options">The options.</param>
        /// <param name="isoMount">The iso mount.</param>
        /// <returns>System.String.</returns>
        protected virtual string GetCommandArguments(IEnumerable<BaseItemDto> items, PlayOptions options, IIsoMount isoMount)
        {
            return GetCommandArguments(items, options.Configuration.Args, isoMount);
        }

        /// <summary>
        /// Gets the command arguments.
        /// </summary>
        /// <param name="items">The items.</param>
        /// <param name="formatString">The format string.</param>
        /// <param name="isoMount">The iso mount.</param>
        /// <returns>System.String.</returns>
        protected string GetCommandArguments(IEnumerable<BaseItemDto> items, string formatString, IIsoMount isoMount)
        {
            var list = items.ToList();

            string path;

            if (list.Count == 1 || !SupportsMultiFilePlayback || isoMount != null)
            {
                path = "\"" + GetPathForCommandLine(list[0], isoMount) + "\"";
            }
            else
            {
                var paths = list.Select(i => "\"" + GetPathForCommandLine(i, null) + "\"");

                path = string.Join(" ", paths.ToArray());
            }

            return formatString.Replace("{PATH}", path);
        }

        /// <summary>
        /// Gets the path for command line.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <param name="isoMount">The iso mount.</param>
        /// <returns>System.String.</returns>
        protected virtual string GetPathForCommandLine(BaseItemDto item, IIsoMount isoMount)
        {
            return isoMount == null ? item.Path : isoMount.MountedPath;
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
