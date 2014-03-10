using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using MediaBrowser.Theater.Api;
using MediaBrowser.Theater.Api.Commands;
using MediaBrowser.Theater.Api.Navigation;
using MediaBrowser.Theater.Api.UserInterface;
using MediaBrowser.Theater.Presentation.ViewModels;

namespace MediaBrowser.Theater.DefaultTheme.Core.ViewModels
{
    public class CommandBarViewModel
        : BaseViewModel
    {
        private readonly List<IGlobalMenuCommand> _commands;
        private readonly INavigator _navigator;
        private ObservableCollection<GlobalCommandViewModel> _commandViewModels;

        public CommandBarViewModel(ITheaterApplicationHost appHost, INavigator navigator)
        {
            _navigator = navigator;
            _commands = appHost.GetExports<IGlobalMenuCommand>().ToList();
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

        private async void UpdateCommandVisibility()
        {
            GlobalCommandViewModel[] visibleCommandViewModels = CommandViewModels.ToArray();
            var commandsToHide = visibleCommandViewModels.Where(v => !v.MenuCommand.EvaluateVisibility(_navigator.CurrentLocation));

            Action removeAction = () => {
                foreach (GlobalCommandViewModel viewModel in commandsToHide) {
                    RemoveCommandViewModel(viewModel);
                }
            };

            await removeAction.OnUiThreadAsync().ConfigureAwait(false);

            List<IGlobalMenuCommand> visibleCommands = visibleCommandViewModels.Select(c => c.MenuCommand).ToList();
            List<IGlobalMenuCommand> commandsToShow = _commands.Where(c => !visibleCommands.Contains(c) && c.EvaluateVisibility(_navigator.CurrentLocation)).ToList();

            Action addAction = () => {
                foreach (IGlobalMenuCommand command in commandsToShow) {
                    AddCommandViewModel(new GlobalCommandViewModel(command));
                }
            };

            await addAction.OnUiThreadAsync().ConfigureAwait(false);
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
            CommandViewModels.Remove(viewModel);
        }
    }

    public class GlobalCommandViewModel
        : BaseViewModel
    {
        private readonly IGlobalMenuCommand _menuCommand;

        public GlobalCommandViewModel(IGlobalMenuCommand menuCommand)
        {
            _menuCommand = menuCommand;
        }

        public IGlobalMenuCommand MenuCommand
        {
            get { return _menuCommand; }
        }

        public IViewModel Icon
        {
            get { return _menuCommand.IconViewModel; }
        }

        public ICommand ExecuteCommand
        {
            get { return _menuCommand.ExecuteCommand; }
        }

        public string DisplayName
        {
            get { return _menuCommand.DisplayName; }
        }
    }
}