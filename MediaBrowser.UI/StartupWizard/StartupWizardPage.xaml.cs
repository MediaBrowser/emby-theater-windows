using MediaBrowser.Model.ApiClient;
using MediaBrowser.Theater.Interfaces.Configuration;
using MediaBrowser.Theater.Interfaces.Navigation;
using MediaBrowser.Theater.Interfaces.Theming;
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
        private readonly IThemeManager _themeManager;
        private readonly INavigationService _nav;
        private readonly ITheaterConfigurationManager _config;
        private readonly IApiClient _apiClient;

        public StartupWizardPage(IThemeManager themeManager, INavigationService nav, ITheaterConfigurationManager config, IApiClient apiClient)
        {
            _themeManager = themeManager;
            _nav = nav;
            _config = config;
            _apiClient = apiClient;
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
            await _nav.Navigate(new StartupWizardPage2(_themeManager, _nav, _config, _apiClient));
        }

        void StartupWizardPage_Loaded(object sender, RoutedEventArgs e)
        {
            _themeManager.CurrentTheme.SetDefaultPageTitle();
        }
    }
}
