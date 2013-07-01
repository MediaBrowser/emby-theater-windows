using MediaBrowser.Model.Updates;
using MediaBrowser.Theater.Interfaces.Navigation;
using MediaBrowser.Theater.Presentation.Controls;
using MediaBrowser.Theater.Presentation.ViewModels;
using System;
using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows.Data;

namespace MediaBrowser.UI.Pages.Plugins
{
    /// <summary>
    /// Interaction logic for PluginCategory.xaml
    /// </summary>
    public partial class PluginCategory : UserControl
    {
        private readonly IEnumerable<PackageInfo> _packages;
        private readonly INavigationService _nav;

        public PluginCategory(string name, IEnumerable<PackageInfo> packages, INavigationService nav)
        {
            _packages = packages;
            _nav = nav;

            InitializeComponent();

            TxtName.Text = name;
        }

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);

            LstItems.ItemInvoked += LstItems_ItemInvoked;

            var items = new RangeObservableCollection<PackageInfo>();
            var view = (ListCollectionView)CollectionViewSource.GetDefaultView(items);
            LstItems.ItemsSource = view;

            items.AddRange(_packages);
        }

        async void LstItems_ItemInvoked(object sender, ItemEventArgs<object> e)
        {
            var packageInfo = (PackageInfo)e.Argument;

            await _nav.Navigate(new PackageInfoPage(packageInfo));
        }
    }
}
