using System.Collections.ObjectModel;
using System.ComponentModel;
using MediaBrowser.Theater.Api.UserInterface.ViewModels;
using MediaBrowser.Theater.Presentation.Events;

namespace MediaBrowser.Theater.DefaultTheme.ViewModels
{
    public class NotificationTrayViewModel
        : BaseViewModel
    {
        public NotificationTrayViewModel(IEventAggregator events)
        {
            Notifications = new ObservableCollection<BaseViewModel>();

            events.Get<ShowNotificationEvent>().Subscribe(e => {
                BaseViewModel vm = e.ViewModel;

                PropertyChangedEventHandler closed = null;
                closed = (sender, args) => {
                    if (vm.IsClosed) {
                        Notifications.Remove(vm);
                        vm.PropertyChanged -= closed;
                    }
                };

                vm.PropertyChanged += closed;

                Notifications.Add(vm);
            }, false);
        }

        public ObservableCollection<BaseViewModel> Notifications { get; private set; }
    }
}