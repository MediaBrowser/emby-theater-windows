using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MediaBrowser.Theater.Api.UserInterface;

namespace MediaBrowser.Theater.Api.Events
{
    public class EventBus<T> : IEventBus<T>
    {
        private readonly List<Action<T>> _handlers = new List<Action<T>>();
        private readonly object _lock = new object();
        private readonly List<WeakReference<Action<T>>> _weakHandlers = new List<WeakReference<Action<T>>>();

        public IDisposable Subscribe(Action<T> handler, bool weak = true)
        {
            lock (_lock) {
                if (weak) {
                    _weakHandlers.Add(new WeakReference<Action<T>>(handler));
                } else {
                    _handlers.Add(handler);
                }

                return new Disposer(this, handler);
            }
        }

        public Task Publish(T message)
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

            return publishAction.OnUiThreadAsync();
        }

        class Disposer : IDisposable
        {
            private readonly EventBus<T> _bus;
            private readonly Action<T> _handler;

            public Disposer(EventBus<T> bus, Action<T> handler)
            {
                _bus = bus;
                _handler = handler;
            }

            public void Dispose()
            {
                lock (_bus._lock) {
                    if (!_bus._handlers.Remove(_handler)) {
                        _bus._weakHandlers.RemoveAll(w => {
                            Action<T> h;
                            return w.TryGetTarget(out h) && h == _handler;
                        });
                    }
                }
            }
        }
    }
}