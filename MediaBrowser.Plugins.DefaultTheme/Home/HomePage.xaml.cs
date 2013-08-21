using MediaBrowser.Model.Dto;
using MediaBrowser.Theater.Interfaces.Presentation;
using MediaBrowser.Theater.Presentation.Pages;
using System.Windows;

namespace MediaBrowser.Plugins.DefaultTheme.Home
{
    /// <summary>
    /// Interaction logic for HomePage.xaml
    /// </summary>
    public partial class HomePage : BasePage, ISupportsItemBackdrops
    {
        /// <summary>
        /// Gets the application window.
        /// </summary>
        /// <value>The application window.</value>
        private readonly IPresentationManager _presentationManager;
        private readonly BaseItemDto _parentItem;

        public HomePage(BaseItemDto parentItem, IPresentationManager presentationManager)
        {
            _presentationManager = presentationManager;
            _parentItem = parentItem;

            InitializeComponent();

            Loaded += HomePage_Loaded;
        }

        void HomePage_Loaded(object sender, RoutedEventArgs e)
        {
            _presentationManager.SetDefaultPageTitle();
        }

        public BaseItemDto BackdropItem
        {
            get { return _parentItem; }
        }
    }
}
