using System;
using MediaBrowser.Theater.Api.UserInterface.Navigation;
using MediaBrowser.Theater.Api.UserInterface.ViewModels;
using MediaBrowser.Theater.Presentation.Events;

namespace MediaBrowser.Theater.DefaultTheme
{
    public struct ShowPageEvent
    {
        public BaseViewModel ViewModel { get; set; }
    }

    public struct ShowNotificationEvent
    {
        public BaseViewModel ViewModel { get; set; }
    }
    
    public class PresentationManager
        : IPresentationManager
    {
        private readonly IEventBus<ShowPageEvent> _showPageEvent;
        private readonly IEventBus<ShowNotificationEvent> _showNotificationEvent;

        public PresentationManager(IEventAggregator events)
        {
            _showPageEvent = events.Get<ShowPageEvent>();
            _showNotificationEvent = events.Get<ShowNotificationEvent>();
        }

        public void ShowPage(BaseViewModel contents)
        {
            _showPageEvent.Publish(new ShowPageEvent { ViewModel = contents });
        }

        public void ShowPopup(BaseViewModel contents)
        {
            throw new NotImplementedException();
        }

        public void ShowNotification(BaseViewModel contents)
        {
            _showNotificationEvent.Publish(new ShowNotificationEvent { ViewModel = contents });
        }
    }
}