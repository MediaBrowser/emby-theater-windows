using System;
using System.Collections.ObjectModel;
using MediaBrowser.Theater.Api.UserInterface.ViewModels;
using MediaBrowser.Theater.Presentation.Events;
using MediaBrowser.Theater.Presentation.ViewModels;

namespace MediaBrowser.Theater.DefaultTheme.ViewModels
{
    public class NotificationTrayViewModel
        : BaseViewModel
    {
        public NotificationTrayViewModel(IEventAggregator events)
        {
            Notifications = new ObservableCollection<IViewModel>();

            events.Get<ShowNotificationEvent>().Subscribe(e => {
                IViewModel vm = e.ViewModel;
                
                EventHandler closed = null;
                closed = (sender, args) => {
                    Notifications.Remove(vm);
                    vm.Closed -= closed;
                };

                vm.Closed += closed;

                Notifications.Add(vm);
            }, false);
        }

        public ObservableCollection<IViewModel> Notifications { get; private set; }
    }
}