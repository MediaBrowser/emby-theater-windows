using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

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
        Page GetPage();

        /// <summary>
        /// Gets the thumb image.
        /// </summary>
        /// <returns>Task{BitmapImage}.</returns>
        Task<BitmapImage> GetThumbImage();
    }
}
