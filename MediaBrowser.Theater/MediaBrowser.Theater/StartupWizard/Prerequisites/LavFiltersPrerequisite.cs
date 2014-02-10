using System;
using System.Threading;
using System.Threading.Tasks;
using MediaBrowser.Theater.Api.System;

namespace MediaBrowser.Theater.StartupWizard.Prerequisites
{
    public class LavFiltersPrerequisite
        : Prerequisite
    {
        private readonly IMediaFilters _mediaFilters;

        public LavFiltersPrerequisite(IMediaFilters mediaFilters)
        {
            _mediaFilters = mediaFilters;
            Name = "LAV Filters";
            IsOptional = false;
            DownloadUrl = "https://code.google.com/p/lavfilters/";
        }

        protected override bool CheckInstallStatus()
        {
            return _mediaFilters.IsLavSplitterInstalled() && _mediaFilters.IsLavVideoInstalled() && _mediaFilters.IsLavAudioInstalled();
        }

        public override async Task Install(IProgress<double> progress, CancellationToken cancellationToken)
        {
            await _mediaFilters.InstallLavFilters(progress, cancellationToken).ConfigureAwait(false);
            await base.Install(progress, cancellationToken);
        }
    }
}