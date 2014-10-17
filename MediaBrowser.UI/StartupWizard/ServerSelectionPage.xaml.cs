using MediaBrowser.Model.ApiClient;
using MediaBrowser.Model.Logging;
using MediaBrowser.Theater.Interfaces;
using MediaBrowser.Theater.Interfaces.Navigation;
using MediaBrowser.Theater.Interfaces.Presentation;
using MediaBrowser.Theater.Presentation.Pages;
using MediaBrowser.Theater.Presentation.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;

namespace MediaBrowser.UI.StartupWizard
{
    /// <summary>
    /// Interaction logic for ServerSelectionPage.xaml
    /// </summary>
    public partial class ServerSelectionPage : BasePage
    {
        protected IConnectionManager ConnectionManager { get; private set; }
        protected IPresentationManager PresentationManager { get; private set; }
        protected INavigationService NavigationManager { get; private set; }
        protected ILogger Logger { get; private set; }

        private List<ServerInfo> _servers;

        public ServerSelectionPage(IConnectionManager connectionManager,
            IPresentationManager presentationManager,
            List<ServerInfo> servers, INavigationService navigationManager, ILogger logger)
        {
            PresentationManager = presentationManager;
            ConnectionManager = connectionManager;
            _servers = servers;
            Logger = logger;
            NavigationManager = navigationManager;

            InitializeComponent();
        }

        public ServerSelectionPage(IConnectionManager connectionManager,
         IPresentationManager presentationManager, INavigationService navigationManager, ILogger logger)
        {
            PresentationManager = presentationManager;
            ConnectionManager = connectionManager;
            Logger = logger;
            NavigationManager = navigationManager;

            InitializeComponent();
        }

        protected override async void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);

            if (_servers == null)
            {
                try
                {
                    _servers = await ConnectionManager.GetAvailableServers(CancellationToken.None);
                }
                catch
                {
                    _servers = new List<ServerInfo>();
                    PresentationManager.ShowDefaultErrorMessage();
                }
            }

            var list = _servers.ToList();
            list.Add(new ServerInfo
            {
                Name = "New Server"
            });

            DataContext = new ServerListViewModel(list);

            ListBox.ItemInvoked += ItemsList_ItemInvoked;
            Loaded += LoginPage_Loaded;
        }

        void LoginPage_Loaded(object sender, RoutedEventArgs e)
        {
            PresentationManager.SetDefaultPageTitle();
        }

        /// <summary>
        /// Logs in a user when one is selected
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        async void ItemsList_ItemInvoked(object sender, ItemEventArgs<object> e)
        {
            var model = (ServerInfoViewModel)e.Argument;
            var server = model.Server;

            if (string.IsNullOrWhiteSpace(server.Id))
            {
                await Dispatcher.InvokeAsync(async () => await NavigationManager.Navigate(new StartupPageServerEntry(NavigationManager, ConnectionManager, PresentationManager, Logger)));
                return;
            }

            try
            {
                var result = await ConnectionManager.Connect(server, CancellationToken.None);

                if (result.State == ConnectionState.Unavailable)
                {
                    PresentationManager.ShowDefaultErrorMessage();
                }
                else
                {
                    App.Instance.NavigateFromConnectionResult(result);
                }
            }
            catch
            {
                PresentationManager.ShowDefaultErrorMessage();
            }
        }
    }
}
