using MediaBrowser.Common.Configuration;
using System;
using System.Threading.Tasks;

namespace MediaBrowser.Theater.Interfaces.Configuration
{
    public interface ITheaterConfigurationManager : IConfigurationManager
    {
        event EventHandler<UserConfigurationUpdatedEventArgs> UserConfigurationUpdated;

        ApplicationConfiguration Configuration { get; }

        UserTheaterConfiguration GetUserTheaterConfiguration(string userId);

        Task UpdateUserTheaterConfiguration(string userId, UserTheaterConfiguration configuration);
    }
}
