using System.Collections.Generic;

namespace MediaBrowser.Theater.Interfaces.Presentation
{
    public interface IAppFactory
    {
        IEnumerable<IApp> GetApps();
    }
}
