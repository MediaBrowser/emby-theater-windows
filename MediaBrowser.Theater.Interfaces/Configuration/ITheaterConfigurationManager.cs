using System.Threading.Tasks;
using MediaBrowser.Common.Configuration;

namespace MediaBrowser.Theater.Interfaces.Configuration
{
    public interface ITheaterConfigurationManager : IConfigurationManager
    {
        ApplicationConfiguration Configuration { get; }

        Task<UserTheaterConfiguration> GetUserTheaterConfiguration(string userId);

        Task UpdateUserTheaterConfiguration(string userId, UserTheaterConfiguration configuration);
    }
}
