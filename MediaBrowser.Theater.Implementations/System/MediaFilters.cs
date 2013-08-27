using MediaBrowser.Common.Net;
using MediaBrowser.Theater.Interfaces.System;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace MediaBrowser.Theater.Implementations.System
{
    public class MediaFilters : IMediaFilters
    {
        private IHttpClient _httpClient;

        public bool IsXyVsFilterInstalled()
        {
            // Returns true if 32-bit component is installed
            return true;
        }

        public bool IsXyVsSubFilterInstalled()
        {
            // Returns true if 32-bit component is installed
            return true;
        }

        public Task InstallXyVsFilter(IProgress<double> progress, CancellationToken cancellationToken)
        {
            // Guess we'll have to hard-code the latest version?
            // http://xy-vsfilter.googlecode.com/files/xy-VSFilter_3.0.0.211_Installer.exe

            return Task.FromResult(true);
        }

        public Task InstallXySubFilter(IProgress<double> progress, CancellationToken cancellationToken)
        {
            // Guess we'll have to hard-code the latest version?
            // http://code.google.com/p/xy-vsfilter/downloads/detail?name=XySubFilter_3.1.0.546_BETA_Installer_v2.exe

            return Task.FromResult(true);
        }

        public bool IsLavFiltersInstalled()
        {
            // Returns true if 32-bit splitter + audio + video are installed

            return true;
        }

        public Task InstallLavFilters(IProgress<double> progress, CancellationToken cancellationToken)
        {
            // Guess we'll have to hard-code the latest version?
            // https://code.google.com/p/lavfilters/downloads/list
            // https://lavfilters.googlecode.com/files/LAVFilters-0.58.2.exe
            
            return Task.FromResult(true);
        }
    }
}
