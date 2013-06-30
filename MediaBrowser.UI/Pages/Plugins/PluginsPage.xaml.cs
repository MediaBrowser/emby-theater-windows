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

        public PluginsPage(IPackageManager packageManager, IThemeManager themeManager)
        {
            _packageManager = packageManager;
            _themeManager = themeManager;
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

                    //ScrollViewer.HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled;
                    //ScrollViewer.VerticalScrollBarVisibility = ScrollBarVisibility.Hidden;

                    //ScrollingPanel.CanVerticallyScroll = true;
                    //ScrollingPanel.CanHorizontallyScroll = false;

                    PageContent.Content = new InstalledPlugins();
                    break;
                case "plugin catalog":

                    //ScrollViewer.HorizontalScrollBarVisibility = ScrollBarVisibility.Hidden;
                    //ScrollViewer.VerticalScrollBarVisibility = ScrollBarVisibility.Disabled;

                    //ScrollingPanel.CanVerticallyScroll = false;
                    //ScrollingPanel.CanHorizontallyScroll = true;

                    PageContent.Content = new PluginCatalog(_packageManager, _themeManager);
                    break;
            }
        }

        void HomePage_Loaded(object sender, RoutedEventArgs e)
        {
        }
    }
}
