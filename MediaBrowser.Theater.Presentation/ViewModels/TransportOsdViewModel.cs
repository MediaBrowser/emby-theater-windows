using System.Threading;
using System.Windows.Input;
using System.Windows.Threading;
using MediaBrowser.Model.ApiClient;
using MediaBrowser.Model.Dto;
using MediaBrowser.Theater.Interfaces.Playback;
using MediaBrowser.Theater.Interfaces.Presentation;
using System;
using System.Linq;
using MediaBrowser.Theater.Presentation.Playback;

namespace MediaBrowser.Theater.Presentation.ViewModels
{
    public class TransportOsdViewModel : BaseViewModel, IDisposable
    {
        public IApiClient ApiClient { get; private set; }
        public IImageManager ImageManager { get; private set; }
        private IPlaybackManager PlaybackManager { get; set; }

        public ICommand PauseCommand { get; private set; }
        public ICommand NextChapterCommand { get; private set; }
        public ICommand PreviousChapterCommand { get; private set; }
        public ICommand SkipBackwardCommand { get; private set; }
        public ICommand SkipForwardCommand { get; private set; }
        public ICommand StopCommand { get; private set; }
        public ICommand PlayCommand { get; private set; }

        private DispatcherTimer CurrentPositionTimer { get; set; }

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
                    OnPropertyChanged("NowPlayingItem");
                }
            }
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

        private bool _canPressPlay;
        public bool CanPressPlay
        {
            get
            {
                return _canPressPlay;
            }
            set
            {
                var changed = !bool.Equals(_canPressPlay, value);
                _canPressPlay = value;

                if (changed)
                {
                    OnPropertyChanged("CanPressPlay");
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

        public TransportOsdViewModel(IPlaybackManager playbackManager, IApiClient apiClient, IImageManager imageManager)
        {
            ImageManager = imageManager;
            ApiClient = apiClient;
            PlaybackManager = playbackManager;

            MediaPlayer = playbackManager.MediaPlayers.FirstOrDefault(i => i.PlayState != PlayState.Idle);

            PauseCommand = new RelayCommand(Pause);
            StopCommand = new RelayCommand(Stop);
            SkipBackwardCommand = new RelayCommand(SkipBackward);
            SkipForwardCommand = new RelayCommand(SkipForward);
            NextChapterCommand = new RelayCommand(NextChapter);
            PreviousChapterCommand = new RelayCommand(PreviousChapter);
            PlayCommand = new RelayCommand(Play);
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
                UpdateSupportsChapters(player, media);
            }
            else if (string.Equals(name, "MediaPlayer"))
            {
                var player = MediaPlayer;

                if (player != null)
                {
                    player.MediaChanged += player_MediaChanged;
                    player.PlayStateChanged += player_PlayStateChanged;

                    if (CurrentPositionTimer == null)
                    {
                        var timer = new DispatcherTimer(DispatcherPriority.Background)
                        {
                            Interval = TimeSpan.FromMilliseconds(250)
                        };

                        timer.Tick += timer_Tick;
                        timer.Start();
                        CurrentPositionTimer = timer;
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

        void timer_Tick(object sender, EventArgs e)
        {
            UpdatePosition(MediaPlayer);
        }

        void player_PlayStateChanged(object sender, EventArgs e)
        {
            UpdatePauseValues(MediaPlayer);
        }

        private void UpdateSupportsChapters(IMediaPlayer player, BaseItemDto media)
        {
            SupportsChapters = player != null && player.CanSeek && media != null && media.Chapters.Count > 0;
        }

        private void UpdatePauseValues(IMediaPlayer player)
        {
            CanPressPlay = IsPaused = player != null && player.PlayState == PlayState.Paused;

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
            if (CurrentPositionTimer != null)
            {
                CurrentPositionTimer.Tick -= timer_Tick;
                CurrentPositionTimer.Stop();
            }
        }
    }
}
