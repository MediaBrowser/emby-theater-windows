using MediaBrowser.Model.ApiClient;
using MediaBrowser.Model.Net;
using MediaBrowser.Theater.Interfaces.Navigation;
using MediaBrowser.Theater.Interfaces.Presentation;
using MediaBrowser.Theater.Interfaces.Session;
using MediaBrowser.Theater.Interfaces.Theming;
using MediaBrowser.Theater.Presentation.Controls;
using MediaBrowser.Theater.Presentation.Pages;
using MediaBrowser.Theater.Presentation.ViewModels;
using System;
using System.Net;
using System.Windows;

namespace MediaBrowser.Theater.Core.Login
{
    /// <summary>
    /// Interaction logic for LoginPage.xaml
    /// </summary>
    public partial class LoginPage : BasePage
    {
        protected IApiClient ApiClient { get; private set; }
        protected IImageManager ImageManager { get; private set; }
        protected INavigationService NavigationManager { get; private set; }
        protected ISessionManager SessionManager { get; private set; }
        protected IPresentationManager PresentationManager { get; private set; }

        public LoginPage(IApiClient apiClient, IImageManager imageManager, INavigationService navigationManager, ISessionManager sessionManager, IPresentationManager presentationManager)
        {
            PresentationManager = presentationManager;
            SessionManager = sessionManager;
            NavigationManager = navigationManager;
            ImageManager = imageManager;
            ApiClient = apiClient;

            InitializeComponent();
        }

        private UserListViewModel _viewModel;

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);

            DataContext = _viewModel = new UserListViewModel(PresentationManager, ApiClient, ImageManager, SessionManager);

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
            }
            catch (HttpException ex)
            {
                if (ex.StatusCode.HasValue && (ex.StatusCode.Value == HttpStatusCode.Unauthorized || ex.StatusCode.Value == HttpStatusCode.Forbidden))
                {
                    PresentationManager.ShowMessage(new MessageBoxInfo
                    {
                        Caption = "Login Failure",
                        Text = "Invalid username or password. Please try again.",
                        Icon = MessageBoxIcon.Error
                    });
                }
                else
                {
                    PresentationManager.ShowDefaultErrorMessage();
                }
            }
        }

        ~LoginPage()
        {
            _viewModel.Dispose();
        }
    }
}
