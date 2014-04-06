using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MediaBrowser.Theater.Api.Navigation
{
    public class Navigator
        : INavigator
    {
        private class NavigationFrame
        {
            public NavigationFrame Parent { get; set; }
            public INavigationContext Context { get; set; }
            public INavigationPath Path { get; set; }
        }

        public INavigationPath CurrentLocation
        {
            get { return _activeFrame.Path; }
        }

        public bool CanGoBack
        {
            get { return _logicalBackStack.Count > 0; }
        }

        public bool CanGoForward
        {
            get { return _logicalForwardStack.Count > 0; }
        }

        private readonly Stack<NavigationFrame> _logicalBackStack;
        private readonly Stack<NavigationFrame> _logicalForwardStack;
        private readonly AsyncLock _navigationLock;

        private NavigationFrame _activeFrame;

        public Navigator()
        {
            _logicalBackStack = new Stack<NavigationFrame>();
            _logicalForwardStack = new Stack<NavigationFrame>();
            _navigationLock = new AsyncLock();
        }

        public event EventHandler<NavigationEventArgs> Navigated;

        protected virtual void OnNavigated(NavigationEventArgs e)
        {
            EventHandler<NavigationEventArgs> handler = Navigated;
            if (handler != null) {
                handler(this, e);
            }
        }

        public Task Initialize(INavigationContext rootContext)
        {
            ClearNavigationHistory();
            _activeFrame = new NavigationFrame { Context = rootContext };

            return _activeFrame.Context.Activate();
        }

        public async Task Navigate(INavigationPath path)
        {
            // major problem here: the lock is not re-entrant, and navigating while inside a navigation will deadlock the thread (waiting for itself to exit)
            // todo change navigation code to cancel old nav requests when a new one is started, instead of locking
            using (await _navigationLock.LockAsync().ConfigureAwait(false)) {

                // we give the navigation request to the current navigation context first, walking down the stack until we find a handler
                for (var frame = _activeFrame; frame != null; frame = frame.Parent) {
                    var resolution = frame.Context.HandleNavigation(path);
                    if (resolution != null) {
                        await ExecuteNavigationAction(resolution).ConfigureAwait(false);
                        OnNavigated(new NavigationEventArgs(CurrentLocation));
                        break;
                    }
                }
            }
        }

        private async Task ExecuteNavigationAction(PathResolution resolution)
        {
            var newFrames = (await FindNavigationFrames(resolution).ConfigureAwait(false)).ToList();

            if (newFrames.Count > 0) {
                PushCurrentFrameOntoStack();
            }

            foreach (var frame in newFrames) {
                // make frame the new active frame
                frame.Parent = _activeFrame;
                _activeFrame = frame;

                // activate the context
                await frame.Context.Activate().ConfigureAwait(false);
            }
        }

        private void PushCurrentFrameOntoStack()
        {
            if (_activeFrame != null) {
                _logicalBackStack.Push(_activeFrame);
                _logicalForwardStack.Clear();
            }
        }

        private async Task<IEnumerable<NavigationFrame>> FindNavigationFrames(PathResolution resolution)
        {
            var additionalFrames = new List<NavigationFrame>();

            // keep resolving for as long as we get new contexts
            while (resolution != null) {
                var context = await resolution.Execute().ConfigureAwait(false);
                if (context == null) {
                    break;
                }

                // add a new navigation frame to represent the context
                additionalFrames.Add(new NavigationFrame {
                    Context = context,
                    Path = resolution.MatchingPath
                });

                var excess = resolution.ExcessPath.LastOrDefault();
                if (excess != null) {
                    // only part of the path was handled, so ask the new context if it can handle the rest
                    resolution = context.HandleNavigation(excess);
                } else {
                    // the full path has been handled, so exit
                    break;
                }
            }

            return additionalFrames;
        }

        public async Task Back()
        {
            if (_logicalBackStack.Count > 0) {
                _logicalForwardStack.Push(_activeFrame);
                _activeFrame = _logicalBackStack.Pop();

                await _activeFrame.Context.Activate();
            }
        }

        public async Task Forward()
        {
            if (_logicalForwardStack.Count > 0) {
                _logicalBackStack.Push(_activeFrame);
                _activeFrame = _logicalForwardStack.Pop();

                await _activeFrame.Context.Activate();
            }
        }

        public void ClearNavigationHistory(int count = Int32.MaxValue)
        {
            _logicalForwardStack.Clear();

            for (int i = 0; i < count && _logicalBackStack.Count > 0; i++) {
                _logicalBackStack.Pop();
            }
        }
    }
}
