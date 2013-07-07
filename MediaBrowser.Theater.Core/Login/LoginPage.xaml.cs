using MediaBrowser.Model.ApiClient;
using MediaBrowser.Theater.Interfaces.Navigation;
using MediaBrowser.Theater.Interfaces.Presentation;
using MediaBrowser.Theater.Interfaces.Session;
using MediaBrowser.Theater.Presentation.Controls;
using System.Windows;

namespace MediaBrowser.Theater.Core.Login
{
    /// <summary>
    /// Interaction logic for LoginPage.xaml
    /// </summary>
    public partial class LoginPage : BaseLoginPage
    {
        public LoginPage(IApiClient apiClient, IImageManager imageManager, INavigationService navigationManager, ISessionManager sessionManager, IPresentationManager presentationManager)
            : base(apiClient, imageManager, navigationManager, sessionManager, presentationManager)
        {
            InitializeComponent();
        }

        /// <summary>
        /// Subclasses must provide the list that holds the users
        /// </summary>
        /// <value>The items list.</value>
        protected override ExtendedListBox ItemsList
        {
            get
            {
                return lstUsers;
            }
        }

        protected override void OnInitialized(System.EventArgs e)
        {
            base.OnInitialized(e);

            Loaded += LoginPage_Loaded;
        }

        void LoginPage_Loaded(object sender, RoutedEventArgs e)
        {
            PresentationManager.ClearBackdrops();
            PresentationManager.SetDefaultPageTitle();
        }
    }
}
