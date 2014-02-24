using System.Windows.Input;
using MediaBrowser.Theater.Api.Navigation;

namespace MediaBrowser.Theater.Api.UserInterface
{
    public interface IGlobalCommand
    {
        IViewModel IconViewModel { get; }
        string DisplayName { get; }
        ICommand ExecuteCommand { get; }
        bool EvaluateVisibility(INavigationPath currentPath);
    }
}