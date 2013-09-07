using MediaBrowser.Common;
using MediaBrowser.Common.Plugins;
using MediaBrowser.Common.Updates;
using MediaBrowser.Theater.Interfaces;
using MediaBrowser.Theater.Interfaces.Navigation;
using MediaBrowser.Theater.Interfaces.Plugins;
using MediaBrowser.Theater.Interfaces.Presentation;
using MediaBrowser.Theater.Interfaces.ViewModels;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace MediaBrowser.Theater.Core.Plugins
{
    /// <summary>
    /// Interaction logic for InstalledPlugins.xaml
    /// </summary>
    public partial class InstalledPlugins : UserControl
    {
        private readonly IApplicationHost _appHost;
        private readonly INavigationService _nav;
        private readonly IPresentationManager _presentationManager;
        private readonly IInstallationManager _installationManager;

        public InstalledPlugins(IApplicationHost appHost, INavigationService nav, IPresentationManager presentationManager, IInstallationManager installationManager)
        {
            _appHost = appHost;
            _nav = nav;
            _presentationManager = presentationManager;
            _installationManager = installationManager;
            InitializeComponent();
        }

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);

            LstItems.ItemInvoked += LstItems_ItemInvoked;

            LoadPlugins();
        }

        private void LoadPlugins()
        {
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

            if (items.Count == 0)
            {
                TxtNoPlugins.Visibility = Visibility.Visible;
                LstItems.Visibility = Visibility.Collapsed;
            }
            else
            {
                TxtNoPlugins.Visibility = Visibility.Collapsed;
                LstItems.Visibility = Visibility.Visible;
            }
        }

        async void LstItems_ItemInvoked(object sender, ItemEventArgs<object> e)
        {
            var viewModel = e.Argument as InstalledPluginViewModel;

            await _nav.Navigate(new InstalledPluginPage(viewModel, _presentationManager, _installationManager, _nav, _appHost));
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
