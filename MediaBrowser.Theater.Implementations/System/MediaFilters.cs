using MediaBrowser.Common.Net;
using MediaBrowser.Model.Logging;
using MediaBrowser.Theater.Interfaces.System;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace MediaBrowser.Theater.Implementations.System
{
    public class MediaFilters : IMediaFilters
    {
        private readonly IHttpClient _httpClient;
        private readonly ILogger _logger;

        public MediaFilters(IHttpClient httpClient, ILogger logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public bool IsXyVsFilterInstalled()
        {
            var path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), "xy-VSFilter");
            
            // Returns true if 32-bit component is installed
            return File.Exists(Path.Combine(path, "VSFilter.dll"));
        }

        public async Task InstallXyVsFilter(IProgress<double> progress, CancellationToken cancellationToken)
        {
            // Guess we'll have to hard-code the latest version?

            const string url = "http://xy-vsfilter.googlecode.com/files/xy-VSFilter_3.0.0.211_Installer.exe";

            var tempFile = await _httpClient.GetTempFile(new HttpRequestOptions
            {
                Url = url,
                CancellationToken = cancellationToken,
                Progress = progress
            });

            var exePath = Path.ChangeExtension(tempFile, ".exe");
            File.Move(tempFile, exePath);

            try
            {
                using (var process = Process.Start(exePath))
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

        public bool IsLavFiltersInstalled()
        {
            // Returns true if 32-bit splitter + audio + video are installed
            var path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), "LAV Filters\\x86");

            return File.Exists(Path.Combine(path, "LAVAudio.ax")) && File.Exists(Path.Combine(path, "LAVVideo.ax")) && File.Exists(Path.Combine(path, "LAVSplitter.ax"));
        }

        public async Task InstallLavFilters(IProgress<double> progress, CancellationToken cancellationToken)
        {
            // Guess we'll have to hard-code the latest version?
            // https://code.google.com/p/lavfilters/downloads/list

            const string url = "https://lavfilters.googlecode.com/files/LAVFilters-0.58.2.exe";

            var tempFile = await _httpClient.GetTempFile(new HttpRequestOptions
            {
                Url = url,
                CancellationToken = cancellationToken,
                Progress = progress
            });

            var exePath = Path.ChangeExtension(tempFile, ".exe");
            File.Move(tempFile, exePath);

            try
            {
                using (var process = Process.Start(exePath))
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
    }
}
