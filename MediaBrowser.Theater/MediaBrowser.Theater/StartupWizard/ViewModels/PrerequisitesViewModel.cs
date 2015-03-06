using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MediaBrowser.Common.Net;
using MediaBrowser.Theater.Api.System;
using MediaBrowser.Theater.StartupWizard.Prerequisites;

namespace MediaBrowser.Theater.StartupWizard.ViewModels
{
    public class PrerequisitesViewModel
        : BaseWizardPage
    {
        private readonly IHttpClient _httpClient;
        public List<PrerequisiteViewModel> Prerequisites { get; private set; }

        private bool _isReady;

        public PrerequisitesViewModel(IMediaFilters mediaFilters, IHttpClient httpClient)
        {
            _httpClient = httpClient;
            var prerequisites = new List<Prerequisite> {
                new LavFiltersPrerequisite(),
                new XySubFilterPrerequisite(mediaFilters),
                new ReClockPrerequisite(mediaFilters)
            };
            
            Prerequisites = prerequisites.Select(p => new PrerequisiteViewModel(p)).ToList();

            foreach (var prerequisite in Prerequisites) {
                prerequisite.PropertyChanged += (sender, args) => {
                    if (args.PropertyName == "WillBeInstalled") {
                        OnPropertyChanged("HasCustomNextPage");
                    }
                };
            }
        }

        public override async void Initialize(WizardViewModel wizard)
        {
            base.Initialize(wizard);

            var searches = Prerequisites.Select(p => p.FindUpdate());
            await Task.WhenAll(searches).ConfigureAwait(false);
            _isReady = true;
            OnPropertyChanged("HasErrors", false);
        }

        public override bool HasCustomNextPage
        {
            get { return Prerequisites.Any(p => p.WillBeInstalled); }
        }

        public override async Task<bool> Validate()
        {
            if (!await base.Validate().ConfigureAwait(false)) {
                return false;
            }

            OnPropertyChanged("HasErrors", false);
            return _isReady;
        }

        public override bool HasErrors
        {
            get { return !_isReady || base.HasErrors; }
        }

        public override IWizardPage Next()
        {
            return new PrerequisitesInstallationViewModel(Prerequisites.Where(p => p.WillBeInstalled), _httpClient);
        }
    }
}