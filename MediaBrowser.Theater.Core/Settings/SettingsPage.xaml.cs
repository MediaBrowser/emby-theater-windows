using MediaBrowser.Common;
using MediaBrowser.Common.Updates;
using MediaBrowser.Theater.Interfaces.Navigation;
using MediaBrowser.Theater.Interfaces.Presentation;
using MediaBrowser.Theater.Interfaces.Session;
using MediaBrowser.Theater.Presentation.Controls;
using MediaBrowser.Theater.Presentation.Pages;
using MediaBrowser.Theater.Presentation.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace MediaBrowser.Theater.Core.Settings
{
    /// <summary>
    /// Interaction logic for SettingsPage.xaml
    /// </summary>
    public partial class SettingsPage : BasePage
    {
        private readonly IPresentationManager _presentationManager;
        private readonly IApplicationHost _appHost;
        private readonly IInstallationManager _installationManager;

        private readonly INavigationService _nav;
        private readonly ISessionManager _session;

        public SettingsPage(IPresentationManager presentationManager, INavigationService nav, ISessionManager session, IApplicationHost appHost, IInstallationManager installationManager)
        {
            _presentationManager = presentationManager;
            _nav = nav;
            _session = session;
            _appHost = appHost;
            _installationManager = installationManager;

            InitializeComponent();

            Loaded += SettingsPage_Loaded;
            MenuList.ItemInvoked += MenuList_ItemInvoked;
            PluginMenuList.ItemInvoked += MenuList_ItemInvoked;
        }

        async void MenuList_ItemInvoked(object sender, ItemEventArgs<object> e)
        {
            var settingsPage = (ISettingsPage)e.Argument;

            var page = (Page)_appHost.CreateInstance(settingsPage.PageType);

            await _nav.Navigate(page);
        }

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);

            SystemInfoContent.Content = new SystemInfoPanel(_appHost, _installationManager);

            var pages = _presentationManager.SettingsPages
                .Where(i => i.IsVisible(_session.CurrentUser))
                .OrderBy(GetOrder)
                .ThenBy(i => i.Name)
                .ToList();

            new ListFocuser(MenuList).FocusAfterContainersGenerated(0);

            LoadSystemSettingsList(pages);
            LoadPluginSettingsList(pages);
        }

        /// <summary>
        /// Handles the Loaded event of the SettingsPage control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        void SettingsPage_Loaded(object sender, RoutedEventArgs e)
        {
            _presentationManager.SetDefaultPageTitle();
        }
        
        private void LoadPluginSettingsList(IEnumerable<ISettingsPage> allSettingsPages)
        {
            var items = new RangeObservableCollection<ISettingsPage>();
            var view = (ListCollectionView)CollectionViewSource.GetDefaultView(items);

            PluginMenuList.ItemsSource = view;

            var pages = allSettingsPages
                .Where(i => i.Category == SettingsPageCategory.Plugin);

            items.AddRange(pages);
        }

        private void LoadSystemSettingsList(IEnumerable<ISettingsPage> allSettingsPages)
        {
            var items = new RangeObservableCollection<ISettingsPage>();
            var view = (ListCollectionView)CollectionViewSource.GetDefaultView(items);

            MenuList.ItemsSource = view;

            var pages = allSettingsPages
                .Where(i => i.Category == SettingsPageCategory.System);

            items.AddRange(pages);
        }

        private int GetOrder(ISettingsPage page)
        {
            var orderedPage = page as IOrderedSettingsPage;

            return orderedPage == null ? 10 : orderedPage.Order;
        }

        protected override void FocusOnFirstLoad()
        {
            // Focus the first settings tile 
        }
    }
}
