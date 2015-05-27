using MediaBrowser.Common.Configuration;
using MediaBrowser.Model.Logging;
using System.Diagnostics;
using System.IO;

namespace MediaBrowser.UI
{
    public class ApplicationUpdater
    {
        private const string UpdaterExe = "Mediabrowser.Updater.exe";
        private const string UpdaterDll = "Mediabrowser.InstallUtil.dll";

        public void UpdateApplication(IApplicationPaths appPaths, string archive, ILogger logger)
        {
            // First see if there is a version file and read that in
            var version = "Unknown";
            if (File.Exists(archive + ".ver"))
            {
                version = File.ReadAllText(archive + ".ver");
            }

            var systemPath = appPaths.ProgramSystemPath;

            // Use our installer passing it the specific archive
            // We need to copy to a temp directory and execute it there
            var source = Path.Combine(systemPath, UpdaterExe);

            logger.Info("Copying updater to temporary location");
            var tempUpdater = Path.Combine(Path.GetTempPath(), UpdaterExe);
            File.Copy(source, tempUpdater, true);
            source = Path.Combine(systemPath, UpdaterDll);
            var tempUpdaterDll = Path.Combine(Path.GetTempPath(), UpdaterDll);

            logger.Info("Copying updater dependencies to temporary location");
            File.Copy(source, tempUpdaterDll, true);
            var product = "mbt";
            // Our updater needs SS and ionic
            source = Path.Combine(systemPath, "ServiceStack.Text.dll");
            File.Copy(source, Path.Combine(Path.GetTempPath(), "ServiceStack.Text.dll"), true);
            source = Path.Combine(systemPath, "SharpCompress.dll");
            File.Copy(source, Path.Combine(Path.GetTempPath(), "SharpCompress.dll"), true);

            logger.Info("Starting updater process.");

            // installpath = program data folder
            // start path = executable to launch
            var args = string.Format("product={0} archive=\"{1}\" caller={2} pismo=false version={3} service={4} installpath=\"{5}\" startpath=\"{6}\" systempath=\"{7}\"",
                    product, archive, Process.GetCurrentProcess().Id, version, string.Empty, appPaths.ProgramDataPath, appPaths.ApplicationPath, systemPath);

            logger.Info("Args: {0}", args);
            Process.Start(tempUpdater, args);

            // That's it.  The installer will do the work once we exit
        }
    }
}
