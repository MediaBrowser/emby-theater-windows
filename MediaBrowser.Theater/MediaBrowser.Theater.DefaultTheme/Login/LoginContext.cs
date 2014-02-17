using System.Threading.Tasks;
using MediaBrowser.Common;
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
        private readonly IPresenter _presenter;
        private readonly ISessionManager _sessionManager;
        private readonly ILogManager _logManager;

        public LoginContext(IApplicationHost appHost, IPresenter presenter, ISessionManager sessionManager, ILogManager logManager) : base(appHost)
        {
            _presenter = presenter;
            _sessionManager = sessionManager;
            _logManager = logManager;
        }

        public override async Task Activate()
        {
            var loginViewModel = new LoginViewModel(_sessionManager, _logManager);
            await _presenter.ShowPage(loginViewModel);
        }
    }
}
