using MediaBrowser.Common.Configuration;
using MediaBrowser.Common.Extensions;
using MediaBrowser.Common.IO;
using MediaBrowser.Model.ApiClient;
using MediaBrowser.Theater.Interfaces.Configuration;
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

        private readonly ITheaterConfigurationManager _config;

        /// <summary>
        /// The _locks
        /// </summary>
        private readonly ConcurrentDictionary<string, SemaphoreSlim> _imageFileLocks = new ConcurrentDictionary<string, SemaphoreSlim>();

        /// <summary>
        /// Initializes a new instance of the <see cref="ImageManager" /> class.
        /// </summary>
        /// <param name="apiClient">The API client.</param>
        /// <param name="paths">The paths.</param>
        /// <param name="config"></param>
        public ImageManager(IApiClient apiClient, IApplicationPaths paths, ITheaterConfigurationManager config)
        {
            _apiClient = apiClient;
            _config = config;

            _remoteImageCache = new FileSystemRepository(Path.Combine(paths.CachePath, "remote-images"));
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

            var scalingMode = _config.Configuration.EnableHighQualityImageScaling
                                  ? BitmapScalingMode.Fant
                                  : BitmapScalingMode.LowQuality;

            RenderOptions.SetBitmapScalingMode(bitmap, scalingMode);

            bitmap.BeginInit();
            bitmap.UriSource = uri;
            bitmap.EndInit();

            return bitmap;
        }

        public async Task<BitmapImage> GetRemoteBitmapAsync(string url, CancellationToken cancellationToken)
        {
            return await Task.Run(() => GetRemoteBitmapAsyncInternal(url, cancellationToken), cancellationToken);
        }

        /// <summary>
        /// Gets the remote bitmap async.
        /// </summary>
        /// <param name="url">The URL.</param>
        /// <returns>Task{BitmapImage}.</returns>
        /// <exception cref="ArgumentNullException">url</exception>
        private async Task<BitmapImage> GetRemoteBitmapAsyncInternal(string url, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(url))
            {
                throw new ArgumentNullException("url");
            }

            var cachePath = _remoteImageCache.GetResourcePath(url.GetMD5().ToString());

            try
            {
                return GetCachedBitmapImage(cachePath);
            }
            catch (IOException)
            {
                // Cache file doesn't exist or is currently being written to.
            }

            cancellationToken.ThrowIfCancellationRequested();

            var semaphore = GetImageFileLock(cachePath);
            await semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);

            // Look in the cache again
            try
            {
                var img = GetCachedBitmapImage(cachePath);
                semaphore.Release();
                return img;
            }
            catch (IOException)
            {
                // Cache file doesn't exist or is currently being written to.
            }

            try
            {
                using (var httpStream = await _apiClient.GetImageStreamAsync(url, cancellationToken).ConfigureAwait(false))
                {
                    var parentPath = Path.GetDirectoryName(cachePath);

                    if (!Directory.Exists(parentPath))
                    {
                        Directory.CreateDirectory(parentPath);
                    }

                    using (var fileStream = new FileStream(cachePath, FileMode.Create, FileAccess.Write, FileShare.Read, StreamDefaults.DefaultFileStreamBufferSize, true))
                    {
                        await httpStream.CopyToAsync(fileStream).ConfigureAwait(false);
                    }

                    return GetCachedBitmapImage(cachePath);
                }
            }
            finally
            {
                semaphore.Release();
            }
        }

        /// <summary>
        /// Gets the remote image async.
        /// </summary>
        /// <param name="url">The URL.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>Task{Image}.</returns>
        public async Task<Image> GetRemoteImageAsync(string url, CancellationToken cancellationToken)
        {
            var bitmap = await GetRemoteBitmapAsync(url, cancellationToken);

            var image = new Image { Source = bitmap };

            var scalingMode = _config.Configuration.EnableHighQualityImageScaling
                      ? BitmapScalingMode.Fant
                      : BitmapScalingMode.LowQuality;

            RenderOptions.SetBitmapScalingMode(image, scalingMode);

            return image;
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
        /// Gets the cached bitmap image.
        /// </summary>
        /// <param name="cachePath">The cache path.</param>
        /// <returns>BitmapImage.</returns>
        private BitmapImage GetCachedBitmapImage(string cachePath)
        {
            var bitmapImage = new BitmapImage
            {
                CacheOption = BitmapCacheOption.OnLoad,
                UriCachePolicy = new RequestCachePolicy(RequestCacheLevel.CacheIfAvailable)
            };

            var scalingMode = _config.Configuration.EnableHighQualityImageScaling
                                  ? BitmapScalingMode.Fant
                                  : BitmapScalingMode.LowQuality;

            RenderOptions.SetBitmapScalingMode(bitmapImage, scalingMode);

            bitmapImage.BeginInit();
            bitmapImage.UriSource = new Uri(cachePath);
            bitmapImage.EndInit();
            bitmapImage.Freeze();
            return bitmapImage;
        }
    }
}
