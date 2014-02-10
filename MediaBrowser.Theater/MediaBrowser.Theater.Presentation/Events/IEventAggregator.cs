namespace MediaBrowser.Theater.Presentation.Events
{
    public interface IEventAggregator
    {
        IEventBus<T> Get<T>();
    }
}