using System.Threading.Tasks;

namespace MediaBrowser.Theater.Api.Navigation
{
    public interface INavigationContext
    {
        Task Activate();
        PathResolution HandleNavigation(INavigationPath path);
    }
}