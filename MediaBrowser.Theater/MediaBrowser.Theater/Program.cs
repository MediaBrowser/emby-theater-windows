using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Windows.Forms;
using MediaBrowser.Common.Constants;
using MediaBrowser.Common.Implementations.Logging;
using MediaBrowser.Common.Implementations.Updates;
using MediaBrowser.Model.Logging;
using MediaBrowser.Theater.Api.Configuration;

namespace MediaBrowser.Theater
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            Mutex singleInstanceMutex = null;

            try {
                bool createdNew;
                singleInstanceMutex = new Mutex(true, @"Local\" + typeof (Program).Assembly.GetName().Name, out createdNew);

                if (!createdNew) {
                    singleInstanceMutex = null;
                    return;
                }

                string appPath = Process.GetCurrentProcess().MainModule.FileName;
                var appPaths = new ApplicationPaths(appPath);
                var logManager = new NlogManager(appPaths.LogDirectoryPath, "theater");
                logManager.ReloadLogger(LogSeverity.Debug);

                bool updateInstalling = InstallUpdatePackage(appPaths, logManager);
                if (!updateInstalling) {
                    LaunchApplication(appPaths, logManager);
                }
            } finally {
                if (singleInstanceMutex != null) {
                    singleInstanceMutex.ReleaseMutex();
                    singleInstanceMutex.Close();
                    singleInstanceMutex.Dispose();
                }
            }
        }

        private static bool InstallUpdatePackage(ApplicationPaths appPaths, NlogManager logManager)
        {
            string updateArchive = Path.Combine(appPaths.TempUpdatePath, Constants.MbTheaterPkgName + ".zip");

            if (File.Exists(updateArchive)) {
                // Update is there - execute update
                try {
                    new ApplicationUpdater().UpdateApplication(MBApplication.MBTheater, appPaths, updateArchive,
                                                               logManager.GetLogger("ApplicationUpdater"), string.Empty);

                    // And just let the app exit so it can update
                    return true;
                } catch (Exception e) {
                    MessageBox.Show(string.Format("Error attempting to update application.\n\n{0}\n\n{1}",
                                                  e.GetType().Name, e.Message));
                }
            }

            return false;
        }

        private static void LaunchApplication(ApplicationPaths appPaths, NlogManager logManager)
        {
            throw new NotImplementedException();
        }
    }
}