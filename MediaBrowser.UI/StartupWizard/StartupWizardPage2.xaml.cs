using MediaBrowser.ApiInteraction;
using MediaBrowser.Model.ApiClient;
using MediaBrowser.Theater.Interfaces.Configuration;
using MediaBrowser.Theater.Interfaces.Navigation;
using MediaBrowser.Theater.Interfaces.Presentation;
using MediaBrowser.Theater.Interfaces.Theming;
using MediaBrowser.Theater.Presentation.Pages;
using System;
using System.Globalization;
using System.Threading;
using System.Windows;

namespace MediaBrowser.UI.StartupWizard
{
    /// <summary>
    /// Interaction logic for StartupWizardPage2.xaml
    /// </summary>
    public partial class StartupWizardPage2 : BasePage
    {
        private readonly IThemeManager _themeManager;
        private readonly INavigationService _nav;
        private readonly ITheaterConfigurationManager _config;
        private readonly IApiClient _apiClient;

        private readonly CultureInfo _usCulture = new CultureInfo("en-US");

        public StartupWizardPage2(IThemeManager themeManager, INavigationService nav, ITheaterConfigurationManager config, IApiClient apiClient)
        {
            _themeManager = themeManager;
            _nav = nav;
            _config = config;
            _apiClient = apiClient;
            InitializeComponent();
        }

        protected override async void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);

            var result = await new ServerLocator().FindServer(CancellationToken.None);

            if (result != null)
            {
                TxtHost.Text = result.Address.ToString();
                TxtPort.Text = result.Port.ToString(_usCulture);
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

                _apiClient.ServerApiPort = port;
                _apiClient.ServerHostName = TxtHost.Text;

                try
                {
                    await _apiClient.GetSystemInfoAsync();

                    _config.Configuration.ServerApiPort = port;
                    _config.Configuration.ServerHostName = TxtHost.Text;
                    _config.SaveConfiguration();

                    await _nav.Navigate(new StartupWizardFinish(_themeManager, _nav));
                }
                catch (Exception)
                {
                    _themeManager.CurrentTheme.ShowMessage(new MessageBoxInfo
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
            _themeManager.CurrentTheme.SetDefaultPageTitle();
        }

        private bool ValidateInput()
        {
            int port;

            if (!int.TryParse(TxtPort.Text, NumberStyles.Integer, _usCulture, out port))
            {
                TxtPort.Focus();

                _themeManager.CurrentTheme.ShowMessage(new MessageBoxInfo
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
