using MediaBrowser.Theater.Api.UserInterface.ViewModels;

namespace MediaBrowser.Theater.Api.UserInterface.Navigation
{
    public interface IPresentationManager
    {
        void ShowPage(BaseViewModel contents);
        void ShowPopup(BaseViewModel contents);
        void ShowNotification(BaseViewModel contents);
    }
}