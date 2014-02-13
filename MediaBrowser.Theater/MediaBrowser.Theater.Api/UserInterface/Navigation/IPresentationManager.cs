using System.Threading.Tasks;
using MediaBrowser.Theater.Api.UserInterface.ViewModels;

namespace MediaBrowser.Theater.Api.UserInterface.Navigation
{
    public interface IPresentationManager
    {
        Task ShowPage(IViewModel contents);
        Task ShowPopup(IViewModel contents);
        Task ShowNotification(IViewModel contents);
    }
}