using System;
using System.ComponentModel;
using System.Threading.Tasks;
using MediaBrowser.Theater.Api;
using MediaBrowser.Theater.Api.Events;
using MediaBrowser.Theater.Api.Navigation;
using MediaBrowser.Theater.Api.Playback;
using MediaBrowser.Theater.Api.System;
using MediaBrowser.Theater.Api.UserInterface;
using MediaBrowser.Theater.Playback;
using MediaBrowser.Theater.Presentation.ViewModels;
using System.Reactive.Linq;

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
        private NotificationTrayViewModel _highPriorityNotifications;
        private CommandBarViewModel _commands;
        private ClockViewModel _clock;
        private bool _isInternalMediaPlaying;

        public RootViewModel(IEventAggregator events, INavigator navigator, ITheaterApplicationHost appHost, RootContext rootContext)
        {
            _navigator = navigator;
            _rootContext = rootContext;
            Notifications = new NotificationTrayViewModel(events, NotificationPriority.Normal);
            HighPriorityNotifications = new NotificationTrayViewModel(events, NotificationPriority.High);
            Commands = new CommandBarViewModel(appHost, navigator);
            Clock = new ClockViewModel();
            IsInFocus = true;

#if DEFAULT_THEME_STYLE_CHECK
            ActivePage = new StyleCheckViewModel();
#else
            events.Get<ShowPageEvent>().Subscribe(message => ActivePage = message.ViewModel);
#endif

            appHost.Resolve<IPlaybackManager>().Events.Subscribe(e => {
                IsInternalMediaPlaying = e.StatusType.IsActiveState();
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
            get { return _activePage == null || GetPresentationOptions().IsFullScreenPage ? null : _activePage; }
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
                OnPresentationOptionsChanged();
            }
        }

        public IViewModel FullScreenActivePage
        {
            get { return _activePage != null && GetPresentationOptions().IsFullScreenPage ? _activePage : null; }
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

        public NotificationTrayViewModel HighPriorityNotifications
        {
            get { return _highPriorityNotifications; }
            private set
            {
                if (Equals(value, _highPriorityNotifications))
                {
                    return;
                }

                _highPriorityNotifications = value;
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
            get { return GetPresentationOptions().ShowMediaBrowserLogo; }
        }

        public bool DisplayCommandBar
        {
            get { return GetPresentationOptions().ShowCommandBar; }
        }

        public bool DisplayClock
        {
            get { return GetPresentationOptions().ShowClock; }
        }

        public bool DisplayTitle
        {
            get { return !string.IsNullOrEmpty(GetPresentationOptions().Title); }
        }

        public string Title
        {
            get { return GetPresentationOptions().Title; }
        }

        public double PlaybackBackgroundOpacity
        {
            get { return GetPresentationOptions().PlaybackBackgroundOpacity; }
        }

        public bool DisplayNotifications
        {
            get { return GetPresentationOptions().ShowNotifications; }
        }

        public bool DisplayHighPriorityNotifications
        {
            get { return GetPresentationOptions().ShowHighPriorityNotifications; }
        }

        private RootPresentationOptions GetPresentationOptions()
        {
            var hasOptions = _activePage as IHasRootPresentationOptions;
            if (hasOptions == null || hasOptions.PresentationOptions == null) {
                return new RootPresentationOptions();
            }

            return hasOptions.PresentationOptions;
        }

        private void ActivePagePropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "PresentationOptions") {
                OnPresentationOptionsChanged();
            }
        }

        private void OnPresentationOptionsChanged()
        {
            OnPropertyChanged("FullScreenActivePage");
            OnPropertyChanged("DisplayLogo");
            OnPropertyChanged("DisplayCommandBar");
            OnPropertyChanged("DisplayClock");
            OnPropertyChanged("DisplayTitle");
            OnPropertyChanged("DisplayNotifications");
            OnPropertyChanged("DisplayHighPriorityNotifications");
            OnPropertyChanged("Title");
            OnPropertyChanged("PlaybackBackgroundOpacity");
        }

        public override async Task Initialize()
        {
            await _navigator.Initialize(_rootContext);
            await base.Initialize();
        }
    }
}