using MediaBrowser.Common;
using MediaBrowser.Common.Updates;
using MediaBrowser.Theater.Interfaces.Navigation;
using MediaBrowser.Theater.Interfaces.Presentation;
using MediaBrowser.Theater.Presentation.Pages;
using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;

namespace MediaBrowser.Theater.Core.Plugins
{
    /// <summary>
    /// Interaction logic for PluginsPage.xaml
    /// </summary>
    public partial class PluginsPage : BasePage
    {
        private readonly IPresentationManager _presentation;

        public PluginsPage(IApplicationHost appHost, INavigationService nav, IPresentationManager presentation, IInstallationManager installationManager)
        {
            _presentation = presentation;
            InitializeComponent();

            var viewModel = new PluginsPageViewModel(appHost, nav, installationManager, presentation);
            viewModel.PropertyChanged += viewModel_PropertyChanged;
            DataContext = viewModel;
        }

        void viewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (string.Equals(e.PropertyName, "CurrentSection"))
            {
                ScrollViewer.ScrollToLeftEnd();

                var current = MenuList.ItemContainerGenerator.ContainerFromItem(MenuList.SelectedItem) as ListBoxItem;

                if (current != null)
                {
                    current.Focus();
                }
            }
        }

        protected override async void OnInitialized(EventArgs e)
        {
            Loaded += HomePage_Loaded;

            base.OnInitialized(e);
        }

        void HomePage_Loaded(object sender, RoutedEventArgs e)
        {
            _presentation.SetDefaultPageTitle();
        }
    }
}
