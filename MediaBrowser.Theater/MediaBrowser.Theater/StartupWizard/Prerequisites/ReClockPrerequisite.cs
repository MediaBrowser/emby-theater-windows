using System;
using System.Threading;
using System.Threading.Tasks;
using MediaBrowser.Theater.Api.System;
using MediaBrowser.Theater.Mpdn;

namespace MediaBrowser.Theater.StartupWizard.Prerequisites
{
    public class ReClockPrerequisite
        : Prerequisite
    {
        private readonly IMediaFilters _mediaFilters;

        public ReClockPrerequisite(IMediaFilters mediaFilters)
        {
            _mediaFilters = mediaFilters;
            Name = "ReClock";
            IsOptional = true;
            DownloadUrl = "http://www.videohelp.com/tools/ReClock-Directshow-Filter";
            RequiresManualInstallation = true;
        }

        public override Task<Mpdn.IUpdate> GetInstaller()
        {
            return Task.FromResult(_mediaFilters.IsReClockInstalled() ? Update.UpToDate : Update.Unavailable);
        }

        public override bool CheckInstallStatus()
        {
            return _mediaFilters.IsReClockInstalled();
        }

//        protected override bool CheckInstallStatus() {
//            return _mediaFilters.IsReClockInstalled();
//        }
//
//        public override async Task Install(IProgress<double> progress, CancellationToken cancellationToken)
//        {
//            await _mediaFilters.InstallReClock(progress, cancellationToken).ConfigureAwait(false);
//            await base.Install(progress, cancellationToken).ConfigureAwait(false);
//        }
    }
}