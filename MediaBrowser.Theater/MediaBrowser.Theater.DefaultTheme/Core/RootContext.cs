using System.Threading.Tasks;
using MediaBrowser.Model.ApiClient;
using MediaBrowser.Model.System;
using MediaBrowser.Theater.Api;
using MediaBrowser.Theater.Api.Library;
using MediaBrowser.Theater.Api.Navigation;
using MediaBrowser.Theater.Api.Session;
using MediaBrowser.Theater.Api.System;
using MediaBrowser.Theater.DefaultTheme.Home;
using MediaBrowser.Theater.DefaultTheme.ItemDetails;
using MediaBrowser.Theater.DefaultTheme.ItemList;
using MediaBrowser.Theater.DefaultTheme.Login;
using MediaBrowser.Theater.DefaultTheme.Login.ViewModels;
using MediaBrowser.Theater.DefaultTheme.Osd;
using MediaBrowser.Theater.DefaultTheme.SideMenu;
using MediaBrowser.Theater.DefaultTheme.SideMenu.ViewModels;
using MediaBrowser.Theater.Presentation.Navigation;

namespace MediaBrowser.Theater.DefaultTheme.Core
{
    public class RootContext
        : NavigationContext
    {
        private readonly INavigator _navigator;
        private readonly ISessionManager _sessionManager;

        public RootContext(ITheaterApplicationHost appHost, INavigator navigator, ISessionManager sessionManager, IConnectionManager connectionManager) : base(appHost)
        {
            _navigator = navigator;
            _sessionManager = sessionManager;

            // create root navigation bindings
            Binder.Bind<LoginPath, PageContext<WelcomePageViewModel>>();
            Binder.Bind<ConnectLoginPath, PageContext<ConnectPinViewModel>>();
            Binder.Bind<LocateServerPath, PageContext<ManualConnectionViewModel>>();
            Binder.Bind<HomePath, HomeContext>();
            Binder.Bind<SideMenuPath, SideMenuContext>();
            Binder.Bind<FullScreenPlaybackPath, FullScreenPlaybackContext>();

            Binder.Bind<ServerSelectionPath, ServerSelectionContext>((path, context) => context.Servers = path.Parameter);
            Binder.Bind<UserSelectionPath, UserSelectionContext>((path, context) => context.ApiClient = path.Parameter);
            Binder.Bind<ItemListPath, ItemListContext>((path, context) => context.Parameters = path.Parameter);

            //Binder.Bind<ItemPath, ItemDetailsContext>((path, context) => context.Item = path.Parameter);

            Binder.Bind<ItemPath>(async path => {
                if (path.Parameter.IsFolder && !path.Parameter.IsType("series") && !path.Parameter.IsType("season")) {
                    var context = appHost.TryResolve<ItemListContext>();
                    context.Parameters = new ItemListParameters {
                        Title = path.Parameter.Name,
                        Items = ItemChildren.Get(connectionManager, sessionManager, path.Parameter)
                    };

                    return context;
                } else {
                    var context = appHost.TryResolve<ItemDetailsContext>();
                    context.Item = path.Parameter;

                    return context;
                }
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