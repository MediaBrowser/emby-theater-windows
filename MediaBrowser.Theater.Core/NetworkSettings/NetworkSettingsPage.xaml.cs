using MediaBrowser.Model.ApiClient;
using MediaBrowser.Theater.Interfaces.Configuration;
using MediaBrowser.Theater.Interfaces.Presentation;
using MediaBrowser.Theater.Interfaces.Session;
using MediaBrowser.Theater.Interfaces.Theming;
using MediaBrowser.Theater.Presentation.Pages;
using System;
using System.Globalization;
using System.Net.Http;
using System.Windows;

namespace MediaBrowser.Theater.Core.NetworkSettings
{
    /// <summary>
    /// Interaction logic for NetworkSettingsPage.xaml
    /// </summary>
    public partial class NetworkSettingsPage : BasePage
    {
        private readonly ITheaterConfigurationManager _config;
        private readonly IApiClient _apiClient;
        private readonly IPresentationManager _presentationManager;
        private readonly ISessionManager _session;

        private readonly CultureInfo _usCulture = new CultureInfo("en-US");
        
        public NetworkSettingsPage(ITheaterConfigurationManager config, IApiClient apiClient, ISessionManager session, IPresentationManager presentationManager)
        {
            _config = config;
            _apiClient = apiClient;
            _session = session;
            _presentationManager = presentationManager;
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
            TxtHost.Text = _config.Configuration.ServerHostName;
            TxtPort.Text = _config.Configuration.ServerApiPort.ToString(_usCulture);
        }

        async void BtnApply_Click(object sender, RoutedEventArgs e)
        {
            if (ValidateInput())
            {
                var currentLocation = _config.Configuration.ServerHostName + ":" +
                                      _config.Configuration.ServerApiPort.ToString(_usCulture);

                var newLocation = TxtHost.Text + ":" + TxtPort.Text;

                if (string.Equals(currentLocation, newLocation, StringComparison.OrdinalIgnoreCase))
                {
                    // Nothing to do
                    return;
                }

                var port = int.Parse(TxtPort.Text, _usCulture);

                var url = string.Format("http://{0}:{1}/mediabrowser/system/info", TxtHost.Text, port);

                try
                {
                    using (var client = new HttpClient())
                    {
                        var json = await client.GetStringAsync(url);
                    }

                    _apiClient.ChangeServerLocation(TxtHost.Text, port);

                    _config.Configuration.ServerApiPort = port;
                    _config.Configuration.ServerHostName = TxtHost.Text;
                    _config.SaveConfiguration();

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
                    _presentationManager.ShowMessage(new MessageBoxInfo
                    {
                        Button = MessageBoxButton.OK,
                        Caption = "Error",
                        Icon = MessageBoxIcon.Error,
                        Text = "Unable to establish a connection with the server. Please check your connection information and try again."
                    });
                }
            }
        }

        private bool ValidateInput()
        {
            int port;

            if (!int.TryParse(TxtPort.Text, NumberStyles.Integer, _usCulture, out port))
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
