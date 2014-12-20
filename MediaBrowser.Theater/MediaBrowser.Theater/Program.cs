using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Windows.Forms;
using MediaBrowser.Common.Implementations.Logging;
using MediaBrowser.Model.ApiClient;
using MediaBrowser.Model.Logging;
using MediaBrowser.Theater.Api.Configuration;
using MessageBox = System.Windows.MessageBox;

namespace MediaBrowser.Theater
{
    public static class Program
    {
        public const string PackageName = "MediaBrowser.Theater";

        [STAThread]
        public static void Main(string[] args)
        {
            Mutex singleInstanceMutex = null;
            bool restartOnExit = false;

            try {
                bool createdNew;
                singleInstanceMutex = new Mutex(true, @"Local\" + typeof (Program).Assembly.GetName().Name, out createdNew);

                if (!createdNew) {
                    singleInstanceMutex = null;
                    return;
                }

                string appPath = Process.GetCurrentProcess().MainModule.FileName;
                var appPaths = new ApplicationPaths(GetProgramDataPath(appPath), appPath);
                var logManager = new NlogManager(appPaths.LogDirectoryPath, "theater");
                logManager.ReloadLogger(LogSeverity.Debug);

                bool updateInstalling = InstallUpdatePackage(appPaths, logManager);
                if (!updateInstalling) {
                    restartOnExit = LaunchApplication(appPaths, logManager);
                }
            }
            finally {
                if (singleInstanceMutex != null) {
                    singleInstanceMutex.ReleaseMutex();
                    singleInstanceMutex.Close();
                    singleInstanceMutex.Dispose();
                }
            }

            if (restartOnExit) {
                Application.Restart();
            }
        }

        public static string GetProgramDataPath(string applicationPath)
        {
            var useDebugPath = false;

#if DEBUG
            useDebugPath = true;
#endif

            var programDataPath = useDebugPath ?
                System.Configuration.ConfigurationManager.AppSettings["DebugProgramDataPath"] :
                System.Configuration.ConfigurationManager.AppSettings["ReleaseProgramDataPath"];

            programDataPath = programDataPath.Replace("%ApplicationData%", Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData));

            // If it's a relative path, e.g. "..\"
            if (!Path.IsPathRooted(programDataPath))
            {
                var path = Path.GetDirectoryName(applicationPath);

                if (string.IsNullOrEmpty(path))
                {
                    throw new ApplicationException("Unable to determine running assembly location");
                }

                programDataPath = Path.Combine(path, programDataPath);

                programDataPath = Path.GetFullPath(programDataPath);
            }

            Directory.CreateDirectory(programDataPath);

            return programDataPath;
        }

        private static bool InstallUpdatePackage(ApplicationPaths appPaths, NlogManager logManager)
        {
            string updateArchive = Path.Combine(appPaths.TempUpdatePath, PackageName + ".zip");

            if (File.Exists(updateArchive)) {
                // Update is there - execute update
                try {
                    new ApplicationUpdater().UpdateApplication(
                        appPaths, updateArchive,
                        logManager.GetLogger("ApplicationUpdater"), string.Empty);

                    // And just let the app exit so it can update
                    return true;
                }
                catch (Exception e) {
                    MessageBox.Show(string.Format("Error attempting to update application.\n\n{0}\n\n{1}",
                                                  e.GetType().Name, e.Message));
                }
            }

            return false;
        }

        private static bool LaunchApplication(ApplicationPaths appPaths, NlogManager logManager)
        {
#if !DEBUG
            ILogger logger = logManager.GetLogger("App");
            try {
#endif
            using (var appHost = new ApplicationHost(appPaths, logManager)) {
                appHost.Init(new Progress<double>()).Wait();

                if (!appHost.TheaterConfigurationManager.Configuration.IsStartupWizardCompleted || appHost.ConnectToServer().Result.State == ConnectionState.Unavailable) {
                    bool completed = appHost.RunStartupWizard();

                    if (completed) {
                        appHost.TheaterConfigurationManager.Configuration.IsStartupWizardCompleted = true;
                        appHost.TheaterConfigurationManager.SaveConfiguration();

                        appHost.Restart().Wait();
                    } else {
                        appHost.Shutdown().Wait();
                    }
                } else {
                    appHost.RunUserInterface();
                }

                return appHost.RestartOnExit;
            }
#if !DEBUG
            } catch (Exception ex) {
                logger.ErrorException("Error launching application", ex);

                MessageBox.Show("There was an error launching Media Browser Theater: " + ex.Message);
                
                // Shutdown the app with an error code
                Environment.Exit(1);
                return false;
            }
#endif
        }
    }
}