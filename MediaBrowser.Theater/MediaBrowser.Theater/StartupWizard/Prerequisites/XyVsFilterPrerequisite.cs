using System;
using System.Threading;
using System.Threading.Tasks;
using MediaBrowser.Theater.Api.System;

namespace MediaBrowser.Theater.StartupWizard.Prerequisites
{
    public class XyVsFilterPrerequisite
        : Prerequisite
    {
        private readonly IMediaFilters _mediaFilters;

        public XyVsFilterPrerequisite(IMediaFilters mediaFilters)
        {
            _mediaFilters = mediaFilters;
            Name = "xy-VSFilter";
            IsOptional = false;
            DownloadUrl = "https://code.google.com/p/xy-vsfilter/wiki/Downloads";
        }

        protected override bool CheckInstallStatus()
        {
            return _mediaFilters.IsXyVsFilterInstalled();
        }

        public override async Task Install(IProgress<double> progress, CancellationToken cancellationToken)
        {
            await _mediaFilters.InstallXyVsFilter(progress, cancellationToken).ConfigureAwait(false);
            await base.Install(progress, cancellationToken);
        }
    }
}