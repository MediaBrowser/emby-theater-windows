using System;
using System.Collections.Generic;
using System.Linq;
using MediaBrowser.Theater.Api.UserInterface;

namespace MediaBrowser.Theater.Api.Events
{
    public class EventBus<T> : IEventBus<T>
    {
        private readonly List<Action<T>> _handlers = new List<Action<T>>();
        private readonly object _lock = new object();
        private readonly List<WeakReference<Action<T>>> _weakHandlers = new List<WeakReference<Action<T>>>();

        public void Subscribe(Action<T> handler, bool weak = true)
        {
            lock (_lock) {
                if (weak) {
                    _weakHandlers.Add(new WeakReference<Action<T>>(handler));
                } else {
                    _handlers.Add(handler);
                }
            }
        }

        public void Publish(T message)
        {
            var handlers = new List<Action<T>>();

            lock (_lock) {
                IEnumerable<Action<T>> weakHandlers = _weakHandlers.Select(wr => {
                    Action<T> handler;
                    wr.TryGetTarget(out handler);
                    return handler;
                }).Where(h => h != null);

                handlers.AddRange(weakHandlers);
                handlers.AddRange(_handlers);
            }

            Action publishAction = () => {
                foreach (var handler in handlers) {
                    handler(message);
                }
            };

            publishAction.OnUiThread();
        }
    }
}