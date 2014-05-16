using System;
using System.Threading.Tasks;

namespace MediaBrowser.Theater.Api.Events
{
    public interface IEventBus<T>
    {
        void Subscribe(Action<T> handler, bool weak = false);
        Task Publish(T message);
    }
}