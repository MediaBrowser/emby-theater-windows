using System.Threading.Tasks;
using MediaBrowser.Common.Configuration;
using MediaBrowser.Common.Plugins;
using MediaBrowser.Model.Serialization;
using MediaBrowser.Theater.Api.Theming;
using MediaBrowser.Theater.DefaultTheme.Configuration;

namespace MediaBrowser.Theater.DefaultTheme
{
    public class Theme
        : BasePlugin<PluginConfiguration>, ITheme
    {
        private readonly App _application;
        private readonly TaskCompletionSource<object> _running;

        public Theme(IApplicationPaths applicationPaths, IXmlSerializer xmlSerializer) : base(applicationPaths, xmlSerializer)
        {
            _running = new TaskCompletionSource<object>();
            _application = new App();
        }

        public override string Name
        {
            get { return "Default Theme"; }
        }

        public void Run()
        {
            _application.Run();
            _running.SetResult(null);
        }

        public Task Shutdown()
        {
            _application.Shutdown();
            return _running.Task;
        }
    }
}