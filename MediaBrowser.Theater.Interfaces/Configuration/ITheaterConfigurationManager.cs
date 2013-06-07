using MediaBrowser.Common.Configuration;

namespace MediaBrowser.Theater.Interfaces.Configuration
{
    public interface ITheaterConfigurationManager : IConfigurationManager
    {
        ApplicationConfiguration Configuration { get; }
    }
}
