using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediaBrowser.Theater.Api.UserInterface.Navigation;

namespace MediaBrowser.Theater.DefaultTheme.Navigation
{
    public class NavigationService
        : INavigationService
    {
        private struct LogicalContext
        {
            public INavigationContext Context { get; set; }
            public INavigationPath Path { get; set; }
            public bool IsManualNavigation { get; set; }
        }

        public INavigationPath CurrentLocation { get; private set; }
        public bool CanGoBack { get; private set; }
        public bool CanGoForward { get; private set; }

        private readonly List<LogicalContext> _contextStack;
        private readonly List<LogicalContext> _forwardContextStack;

        public NavigationService()
        {
            _contextStack = new List<LogicalContext>();
            _forwardContextStack = new List<LogicalContext>();
        }

        public Task Navigate(INavigationPath path)
        {
            // from top of context stack down to root, until handler found
            //   ask context to handle navigation
            //   if a context returns a resolved path, execute and if new context is not null
            //     push new context onto stack together with IsManualNavigation=true and activate
            //     recursively resolve final excess path against top of stack and push (and activate) context until path is no longer resolved
            //       push with IsManualNavigation=false

            throw new NotImplementedException();
        }

        public Task Back()
        {
            // pop stack until a context with IsManualNavigation is found, pushing onto forward stack
            //   activate context
            // if no context found
            //   navigate to /home

            throw new NotImplementedException();
        }

        public Task Forward()
        {
            // pop forward stack until IsManualNavigation is found, pushing onto context stack
            //   activate context

            throw new NotImplementedException();
        }

        public void ClearNavigationHistory(int count = Int32.MaxValue)
        {
            // clear forward stack
            // remove count items from stack

            throw new NotImplementedException();
        }
    }

    public class PathResolution
    {
        public INavigationContext Execute()
        {
            throw new NotImplementedException();
        }

        public IEnumerable<INavigationPath> ExcessPath { get; private set; }
    }

    public class PathBinder
    {
        public void Bind<T>(Func<T, INavigationContext> handler)
        {
            throw new NotImplementedException();
        }

        public PathResolution Resolve(INavigationContext path)
        {
            throw new NotImplementedException();
        }
    }

    public interface INavigationContext
    {
        Task Activate();
        PathResolution HandleNavigation(INavigationPath path);
    }
}
