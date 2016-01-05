using System.Windows.Controls;
using MediaBrowser.Model.Dto;
using MediaBrowser.Theater.Interfaces.Presentation;
using MediaBrowser.Theater.Presentation.Pages;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;

namespace MediaBrowser.Plugins.DefaultTheme.Home
{
    /// <summary>
    /// Interaction logic for HomePage.xaml
    /// </summary>
    public partial class HomePage : BasePage, ISupportsBackdrops, IItemPage, IHomePage, ISupportSearch
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
            var viewModel = DataContext as HomePageViewModel;

            if (viewModel != null)
            {
                viewModel.DisableActivePresentation();
            }

            _presentationManager.RemoveResourceDictionary(_dynamicResourceDictionary);
        }

        private static bool _newAppDisplayed;
        void HomePage_Loaded(object sender, RoutedEventArgs e)
        {
            var viewModel = DataContext as HomePageViewModel;

            if (viewModel != null)
            {
                viewModel.EnableActivePresentation();
            }

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

            if (viewModel != null)
            {
                viewModel.SetBackdrops();
            }

            if (!_newAppDisplayed && DateTime.UtcNow >= new DateTime(2016, 1, 15))
            {
                _newAppDisplayed = true;

                var result = _presentationManager.ShowMessage(new MessageBoxInfo
                {
                    Button = MessageBoxButton.OKCancel,
                    Caption = "Try the new Emby Theater!",
                    Icon = Theater.Interfaces.Theming.MessageBoxIcon.Information,
                    Text = "The new Emby Theater is faster, smoother, and looks better than ever. Get it now on the Emby website!"
                });

                if (result == MessageBoxResult.OK)
                {
                    var process = new Process
                    {
                        StartInfo = new ProcessStartInfo
                        {
                            FileName = "http://emby.media/download"
                        },

                        EnableRaisingEvents = true,
                    };

                    process.Exited += process_Exited;

                    try
                    {
                        process.Start();
                    }
                    catch
                    {
                    }
                }
            }
        }

        void process_Exited(object sender, EventArgs e)
        {
            ((Process)sender).Dispose();
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
