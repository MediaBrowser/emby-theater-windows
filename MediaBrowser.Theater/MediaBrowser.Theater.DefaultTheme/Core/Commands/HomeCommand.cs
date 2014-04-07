using System.Windows.Input;
using MediaBrowser.Theater.Api.Commands;
using MediaBrowser.Theater.Api.Navigation;
using MediaBrowser.Theater.Api.UserInterface;
using MediaBrowser.Theater.Presentation;
using MediaBrowser.Theater.Presentation.ViewModels;

namespace MediaBrowser.Theater.DefaultTheme.Core.Commands
{
    public class HomeCommand
        : IGlobalMenuCommand
    {
        public HomeCommand(INavigator navigator)
        {
            ExecuteCommand = new RelayCommand(arg => navigator.Navigate(Go.To.Home()));
        }

        public string DisplayName
        {
            get { return "MediaBrowser.Theater.DefaultTheme:Strings:HomeCommand".Localize(); }
        }

        public ICommand ExecuteCommand { get; private set; }

        public IViewModel IconViewModel
        {
            get { return new HomeCommandIconViewModel(); }
        }

        public MenuCommandGroup Group
        {
            get { return MenuCommandGroup.Navigation; }
        }

        public int SortOrder
        {
            get { return 0; }
        }

        public bool EvaluateVisibility(INavigationPath currentPath)
        {
            return !(currentPath is HomePath) && !(currentPath is LoginPath);
        }
    }

    public class HomeCommandIconViewModel : BaseViewModel { }
}