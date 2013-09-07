using MediaBrowser.Model.Dto;
using MediaBrowser.Theater.Interfaces.Presentation;
using MediaBrowser.Theater.Presentation.Pages;
using System;
using System.Linq;
using System.Windows;

namespace MediaBrowser.Plugins.DefaultTheme.Home
{
    /// <summary>
    /// Interaction logic for HomePage.xaml
    /// </summary>
    public partial class HomePage : BasePage, ISupportsItemBackdrops, IItemPage
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
            Unloaded += HomePage_Unloaded;
        }

        private ResourceDictionary _dynamicResourceDictionary;

        void HomePage_Unloaded(object sender, RoutedEventArgs e)
        {
            _presentationManager.RemoveResourceDictionary(_dynamicResourceDictionary);
        }

        void HomePage_Loaded(object sender, RoutedEventArgs e)
        {
            _presentationManager.SetDefaultPageTitle();

            if (_dynamicResourceDictionary == null)
            {
                var namespaceName = typeof(DefaultTheme).Namespace;

                _dynamicResourceDictionary = new[] { "HomePageGlobals" }.Select(i => new ResourceDictionary
                {
                    Source = new Uri("pack://application:,,,/" + namespaceName + ";component/Resources/" + i + ".xaml", UriKind.Absolute)

                }).First();
            }

            _presentationManager.AddResourceDictionary(_dynamicResourceDictionary);
        }

        public BaseItemDto BackdropItem
        {
            get { return _parentItem; }
        }

        public BaseItemDto PageItem
        {
            get { return _parentItem; }
        }

        public ViewType ViewType
        {
            get { return ViewType.Home; }
        }
    }
}
