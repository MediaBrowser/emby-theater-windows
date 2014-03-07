using System.Threading.Tasks;
using MediaBrowser.Model.ApiClient;
using MediaBrowser.Theater.Api.Session;
using MediaBrowser.Theater.Api.UserInterface;
using MediaBrowser.Theater.Presentation.ViewModels;

namespace MediaBrowser.Theater.DefaultTheme.SideMenu.ViewModels
{
    public class SideMenuViewModel
        : BaseViewModel
    {
        public SideMenuUsersViewModel Users { get; private set; }

        public SideMenuViewModel(ISessionManager sessionManager, IImageManager imageManager, IApiClient apiClient)
        {
            Users = new SideMenuUsersViewModel(sessionManager, imageManager, apiClient);
        }
    }
}