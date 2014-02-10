using System;
using MediaBrowser.Theater.Api.Theming.Navigation;
using MediaBrowser.Theater.Api.Theming.ViewModels;
using MediaBrowser.Theater.Presentation.Events;

namespace MediaBrowser.Theater.DefaultTheme
{
    public struct ShowPageEvent
    {
        public BaseViewModel ViewModel { get; set; }
    }
    
    public class PresentationManager
        : IPresentationManager
    {
        private readonly IEventBus<ShowPageEvent> _showPageEvent;

        public PresentationManager(IEventAggregator events)
        {
            _showPageEvent = events.Get<ShowPageEvent>();
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
            throw new NotImplementedException();
        }
    }
}