using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using MediaBrowser.Model.ApiClient;
using MediaBrowser.Model.Dto;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.Logging;
using MediaBrowser.Theater.Api.Events;
using MediaBrowser.Theater.Api.Library;
using MediaBrowser.Theater.Api.Navigation;
using MediaBrowser.Theater.Api.UserInterface;
using MediaBrowser.Theater.DefaultTheme.Core.ViewModels;
using MediaBrowser.Theater.DefaultTheme.ItemDetails.ViewModels;
using MediaBrowser.Theater.Playback;
using MediaBrowser.Theater.Presentation.ViewModels;

namespace MediaBrowser.Theater.DefaultTheme.Osd.ViewModels
{
    public class OsdChaptersViewModel : BaseViewModel
    {
        public OsdChaptersViewModel(BaseItemDto item, IConnectionManager connectionManager, IImageManager imageManager, IPlaybackManager playbackManager, PlaybackStatus latestStatus)
        {
            Chapters = item.Chapters.Select(c => {
                var vm = new OsdChapterViewModel(item, c, connectionManager, imageManager, playbackManager);
                vm.Selected += () => Close();
                return vm;
            }).ToList();

            for (int i = 0; i < Chapters.Count; i++) {
                OsdChapterViewModel current = Chapters[i];
                ChapterInfoDto next = i < Chapters.Count - 1 ? item.Chapters[i + 1] : null;

                if (next == null || next.StartPositionTicks > latestStatus.Progress) {
                    current.IsPlaying = true;
                    break;
                }
            }

            if (Chapters.Count > 0 && !Chapters.Any(c => c.IsPlaying)) {
                Chapters.First().IsPlaying = true;
            }
        }

        public List<OsdChapterViewModel> Chapters { get; set; }
    }

    public class OsdAudioTracksViewModel : BaseViewModel
    {
        public OsdAudioTracksViewModel(PlaybackStatus status, IPlaybackManager playbackManager)
        {
            Streams = status.PlayableMedia.Source.MediaStreams
                            .Where(s => s.Type == MediaStreamType.Audio)
                            .Select(s => {
                                var vm = new AudioTrackViewModel(s, status, playbackManager);
                                vm.Selected += () => Close();
                                return vm;
                            });
        }

        public IEnumerable<AudioTrackViewModel> Streams { get; set; }
    }

    public class OsdSubtitleTracksViewModel : BaseViewModel
    {
        public OsdSubtitleTracksViewModel(PlaybackStatus status, IPlaybackManager playbackManager)
        {
            Streams = status.PlayableMedia.Source.MediaStreams
                            .Where(s => s.Type == MediaStreamType.Audio)
                            .Select(s => {
                                var vm = new SubtitleViewModel(s, status, playbackManager);
                                vm.Selected += () => Close();
                                return vm;
                            });
        }

        public IEnumerable<SubtitleViewModel> Streams { get; set; }
    }

    public class OsdChapterViewModel : ChapterViewModel
    {
        public OsdChapterViewModel(BaseItemDto item, ChapterInfoDto chapter, IConnectionManager connectionManager, IImageManager imageManager, IPlaybackManager playbackManager)
            : base(item, chapter, connectionManager, imageManager, playbackManager) { }

        public bool IsPlaying { get; set; }
    }

    public class SubtitleViewModel : BaseViewModel
    {
        private readonly MediaStream _stream;

        public SubtitleViewModel(MediaStream stream, PlaybackStatus status, IPlaybackManager playbackManager)
        {
            _stream = stream;
            IsPlaying = status.ActiveStreams.Contains(stream);

            ChangeStreamCommand = new RelayCommand(arg => {
                playbackManager.AccessSession(s => s.SelectStream(MediaStreamType.Subtitle, stream.Index));
                OnSelected();
            });
        }

        public ICommand ChangeStreamCommand { get; private set; }

        public bool IsPlaying { get; private set; }

        public string DisplayName
        {
            get
            {
                string language = _stream.Language ?? "Unknown";
                string tags = new[] { _stream.IsDefault ? "default" : null, _stream.IsExternal ? "external" : null, _stream.IsForced ? "forced" : null, _stream.Codec }
                    .Where(t => !string.IsNullOrEmpty(t))
                    .Aggregate((a, b) => a + ", " + b);

                if (!string.IsNullOrEmpty(tags)) {
                    return string.Format("{0} ({1})", language, tags);
                }

                return language;
            }
        }

        public event Action Selected;

        protected virtual void OnSelected()
        {
            Action handler = Selected;
            if (handler != null) {
                handler();
            }
        }
    }

    public class AudioTrackViewModel : BaseViewModel
    {
        private readonly MediaStream _stream;

        public AudioTrackViewModel(MediaStream stream, PlaybackStatus status, IPlaybackManager playback)
        {
            _stream = stream;
            IsPlaying = status.ActiveStreams.Contains(stream);

            ChangeStreamCommand = new RelayCommand(arg => {
                playback.AccessSession(s => s.SelectStream(MediaStreamType.Audio, stream.Index));
                OnSelected();
            });
        }

        public ICommand ChangeStreamCommand { get; private set; }

        public bool IsPlaying { get; private set; }

        public string DisplayName
        {
            get
            {
                string language = _stream.Language ?? "Unknown";
                string tags = new[] { _stream.IsDefault ? "default" : null, _stream.ChannelLayout, _stream.Codec }
                    .Where(t => !string.IsNullOrEmpty(t))
                    .Aggregate((a, b) => a + ", " + b);

                if (!string.IsNullOrEmpty(tags)) {
                    return string.Format("{0} ({1})", language, tags);
                }

                return language;
            }
        }

        public event Action Selected;

        protected virtual void OnSelected()
        {
            Action handler = Selected;
            if (handler != null) {
                handler();
            }
        }
    }

    public class OsdViewModel : BaseViewModel, IDisposable, IHasRootPresentationOptions
    {
        private readonly IConnectionManager _connectionManager;
        private readonly IDisposable _eventsSubscription;
        private readonly IDisposable _sessionsSubscription;

        private bool _canPause;
        private bool _canPlay;
        private bool _canSeek;
        private bool _canSelectAudioTrack;
        private bool _canSelectSubtitleTrack;
        private Timer _clockTickTimer;
        private int _delayCounter;
        private string _displayDuration;
        private string _displayPosition;
        private long _durationTicks;
        private bool _isPaused;
        private BaseItemDto _nowPlayingItem;
        private long _positionTicks;
        private IPlaybackSession _session;
        private bool _showOsd;
        private PlaybackStatus _status;
        private bool _supportsChapters;
        private bool _canSkip;

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

            _sessionsSubscription = playbackManager.Sessions.Subscribe(session => {
                _session = session;
                UpdatePlayerCapabilities(session);
            });

            _eventsSubscription = playbackManager.Events.Subscribe(playbackEvent => {
                if (_status.StatusType != playbackEvent.StatusType) {
                    if (_status.StatusType == PlaybackStatusType.Playing) {
                        Delay(TimeSpan.FromSeconds(1), () => ShowOsd = false);
                    }

                    if (_status.StatusType == PlaybackStatusType.Paused) {
                        Delay(TimeSpan.Zero, () => ShowOsd = true);
                    }
                }

                _status = playbackEvent;

                if (playbackEvent.StatusType.IsActiveState()) {
                    NowPlayingItem = playbackEvent.PlayableMedia.Media.Item;
                    UpdateStatus(playbackEvent);
                } else if (IsActive) {
                    NowPlayingItem = null;
                    Close();
                }
            });

            Closed += (s, e) => {
                OnPropertyChanged("ShowOsd");
                nav.Back();
            };

            PresentationOptions = new RootPresentationOptions {
                IsFullScreenPage = true,
                ShowClock = false,
                ShowCommandBar = false,
                ShowMediaBrowserLogo = false,
                ShowHighPriorityNotifications = true,
                ShowNotifications = false,
                PlaybackBackgroundOpacity = 0.0
            };

            _clockTickTimer = new Timer(arg => OnPropertyChanged("ClockShortTime"), null, TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(1));
        }

        public override async Task Initialize()
        {
            await PlaybackManager.AccessSession(s => UpdatePlayerCapabilities(s));
            await base.Initialize();
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

        public BaseItemDto NowPlayingItem
        {
            get { return _nowPlayingItem; }
            set
            {
                if (Equals(_nowPlayingItem, value)) {
                    return;
                }

                _nowPlayingItem = value;

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

        public bool CanSkip
        {
            get { return _canSkip; }
            private set
            {
                if (value.Equals(_canSkip)) {
                    return;
                }
                _canSkip = value;
                OnPropertyChanged();
            }
        }

        public void Dispose()
        {
            DisposeClockTickTimer();

            _sessionsSubscription.Dispose();
            _eventsSubscription.Dispose();
        }

        public RootPresentationOptions PresentationOptions { get; private set; }

        private async void Delay(TimeSpan duration, Action action)
        {
            int count = Interlocked.Increment(ref _delayCounter);
            await Task.Delay(duration);
            if (_delayCounter == count) {
                action();
            }
        }

        private void UpdatePlayerCapabilities(IPlaybackSession session)
        {
            CanPlay = session.Capabilities.CanPlay;
            CanPause = session.Capabilities.CanPause;
            CanSeek = session.Capabilities.CanSeek;
        }

        private void UpdateStatus(PlaybackStatus status)
        {
            DisplayDuration = status.Duration.HasValue ? GetTimeString(status.Duration.Value) : "--:--";
            DurationTicks = status.Duration.HasValue ? status.Duration.Value : 0;

            DisplayPosition = status.Progress.HasValue ? GetTimeString(status.Progress.Value) : "--:--";
            PositionTicks = status.Progress.HasValue ? status.Progress.Value : 0;

            IsPaused = status.StatusType == PlaybackStatusType.Paused;

            CanSelectAudioTrack = _session.Capabilities.CanChangeStreams && status.PlayableMedia.Source != null && status.PlayableMedia.Source.MediaStreams.Count(stream => stream.Type == MediaStreamType.Audio) > 1;
            CanSelectSubtitleTrack = _session.Capabilities.CanChangeStreams && status.PlayableMedia.Source != null && status.PlayableMedia.Source.MediaStreams.Count(stream => stream.Type == MediaStreamType.Subtitle) > 1;

            SupportsChapters = _session.Capabilities.CanSeek && status.PlayableMedia.Media.Item.Chapters != null && status.PlayableMedia.Media.Item.Chapters.Count > 1;
            CanSkip = PlaybackManager.Queue.Count > 1;
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

        public void Pause(object commandParameter)
        {
            PlaybackManager.AccessSession(s => s.Pause());
        }

        public void Stop(object commandParameter)
        {
            PlaybackManager.StopPlayback();
        }

        public void SkipBackward(object commandParameter)
        {
            PlaybackManager.AccessSession(s => s.SkipToPrevious());
        }

        public void SkipForward(object commandParameter)
        {
            PlaybackManager.AccessSession(s => s.SkipToNext());
        }

        public void NextChapter(object commandParameter)
        {
            PlaybackManager.AccessSession(s => s.NextChapter());
        }

        public void PreviousChapter(object commandParameter)
        {
            PlaybackManager.AccessSession(s => s.PreviousChapter());
        }

        public void Play(object commandParameter)
        {
            PlaybackManager.AccessSession(s => s.Play());
        }

        public void PlayPause(object commandParameter)
        {
            PlaybackManager.AccessSession(s => s.PlayPause());
        }

        public void Seek(long positionTicks)
        {
            PlaybackManager.AccessSession(s => s.Seek(positionTicks));
        }

        public void ShowChapterSelection(object commandParameter)
        {
            var vm = new OsdChaptersViewModel(NowPlayingItem, _connectionManager, ImageManager, PlaybackManager, _status);
            PresentationManager.ShowPopup(vm, false, false);
        }

        public void ShowSubtitleSelection(object commandParameter)
        {
            var vm = new OsdSubtitleTracksViewModel(_status, PlaybackManager);
            PresentationManager.ShowPopup(vm, false, false);
        }

        public void ShowAudioSelection(object commandParameter)
        {
            var vm = new OsdAudioTracksViewModel(_status, PlaybackManager);
            PresentationManager.ShowPopup(vm, false, false);
        }

        private void DisposeClockTickTimer()
        {
            if (_clockTickTimer != null) {
                _clockTickTimer.Dispose();
                _clockTickTimer = null;
            }
        }
    }
}