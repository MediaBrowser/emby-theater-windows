using Emby.Theater.App;
using MediaBrowser.Model.Logging;
using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Threading;
using Emby.Theater.IO;
using Emby.Theater.Logging;
using Emby.Theater.Networking;
using MediaBrowser.Common.Configuration;
using MediaBrowser.Common.Net;
using MediaBrowser.Model.IO;
using MediaBrowser.Model.System;
using Microsoft.Win32;

namespace Emby.Theater
{
    static class Program
    {
        public static string UpdatePackageName
        {
            get
            {
                if (Environment.Is64BitOperatingSystem)
                {
                    return "emby-theater-x64.zip";
                }

                return "emby-theater-x86.zip";
            }
        }

        public static bool Is64BitElectron
        {
            get { return Environment.Is64BitOperatingSystem; }
        }

        private static ILogger _logger;
        private static IApplicationPaths _appPaths;
        private static ILogManager _logManager;

        public static string ApplicationPath;

        private static IFileSystem FileSystem;
        private static bool _restartOnShutdown;

        /// <summary>
        /// /// The main entry point for the application.
        /// /// </summary>
        [STAThread]
        static void Main()
        {
            ApplicationPath = Assembly.GetEntryAssembly().Location;

            var environmentInfo = GetEnvironmentInfo();

            var appPaths = new ApplicationPaths(GetProgramDataPath(ApplicationPath), ApplicationPath);
            _appPaths = appPaths;

            using (var logManager = new SimpleLogManager(appPaths.LogDirectoryPath, "server"))
            {
                _logManager = logManager;
                logManager.ReloadLogger(LogSeverity.Debug);
                logManager.AddConsoleOutput();

                var logger = _logger = logManager.GetLogger("Main");

                logger.Info("Application path: {0}", ApplicationPath);

                ApplicationHost.LogEnvironmentInfo(logger, appPaths, true);

                AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

                // Mutex credit: https://stackoverflow.com/questions/229565/what-is-a-good-pattern-for-using-a-global-mutex-in-c/229567

                // unique id for global mutex - Global prefix means it is global to the machine
                string mutexId = string.Format("Global\\{{{0}}}", "EmbyServer");

                // Need a place to store a return value in Mutex() constructor call
                bool createdNew;

                // edited by MasonGZhwiti to prevent race condition on security settings via VanNguyen
                using (var mutex = new Mutex(false, mutexId, out createdNew))
                {
                    // edited by acidzombie24
                    var hasHandle = false;
                    try
                    {
                        // note, you may want to time out here instead of waiting forever
                        // edited by acidzombie24
                        hasHandle = mutex.WaitOne(5000, false);
                        if (hasHandle == false)
                        {
                            logger.Info("Exiting because another instance is already running.");
                            return;
                        }
                    }
                    catch (AbandonedMutexException)
                    {
                        // Log the fact that the mutex was abandoned in another process,
                        // it will still get acquired
                        hasHandle = true;
                        logger.Info("Mutex was abandoned in another process.");
                    }

                    using (new MutexHandle(mutex, hasHandle, _logger))
                    {
                        if (PerformUpdateIfNeeded(appPaths, environmentInfo, logger))
                        {
                            logger.Info("Exiting to perform application update.");
                            return;
                        }

                        RunApplication(appPaths, logManager, environmentInfo, new StartupOptions(Environment.GetCommandLineArgs()));
                    }
                }

                logger.Info("Shutdown complete");

                if (_restartOnShutdown)
                {
                    // This is artificial, but add some delay to ensure sockets are released.
                    var delay = environmentInfo.OperatingSystem == MediaBrowser.Model.System.OperatingSystem.Windows
                        ? 5000
                        : 60000;

                    var task = Task.Delay(delay);
                    Task.WaitAll(task);

                    logger.Info("Starting new server process");
                    var restartCommandLine = GetRestartCommandLine();

                    Process.Start(restartCommandLine.Item1, restartCommandLine.Item2);
                }
            }
        }

        public static Tuple<string, string> GetRestartCommandLine()
        {
            var currentProcess = Process.GetCurrentProcess();
            var processModulePath = currentProcess.MainModule.FileName;

            return new Tuple<string, string>(processModulePath, Environment.CommandLine);
        }
        /// <summary>
        /// Runs the application.
        /// </summary>
        private static void RunApplication(ApplicationPaths appPaths, ILogManager logManager, IEnvironmentInfo environmentInfo, StartupOptions options)
        {
            var fileSystem = new ManagedFileSystem(logManager.GetLogger("FileSystem"), environmentInfo, appPaths.TempDirectory);

            FileSystem = fileSystem;

            INetworkManager networkManager = new NetworkManager(logManager.GetLogger("NetworkManager"));

            using (var appHost = new ApplicationHost(appPaths,
                logManager,
                options,
                fileSystem,
                null,
                UpdatePackageName,
                environmentInfo,
                new Emby.Theater.App.SystemEvents(logManager.GetLogger("SystemEvents")),
                networkManager))
            {
                var initProgress = new Progress<double>();

                Func<IHttpClient> httpClient = () => appHost.HttpClient;

                using (new ElectronApp(_logger, appPaths, httpClient, environmentInfo))
                {
                    var task = appHost.Init(initProgress);
                    Task.WaitAll(task);

                    task = InstallVcredist2015IfNeeded(appHost.HttpClient, _logger);
                    Task.WaitAll(task);

                    task = InstallCecDriver(appPaths, appHost.HttpClient);
                    Task.WaitAll(task);

                    using (var server = new TheaterServer(_logger, appHost))
                    {
                        task = appHost.RunStartupTasks();
                        Task.WaitAll(task);

                        Application.EnableVisualStyles();
                        Application.SetCompatibleTextRenderingDefault(false);

                        Application.Run(new AppContext(server));
                    }
                }
            }
        }

        public static void Exit()
        {
            Application.Exit();
        }

        private static IEnvironmentInfo GetEnvironmentInfo()
        {
            var info = new EnvironmentInfo();

            return info;
        }

        /// <summary>
        /// Performs the update if needed.
        /// </summary>
        private static bool PerformUpdateIfNeeded(IApplicationPaths appPaths, IEnvironmentInfo environmentInfo, ILogger logger)
        {
            //// Look for the existence of an update archive
            var updateArchive = Path.Combine(appPaths.TempUpdatePath, UpdatePackageName);

            if (File.Exists(updateArchive))
            {
                logger.Info("An update is available from {0}", updateArchive);

                // Update is there - execute update
                try
                {
                    new ApplicationUpdater().UpdateApplication(appPaths, updateArchive, logger);

                    // And just let the app exit so it can update
                    return true;
                }
                catch (Exception e)
                {
                    logger.ErrorException("Error running updater.", e);
                }
            }

            return false;
        }

        private static async Task InstallVcredist2013IfNeeded(IHttpClient httpClient, ILogger logger)
        {
            // Reference 
            // http://stackoverflow.com/questions/12206314/detect-if-visual-c-redistributable-for-visual-studio-2012-is-installed

            try
            {
                var subkey = Environment.Is64BitProcess
                    ? "SOFTWARE\\WOW6432Node\\Microsoft\\VisualStudio\\12.0\\VC\\Runtimes\\x64"
                    : "SOFTWARE\\Microsoft\\VisualStudio\\12.0\\VC\\Runtimes\\x86";

                using (RegistryKey ndpKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Default)
                    .OpenSubKey(subkey))
                {
                    if (ndpKey != null && ndpKey.GetValue("Version") != null)
                    {
                        var installedVersion = ((string)ndpKey.GetValue("Version")).TrimStart('v');
                        if (installedVersion.StartsWith("12", StringComparison.OrdinalIgnoreCase))
                        {
                            return;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                logger.ErrorException("Error getting .NET Framework version", ex);
                return;
            }

            try
            {
                await InstallVcredist(httpClient, GetVcredist2013Url()).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                logger.ErrorException("Error installing Visual Studio C++ runtime", ex);
            }
        }

        private static string GetVcredist2013Url()
        {
            if (Is64BitElectron)
            {
                return "https://github.com/MediaBrowser/Emby.Resources/raw/master/vcredist2013/vcredist_x64.exe";
            }

            // TODO: ARM url - https://github.com/MediaBrowser/Emby.Resources/raw/master/vcredist2013/vcredist_arm.exe

            return "https://github.com/MediaBrowser/Emby.Resources/raw/master/vcredist2013/vcredist_x86.exe";
        }

        private static async Task InstallVcredist2015IfNeeded(IHttpClient httpClient, ILogger logger)
        {
            // Reference 
            // http://stackoverflow.com/questions/12206314/detect-if-visual-c-redistributable-for-visual-studio-2012-is-installed

            try
            {
                RegistryKey key;

                if (Environment.Is64BitProcess)
                {
                    key = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Default)
                        .OpenSubKey("SOFTWARE\\Classes\\Installer\\Dependencies\\{d992c12e-cab2-426f-bde3-fb8c53950b0d}");

                    if (key == null)
                    {
                        key = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Default)
                            .OpenSubKey("SOFTWARE\\WOW6432Node\\Microsoft\\VisualStudio\\14.0\\VC\\Runtimes\\x64");
                    }
                }
                else
                {
                    key = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Default)
                        .OpenSubKey("SOFTWARE\\Classes\\Installer\\Dependencies\\{e2803110-78b3-4664-a479-3611a381656a}");

                    if (key == null)
                    {
                        key = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Default)
                            .OpenSubKey("SOFTWARE\\Microsoft\\VisualStudio\\14.0\\VC\\Runtimes\\x86");
                    }
                }

                if (key != null)
                {
                    using (key)
                    {
                        var version = key.GetValue("Version");
                        if (version != null)
                        {
                            var installedVersion = ((string)version).TrimStart('v');
                            if (installedVersion.StartsWith("14", StringComparison.OrdinalIgnoreCase))
                            {
                                return;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                logger.ErrorException("Error getting .NET Framework version", ex);
                return;
            }

            try
            {
                await InstallVcredist(httpClient, GetVcredist2015Url()).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                logger.ErrorException("Error installing Visual Studio C++ runtime", ex);
            }
        }

        private static string GetVcredist2015Url()
        {
            if (Is64BitElectron)
            {
                return "https://github.com/MediaBrowser/Emby.Resources/raw/master/vcredist2015/vc_redist.x64.exe";
            }

            // TODO: ARM url - https://github.com/MediaBrowser/Emby.Resources/raw/master/vcredist2015/vcredist_arm.exe

            return "https://github.com/MediaBrowser/Emby.Resources/raw/master/vcredist2015/vc_redist.x86.exe";
        }

        private async static Task InstallVcredist(IHttpClient httpClient, string url)
        {
            var tmp = await httpClient.GetTempFile(new HttpRequestOptions
            {
                Url = url,
                Progress = new Progress<double>()

            }).ConfigureAwait(false);

            var exePath = Path.ChangeExtension(tmp, ".exe");
            File.Copy(tmp, exePath);

            var startInfo = new ProcessStartInfo
            {
                FileName = exePath,

                CreateNoWindow = true,
                WindowStyle = ProcessWindowStyle.Hidden,
                Verb = "runas",
                ErrorDialog = false
            };

            _logger.Info("Running {0}", startInfo.FileName);

            using (var process = Process.Start(startInfo))
            {
                process.WaitForExit();
            }
        }

        private static async Task InstallCecDriver(IApplicationPaths appPaths, IHttpClient httpClient)
        {
            var path = Path.Combine(appPaths.ProgramDataPath, "cec-driver");
            Directory.CreateDirectory(path);

            var cancelPath = Path.Combine(path, "cancel");
            if (File.Exists(cancelPath))
            {
                _logger.Info("HDMI CEC driver installation was previously cancelled.");
                return;
            }

            if (File.Exists(Path.Combine(path, "p8usb-cec.inf")))
            {
                _logger.Info("HDMI CEC driver already installed.");

                // Needed by CEC
                await InstallVcredist2013IfNeeded(httpClient, _logger).ConfigureAwait(false);

                return;
            }

            var result = MessageBox.Show("Click OK to install the PulseEight HDMI CEC driver, which allows you to control Emby Theater with your HDTV remote control (compatible hardware required).", "HDMI CEC Driver", MessageBoxButtons.OKCancel);

            if (result == DialogResult.Cancel)
            {
                File.Create(cancelPath);
                return;
            }

            // Needed by CEC
            await InstallVcredist2013IfNeeded(httpClient, _logger).ConfigureAwait(false);

            try
            {
                var installerPath = Path.Combine(Path.GetDirectoryName(ApplicationPath), "cec", "p8-usbcec-driver-installer.exe");

                using (var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        Arguments = " /S /D=" + path,
                        FileName = installerPath,
                        CreateNoWindow = true,
                        WindowStyle = ProcessWindowStyle.Hidden,
                        Verb = "runas",
                        ErrorDialog = false
                    }
                })
                {
                    process.Start();
                    process.WaitForExit();
                }
            }
            catch (Exception ex)
            {
                _logger.ErrorException("Error installing cec driver", ex);
            }
        }

        public static string GetProgramDataPath(string applicationPath)
        {
            var programDataPath = System.Configuration.ConfigurationManager.AppSettings["ProgramDataPath"];

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

            if (string.Equals(Path.GetFileName(Path.GetDirectoryName(applicationPath)), "system", StringComparison.OrdinalIgnoreCase))
            {
                programDataPath = Path.GetDirectoryName(programDataPath);
            }

            Directory.CreateDirectory(programDataPath);

            return programDataPath;
        }

        /// <summary>
        /// Handles the UnhandledException event of the CurrentDomain control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="UnhandledExceptionEventArgs"/> instance containing the event data.</param>
        static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            var exception = (Exception)e.ExceptionObject;

            new UnhandledExceptionWriter(_appPaths, _logger, _logManager, FileSystem, new ConsoleLogger()).Log(exception);

            if (!Debugger.IsAttached)
            {
                Environment.Exit(System.Runtime.InteropServices.Marshal.GetHRForException(exception));
            }
        }
    }
}
