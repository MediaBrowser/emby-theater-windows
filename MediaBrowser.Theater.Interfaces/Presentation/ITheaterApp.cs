using System.Threading.Tasks;
using System.Windows;

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
        /// Gets the page.
        /// </summary>
        /// <returns>Page.</returns>
        Task Launch();

        /// <summary>
        /// Gets the tile image.
        /// </summary>
        /// <returns>FrameworkElement.</returns>
        FrameworkElement GetTileImage();
    }
}
