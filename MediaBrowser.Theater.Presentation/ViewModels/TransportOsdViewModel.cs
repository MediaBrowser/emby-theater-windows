using MediaBrowser.Model.ApiClient;
using MediaBrowser.Model.Dto;
using MediaBrowser.Model.Logging;
using MediaBrowser.Theater.Interfaces.Playback;
using MediaBrowser.Theater.Interfaces.Presentation;
using MediaBrowser.Theater.Interfaces.Reflection;
using MediaBrowser.Theater.Interfaces.ViewModels;
using MediaBrowser.Theater.Presentation.Playback;
using System;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Windows.Input;
using System.Windows.Threading;

namespace MediaBrowser.Theater.Presentation.ViewModels
{
    [TypeDescriptionProvider(typeof(HyperTypeDescriptionProvider))]
    public class TransportOsdViewModel : BaseViewModel, IDisposable
    {
        public IApiClient ApiClient { get; private set; }
        public IImageManager ImageManager { get; private set; }
        public IPlaybackManager PlaybackManager { get; set; }
        public IPresentationManager PresentationManager { get; set; }
        public ILogger Logger { get; set; }

        public ICommand PauseCommand { get; private set; }
        public ICommand NextChapterCommand { get; private set; }
        public ICommand PreviousChapterCommand { get; private set; }
        public ICommand SkipBackwardCommand { get; private set; }
        public ICommand SkipForwardCommand { get; private set; }
        public ICommand StopCommand { get; private set; }
        public ICommand PlayCommand { get; private set; }

        private Timer _currentPositionTimer;

        private readonly Dispatcher _currentDispatcher;

        private IMediaPlayer _mediaPlayer;
        public IMediaPlayer MediaPlayer
        {
            get
            {
                return _mediaPlayer;
            }
            set
            {
                var old = _mediaPlayer;

                if (old != null)
                {
                    RemovePlayerEvents(old);
                }

                var changed = old != value;
                _mediaPlayer = value;

                if (changed)
                {
                    OnPropertyChanged("MediaPlayer");
                }
            }
        }

        private BaseItemDto _nowPlayingItem;
        public BaseItemDto NowPlayingItem
        {
            get
            {
                return _nowPlayingItem;
            }
            set
            {
                var changed = _nowPlayingItem != value;

                _nowPlayingItem = value;

                if (changed)
                {
                    NowPlayingItemViewModel = new ItemViewModel(ApiClient, ImageManager, PlaybackManager, PresentationManager, Logger)
                    {
                        Item = value
                    };

                    OnPropertyChanged("NowPlayingItem");
                }
            }
        }

        private ItemViewModel _nowPlayingItemViewModel;
        public ItemViewModel NowPlayingItemViewModel
        {
            get
            {
                return _nowPlayingItemViewModel;
            }
            set
            {
                var changed = _nowPlayingItemViewModel != value;
                _nowPlayingItemViewModel = value;

                if (changed)
                {
                    OnPropertyChanged("NowPlayingItemViewModel");
                }
            }
        }

        public string ClockShortTime
        {
            get { return DateTime.Now.ToShortTimeString(); }
        }

        private string _displayDuration;
        public string DisplayDuration
        {
            get
            {
                return _displayDuration;
            }
            set
            {
                var changed = !string.Equals(_displayDuration, value);
                _displayDuration = value;

                if (changed)
                {
                    OnPropertyChanged("DisplayDuration");
                }
            }
        }

        private string _displayPosition;
        public string DisplayPosition
        {
            get
            {
                return _displayPosition;
            }
            set
            {
                var changed = !string.Equals(_displayPosition, value);
                _displayPosition = value;

                if (changed)
                {
                    OnPropertyChanged("DisplayPosition");
                }
            }
        }

        private bool _showInfoPanel;
        public bool ShowInfoPanel
        {
            get
            {
                return _showInfoPanel;
            }
            set
            {
                var changed = !bool.Equals(_showInfoPanel, value);
                _showInfoPanel = value;

                if (changed)
                {
                    OnPropertyChanged("ShowInfoPanel");
                }
            }
        }

        private bool _canSeek;
        public bool CanSeek
        {
            get
            {
                return _canSeek;
            }
            set
            {
                var changed = !bool.Equals(_canSeek, value);
                _canSeek = value;

                if (changed)
                {
                    OnPropertyChanged("CanSeek");
                }
            }
        }

        private bool _supportsChapters;
        public bool SupportsChapters
        {
            get
            {
                return _supportsChapters;
            }
            set
            {
                var changed = !bool.Equals(_supportsChapters, value);
                _supportsChapters = value;

                if (changed)
                {
                    OnPropertyChanged("SupportsChapters");
                }
            }
        }

        private bool _canSelectAudioTrack;
        public bool CanSelectAudioTrack
        {
            get
            {
                return _canSelectAudioTrack;
            }
            set
            {
                var changed = !bool.Equals(_canSelectAudioTrack, value);
                _canSelectAudioTrack = value;

                if (changed)
                {
                    OnPropertyChanged("CanSelectAudioTrack");
                }
            }
        }

        private bool _canSelectSubtitleTrack;
        public bool CanSelectSubtitleTrack
        {
            get
            {
                return _canSelectSubtitleTrack;
            }
            set
            {
                var changed = !bool.Equals(_canSelectSubtitleTrack, value);
                _canSelectSubtitleTrack = value;

                if (changed)
                {
                    OnPropertyChanged("CanSelectSubtitleTrack");
                }
            }
        }

        private bool _isPaused;
        public bool IsPaused
        {
            get
            {
                return _isPaused;
            }
            set
            {
                var changed = !bool.Equals(_isPaused, value);
                _isPaused = value;

                if (changed)
                {
                    OnPropertyChanged("IsPaused");
                }
            }
        }

        private bool _canPause;
        public bool CanPause
        {
            get
            {
                return _canPause;
            }
            set
            {
                var changed = !bool.Equals(_canPause, value);
                _canPause = value;

                if (changed)
                {
                    OnPropertyChanged("CanPause");
                }
            }
        }

        private bool _canPlay;
        public bool CanPlay
        {
            get
            {
                return _canPlay;
            }
            set
            {
                var changed = !bool.Equals(_canPlay, value);
                _canPlay = value;

                if (changed)
                {
                    OnPropertyChanged("CanPlay");
                }
            }
        }

        private long _durationTicks;
        public long DurationTicks
        {
            get
            {
                return _durationTicks;
            }
            set
            {
                var changed = !long.Equals(_durationTicks, value);
                _durationTicks = value;

                if (changed)
                {
                    OnPropertyChanged("DurationTicks");
                }
            }
        }

        private long _positionTicks;
        public long PositionTicks
        {
            get
            {
                return _positionTicks;
            }
            set
            {
                var changed = !long.Equals(_positionTicks, value);
                _positionTicks = value;

                if (changed)
                {
                    OnPropertyChanged("PositionTicks");
                }
            }
        }

        public TransportOsdViewModel(IPlaybackManager playbackManager, IApiClient apiClient, IImageManager imageManager, IPresentationManager presentationManager, ILogger logger)
        {
            Logger = logger;
            PresentationManager = presentationManager;
            ImageManager = imageManager;
            ApiClient = apiClient;
            PlaybackManager = playbackManager;

            _currentDispatcher = Dispatcher.CurrentDispatcher;

            PauseCommand = new RelayCommand(Pause);
            StopCommand = new RelayCommand(Stop);
            SkipBackwardCommand = new RelayCommand(SkipBackward);
            SkipForwardCommand = new RelayCommand(SkipForward);
            NextChapterCommand = new RelayCommand(NextChapter);
            PreviousChapterCommand = new RelayCommand(PreviousChapter);
            PlayCommand = new RelayCommand(Play);

            MediaPlayer = playbackManager.MediaPlayers.FirstOrDefault(i => i.PlayState != PlayState.Idle);
        }

        private void RemovePlayerEvents(IMediaPlayer player)
        {
            player.MediaChanged -= player_MediaChanged;
            player.PlayStateChanged -= player_PlayStateChanged;
        }

        void player_MediaChanged(object sender, MediaChangeEventArgs e)
        {
            NowPlayingItem = e.NewMedia;
        }

        private void UpdatePosition(IMediaPlayer player)
        {
            var ticks = player != null ? player.CurrentPositionTicks : null;

            DisplayPosition = ticks.HasValue ? GetTimeString(ticks.Value) : "--:--";
            PositionTicks = ticks.HasValue ? ticks.Value : 0;

            OnPropertyChanged("ClockShortTime");
        }

        public ChapterInfoListViewModel CreateChaptersViewModel()
        {
            var vm = new ChapterInfoListViewModel(ApiClient, ImageManager, PlaybackManager, PresentationManager)
            {
                Item = NowPlayingItem,
                StartPositionTicks = PositionTicks
            };

            return vm;
        }

        /// <summary>
        /// Gets the time string.
        /// </summary>
        /// <param name="ticks">The ticks.</param>
        /// <returns>System.String.</returns>
        private string GetTimeString(long ticks)
        {
            var timespan = TimeSpan.FromTicks(ticks);

            return timespan.TotalHours >= 1 ? timespan.ToString("hh':'mm':'ss") : timespan.ToString("mm':'ss");
        }

        public override void OnPropertyChanged(string name)
        {
            base.OnPropertyChanged(name);

            if (string.Equals(name, "NowPlayingItem"))
            {
                var media = NowPlayingItem;

                var ticks = media != null ? media.RunTimeTicks : null;

                DisplayDuration = ticks.HasValue ? GetTimeString(ticks.Value) : "--:--";
                DurationTicks = ticks.HasValue ? ticks.Value : 0;

                var player = MediaPlayer;

                CanSeek = player != null && player.CanSeek;

                UpdatePauseValues(player);
                UpdatePlayerCapabilities(player, media);
            }
            else if (string.Equals(name, "MediaPlayer"))
            {
                var player = MediaPlayer;

                if (player != null)
                {
                    player.MediaChanged += player_MediaChanged;
                    player.PlayStateChanged += player_PlayStateChanged;

                    if (_currentPositionTimer == null)
                    {
                        var timer = new Timer(PositionTimerCallback, null, 0, 250);

                        _currentPositionTimer = timer;
                    }
                }
                else
                {
                    DisposeCurrentPositionTimer();
                }

                NowPlayingItem = player == null ? null : player.CurrentMedia;

                UpdatePauseValues(player);
            }
        }

        private void PositionTimerCallback(object state)
        {
            _currentDispatcher.InvokeAsync(() => UpdatePosition(MediaPlayer));
        }

        void player_PlayStateChanged(object sender, EventArgs e)
        {
            _currentDispatcher.InvokeAsync(() => UpdatePauseValues(MediaPlayer));
        }

        private void UpdatePlayerCapabilities(IMediaPlayer player, BaseItemDto media)
        {
            SupportsChapters = player != null && player.CanSeek && media != null && media.Chapters.Count > 0;

            var videoPlayer = player as IVideoPlayer;

            CanSelectAudioTrack = videoPlayer != null && videoPlayer.CanSelectAudioTrack && media != null && videoPlayer.AudioStreams.Count > 0;
            CanSelectSubtitleTrack = videoPlayer != null && videoPlayer.CanSelectSubtitleTrack && media != null && videoPlayer.SubtitleStreams.Count > 0;
        }

        private void UpdatePauseValues(IMediaPlayer player)
        {
            CanPlay = IsPaused = player != null && player.PlayState == PlayState.Paused;

            CanPause = player != null && player.CanPause && player.PlayState == PlayState.Playing;
        }

        public void Pause(object commandParameter)
        {
            if (_mediaPlayer != null)
            {
                _mediaPlayer.Pause();
            }
        }

        public void Stop(object commandParameter)
        {
            if (_mediaPlayer != null)
            {
                _mediaPlayer.Stop();
            }
        }

        public void SkipBackward(object commandParameter)
        {
            if (_mediaPlayer != null)
            {
                _mediaPlayer.SkipBackward();
            }
        }

        public void SkipForward(object commandParameter)
        {
            if (_mediaPlayer != null)
            {
                _mediaPlayer.SkipForward();
            }
        }

        public void NextChapter(object commandParameter)
        {
            if (_mediaPlayer != null)
            {
                _mediaPlayer.GoToNextChapter();
            }
        }

        public void PreviousChapter(object commandParameter)
        {
            if (_mediaPlayer != null)
            {
                _mediaPlayer.GoToPreviousChapter();
            }
        }

        public void Play(object commandParameter)
        {
            if (_mediaPlayer != null)
            {
                if (_mediaPlayer.PlayState == PlayState.Paused)
                {
                    _mediaPlayer.UnPause();
                }
            }
        }

        public void Seek(long positionTicks)
        {
            if (_mediaPlayer != null)
            {
                _mediaPlayer.Seek(positionTicks);
            }
        }

        public void Dispose()
        {
            var player = _mediaPlayer;

            if (player != null)
            {
                RemovePlayerEvents(player);
            }

            DisposeCurrentPositionTimer();
        }

        private void DisposeCurrentPositionTimer()
        {
            if (_currentPositionTimer != null)
            {
                _currentPositionTimer.Dispose();
                _currentPositionTimer = null;
            }
        }
    }
}
