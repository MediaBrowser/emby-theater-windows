using MediaBrowser.Common.Implementations;

namespace MediaBrowser.Theater.Implementations.Configuration
{
    /// <summary>
    /// Class UIApplicationPaths
    /// </summary>
    public class ApplicationPaths : BaseApplicationPaths
    {
        public ApplicationPaths(string programDataPath, string applicationPath)
            : base(programDataPath, applicationPath)
        {
        }
    }
}
