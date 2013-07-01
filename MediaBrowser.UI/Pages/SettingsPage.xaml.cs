using MediaBrowser.Theater.Interfaces.Navigation;
using MediaBrowser.Theater.Interfaces.Presentation;
using MediaBrowser.Theater.Presentation.Controls;
using MediaBrowser.Theater.Presentation.Pages;
using MediaBrowser.Theater.Presentation.ViewModels;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Data;

namespace MediaBrowser.UI.Pages
{
    /// <summary>
    /// Interaction logic for SettingsPage.xaml
    /// </summary>
    public partial class SettingsPage : BasePage
    {
        private readonly IPresentationManager _presentationManager;

        private readonly INavigationService _nav;

        /// <summary>
        /// Initializes a new instance of the <see cref="SettingsPage" /> class.
        /// </summary>
        /// <param name="presentationManager">The presentation manager.</param>
        /// <param name="nav">The nav.</param>
        public SettingsPage(IPresentationManager presentationManager, INavigationService nav)
        {
            _presentationManager = presentationManager;
            _nav = nav;

            InitializeComponent();

            Loaded += SettingsPage_Loaded;
            MenuList.ItemInvoked += MenuList_ItemInvoked;
            PluginMenuList.ItemInvoked += MenuList_ItemInvoked;
        }

        async void MenuList_ItemInvoked(object sender, ItemEventArgs<object> e)
        {
            var settingsPage = (ISettingsPage)e.Argument;

            await _nav.Navigate(settingsPage.GetPage());
        }

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);

            LoadSystemSettingsList();
            LoadPluginSettingsList();
        }

        private void LoadPluginSettingsList()
        {
            var items = new RangeObservableCollection<ISettingsPage>();
            var view = (ListCollectionView)CollectionViewSource.GetDefaultView(items);

            PluginMenuList.ItemsSource = view;

            var pages = _presentationManager.SettingsPages
                .Where(i => i.Category == SettingsPageCategory.Plugin)
                .OrderBy(GetOrder)
                .ThenBy(i => i.Name);

            items.AddRange(pages);
        }

        private void LoadSystemSettingsList()
        {
            var items = new RangeObservableCollection<ISettingsPage>();
            var view = (ListCollectionView)CollectionViewSource.GetDefaultView(items);

            MenuList.ItemsSource = view;

            var pages = _presentationManager.SettingsPages
                .Where(i => i.Category == SettingsPageCategory.System)
                .OrderBy(GetOrder)
                .ThenBy(i => i.Name);

            items.AddRange(pages);
        }

        private int GetOrder(ISettingsPage page)
        {
            var orderedPage = page as IOrderedSettingsPage;

            return orderedPage == null ? 10 : orderedPage.Order;
        }

        /// <summary>
        /// Handles the Loaded event of the SettingsPage control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        void SettingsPage_Loaded(object sender, RoutedEventArgs e)
        {
            _presentationManager.SetDefaultPageTitle();
            _presentationManager.ClearBackdrops();
        }
    }
}
