using System;
using System.Windows.Data;
using MediaBrowser.Model.Updates;
using System.Collections.Generic;
using System.Windows.Controls;
using MediaBrowser.Theater.Presentation.ViewModels;

namespace MediaBrowser.UI.Pages.Plugins
{
    /// <summary>
    /// Interaction logic for PluginCategory.xaml
    /// </summary>
    public partial class PluginCategory : UserControl
    {
        private IEnumerable<PackageInfo> _packages;

        public PluginCategory(string name, IEnumerable<PackageInfo> packages)
        {
            _packages = packages;
            
            InitializeComponent();

            TxtName.Text = name;
        }

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);

            var items = new RangeObservableCollection<PackageInfo>();
            var view = (ListCollectionView)CollectionViewSource.GetDefaultView(items);
            LstItems.ItemsSource = view;

            items.AddRange(_packages);

            view.CurrentChanged += view_CurrentChanged;
        }

        void view_CurrentChanged(object sender, EventArgs e)
        {
        }
    }
}
