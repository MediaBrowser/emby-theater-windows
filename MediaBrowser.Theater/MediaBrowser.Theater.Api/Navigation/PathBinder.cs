using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MediaBrowser.Common;

namespace MediaBrowser.Theater.Api.Navigation
{
    public class PathBinder
    {
        private readonly IApplicationHost _appHost;
        private readonly Dictionary<Type, Func<INavigationPath, Task<INavigationContext>>> _handlers = new Dictionary<Type, Func<INavigationPath, Task<INavigationContext>>>();

        public PathBinder(IApplicationHost appHost)
        {
            _appHost = appHost;
        }

        public void Bind<T>(Func<T, Task<INavigationContext>> handler)
            where T : INavigationPath
        {
            var key = typeof (T);
            _handlers[key] = path => handler((T)path);
        }
        
        public void Bind<TPath, TContext>()
            where TPath : INavigationPath
            where TContext : INavigationContext
        {
            Bind<TPath>(path => Task.FromResult<INavigationContext>(_appHost.Resolve<TContext>()));
        }

        public void Bind<TPath, TContext>(Action<TPath, TContext> initialize)
            where TPath : INavigationPath
            where TContext : INavigationContext
        {
            Bind<TPath>(path => {
                var context = _appHost.Resolve<TContext>();
                initialize(path, context);

                return Task.FromResult<INavigationContext>(context);
            });
        }

        public PathResolution Resolve(INavigationPath path)
        {
            return Resolve(path, new List<INavigationPath>());
        }

        private PathResolution Resolve(INavigationPath path, IEnumerable<INavigationPath> excess)
        {
            if (path == null) {
                return null;
            }

            var key = path.GetType();
            Func<INavigationPath, Task<INavigationContext>> handler;
            if (_handlers.TryGetValue(key, out handler)) {
                return new PathResolution(handler, excess.Reverse(), path);
            }

            var withParent = path as INavigationPath<INavigationPath>;
            if (withParent != null) {
                excess = new List<INavigationPath>(excess) { path };
                return Resolve(withParent.Parent, excess);
            }

            return null;
        }
    }
}