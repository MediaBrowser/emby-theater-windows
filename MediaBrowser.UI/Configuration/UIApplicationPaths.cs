using MediaBrowser.Common.Implementations;
using System.IO;
using MediaBrowser.Common.Kernel;

namespace MediaBrowser.UI.Configuration
{
    /// <summary>
    /// Class UIApplicationPaths
    /// </summary>
    public class UIApplicationPaths : BaseApplicationPaths, IApplicationPaths
    {
        /// <summary>
        /// The _remote image cache path
        /// </summary>
        private string _remoteImageCachePath;
        /// <summary>
        /// Gets the remote image cache path.
        /// </summary>
        /// <value>The remote image cache path.</value>
        public string RemoteImageCachePath
        {
            get
            {
                if (_remoteImageCachePath == null)
                {
                    _remoteImageCachePath = Path.Combine(CachePath, "remote-images");

                    if (!Directory.Exists(_remoteImageCachePath))
                    {
                        Directory.CreateDirectory(_remoteImageCachePath);
                    }
                }

                return _remoteImageCachePath;
            }
        }
    }
}
