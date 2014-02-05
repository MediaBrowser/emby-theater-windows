using System.Threading.Tasks;

namespace MediaBrowser.Theater.Api.Theming
{
    public interface ITheme
    {
        /// <summary>
        ///     Gets the name of the theme.
        /// </summary>
        string Name { get; }

        /// <summary>
        ///     Starts the theme. The theme is expected to present its GUI.
        /// </summary>
        void Run();

        /// <summary>
        ///     Shuts down the theme. The theme is expected to close its GUI.
        /// </summary>
        /// <returns>An a representing the asynchronous operation.</returns>
        Task Shutdown();
    }
}