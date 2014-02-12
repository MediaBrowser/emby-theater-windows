using System.Threading.Tasks;
using MediaBrowser.Common.Plugins;
using MediaBrowser.Theater.Api.UserInterface.Navigation;

namespace MediaBrowser.Theater.Api.UserInterface
{
    public interface ITheme : IPlugin
    {
        /// <summary>
        ///     Gets the presentation manager used to show UI elements.
        /// </summary>
        IPresentationManager Presentation { get; }

        /// <summary>
        ///     Gets the navigation service implementation.
        /// </summary>
        INavigationService Navigation { get; }

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