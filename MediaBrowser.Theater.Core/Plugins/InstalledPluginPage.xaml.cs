using MediaBrowser.Theater.Interfaces.Presentation;
using MediaBrowser.Theater.Presentation.Pages;
using System;
using System.Windows;

namespace MediaBrowser.Theater.Core.Plugins
{
    /// <summary>
    /// Interaction logic for InstalledPluginPage.xaml
    /// </summary>
    public partial class InstalledPluginPage : BasePage
    {
        private readonly InstalledPluginViewModel _plugin;
        private readonly IPresentationManager _presentationManager;

        public InstalledPluginPage(InstalledPluginViewModel plugin, IPresentationManager presentationManager)
        {
            _plugin = plugin;
            _presentationManager = presentationManager;

            InitializeComponent();

            DataContext = plugin;
        }

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);

            Loaded += InstalledPluginPage_Loaded;
        }

        void InstalledPluginPage_Loaded(object sender, RoutedEventArgs e)
        {
            _presentationManager.SetDefaultPageTitle();
        }
    }
}
