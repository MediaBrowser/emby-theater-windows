using System;
using System.Collections.Concurrent;
using System.IO;
using System.Net.Cache;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using MediaBrowser.Common.Configuration;
using MediaBrowser.Common.Extensions;
using MediaBrowser.Common.IO;
using MediaBrowser.Theater.Api.Configuration;
using MediaBrowser.Theater.Api.Session;
using MediaBrowser.Theater.Api.System;

namespace MediaBrowser.Theater.Api.UserInterface
{
    /// <summary>
    ///     Class ImageManager
    /// </summary>
    public class ImageManager : IImageManager
    {
        private readonly ISessionManager _sessionManager;
        private readonly ITheaterConfigurationManager _config;

        /// <summary>
        ///     The _locks
        /// </summary>
        private readonly ConcurrentDictionary<string, SemaphoreSlim> _imageFileLocks = new ConcurrentDictionary<string, SemaphoreSlim>();

        /// <summary>
        ///     The _remote image cache
        /// </summary>
        private readonly FileSystemRepository _remoteImageCache;

        /// <summary>
        ///     Initializes a new instance of the <see cref="ImageManager" /> class.
        /// </summary>
        /// <param name="sessionManager"></param>
        /// <param name="paths">The paths.</param>
        /// <param name="config"></param>
        public ImageManager(ISessionManager sessionManager, IApplicationPaths paths, ITheaterConfigurationManager config)
        {
            _sessionManager = sessionManager;
            _config = config;

            _remoteImageCache = new FileSystemRepository(Path.Combine(paths.CachePath, "remote-images"));
        }

        /// <summary>
        ///     Gets the bitmap image.
        /// </summary>
        /// <param name="uri">The URI.</param>
        /// <returns>BitmapImage.</returns>
        /// <exception cref="ArgumentNullException">uri</exception>
        public BitmapImage GetBitmapImage(Uri uri)
        {
            if (uri == null) {
                throw new ArgumentNullException("uri");
            }

            var bitmap = new BitmapImage {
                CreateOptions = BitmapCreateOptions.DelayCreation,
                CacheOption = BitmapCacheOption.OnDemand,
                UriCachePolicy = new RequestCachePolicy(RequestCacheLevel.CacheIfAvailable)
            };

            BitmapScalingMode scalingMode = _config.Configuration.EnableHighQualityImageScaling
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
        ///     Gets the remote image async.
        /// </summary>
        /// <param name="url">The URL.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>Task{Image}.</returns>
        public async Task<Image> GetRemoteImageAsync(string url, CancellationToken cancellationToken)
        {
            BitmapImage bitmap = await GetRemoteBitmapAsync(url, cancellationToken);

            var image = new Image { Source = bitmap };

            BitmapScalingMode scalingMode = _config.Configuration.EnableHighQualityImageScaling
                                                ? BitmapScalingMode.Fant
                                                : BitmapScalingMode.LowQuality;

            RenderOptions.SetBitmapScalingMode(image, scalingMode);

            return image;
        }

        /// <summary>
        ///     Gets the remote bitmap async.
        /// </summary>
        /// <param name="url">The URL.</param>
        /// <returns>Task{BitmapImage}.</returns>
        /// <exception cref="ArgumentNullException">url</exception>
        private async Task<BitmapImage> GetRemoteBitmapAsyncInternal(string url, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(url)) {
                throw new ArgumentNullException("url");
            }

            string cachePath = _remoteImageCache.GetResourcePath(url.GetMD5().ToString());

            try {
                var result = GetCachedBitmapImage(cachePath);
                if (result != null) {
                    return result;
                }
            }
            catch (IOException) {
                // Cache file doesn't exist or is currently being written to.
            }

            cancellationToken.ThrowIfCancellationRequested();

            SemaphoreSlim semaphore = GetImageFileLock(cachePath);
            await semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);

            // Look in the cache again
            try {
                BitmapImage img = GetCachedBitmapImage(cachePath);
                if (img != null) {
                    semaphore.Release();
                    return img;
                }
            }
            catch (IOException) {
                // Cache file doesn't exist or is currently being written to.
            }

            try {
                using (Stream httpStream = await _sessionManager.ActiveApiClient.GetImageStreamAsync(url, cancellationToken).ConfigureAwait(false)) {
                    string parentPath = Path.GetDirectoryName(cachePath);

                    if (!Directory.Exists(parentPath)) {
                        Directory.CreateDirectory(parentPath);
                    }

                    using (var fileStream = new FileStream(cachePath, FileMode.Create, FileAccess.Write, FileShare.Read, StreamDefaults.DefaultFileStreamBufferSize, true)) {
                        await httpStream.CopyToAsync(fileStream).ConfigureAwait(false);
                    }

                    return GetCachedBitmapImage(cachePath);
                }
            }
            finally {
                semaphore.Release();
            }
        }

        /// <summary>
        ///     Gets the lock.
        /// </summary>
        /// <param name="filename">The filename.</param>
        /// <returns>System.Object.</returns>
        private SemaphoreSlim GetImageFileLock(string filename)
        {
            return _imageFileLocks.GetOrAdd(filename, key => new SemaphoreSlim(1, 1));
        }

        /// <summary>
        ///     Gets the cached bitmap image.
        /// </summary>
        /// <param name="cachePath">The cache path.</param>
        /// <returns>BitmapImage.</returns>
        private BitmapImage GetCachedBitmapImage(string cachePath)
        {
            var bitmapImage = new BitmapImage();

            if (!File.Exists(cachePath)) {
                return null;
            }

            using (var stream = File.OpenRead(cachePath)) {
                bitmapImage.BeginInit();
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.StreamSource = stream;
                bitmapImage.EndInit();
            }

            bitmapImage.Freeze();
            return bitmapImage;
        }
    }
}