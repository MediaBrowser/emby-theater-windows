using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using MediaBrowser.Common.Configuration;
using MediaBrowser.Common.Plugins;
using MediaBrowser.Model.Serialization;
using MediaBrowser.Theater.Api;
using MediaBrowser.Theater.Api.Events;
using MediaBrowser.Theater.Api.Navigation;
using MediaBrowser.Theater.Api.UserInterface;
using MediaBrowser.Theater.DefaultTheme.Configuration;
using MediaBrowser.Theater.DefaultTheme.Core;
using MediaBrowser.Theater.DefaultTheme.Core.ViewModels;

namespace MediaBrowser.Theater.DefaultTheme
{
    public class Theme
        : BasePlugin<PluginConfiguration>, ITheme
    {
        private readonly TaskCompletionSource<object> _running;
        private readonly ITheaterApplicationHost _appHost;
        private readonly WindowManager _windowManager;
        private readonly IEventAggregator _events;
        private readonly INavigator _navigator;
        private readonly RootContext _rootContext;
        private App _application;

        public static Theme Instance { get; private set; }

        public Theme(ITheaterApplicationHost appHost, IApplicationPaths applicationPaths, IXmlSerializer xmlSerializer, WindowManager windowManager, IEventAggregator events, INavigator navigator, RootContext rootContext)
            : base(applicationPaths, xmlSerializer)
        {
            _running = new TaskCompletionSource<object>();
            _appHost = appHost;
            _windowManager = windowManager;
            _events = events;
            _navigator = navigator;
            _rootContext = rootContext;

            Instance = this;
        }

        public override string Name
        {
            get { return "Default Theme"; }
        }

        public void Run()
        {
            _application = new App();
            ApplyPalette(_application);

            UIDispatchExtensions.ResetDispatcher();

            MainWindow mainWindow = _windowManager.CreateMainWindow(Configuration, new RootViewModel(_events, _navigator, _appHost, _rootContext));
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
            _windowManager.SaveWindowPosition(Configuration);
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