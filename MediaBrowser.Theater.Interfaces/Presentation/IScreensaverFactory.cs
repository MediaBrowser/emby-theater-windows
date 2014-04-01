using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaBrowser.Theater.Interfaces.Presentation
{
    public interface IScreensaverFactory
    {
        /// <summary>
        /// Gets the name.
        /// </summary>
        /// <value>The name.</value>
        string Name { get; }

        IScreensaver GetScreensaver();
    }
}