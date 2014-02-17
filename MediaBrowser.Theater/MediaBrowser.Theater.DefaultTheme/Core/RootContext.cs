using System;
using System.Threading.Tasks;
using MediaBrowser.Model.Logging;
using MediaBrowser.Theater.Api;
using MediaBrowser.Theater.Api.Configuration;
using MediaBrowser.Theater.Api.Navigation;
using MediaBrowser.Theater.Api.Session;
using MediaBrowser.Theater.DefaultTheme.Home;
using MediaBrowser.Theater.DefaultTheme.Login;

namespace MediaBrowser.Theater.DefaultTheme.Core
{
    public class RootContext
        : NavigationContext
    {
        private readonly ITheaterApplicationHost _appHost;
        private readonly ILogger _logger;
        private readonly ISessionManager _sessionManager;
        private readonly INavigator _navigator;

        public RootContext(ITheaterApplicationHost appHost, INavigator navigator, ISessionManager sessionManager, ILogManager logManager) : base(appHost)
        {
            _appHost = appHost;
            _navigator = navigator;
            _sessionManager = sessionManager;
            _logger = logManager.GetLogger("RootContext");

            // create root navigation bindings
            Binder.Bind<LoginPath, LoginContext>();
            Binder.Bind<HomePath, HomeContext>();
        }

        public override Task Activate()
        {
            AttemptLogin();
            return Task.FromResult(0);
        }

        private async void AttemptLogin()
        {
            //Check for auto-login credientials
            ApplicationConfiguration config = _appHost.TheaterConfigurationManager.Configuration;
            try {
                if (config.AutoLoginConfiguration.UserName != null && config.AutoLoginConfiguration.UserPasswordHash != null) {
                    //Attempt password login
                    await _sessionManager.LoginWithHash(config.AutoLoginConfiguration.UserName, config.AutoLoginConfiguration.UserPasswordHash, true);
                    return;
                }
                if (config.AutoLoginConfiguration.UserName != null) {
                    //Attempt passwordless login
                    await _sessionManager.Login(config.AutoLoginConfiguration.UserName, string.Empty, true);
                    return;
                }
            }
            catch (UnauthorizedAccessException ex) {
                //Login failed, redirect to login page and clear the auto-login
                _logger.ErrorException("Auto-login failed", ex, config.AutoLoginConfiguration.UserName);

                config.AutoLoginConfiguration = new AutoLoginConfiguration();
                _appHost.TheaterConfigurationManager.SaveConfiguration();
            }
            catch (FormatException ex) {
                //Login failed, redirect to login page and clear the auto-login
                _logger.ErrorException("Auto-login password hash corrupt", ex);

                config.AutoLoginConfiguration = new AutoLoginConfiguration();
                _appHost.TheaterConfigurationManager.SaveConfiguration();
            }

            await _navigator.Navigate(Go.To.Login());
        }
    }
}