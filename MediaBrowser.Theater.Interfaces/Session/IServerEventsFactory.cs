using MediaBrowser.Model.ApiClient;

namespace MediaBrowser.Theater.Interfaces.Session
{
    /// <summary>
    /// Interface IServerEventsFactory
    /// </summary>
    public interface IServerEventsFactory
    {
        /// <summary>
        /// Gets the server events.
        /// </summary>
        /// <returns>IServerEvents.</returns>
        IServerEvents GetServerEvents();
    }
}
