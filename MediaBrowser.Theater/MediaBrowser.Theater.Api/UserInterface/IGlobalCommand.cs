using System.Windows.Input;

namespace MediaBrowser.Theater.Api.UserInterface
{
    public interface IGlobalCommand
    {
        IViewModel IconViewModel { get; }
        string DisplayName { get; }
        ICommand ExecuteCommand { get; }
    }
}