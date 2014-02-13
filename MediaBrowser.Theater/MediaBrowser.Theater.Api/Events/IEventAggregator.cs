namespace MediaBrowser.Theater.Api.Events
{
    public interface IEventAggregator
    {
        IEventBus<T> Get<T>();
    }
}