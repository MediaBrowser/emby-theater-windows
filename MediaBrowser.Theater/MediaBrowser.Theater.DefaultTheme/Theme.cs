using System;
using System.Threading.Tasks;
using System.Windows;
using MediaBrowser.Common.Configuration;
using MediaBrowser.Common.Plugins;
using MediaBrowser.Model.Serialization;
using MediaBrowser.Theater.Api.UserInterface;
using MediaBrowser.Theater.Api.UserInterface.Navigation;
using MediaBrowser.Theater.DefaultTheme.Configuration;
using MediaBrowser.Theater.DefaultTheme.ViewModels;

namespace MediaBrowser.Theater.DefaultTheme
{
    public class Theme
        : BasePlugin<PluginConfiguration>, ITheme
    {
        private readonly TaskCompletionSource<object> _running;
        private App _application;
        private readonly PresentationManager _presentation;

        public Theme(IApplicationPaths applicationPaths, IXmlSerializer xmlSerializer, PresentationManager presentationManager) : base(applicationPaths, xmlSerializer)
        {
            _running = new TaskCompletionSource<object>();
            _presentation = presentationManager;
        }

        public override string Name
        {
            get { return "Default Theme"; }
        }

        public IPresentationManager Presentation
        {
            get { return _presentation; }
        }

        public INavigationService Navigation { get; private set; }

        public void Run()
        {
            _application = new App();
            ApplyPalette(_application);

            UIDispatchExtensions.ResetDispatcher();

            var mainWindow = _presentation.CreateMainWindow(Configuration);
            _application.Run(mainWindow);

            Cleanup();

            _running.SetResult(null);
        }

        public Task Shutdown()
        {
            if (_application != null) {
                _application.Shutdown();
            } else {
                _running.SetResult(null);
            }

            return _running.Task;
        }

        private void Cleanup()
        {
            _presentation.SaveWindowPosition(Configuration);
            SaveConfiguration();
        }

        private void ApplyPalette(Application app)
        {
            ResourceDictionary resources = Configuration.Palette.GetResources();
            foreach (object resource in resources.Keys) {
                app.Resources[resource] = resources[resource];
            }
        }
    }
}