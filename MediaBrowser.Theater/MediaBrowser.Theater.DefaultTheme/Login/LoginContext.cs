using System.Threading.Tasks;
using MediaBrowser.Common;
using MediaBrowser.Model.ApiClient;
using MediaBrowser.Model.Logging;
using MediaBrowser.Theater.Api.Navigation;
using MediaBrowser.Theater.Api.Session;
using MediaBrowser.Theater.Api.UserInterface;
using MediaBrowser.Theater.DefaultTheme.Login.ViewModels;

namespace MediaBrowser.Theater.DefaultTheme.Login
{
    public class LoginContext
        : NavigationContext
    {
        private readonly ILogManager _logManager;
        private readonly IImageManager _imageManager;
        private readonly IPresenter _presenter;
        private readonly ISessionManager _sessionManager;

        private LoginViewModel _loginViewModel;

        public LoginContext(IApplicationHost appHost, IPresenter presenter, ISessionManager sessionManager,
                            ILogManager logManager, IImageManager imageManager) : base(appHost)
        {
            _presenter = presenter;
            _sessionManager = sessionManager;
            _logManager = logManager;
            _imageManager = imageManager;
        }

        public override async Task Activate()
        {
            if (_loginViewModel == null || !_loginViewModel.IsActive) {
                _loginViewModel = new LoginViewModel(_sessionManager, _logManager, _imageManager);
            }

            await _presenter.ShowPage(_loginViewModel);
        }
    }
}