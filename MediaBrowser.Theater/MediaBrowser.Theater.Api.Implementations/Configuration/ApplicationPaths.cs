using MediaBrowser.Common.Implementations;

namespace MediaBrowser.Theater.Api.Configuration
{
    public class ApplicationPaths : BaseApplicationPaths, ITheaterApplicationPaths
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="ApplicationPaths" /> class.
        /// </summary>
        public ApplicationPaths(string programDataPath, string applicationPath)
            : base(programDataPath, applicationPath) { }
    }
}