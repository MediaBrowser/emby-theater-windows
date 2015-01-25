using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Threading;
using MediaBrowser.Model.ApiClient;
using MediaBrowser.Model.Dto;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.Logging;
using MediaBrowser.Theater.Api.Events;
using MediaBrowser.Theater.Api.Library;
using MediaBrowser.Theater.Api.Navigation;
using MediaBrowser.Theater.Api.Playback;
using MediaBrowser.Theater.Api.UserInterface;
using MediaBrowser.Theater.DefaultTheme.Core.ViewModels;
using MediaBrowser.Theater.DefaultTheme.ItemDetails.ViewModels;
using MediaBrowser.Theater.Presentation.ViewModels;

namespace MediaBrowser.Theater.DefaultTheme.Osd.ViewModels
{
    public class OsdChaptersViewModel : BaseViewModel
    {
        public List<OsdChapterViewModel> Chapters { get; set; }

        public OsdChaptersViewModel(BaseItemDto item, IConnectionManager connectionManager, IImageManager imageManager, IVideoPlayer player)
        {
            Chapters = item.Chapters.Select(c => {
                var vm = new OsdChapterViewModel(item, c, connectionManager, imageManager, player);
                vm.Selected += () => Close();
                return vm;
            }).ToList();

            for (int i = 0; i < Chapters.Count; i++) {
                var current = Chapters[i];
                var next = i < Chapters.Count - 1 ? item.Chapters[i + 1] : null;

                if (next == null || next.StartPositionTicks > player.CurrentPositionTicks) {
                    current.IsPlaying = true;
                    break;
                }
            }

            if (Chapters.Count > 0 && !Chapters.Any(c => c.IsPlaying)) {
                Chapters.First().IsPlaying = true;
            }
        }
    }

    public class OsdAudioTracksViewModel : BaseViewModel
    {
        public IEnumerable<AudioTrackViewModel> Streams { get; set; }

        public OsdAudioTracksViewModel(IVideoPlayer player)
        {
            Streams = player.SelectableStreams
                            .Where(s => s.Type == MediaStreamType.Audio)
                            .Select(s => {
                                var vm = new AudioTrackViewModel(s, player);
                                vm.Selected += () => Close();
                                return vm;
                            });
        }
    }

    public class OsdSubtitleTracksViewModel : BaseViewModel
    {
        public IEnumerable<SubtitleViewModel> Streams { get; set; }

        public OsdSubtitleTracksViewModel(IVideoPlayer player)
        {
            Streams = player.SelectableStreams
                            .Where(s => s.Type == MediaStreamType.Subtitle)
                            .Select(s => {
                                var vm = new SubtitleViewModel(s, player);
                                vm.Selected += () => Close();
                                return vm;
                            });
        }
    }

    public class OsdChapterViewModel : ChapterViewModel
    {
        public bool IsPlaying { get; set; }

        public OsdChapterViewModel(BaseItemDto item, ChapterInfoDto chapter, IConnectionManager connectionManager, IImageManager imageManager, IVideoPlayer player) 
            : base(item, chapter, connectionManager, imageManager, player) { }
    }

    public class SubtitleViewModel : BaseViewModel
    {
        private readonly SelectableMediaStream _stream;
        private readonly IVideoPlayer _player;

        public event Action Selected;

        protected virtual void OnSelected()
        {
            Action handler = Selected;
            if (handler != null) {
                handler();
            }
        }

        public SubtitleViewModel(SelectableMediaStream stream, IVideoPlayer player)
        {
            _stream = stream;
            _player = player;

            ChangeStreamommand = new RelayCommand(arg =>
            {
                _player.SetSubtitleStreamIndex(stream.Index);
                OnSelected();
            });
        }

        public ICommand ChangeStreamommand { get; private set; }

        public bool IsPlaying
        {
            get { return _player.CurrentSubtitleStreamIndex == _stream.Index; } 
        }

        public string DisplayName
        {
            get { return _stream.Name; }
        }
    }

    public class AudioTrackViewModel : BaseViewModel
    {
        private readonly SelectableMediaStream _stream;
        private readonly IVideoPlayer _player;

        public event Action Selected;

        protected virtual void OnSelected()
        {
            Action handler = Selected;
            if (handler != null) {
                handler();
            }
        }

        public AudioTrackViewModel(SelectableMediaStream stream, IVideoPlayer player)
        {
            _stream = stream;
            _player = player;

            ChangeStreamommand = new RelayCommand(arg =>
            {
                _player.SetAudioStreamIndex(stream.Index);
                OnSelected();
            });
        }

        public ICommand ChangeStreamommand { get; private set; }

        public bool IsPlaying
        {
            get { return _player.CurrentAudioStreamIndex == _stream.Index; }
        }

        public string DisplayName
        {
            get { return _stream.Name; }
        }
    }

    public class OsdViewModel : BaseViewModel, IDisposable, IHasRootPresentationOptions
    {
        private readonly IConnectionManager _connectionManager;
        // reference these handlers so they are not GC'd until the osd view model is GC'd
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
        private bool _showOsd;
        
        public OsdViewModel(IPlaybackManager playbackManager, IImageManager imageManager, IPresenter presentationManager, ILogger logger, INavigator nav, IEventAggregator events, IConnectionManager connectionManager)
        {
            _connectionManager = connectionManager;
            Logger = logger;
            PresentationManager = presentationManager;
            ImageManager = imageManager;
            PlaybackManager = playbackManager;
            NavigationService = nav;

            PauseCommand = new RelayCommand(Pause);
            StopCommand = new RelayCommand(Stop);
            SkipBackwardCommand = new RelayCommand(SkipBackward);
            SkipForwardCommand = new RelayCommand(SkipForward);
            NextChapterCommand = new RelayCommand(NextChapter);
            PreviousChapterCommand = new RelayCommand(PreviousChapter);
            PlayCommand = new RelayCommand(Play);
            PlayPauseCommand = new RelayCommand(PlayPause);
            SelectChapterCommand = new RelayCommand(ShowChapterSelection);
            SelectSubtitleTrackCommand = new RelayCommand(ShowSubtitleSelection);
            SelectAudioTrackCommand = new RelayCommand(ShowAudioSelection);

            _playbackStopHandler = args => {
                if (!IsActive) {
                    return;
                }

                MediaPlayer = null;
                NowPlayingItem = null;
                Close();
            };

            _playbackStartHandler = args => {
                MediaPlayer = args.Player;
                NowPlayingItem = args.Player.CurrentMedia;
            };

            Closed += (s, e) => {
                OnPropertyChanged("ShowOsd");
                nav.Back();
            };

            events.Get<PlaybackStopEventArgs>().Subscribe(_playbackStopHandler, true);
            events.Get<PlaybackStartEventArgs>().Subscribe(_playbackStartHandler, true);
           
            MediaPlayer = playbackManager.MediaPlayers.FirstOrDefault(i => i.PlayState != PlayState.Idle);

            PresentationOptions = new RootPresentationOptions {
                IsFullScreenPage = true,
                ShowClock = false,
                ShowCommandBar = false,
                ShowMediaBrowserLogo = false,
                ShowHighPriorityNotifications = true,
                ShowNotifications = false,
                PlaybackBackgroundOpacity = 0.0
            };
        }

        private int _delayCounter;
        private async void Delay(TimeSpan duration, Action action)
        {
            var count = Interlocked.Increment(ref _delayCounter);
            await Task.Delay(duration);
            if (_delayCounter == count) {
                action();
            }
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
        public ICommand SelectChapterCommand { get; private set; }
        public ICommand SelectSubtitleTrackCommand { get; private set; }
        public ICommand SelectAudioTrackCommand { get; private set; }


        public bool ShowOsd
        {
            get { return _showOsd && NowPlayingItem != null && IsActive; }
            set
            {
                if (Equals(ShowOsd, value)) {
                    return;
                }

                _showOsd = value;
                OnPropertyChanged();
            }
        }

        public IMediaPlayer MediaPlayer
        {
            get { return _mediaPlayer; }
            set
            {
                if (Equals(_mediaPlayer, value)) {
                    return;
                }
                
                if (_mediaPlayer != null) {
                    RemovePlayerEvents(_mediaPlayer);
                }

                _mediaPlayer = value;

                if (_mediaPlayer != null) {
                    _mediaPlayer.MediaChanged += player_MediaChanged;
                    _mediaPlayer.PlayStateChanged += player_PlayStateChanged;

                    if (_currentPositionTimer == null) {
                        var timer = new Timer(PositionTimerCallback, null, 0, 250);

                        _currentPositionTimer = timer;
                    }
                } else {
                    DisposeCurrentPositionTimer();
                }

                NowPlayingItem = _mediaPlayer == null ? null : _mediaPlayer.CurrentMedia;
                UpdatePauseValues(_mediaPlayer);

                OnPropertyChanged();
            }
        }

        public BaseItemDto NowPlayingItem
        {
            get { return _nowPlayingItem; }
            set
            {
                if (Equals(_nowPlayingItem, value)) {
                    return;
                }

                _nowPlayingItem = value;

                IMediaPlayer player = MediaPlayer;

                long? ticks = player != null ? player.CurrentDurationTicks : null;

                DisplayDuration = ticks.HasValue ? GetTimeString(ticks.Value) : "--:--";
                DurationTicks = ticks.HasValue ? ticks.Value : 0;

                CanSeek = player != null && player.CanSeek;

                UpdatePauseValues(player);
                UpdatePlayerCapabilities(player, _nowPlayingItem);

                OnPropertyChanged();
                OnPropertyChanged("DisplayName");
                OnPropertyChanged("ShowOsd");
                TemporarilyShowOsd();
            }
        }

        public string DisplayName
        {
            get { return _nowPlayingItem.GetDisplayName(new DisplayNameFormat(true, false)); }
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
                    OnPropertyChanged();
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
                    OnPropertyChanged();
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
                    OnPropertyChanged();
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
                    OnPropertyChanged();
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
                    OnPropertyChanged();
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
                    OnPropertyChanged();
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
                    OnPropertyChanged();
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
                    OnPropertyChanged();
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
                OnPropertyChanged();
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
                OnPropertyChanged();
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
                OnPropertyChanged();
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
        
        private void PositionTimerCallback(object state)
        {
            UpdatePosition(MediaPlayer);
        }

        private void player_PlayStateChanged(object sender, EventArgs e)
        {
            UpdatePauseValues(MediaPlayer);

            if (MediaPlayer.PlayState == PlayState.Playing) {
                Delay(TimeSpan.FromSeconds(1), () => ShowOsd = false);
            }

            if (MediaPlayer.PlayState == PlayState.Paused) {
                Delay(TimeSpan.Zero, () => ShowOsd = true);
            }
        }

        public void ToggleOsd()
        {
            Delay(TimeSpan.Zero, () => ShowOsd = !ShowOsd);
        }

        public void TemporarilyShowOsd()
        {
            Delay(TimeSpan.FromSeconds(0), () => {
                ShowOsd = true;
                Delay(TimeSpan.FromSeconds(3), () => ShowOsd = false);
            });
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

            CanSelectAudioTrack = videoPlayer != null && videoPlayer.CanSelectAudioTrack && media != null && audioStreams.Count > 1;
            CanSelectSubtitleTrack = videoPlayer != null && videoPlayer.CanSelectSubtitleTrack && media != null && subtitleStreams.Count > 1;
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

        public void ShowChapterSelection(object commandParameter)
        {
            var player = MediaPlayer as IVideoPlayer;
            if (player != null) {
                var vm = new OsdChaptersViewModel(NowPlayingItem, _connectionManager, ImageManager, player);
                PresentationManager.ShowPopup(vm, false, false);
            }
        }

        public void ShowSubtitleSelection(object commandParameter)
        {
            var player = MediaPlayer as IVideoPlayer;
            if (player != null) {
                var vm = new OsdSubtitleTracksViewModel(player);
                PresentationManager.ShowPopup(vm, false, false);
            }
        }

        public void ShowAudioSelection(object commandParameter)
        {
            var player = MediaPlayer as IVideoPlayer;
            if (player != null) {
                var vm = new OsdAudioTracksViewModel(player);
                PresentationManager.ShowPopup(vm, false, false);
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