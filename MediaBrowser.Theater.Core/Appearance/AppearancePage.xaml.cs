using System.Collections.Generic;
using System.Threading;
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
        private readonly IPresentationManager _presentation;
        private readonly IThemeManager _themeManager;
        private readonly INavigationService _nav;
        private readonly IScreensaverManager _screensaverManager;

        public AppearancePage(ITheaterConfigurationManager config, ISessionManager session, IImageManager imageManager, IPresentationManager presentation, IThemeManager themeManager, INavigationService nav, IScreensaverManager screensaverManager)
        {
            _config = config;
            _session = session;
            _imageManager = imageManager;
            _presentation = presentation;
            _themeManager = themeManager;
            _nav = nav;
            _screensaverManager = screensaverManager;

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
            SelectTheme.SelectedItemChanged += SelectTheme_SelectedItemChanged;

            SelectSreensaver.Options = new List<SelectListItem> { new SelectListItem { Text = "None", Value = "None" } }.
                                            Union(
                                                    _screensaverManager.ScreensaverFactories.Select(i => new SelectListItem
                                                    {
                                                        Text = i.Name,
                                                        Value = i.Name

                                                    })).ToList();
            SelectSreensaver.SelectedItemChanged += SelectScreensaver_SelectedItemChanged;

           

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

        private void SelectScreensaver_SelectedItemChanged(object sender, EventArgs e)
        {
            _screensaverManager.CurrentScreensaverName = SelectSreensaver.SelectedValue; ;
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
                var apiClient = _session.ActiveApiClient;

                try
                {
                    UserImage.Source = await _imageManager.GetRemoteBitmapAsync(apiClient, apiClient.GetUserImageUrl(_session.CurrentUser, new ImageOptions
                    {
                    }), CancellationToken.None);

                    UserDefaultImage.Visibility = Visibility.Collapsed;
                    UserImage.Visibility = Visibility.Visible;

                    return;
                }
                catch (Exception)
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

        private void LoadConfiguration()
        {
            var userConfig = _config.GetUserTheaterConfiguration(_session.LocalUserId);

            var homePageOption = SelectHomePage.Options.FirstOrDefault(i => string.Equals(i.Text, userConfig.HomePage, StringComparison.OrdinalIgnoreCase)) ??
                SelectHomePage.Options.FirstOrDefault(i => string.Equals(i.Text, "Default")) ??
                SelectHomePage.Options.First();

            SelectHomePage.SelectedValue = homePageOption.Value;

            var themeOption = SelectTheme.Options.FirstOrDefault(i => string.Equals(i.Text, userConfig.Theme, StringComparison.OrdinalIgnoreCase)) ??
                SelectTheme.Options.FirstOrDefault(i => string.Equals(i.Text, "Default")) ??
                SelectTheme.Options.First();

            SelectTheme.SelectedValue = themeOption.Value;

            var screensaverOption = SelectSreensaver.Options.FirstOrDefault(i => string.Equals(i.Text, userConfig.Screensaver, StringComparison.OrdinalIgnoreCase)) ??
                SelectSreensaver.Options.FirstOrDefault(i => string.Equals(i.Text, _screensaverManager.CurrentScreensaverName )) ??
                SelectSreensaver.Options.First();

            SelectSreensaver.SelectedValue = screensaverOption.Value;

            ChkShowBackButton.IsChecked = userConfig.ShowBackButton;

            ChkShowExternalDiscApp.IsChecked = userConfig.ShowExternalDiscApp;
        }

        private async Task SaveConfiguration()
        {
            var userConfig = _config.GetUserTheaterConfiguration(_session.LocalUserId);

            userConfig.HomePage = SelectHomePage.SelectedValue;
            userConfig.Theme = SelectTheme.SelectedValue;
            userConfig.Screensaver = SelectSreensaver.SelectedValue;
            userConfig.ShowBackButton = ChkShowBackButton.IsChecked ?? false;
            userConfig.ShowExternalDiscApp = ChkShowExternalDiscApp.IsChecked ?? false;

            await _config.UpdateUserTheaterConfiguration(_session.LocalUserId, userConfig);
        }
    }
}
