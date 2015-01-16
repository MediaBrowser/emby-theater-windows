using System;
using System.Threading;
using System.Threading.Tasks;
using MediaBrowser.Model.ApiClient;
using MediaBrowser.Model.System;
using MediaBrowser.Theater.Api;
using MediaBrowser.Theater.Api.Events;
using MediaBrowser.Theater.Api.Library;
using MediaBrowser.Theater.Api.Navigation;
using MediaBrowser.Theater.Api.Playback;
using MediaBrowser.Theater.Api.Session;
using MediaBrowser.Theater.Api.System;
using MediaBrowser.Theater.Api.UserInterface;
using MediaBrowser.Theater.DefaultTheme.Core.ViewModels;
using MediaBrowser.Theater.DefaultTheme.Home;
using MediaBrowser.Theater.DefaultTheme.ItemDetails;
using MediaBrowser.Theater.DefaultTheme.ItemList;
using MediaBrowser.Theater.DefaultTheme.Login;
using MediaBrowser.Theater.DefaultTheme.Login.ViewModels;
using MediaBrowser.Theater.DefaultTheme.Osd;
using MediaBrowser.Theater.DefaultTheme.Osd.ViewModels;
using MediaBrowser.Theater.DefaultTheme.SideMenu;
using MediaBrowser.Theater.DefaultTheme.SideMenu.ViewModels;
using MediaBrowser.Theater.Presentation.Navigation;
using MediaBrowser.Theater.Presentation.ViewModels;

namespace MediaBrowser.Theater.DefaultTheme.Core
{
    public class RootContext
        : NavigationContext
    {
        private readonly ITheaterApplicationHost _appHost;
        private readonly IConnectionManager _connectionManager;

        public RootContext(ITheaterApplicationHost appHost, ISessionManager sessionManager, IConnectionManager connectionManager, IPresenter presenter, IEventAggregator events) : base(appHost)
        {
            _appHost = appHost;
            _connectionManager = connectionManager;

            // create root navigation bindings
            Binder.Bind<LoginPath, PageContext<WelcomePageViewModel>>();
            Binder.Bind<ConnectLoginPath, PageContext<ConnectPinViewModel>>();
            Binder.Bind<LocateServerPath, PageContext<ManualConnectionViewModel>>();
            Binder.Bind<HomePath, HomeContext>();
            Binder.Bind<SideMenuPath, SideMenuContext>();
            Binder.Bind<FullScreenPlaybackPath, FullScreenPlaybackContext>();
            
            Binder.Bind<UserSelectionPath, UserSelectionContext>((path, context) => context.ApiClient = path.Parameter);
            Binder.Bind<ItemListPath, ItemListContext>((path, context) => context.Parameters = path.Parameter);
            Binder.Bind<ServerSelectionPath, ServerSelectionContext>((path, context) =>
            {
                context.Servers = path.Parameter != null ? path.Parameter.Servers : null;
                context.IsConnectUser = path.Parameter != null && path.Parameter.IsConnectUser;
            });

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

            events.Get<PlaybackStartEventArgs>().Subscribe(e => {
                var notification = appHost.TryResolve<NowPlayingNotificationViewModel>();
                notification.ActivePlayer = e.Player;
                presenter.ShowNotification(notification);
//                presenter.ShowNotification(new ItemArtworkViewModel(e.Player.CurrentMedia, connectionManager, appHost.TryResolve<IImageManager>()));
            });
        }

        public override async Task Activate()
        {
            var result = await _connectionManager.Connect(CancellationToken.None);
            await _appHost.HandleConnectionStatus(result);
        }
    }
}