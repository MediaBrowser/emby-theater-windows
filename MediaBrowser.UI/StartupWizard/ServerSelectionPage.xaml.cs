using MediaBrowser.Model.ApiClient;
using MediaBrowser.Theater.Interfaces;
using MediaBrowser.Theater.Interfaces.Presentation;
using MediaBrowser.Theater.Interfaces.Theming;
using MediaBrowser.Theater.Presentation.Pages;
using MediaBrowser.Theater.Presentation.ViewModels;
using System;
using System.Collections.Generic;
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

        private readonly List<ServerInfo> _servers;
        
        public ServerSelectionPage(IConnectionManager connectionManager, 
            IPresentationManager presentationManager, 
            List<ServerInfo> servers)
        {
            PresentationManager = presentationManager;
            ConnectionManager = connectionManager;
            _servers = servers;

            InitializeComponent();
        }

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);

            DataContext = new ServerListViewModel(_servers);

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

            try
            {
                var result = await ConnectionManager.Connect(server, CancellationToken.None);

                App.Instance.NavigateFromConnectionResult(result);
            }
            catch (Exception ex)
            {
                PresentationManager.ShowMessage(new MessageBoxInfo
                {
                    Caption = "Login Failure",
                    Text = ex.Message,
                    Icon = MessageBoxIcon.Error
                });
            }
        }
    }
}
