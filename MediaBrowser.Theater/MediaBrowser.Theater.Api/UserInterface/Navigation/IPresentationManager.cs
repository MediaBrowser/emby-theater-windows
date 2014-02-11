using MediaBrowser.Theater.Api.Theming.ViewModels;

namespace MediaBrowser.Theater.Api.Theming.Navigation
{
    public interface IPresentationManager
    {
        void ShowPage(BaseViewModel contents);
        void ShowPopup(BaseViewModel contents);
        void ShowNotification(BaseViewModel contents);
    }
}