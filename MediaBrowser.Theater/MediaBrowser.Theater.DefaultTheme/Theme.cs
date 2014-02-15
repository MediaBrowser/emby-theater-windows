using System.Threading.Tasks;
using System.Windows;
using MediaBrowser.Common;
using MediaBrowser.Common.Configuration;
using MediaBrowser.Common.Plugins;
using MediaBrowser.Model.Serialization;
using MediaBrowser.Theater.Api.Events;
using MediaBrowser.Theater.Api.UserInterface;
using MediaBrowser.Theater.Api.UserInterface.Navigation;
using MediaBrowser.Theater.DefaultTheme.Configuration;
using MediaBrowser.Theater.DefaultTheme.Navigation;

namespace MediaBrowser.Theater.DefaultTheme
{
    public class Theme
        : BasePlugin<PluginConfiguration>, ITheme
    {
        private readonly INavigator _navigator;
        private readonly Presenter _presenter;
        private readonly TaskCompletionSource<object> _running;
        private App _application;

        public Theme(IApplicationPaths applicationPaths, IXmlSerializer xmlSerializer, IEventAggregator events, IApplicationHost appHost) : base(applicationPaths, xmlSerializer)
        {
            _running = new TaskCompletionSource<object>();
            _presenter = new Presenter(events);
            _navigator = new RootNavigationManager(new RootContext(appHost));
        }

        public override string Name
        {
            get { return "Default Theme"; }
        }

        public IPresenter Presenter
        {
            get { return _presenter; }
        }

        public INavigator Navigator
        {
            get { return _navigator; }
        }

        public void Run()
        {
            _application = new App();
            ApplyPalette(_application);

            UIDispatchExtensions.ResetDispatcher();

            var mainWindow = _presenter.CreateMainWindow(Configuration);
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
            _presenter.SaveWindowPosition(Configuration);
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