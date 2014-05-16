using System.Threading.Tasks;

namespace MediaBrowser.Theater.Api.UserInterface
{
    public interface IRequiresInitialization
    {
        bool IsInitialized { get; }
        Task Initialize();
    }
}