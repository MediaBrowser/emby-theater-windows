using MediaBrowser.Common.Events;
using MediaBrowser.Model.ApiClient;
using MediaBrowser.Model.Dto;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.Logging;
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

namespace MediaBrowser.Theater.DirectShow
{
    public class InternalDirectShowPlayer : IInternalMediaPlayer
    {
        private DirectShowPlayer _mediaPlayer;
        private readonly ILogger _logger;
        private readonly IHiddenWindow _hiddenWindow;
        private readonly IPresentationManager _presentation;
        private readonly IUserInputManager _userInput;
        private readonly IApiClient _apiClient;
        private readonly IPlaybackManager _playbackManager;

        private readonly Task _taskResult = Task.FromResult(true);

        public event EventHandler<MediaChangeEventArgs> MediaChanged;

        public event EventHandler<PlaybackStopEventArgs> PlaybackCompleted;

        private List<BaseItemDto> _playlist = new List<BaseItemDto>();

        public InternalDirectShowPlayer(ILogger logger, IHiddenWindow hiddenWindow, IPresentationManager presentation, IUserInputManager userInput, IApiClient apiClient, IPlaybackManager playbackManager)
        {
            _logger = logger;
            _hiddenWindow = hiddenWindow;
            _presentation = presentation;
            _userInput = userInput;
            _apiClient = apiClient;
            _playbackManager = playbackManager;
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
            await _presentation.Window.Dispatcher.InvokeAsync(async () => await PlayInternal(options));
        }

        private Task PlayInternal(PlayOptions options)
        {
            CurrentPlaylistIndex = 0;
            CurrentPlayOptions = options;

            _playlist = options.Items.ToList();

            try
            {
                _mediaPlayer = new DirectShowPlayer(_logger, _hiddenWindow, this);
                _mediaPlayer.FormBorderStyle = FormBorderStyle.None;
                _mediaPlayer.TopLevel = false;
                _hiddenWindow.WindowsFormsHost.Child = _mediaPlayer;

                _mediaPlayer.Play(options.Items.First());

                var position = options.StartPositionTicks;

                if (position > 0)
                {
                    _mediaPlayer.Seek(position);
                }

                _userInput.KeyDown += _userInput_KeyDown;
            }
            catch (Exception ex)
            {
                _logger.ErrorException("Error beginning playback", ex);

                DisposePlayer();

                throw;
            }

            return _taskResult;
        }

        void _userInput_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Pause:
                    _mediaPlayer.Pause();
                    break;
                //case Keys.VolumeDown:
                //    _mediaListPlayer.InnerPlayer.Volume -= 5;
                //    break;
                //case Keys.VolumeUp:
                //    _mediaListPlayer.InnerPlayer.Volume += 5;
                //    _mediaListPlayer.InnerPlayer.Mute = false;
                //    break;
                //case Keys.VolumeMute:
                //    _mediaListPlayer.InnerPlayer.ToggleMute();
                //    break;
                case Keys.MediaNextTrack:
                    break;
                case Keys.MediaPlayPause:
                    _mediaPlayer.Pause();
                    break;
                case Keys.MediaPreviousTrack:
                    break;
                case Keys.MediaStop:
                    _mediaPlayer.Stop();
                    break;
                default:
                    return;
            }
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

        private void DisposePlayer()
        {
            if (_mediaPlayer != null)
            {
                _presentation.Window.Dispatcher.InvokeAsync(() => _mediaPlayer.Dispose());
            }
        }

        public Task Pause()
        {
            if (_mediaPlayer != null)
            {
                _mediaPlayer.Pause();
            }
            return Task.FromResult(true);
        }

        public Task UnPause()
        {
            if (_mediaPlayer != null)
            {
                _mediaPlayer.Unpause();
            }
            return Task.FromResult(true);
        }

        public Task Stop()
        {
            if (_mediaPlayer != null)
            {
                _mediaPlayer.Stop();
            }
            return Task.FromResult(true);
        }

        public Task Seek(long positionTicks)
        {
            if (_mediaPlayer != null)
            {
                _mediaPlayer.Seek(positionTicks);
            }
            return Task.FromResult(true);
        }

        /// <summary>
        /// Occurs when [play state changed].
        /// </summary>
        public event EventHandler PlayStateChanged;
        internal void OnPlayStateChanged()
        {
            EventHelper.FireEventIfNotNull(PlayStateChanged, this, EventArgs.Empty, _logger);
        }

        internal void OnPlaybackStopped(BaseItemDto media, long? positionTicks)
        {
            DisposePlayer();

            var args = new PlaybackStopEventArgs
            {
                Player = this,
                Playlist = _playlist,
                EndingMedia = media,
                EndingPositionTicks = positionTicks

            };
            
            EventHelper.FireEventIfNotNull(PlaybackCompleted, this, args, _logger);

            _playbackManager.ReportPlaybackCompleted(args);
        }
    }
}
