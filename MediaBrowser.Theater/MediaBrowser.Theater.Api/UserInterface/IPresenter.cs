using System;
using System.Threading.Tasks;
using System.Windows;

namespace MediaBrowser.Theater.Api.UserInterface
{
    public interface IPresenter
    {
        Window MainApplicationWindow { get; }
        IntPtr MainApplicationWindowHandle { get; }

        Task ShowPage(IViewModel contents);
        Task ShowPopup(IViewModel contents);
        Task ShowNotification(IViewModel contents);
    }
}