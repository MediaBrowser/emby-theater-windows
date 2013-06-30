using MediaBrowser.Common;
using MediaBrowser.Common.Updates;
using MediaBrowser.Theater.Interfaces.Theming;
using MediaBrowser.Theater.Presentation.Controls;
using MediaBrowser.Theater.Presentation.Pages;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace MediaBrowser.UI.Pages.Plugins
{
    /// <summary>
    /// Interaction logic for PluginsPage.xaml
    /// </summary>
    public partial class PluginsPage : BasePage
    {
        private readonly IPackageManager _packageManager;
        private readonly IThemeManager _themeManager;
        private readonly IApplicationHost _appHost;

        public PluginsPage(IPackageManager packageManager, IThemeManager themeManager, IApplicationHost appHost)
        {
            _packageManager = packageManager;
            _themeManager = themeManager;
            _appHost = appHost;
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

                    PageContent.Content = new InstalledPlugins(_appHost);
                    break;
                case "plugin catalog":

                    PageContent.Content = new PluginCatalog(_packageManager, _themeManager);
                    break;
            }
        }

        void HomePage_Loaded(object sender, RoutedEventArgs e)
        {
        }
    }
}
