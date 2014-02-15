using System.Threading.Tasks;

namespace MediaBrowser.Theater.Api.UserInterface.Navigation
{
    public interface INavigationContext
    {
        Task Activate();
        PathResolution HandleNavigation(INavigationPath path);
    }
}