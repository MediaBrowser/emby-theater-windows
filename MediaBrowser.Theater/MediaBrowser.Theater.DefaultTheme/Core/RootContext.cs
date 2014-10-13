using System.Threading.Tasks;
using MediaBrowser.Model.System;
using MediaBrowser.Theater.Api;
using MediaBrowser.Theater.Api.Navigation;
using MediaBrowser.Theater.Api.Session;
using MediaBrowser.Theater.Api.System;
using MediaBrowser.Theater.DefaultTheme.Home;
using MediaBrowser.Theater.DefaultTheme.ItemDetails;
using MediaBrowser.Theater.DefaultTheme.ItemList;
using MediaBrowser.Theater.DefaultTheme.Login;
using MediaBrowser.Theater.DefaultTheme.Osd;
using MediaBrowser.Theater.DefaultTheme.SideMenu;
using MediaBrowser.Theater.DefaultTheme.SideMenu.ViewModels;

namespace MediaBrowser.Theater.DefaultTheme.Core
{
    public class RootContext
        : NavigationContext
    {
        private readonly INavigator _navigator;
        private readonly ISessionManager _sessionManager;

        public RootContext(ITheaterApplicationHost appHost, INavigator navigator, ISessionManager sessionManager) : base(appHost)
        {
            _navigator = navigator;
            _sessionManager = sessionManager;

            // create root navigation bindings
            Binder.Bind<LoginPath, LoginContext>();
            Binder.Bind<HomePath, HomeContext>();
            Binder.Bind<SideMenuPath, SideMenuContext>();
            Binder.Bind<FullScreenPlaybackPath, FullScreenPlaybackContext>();

            Binder.Bind<ItemListPath>(async path => {
                var context = appHost.CreateInstance(typeof (ItemListContext)) as ItemListContext;
                context.Parameters = path.Parameter;

                return context;
            });

            Binder.Bind<ItemPath>(async path => {
                var context = appHost.CreateInstance(typeof (ItemDetailsContext)) as ItemDetailsContext;
                context.Item = path.Parameter;

                return context;
            });
        }

        public override Task Activate()
        {
            //AttemptLogin();
            if (_sessionManager.CurrentUser == null) {
                _navigator.Navigate(Go.To.Login());
            } else {
                _navigator.Navigate(Go.To.Home());
            }

            return Task.FromResult(0);
        }
//
//        private async void AttemptLogin()
//        {
//            PublicSystemInfo serverInfo = await _serverConnectionManager.AttemptServerConnection();
//            if (serverInfo != null) {
//                bool autoLoggedIn = await _serverConnectionManager.AttemptAutoLogin(serverInfo);
//                if (!autoLoggedIn && !(_navigator.CurrentLocation is LoginPath)) {
//                    await _navigator.Navigate(Go.To.Login());
//                }
//            }
//        }
    }
}