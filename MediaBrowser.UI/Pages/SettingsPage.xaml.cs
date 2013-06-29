using MediaBrowser.Theater.Interfaces.Navigation;
using MediaBrowser.Theater.Interfaces.Presentation;
using MediaBrowser.Theater.Interfaces.Theming;
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
        /// <summary>
        /// The _theme manager
        /// </summary>
        private readonly IThemeManager _themeManager;

        private readonly IPresentationManager _presentationManager;

        private readonly INavigationService _nav;

        /// <summary>
        /// Initializes a new instance of the <see cref="SettingsPage" /> class.
        /// </summary>
        /// <param name="themeManager">The theme manager.</param>
        /// <param name="presentationManager">The presentation manager.</param>
        /// <param name="nav">The nav.</param>
        public SettingsPage(IThemeManager themeManager, IPresentationManager presentationManager, INavigationService nav)
        {
            _themeManager = themeManager;
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

            var pages = _presentationManager.SettingsPages.Where(i => !(i is ISystemSettingsPage))
                .OrderBy(i => i.Name);

            items.AddRange(pages);
        }

        private void LoadSystemSettingsList()
        {
            var items = new RangeObservableCollection<ISettingsPage>();
            var view = (ListCollectionView)CollectionViewSource.GetDefaultView(items);

            MenuList.ItemsSource = view;

            var pages = _presentationManager.SettingsPages.OfType<ISystemSettingsPage>()
                .OrderBy(i => i.Order ?? 10)
                .ThenBy(i => i.Name);

            items.AddRange(pages);
        }

        /// <summary>
        /// Handles the Loaded event of the SettingsPage control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        void SettingsPage_Loaded(object sender, RoutedEventArgs e)
        {
            _themeManager.CurrentTheme.SetDefaultPageTitle();
        }
    }
}
