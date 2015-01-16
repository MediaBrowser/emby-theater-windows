using System;
using System.Collections.ObjectModel;
using MediaBrowser.Theater.Api.Events;
using MediaBrowser.Theater.Api.UserInterface;
using MediaBrowser.Theater.Presentation.ViewModels;

namespace MediaBrowser.Theater.DefaultTheme.Core.ViewModels
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

                var activity = vm as IHasActivityStatus;
                if (activity != null) {
                    activity.IsActive = true;
                }

            }, false);
        }

        public ObservableCollection<IViewModel> Notifications { get; private set; }
    }
}