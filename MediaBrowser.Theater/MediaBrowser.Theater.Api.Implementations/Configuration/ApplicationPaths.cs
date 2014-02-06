using MediaBrowser.Common.Implementations;

namespace MediaBrowser.Theater.Api.Configuration
{
    public class ApplicationPaths : BaseApplicationPaths, ITheaterApplicationPaths
    {
#if DEBUG
        /// <summary>
        ///     Initializes a new instance of the <see cref="ApplicationPaths" /> class.
        /// </summary>
        public ApplicationPaths(string applicationPath)
            : base(true, applicationPath) { }
#else
        public ApplicationPaths(string applicationPath)
            : base(false, applicationPath)
        {
        }
#endif
    }
}