using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using MediaBrowser.Common.Net;
using MediaBrowser.Model.Logging;

namespace MediaBrowser.Theater.Api.System
{
    public class MediaFilters : IMediaFilters
    {
        #region COM Prerequisites

        // Used for prerequisite detection only. These were copied from the imports used in the direct show solution
        [ComImport, Guid("E8E73B6B-4CB3-44A4-BE99-4F7BCB96E491")]
        internal class LAVAudio { }

        [ComImport, Guid("B98D13E7-55DB-4385-A33D-09FD1BA26338")]
        internal class LAVSplitter { }

        [ComImport, Guid("EE30215D-164F-4A92-A4EB-9D4C13390F9F")]
        internal class LAVVideo { }

        [ComImport, Guid("93A22E7A-5091-45EF-BA61-6DA26156A5D0")]
        internal class XYVSFilter { }

        [ComImport, Guid("2DFCB782-EC20-4A7C-B530-4577ADB33F21")]
        internal class XySubFilter { }

        [ComImport, Guid("E1A8B82A-32CE-4B0D-BE0D-AA68C772E423")]
        internal class MadVR { }

        [ComImport, Guid("9DC15360-914C-46B8-B9DF-BFE67FD36C6A")]
        internal class ReclockAudioRenderer { }

        #endregion

        private readonly IHttpClient _httpClient;
        private readonly ILogger _logger;

        public MediaFilters(IHttpClient httpClient, ILogger logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public bool IsXyVsFilterInstalled()
        {
            // Returns true if XY-VSFilter is installed
            return CanInstantiate<XYVSFilter>();
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
            return CanInstantiate<XySubFilter>();
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
            return CanInstantiate<LAVSplitter>();
        }

        public bool IsLavAudioInstalled()
        {
            // Returns true if 32-bit splitter + audio + video are installed
            return CanInstantiate<LAVAudio>();
        }

        public bool IsLavVideoInstalled()
        {
            // Returns true if 32-bit splitter + audio + video are installed
            return CanInstantiate<LAVVideo>();
        }

        public async Task InstallLavFilters(IProgress<double> progress, CancellationToken cancellationToken)
        {
            // Guess we'll have to hard-code the latest version?
            // https://code.google.com/p/lavfilters/downloads/list

            const string url = "https://lavfilters.googlecode.com/files/LAVFilters-0.59.1.exe";

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
            return CanInstantiate<MadVR>();
        }

        public bool IsReClockInstalled()
        {
            return CanInstantiate<ReclockAudioRenderer>();
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

        private bool CanInstantiate<T>() where T : new()
        {
            try {
                var obj = new T();
            }
            catch (Exception ex) {
                return false;
            }

            return true;
        }

        private void OpenLavConfiguration(string path)
        {
            string rundllPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.SystemX86), "rundll32.exe");

            string args = string.Format("\"{0}\",OpenConfiguration", path);

            Process.Start(rundllPath, args);
        }
    }
}