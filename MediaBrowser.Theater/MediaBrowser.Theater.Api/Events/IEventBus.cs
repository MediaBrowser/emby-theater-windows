using System;

namespace MediaBrowser.Theater.Api.Events
{
    public interface IEventBus<T>
    {
        void Subscribe(Action<T> handler, bool weak = false);
        void Publish(T message);
    }
}