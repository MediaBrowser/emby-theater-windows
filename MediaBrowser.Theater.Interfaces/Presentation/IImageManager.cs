using System;
using System.Threading;
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
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>Task{BitmapImage}.</returns>
        Task<BitmapImage> GetRemoteBitmapAsync(string url, CancellationToken cancellationToken);

        /// <summary>
        /// Gets the remote image async.
        /// </summary>
        /// <param name="url">The URL.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>Task{Image}.</returns>
        Task<Image> GetRemoteImageAsync(string url, CancellationToken cancellationToken);
    }

    /// <summary>
    /// Class ImageManagerExtensions
    /// </summary>
    public static class ImageManagerExtensions
    {
        /// <summary>
        /// Gets the remote bitmap async.
        /// </summary>
        /// <param name="imageManager">The image manager.</param>
        /// <param name="url">The URL.</param>
        /// <returns>Task{BitmapImage}.</returns>
        public static Task<BitmapImage> GetRemoteBitmapAsync(this IImageManager imageManager, string url)
        {
            return imageManager.GetRemoteBitmapAsync(url, CancellationToken.None);
        }

        /// <summary>
        /// Gets the remote image async.
        /// </summary>
        /// <param name="imageManager">The image manager.</param>
        /// <param name="url">The URL.</param>
        /// <returns>Task{Image}.</returns>
        public static Task<Image> GetRemoteImageAsync(this IImageManager imageManager, string url)
        {
            return imageManager.GetRemoteImageAsync(url, CancellationToken.None);
        }
    }
}
