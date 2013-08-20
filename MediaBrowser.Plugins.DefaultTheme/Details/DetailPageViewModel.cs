using MediaBrowser.Plugins.DefaultTheme.Home;
using MediaBrowser.Theater.Interfaces.ViewModels;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MediaBrowser.Plugins.DefaultTheme.Details
{
    public class DetailPageViewModel : TabbedViewModel
    {
        protected override Task<IEnumerable<string>> GetSectionNames()
        {
            throw new NotImplementedException();
        }

        protected override BaseViewModel GetContentViewModel(string section)
        {
            throw new NotImplementedException();
        }
    }
}
