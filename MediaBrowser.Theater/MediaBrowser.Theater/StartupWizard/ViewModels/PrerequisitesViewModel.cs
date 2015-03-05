using System.Collections.Generic;
using System.Linq;
using MediaBrowser.Common.Net;
using MediaBrowser.Theater.Api.System;
using MediaBrowser.Theater.StartupWizard.Prerequisites;

namespace MediaBrowser.Theater.StartupWizard.ViewModels
{
    public class PrerequisitesViewModel
        : BaseWizardPage
    {
        public List<PrerequisiteViewModel> Prerequisites { get; private set; }

        public PrerequisitesViewModel(IMediaFilters mediaFilters, IHttpClient httpClient)
        {
            var prerequisites = new List<Prerequisite> {
                new LavFiltersPrerequisite(httpClient),
                new XySubFilterPrerequisite(mediaFilters),
                new ReClockPrerequisite(mediaFilters)
            };

            foreach (var item in prerequisites) {
                item.UpdateInstallStatus();
            }

            Prerequisites = prerequisites.Select(p => new PrerequisiteViewModel(p)).ToList();

            foreach (var prerequisite in Prerequisites) {
                prerequisite.PropertyChanged += (sender, args) => {
                    if (args.PropertyName == "WillBeInstalled") {
                        OnPropertyChanged("HasCustomNextPage");
                    }
                };
            }
        }

        public override bool HasCustomNextPage
        {
            get { return Prerequisites.Any(p => p.WillBeInstalled); }
        }

        public override IWizardPage Next()
        {
            return new PrerequisitesInstallationViewModel(Prerequisites.Where(p => p.WillBeInstalled).Select(p => p.Prerequisite));
        }
    }
}