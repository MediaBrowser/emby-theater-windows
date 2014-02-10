using MediaBrowser.Theater.Api.Theming.ViewModels;
using MediaBrowser.Theater.Presentation.Events;

namespace MediaBrowser.Theater.DefaultTheme.ViewModels
{
    public class RootViewModel
        : BaseViewModel
    {
        private BaseViewModel _activePage;

        public RootViewModel(IEventAggregator events)
        {
            events.Get<ShowPageEvent>().Subscribe(message => ActivePage = message.ViewModel);
        }

        public BaseViewModel ActivePage
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
    }
}