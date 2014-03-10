using MediaBrowser.Theater.Api.Navigation;
using MediaBrowser.Theater.Api.UserInterface;

namespace MediaBrowser.Theater.Api.Commands
{
    public interface IGlobalMenuCommand : IMenuCommand
    {
        bool EvaluateVisibility(INavigationPath currentPath);
    }
}