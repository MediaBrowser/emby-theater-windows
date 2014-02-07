using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Windows.Forms;
using MediaBrowser.Common.Constants;
using MediaBrowser.Common.Implementations.Logging;
using MediaBrowser.Common.Implementations.Updates;
using MediaBrowser.Model.Logging;
using MediaBrowser.Theater.Api.Configuration;
using MediaBrowser.Theater.StartupWizard;
using MediaBrowser.Theater.StartupWizard.ViewModels;

namespace MediaBrowser.Theater
{
    public static class Program
    {
        [STAThread]
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

        private static async void LaunchApplication(ApplicationPaths appPaths, NlogManager logManager)
        {
#if !DEBUG
            ILogger logger = logManager.GetLogger("App");
            try {
#endif
            var appHost = new ApplicationHost(appPaths, logManager);

                appHost.Init(new Progress<double>()).Wait();

                if (!appHost.IsFirstRun) {
                    var completed = appHost.RunStartupWizard();

                    if (completed) {
                        await appHost.Restart();
                    } else {
                        await appHost.Shutdown();
                    }
                } else {
                    appHost.RunUserInterface();
                }
#if !DEBUG
            } catch (Exception ex) {
                logger.ErrorException("Error launching application", ex);

                MessageBox.Show("There was an error launching Media Browser Theater: " + ex.Message);
                
                // Shutdown the app with an error code
                Environment.Exit(1);
            }
#endif
        }
    }
}