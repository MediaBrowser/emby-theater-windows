using System.Threading.Tasks;
using MediaBrowser.Model.System;

namespace MediaBrowser.Theater.Api.System
{
    public interface IServerConnectionManager
    {
        Task<PublicSystemInfo> AttemptServerConnection();
        Task<bool> AttemptAutoLogin(PublicSystemInfo systemInfo);
        Task SendWakeOnLanCommand();
    }
}