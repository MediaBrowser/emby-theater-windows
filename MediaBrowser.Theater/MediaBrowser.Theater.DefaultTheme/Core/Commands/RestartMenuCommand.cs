using System.Diagnostics;
using System.Windows.Input;
using MediaBrowser.Theater.Api.Commands;
using MediaBrowser.Theater.Api.UserInterface;
using MediaBrowser.Theater.Presentation;
using MediaBrowser.Theater.Presentation.ViewModels;

namespace MediaBrowser.Theater.DefaultTheme.Core.Commands
{
    public class RestartMenuCommand
        : IMenuCommand
    {
        public RestartMenuCommand( /*IPlaybackManager playbackManager*/)
        {
            ExecuteCommand = new RelayCommand(arg => {
                //playbackManager.StopAllPlayback();
                Process.Start("shutdown", "/r /t 0");
            });
        }

        public string DisplayName
        {
            get { return "MediaBrowser.Theater.DefaultTheme:Strings:Core_RestartCommand".Localize(); }
        }

        public ICommand ExecuteCommand { get; private set; }

        public IViewModel IconViewModel
        {
            get { return new RestartMenuCommandIconViewModel(); }
        }

        public MenuCommandGroup Group
        {
            get { return MenuCommandGroup.Power; }
        }

        public int SortOrder
        {
            get { return 30; }
        }
    }

    public class RestartMenuCommandIconViewModel
        : BaseViewModel { }
}