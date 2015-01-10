using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediaBrowser.Model.IO;
using MediaBrowser.Theater.Api.Configuration;
using MediaBrowser.Theater.DirectShow;

namespace MediaBrowser.Theater.StartupWizard.ViewModels
{
    public class CompleteInstallationViewModel
        : BaseWizardPage
    {
        private readonly ITheaterConfigurationManager _configurationManager;
        private readonly IZipClient _zipClient;

        public CompleteInstallationViewModel(ITheaterConfigurationManager configurationManager, IZipClient zipClient)
        {
            _configurationManager = configurationManager;
            _zipClient = zipClient;
        }

        public override async Task Initialize()
        {
            await base.Initialize();

            Task.Run(() => {
                URCOMLoader.EnsureObjects(_configurationManager, _zipClient, true);
                Wizard.MoveNext();
            });
        }
    }
}
