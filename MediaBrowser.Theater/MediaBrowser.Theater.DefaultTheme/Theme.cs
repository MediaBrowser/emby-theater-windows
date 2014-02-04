using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediaBrowser.Theater.Api.Theming;

namespace MediaBrowser.Theater.DefaultTheme
{
    public class Theme
        : ITheme
    {
        public string Name { get; private set; }

        public void Start()
        {
            throw new NotImplementedException();
        }
    }
}
