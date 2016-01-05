using MediaBrowser.Common.Configuration;

namespace Emby.Theater.Configuration
{
    public interface ITheaterConfigurationManager : IConfigurationManager
    {
        ApplicationConfiguration Configuration { get; }
    }
}
