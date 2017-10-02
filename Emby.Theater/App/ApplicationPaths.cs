using System.IO;
using Emby.Theater.AppBase;

namespace Emby.Theater.App
{
    public class ApplicationPaths : BaseApplicationPaths
    {
        public ApplicationPaths(string programDataPath, string applicationPath)
            : base(programDataPath, Path.GetDirectoryName(applicationPath))
        {
        }
    }
}
