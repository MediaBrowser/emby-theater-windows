using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MediaBrowser.Common.Configuration;
using MediaBrowser.Common.Net;
using MediaBrowser.Model.Logging;
using MediaBrowser.Model.System;

namespace Emby.Theater.App
{
    public class ElectronApp : IDisposable
    {
        private Process _process;
        private ILogger _logger;
        private Func<IHttpClient> _httpClient;
        private IApplicationPaths _appPaths;
        private IEnvironmentInfo _environmentInfo;

        public ElectronApp(ILogger logger, IApplicationPaths appPaths, Func<IHttpClient> httpClient, IEnvironmentInfo environment)
        {
            _logger = logger;
            _appPaths = appPaths;
            _httpClient = httpClient;
            _environmentInfo = environment;
            StartProcess();
        }

        public void Dispose()
        {
            CloseProcess();
            GC.SuppressFinalize(this);
        }

        private void StartProcess()
        {
            var appDirectoryPath = Path.GetDirectoryName(Program.ApplicationPath);

            var is64BitElectron = Program.Is64BitElectron;

            var architecture = is64BitElectron ? "x64" : "x86";
            var archPath = Path.Combine(appDirectoryPath, architecture);
            var electronExePath = Path.Combine(archPath, "electron", "electron.exe");
            var electronAppPath = Path.Combine(appDirectoryPath, "electronapp");
            var mpvExePath = Path.Combine(archPath, "mpv", "mpv.exe");

            var dataPath = Path.Combine(_appPaths.DataPath, "electron");

            var cecPath = Path.Combine(Path.GetDirectoryName(Program.ApplicationPath), "cec");
            if (is64BitElectron)
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
                    Arguments = string.Format("\"{0}\" \"{1}\" \"{2}\" \"{3}\"", electronAppPath, dataPath, cecPath, mpvExePath)
                },

                EnableRaisingEvents = true,
            };

            _logger.Info("{0} {1}", process.StartInfo.FileName, process.StartInfo.Arguments);

            process.Exited += Process_Exited;

            process.Start();

            //process.WaitForInputIdle(3000);

            while (process.MainWindowHandle.Equals(IntPtr.Zero))
            {
                var task = Task.Delay(50);
                Task.WaitAll(task);
            }
        }

        private void Process_Exited(object sender, EventArgs e)
        {
            Program.Exit();
        }

        private void CloseProcess()
        {
            var process = _process;

            if (process == null)
            {
                return;
            }

            _logger.Info("Closing electron");

            using (process)
            {
                try
                {
                    var url = "http://127.0.0.1:8024/exit";
                    using (_httpClient().Post(url, new Dictionary<string, string>(), CancellationToken.None).Result)
                    {

                    }
                }
                catch (Exception ex)
                {
                    _logger.ErrorException("Error attempting to close electron", ex);
                }

                var exited = false;

                try
                {
                    _logger.Info("electron WaitForExit");
                    exited = process.WaitForExit(1000);
                }
                catch (Exception ex)
                {
                    _logger.ErrorException("Error in WaitForExit", ex);
                }

                if (exited)
                {
                    _logger.Info("electron exited");
                }
                else
                {
                    try
                    {
                        _logger.Info("electron Kill");
                        process.Kill();
                    }
                    catch
                    {

                    }
                }
            }

            _process = null;
        }
    }
}
