using MediaBrowser.Common;
using MediaBrowser.Common.Plugins;
using MediaBrowser.Theater.Interfaces.Navigation;
using MediaBrowser.Theater.Interfaces.Plugins;
using MediaBrowser.Theater.Presentation.Controls;
using MediaBrowser.Theater.Presentation.ViewModels;
using System;
using System.Linq;
using System.Windows;
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
        private readonly INavigationService _nav;

        public InstalledPlugins(IApplicationHost appHost, INavigationService nav)
        {
            _appHost = appHost;
            _nav = nav;
            InitializeComponent();
        }

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);

            var items = new RangeObservableCollection<InstalledPluginViewModel>();
            var view = (ListCollectionView)CollectionViewSource.GetDefaultView(items);
            LstItems.ItemsSource = view;

            items.AddRange(_appHost.Plugins.Select(i =>
            {
                var hasThumb = i as IHasThumbImage;

                return new InstalledPluginViewModel
                {
                    Name = i.Name,
                    Version = i.Version.ToString(),
                    ImageVisibility = hasThumb == null ? Visibility.Collapsed : Visibility.Visible,
                    DefaultImageVisibility = hasThumb == null ? Visibility.Visible : Visibility.Collapsed,
                    ImageUri = hasThumb == null ? null : hasThumb.ThumbUri,
                    Plugin = i
                };
            }));

            LstItems.ItemInvoked += LstItems_ItemInvoked;
        }

        async void LstItems_ItemInvoked(object sender, ItemEventArgs<object> e)
        {
            var viewModel = e.Argument as InstalledPluginViewModel;

            await _nav.Navigate(new InstalledPluginPage(viewModel));
        }
    }

    public class InstalledPluginViewModel
    {
        public string Name { get; set; }

        public string Version { get; set; }

        public Uri ImageUri { get; set; }

        public Visibility ImageVisibility { get; set; }

        public Visibility DefaultImageVisibility { get; set; }

        public IPlugin Plugin { get; set; }
    }
}
