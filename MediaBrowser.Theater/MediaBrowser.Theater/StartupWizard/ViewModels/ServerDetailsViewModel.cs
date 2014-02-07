using System;
using MediaBrowser.Theater.Api.Theming.ViewModels;

namespace MediaBrowser.Theater.StartupWizard.ViewModels
{
    public class ServerDetailsViewModel
        : BaseViewModel, IWizardPage
    {
        public bool CanMoveNext
        {
            get { return true; }
        }

        public bool HasCustomNextPage
        {
            get { return false; }
        }

        public IWizardPage Next()
        {
            throw new NotImplementedException();
        }
    }
}