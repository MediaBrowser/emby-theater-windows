using MediaBrowser.Model.Updates;
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

        public InstalledPluginPage(InstalledPluginViewModel plugin)
        {
            _plugin = plugin;

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

        void BtnUninstall_Click(object sender, RoutedEventArgs e)
        {
        }

        void InstalledPluginPage_Loaded(object sender, RoutedEventArgs e)
        {
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
