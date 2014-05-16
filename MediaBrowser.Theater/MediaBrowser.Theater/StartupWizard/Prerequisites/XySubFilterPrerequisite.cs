using System;
using System.Threading;
using System.Threading.Tasks;
using MediaBrowser.Theater.Api.System;

namespace MediaBrowser.Theater.StartupWizard.Prerequisites
{
    public class XySubFilterPrerequisite
        : Prerequisite
    {
        private readonly IMediaFilters _mediaFilters;

        public XySubFilterPrerequisite(IMediaFilters mediaFilters)
        {
            _mediaFilters = mediaFilters;
            Name = "XySubFilter";
            IsOptional = true;
            DownloadUrl = "https://code.google.com/p/xy-vsfilter/wiki/Downloads";
        }

        protected override bool CheckInstallStatus()
        {
            return _mediaFilters.IsXySubFilterInstalled();
        }

        public override async Task Install(IProgress<double> progress, CancellationToken cancellationToken)
        {
            await _mediaFilters.InstallXySubFilter(progress, cancellationToken).ConfigureAwait(false);
            await base.Install(progress, cancellationToken);
        }
    }
}