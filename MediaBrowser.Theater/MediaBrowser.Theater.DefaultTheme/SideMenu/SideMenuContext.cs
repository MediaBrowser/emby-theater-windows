using System.Threading.Tasks;
using MediaBrowser.Model.ApiClient;
using MediaBrowser.Theater.Api;
using MediaBrowser.Theater.Api.Navigation;
using MediaBrowser.Theater.Api.Session;
using MediaBrowser.Theater.Api.UserInterface;
using MediaBrowser.Theater.DefaultTheme.SideMenu.ViewModels;

namespace MediaBrowser.Theater.DefaultTheme.SideMenu
{
    public class SideMenuContext
        : NavigationContext
    {
        private readonly ITheaterApplicationHost _appHost;
        private readonly IPresenter _presenter;
        private readonly ISessionManager _sessionManager;
        private readonly IImageManager _imageManager;
        private readonly IApiClient _apiClient;

        public SideMenuContext(ITheaterApplicationHost appHost, IPresenter presenter, ISessionManager sessionManager, IImageManager imageManager, IApiClient apiClient)
            : base(appHost)
        {
            _appHost = appHost;
            _presenter = presenter;
            _sessionManager = sessionManager;
            _imageManager = imageManager;
            _apiClient = apiClient;
        }

        public override async Task Activate()
        {
            var viewModel = new SideMenuViewModel(_sessionManager, _imageManager, _apiClient);
            await _presenter.ShowPopup(viewModel);
        }
    }
}