using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using MediaBrowser.Theater.Api;
using MediaBrowser.Theater.Api.Navigation;
using MediaBrowser.Theater.Api.UserInterface;
using MediaBrowser.Theater.Presentation.ViewModels;

namespace MediaBrowser.Theater.DefaultTheme.Core.ViewModels
{
    public class CommandBarViewModel
        : BaseViewModel
    {
        private readonly List<IGlobalCommand> _commands;
        private readonly INavigator _navigator;
        private ObservableCollection<GlobalCommandViewModel> _commandViewModels;

        public CommandBarViewModel(ITheaterApplicationHost appHost, INavigator navigator)
        {
            _navigator = navigator;
            _commands = appHost.GetExports<IGlobalCommand>().ToList();
            CommandViewModels = new ObservableCollection<GlobalCommandViewModel>();

            _navigator.Navigated += (s, e) => UpdateCommandVisibility();
        }

        public ObservableCollection<GlobalCommandViewModel> CommandViewModels
        {
            get { return _commandViewModels; }
            set
            {
                if (Equals(_commandViewModels, value)) {
                    return;
                }

                _commandViewModels = value;
                OnPropertyChanged();
            }
        }

        private void UpdateCommandVisibility()
        {
            GlobalCommandViewModel[] visibleCommandViewModels = CommandViewModels.ToArray();
            foreach (GlobalCommandViewModel viewModel in visibleCommandViewModels) {
                if (!viewModel.Command.EvaluateVisibility(_navigator.CurrentLocation)) {
                    RemoveCommandViewModel(viewModel);
                }
            }

            List<IGlobalCommand> visibleCommands = visibleCommandViewModels.Select(c => c.Command).ToList();
            List<IGlobalCommand> hiddenCommands = _commands.Where(c => !visibleCommands.Contains(c)).ToList();
            foreach (IGlobalCommand command in hiddenCommands) {
                if (command.EvaluateVisibility(_navigator.CurrentLocation)) {
                    AddCommandViewModel(new GlobalCommandViewModel(command));
                }
            }
        }

        private void AddCommandViewModel(GlobalCommandViewModel viewModel)
        {
            EventHandler closed = null;
            closed = (sender, args) => {
                RemoveCommandViewModel(viewModel);
                viewModel.Closed -= closed;
            };

            viewModel.Closed += closed;

            CommandViewModels.Add(viewModel);
        }

        private async void RemoveCommandViewModel(GlobalCommandViewModel viewModel)
        {
            await viewModel.Close();
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

        public IGlobalCommand Command
        {
            get { return _command; }
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