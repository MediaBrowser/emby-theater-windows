using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows.Input;
using System.Windows.Threading;
using MediaBrowser.Model.ApiClient;
using MediaBrowser.Model.Dto;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.Logging;
using MediaBrowser.Theater.Api.Events;
using MediaBrowser.Theater.Api.Navigation;
using MediaBrowser.Theater.Api.Playback;
using MediaBrowser.Theater.Api.UserInterface;
using MediaBrowser.Theater.DefaultTheme.Core.ViewModels;
using MediaBrowser.Theater.Presentation.ViewModels;

namespace MediaBrowser.Theater.DefaultTheme.Osd.ViewModels
{
    public class OsdViewModel : BaseViewModel, IDisposable, IHasRootPresentationOptions
    {
        private readonly IServerEvents _serverEvents;
        private readonly Action<PlaybackStopEventArgs> _playbackStopHandler;
        private readonly Action<PlaybackStartEventArgs> _playbackStartHandler;
        private bool _canPause;
        private bool _canPlay;
        private bool _canSeek;
        private bool _canSelectAudioTrack;
        private bool _canSelectSubtitleTrack;
        private Timer _currentPositionTimer;
        private string _displayDuration;
        private string _displayPosition;
        private long _durationTicks;
        private bool _isPaused;
        private IMediaPlayer _mediaPlayer;
        private BaseItemDto _nowPlayingItem;
        private long _positionTicks;
        private bool _supportsChapters;

        public OsdViewModel(IPlaybackManager playbackManager, IApiClient apiClient, IImageManager imageManager, IPresenter presentationManager, ILogger logger, INavigator nav, IServerEvents serverEvents, IEventAggregator events)
        {
            Logger = logger;
            PresentationManager = presentationManager;
            ImageManager = imageManager;
            ApiClient = apiClient;
            PlaybackManager = playbackManager;
            NavigationService = nav;
            _serverEvents = serverEvents;

            PauseCommand = new RelayCommand(Pause);
            StopCommand = new RelayCommand(Stop);
            SkipBackwardCommand = new RelayCommand(SkipBackward);
            SkipForwardCommand = new RelayCommand(SkipForward);
            NextChapterCommand = new RelayCommand(NextChapter);
            PreviousChapterCommand = new RelayCommand(PreviousChapter);
            PlayCommand = new RelayCommand(Play);
            PlayPauseCommand = new RelayCommand(PlayPause);

            _playbackStopHandler = args => NavigationService.Back();
            _playbackStartHandler = args => {
                MediaPlayer = args.Player;
                NowPlayingItem = args.Player.CurrentMedia;
            };

            events.Get<PlaybackStopEventArgs>().Subscribe(_playbackStopHandler, true);
            events.Get<PlaybackStartEventArgs>().Subscribe(_playbackStartHandler, true);
           
            MediaPlayer = playbackManager.MediaPlayers.FirstOrDefault(i => i.PlayState != PlayState.Idle);

            PresentationOptions = new RootPresentationOptions {
                IsFullScreenPage = true,
                ShowClock = false,
                ShowCommandBar = false,
                ShowMediaBrowserLogo = false
            };
        }

        public IApiClient ApiClient { get; private set; }
        public IImageManager ImageManager { get; private set; }
        public IPlaybackManager PlaybackManager { get; set; }
        public IPresenter PresentationManager { get; set; }
        public ILogger Logger { get; set; }
        public INavigator NavigationService { get; set; }

        public ICommand PauseCommand { get; private set; }
        public ICommand NextChapterCommand { get; private set; }
        public ICommand PreviousChapterCommand { get; private set; }
        public ICommand SkipBackwardCommand { get; private set; }
        public ICommand SkipForwardCommand { get; private set; }
        public ICommand StopCommand { get; private set; }
        public ICommand PlayCommand { get; private set; }
        public ICommand PlayPauseCommand { get; private set; }

        public IMediaPlayer MediaPlayer
        {
            get { return _mediaPlayer; }
            set
            {
                IMediaPlayer old = _mediaPlayer;

                if (old != null) {
                    RemovePlayerEvents(old);
                }

                bool changed = old != value;
                _mediaPlayer = value;

                if (changed) {
                    OnPropertyChanged("MediaPlayer");
                }
            }
        }

        public BaseItemDto NowPlayingItem
        {
            get { return _nowPlayingItem; }
            set
            {
                bool changed = _nowPlayingItem != value;

                _nowPlayingItem = value;

                if (changed) {
                    OnPropertyChanged("NowPlayingItem");
                    OnPropertyChanged("DisplayName");
                }
            }
        }

        public string DisplayName
        {
            get { return ItemTileViewModel.GetDisplayNameWithAiredSpecial(_nowPlayingItem); }
        }

        public string ClockShortTime
        {
            get { return DateTime.Now.ToShortTimeString().Replace(" ", string.Empty); }
        }

        public string DisplayDuration
        {
            get { return _displayDuration; }
            set
            {
                bool changed = !string.Equals(_displayDuration, value);
                _displayDuration = value;

                if (changed) {
                    OnPropertyChanged("DisplayDuration");
                }
            }
        }

        public string DisplayPosition
        {
            get { return _displayPosition; }
            set
            {
                bool changed = !string.Equals(_displayPosition, value);
                _displayPosition = value;

                if (changed) {
                    OnPropertyChanged("DisplayPosition");
                }
            }
        }

        public bool CanSeek
        {
            get { return _canSeek; }
            set
            {
                bool changed = !Equals(_canSeek, value);
                _canSeek = value;

                if (changed) {
                    OnPropertyChanged("CanSeek");
                }
            }
        }

        public bool IsPaused
        {
            get { return _isPaused; }
            set
            {
                bool changed = !Equals(_isPaused, value);
                _isPaused = value;

                if (changed) {
                    OnPropertyChanged("IsPaused");
                }
            }
        }

        public bool CanPause
        {
            get { return _canPause; }
            set
            {
                bool changed = !Equals(_canPause, value);
                _canPause = value;

                if (changed) {
                    OnPropertyChanged("CanPause");
                }
            }
        }

        public bool CanPlay
        {
            get { return _canPlay; }
            set
            {
                bool changed = !Equals(_canPlay, value);
                _canPlay = value;

                if (changed) {
                    OnPropertyChanged("CanPlay");
                }
            }
        }

        public long DurationTicks
        {
            get { return _durationTicks; }
            set
            {
                bool changed = !Equals(_durationTicks, value);
                _durationTicks = value;

                if (changed) {
                    OnPropertyChanged("DurationTicks");
                }
            }
        }

        public long PositionTicks
        {
            get { return _positionTicks; }
            set
            {
                bool changed = !Equals(_positionTicks, value);
                _positionTicks = value;

                if (changed) {
                    OnPropertyChanged("PositionTicks");
                }
            }
        }

        public bool CanSelectSubtitleTrack
        {
            get { return _canSelectSubtitleTrack; }
            set
            {
                if (value.Equals(_canSelectSubtitleTrack)) {
                    return;
                }
                _canSelectSubtitleTrack = value;
                OnPropertyChanged("CanSelectSubtitleTrack");
            }
        }

        public bool CanSelectAudioTrack
        {
            get { return _canSelectAudioTrack; }
            set
            {
                if (value.Equals(_canSelectAudioTrack)) {
                    return;
                }
                _canSelectAudioTrack = value;
                OnPropertyChanged("CanSelectAudioTrack");
            }
        }

        public bool SupportsChapters
        {
            get { return _supportsChapters; }
            private set
            {
                if (value.Equals(_supportsChapters)) {
                    return;
                }
                _supportsChapters = value;
                OnPropertyChanged("SupportsChapters");
            }
        }

        public void Dispose()
        {
            IMediaPlayer player = _mediaPlayer;

            if (player != null) {
                RemovePlayerEvents(player);
            }

            DisposeCurrentPositionTimer();
        }

        private void RemovePlayerEvents(IMediaPlayer player)
        {
            player.MediaChanged -= player_MediaChanged;
            player.PlayStateChanged -= player_PlayStateChanged;
        }

        private void player_MediaChanged(object sender, MediaChangeEventArgs e)
        {
            NowPlayingItem = e.NewMedia;
        }

        private void UpdatePosition(IMediaPlayer player)
        {
            long? ticks = player != null ? player.CurrentPositionTicks : null;

            DisplayPosition = ticks.HasValue ? GetTimeString(ticks.Value) : "--:--";
            PositionTicks = ticks.HasValue ? ticks.Value : 0;

            OnPropertyChanged("ClockShortTime");
        }

        /// <summary>
        ///     Gets the time string.
        /// </summary>
        /// <param name="ticks">The ticks.</param>
        /// <returns>System.String.</returns>
        private string GetTimeString(long ticks)
        {
            TimeSpan timespan = TimeSpan.FromTicks(ticks);

            return timespan.TotalHours >= 1 ? timespan.ToString("hh':'mm':'ss") : timespan.ToString("mm':'ss");
        }

        protected override void OnPropertyChanged(string name)
        {
            base.OnPropertyChanged(name);

            if (string.Equals(name, "NowPlayingItem")) {
                BaseItemDto media = NowPlayingItem;

                IMediaPlayer player = MediaPlayer;

                long? ticks = player != null ? player.CurrentDurationTicks : null;

                DisplayDuration = ticks.HasValue ? GetTimeString(ticks.Value) : "--:--";
                DurationTicks = ticks.HasValue ? ticks.Value : 0;

                CanSeek = player != null && player.CanSeek;

                UpdatePauseValues(player);
                UpdatePlayerCapabilities(player, media);
            } else if (string.Equals(name, "MediaPlayer")) {
                IMediaPlayer player = MediaPlayer;

                if (player != null) {
                    player.MediaChanged += player_MediaChanged;
                    player.PlayStateChanged += player_PlayStateChanged;

                    if (_currentPositionTimer == null) {
                        var timer = new Timer(PositionTimerCallback, null, 0, 250);

                        _currentPositionTimer = timer;
                    }
                } else {
                    DisposeCurrentPositionTimer();
                }

                NowPlayingItem = player == null ? null : player.CurrentMedia;

                UpdatePauseValues(player);
            }
        }

        private void PositionTimerCallback(object state)
        {
            UpdatePosition(MediaPlayer);
        }

        private void player_PlayStateChanged(object sender, EventArgs e)
        {
            UpdatePauseValues(MediaPlayer);
        }

        private void UpdatePlayerCapabilities(IMediaPlayer player, BaseItemDto media)
        {
            SupportsChapters = player != null && player.CanSeek && media != null && media.Chapters != null && media.Chapters.Count > 0;

            var videoPlayer = player as IVideoPlayer;

            IReadOnlyList<SelectableMediaStream> selectableStreams = videoPlayer == null
                                                                         ? new List<SelectableMediaStream>()
                                                                         : videoPlayer.SelectableStreams;

            List<SelectableMediaStream> audioStreams = selectableStreams.Where(i => i.Type == MediaStreamType.Audio).ToList();
            List<SelectableMediaStream> subtitleStreams = selectableStreams.Where(i => i.Type == MediaStreamType.Subtitle).ToList();

            CanSelectAudioTrack = videoPlayer != null && videoPlayer.CanSelectAudioTrack && media != null && audioStreams.Count > 0;
            CanSelectSubtitleTrack = videoPlayer != null && videoPlayer.CanSelectSubtitleTrack && media != null && subtitleStreams.Count > 0;
        }

        private void UpdatePauseValues(IMediaPlayer player)
        {
            CanPlay = IsPaused = player != null && player.PlayState == PlayState.Paused;
            CanPause = player != null && player.CanPause && player.PlayState == PlayState.Playing;
        }

        public void Pause(object commandParameter)
        {
            if (_mediaPlayer != null) {
                _mediaPlayer.Pause();
            }
        }

        public void Stop(object commandParameter)
        {
            if (_mediaPlayer != null) {
                _mediaPlayer.Stop();
            }
        }

        public void SkipBackward(object commandParameter)
        {
            if (_mediaPlayer != null) {
                _mediaPlayer.SkipBackward();
            }
        }

        public void SkipForward(object commandParameter)
        {
            if (_mediaPlayer != null) {
                _mediaPlayer.SkipForward();
            }
        }

        public void NextChapter(object commandParameter)
        {
            if (_mediaPlayer != null) {
                _mediaPlayer.GoToNextChapter();
            }
        }

        public void PreviousChapter(object commandParameter)
        {
            if (_mediaPlayer != null) {
                _mediaPlayer.GoToPreviousChapter();
            }
        }

        public void Play(object commandParameter)
        {
            if (_mediaPlayer != null) {
                if (_mediaPlayer.PlayState == PlayState.Paused) {
                    _mediaPlayer.UnPause();
                }
            }
        }

        public void PlayPause(object commandParameter)
        {
            if (IsPaused) {
                Play(commandParameter);
            } else {
                Pause(commandParameter);
            }
        }

        public void Seek(long positionTicks)
        {
            if (_mediaPlayer != null) {
                _mediaPlayer.Seek(positionTicks);
            }
        }

        private void DisposeCurrentPositionTimer()
        {
            if (_currentPositionTimer != null) {
                _currentPositionTimer.Dispose();
                _currentPositionTimer = null;
            }
        }

        public RootPresentationOptions PresentationOptions { get; private set; }
    }
}