using System;
using System.Threading;
using System.Threading.Tasks;
using MediaBrowser.Theater.Api.System;

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
        }

        protected override bool CheckInstallStatus() {
            return _mediaFilters.IsReClockInstalled();
        }

        public override async Task Install(IProgress<double> progress, CancellationToken cancellationToken)
        {
            await _mediaFilters.InstallReClock(progress, cancellationToken).ConfigureAwait(false);
            await base.Install(progress, cancellationToken);
        }
    }
}