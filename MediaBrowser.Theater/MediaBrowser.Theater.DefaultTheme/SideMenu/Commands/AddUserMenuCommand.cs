using System.Windows.Input;
using MediaBrowser.Theater.Api.Commands;
using MediaBrowser.Theater.Api.UserInterface;
using MediaBrowser.Theater.Presentation.ViewModels;

namespace MediaBrowser.Theater.DefaultTheme.SideMenu.Commands
{
    public class AddUserMenuCommand
        : IMenuCommand
    {
        public string DisplayName
        {
            get { return "Add User to Session"; }
        }

        public ICommand ExecuteCommand { get; private set; }

        public IViewModel IconViewModel
        {
            get { return new AddUserCommandIconViewModel(); }
        }

        public MenuCommandGroup Group
        {
            get { return MenuCommandGroup.User; }
        }

        public int SortOrder
        {
            get { return 0; }
        }
    }

    public class AddUserCommandIconViewModel : BaseViewModel { }
}