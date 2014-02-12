using System.Threading.Tasks;
using System.Windows;
using MediaBrowser.Common.Configuration;
using MediaBrowser.Common.Plugins;
using MediaBrowser.Model.Serialization;
using MediaBrowser.Theater.Api.UserInterface;
using MediaBrowser.Theater.Api.UserInterface.Navigation;
using MediaBrowser.Theater.Api.UserInterface.ViewModels;
using MediaBrowser.Theater.DefaultTheme.Configuration;
using MediaBrowser.Theater.DefaultTheme.ViewModels;
using MediaBrowser.Theater.Presentation.ViewModels;

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
            ApplyPalette(_application);

            UIDispatchExtensions.ResetDispatcher();
            
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
        
        private void ApplyPalette(Application app)
        {
            app.Resources["AccentColor"] = Configuration.Palette.Accent;
            app.Resources["ForegroundColor"] = Configuration.Palette.Style == ThemeStyle.Light ? ColorPalette.LightForeground : ColorPalette.DarkForeground;
            app.Resources["BackgroundColor"] = Configuration.Palette.Style == ThemeStyle.Light ? ColorPalette.LightBackground : ColorPalette.DarkBackground;
        }
    }
}