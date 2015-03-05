using System;
using System.Threading;
using System.Threading.Tasks;
using MediaBrowser.Common.Net;
using MediaBrowser.Theater.Mpdn;

namespace MediaBrowser.Theater.StartupWizard.Prerequisites
{
    public class LavFiltersPrerequisite
        : Prerequisite
    {
        private readonly IHttpClient _httpClient;
        private readonly LavFiltersInstaller _installer;

        public LavFiltersPrerequisite(IHttpClient httpClient)
        {
            _httpClient = httpClient;
            _installer = new LavFiltersInstaller();

            Name = "LAV Filters";
            IsOptional = false;
            DownloadUrl = "https://github.com/Nevcairiel/LAVFilters/releases";
            RequiresManualInstallation = !_installer.CanInstall;
        }

        protected override bool CheckInstallStatus()
        {
            return _installer.IsInstalled;
        }

        public override async Task Install(IProgress<double> progress, CancellationToken cancellationToken)
        {
            var update = await _installer.FindUpdate().ConfigureAwait(false);
            if (update.Type != UpdateType.Unavailable) {
                await update.Install(progress, _httpClient).ConfigureAwait(false);
            }

            await base.Install(progress, cancellationToken).ConfigureAwait(false);
        }
    }
}