using MediaBrowser.Common.Configuration;
using MediaBrowser.Common.Extensions;
using MediaBrowser.Common.IO;
using MediaBrowser.Model.ApiClient;
using MediaBrowser.Theater.Interfaces.Presentation;
using System;
using System.Collections.Concurrent;
using System.IO;
using System.Net.Cache;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace MediaBrowser.Theater.Implementations.Presentation
{
    /// <summary>
    /// Class ImageManager
    /// </summary>
    public class ImageManager : IImageManager
    {
        /// <summary>
        /// The _remote image cache
        /// </summary>
        private readonly FileSystemRepository _remoteImageCache;

        /// <summary>
        /// The _api client
        /// </summary>
        private readonly IApiClient _apiClient;

        /// <summary>
        /// The _locks
        /// </summary>
        private readonly ConcurrentDictionary<string, SemaphoreSlim> _imageFileLocks = new ConcurrentDictionary<string, SemaphoreSlim>();

        /// <summary>
        /// Initializes a new instance of the <see cref="ImageManager" /> class.
        /// </summary>
        /// <param name="apiClient">The API client.</param>
        /// <param name="paths">The paths.</param>
        public ImageManager(IApiClient apiClient, IApplicationPaths paths)
        {
            _apiClient = apiClient;

            _remoteImageCache = new FileSystemRepository(Path.Combine(paths.CachePath, "remote-images"));
        }

        /// <summary>
        /// Gets the bitmap image.
        /// </summary>
        /// <param name="uri">The URI.</param>
        /// <returns>BitmapImage.</returns>
        /// <exception cref="System.ArgumentNullException">uri</exception>
        public BitmapImage GetBitmapImage(string uri)
        {
            if (string.IsNullOrEmpty(uri))
            {
                throw new ArgumentNullException("uri");
            }

            return GetBitmapImage(new Uri(uri));
        }

        /// <summary>
        /// Gets the bitmap image.
        /// </summary>
        /// <param name="uri">The URI.</param>
        /// <returns>BitmapImage.</returns>
        /// <exception cref="System.ArgumentNullException">uri</exception>
        public BitmapImage GetBitmapImage(Uri uri)
        {
            if (uri == null)
            {
                throw new ArgumentNullException("uri");
            }

            var bitmap = new BitmapImage
            {
                CreateOptions = BitmapCreateOptions.DelayCreation,
                CacheOption = BitmapCacheOption.OnDemand,
                UriCachePolicy = new RequestCachePolicy(RequestCacheLevel.CacheIfAvailable)
            };

            bitmap.BeginInit();
            bitmap.UriSource = uri;
            bitmap.EndInit();

            RenderOptions.SetBitmapScalingMode(bitmap, BitmapScalingMode.Fant);
            return bitmap;
        }

        /// <summary>
        /// Gets the remote bitmap async.
        /// </summary>
        /// <param name="url">The URL.</param>
        /// <returns>Task{BitmapImage}.</returns>
        /// <exception cref="System.ArgumentNullException">url</exception>
        public async Task<BitmapImage> GetRemoteBitmapAsync(string url)
        {
            if (string.IsNullOrEmpty(url))
            {
                throw new ArgumentNullException("url");
            }

            var cachePath = _remoteImageCache.GetResourcePath(url.GetMD5().ToString());

            if (File.Exists(cachePath))
            {
                return GetCachedBitmapImage(cachePath);
            }

            return await Task.Run(async () =>
            {
                var semaphore = GetImageFileLock(cachePath);
                await semaphore.WaitAsync().ConfigureAwait(false);

                // Look in the cache again
                if (File.Exists(cachePath))
                {
                    semaphore.Release();

                    return GetCachedBitmapImage(cachePath);
                }

                try
                {
                    using (var httpStream = await _apiClient.GetImageStreamAsync(url))
                    {
                        return await GetBitmapImageAsync(httpStream, cachePath);
                    }
                }
                finally
                {
                    semaphore.Release();
                }

            }).ConfigureAwait(false);
        }

        /// <summary>
        /// Gets the remote image async.
        /// </summary>
        /// <param name="url">The URL.</param>
        /// <returns>Task{Image}.</returns>
        public async Task<Image> GetRemoteImageAsync(string url)
        {
            var bitmap = await GetRemoteBitmapAsync(url);

            return new Image { Source = bitmap };
        }

        /// <summary>
        /// Gets the lock.
        /// </summary>
        /// <param name="filename">The filename.</param>
        /// <returns>System.Object.</returns>
        private SemaphoreSlim GetImageFileLock(string filename)
        {
            return _imageFileLocks.GetOrAdd(filename, key => new SemaphoreSlim(1, 1));
        }

        /// <summary>
        /// Gets the image async.
        /// </summary>
        /// <param name="sourceStream">The source stream.</param>
        /// <param name="cachePath">The cache path.</param>
        /// <returns>Task{BitmapImage}.</returns>
        private async Task<BitmapImage> GetBitmapImageAsync(Stream sourceStream, string cachePath)
        {
            var parentPath = Path.GetDirectoryName(cachePath);

            if (!Directory.Exists(parentPath))
            {
                Directory.CreateDirectory(parentPath);
            }

            using (var fileStream = new FileStream(cachePath, FileMode.Create, FileAccess.Write, FileShare.Read, StreamDefaults.DefaultFileStreamBufferSize, true))
            {
                await sourceStream.CopyToAsync(fileStream).ConfigureAwait(false);
            }

            return GetCachedBitmapImage(cachePath);
        }

        /// <summary>
        /// Gets the cached bitmap image.
        /// </summary>
        /// <param name="cachePath">The cache path.</param>
        /// <returns>BitmapImage.</returns>
        private BitmapImage GetCachedBitmapImage(string cachePath)
        {
            var bitmapImage = new BitmapImage
            {
                CreateOptions = BitmapCreateOptions.DelayCreation,
                CacheOption = BitmapCacheOption.Default,
                UriCachePolicy = new RequestCachePolicy(RequestCacheLevel.CacheIfAvailable)
            };

            RenderOptions.SetBitmapScalingMode(bitmapImage, BitmapScalingMode.Fant);

            bitmapImage.BeginInit();
            bitmapImage.UriSource = new Uri(cachePath);
            bitmapImage.EndInit();
            bitmapImage.Freeze();
            return bitmapImage;
        }
    }
}
