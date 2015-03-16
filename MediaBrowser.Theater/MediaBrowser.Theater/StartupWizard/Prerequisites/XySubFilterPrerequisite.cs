using System.Threading.Tasks;
using MediaBrowser.Theater.Mpdn;

namespace MediaBrowser.Theater.StartupWizard.Prerequisites
{
    public class XySubFilterPrerequisite
        : Prerequisite
    {
        private readonly XySubFilterInstaller _installer;

        public XySubFilterPrerequisite()
        {
            _installer = new XySubFilterInstaller();
            Name = "XySubFilter";
            IsOptional = false;
            DownloadUrl = "https://code.google.com/p/xy-vsfilter/wiki/Downloads";
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