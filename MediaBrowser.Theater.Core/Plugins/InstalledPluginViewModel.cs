using MediaBrowser.Common;
using MediaBrowser.Common.Plugins;
using MediaBrowser.Common.Updates;
using MediaBrowser.Model.Updates;
using MediaBrowser.Theater.Interfaces.Navigation;
using MediaBrowser.Theater.Interfaces.Plugins;
using MediaBrowser.Theater.Interfaces.Presentation;
using MediaBrowser.Theater.Interfaces.Theming;
using MediaBrowser.Theater.Interfaces.ViewModels;
using System;
using System.Windows;
using System.Windows.Input;

namespace MediaBrowser.Theater.Core.Plugins
{
    public class InstalledPluginViewModel : BaseViewModel
    {
        private readonly IPresentationManager _presentationManager;
        private readonly IInstallationManager _installationManager;
        private readonly INavigationService _nav;

        private readonly IApplicationHost _appHost;
        
        public ICommand UninstallCommand { get; private set; }

        public string Name
        {
            get { return Plugin.Name; }
        }

        public string Version
        {
            get { return Plugin.Version.ToString(); }
        }

        public Uri ImageUri
        {
            get
            {
                var hasImage = Plugin as IHasThumbImage;

                if (hasImage != null)
                {
                    return hasImage.ThumbUri;
                }

                return null;
            }
        }

        public Visibility ImageVisibility
        {
            get
            {
                var hasImage = Plugin as IHasThumbImage;

                return hasImage != null ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        public Visibility DefaultImageVisibility
        {
            get
            {
                var hasImage = Plugin as IHasThumbImage;

                return hasImage != null ? Visibility.Collapsed : Visibility.Visible;
            }
        }

        private IPlugin _plugin;
        public IPlugin Plugin
        {
            get { return _plugin; }
            set
            {
                _plugin = value;
                OnPropertyChanged("Plugin");
                OnPropertyChanged("Name");
                OnPropertyChanged("Version");
                OnPropertyChanged("ImageUri");
                OnPropertyChanged("ImageVisibility");
                OnPropertyChanged("DefaultImageVisibility");
            }
        }

        public InstalledPluginViewModel(IApplicationHost appHost, INavigationService nav, IInstallationManager installationManager, IPresentationManager presentationManager)
        {
            _appHost = appHost;
            _nav = nav;
            _installationManager = installationManager;
            _presentationManager = presentationManager;
            UninstallCommand = new RelayCommand(o => Uninstall());
        }

        private async void Uninstall()
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
                _installationManager.UninstallPlugin(_plugin);

                await _nav.Navigate(new PluginsPage(_appHost, _nav, _presentationManager, _installationManager));

                // Remove both this page and the previous page from the history
                _nav.RemovePagesFromHistory(2);
            }
        }

        public void SaveConfiguration()
        {
            _plugin.SaveConfiguration();
        }
    }
}
