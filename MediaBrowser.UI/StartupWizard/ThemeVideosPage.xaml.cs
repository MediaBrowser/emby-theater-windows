using MediaBrowser.Model.ApiClient;
using MediaBrowser.Theater.Interfaces.Navigation;
using MediaBrowser.Theater.Interfaces.Presentation;
using MediaBrowser.Theater.Presentation.Pages;
using System.Diagnostics;
using System.Windows;

namespace MediaBrowser.UI.StartupWizard
{
    /// <summary>
    /// Interaction logic for ThemeVideosPage.xaml
    /// </summary>
    public partial class ThemeVideosPage : BasePage
    {
        private readonly IPresentationManager _presentation;
        private readonly INavigationService _nav;
        private readonly IApiClient _apiClient;

        public ThemeVideosPage(IPresentationManager presentation, INavigationService nav, IApiClient apiClient)
        {
            _presentation = presentation;
            _nav = nav;
            _apiClient = apiClient;
            InitializeComponent();

            BtnNext.Click += BtnNext_Click;
            BtnInstall.Click += BtnInstall_Click;
        }

        async void BtnNext_Click(object sender, RoutedEventArgs e)
        {
            await _nav.Navigate(new StartupWizardFinish(_nav, _presentation));
        }

        void BtnInstall_Click(object sender, RoutedEventArgs e)
        {
            var url = _apiClient.GetApiUrl("dashboard/addPlugin.html?name=TV%20Theme%20Videos&guid=341E0063-2167-4602-803C-D761F6836851");

            Process.Start(url);
        }
    }
}
