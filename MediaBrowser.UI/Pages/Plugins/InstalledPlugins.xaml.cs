using System.Windows;
using MediaBrowser.Common;
using MediaBrowser.Theater.Presentation.ViewModels;
using System;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Data;

namespace MediaBrowser.UI.Pages.Plugins
{
    /// <summary>
    /// Interaction logic for InstalledPlugins.xaml
    /// </summary>
    public partial class InstalledPlugins : UserControl
    {
        private readonly IApplicationHost _appHost;

        public InstalledPlugins(IApplicationHost appHost)
        {
            _appHost = appHost;
            InitializeComponent();
        }

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);

            var items = new RangeObservableCollection<InstalledPluginViewModel>();
            var view = (ListCollectionView)CollectionViewSource.GetDefaultView(items);
            LstItems.ItemsSource = view;

            items.AddRange(_appHost.Plugins.Select(i => new InstalledPluginViewModel
            {
                Name = i.Name,
                Version = i.Version.ToString(),
                ImageVisibility = Visibility.Collapsed,
                DefaultImageVisibility = Visibility.Visible
            }));
        }
    }

    public class InstalledPluginViewModel
    {
        public string Name { get; set; }

        public string Version { get; set; }

        public Uri ImageUri { get; set; }

        public Visibility ImageVisibility { get; set; }

        public Visibility DefaultImageVisibility { get; set; }
    }
}
