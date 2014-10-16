using MediaBrowser.Model.ApiClient;
using MediaBrowser.Model.Logging;
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
    /// Interaction logic for StartupWizardPage3.xaml
    /// </summary>
    public partial class StartupWizardPage3 : BasePage
    {
        private readonly IPresentationManager _presentation;
        private readonly INavigationService _nav;
        private readonly IConnectionManager _connectionManager;
        private readonly ILogger _logger;

        private readonly CultureInfo _usCulture = new CultureInfo("en-US");

        public StartupWizardPage3(INavigationService nav, IConnectionManager connectionManager, IPresentationManager presentation, ILogger logger)
        {
            _nav = nav;
            _connectionManager = connectionManager;
            _presentation = presentation;
            _logger = logger;
            InitializeComponent();
        }

        protected override async void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);

            TxtHost.Text = string.Empty;
            TxtPort.Text = string.Empty;

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
                var serverAddress = string.Format("http://{0}", TxtHost.Text);
                if (!string.IsNullOrEmpty(TxtPort.Text))
                {
                    serverAddress += ":" + TxtPort.Text;
                }

                try
                {
                    var connectionResult = await _connectionManager.Connect(serverAddress, CancellationToken.None);

                    if (connectionResult.State == ConnectionState.Unavailable)
                    {
                        ShowUnavailableMessage();
                        return;
                    }

                    await _nav.Navigate(new StartupWizardFinish(_nav, _presentation));
                }
                catch (Exception)
                {
                    ShowUnavailableMessage();
                }
            }
        }

        private void ShowUnavailableMessage()
        {
            _presentation.ShowMessage(new MessageBoxInfo
            {
                Button = MessageBoxButton.OK,
                Caption = "Error",
                Icon = MessageBoxIcon.Error,
                Text = "Unable to establish a connection with the server. Please check your connection information and try again."
            });
        }

        void StartupWizardPage_Loaded(object sender, RoutedEventArgs e)
        {
            _presentation.SetDefaultPageTitle();
        }

        private bool ValidateInput()
        {
            int port;

            if (string.IsNullOrEmpty(TxtHost.Text))
            {
                TxtHost.Focus();

                _presentation.ShowMessage(new MessageBoxInfo
                {
                    Button = MessageBoxButton.OK,
                    Caption = "Error",
                    Icon = MessageBoxIcon.Error,
                    Text = "Please enter a valid server address."
                });

                return false;
            }
            
            if (!string.IsNullOrEmpty(TxtPort.Text) && !int.TryParse(TxtPort.Text, NumberStyles.Integer, _usCulture, out port))
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
