using System.Runtime.InteropServices;

namespace MediaBrowser.Theater.Presentation.Interop
{
    public static class WebBrowserHelper
    {
        private const int Feature = 21; //FEATURE_DISABLE_NAVIGATION_SOUNDS
        private const int SetFeatureOnProcess = 0x00000002;

        [DllImport("urlmon.dll")]
        [PreserveSig]
        [return: MarshalAs(UnmanagedType.Error)]
        static extern int CoInternetSetFeatureEnabled(int featureEntry,
        [MarshalAs(UnmanagedType.U4)] int dwFlags,
        bool fEnable);

        public static void DisableFrameNavigationSound()
        {
            CoInternetSetFeatureEnabled(Feature, SetFeatureOnProcess, true);
        }
    }
}
