using System.Windows.Input;
using MediaBrowser.Theater.Api.Commands;
using MediaBrowser.Theater.Api.Navigation;
using MediaBrowser.Theater.Api.Session;
using MediaBrowser.Theater.Api.UserInterface;
using MediaBrowser.Theater.Presentation.ViewModels;

namespace MediaBrowser.Theater.DefaultTheme.SideMenu.Commands
{
    public class LogoutMenuCommand
        : IMenuCommand
    {
        public LogoutMenuCommand(ISessionManager session, INavigator navigator)
        {
            ExecuteCommand = new RelayCommand(async arg => {
                await session.Logout();
            });
        }

        public string DisplayName
        {
            get { return "Logout"; }
        }

        public ICommand ExecuteCommand { get; private set; }

        public IViewModel IconViewModel
        {
            get { return new LogoutMenuCommandIconViewModel(); }
        }

        public MenuCommandGroup Group
        {
            get { return MenuCommandGroup.User; }
        }

        public int SortOrder
        {
            get { return 10; }
        }
    }

    public class LogoutMenuCommandIconViewModel
        : BaseViewModel { }
}