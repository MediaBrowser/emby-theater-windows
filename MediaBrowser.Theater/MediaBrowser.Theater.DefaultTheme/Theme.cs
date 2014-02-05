using System.Threading.Tasks;
using MediaBrowser.Theater.Api.Theming;

namespace MediaBrowser.Theater.DefaultTheme
{
    public class Theme
        : ITheme
    {
        private readonly App _application;
        private readonly TaskCompletionSource<object> _running;

        public Theme()
        {
            _running = new TaskCompletionSource<object>();
            _application = new App();
        }

        public string Name
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