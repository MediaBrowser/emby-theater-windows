using System.Windows.Input;
using MediaBrowser.Theater.Api.Navigation;
using MediaBrowser.Theater.Api.UserInterface;
using MediaBrowser.Theater.DefaultTheme.Search.ViewModels;
using MediaBrowser.Theater.Presentation.ViewModels;

namespace MediaBrowser.Theater.DefaultTheme.SideMenu.ViewModels
{
    class OpenSideMenuCommand
        : IGlobalCommand
    {
        public OpenSideMenuCommand(INavigator navigator)
        {
            ExecuteCommand = new RelayCommand(arg => navigator.Navigate(Go.To.SideMenu()));
        }

        public IViewModel IconViewModel 
        {
            get { return new OpenSideMenuCommandViewModel(); }
        }

        public string DisplayName
        {
            get { return "User"; }
        }

        public ICommand ExecuteCommand { get; private set; }

        public bool EvaluateVisibility(INavigationPath currentPath)
        {
            return true;
        }
    }

    public class OpenSideMenuCommandViewModel : BaseViewModel { }
}
