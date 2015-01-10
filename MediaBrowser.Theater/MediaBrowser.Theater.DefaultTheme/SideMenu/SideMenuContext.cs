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

        public SideMenuContext(ITheaterApplicationHost appHost, IPresenter presenter, ISessionManager sessionManager, IImageManager imageManager)
            : base(appHost)
        {
            _appHost = appHost;
            _presenter = presenter;
            _sessionManager = sessionManager;
            _imageManager = imageManager;
        }

        public override Task Activate()
        {
            var viewModel = new SideMenuViewModel(_appHost, _sessionManager, _imageManager);
            _presenter.ShowPopup(viewModel);

            return Task.FromResult(0);
        }
    }
}