using MediaBrowser.Model.ApiClient;
using MediaBrowser.Model.Logging;
using MediaBrowser.Theater.Interfaces.Configuration;
using MediaBrowser.Theater.Interfaces.Navigation;
using MediaBrowser.Theater.Interfaces.Presentation;
using MediaBrowser.Theater.Presentation.Pages;
using System;
using System.Windows;

namespace MediaBrowser.UI.StartupWizard
{
    /// <summary>
    /// Interaction logic for StartupWizardPage.xaml
    /// </summary>
    public partial class StartupWizardPage : BasePage
    {
        private readonly IPresentationManager _presentation;
        private readonly INavigationService _nav;
        private readonly ITheaterConfigurationManager _config;
        private readonly IApiClient _apiClient;
        private readonly ILogger _logger;

        public StartupWizardPage(INavigationService nav, ITheaterConfigurationManager config, IApiClient apiClient, IPresentationManager presentation, ILogger logger)
        {
            _nav = nav;
            _config = config;
            _apiClient = apiClient;
            _presentation = presentation;
            _logger = logger;
            InitializeComponent();
        }

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);

            Loaded += StartupWizardPage_Loaded;
            BtnNext.Click += BtnNext_Click;
        }

        async void BtnNext_Click(object sender, RoutedEventArgs e)
        {
            await _nav.Navigate(new StartupWizardPage2(_nav, _config, _apiClient, _presentation, _logger));
        }

        void StartupWizardPage_Loaded(object sender, RoutedEventArgs e)
        {
            _presentation.SetDefaultPageTitle();
        }
    }
}
