using System.Runtime.InteropServices;

namespace Emby.Theater
{
    public static class NativeWindowMethods
    {
        [DllImport("dwmapi.dll", PreserveSig = false)]
        public static extern bool DwmIsCompositionEnabled();
    }
}
