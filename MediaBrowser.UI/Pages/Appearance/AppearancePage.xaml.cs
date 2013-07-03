using System.Threading.Tasks;
using MediaBrowser.Model.ApiClient;
using MediaBrowser.Model.Dto;
using MediaBrowser.Model.Net;
using MediaBrowser.Theater.Interfaces.Configuration;
using MediaBrowser.Theater.Interfaces.Navigation;
using MediaBrowser.Theater.Interfaces.Presentation;
using MediaBrowser.Theater.Interfaces.Session;
using MediaBrowser.Theater.Interfaces.Theming;
using MediaBrowser.Theater.Presentation.Controls;
using MediaBrowser.Theater.Presentation.Pages;
using System;
using System.Linq;
using System.Windows;

namespace MediaBrowser.UI.Pages.Appearance
{
    /// <summary>
    /// Interaction logic for AppearancePage.xaml
    /// </summary>
    public partial class AppearancePage : BasePage
    {
        private readonly ITheaterConfigurationManager _config;
        private readonly ISessionManager _session;
        private readonly IImageManager _imageManager;
        private readonly IApiClient _apiClient;
        private readonly IPresentationManager _presentation;
        private readonly IThemeManager _themeManager;
        private readonly INavigationService _nav;

        public AppearancePage(ITheaterConfigurationManager config, ISessionManager session, IImageManager imageManager, IApiClient apiClient, IPresentationManager presentation, IThemeManager themeManager, INavigationService nav)
        {
            _config = config;
            _session = session;
            _imageManager = imageManager;
            _apiClient = apiClient;
            _presentation = presentation;
            _themeManager = themeManager;
            _nav = nav;
            InitializeComponent();
        }

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);

            BtnApply.Click += BtnApply_Click;

            SelectHomePage.Options = _presentation.HomePages.Select(i => new SelectListItem
            {
                Text = i.Name,
                Value = i.Name

            }).ToList();

            SelectTheme.Options = _themeManager.Themes.Select(i => new SelectListItem
            {
                Text = i.Name,
                Value = i.Name

            }).ToList();

            SetUserImage();
            LoadConfiguration();
        }

        async void BtnApply_Click(object sender, RoutedEventArgs e)
        {
            await SaveConfiguration();

            await _themeManager.LoadTheme(_themeManager.Themes.First(i => string.Equals(i.Name, SelectTheme.SelectedItem.Text)));

            await _nav.NavigateToHomePage(_session.CurrentUser.Id);

            _nav.ClearHistory();
        }

        private async void SetUserImage()
        {
            if (_session.CurrentUser.HasPrimaryImage)
            {
                try
                {
                    UserImage.Source = await _imageManager.GetRemoteBitmapAsync(_apiClient.GetUserImageUrl(_session.CurrentUser, new ImageOptions
                    {
                    }));

                    UserDefaultImage.Visibility = Visibility.Collapsed;
                    UserImage.Visibility = Visibility.Visible;

                    return;
                }
                catch (HttpException)
                {
                    // Logged at lower levels
                }
            }

            SetDefaultUserImage();
        }

        private void SetDefaultUserImage()
        {
            UserDefaultImage.Visibility = Visibility.Visible;
            UserImage.Visibility = Visibility.Collapsed;
        }

        private async void LoadConfiguration()
        {
            var userConfig = await _config.GetUserTheaterConfiguration(_session.CurrentUser.Id);

            var homePageOption = SelectHomePage.Options.FirstOrDefault(i => string.Equals(i.Text, userConfig.HomePage, StringComparison.OrdinalIgnoreCase)) ?? 
                SelectHomePage.Options.FirstOrDefault(i => string.Equals(i.Text, "Default")) ?? 
                SelectHomePage.Options.First();

            SelectHomePage.SelectedValue = homePageOption.Value;

            var themeOption = SelectTheme.Options.FirstOrDefault(i => string.Equals(i.Text, userConfig.Theme, StringComparison.OrdinalIgnoreCase)) ??
                SelectTheme.Options.FirstOrDefault(i => string.Equals(i.Text, "Default")) ??
                SelectTheme.Options.First();

            SelectTheme.SelectedValue = themeOption.Value;
        }

        private async Task SaveConfiguration()
        {
            var userConfig = await _config.GetUserTheaterConfiguration(_session.CurrentUser.Id);

            userConfig.HomePage = SelectHomePage.SelectedValue;
            userConfig.Theme = SelectTheme.SelectedValue;

            await _config.UpdateUserTheaterConfiguration(_session.CurrentUser.Id, userConfig);
        }
    }
}
