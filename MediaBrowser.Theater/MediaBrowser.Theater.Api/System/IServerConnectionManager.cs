using System.Threading.Tasks;

namespace MediaBrowser.Theater.Api.System
{
    public interface IServerConnectionManager
    {
        Task<bool> AttemptServerConnection();
        Task SendWakeOnLanCommand();
    }
}