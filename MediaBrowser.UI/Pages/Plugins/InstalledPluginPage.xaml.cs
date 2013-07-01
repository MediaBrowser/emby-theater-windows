using MediaBrowser.Common;
using MediaBrowser.Common.Updates;
using MediaBrowser.Model.Updates;
using MediaBrowser.Theater.Interfaces.Navigation;
using MediaBrowser.Theater.Interfaces.Presentation;
using MediaBrowser.Theater.Interfaces.Theming;
using MediaBrowser.Theater.Presentation.Controls;
using MediaBrowser.Theater.Presentation.Pages;
using System;
using System.Collections.Generic;
using System.Windows;

namespace MediaBrowser.UI.Pages.Plugins
{
    /// <summary>
    /// Interaction logic for InstalledPluginPage.xaml
    /// </summary>
    public partial class InstalledPluginPage : BasePage
    {
        private readonly InstalledPluginViewModel _plugin;
        private readonly IPresentationManager _presentationManager;
        private readonly IInstallationManager _installationManager;
        private readonly INavigationService _nav;

        private readonly IPackageManager _packageManager;
        private readonly IApplicationHost _appHost;

        public InstalledPluginPage(InstalledPluginViewModel plugin, IPresentationManager presentationManager, IInstallationManager installationManager, INavigationService nav, IPackageManager packageManager, IApplicationHost appHost)
        {
            _plugin = plugin;
            _presentationManager = presentationManager;
            _installationManager = installationManager;
            _nav = nav;
            _packageManager = packageManager;
            _appHost = appHost;

            InitializeComponent();
        }

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);

            SelectUpdateLevel.Options = new List<SelectListItem> 
            { 
                new SelectListItem{ Text = "Official Release", Value = PackageVersionClass.Release.ToString()},
                new SelectListItem{ Text = "Beta", Value = PackageVersionClass.Beta.ToString()},
                new SelectListItem{ Text = "Dev", Value = PackageVersionClass.Dev.ToString()}
            };

            Loaded += InstalledPluginPage_Loaded;
            Unloaded += InstalledPluginPage_Unloaded;
            BtnUninstall.Click += BtnUninstall_Click;
        }

        async void BtnUninstall_Click(object sender, RoutedEventArgs e)
        {
            var msgResult = _presentationManager.ShowMessage(new MessageBoxInfo
            {
                Button = MessageBoxButton.OKCancel,
                Caption = "Confirm Uninstallation",
                Icon = MessageBoxIcon.Warning,
                Text = "Are you sure you wish to uninstall " + _plugin.Name + "?"
            });

            if (msgResult == MessageBoxResult.OK)
            {
                _installationManager.UninstallPlugin(_plugin.Plugin);

                await _nav.Navigate(new PluginsPage(_packageManager, _appHost, _nav, _presentationManager, _installationManager));

                // Remove both this page and the previous page from the history
                _nav.RemovePagesFromHistory(2);
            }
        }

        void InstalledPluginPage_Loaded(object sender, RoutedEventArgs e)
        {
            _presentationManager.SetDefaultPageTitle();
            _presentationManager.ClearBackdrops();

            SelectUpdateLevel.SelectedValue = _plugin.Plugin.Configuration.UpdateClass.ToString();

            TxtName.Text = _plugin.Name;
            TxtVersion.Text = "Version " + _plugin.Version;
        }

        void InstalledPluginPage_Unloaded(object sender, RoutedEventArgs e)
        {
            PackageVersionClass updateLevel;

            if (Enum.TryParse(SelectUpdateLevel.SelectedValue, out updateLevel))
            {
                _plugin.Plugin.Configuration.UpdateClass = updateLevel;
            }

            _plugin.Plugin.SaveConfiguration();
        }
    }
}
