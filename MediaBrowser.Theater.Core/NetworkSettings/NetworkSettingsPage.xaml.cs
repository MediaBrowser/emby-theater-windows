using MediaBrowser.Model.ApiClient;
using MediaBrowser.Theater.Interfaces.Configuration;
using MediaBrowser.Theater.Interfaces.Presentation;
using MediaBrowser.Theater.Interfaces.Session;
using MediaBrowser.Theater.Interfaces.Theming;
using MediaBrowser.Theater.Presentation.Pages;
using System;
using System.Globalization;
using System.Threading;
using System.Windows;

namespace MediaBrowser.Theater.Core.NetworkSettings
{
    /// <summary>
    /// Interaction logic for NetworkSettingsPage.xaml
    /// </summary>
    public partial class NetworkSettingsPage : BasePage
    {
        private readonly ITheaterConfigurationManager _config;
        private readonly IConnectionManager _connectionManager;
        private readonly IPresentationManager _presentationManager;
        private readonly ISessionManager _session;

        private readonly CultureInfo _usCulture = new CultureInfo("en-US");

        public NetworkSettingsPage(ITheaterConfigurationManager config, ISessionManager session, IPresentationManager presentationManager, IConnectionManager connectionManager)
        {
            _config = config;
            _session = session;
            _presentationManager = presentationManager;
            _connectionManager = connectionManager;
            InitializeComponent();
        }

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);

            BtnApply.Click += BtnApply_Click;

            Loaded += NetworkSettingsPage_Loaded;
        }

        void NetworkSettingsPage_Loaded(object sender, RoutedEventArgs e)
        {
            TxtHost.Text = string.Empty;
            TxtPort.Text = string.Empty;

            var apiClient = _session.ActiveApiClient;

            if (!string.IsNullOrEmpty(apiClient.ServerAddress))
            {
                var uri = new Uri(apiClient.ServerAddress);

                TxtHost.Text = uri.Host;

                if (!uri.IsDefaultPort)
                {
                    TxtPort.Text = uri.Port.ToString(CultureInfo.InvariantCulture);
                }
            }
        }

        async void BtnApply_Click(object sender, RoutedEventArgs e)
        {
            if (ValidateInput())
            {
                var serverAddress = string.Format("http://{0}", TxtHost.Text);
                if (!string.IsNullOrEmpty(TxtPort.Text))
                {
                    serverAddress += ":" + TxtPort.Text;
                }

                _presentationManager.ShowModalLoadingAnimation();

                try
                {
                    var connectionResult = await _connectionManager.Connect(serverAddress, CancellationToken.None);

                    if (connectionResult.State == ConnectionState.Unavailable)
                    {
                        ShowUnavailableMessage();
                        return;
                    }

                    _presentationManager.HideModalLoadingAnimation();

                    _presentationManager.ShowMessage(new MessageBoxInfo
                    {
                        Button = MessageBoxButton.OK,
                        Caption = "Connection Confirmed",
                        Icon = MessageBoxIcon.Information,
                        Text = "The new server location has been confirmed. Press OK to login."
                    });

                    await _session.Logout();
                }
                catch (Exception)
                {
                    _presentationManager.HideModalLoadingAnimation();

                    ShowUnavailableMessage();
                }
            }
        }

        private void ShowUnavailableMessage()
        {
            _presentationManager.ShowMessage(new MessageBoxInfo
            {
                Button = MessageBoxButton.OK,
                Caption = "Error",
                Icon = MessageBoxIcon.Error,
                Text = "Unable to establish a connection with the server. Please check your connection information and try again."
            });
        }

        private bool ValidateInput()
        {
            int port;

            if (!string.IsNullOrEmpty(TxtPort.Text) && !int.TryParse(TxtPort.Text, NumberStyles.Integer, _usCulture, out port))
            {
                TxtPort.Focus();

                _presentationManager.ShowMessage(new MessageBoxInfo
                {
                    Button = MessageBoxButton.OK,
                    Caption = "Error",
                    Icon = MessageBoxIcon.Error,
                    Text = "Please enter a valid port number."
                });

                return false;
            }

            return true;
        }
    }
}
