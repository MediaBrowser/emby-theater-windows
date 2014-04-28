using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace MediaBrowser.UI.EntryPoints.CommandActions
{
    class MBTScreenDump
    {
        [StructLayout(LayoutKind.Sequential)]
        private struct Rect
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }

        [DllImport("user32.dll")]
        private static extern IntPtr GetWindowRect(IntPtr hWnd, ref Rect rect);

        public static Bitmap GetWindowImage(IntPtr windowHandle)
        {
            var boundsRect = new Rect();

            GetWindowRect(windowHandle, ref boundsRect);
            var bounds = new Rectangle(boundsRect.Left, boundsRect.Top, boundsRect.Right - boundsRect.Left, boundsRect.Bottom - boundsRect.Top);

            var bitmap = new Bitmap(bounds.Width, bounds.Height);

            using (var g = Graphics.FromImage(bitmap))
            {
                g.CopyFromScreen(new System.Drawing.Point(bounds.Left, bounds.Top), System.Drawing.Point.Empty, bounds.Size);
            }

            return bitmap;
        }

        private static string GetUniquePathAndFileName()
        {
            //ar x = System.Configuration.ConfigurationSettings.
            // string value = System.Configuration.ConfigurationManager.AppSettings["SreenDumpPath"];
            return @"C://test.jpg";
        }

        public static void GetAndSaveWindowsImage(IntPtr windowHandle)
        {
            // ToDo - Allow arguments - Filename of Dump, Windown or WholeScreen
            using (var bitmap = GetWindowImage(windowHandle))
            {
                var path = GetUniquePathAndFileName();
                bitmap.Save(path, ImageFormat.Jpeg);
            }
        }

    }
}