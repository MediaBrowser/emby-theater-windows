using MediaBrowser.Common.Configuration;

namespace MediaBrowser.Theater.Api.Configuration
{
    public interface ITheaterConfigurationManager : IConfigurationManager
    {
        ApplicationConfiguration Configuration { get; }
    }
}