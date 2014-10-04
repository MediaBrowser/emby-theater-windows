using MediaBrowser.Model.ApiClient;
using MediaBrowser.Model.Logging;
using MediaBrowser.Theater.Interfaces.Configuration;
using MediaBrowser.Theater.Interfaces.Navigation;
using MediaBrowser.Theater.Interfaces.Presentation;
using MediaBrowser.Theater.Interfaces.System;
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
        private readonly IConnectionManager _connectionManager;
        private readonly ILogger _logger;
        private readonly IMediaFilters _mediaFilters;

        public StartupWizardPage(INavigationService nav, ITheaterConfigurationManager config, IConnectionManager connectionManager, IPresentationManager presentation, ILogger logger, IMediaFilters mediaFilters)
        {
            _nav = nav;
            _config = config;
            _connectionManager = connectionManager;
            _presentation = presentation;
            _logger = logger;
            _mediaFilters = mediaFilters;
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
            await _nav.Navigate(new StartupWizardPage2(_nav, _config, _connectionManager, _presentation, _logger, _mediaFilters));
        }

        void StartupWizardPage_Loaded(object sender, RoutedEventArgs e)
        {
            _presentation.SetDefaultPageTitle();
        }
    }
}
