using System;
using System.Threading.Tasks;
using MediaBrowser.Theater.Api.Events;
using MediaBrowser.Theater.Api.UserInterface.ViewModels;
using MediaBrowser.Theater.Presentation.ViewModels;

namespace MediaBrowser.Theater.DefaultTheme.ViewModels
{
    public class RootViewModel
        : BaseViewModel
    {
        private IViewModel _activePage;
        private NotificationTrayViewModel _notifications;
        private bool _isInFocus;
        private IViewModel _backgroundMedia;

        public RootViewModel(IEventAggregator events)
        {
            Notifications = new NotificationTrayViewModel(events);
            IsInFocus = true;

            events.Get<ShowPageEvent>().Subscribe(message => ActivePage = message.ViewModel);

            // test page
            events.Get<ShowPageEvent>().Publish(new ShowPageEvent { ViewModel = new HelloWorldViewModel() });
            events.Get<ShowNotificationEvent>().Publish(new ShowNotificationEvent {
                ViewModel = new NotificationViewModel(TimeSpan.FromSeconds(5)) {
                    Contents = new HelloWorldViewModel(),
                    Icon = new HelloWorldViewModel()
                }
            });

            Task.Run(async () => {
                while (true) {
                    await Task.Delay(3000);
                    events.Get<ShowNotificationEvent>().Publish(new ShowNotificationEvent {
                        ViewModel = new NotificationViewModel(TimeSpan.FromSeconds(5)) {
                            Contents = new HelloWorldViewModel(),
                            Icon = new HelloWorldViewModel()
                        }
                    });

                    IsInFocus = !IsInFocus;
                }
            });
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
    }
}