
namespace MediaBrowser.Theater.Interfaces.Presentation
{
    /// <summary>
    /// Interface ITheaterApp
    /// </summary>
    public interface ITheaterApp
    {
        /// <summary>
        /// Gets the name.
        /// </summary>
        /// <value>The name.</value>
        string Name { get; }

        /// <summary>
        /// Launches this instance.
        /// </summary>
        void Launch();
    }
}
