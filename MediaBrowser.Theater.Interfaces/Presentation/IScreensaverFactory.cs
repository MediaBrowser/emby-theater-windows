using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaBrowser.Theater.Interfaces.Presentation
{
    public interface IScreensaverFactory
    {
        IScreensaver GetScreensaver();
    }
}