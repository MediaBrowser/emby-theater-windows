using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MediaBrowser.Theater.Api;
using MediaBrowser.Theater.Api.UserInterface.Navigation;

namespace MediaBrowser.Theater.DefaultTheme.Navigation
{
    public class NavigationService
        : INavigationService
    {
        private struct NavigationFrame
        {
            public INavigationContext Context { get; set; }
            public INavigationPath Path { get; set; }
            public bool IsUserNavigation { get; set; }
        }

        public INavigationPath CurrentLocation { get; private set; }
        public bool CanGoBack { get; private set; }
        public bool CanGoForward { get; private set; }

        private readonly Stack<NavigationFrame> _contextStack;
        private readonly Stack<NavigationFrame> _logicalBackStack;
        private readonly Stack<NavigationFrame> _logicalForwardStack;
        private readonly AsyncLock _navigationLock;

        public NavigationService()
        {
            _contextStack = new Stack<NavigationFrame>();
            _logicalBackStack = new Stack<NavigationFrame>();
            _logicalForwardStack = new Stack<NavigationFrame>();
            _navigationLock = new AsyncLock();
        }

        public async Task Navigate(INavigationPath path)
        {
            using (await _navigationLock.LockAsync().ConfigureAwait(false)) {

                // we give the navigation request to the current navigation context first, walking down the stack until we find a handler
                foreach (var frame in _contextStack) {
                    var resolution = frame.Context.HandleNavigation(path);

                    if (resolution != null) {
                        await ExecuteNavigationAction(path, resolution).ConfigureAwait(false);
                        break;
                    }
                }
            }
        }
        
        private async Task ExecuteNavigationAction(INavigationPath path, PathResolution resolution)
        {
            var newFrames = FindNavigationFrames(resolution).ToList();

            for (int i = 0; i < newFrames.Count; i++) {
                var frame = newFrames[i];

                // mark the last frame as a user navigation frame
                frame.IsUserNavigation = i == newFrames.Count - 1;

                // push new frames onto the stack
                _contextStack.Push(frame);

                // activate the context
                await frame.Context.Activate().ConfigureAwait(false);
            }
        }

        private IEnumerable<NavigationFrame> FindNavigationFrames(PathResolution resolution)
        {
            INavigationContext context;

            // execute the resolution for as long as we get new contexts
            while (resolution != null && (context = resolution.Execute()) != null) {

                // yield a new navigation frame to represent the context
                yield return new NavigationFrame {
                    Context = context,
                    Path = resolution.MatchingPath,
                    IsUserNavigation = false
                };

                var excess = resolution.ExcessPath.LastOrDefault();
                if (excess != null) {
                    // only part of the path was handled, so ask the new context if it can handle the rest
                    resolution = context.HandleNavigation(excess);
                } else {
                    // the full path has been handled, so exit
                    break;
                }
            }
        }

        public async Task Back()
        {
           
        }

        public async Task Forward()
        {
            
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
        public INavigationPath MatchingPath { get; private set; }
    }

    public class PathBinder
    {
        public void Bind<T>(Func<T, INavigationContext> handler)
        {
            throw new NotImplementedException();
        }

        public PathResolution Resolve(INavigationContext path)
        {
            // look for bindings with an exact match to the path type
            // look for bindings for a base class path type
            // repeat with the path parent

            throw new NotImplementedException();
        }
    }

    public interface INavigationContext
    {
        Task Activate();
        PathResolution HandleNavigation(INavigationPath path);
    }
}
