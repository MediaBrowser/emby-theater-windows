using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using MediaBrowser.Theater.Api.Commands.ItemCommands;
using MediaBrowser.Theater.Api.UserInterface;

namespace MediaBrowser.Theater.DefaultTheme.ItemCommands
{
    public class CommandsPopupViewModel : BaseViewModel
    {
        public List<ItemCommandListItemViewModel> Commands { get; private set; }

        public CommandsPopupViewModel(IEnumerable<IItemCommand> commands)
        {
            Commands = commands.Select(c => new ItemCommandListItemViewModel(c)).ToList();
            var first = Commands.FirstOrDefault();
            if (first != null) {
                first.AutoFocus = true;
            }
        }
    }

    public class ItemCommandListItemViewModel : BaseViewModel
    {
        public bool AutoFocus { get; set; }

        public IViewModel Icon { get; private set; }
        public string Name { get; private set; }
        public ICommand ExecuteCommand { get; private set; }

        public ItemCommandListItemViewModel(IItemCommand command)
        {
            Icon = command.IconViewModel;
            Name = command.DisplayName;
            ExecuteCommand = command.ExecuteCommand;
        }
    }
}
