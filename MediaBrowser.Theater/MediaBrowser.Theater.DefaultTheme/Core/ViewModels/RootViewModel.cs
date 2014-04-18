using System.ComponentModel;
using System.Threading.Tasks;
using MediaBrowser.Theater.Api;
using MediaBrowser.Theater.Api.Events;
using MediaBrowser.Theater.Api.Navigation;
using MediaBrowser.Theater.Api.Playback;
using MediaBrowser.Theater.Api.UserInterface;
using MediaBrowser.Theater.Presentation.ViewModels;

namespace MediaBrowser.Theater.DefaultTheme.Core.ViewModels
{
    public class RootViewModel
        : BaseViewModel
    {
        private readonly INavigator _navigator;
        private readonly RootContext _rootContext;
        private IViewModel _activePage;
        private IViewModel _backgroundMedia;
        private bool _isInFocus;
        private NotificationTrayViewModel _notifications;
        private CommandBarViewModel _commands;
        private ClockViewModel _clock;
        private bool _isInternalMediaPlaying;

        public RootViewModel(IEventAggregator events, INavigator navigator, ITheaterApplicationHost appHost, RootContext rootContext)
        {
            _navigator = navigator;
            _rootContext = rootContext;
            Notifications = new NotificationTrayViewModel(events);
            Commands = new CommandBarViewModel(appHost, navigator);
            Clock = new ClockViewModel();
            IsInFocus = true;

            events.Get<ShowPageEvent>().Subscribe(message => ActivePage = message.ViewModel);
            events.Get<PlaybackStopEventArgs>().Subscribe(message => IsInternalMediaPlaying = false);
            events.Get<PlaybackStartEventArgs>().Subscribe(message => {
                if (message.Player is IInternalMediaPlayer && message.Player is IVideoPlayer && message.Options.Items[0].IsVideo) {
                    IsInternalMediaPlaying = true;
                }
            });
        }

        public bool IsInternalMediaPlaying
        {
            get { return _isInternalMediaPlaying; }
            private set
            {
                if (value.Equals(_isInternalMediaPlaying)) {
                    return;
                }
                _isInternalMediaPlaying = value;
                OnPropertyChanged();
            }
        }

        public IViewModel ActivePage
        {
            get { return _activePage; }
            set
            {
                if (Equals(value, _activePage)) {
                    return;
                }

                if (_activePage != null) {
                    _activePage.PropertyChanged -= ActivePagePropertyChanged;
                }

                _activePage = value;

                if (_activePage != null) {
                    _activePage.PropertyChanged += ActivePagePropertyChanged;
                }

                OnPropertyChanged();
                OnPropertyChanged("DisplayLogo");
                OnPropertyChanged("DisplayCommandBar");
                OnPropertyChanged("DisplayTitle");
                OnPropertyChanged("Title");
            }
        }

        public NotificationTrayViewModel Notifications
        {
            get { return _notifications; }
            private set
            {
                if (Equals(value, _notifications)) {
                    return;
                }
                _notifications = value;
                OnPropertyChanged();
            }
        }

        public CommandBarViewModel Commands
        {
            get { return _commands; }
            private set
            {
                if (Equals(_commands, value)) {
                    return;
                }

                _commands = value;
                OnPropertyChanged();
            }
        }

        public ClockViewModel Clock
        {
            get { return _clock; }
            private set
            {
                if (Equals(_clock, value)) {
                    return;
                }

                _clock = value;
                OnPropertyChanged();
            }
        }

        public IViewModel BackgroundMedia
        {
            get { return _backgroundMedia; }
            set
            {
                if (Equals(value, _backgroundMedia)) {
                    return;
                }
                _backgroundMedia = value;
                OnPropertyChanged();
            }
        }

        public bool IsInFocus
        {
            get { return _isInFocus; }
            set
            {
                if (value.Equals(_isInFocus)) {
                    return;
                }
                _isInFocus = value;
                OnPropertyChanged();
            }
        }

        public bool DisplayLogo
        {
            get
            {
                var options = ActivePage as IRootPresentationOptions;
                return options == null || options.ShowMediaBrowserLogo;
            }
        }

        public bool DisplayCommandBar
        {
            get
            {
                var options = ActivePage as IRootPresentationOptions;
                return options == null || options.ShowCommandBar;
            }
        }

        public bool DisplayClock
        {
            get
            {
                var options = ActivePage as IRootPresentationOptions;
                return options == null || options.ShowClock;
            }
        }

        public bool DisplayTitle
        {
            get
            {
                var options = ActivePage as IRootPresentationOptions;
                return options != null && !string.IsNullOrEmpty(options.Title);
            }
        }

        public string Title
        {
            get
            {
                var options = ActivePage as IRootPresentationOptions;
                return options != null ? options.Title : null;
            }
        }

        private void ActivePagePropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "ShowMediaBrowserLogo") {
                OnPropertyChanged("DisplayLogo");
            }

            if (e.PropertyName == "ShowCommandBar") {
                OnPropertyChanged("DisplayCommandBar");
            }

            if (e.PropertyName == "ShowClock") {
                OnPropertyChanged("DisplayClock");
            }

            if (e.PropertyName == "Title") {
                OnPropertyChanged("DisplayTitle");
                OnPropertyChanged("Title");
            }
        }

        public override async Task Initialize()
        {
            await _navigator.Initialize(_rootContext);
            await base.Initialize();
        }
    }
}