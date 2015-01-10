using System;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace MediaBrowser.Theater.Api.UserInterface
{
    public static class UIDispatchExtensions
    {
        private static readonly Task Completed = Task.FromResult<object>(null);
        private static Dispatcher _dispatcher;

        private static Dispatcher Dispatcher
        {
            get { return _dispatcher ?? (_dispatcher = Dispatcher.CurrentDispatcher); }
        }

        public static void ResetDispatcher()
        {
            _dispatcher = Dispatcher.CurrentDispatcher;
        }
        
        /// <summary>
        ///     Executes the specified action on the UI thread.
        /// </summary>
        /// <param name="action">The action to execute.</param>
        public static void OnUiThread(this Action action)
        {
            if (Dispatcher == null || Dispatcher.CheckAccess())
                action();
            else {
                try {
                    Dispatcher.Invoke(action);
                }
                catch (TaskCanceledException) {
                }
            }
        }

        /// <summary>
        ///     Asynchronously executes the specified action on the UI thread.
        /// </summary>
        /// <param name="action">The action to execute.</param>
        /// <returns>A task representing the asychronous operation.</returns>
        public static Task OnUiThreadAsync(this Action action)
        {
            if (Dispatcher == null || Dispatcher.CheckAccess())
            {
                action();
                return Completed;
            }

            var completionSource = new TaskCompletionSource<object>();

            Action wrapper = () =>
            {
                try
                {
                    action();
                    completionSource.SetResult(null);
                }
                catch (Exception e)
                {
                    completionSource.SetException(e);
                }
            };

            Dispatcher.BeginInvoke(wrapper);
            return completionSource.Task;
        }

        /// <summary>
        ///     Asynchronously executes the specified action on the UI thread.
        /// </summary>
        /// <param name="action">The action to execute.</param>
        /// <returns>A task representing the asychronous operation.</returns>
        public static Task<T> OnUiThreadAsync<T>(this Func<T> action)
        {
            if (Dispatcher == null || Dispatcher.CheckAccess()) {
                var result = action();
                return Task.FromResult(result);
            }

            var completionSource = new TaskCompletionSource<T>();

            Action wrapper = () =>
            {
                try
                {
                    var result = action();
                    completionSource.SetResult(result);
                }
                catch (Exception e)
                {
                    completionSource.SetException(e);
                }
            };

            Dispatcher.BeginInvoke(wrapper);
            return completionSource.Task;
        }
    }
}