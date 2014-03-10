using System.Windows.Input;
using MediaBrowser.Theater.Api;
using MediaBrowser.Theater.Api.Commands;
using MediaBrowser.Theater.Api.UserInterface;
using MediaBrowser.Theater.Presentation.ViewModels;

namespace MediaBrowser.Theater.DefaultTheme.Core.Commands
{
    public class ExitMenuCommand
        : IMenuCommand
    {
        public ExitMenuCommand(ITheaterApplicationHost appHost)
        {
            ExecuteCommand = new RelayCommand(arg => appHost.Shutdown());
        }

        public string DisplayName
        {
            get { return "Exit Application"; }
        }

        public ICommand ExecuteCommand { get; private set; }

        public IViewModel IconViewModel
        {
            get { return new ExitMenuCommandIconViewModel(); }
        }

        public MenuCommandGroup Group
        {
            get { return MenuCommandGroup.Power; }
        }

        public int SortOrder
        {
            get { return 40; }
        }
    }

    public class ExitMenuCommandIconViewModel
        : BaseViewModel { }
}