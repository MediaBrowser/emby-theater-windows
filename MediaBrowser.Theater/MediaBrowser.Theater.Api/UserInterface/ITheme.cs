using System;
using System.Threading.Tasks;
using MediaBrowser.Common.Plugins;

namespace MediaBrowser.Theater.Api.UserInterface
{
    public interface ITheme : IPlugin
    {
        /// <summary>
        ///     Starts the theme. The theme is expected to present its GUI.
        /// </summary>
        void Run();

        /// <summary>
        ///     Shuts down the theme. The theme is expected to close its GUI.
        /// </summary>
        /// <returns>An a representing the asynchronous operation.</returns>
        Task Shutdown();

        event Action ApplicationStarted;
    }
}