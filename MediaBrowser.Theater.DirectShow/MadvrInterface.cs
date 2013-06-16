using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Globalization;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Microsoft.DirectX.PrivateImplementationDetails;
using System.Threading;

namespace MediaBrowser.UI.Playback.DirectShow
{
    // when using the (1) bitmaps method, you can register a mouse callback
    // this callback will be called whenever a mouse event occurs
    // mouse pos (0, 0) is the left top corner of the OSD bitmap element
    // return "true" if your callback has handled the mouse message and
    // if you want the mouse message to be "eaten" (instead of passed on)
    public delegate void OSDMOUSECALLBACK(string name, System.IntPtr context, uint message, System.IntPtr wParam, int posX, int posY);

    // when using the (2) render callbacks method, you need to provide
    // madVR with an instance of the IOsdRenderCallback interface
    // it contains three callbacks you have to provide
    [ComImport, System.Security.SuppressUnmanagedCodeSecurity,
    Guid("57FBF6DC-3E5F-4641-935A-CB62F00C9958"),
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IOsdRenderCallback
    {
        [PreserveSig]
        int SetDevice([Out] out IDirect3DDevice9 dev);

        [PreserveSig]
        int ClearBackground(
            [In, MarshalAs(UnmanagedType.LPWStr)] string name,
            [Out] out IntPtr frameStart,
            [Out] Rectangle fullOutputRect,
            [Out] Rectangle activeVideoRect
            );

        [PreserveSig]
        int RenderOsd(
            [In, MarshalAs(UnmanagedType.LPWStr)] string name,
            [Out] out IntPtr frameStart,
            [Out] Rectangle fullOutputRect,
            [Out] Rectangle activeVideoRect
            );
    }

    // this is the main interface which madVR provides to you
    [ComImport, System.Security.SuppressUnmanagedCodeSecurity,
    Guid("3AE03A88-F613-4BBA-AD3E-EE236976BF9A"),
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IMadVROsdServices
    {
        [PreserveSig]                                                               // this API provides the (1) bitmap based method
        int OsdSetBitmap(
            [In, MarshalAs(UnmanagedType.LPWStr)] string name,                      // name of the OSD element, e.g. "YourMediaPlayer.SeekBar"
            [In] IntPtr leftEye,                                                    // OSD bitmap, should be 24bit or 32bit, NULL deletes the OSD element
            [In] IntPtr rightEye,                                                   // specify when your OSD is 3D, otherwise set to NULL
            [In, MarshalAs(UnmanagedType.U4)] uint colorKey,                        // transparency color key, set to 0 if your bitmap has an 8bit alpha channel
            [In, MarshalAs(UnmanagedType.U4)] int posX,                             // where to draw the OSD element?
            [In, MarshalAs(UnmanagedType.U4)] int posY,                             //
            [In, MarshalAs(UnmanagedType.Bool)] bool posRelativeToVideoRect,        // draw relative to TRUE: the active video rect; FALSE: the full output rect
            [In, MarshalAs(UnmanagedType.U4)] int zOrder,                           // high zOrder OSD elements are drawn on top of those with smaller zOrder values
            [In, MarshalAs(UnmanagedType.U4)] uint duration,                        // how many milliseconds shall the OSD element be shown (0 = infinite)?
            [In, MarshalAs(UnmanagedType.U4)] uint flags,                           // undefined - set to 0
            [Out] OSDMOUSECALLBACK callback,                                        // optional callback for mouse events
            [Out] IntPtr callbackContext,                                           // this context is passed to the callback
            IntPtr reserved                                                         // undefined - set to NULL
       );

        [PreserveSig]                                                               // this API allows you to ask the current video rectangles
        int OsdGetVideoRects(
          [Out] out Rectangle fullOutputRect,                                                 // (0, 0, outputSurfaceWidth, outputSurfaceHeight)
          [Out] out Rectangle activeVideoRect                                                 // active video rendering rect inside of fullOutputRect
        );

        [PreserveSig]                                                               // this API provides the (2) render callback based method
        int OsdSetRenderCallback(
          [In, MarshalAs(UnmanagedType.LPWStr)] string name,                        // name of the OSD callback, e.g. "YourMediaPlayer.OsdCallbacks"
          [Out] IOsdRenderCallback callback,                                        // OSD callback interface, set to NULL to unregister the callback
          IntPtr reserved                                                           // undefined - set to NULL
        );
        // this API allows you to force madVR to redraw the current video frame
        [PreserveSig]                                                               // useful when using the (2) render callback method, when the graph is paused
        int OsdRedrawFrame();
    }

    // IMadVRTextOsd
    // ---------------------------------------------------------------------------
    // This interface allows you to draw simple text messages.
    // madVR uses it internally, too, for showing various messages to the user.
    // The messages are shown in the top left corner of the video rendering window.
    // The messages work in both windowed and fullscreen exclusive mode.
    // There can always be only one message active at the same time, so basically
    // the messages are overwriting each other.
    [ComImport, System.Security.SuppressUnmanagedCodeSecurity,
    Guid("ABA34FDA-DD22-4E00-9AB4-4ABF927D0B0C"),
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IMadVRTextOsd
    {
        [PreserveSig]
        int OsdDisplayMessage([In, MarshalAs(UnmanagedType.LPWStr)] string text, [In, MarshalAs(UnmanagedType.U4)] uint milliseconds);

        [PreserveSig]
        int OsdClearMessage();
    };

    // IMadVRSeekbarControl
    // ---------------------------------------------------------------------------
    // if you draw your own seekbar and absolutely insist on disliking madVR's
    // own seekbar, you can forcefully hide it by using this interface
    // using this interface only affects the current madVR instance
    [ComImport, System.Security.SuppressUnmanagedCodeSecurity,
    Guid("D2D3A520-7CFA-46EB-BA3B-6194A028781C"),
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IMadVRSeekbarControl
    {
        [PreserveSig]
        int DisableSeekbar(IntPtr disable);
    };

    // IMadVRExclusiveModeControl
    // ---------------------------------------------------------------------------
    // you can use this interface to turn madVR's automatic exclusive mode on/off
    // using this interface only affects the current madVR instance
    [ComImport, System.Security.SuppressUnmanagedCodeSecurity,
    Guid("88A69329-3CD3-47D6-ADEF-89FA23AFC7F3"),
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IMadVRExclusiveModeControl
    {
        [PreserveSig]
        int DisableExclusiveMode(IntPtr disable);
    };

    // IMadVRExclusiveModeInfo
    // ---------------------------------------------------------------------------
    // this interface allows you to ask...
    // ... whether madVR is currently in exclusive mode
    // ... whether the madVR exclusive mode seek bar is currently enabled

    // If madVR is in fullscreen exclusive mode, you should be careful with
    // which GUI you show, because showing any window based GUI will make madVR
    // automatically switch back to windowed mode. That's ok if that's what you
    // really want, just be aware of that. A good alternative is to use the
    // graphical or text base OSD interfaces (see above). Using them instead of
    // a window based GUI means that madVR can stay in exclusive mode all the
    // time.

    // Since madVR has its own seek bar (which is only shown in fullscreen
    // exclusive mode, though), before showing your own seek bar you should
    // check whether madVR is in fullscreen exclusive mode and whether the
    // user has enabled madVR's own seek bar. If so, you should probably not
    // show your own seek bar. If the user, however, has the madVR seek bar
    // disabled, you should still show your own seek bar, because otherwise
    // the user will have no way to seek at all.
    [ComImport, System.Security.SuppressUnmanagedCodeSecurity,
    Guid("D6EE8031-214E-4E9E-A3A7-458925F933AB"),
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IMadVRExclusiveModeInfo
    {
        [PreserveSig]
        void IsExclusiveModeActive([Out, MarshalAs(UnmanagedType.I1)] out bool Status);

        [PreserveSig]
        void IsMadVRSeekbarEnabled([Out, MarshalAs(UnmanagedType.I1)] out bool Status);
    };



    public class MadvrInterface
    {
        public static OSDMOUSECALLBACK mouseCallback;
        public static IntPtr callbackContext;
        public static IntPtr reserved;
        public static IOsdRenderCallback osdRenderCallback;

        public static void ShowMadVrMessage(string Message, uint DisplayTime, MadVR madvr)
        {
            if (madvr == null)
            {
                return;
            }
            else
            {
                IMadVRTextOsd osd = (IMadVRTextOsd)madvr;
                osd.OsdDisplayMessage(Message, DisplayTime);
            }
        }

        public static void ShowMadVrBitmap(string name, IntPtr leftEye, int Posx, int Posy, uint Duration, int ImagePriority, MadVR madvr)
        {
            if (madvr == null)
            {
                return;
            }

            IntPtr rightEye = IntPtr.Zero;
            uint colorkey = 0;
            reserved = IntPtr.Zero;

            IMadVROsdServices osdServices = (IMadVROsdServices)madvr;
            osdServices.OsdSetBitmap(name, leftEye, rightEye, colorkey, Posx, Posy, false, ImagePriority, Duration, 0, mouseCallback, callbackContext, reserved);
            if (name.ToLower().Contains("pause") || name.ToLower().StartsWith("pause"))
            {
                osdServices.OsdRedrawFrame();
            }
            //else if (Madvr_VideoPlayer.Plugin.Madvr_VideoPlayer.VideoPlayer.m_state == PlaybackState.Paused)
            //{
            //    osdServices.OsdRedrawFrame();
            //}
        }

        public static void ClearMadVrBitmap(string name, MadVR madvr)
        {
            if (madvr == null)
            {
                return;
            }

            IntPtr leftEye = IntPtr.Zero;
            IntPtr rightEye = IntPtr.Zero;
            uint colorkey = 0;
            int Posx = 0;
            int Posy = 0;
            uint Duration = 1;
            int ImagePriority = 100;
            reserved = IntPtr.Zero;

            IMadVROsdServices osdServices = (IMadVROsdServices)madvr;
            osdServices.OsdSetBitmap(name, leftEye, rightEye, colorkey, Posx, Posy, false, ImagePriority, Duration, 0, mouseCallback, callbackContext, reserved);
        }

        public static void EnableExclusiveMode(bool enable, MadVR madvr)
        {
            if (madvr == null)
            {
                return;
            }

            Int32 iEnable;
            if (enable == true)
            {
                iEnable = 1;
            }
            else
            {
                iEnable = 0;
            }
            int size = Marshal.SizeOf(iEnable);
            IntPtr pBool = Marshal.AllocHGlobal(size);
            Marshal.WriteInt32(pBool, 0, 1);  // last parameter 0 (FALSE), 1 (TRUE)
            IMadVRExclusiveModeControl eModeControl = (IMadVRExclusiveModeControl)madvr;
            eModeControl.DisableExclusiveMode(pBool);
            Marshal.FreeHGlobal(pBool);
        }

        public static bool InExclusiveMode(MadVR madvr)
        {
            try
            {
                IMadVRExclusiveModeInfo exclusiveInfo = (IMadVRExclusiveModeInfo)madvr;
                bool status = false;
                exclusiveInfo.IsExclusiveModeActive(out status);
                return status;
            }
            catch (Exception)
            {
                return false;
            }            
        }

        public static bool MadvrSeekBarEnabled(MadVR madvr)
        {
            try
            {
                IMadVRExclusiveModeInfo exclusiveInfo = (IMadVRExclusiveModeInfo)madvr;
                bool status = false;
                exclusiveInfo.IsMadVRSeekbarEnabled(out status);
                return status;
            }
            catch (Exception)
            {
                return false;
            }            
        }

        public static void EnableMadvrSeekBar(bool enable, MadVR madvr)
        {
            if (madvr == null)
            {
                return;
            }

            Int32 iEnable;
            if (enable == true)
            {
                iEnable = 1;
            }
            else
            {
                iEnable = 0;
            }
            int size = Marshal.SizeOf(iEnable);
            IntPtr pBool = Marshal.AllocHGlobal(size);
            Marshal.WriteInt32(pBool, 0, 1);  // last parameter 0 (FALSE), 1 (TRUE)
            IMadVRSeekbarControl mvrSeekbar = (IMadVRSeekbarControl)madvr;
            mvrSeekbar.DisableSeekbar(pBool);
            Marshal.FreeHGlobal(pBool);
        }

        public static void SetRenderCallback(MadVR madvr)
        {
            IMadVROsdServices osdServices = (IMadVROsdServices)madvr;
            osdServices.OsdSetRenderCallback("MeediosPlayer", osdRenderCallback, reserved);
            IDirect3DDevice9 test;
            osdRenderCallback.SetDevice(out test);
        }

        public static Rectangle GetMediaDimensions(bool GetActiveVideo, MadVR madvr)
        {
            Rectangle FullOutput = new Rectangle();
            Rectangle ActiveVideo = new Rectangle();
            try
            {
                IMadVROsdServices osdServices = (IMadVROsdServices)madvr;
                osdServices.OsdGetVideoRects(out FullOutput, out ActiveVideo);

                if (GetActiveVideo == true)
                {
                    return ActiveVideo;
                }
                else
                {
                    return FullOutput;
                }
            }
            catch (Exception)
            {
                return FullOutput;
            }            
        }
    }
}
