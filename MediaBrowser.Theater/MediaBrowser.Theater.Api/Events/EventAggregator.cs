using System;
using System.Collections.Generic;

namespace MediaBrowser.Theater.Api.Events
{
    public class EventAggregator : IEventAggregator
    {
        private readonly Dictionary<Type, object> _events = new Dictionary<Type, object>();

        public IEventBus<T> Get<T>()
        {
            Type eventType = typeof (T);

            lock (_events) {
                object bus;
                if (_events.TryGetValue(eventType, out bus)) {
                    return bus as EventBus<T>;
                }

                var newEventBus = new EventBus<T>();
                _events.Add(eventType, newEventBus);

                return newEventBus;
            }
        }
    }
}