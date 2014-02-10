using System;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace MediaBrowser.Theater.Api.Theming.ViewModels
{
    public static class UIDispatchExtensions
    {
        private static readonly Task Completed = Task.FromResult<object>(null);
        private static Dispatcher _dispatcher;

        private static Dispatcher Dispatcher
        {
            get { return _dispatcher ?? (_dispatcher = Dispatcher.CurrentDispatcher); }
        }

        /// <summary>
        ///     Executes the specified action on the UI thread.
        /// </summary>
        /// <param name="action">The action to execute.</param>
        public static void OnUiThread(this Action action)
        {
            if (Dispatcher == null || Dispatcher.CheckAccess())
                action();
            else
                Dispatcher.Invoke(action);
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
    }
}