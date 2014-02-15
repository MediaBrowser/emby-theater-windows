using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediaBrowser.Common;
using MediaBrowser.Theater.Api.UserInterface.Navigation;

namespace MediaBrowser.Theater.DefaultTheme.Navigation
{
    public class RootContext
        : NavigationContext
    {
        public RootContext(IApplicationHost appHost) : base(appHost) { }
        
        public override Task Activate()
        {
            throw new NotImplementedException();
        }
    }
}
