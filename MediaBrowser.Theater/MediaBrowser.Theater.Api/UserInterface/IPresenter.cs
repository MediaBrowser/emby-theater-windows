using System.Threading.Tasks;

namespace MediaBrowser.Theater.Api.UserInterface
{
    public interface IPresenter
    {
        Task ShowPage(IViewModel contents);
        Task ShowPopup(IViewModel contents);
        Task ShowNotification(IViewModel contents);
    }
}