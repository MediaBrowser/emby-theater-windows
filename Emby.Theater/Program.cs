using Emby.Theater.App;
using MediaBrowser.Common.Implementations.Logging;
using MediaBrowser.Model.Logging;
using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Emby.Theater.DirectShow.Window;
using Emby.Theater.Window;
using MediaBrowser.Common.Configuration;
using MediaBrowser.Common.Net;
using Microsoft.Win32;

namespace Emby.Theater
{
    static class Program
    {
        public static string UpdatePackageName = "emby.theater.zip";

        private static Mutex _singleInstanceMutex;
        private static ApplicationHost _appHost;
        private static ILogger _logger;

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

            bool createdNew;

            _singleInstanceMutex = new Mutex(true, @"Local\" + typeof(Program).Assembly.GetName().Name, out createdNew);

            if (!createdNew)
            {
                _singleInstanceMutex = null;
                return;
            }

            var appPath = Process.GetCurrentProcess().MainModule.FileName;

            // Look for the existence of an update archive
            var appPaths = new ApplicationPaths(GetProgramDataPath(appPath), appPath);
            var logManager = new NlogManager(appPaths.LogDirectoryPath, "theater");
            logManager.ReloadLogger(LogSeverity.Debug);

            var updateArchive = Path.Combine(appPaths.TempUpdatePath, UpdatePackageName);

            if (File.Exists(updateArchive))
            {
                ReleaseMutex();

                // Update is there - execute update
                try
                {
                    new ApplicationUpdater().UpdateApplication(appPaths, updateArchive,
                        logManager.GetLogger("ApplicationUpdater"));

                    // And just let the app exit so it can update
                    return;
                }
                catch (Exception e)
                {
                    MessageBox.Show(string.Format("Error attempting to update application.\n\n{0}\n\n{1}",
                        e.GetType().Name, e.Message));
                }
            }

            _logger = logManager.GetLogger("App");

            bool supportsTransparency;

            try
            {
                supportsTransparency = NativeWindowMethods.DwmIsCompositionEnabled();
            }
            catch (Exception ex)
            {
                _logger.ErrorException("Error in DwmIsCompositionEnabled", ex);
                supportsTransparency = true;
            }

            _logger.Info("OS Supports window transparency?: {0}", supportsTransparency);

            try
            {
                var task = InstallVcredist2015IfNeeded(_appHost, _logger);
                Task.WaitAll(task);

                _appHost = new ApplicationHost(appPaths, logManager);

                var initTask = _appHost.Init(new Progress<Double>());
                Task.WaitAll(initTask);

                task = InstallCecDriver(appPaths);
                Task.WaitAll(task);

                var electronTask = StartElectron(appPaths, supportsTransparency);
                Task.WaitAll(electronTask);

                var electronProcess = electronTask.Result;

                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);

                _mainForm = new MainForm(_logger, _appHost.TheaterConfigurationManager, _appHost, electronProcess);
                Application.Run(_mainForm);
            }
            catch (Exception ex)
            {
                _logger.ErrorException("Error launching application", ex);

                MessageBox.Show("There was an error launching Emby: " + ex.Message);

                // Shutdown the app with an error code
                Environment.Exit(1);
            }
            finally
            {
                ReleaseMutex();
            }
        }

        private static async Task InstallVcredist2013IfNeeded(ApplicationHost appHost, ILogger logger)
        {
            // Reference 
            // http://stackoverflow.com/questions/12206314/detect-if-visual-c-redistributable-for-visual-studio-2012-is-installed

            try
            {
                var subkey = Environment.Is64BitOperatingSystem
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
                await InstallVcredist2013().ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                logger.ErrorException("Error installing Visual Studio C++ runtime", ex);
            }
        }

        private async static Task InstallVcredist2013()
        {
            var httpClient = _appHost.HttpClient;

            var tmp = await httpClient.GetTempFile(new HttpRequestOptions
            {
                Url = GetVcredist2013Url(),
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

        private static string GetVcredist2013Url()
        {
            if (Environment.Is64BitOperatingSystem)
            {
                return "https://github.com/MediaBrowser/Emby.Resources/raw/master/vcredist2013/vcredist_x64.exe";
            }

            // TODO: ARM url - https://github.com/MediaBrowser/Emby.Resources/raw/master/vcredist2013/vcredist_arm.exe

            return "https://github.com/MediaBrowser/Emby.Resources/raw/master/vcredist2013/vcredist_x86.exe";
        }

        private static async Task InstallVcredist2015IfNeeded(ApplicationHost appHost, ILogger logger)
        {
            // Reference 
            // http://stackoverflow.com/questions/12206314/detect-if-visual-c-redistributable-for-visual-studio-2012-is-installed

            try
            {
                var subkey = Environment.Is64BitOperatingSystem
                    ? "SOFTWARE\\WOW6432Node\\Microsoft\\VisualStudio\\14.0\\VC\\Runtimes\\x64"
                    : "SOFTWARE\\Microsoft\\VisualStudio\\14.0\\VC\\Runtimes\\x86";

                using (RegistryKey ndpKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Default)
                    .OpenSubKey(subkey))
                {
                    if (ndpKey != null && ndpKey.GetValue("Version") != null)
                    {
                        var installedVersion = ((string)ndpKey.GetValue("Version")).TrimStart('v');
                        if (installedVersion.StartsWith("14", StringComparison.OrdinalIgnoreCase))
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

            var arch = Environment.Is64BitOperatingSystem ? "x64" : "x86";

            var msg = string.Format(
                    "The Visual C++ 2015 {0} Redistributable is required. Click OK to open the Microsoft website where you can install it. When asked to select x64 or x86, select {0}. After you have completed the installation, please run Emby Theater again.",
                    arch);

            MessageBox.Show(msg);

            Process.Start("https://www.microsoft.com/en-us/download/details.aspx?id=48145");

            Environment.Exit(0);
        }

        private static async Task InstallCecDriver(IApplicationPaths appPaths)
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
                await InstallVcredist2013IfNeeded(_appHost, _logger).ConfigureAwait(false);

                return;
            }

            var result = MessageBox.Show("Click OK to install the PulseEight HDMI CEC driver, which allows you to control Emby Theater with your HDTV remote control (compatible hardware required).", "HDMI CEC Driver", MessageBoxButtons.OKCancel);

            if (result == DialogResult.Cancel)
            {
                File.Create(cancelPath);
                return;
            }

            // Needed by CEC
            await InstallVcredist2013IfNeeded(_appHost, _logger).ConfigureAwait(false);

            try
            {
                var installerPath = Path.Combine(Path.GetDirectoryName(appPaths.ApplicationPath), "cec", "p8-usbcec-driver-installer.exe");

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

        private static async Task<Process> StartElectron(IApplicationPaths appPaths, bool supportsTransparency)
        {
            var appDirectoryPath = Path.GetDirectoryName(appPaths.ApplicationPath);

            var architecture = Environment.Is64BitOperatingSystem ? "x64" : "x86";
            var electronExePath = Path.Combine(appDirectoryPath, architecture, "electron", "electron.exe");
            var electronAppPath = Path.Combine(appDirectoryPath, "electronapp");

            var dataPath = Path.Combine(appPaths.DataPath, "electron");

            var cecPath = Path.Combine(Path.GetDirectoryName(appPaths.ApplicationPath), "cec");
            if (Environment.Is64BitOperatingSystem)
            {
                cecPath = Path.Combine(cecPath, "cec-client.x64.exe");
            }
            else
            {
                cecPath = Path.Combine(cecPath, "cec-client.exe");
            }

            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    UseShellExecute = false,

                    FileName = electronExePath,
                    Arguments = string.Format("\"{0}\" \"{1}\" \"{2}\" \"{3}\"", electronAppPath, dataPath, supportsTransparency.ToString().ToLower(), cecPath)
                },

                EnableRaisingEvents = true,
            };

            _logger.Info("{0} {1}", process.StartInfo.FileName, process.StartInfo.Arguments);

            process.Start();
            process.Exited += process_Exited;

            //process.WaitForInputIdle(3000);

            while (process.MainWindowHandle.Equals(IntPtr.Zero))
            {
                await Task.Delay(50);
            }
            return process;
        }

        static void process_Exited(object sender, EventArgs e)
        {
            _appHost.Shutdown();
        }

        private static Form _mainForm;
        public static void Exit()
        {
            if (_mainForm != null)
            {
                _mainForm.InvokeIfRequired(() =>
                {
                    _mainForm.Close();
                    Application.Exit();
                });
            }
        }

        /// <summary>
        /// Releases the mutex.
        /// </summary>
        private static void ReleaseMutex()
        {
            if (_singleInstanceMutex == null)
            {
                return;
            }

            _singleInstanceMutex.ReleaseMutex();
            _singleInstanceMutex.Close();
            _singleInstanceMutex.Dispose();
            _singleInstanceMutex = null;
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

            new UnhandledExceptionWriter(_appHost.TheaterConfigurationManager.ApplicationPaths, _logger, _appHost.LogManager).Log(exception);

            MessageBox.Show("Unhandled exception: " + exception.Message);

            if (!Debugger.IsAttached)
            {
                Environment.Exit(Marshal.GetHRForException(exception));
            }
        }
    }
}
