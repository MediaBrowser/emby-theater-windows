using MediaBrowser.Model.ApiClient;
using MediaBrowser.Plugins.DefaultTheme.Resources;
using MediaBrowser.Theater.Interfaces.Navigation;
using MediaBrowser.Theater.Interfaces.Presentation;
using MediaBrowser.Theater.Interfaces.Session;
using MediaBrowser.Theater.Interfaces.Theming;
using MediaBrowser.Theater.Presentation.Controls;
using MediaBrowser.Theater.Presentation.Pages;
using System.Windows;

namespace MediaBrowser.Plugins.DefaultTheme.Pages
{
    /// <summary>
    /// Interaction logic for LoginPage.xaml
    /// </summary>
    public partial class LoginPage : BaseLoginPage
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LoginPage" /> class.
        /// </summary>
        public LoginPage(IApiClient apiClient, IImageManager imageManager, INavigationService navService, ISessionManager sessionManager, IApplicationWindow appWindow, IThemeManager themeManager)
            : base(apiClient, imageManager, navService, sessionManager, appWindow, themeManager)
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
            AppResources.Instance.SetDefaultPageTitle();
        }
    }
}
