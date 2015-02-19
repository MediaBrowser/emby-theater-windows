using System;
using System.Threading.Tasks;

namespace MediaBrowser.Theater.Api.Events
{
    public interface IEventBus<T>
    {
        IDisposable Subscribe(Action<T> handler, bool weak = false);
        Task Publish(T message);
    }
}