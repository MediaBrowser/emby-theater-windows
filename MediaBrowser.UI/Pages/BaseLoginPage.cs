using MediaBrowser.Model.ApiClient;
using MediaBrowser.Model.Net;
using MediaBrowser.Theater.Interfaces.Navigation;
using MediaBrowser.Theater.Interfaces.Presentation;
using MediaBrowser.Theater.Interfaces.Session;
using MediaBrowser.UI.Controls;
using MediaBrowser.UI.ViewModels;
using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Navigation;

namespace MediaBrowser.UI.Pages
{
    /// <summary>
    /// Provides a base page for theme login pages
    /// </summary>
    public abstract class BaseLoginPage : Page
    {
        protected IApiClient ApiClient { get; private set; }
        protected IImageManager ImageManager { get; private set; }
        protected INavigationService NavigationManager { get; private set; }
        protected ISessionManager SessionManager { get; private set; }
        protected IApplicationWindow ApplicationWindow { get; private set; }

        protected RangeObservableCollection<UserDtoViewModel> ListItems { get; private set; }
        protected ListCollectionView ListCollectionView { get; private set; }

        private IInputElement _lastFocused;

        protected BaseLoginPage(IApiClient apiClient, IImageManager imageManager, INavigationService navigationManager, ISessionManager sessionManager, IApplicationWindow applicationWindow)
        {
            ApplicationWindow = applicationWindow;
            SessionManager = sessionManager;
            NavigationManager = navigationManager;
            ImageManager = imageManager;
            ApiClient = apiClient;
        }

        /// <summary>
        /// Subclasses must provide the list that holds the users
        /// </summary>
        /// <value>The items list.</value>
        protected abstract ExtendedListBox ItemsList { get; }

        /// <summary>
        /// Raises the <see cref="E:Initialized" /> event.
        /// </summary>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected override async void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);

            FocusManager.SetIsFocusScope(this, true);

            ListItems = new RangeObservableCollection<UserDtoViewModel>();
            ListCollectionView = (ListCollectionView)CollectionViewSource.GetDefaultView(ListItems);

            ItemsList.ItemsSource = ListCollectionView;

            ItemsList.ItemInvoked += ItemsList_ItemInvoked;
            Loaded += BaseLoginPage_Loaded;

            await ReloadUsers(true);
        }

        /// <summary>
        /// Handles the Loaded event of the BaseLoginPage control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> instance containing the event data.</param>
        void BaseLoginPage_Loaded(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigating -= NavigationService_Navigating;
            NavigationService.Navigating += NavigationService_Navigating;

            ApplicationWindow.ClearBackdrops();
        }

        void NavigationService_Navigating(object sender, NavigatingCancelEventArgs e)
        {
            if (e.Content.Equals(this))
            {
                FocusManager.SetFocusedElement(this, _lastFocused);
            }
            else
            {
                _lastFocused = FocusManager.GetFocusedElement(this);
            }
        }

        /// <summary>
        /// Reloads the users.
        /// </summary>
        /// <returns>Task.</returns>
        protected async Task ReloadUsers(bool isInitialLoad)
        {
            // Record the current item
            var currentItem = ListCollectionView.CurrentItem as UserDtoViewModel;

            try
            {
                var users = await ApiClient.GetUsersAsync();

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
                
                ListItems.Clear();

                ListItems.AddRange(users.Select(i => new UserDtoViewModel(ApiClient, ImageManager) { User = i }));

                if (_lastFocused == null)
                {
                    MoveFocus(new TraversalRequest(FocusNavigationDirection.First));
                }

                if (selectedIndex.HasValue)
                {
                    new ListFocuser(ItemsList).FocusAfterContainersGenerated(selectedIndex.Value);
                }

            }
            catch (HttpException)
            {
                App.Instance.ShowDefaultErrorMessage();
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

            try
            {
                await SessionManager.Login(user, string.Empty);
            }
            catch (HttpException ex)
            {
                if (ex.StatusCode.HasValue && ex.StatusCode.Value == HttpStatusCode.Unauthorized)
                {
                    App.Instance.ShowErrorMessage("Invalid username or password. Please try again.", caption: "Login Failure");
                }
                else
                {
                    App.Instance.ShowDefaultErrorMessage();
                }
            }
        }
    }
}
