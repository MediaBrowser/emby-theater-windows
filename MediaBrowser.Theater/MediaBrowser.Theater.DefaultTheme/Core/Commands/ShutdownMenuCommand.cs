using System.Diagnostics;
using System.Windows.Input;
using MediaBrowser.Theater.Api.Commands;
using MediaBrowser.Theater.Api.UserInterface;
using MediaBrowser.Theater.Presentation.ViewModels;

namespace MediaBrowser.Theater.DefaultTheme.Core.Commands
{
    public class ShutdownMenuCommand
        : IMenuCommand
    {
        public ShutdownMenuCommand( /*IPlaybackManager playbackManager*/)
        {
            ExecuteCommand = new RelayCommand(arg => {
                //playbackManager.StopAllPlayback();
                Process.Start("shutdown", "/s /t 0");
            });
        }

        public string DisplayName
        {
            get { return "Turn Off"; }
        }

        public ICommand ExecuteCommand { get; private set; }

        public IViewModel IconViewModel
        {
            get { return new ShutdownMenuCommandIconViewModel(); }
        }

        public MenuCommandGroup Group
        {
            get { return MenuCommandGroup.Power; }
        }

        public int SortOrder
        {
            get { return 10; }
        }
    }

    public class ShutdownMenuCommandIconViewModel
        : BaseViewModel { }
}