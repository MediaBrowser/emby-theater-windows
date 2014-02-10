using System.Threading.Tasks;
using MediaBrowser.Common.Configuration;
using MediaBrowser.Common.Plugins;
using MediaBrowser.Model.Serialization;
using MediaBrowser.Theater.Api.Theming;
using MediaBrowser.Theater.Api.Theming.Navigation;
using MediaBrowser.Theater.DefaultTheme.Configuration;
using MediaBrowser.Theater.DefaultTheme.ViewModels;

namespace MediaBrowser.Theater.DefaultTheme
{
    public class Theme
        : BasePlugin<PluginConfiguration>, ITheme
    {
        private readonly RootViewModel _rootViewModel;
        private readonly TaskCompletionSource<object> _running;
        private App _application;

        public Theme(IApplicationPaths applicationPaths, IXmlSerializer xmlSerializer, PresentationManager presentationManager, RootViewModel rootViewModel) : base(applicationPaths, xmlSerializer)
        {
            _rootViewModel = rootViewModel;
            _running = new TaskCompletionSource<object>();
            Presentation = presentationManager;
        }

        public override string Name
        {
            get { return "Default Theme"; }
        }

        public IPresentationManager Presentation { get; private set; }
        public INavigationService Navigation { get; private set; }

        public void Run()
        {
            _application = new App();

            _application.Run(new MainWindow { DataContext = _rootViewModel });
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
    }
}