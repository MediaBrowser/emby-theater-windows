using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using MediaBrowser.Model.ApiClient;

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

        /// <summary>
        /// Gets the remote bitmap async.
        /// </summary>
        /// <param name="apiClient">The API client.</param>
        /// <param name="url">The URL.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>Task{BitmapImage}.</returns>
        Task<BitmapImage> GetRemoteBitmapAsync(IApiClient apiClient, string url, CancellationToken cancellationToken);

        /// <summary>
        /// Gets the remote image async.
        /// </summary>
        /// <param name="apiClient">The API client.</param>
        /// <param name="url">The URL.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>Task{Image}.</returns>
        Task<Image> GetRemoteImageAsync(IApiClient apiClient, string url, CancellationToken cancellationToken);
    }
}
