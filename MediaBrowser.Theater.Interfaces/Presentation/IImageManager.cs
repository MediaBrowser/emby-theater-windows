using System;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace MediaBrowser.Theater.Interfaces.Presentation
{
    /// <summary>
    /// Interface IImageManager
    /// </summary>
    public interface IImageManager
    {
        /// <summary>
        /// Gets the bitmap image.
        /// </summary>
        /// <param name="uri">The URI.</param>
        /// <returns>BitmapImage.</returns>
        BitmapImage GetBitmapImage(Uri uri);

        /// <summary>
        /// Gets the remote bitmap async.
        /// </summary>
        /// <param name="url">The URL.</param>
        /// <returns>Task{BitmapImage}.</returns>
        Task<BitmapImage> GetRemoteBitmapAsync(string url);

        /// <summary>
        /// Gets the remote image async.
        /// </summary>
        /// <param name="url">The URL.</param>
        /// <returns>Task{Image}.</returns>
        Task<Image> GetRemoteImageAsync(string url);
    }
}
