using System.Windows.Input;
using MediaBrowser.Theater.Api.Navigation;
using MediaBrowser.Theater.Api.UserInterface;
using MediaBrowser.Theater.Presentation.ViewModels;

namespace MediaBrowser.Theater.DefaultTheme.Search.ViewModels
{
    public class SearchCommand
        : IGlobalCommand
    {
        public SearchCommand(INavigator navigator)
        {
            ExecuteCommand = new RelayCommand(arg => navigator.Navigate(Go.To.Search()));
        }

        public IViewModel IconViewModel 
        {
            get { return new SearchCommandIconViewModel(); }
        }

        public string DisplayName
        {
            get { return "Search"; }
        }

        public ICommand ExecuteCommand { get; private set; }
    }

    public class SearchCommandIconViewModel : BaseViewModel { }
}