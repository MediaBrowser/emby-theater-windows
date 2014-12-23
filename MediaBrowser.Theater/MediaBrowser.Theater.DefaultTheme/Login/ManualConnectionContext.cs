using System.Threading.Tasks;
using MediaBrowser.Common;
using MediaBrowser.Theater.Api.Navigation;
using MediaBrowser.Theater.Api.UserInterface;
using MediaBrowser.Theater.DefaultTheme.Login.ViewModels;

namespace MediaBrowser.Theater.DefaultTheme.Login
{
    public class ManualConnectionContext
        : NavigationContext
    {
        private readonly IApplicationHost _appHost;
        private readonly IPresenter _presenter;
        private ManualConnectionViewModel _viewModel;

        public ManualConnectionContext(IApplicationHost appHost, IPresenter presenter) : base(appHost)
        {
            _appHost = appHost;
            _presenter = presenter;
        }

        public override async Task Activate()
        {
            if (_viewModel == null || !_viewModel.IsActive) {
                _viewModel = _appHost.TryResolve<ManualConnectionViewModel>();
            }

            await _presenter.ShowPage(_viewModel);
        }
    }
}
