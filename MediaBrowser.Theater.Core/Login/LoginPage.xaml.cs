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
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;

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

        private RangeObservableCollection<UserDtoViewModel> _listItems;
        private ListCollectionView _listCollectionView;

        public LoginPage(IApiClient apiClient, IImageManager imageManager, INavigationService navigationManager, ISessionManager sessionManager, IPresentationManager presentationManager)
        {
            PresentationManager = presentationManager;
            SessionManager = sessionManager;
            NavigationManager = navigationManager;
            ImageManager = imageManager;
            ApiClient = apiClient;

            InitializeComponent();
        }

        protected override async void OnInitialized(System.EventArgs e)
        {
            base.OnInitialized(e);

            lstUsers.ItemInvoked += ItemsList_ItemInvoked;
            Loaded += LoginPage_Loaded;

            _listItems = new RangeObservableCollection<UserDtoViewModel>();
            _listCollectionView = (ListCollectionView)CollectionViewSource.GetDefaultView(_listItems);

            lstUsers.ItemsSource = _listCollectionView;

            await ReloadUsers(true);
        }

        void LoginPage_Loaded(object sender, RoutedEventArgs e)
        {
            PresentationManager.SetDefaultPageTitle();
        }

        /// <summary>
        /// Reloads the users.
        /// </summary>
        /// <returns>Task.</returns>
        protected async Task ReloadUsers(bool isInitialLoad)
        {
            // Record the current item
            var currentItem = _listCollectionView.CurrentItem as UserDtoViewModel;

            try
            {
                var users = await ApiClient.GetPublicUsersAsync();

                int? selectedIndex = null;

                if (isInitialLoad)
                {
                    selectedIndex = 0;
                }
                else if (currentItem != null)
                {
                    var index = Array.FindIndex(users, i => string.Equals(i.Id, currentItem.User.Id));

                    if (index != -1)
                    {
                        selectedIndex = index;
                    }
                }

                _listItems.Clear();

                _listItems.AddRange(users.Select(i => new UserDtoViewModel(ApiClient, ImageManager) { User = i }));

                MoveFocus(new TraversalRequest(FocusNavigationDirection.First));

                if (selectedIndex.HasValue)
                {
                    new ListFocuser(lstUsers).FocusAfterContainersGenerated(selectedIndex.Value);
                }

            }
            catch (HttpException)
            {
                PresentationManager.ShowDefaultErrorMessage();
            }
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
    }
}
