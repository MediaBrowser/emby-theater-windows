using MediaBrowser.Common.Configuration;
using MediaBrowser.Model.Configuration;

namespace Emby.Theater.Configuration
{
    public interface ITheaterConfigurationManager : IConfigurationManager
    {
        BaseApplicationConfiguration Configuration { get; }
    }
}
