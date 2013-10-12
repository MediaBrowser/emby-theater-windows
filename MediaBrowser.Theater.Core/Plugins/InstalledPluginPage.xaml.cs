using MediaBrowser.Model.Updates;
using MediaBrowser.Theater.Interfaces.Presentation;
using MediaBrowser.Theater.Presentation.Controls;
using MediaBrowser.Theater.Presentation.Pages;
using System;
using System.Collections.Generic;
using System.Windows;
using MediaBrowser.Theater.Presentation.ViewModels;

namespace MediaBrowser.Theater.Core.Plugins
{
    /// <summary>
    /// Interaction logic for InstalledPluginPage.xaml
    /// </summary>
    public partial class InstalledPluginPage : BasePage
    {
        private readonly InstalledPluginViewModel _plugin;
        private readonly IPresentationManager _presentationManager;

        public InstalledPluginPage(InstalledPluginViewModel plugin, IPresentationManager presentationManager)
        {
            _plugin = plugin;
            _presentationManager = presentationManager;

            InitializeComponent();

            DataContext = plugin;
        }

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);

            SelectUpdateLevel.Options = new List<SelectListItem> 
            { 
                 new SelectListItem{ Text = "Dev (Unstable)", Value = PackageVersionClass.Dev.ToString()},
                 new SelectListItem{ Text = "Beta", Value = PackageVersionClass.Beta.ToString()},
                 new SelectListItem{ Text = "Official Release", Value = PackageVersionClass.Release.ToString()}
            };

            Loaded += InstalledPluginPage_Loaded;
            Unloaded += InstalledPluginPage_Unloaded;
        }

        void InstalledPluginPage_Loaded(object sender, RoutedEventArgs e)
        {
            _presentationManager.SetDefaultPageTitle();

            SelectUpdateLevel.SelectedValue = _plugin.UpdateClass.ToString();
        }

        void InstalledPluginPage_Unloaded(object sender, RoutedEventArgs e)
        {
            PackageVersionClass updateLevel;

            if (Enum.TryParse(SelectUpdateLevel.SelectedValue, out updateLevel))
            {
                _plugin.UpdateClass = updateLevel;
            }

            _plugin.SaveConfiguration();
        }
    }
}
