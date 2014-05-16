using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using MediaBrowser.Common.Net;
using MediaBrowser.Model.Logging;
using Microsoft.Win32;

namespace MediaBrowser.Theater.Api.System
{
    public class MediaFilters : IMediaFilters
    {
        private const string CodecsRegistryKey = @"HKEY_LOCAL_MACHINE\SOFTWARE\Classes\Wow6432Node\CLSID\{083863F1-70DE-11D0-BD40-00A0C911CE86}\Instance\";

        private readonly IHttpClient _httpClient;
        private readonly ILogger _logger;

        public MediaFilters(IHttpClient httpClient, ILogManager logManager)
        {
            _httpClient = httpClient;
            _logger = logManager.GetLogger("MediaFilters");
        }

        public bool IsXyVsFilterInstalled()
        {
            // Returns true if XY-VSFilter is installed
            var id = Registry.GetValue(CodecsRegistryKey + "{93A22E7A-5091-45EF-BA61-6DA26156A5D0}", "CLSID", "id");
            return id != null;
        }

        public async Task InstallXyVsFilter(IProgress<double> progress, CancellationToken cancellationToken)
        {
            // Guess we'll have to hard-code the latest version?

            const string url = "http://xy-vsfilter.googlecode.com/files/xy-VSFilter_3.0.0.211_Installer.exe";

            string tempFile = await _httpClient.GetTempFile(new HttpRequestOptions {
                Url = url,
                CancellationToken = cancellationToken,
                Progress = progress
            });

            string exePath = Path.ChangeExtension(tempFile, ".exe");
            File.Move(tempFile, exePath);

            try {
                using (Process process = Process.Start(exePath)) {
                    process.WaitForExit();
                }
            }
            finally {
                try {
                    File.Delete(exePath);
                }
                catch (Exception ex) {
                    _logger.ErrorException("Error deleting {0}", ex, exePath);
                }
            }
        }

        public bool IsXySubFilterInstalled()
        {
            var id = Registry.GetValue(CodecsRegistryKey + "{2DFCB782-EC20-4A7C-B530-4577ADB33F21}", "CLSID", "id");
            return id != null;
        }

        public async Task InstallXySubFilter(IProgress<double> progress, CancellationToken cancellationToken)
        {
             // Guess we'll have to hard-code the latest version?

            const string url = "https://xy-vsfilter.googlecode.com/files/XySubFilter_3.1.0.546_BETA_Installer_v2.exe";

            string tempFile = await _httpClient.GetTempFile(new HttpRequestOptions {
                Url = url,
                CancellationToken = cancellationToken,
                Progress = progress
            });

            string exePath = Path.ChangeExtension(tempFile, ".exe");
            File.Move(tempFile, exePath);

            try {
                using (Process process = Process.Start(exePath)) {
                    process.WaitForExit();
                }
            }
            finally {
                try {
                    File.Delete(exePath);
                }
                catch (Exception ex) {
                    _logger.ErrorException("Error deleting {0}", ex, exePath);
                }
            }
        }

        public bool IsLavSplitterInstalled()
        {
            // Returns true if 32-bit splitter are installed
            var id = Registry.GetValue(CodecsRegistryKey + "{B98D13E7-55DB-4385-A33D-09FD1BA26338}", "CLSID", "id");
            return id != null;
        }

        public bool IsLavAudioInstalled()
        {
            // Returns true if 32-bit splitter + audio + video are installed
            var id = Registry.GetValue(CodecsRegistryKey + "{E8E73B6B-4CB3-44A4-BE99-4F7BCB96E491}", "CLSID", "id");
            return id != null;
        }

        public bool IsLavVideoInstalled()
        {
            // Returns true if 32-bit splitter + audio + video are installed
            var id = Registry.GetValue(CodecsRegistryKey + "{EE30215D-164F-4A92-A4EB-9D4C13390F9F}", "CLSID", "id");
            return id != null;
        }

        public async Task InstallLavFilters(IProgress<double> progress, CancellationToken cancellationToken)
        {
            // Guess we'll have to hard-code the latest version?
            // https://code.google.com/p/lavfilters/downloads/list

            const string url = "https://lavfilters.googlecode.com/files/LAVFilters-0.60.1.exe";

            string tempFile = await _httpClient.GetTempFile(new HttpRequestOptions {
                Url = url,
                CancellationToken = cancellationToken,
                Progress = progress
            });

            string exePath = Path.ChangeExtension(tempFile, ".exe");
            File.Move(tempFile, exePath);

            try {
                using (Process process = Process.Start(exePath)) {
                    process.WaitForExit();
                }
            }
            finally {
                try {
                    File.Delete(exePath);
                }
                catch (Exception ex) {
                    _logger.ErrorException("Error deleting {0}", ex, exePath);
                }
            }
        }


        public void LaunchLavAudioConfiguration()
        {
            string lavPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), "LAV Filters\\x86", "LAVAudio.ax");

            OpenLavConfiguration(lavPath);
        }

        public void LaunchLavSplitterConfiguration()
        {
            string lavPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), "LAV Filters\\x86", "LAVSplitter.ax");

            OpenLavConfiguration(lavPath);
        }

        public bool IsMadVrInstalled()
        {
            var id = Registry.GetValue(CodecsRegistryKey + "{E1A8B82A-32CE-4B0D-BE0D-AA68C772E423}", "CLSID", "id");
            return id != null;
        }

        public bool IsReClockInstalled()
        {
            var id = Registry.GetValue(CodecsRegistryKey + "{9DC15360-914C-46B8-B9DF-BFE67FD36C6A}", "CLSID", "id");
            return id != null;
        }

        public async Task InstallReClock(IProgress<double> progress, CancellationToken cancellationToken)
        {
            // Guess we'll have to hard-code the latest version?
            // http://www.videohelp.com/tools/ReClock-Directshow-Filter

            const string url = "http://www.videohelp.com/download/SetupReClock1883.exe";

            string tempFile = await _httpClient.GetTempFile(new HttpRequestOptions
            {
                Url = url,
                CancellationToken = cancellationToken,
                Progress = progress
            });

            string exePath = Path.ChangeExtension(tempFile, ".exe");
            File.Move(tempFile, exePath);

            try
            {
                using (Process process = Process.Start(exePath))
                {
                    process.WaitForExit();
                }
            }
            finally
            {
                try
                {
                    File.Delete(exePath);
                }
                catch (Exception ex)
                {
                    _logger.ErrorException("Error deleting {0}", ex, exePath);
                }
            }
        }

        private void OpenLavConfiguration(string path)
        {
            string rundllPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.SystemX86), "rundll32.exe");

            string args = string.Format("\"{0}\",OpenConfiguration", path);

            Process.Start(rundllPath, args);
        }
    }
}