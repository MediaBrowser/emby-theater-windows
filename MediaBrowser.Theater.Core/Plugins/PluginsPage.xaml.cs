using MediaBrowser.Common;
using MediaBrowser.Common.Updates;
using MediaBrowser.Theater.Interfaces.Navigation;
using MediaBrowser.Theater.Interfaces.Presentation;
using MediaBrowser.Theater.Presentation.Controls;
using MediaBrowser.Theater.Presentation.Pages;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace MediaBrowser.Theater.Core.Plugins
{
    /// <summary>
    /// Interaction logic for PluginsPage.xaml
    /// </summary>
    public partial class PluginsPage : BasePage
    {
        private readonly IPresentationManager _presentation;
        private readonly IApplicationHost _appHost;
        private readonly INavigationService _nav;
        private readonly IInstallationManager _installationManager;

        public PluginsPage(IApplicationHost appHost, INavigationService nav, IPresentationManager presentation, IInstallationManager installationManager)
        {
            _appHost = appHost;
            _nav = nav;
            _presentation = presentation;
            _installationManager = installationManager;
            InitializeComponent();
        }

        protected override async void OnInitialized(EventArgs e)
        {
            Loaded += HomePage_Loaded;

            MenuList.SelectionChanged += MenuList_SelectionChanged;
            new ListFocuser(MenuList).FocusAfterContainersGenerated(0);

            var views = new List<string>();

            views.Add("installed plugins");
            views.Add("plugin catalog");

            MenuList.ItemsSource = CollectionViewSource.GetDefaultView(views);

            base.OnInitialized(e);
        }

        void MenuList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var item = MenuList.SelectedItem as string;

            ScrollingPanel.SetHorizontalOffset(0);

            switch (item)
            {
                case "installed plugins":

                    PageContent.Content = new InstalledPlugins(_appHost, _nav, _presentation, _installationManager);
                    break;
                case "plugin catalog":

                    PageContent.Content = new PluginCatalog(_presentation, _nav, _appHost, _installationManager);
                    break;
            }
        }

        void HomePage_Loaded(object sender, RoutedEventArgs e)
        {
            _presentation.SetDefaultPageTitle();
        }
    }
}
