using MediaBrowser.Common.Implementations;

namespace MediaBrowser.Theater.Implementations.Configuration
{
    /// <summary>
    /// Class UIApplicationPaths
    /// </summary>
    public class ApplicationPaths : BaseApplicationPaths
    {
#if DEBUG
        /// <summary>
        /// Initializes a new instance of the <see cref="ApplicationPaths" /> class.
        /// </summary>
        public ApplicationPaths(string applicationPath)
            : base(true, applicationPath)
        {
        }
#else
        public ApplicationPaths(string applicationPath)
            : base(false, applicationPath)
        {
        }
#endif

    }
}
