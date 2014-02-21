using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using MediaBrowser.Theater.Api;
using MediaBrowser.Theater.Api.UserInterface;
using MediaBrowser.Theater.Presentation.ViewModels;

namespace MediaBrowser.Theater.DefaultTheme.Core.ViewModels
{
    public class CommandBarViewModel
        : BaseViewModel
    {
        private ObservableCollection<GlobalCommandViewModel> _commands;

        public CommandBarViewModel(ITheaterApplicationHost appHost)
        {
            IEnumerable<IGlobalCommand> commands = appHost.GetExports<IGlobalCommand>();
            Commands = new ObservableCollection<GlobalCommandViewModel>(commands.Select(c => new GlobalCommandViewModel(c)));
        }

        public ObservableCollection<GlobalCommandViewModel> Commands
        {
            get { return _commands; }
            set
            {
                if (Equals(_commands, value)) {
                    return;
                }

                _commands = value;
                OnPropertyChanged();
            }
        }
    }

    public class GlobalCommandViewModel
        : BaseViewModel
    {
        private readonly IGlobalCommand _command;

        public GlobalCommandViewModel(IGlobalCommand command)
        {
            _command = command;
        }

        public IViewModel Icon
        {
            get { return _command.IconViewModel; }
        }

        public ICommand ExecuteCommand
        {
            get { return _command.ExecuteCommand; }
        }

        public string DisplayName
        {
            get { return _command.DisplayName; }
        }
    }
}