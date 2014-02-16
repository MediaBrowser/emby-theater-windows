using System.Threading.Tasks;
using MediaBrowser.Theater.Api.Events;
using MediaBrowser.Theater.Api.Navigation;
using MediaBrowser.Theater.Api.UserInterface;
using MediaBrowser.Theater.DefaultTheme.Navigation;
using MediaBrowser.Theater.Presentation.ViewModels;

namespace MediaBrowser.Theater.DefaultTheme.ViewModels
{
    public class RootViewModel
        : BaseViewModel
    {
        private readonly INavigator _navigator;
        private readonly RootContext _rootContext;
        private IViewModel _activePage;
        private NotificationTrayViewModel _notifications;
        private bool _isInFocus;
        private IViewModel _backgroundMedia;

        public RootViewModel(IEventAggregator events, INavigator navigator, RootContext rootContext)
        {
            _navigator = navigator;
            _rootContext = rootContext;
            Notifications = new NotificationTrayViewModel(events);
            IsInFocus = true;

            events.Get<ShowPageEvent>().Subscribe(message => ActivePage = message.ViewModel);
        }

        public IViewModel ActivePage
        {
            get { return _activePage; }
            set
            {
                if (Equals(value, _activePage)) {
                    return;
                }
                _activePage = value;
                OnPropertyChanged();
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

        public override async Task Initialize()
        {
            await _navigator.Initialize(_rootContext);
            await base.Initialize();
        }
    }
}