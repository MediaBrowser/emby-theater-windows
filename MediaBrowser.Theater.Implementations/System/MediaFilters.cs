using MediaBrowser.Common.Net;
using MediaBrowser.Model.Logging;
using MediaBrowser.Theater.Interfaces.System;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

namespace MediaBrowser.Theater.Implementations.System
{

    public class MediaFilters : IMediaFilters
    {
        #region COM Prerequisites

        // Used for prerequisite detection only. These were copied from the imports used in the direct show solution
        [ComImport]
        [Guid("171252A0-8820-4AFE-9DF8-5C92B2D66B04")]
        internal class LAVSplitter
        {
        }

        [ComImport]
        [Guid("EE30215D-164F-4A92-A4EB-9D4C13390F9F")]
        internal class LAVVideo
        {
        }

        [ComImport]
        [Guid("E8E73B6B-4CB3-44A4-BE99-4F7BCB96E491")]
        internal class LAVAudio
        {
        }

        [ComImport]
        [Guid("93A22E7A-5091-45EF-BA61-6DA26156A5D0")]
        internal class XYVSFilter
        {
        }

        [ComImport]
        [Guid("2DFCB782-EC20-4A7C-B530-4577ADB33F21")]
        internal class XySubFilter
        {
        }

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
            return this.CanInstantiate<XYVSFilter>();
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
            return this.CanInstantiate<LAVAudio>();
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


        private bool CanInstantiate<T>() where T : new()
        {
            try
            {
                var obj = new T();
            }
            catch (Exception ex)
            {
                return false;
            }

            return true;
        }
    }
}
