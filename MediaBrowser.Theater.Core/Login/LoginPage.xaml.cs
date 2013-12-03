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

        public LoginPage(IApiClient apiClient, IImageManager imageManager, INavigationService navigationManager, ISessionManager sessionManager, IPresentationManager presentationManager, ITheaterConfigurationManager configManager)
        {
            PresentationManager = presentationManager;
            SessionManager = sessionManager;
            NavigationManager = navigationManager;
            ImageManager = imageManager;
            ApiClient = apiClient;
            ConfigurationManager = configManager;

            InitializeComponent();
        }

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);

            DataContext = new UserListViewModel(PresentationManager, ApiClient, ImageManager, SessionManager, NavigationManager);

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

            if (user.HasPassword)
            {
                await NavigationManager.Navigate(new ManualLoginPage(user.Name, SessionManager, PresentationManager));
                return;
            }

            try
            {
                await SessionManager.Login(user.Name, string.Empty);

                //If login sucessful and auto login checkbox is ticked then save the auto-login config
                if (ChkAutoLogin.IsChecked == true)
                {
                    ConfigurationManager.Configuration.AutoLoginConfiguration.UserName = user.Name;
                    ConfigurationManager.SaveConfiguration();
                }
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
