using System.Windows.Input;
using MediaBrowser.Theater.Api.Commands;
using MediaBrowser.Theater.Api.Navigation;
using MediaBrowser.Theater.Api.UserInterface;
using MediaBrowser.Theater.Presentation.ViewModels;

namespace MediaBrowser.Theater.DefaultTheme.Search.ViewModels
{
    public class SearchMenuCommand
        : IGlobalMenuCommand
    {
        public SearchMenuCommand(INavigator navigator)
        {
            ExecuteCommand = new RelayCommand(arg => navigator.Navigate(Go.To.Search()));
        }

        public IViewModel IconViewModel 
        {
            get { return new SearchCommandIconViewModel(); }
        }

        public MenuCommandGroup Group
        {
            get { return MenuCommandGroup.Navigation; }
        }

        public int SortOrder
        {
            get { return 0; }
        }

        public string DisplayName
        {
            get { return "Search"; }
        }

        public ICommand ExecuteCommand { get; private set; }
        public bool EvaluateVisibility(INavigationPath currentPath)
        {
            return !(currentPath is SearchPath) && !(currentPath is LoginPath);
        }
    }

    public class SearchCommandIconViewModel : BaseViewModel { }
}