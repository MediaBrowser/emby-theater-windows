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
using System.Threading.Tasks;
using System.Windows;

namespace MediaBrowser.Theater.Core.Appearance
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

            SelectBackdropTransition.Options =
                SelectNavigationTransition.Options =

                new[] { "none", "blur", "circle reveal", "cloud reveal", "fade", "horizontal blinds", "horizontal slide", "horizontal wipe", "ripple", "smooth swirl", "vertical blinds", "vertical slide", "vertical wipe", "wave" }

                .Select(i => new SelectListItem { Text = i, Value = i }).ToList();

            SelectTheme.SelectedItemChanged += SelectTheme_SelectedItemChanged;

            SetUserImage();
            LoadConfiguration();
        }

        void SelectTheme_SelectedItemChanged(object sender, EventArgs e)
        {
            var theme = SelectTheme.SelectedValue;

            var themeInstance = _themeManager.Themes.First(i => string.Equals(i.Name, theme));

            if (!string.IsNullOrEmpty(themeInstance.DefaultHomePageName))
            {
                SelectHomePage.SelectedValue = themeInstance.DefaultHomePageName;
            }
        }

        async void BtnApply_Click(object sender, RoutedEventArgs e)
        {
            await SaveConfiguration();

            await _themeManager.LoadTheme(_themeManager.Themes.First(i => string.Equals(i.Name, SelectTheme.SelectedItem.Text)));

            await _nav.NavigateToHomePage();

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

            SelectNavigationTransition.SelectedValue = SelectNavigationTransition.ContainsValue(userConfig.NavigationTransition) ? userConfig.NavigationTransition : "none";
            SelectBackdropTransition.SelectedValue = SelectBackdropTransition.ContainsValue(userConfig.BackdropTransition) ? userConfig.BackdropTransition : "none";
        }

        private async Task SaveConfiguration()
        {
            var userConfig = await _config.GetUserTheaterConfiguration(_session.CurrentUser.Id);

            userConfig.HomePage = SelectHomePage.SelectedValue;
            userConfig.Theme = SelectTheme.SelectedValue;

            userConfig.NavigationTransition = string.Equals(SelectNavigationTransition.SelectedValue, "none")
                                                             ? string.Empty
                                                             : SelectNavigationTransition.SelectedValue;

            userConfig.BackdropTransition = string.Equals(SelectBackdropTransition.SelectedValue, "none")
                                                             ? string.Empty
                                                             : SelectBackdropTransition.SelectedValue;

            await _config.UpdateUserTheaterConfiguration(_session.CurrentUser.Id, userConfig);
        }
    }
}
