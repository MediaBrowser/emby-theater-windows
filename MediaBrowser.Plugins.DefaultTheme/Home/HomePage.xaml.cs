using System.Windows.Controls;
using MediaBrowser.Model.Dto;
using MediaBrowser.Theater.Interfaces.Presentation;
using MediaBrowser.Theater.Presentation.Pages;
using System;
using System.ComponentModel;
using System.Linq;
using System.Windows;

namespace MediaBrowser.Plugins.DefaultTheme.Home
{
    /// <summary>
    /// Interaction logic for HomePage.xaml
    /// </summary>
    public partial class HomePage : BasePage, ISupportsBackdrops, IItemPage
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

            DataContextChanged += PanoramaDetailPage_DataContextChanged;
        }

        void PanoramaDetailPage_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            var viewModel = e.NewValue as HomePageViewModel;

            if (viewModel != null)
            {
                viewModel.PropertyChanged += viewModel_PropertyChanged;
            }
        }

        void viewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (string.Equals(e.PropertyName, "CurrentSection"))
            {
                ScrollViewer.ScrollToLeftEnd();

                var current = MenuList.ItemContainerGenerator.ContainerFromItem(MenuList.SelectedItem) as ListBoxItem;

                if (current != null)
                {
                    current.Focus();
                }
            }
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

            var vm = DataContext as HomePageViewModel;

            if (vm != null)
            {
                vm.SetBackdrops();
            }
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
