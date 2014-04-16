//using Microsoft.DirectX.PrivateImplementationDetails;
using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Threading;
using System.Text;

namespace MediaBrowser.Theater.DirectShow
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
        int SetDevice([Out] out /*IDirect3DDevice9*/IntPtr dev);

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
        //void IsExclusiveModeActive([Out, MarshalAs(UnmanagedType.I1)] out bool Status);
        int IsExclusiveModeActive();

        [PreserveSig]
        void IsMadVRSeekbarEnabled([Out, MarshalAs(UnmanagedType.I1)] out bool Status);
    };


    // ---------------------------------------------------------------------------
    // IMadVRSettings
    // ---------------------------------------------------------------------------

    // this interface allows you to read and write madVR settings

    // For each folder and value there exists both a short ID and a long
    // description. The short ID will never change. The long description may be
    // modified in a future version. So it's preferable to use the ID, but you can
    // also address settings by using the clear text description.

    // The "path" parameter can simply be set to the ID or to the description of
    // the setting value. Alternatively you can use a partial or full path to the
    // setting value. E.g. the following calls will all return the same value:
    // (1) GetBoolean(L"dontDither", &boolVal);
    // (2) GetBoolean(L"don't use dithering", &boolVal);
    // (3) GetBoolean(L"tradeQuality\dontDither", &boolVal);
    // (4) GetBoolean(L"performanceTweaks\trade quality for performance\dontDither", &boolVal);

    // Using the full path can make sense if you want to access a specific profile.
    // If you don't specify a path, you automatically access the currently active
    // profile.
    [ComImport, System.Security.SuppressUnmanagedCodeSecurity,
    Guid("6F8A566C-4E19-439E-8F07-20E46ED06DEE"),
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IMadVRSettings
    {
        // returns the revision number of the settings record
        // the revision number is increased by 1 every time a setting changes
        [PreserveSig]
        bool SettingsGetRevision(ref long revision);

        // export the whole settings record to a binary data buffer
        // the buffer is allocated by mvrSettings_Export by using LocalAlloc
        // it's the caller's responsibility to free the buffer again by using LocalFree
        [PreserveSig]
        bool SettingsExport([Out] out IntPtr buf, int size);

        // import the settings from a binary data buffer
        [PreserveSig]
        bool SettingsImport(IntPtr buf, int size);

        // modify a specific value
        [PreserveSig]
        bool SettingsSetString(
            [MarshalAs(UnmanagedType.LPWStr), In] 
            string path,
            [MarshalAs(UnmanagedType.LPWStr), In] 
            string value
            );

        [PreserveSig]
        bool SettingsSetInteger(
            [MarshalAs(UnmanagedType.LPWStr), In] 
            string path, 
            int value
            );

        [PreserveSig]
        bool SettingsSetBoolean(
            [MarshalAs(UnmanagedType.LPWStr), In] 
            string path, 
            bool value
            );

        // The buffer for mvrSettings_GetString must be provided by the caller and
        // bufLenInChars set to the buffer's length (please note: 1 char -> 2 bytes).
        // If the buffer is too small, the API fails and GetLastError returns
        // ERROR_MORE_DATA. On return, bufLenInChars is set to the required buffer size.
        // The buffer for mvrSettings_GetBinary is allocated by mvrSettings_GetBinary.
        // The caller is responsible for freeing it by using LocalAlloc().
        [PreserveSig]
        bool SettingsGetString(
            [MarshalAs(UnmanagedType.LPWStr), In]
            string path, 
            [MarshalAs(UnmanagedType.LPWStr), Out] StringBuilder value, 
            ref int bufLenInChars);
        [PreserveSig]
        bool SettingsGetInteger(
            [MarshalAs(UnmanagedType.LPWStr), In] 
            string path, 
            ref int value
            );
        [PreserveSig]
        bool SettingsGetBoolean(
            [MarshalAs(UnmanagedType.LPWStr), In] 
            string path, 
            ref bool value);
        [PreserveSig]
        bool SettingsGetBinary(
            [MarshalAs(UnmanagedType.LPWStr), In]
            string path, 
            object[] value, 
            int bufLenInBytes
            );
    }

    public class MadVRSettings
    {
        IMadVRSettings _madVR = null;

        public bool IsValid
        {
            get
            {
                if (_madVR == null)
                    return false;
                else
                    return true;
            }
        }

        public MadVRSettings(object madVR)
        {
            _madVR = madVR as IMadVRSettings;
        }

        public bool SetString(string setting, string value)
        {
            if (IsValid)
            {
                return _madVR.SettingsSetString(setting, value);
            }

            return false;
        }

        public string GetString(string setting)
        {
            string retVal = string.Empty;

            if (IsValid)
            {
                int sbLen = 100;
                StringBuilder smMode = new StringBuilder(sbLen);

                bool success = _madVR.SettingsGetString(setting, smMode, ref sbLen);
                if (sbLen > smMode.Capacity)
                {
                    smMode = new StringBuilder(sbLen);
                    success = _madVR.SettingsGetString(setting, smMode, ref sbLen);
                }
                if (success)
                    retVal = smMode.ToString();
            }

            return retVal;
        }

        public bool SetBool(string setting, bool value)
        {
            bool retVal = false;

            if (IsValid)
            {
                retVal = _madVR.SettingsSetBoolean(setting, value);
            }

            return retVal;
        }

        public bool GetBool(string setting)
        {
            bool retVal = false;

            if (IsValid)
            {
                bool success = _madVR.SettingsGetBoolean(setting, ref retVal);
            }

            return retVal;
        }

        public bool SetInt(string setting, int value)
        {
            bool retVal = false;

            if (IsValid)
            {
                retVal = _madVR.SettingsSetInteger(setting, value);
            }

            return retVal;
        }

        public int GetInt(string setting)
        {
            int retVal = -1;

            if (IsValid)
            {
                bool success = _madVR.SettingsGetInteger(setting, ref retVal);
            }

            return retVal;
        }
    }

    public class MadvrInterface
    {
        public static OSDMOUSECALLBACK mouseCallback;
        public static IntPtr callbackContext;
        public static IntPtr reserved;
        public static IOsdRenderCallback osdRenderCallback;

        public static void ShowMadVrMessage(string Message, uint DisplayTime, MadVR madvr)
        {
            var osd = (IMadVRTextOsd)madvr;
            osd.OsdDisplayMessage(Message, DisplayTime);
        }

        private static string CurrentName;
        private static IntPtr CurrentLeftEye;
        private static int CurrentPosX;
        private static int CurrentPosY;
        private static uint CurrentDuration;
        private static int CurrentImagePriority;


        public static bool startTransition = false;
        public static void ShowMadVrBitmap(string name, IntPtr leftEye, int Posx, int Posy, uint Duration, int ImagePriority, MadVR madvr)
        {
            IntPtr rightEye = IntPtr.Zero;
            uint colorkey = 0;
            reserved = IntPtr.Zero;
            //mouseCallback = new OSDMOUSECALLBACK(OnMouseEvent);
            IMadVROsdServices osdServices = (IMadVROsdServices)madvr;

            int ScreenBottom = 0;// Madvr_VideoPlayer.Plugin.Madvr_VideoPlayer.VideoPlayer.ScreenHeight;

            if (Posy > (ScreenBottom / 2))
            {
                if (name != "transportTime")
                {
                    CurrentName = name;
                    CurrentLeftEye = leftEye;
                    CurrentPosX = Posx;
                    CurrentPosY = Posy;
                    CurrentDuration = Duration;
                    CurrentImagePriority = ImagePriority;


                    if (startTransition == false)
                    {
                        while (Posy < ScreenBottom)
                        {
                            osdServices.OsdSetBitmap(name, leftEye, rightEye, colorkey, Posx, ScreenBottom, false, ImagePriority, Duration, 0, mouseCallback, callbackContext, reserved);
                            if (Posy < ScreenBottom)
                            {
                                ScreenBottom--;
                            }
                            if (Posy < ScreenBottom)
                            {
                                ScreenBottom--;
                            }
                            if (Posy < ScreenBottom)
                            {
                                ScreenBottom--;
                            }
                            if (Posy < ScreenBottom)
                            {
                                ScreenBottom--;
                            }
                            if (name == "iMovieInfoImage")
                            {
                                if (Posy < ScreenBottom)
                                {
                                    ScreenBottom--;
                                }
                                if (Posy < ScreenBottom)
                                {
                                    ScreenBottom--;
                                }
                                if (Posy < ScreenBottom)
                                {
                                    ScreenBottom--;
                                }
                                if (Posy < ScreenBottom)
                                {
                                    ScreenBottom--;
                                }
                            }

                            Thread.Sleep(1);
                        }
                        osdServices.OsdSetBitmap(name, leftEye, rightEye, colorkey, Posx, ScreenBottom, false, ImagePriority, Duration, 0, mouseCallback, callbackContext, reserved);

                        startTransition = true;
                    }
                    else
                    {
                        osdServices.OsdSetBitmap(name, leftEye, rightEye, colorkey, Posx, Posy, false, ImagePriority, Duration, 0, mouseCallback, callbackContext, reserved);
                    }
                }
                else
                {
                    osdServices.OsdSetBitmap(name, leftEye, rightEye, colorkey, Posx, Posy, false, ImagePriority, Duration, 0, mouseCallback, callbackContext, reserved);
                }
            }
            else
            {
                osdServices.OsdSetBitmap(name, leftEye, rightEye, colorkey, Posx, Posy, false, ImagePriority, Duration, 0, mouseCallback, callbackContext, reserved);
            }
            if (name.ToLower().Contains("pause") || name.ToLower().StartsWith("pause"))
            {
                osdServices.OsdRedrawFrame();
            }
            //else if (Madvr_VideoPlayer.Plugin.Madvr_VideoPlayer.VideoPlayer.m_state == "Paused")
            //{
            //    osdServices.OsdRedrawFrame();
            //}
        }

        public static void RedrawFrame(MadVR madvr)
        {
            var osdServices = (IMadVROsdServices)madvr;
            osdServices.OsdRedrawFrame();
        }



        //public static void ClearMadVrBitmap(string name, MeediOS.Media.MadVR madvr)
        //{
        //    try
        //    {
        //        if (madvr == null)
        //        {
        //            Madvr_VideoPlayer.Plugin.WriteLog("[Madvr Interface] Showbitmap Interface error: madvr is null");
        //            return;
        //        }

        //        IntPtr leftEye = IntPtr.Zero;
        //        IntPtr rightEye = IntPtr.Zero;
        //        uint colorkey = 0;
        //        int Posx = 0;
        //        int Posy = 0;
        //        uint Duration = 1;
        //        int ImagePriority = 100;
        //        reserved = IntPtr.Zero;

        //        IMadVROsdServices osdServices = (IMadVROsdServices)madvr;


        //        //osdServices.OsdSetBitmap(name, leftEye, rightEye, colorkey, Posx, Posy, false, ImagePriority, Duration, 0, mouseCallback, callbackContext, reserved);

        //        int ScreenBottom = Madvr_VideoPlayer.Plugin.Madvr_VideoPlayer.VideoPlayer.ScreenHeight;
        //        if (CurrentPosY > (ScreenBottom / 2))
        //        {
        //            if (name != "transportTime")
        //            {
        //                if (startTransition == true
        //                    && name == CurrentName
        //                    && (Madvr_VideoPlayer.Plugin.ArrowDirection == "up" || Madvr_VideoPlayer.Plugin.ArrowDirection == "down"))
        //                {
        //                    while (CurrentPosY < ScreenBottom)
        //                    {
        //                        osdServices.OsdSetBitmap(CurrentName, CurrentLeftEye, rightEye, colorkey, CurrentPosX, CurrentPosY, false, CurrentImagePriority, CurrentDuration, 0, mouseCallback, callbackContext, reserved);
        //                        if (CurrentPosY < ScreenBottom)
        //                        {
        //                            CurrentPosY++;
        //                        }
        //                        if (CurrentPosY < ScreenBottom)
        //                        {
        //                            CurrentPosY++;
        //                        }
        //                        if (CurrentPosY < ScreenBottom)
        //                        {
        //                            CurrentPosY++;
        //                        }
        //                        if (CurrentPosY < ScreenBottom)
        //                        {
        //                            CurrentPosY++;
        //                        }
        //                        if (name == "iMovieInfoImage")
        //                        {
        //                            if (CurrentPosY < ScreenBottom)
        //                            {
        //                                CurrentPosY++;
        //                            }
        //                            if (CurrentPosY < ScreenBottom)
        //                            {
        //                                CurrentPosY++;
        //                            }
        //                            if (CurrentPosY < ScreenBottom)
        //                            {
        //                                CurrentPosY++;
        //                            }
        //                            if (CurrentPosY < ScreenBottom)
        //                            {
        //                                CurrentPosY++;
        //                            }
        //                        }

        //                        Thread.Sleep(1);
        //                    }
        //                    osdServices.OsdSetBitmap(name, leftEye, rightEye, colorkey, Posx, Posy, false, ImagePriority, Duration, 0, mouseCallback, callbackContext, reserved);

        //                    startTransition = false;
        //                }
        //                else
        //                {
        //                    osdServices.OsdSetBitmap(name, leftEye, rightEye, colorkey, Posx, Posy, false, ImagePriority, Duration, 0, mouseCallback, callbackContext, reserved);
        //                }
        //            }
        //            else
        //            {
        //                osdServices.OsdSetBitmap(name, leftEye, rightEye, colorkey, Posx, Posy, false, ImagePriority, Duration, 0, mouseCallback, callbackContext, reserved);
        //            }
        //        }
        //        else
        //        {
        //            osdServices.OsdSetBitmap(name, leftEye, rightEye, colorkey, Posx, Posy, false, ImagePriority, Duration, 0, mouseCallback, callbackContext, reserved);
        //        }

        //    }
        //    catch (Exception e)
        //    {
        //        Madvr_VideoPlayer.Plugin.WriteLog("[Madvr Interface] MadVr Clear Bitmap Error: " + e);
        //    }
        //}

        public static void EnableExclusiveMode(bool enable, object madvr)
        {
            var eModeControl = madvr as IMadVRExclusiveModeControl;

            if (enable)
            {
                var size = Marshal.SizeOf(typeof(Int32));
                var pBool = Marshal.AllocHGlobal(size);

                Marshal.WriteInt32(pBool, 0, 1);  // last parameter 0 (FALSE), 1 (TRUE)

                eModeControl.DisableExclusiveMode(pBool);

                Marshal.FreeHGlobal(pBool);
                pBool = IntPtr.Zero;
            }
            else  //Works - Set to Windowed Mode
            {
                var size = Marshal.SizeOf(typeof(Int32));
                var pBool = Marshal.AllocHGlobal(size);

                Marshal.WriteInt32(pBool, 0, 0);  // last parameter 0 (FALSE), 1 (TRUE)

                eModeControl.DisableExclusiveMode(pBool);

                Marshal.FreeHGlobal(pBool);
                pBool = IntPtr.Zero;
            }
        }

        public static bool InExclusiveMode(MadVR madvr)
        {
            var exclusiveInfo = madvr as IMadVRExclusiveModeInfo;

            return exclusiveInfo.IsExclusiveModeActive() == 1;
        }

        //public static bool MadvrSeekBarEnabled(MeediOS.Media.MadVR madvr)
        //{
        //    try
        //    {
        //        IMadVRExclusiveModeInfo exclusiveInfo = (IMadVRExclusiveModeInfo)madvr;
        //        bool status = false;
        //        exclusiveInfo.IsMadVRSeekbarEnabled(out status);
        //        return status;
        //    }
        //    catch (Exception e)
        //    {
        //        Madvr_VideoPlayer.Plugin.WriteLog("[Madvr Interface] Get madvr seek bar enabled interface error: " + e);
        //    }
        //    return false;
        //}

        //public static void EnableMadvrSeekBar(bool enable, MeediOS.Media.MadVR madvr)
        //{
        //    try
        //    {
        //        if (madvr == null)
        //        {
        //            Madvr_VideoPlayer.Plugin.WriteLog("[Madvr Interface] Madvr seek bar error: madvr is null");
        //            return;
        //        }

        //        Int32 iEnable;
        //        if (enable == true)
        //        {
        //            iEnable = 1;
        //        }
        //        else
        //        {
        //            iEnable = 0;
        //        }
        //        int size = Marshal.SizeOf(iEnable);
        //        IntPtr pBool = Marshal.AllocHGlobal(size);
        //        Marshal.WriteInt32(pBool, 0, 1);  // last parameter 0 (FALSE), 1 (TRUE)
        //        IMadVRSeekbarControl mvrSeekbar = (IMadVRSeekbarControl)madvr;
        //        mvrSeekbar.DisableSeekbar(pBool);
        //        Marshal.FreeHGlobal(pBool);
        //    }
        //    catch (Exception e)
        //    {
        //        Madvr_VideoPlayer.Plugin.WriteLog("[Madvr Interface] Exclusive mode interface: " + e);
        //    }
        //}

        //public static void SetRenderCallback(MeediOS.Media.MadVR madvr)
        //{
        //    try
        //    {
        //        IMadVROsdServices osdServices = (IMadVROsdServices)madvr;
        //        osdServices.OsdSetRenderCallback("MeediosPlayer", osdRenderCallback, reserved);
        //        IDirect3DDevice9 test;
        //        osdRenderCallback.SetDevice(out test);

        //    }
        //    catch (Exception e)
        //    {
        //        Madvr_VideoPlayer.Plugin.WriteLog("[Madvr Interface] Error setting callback: " + e);
        //    }
        //}

        //public static Rectangle GetMediaDimensions(bool GetActiveVideo, MeediOS.Media.MadVR madvr)
        //{
        //    if (madvr == null)
        //    {
        //        Madvr_VideoPlayer.Plugin.WriteLog("[Madvr Interface] Madvr is null");
        //    }

        //    Rectangle FullOutput = new Rectangle();
        //    Rectangle ActiveVideo = new Rectangle();
        //    try
        //    {
        //        IMadVROsdServices osdServices = (IMadVROsdServices)madvr;
        //        osdServices.OsdGetVideoRects(out FullOutput, out ActiveVideo);

        //        Madvr_VideoPlayer.Plugin.WriteLog("Active video width: " + ActiveVideo.Width.ToString());
        //        Madvr_VideoPlayer.Plugin.WriteLog("Active video height: " + ActiveVideo.Height.ToString());
        //        Madvr_VideoPlayer.Plugin.WriteLog("Active video left: " + ActiveVideo.Left.ToString());
        //        Madvr_VideoPlayer.Plugin.WriteLog("Active video right: " + ActiveVideo.Right.ToString());
        //        Madvr_VideoPlayer.Plugin.WriteLog("Active video top: " + ActiveVideo.Top.ToString());
        //        Madvr_VideoPlayer.Plugin.WriteLog("Active video bottom: " + ActiveVideo.Bottom.ToString());

        //        Madvr_VideoPlayer.Plugin.WriteLog("Full output width: " + FullOutput.Width.ToString());
        //        Madvr_VideoPlayer.Plugin.WriteLog("Full output height: " + FullOutput.Height.ToString());
        //        Madvr_VideoPlayer.Plugin.WriteLog("Full output left: " + FullOutput.Left.ToString());
        //        Madvr_VideoPlayer.Plugin.WriteLog("Full output right: " + FullOutput.Right.ToString());
        //        Madvr_VideoPlayer.Plugin.WriteLog("Full output top: " + FullOutput.Top.ToString());
        //        Madvr_VideoPlayer.Plugin.WriteLog("Full output bottom: " + FullOutput.Bottom.ToString());

        //        if (GetActiveVideo == true)
        //        {
        //            return ActiveVideo;
        //        }
        //        else
        //        {
        //            return FullOutput;
        //        }
        //    }
        //    catch (Exception e)
        //    {
        //        Madvr_VideoPlayer.Plugin.WriteLog("[Madvr Interface] Error getting dimensions: " + e);
        //    }
        //    return FullOutput;
        //}

        //public static void GetMadvrSettings(MeediOS.Media.MadVR madvr)
        //{
        //    try
        //    {
        //        IMadVRSettings mvrSettings = (IMadVRSettings)madvr;
        //        //string path = "decodeH264";
        //        //string value = "";
        //        //int bufferSize = 25;
        //        //int decodeh264 = mvrSettings.SettingsGetString(path, out value, bufferSize);

        //        string path = "autoActivateDeinterlacing";
        //        IntPtr settingbool;
        //        int autoActivateDeinterlacing = mvrSettings.SettingsGetBoolean(path, out settingbool);

        //        Madvr_VideoPlayer.Plugin.WriteLog("Madvr Setting: " + path + ": " + settingbool.ToString());
        //    }
        //    catch (Exception e)
        //    {
        //        Madvr_VideoPlayer.Plugin.WriteLog("Madvr Setting Error: " + e);
        //    }


        //    try
        //    {
        //        Madvr_VideoPlayer.Plugin.WriteLog("Getting Madvr Setting");
        //        SettingFilePath = "C:\\Users\\Administrator\\Desktop\\madVR\\settings.bin";
        //        string s1;
        //        byte b1;
        //        int i1;
        //        float f1;
        //        double d1;
        //        char[] ca;

        //        using (BinaryReader binReader = new BinaryReader(File.Open(SettingFilePath, FileMode.Open)))
        //        {
        //            try
        //            {


        //                while (true)
        //                {
        //                    s1 = binReader.ReadString();
        //                    b1 = binReader.ReadByte();
        //                    i1 = binReader.ReadInt32();
        //                    f1 = binReader.ReadSingle();
        //                    d1 = binReader.ReadDouble();
        //                    ca = binReader.ReadChars(5);

        //                    //Console.WriteLine(f1);
        //                    Madvr_VideoPlayer.Plugin.WriteLog("Madvr Setting: " + s1);
        //                }
        //            }
        //            catch (EndOfStreamException ex)
        //            {
        //                // end of file reached
        //            }
        //        }
        //    }
        //    catch (Exception e)
        //    {
        //        Madvr_VideoPlayer.Plugin.WriteLog("Madvr Setting Error: " + e);
        //    }

        //}
    }
}
