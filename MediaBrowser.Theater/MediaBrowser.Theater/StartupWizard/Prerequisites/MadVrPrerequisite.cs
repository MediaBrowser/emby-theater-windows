using System;
using System.Threading;
using System.Threading.Tasks;
using MediaBrowser.Theater.Api.System;

namespace MediaBrowser.Theater.StartupWizard.Prerequisites
{
    public class MadVrPrerequisite
        : Prerequisite
    {
        private readonly IMediaFilters _mediaFilters;

        public MadVrPrerequisite(IMediaFilters mediaFilters)
        {
            _mediaFilters = mediaFilters;
            Name = "MadVR";
            IsOptional = true;
            RequiresManualInstallation = true;
            DownloadUrl = "http://forum.doom9.org/showthread.php?t=146228";
        }

        protected override bool CheckInstallStatus()
        {
            return _mediaFilters.IsMadVrInstalled();
        }

        public override async Task Install(IProgress<double> progress, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}