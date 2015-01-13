using System;
using System.Threading.Tasks;
using System.Windows;

namespace MediaBrowser.Theater.Api.UserInterface
{
    public interface IPresenter
    {
        Window MainApplicationWindow { get; }
        IntPtr MainApplicationWindowHandle { get; }
        Window ActiveWindow { get; }
        event Action<Window> MainWindowLoaded;
        void EnsureApplicationWindowHasFocus();
        FrameworkElement GetFocusedElement();

        Task ShowPage(IViewModel contents);
        Task ShowPopup(IViewModel contents, bool unfocusMainWindow = true, bool navigateBackOnClose = true);
        Task ShowNotification(IViewModel contents);
        MessageBoxResult ShowMessage(MessageBoxInfo messageBoxInfo);
        IViewModel CurrentPage { get; }
    }

    public class PageLoadedEvent
    {
        public IViewModel ViewModel { get; set; }
    }
}