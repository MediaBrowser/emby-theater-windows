using System.Threading.Tasks;
using MediaBrowser.Common;

namespace MediaBrowser.Theater.Api.UserInterface.Navigation
{
    public abstract class NavigationContext
        : INavigationContext
    {
        private readonly PathBinder _binder;

        protected NavigationContext(IApplicationHost appHost)
        {
            _binder = new PathBinder(appHost);
        }

        public PathBinder Binder
        {
            get { return _binder; }
        }

        public abstract Task Activate();

        public virtual PathResolution HandleNavigation(INavigationPath path)
        {
            return Binder.Resolve(path);
        }
    }
}