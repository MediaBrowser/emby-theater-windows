using MediaBrowser.Common;
using MediaBrowser.Theater.Api.Configuration;

namespace MediaBrowser.Theater.Api
{
    public interface ITheaterApplicationHost
        : IApplicationHost
    {
        ITheaterConfigurationManager TheaterConfigurationManager { get; }
    }
}