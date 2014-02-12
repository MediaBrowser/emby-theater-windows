using MediaBrowser.Theater.Api.UserInterface.ViewModels;

namespace MediaBrowser.Theater.Api.UserInterface.Navigation
{
    public interface IPresentationManager
    {
        void ShowPage(IViewModel contents);
        void ShowPopup(IViewModel contents);
        void ShowNotification(IViewModel contents);
    }
}