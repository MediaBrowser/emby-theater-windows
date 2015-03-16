using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Windows.Forms;
using CLAP;
using MediaBrowser.Common.Implementations.Logging;
using MediaBrowser.Model.Logging;
using MediaBrowser.Theater.Api.Configuration;
using ConfigurationManager = System.Configuration.ConfigurationManager;
using MessageBox = System.Windows.MessageBox;

namespace MediaBrowser.Theater
{
    public static class Program
    {
        [STAThread]
        public static int Main(string[] args)
        {
            Parser.Run<Launcher>(args);
            return Launcher.StatusCode;
        }
    }

    public class Launcher
    {
        public const string PackageName = "MediaBrowser.Theater";

        public static int StatusCode { get; private set; }


        [Verb]
        public static void Setup()
        {
            bool completed = false;

            Start(false, appHost => { completed = appHost.RunStartupWizard(); });

            StatusCode = completed ? 0 : 1;
        }

        [Empty]
        public static void RunApplication()
        {
            bool restart = false;

            RunExclusive(() => Start(true, appHost => {
                if (!appHost.TheaterConfigurationManager.Configuration.IsStartupWizardCompleted) {
                    bool completed = RunStartupWizardProcess();
                    if (completed) {
                        appHost.TheaterConfigurationManager.Configuration.IsStartupWizardCompleted = true;
                        appHost.TheaterConfigurationManager.SaveConfiguration();
                    }

                    restart = completed;
                } else {
                    appHost.RunUserInterface();
                    restart = appHost.RestartOnExit;
                }
            }));

            if (restart) {
                Application.Restart();
            }
        }

        private static bool RunStartupWizardProcess()
        {
            var startInfo = new ProcessStartInfo {
                FileName = Process.GetCurrentProcess().MainModule.FileName,
                Arguments = "setup",
                Verb = "runas"
            };

#if DEBUG
            if (startInfo.FileName.EndsWith(".vshost.exe")) {
                startInfo.FileName = startInfo.FileName.Substring(0, startInfo.FileName.Length - ".vshost.exe".Length) + ".exe";
            }
#endif

            using (Process process = Process.Start(startInfo)) {
                process.WaitForExit();
                return process.ExitCode == 0;
            }
        }

        private static void RunExclusive(Action action)
        {
            Mutex singleInstanceMutex = null;

            try {
                bool createdNew;
                singleInstanceMutex = new Mutex(true, @"Local\" + typeof (Program).Assembly.GetName().Name, out createdNew);

                if (!createdNew) {
                    singleInstanceMutex = null;
                    return;
                }

                action();
            }
            finally {
                if (singleInstanceMutex != null) {
                    singleInstanceMutex.ReleaseMutex();
                    singleInstanceMutex.Close();
                    singleInstanceMutex.Dispose();
                }
            }
        }

        private static void Start(bool installUpdates, Action<ApplicationHost> startup)
        {
            string appPath = Process.GetCurrentProcess().MainModule.FileName;
            var appPaths = new ApplicationPaths(GetProgramDataPath(appPath), appPath);
            var logManager = new NlogManager(appPaths.LogDirectoryPath, "theater");
            logManager.ReloadLogger(LogSeverity.Debug);
            logManager.AddConsoleOutput();

            bool updateInstalling = installUpdates && InstallUpdatePackage(appPaths, logManager);
            if (!updateInstalling) {
                LaunchApplication(appPaths, logManager, startup);
            }
        }


//        [STAThread]
//        public static void Main(string[] args)
//        {
//            Mutex singleInstanceMutex = null;
//            bool restartOnExit = false;
//
//            try {
//                bool createdNew;
//                singleInstanceMutex = new Mutex(true, @"Local\" + typeof (Program).Assembly.GetName().Name, out createdNew);
//
//                if (!createdNew) {
//                    singleInstanceMutex = null;
//                    return;
//                }
//
//                string appPath = Process.GetCurrentProcess().MainModule.FileName;
//                var appPaths = new ApplicationPaths(GetProgramDataPath(appPath), appPath);
//                var logManager = new NlogManager(appPaths.LogDirectoryPath, "theater");
//                logManager.ReloadLogger(LogSeverity.Debug);
//                logManager.AddConsoleOutput();
//
//                bool updateInstalling = InstallUpdatePackage(appPaths, logManager);
//                if (!updateInstalling) {
//                    restartOnExit = LaunchApplication(appPaths, logManager);
//                }
//            }
//            finally {
//                if (singleInstanceMutex != null) {
//                    singleInstanceMutex.ReleaseMutex();
//                    singleInstanceMutex.Close();
//                    singleInstanceMutex.Dispose();
//                }
//            }
//
//            if (restartOnExit) {
//                Application.Restart();
//            }
//        }

        public static string GetProgramDataPath(string applicationPath)
        {
            bool useDebugPath = false;

#if DEBUG
            useDebugPath = true;
#endif

            string programDataPath = useDebugPath ?
                                         ConfigurationManager.AppSettings["DebugProgramDataPath"] :
                                         ConfigurationManager.AppSettings["ReleaseProgramDataPath"];

            programDataPath = programDataPath.Replace("%ApplicationData%", Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData));

            // If it's a relative path, e.g. "..\"
            if (!Path.IsPathRooted(programDataPath)) {
                string path = Path.GetDirectoryName(applicationPath);

                if (string.IsNullOrEmpty(path)) {
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

        private static void LaunchApplication(ApplicationPaths appPaths, NlogManager logManager, Action<ApplicationHost> startup)
        {
#if !DEBUG
            ILogger logger = logManager.GetLogger("App");
            try {
#endif
            using (var appHost = new ApplicationHost(appPaths, logManager)) {
                appHost.Init(new Progress<double>()).Wait();
                startup(appHost);
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