using System;

namespace MediaBrowser.Theater.Presentation.Events
{
    public interface IEventBus<T>
    {
        void Subscribe(Action<T> handler, bool weak = true);
        void Publish(T message);
    }
}