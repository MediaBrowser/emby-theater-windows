using System.Threading.Tasks;

namespace MediaBrowser.Theater.Api.UserInterface.ViewModels
{
    public interface IRequiresInitialization
    {
        bool IsInitialized { get; }
        Task Initialize();
    }
}