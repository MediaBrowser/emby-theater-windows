using MediaBrowser.ApiInteraction;
using MediaBrowser.Model.ApiClient;
using MediaBrowser.Model.Logging;
using MediaBrowser.Theater.Interfaces.Configuration;
using MediaBrowser.Theater.Interfaces.Navigation;
using MediaBrowser.Theater.Interfaces.Presentation;
using MediaBrowser.Theater.Interfaces.System;
using MediaBrowser.Theater.Interfaces.Theming;
using MediaBrowser.Theater.Presentation.Pages;
using System;
using System.Globalization;
using System.Net.Http;
using System.Threading;
using System.Windows;

namespace MediaBrowser.UI.StartupWizard
{
    /// <summary>
    /// Interaction logic for StartupWizardPage2.xaml
    /// </summary>
    public partial class StartupWizardPage2 : BasePage
    {
        private readonly IPresentationManager _presentation;
        private readonly INavigationService _nav;
        private readonly ITheaterConfigurationManager _config;
        private readonly IApiClient _apiClient;
        private readonly ILogger _logger;
        private readonly IMediaFilters _mediaFilters;

        private readonly CultureInfo _usCulture = new CultureInfo("en-US");

        public StartupWizardPage2(INavigationService nav, ITheaterConfigurationManager config, IApiClient apiClient, IPresentationManager presentation, ILogger logger, IMediaFilters mediaFilters)
        {
            _nav = nav;
            _config = config;
            _apiClient = apiClient;
            _presentation = presentation;
            _logger = logger;
            _mediaFilters = mediaFilters;
            InitializeComponent();
        }

        protected override async void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);

            try
            {
                var result = await new ServerLocator().FindServer(CancellationToken.None);

                if (result != null)
                {
                    TxtHost.Text = result.Address.ToString();
                    TxtPort.Text = result.Port.ToString(_usCulture);
                }
            }
            catch (Exception ex)
            {
                _logger.ErrorException("Error attempting to locate server.", ex);
            }

            Loaded += StartupWizardPage_Loaded;
            BtnNext.Click += BtnNext_Click;
            BtnBack.Click += BtnBack_Click;
        }

        async void BtnBack_Click(object sender, RoutedEventArgs e)
        {
            await _nav.NavigateBack();
        }

        async void BtnNext_Click(object sender, RoutedEventArgs e)
        {
            if (ValidateInput())
            {
                var port = int.Parse(TxtPort.Text, _usCulture);

                var url = string.Format("http://{0}:{1}/mediabrowser/system/info", TxtHost.Text, port);

                try
                {
                    using (var client = new HttpClient())
                    {
                        var json = await client.GetStringAsync(url);
                    }

                    _apiClient.ServerApiPort = port;
                    _apiClient.ServerHostName = TxtHost.Text;
                    
                    _config.Configuration.ServerApiPort = port;
                    _config.Configuration.ServerHostName = TxtHost.Text;
                    _config.SaveConfiguration();

                    await _nav.Navigate(new StartupWizardLav(_nav, _presentation, _mediaFilters));
                }
                catch (Exception)
                {
                    _presentation.ShowMessage(new MessageBoxInfo
                    {
                        Button = MessageBoxButton.OK,
                        Caption = "Error",
                        Icon = MessageBoxIcon.Error,
                        Text = "Unable to establish a connection with the server. Please check your connection information and try again."
                    });
                }
            }
        }

        void StartupWizardPage_Loaded(object sender, RoutedEventArgs e)
        {
            _presentation.SetDefaultPageTitle();
        }

        private bool ValidateInput()
        {
            int port;

            if (!int.TryParse(TxtPort.Text, NumberStyles.Integer, _usCulture, out port))
            {
                TxtPort.Focus();

                _presentation.ShowMessage(new MessageBoxInfo
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
