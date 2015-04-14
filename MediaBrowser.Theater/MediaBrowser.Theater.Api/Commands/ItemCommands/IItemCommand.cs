using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Input;
using MediaBrowser.Model.Dto;
using MediaBrowser.Theater.Api.UserInterface;

namespace MediaBrowser.Theater.Api.Commands.ItemCommands
{
    public interface IItemCommandsManager
    {
        Task<IEnumerable<IItemCommand>> GetCommands(BaseItemDto item);
        void ShowItemCommandsMenu(BaseItemDto item);
    }
    
    public interface IItemCommand
    {
        Task Initialize(BaseItemDto item);
        bool IsEnabled { get; }
        string DisplayName { get; }
        ICommand ExecuteCommand { get; }
        IViewModel IconViewModel { get; }
        int SortOrder { get; }
    }
}
