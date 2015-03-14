using System.Threading.Tasks;
using MediaBrowser.Theater.Mpdn;

namespace MediaBrowser.Theater.StartupWizard.Prerequisites
{
    public class LavFiltersPrerequisite
        : Prerequisite
    {
        private readonly LavFiltersInstaller _installer;
        
        public LavFiltersPrerequisite()
        {
            _installer = new LavFiltersInstaller();

            Name = "LAV Filters";
            IsOptional = false;
            DownloadUrl = "https://github.com/Nevcairiel/LAVFilters/releases";
            RequiresManualInstallation = !_installer.CanInstall;
        }

        public override Task<IUpdate> GetInstaller()
        {
            return _installer.FindUpdate();
        }

        public override bool CheckInstallStatus()
        {
            return _installer.IsInstalled;
        }
    }
}