using System;

namespace MediaBrowser.Theater.Api.UserInterface
{
    public struct MainWindowState
    {
        public double Top { get; set; }
        public double Left { get; set; }
        public double Width { get; set; }
        public double Height { get; set; }
        public WindowState State { get; set; }
        public double DpiScale { get; set; }
    }

    public enum WindowState
    {
        Windowed,
        Maximized,
        Minimized
    }

    public interface IWindowManager
    {
        MainWindowState MainWindowState { get; }
        IDisposable UseBackgroundWindow(IntPtr hwnd);
        void FocusMainWindow();
    }
}