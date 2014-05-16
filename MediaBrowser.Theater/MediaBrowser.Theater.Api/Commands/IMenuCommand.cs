using System.Windows.Input;
using MediaBrowser.Theater.Api.UserInterface;

namespace MediaBrowser.Theater.Api.Commands
{
    public interface IMenuCommand
    {
        string DisplayName { get; }
        ICommand ExecuteCommand { get; }
        IViewModel IconViewModel { get; }
        MenuCommandGroup Group { get; }
        int SortOrder { get; }
    }

    public class MenuCommandGroup
    {
        public int SortOrder { get; set; }

        public static readonly MenuCommandGroup User = new MenuCommandGroup { SortOrder = 10 };
        public static readonly MenuCommandGroup Navigation = new MenuCommandGroup { SortOrder = 20 };
        public static readonly MenuCommandGroup Power = new MenuCommandGroup { SortOrder = 30 };
    }
}