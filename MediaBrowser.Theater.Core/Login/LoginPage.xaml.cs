using MediaBrowser.Model.ApiClient;
using MediaBrowser.Theater.Interfaces;
using MediaBrowser.Theater.Interfaces.Configuration;
using MediaBrowser.Theater.Interfaces.Navigation;
using MediaBrowser.Theater.Interfaces.Presentation;
using MediaBrowser.Theater.Interfaces.Session;
using MediaBrowser.Theater.Interfaces.Theming;
using MediaBrowser.Theater.Presentation.Pages;
using MediaBrowser.Theater.Presentation.ViewModels;
using System;
using System.Windows;

namespace MediaBrowser.Theater.Core.Login
{
    /// <summary>
    /// Interaction logic for LoginPage.xaml
    /// </summary>
    public partial class LoginPage : BasePage, ILoginPage
    {
        protected IApiClient ApiClient { get; private set; }
        protected IImageManager ImageManager { get; private set; }
        protected INavigationService NavigationManager { get; private set; }
        protected ISessionManager SessionManager { get; private set; }
        protected IPresentationManager PresentationManager { get; private set; }
        protected ITheaterConfigurationManager ConfigurationManager { get; private set; }

        public LoginPage(IImageManager imageManager, INavigationService navigationManager, ISessionManager sessionManager, IPresentationManager presentationManager, ITheaterConfigurationManager configManager, IApiClient apiClient)
        {
            ApiClient = apiClient;
            PresentationManager = presentationManager;
            SessionManager = sessionManager;
            NavigationManager = navigationManager;
            ImageManager = imageManager;
            ConfigurationManager = configManager;

            InitializeComponent();
        }

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);

            DataContext = new UserListViewModel(PresentationManager, ImageManager, SessionManager, ApiClient)
            {
                AddMediaBrowserConnectEntry = true,
                AddSwitchServerEntry = true
            };

            LstUsers.ItemInvoked += ItemsList_ItemInvoked;
            Loaded += LoginPage_Loaded;
        }

        void LoginPage_Loaded(object sender, RoutedEventArgs e)
        {
            PresentationManager.SetDefaultPageTitle();
        }

        /// <summary>
        /// Logs in a user when one is selected
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        async void ItemsList_ItemInvoked(object sender, ItemEventArgs<object> e)
        {
            var model = (UserDtoViewModel)e.Argument;
            var user = model.User;

            if (user.Id == "ChangeServer")
            {
                await NavigationManager.NavigateToServerSelection();
                return;
            }

            if (user.Id == "Connect")
            {
                await NavigationManager.NavigateToConnectLogin();
                return;
            }
            
            if (user.HasPassword)
            {
                await NavigationManager.Navigate(new ManualLoginPage(user.Name, ChkAutoLogin.IsChecked, SessionManager, PresentationManager, ApiClient));
                return;
            }

            try
            {
                await SessionManager.LoginToServer(ApiClient, user.Name, string.Empty, (bool)ChkAutoLogin.IsChecked);
            }
            catch (Exception ex)
            {
                PresentationManager.ShowMessage(new MessageBoxInfo
                {
                    Caption = "Login Failure",
                    Text = ex.Message,
                    Icon = MessageBoxIcon.Error
                });
            }
        }
    }
}
