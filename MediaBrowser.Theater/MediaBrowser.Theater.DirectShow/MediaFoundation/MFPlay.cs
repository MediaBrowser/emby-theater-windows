#region license

/*
MediaFoundationLib - Provide access to MediaFoundation interfaces via .NET
Copyright (C) 2007
http://mfnet.sourceforge.net

This library is free software; you can redistribute it and/or
modify it under the terms of the GNU Lesser General Public
License as published by the Free Software Foundation; either
version 2.1 of the License, or (at your option) any later version.

This library is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
Lesser General Public License for more details.

You should have received a copy of the GNU Lesser General Public
License along with this library; if not, write to the Free Software
Foundation, Inc., 51 Franklin St, Fifth Floor, Boston, MA  02110-1301  USA
*/

#endregion

using System;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Security;

using MediaFoundation.Misc;
using System.Drawing;

using MediaFoundation.EVR;

namespace MediaFoundation.MFPlayer
{
    #region Declarations

#if ALLOW_UNTESTED_INTERFACES

    [Flags, UnmanagedName("MFP_CREATION_OPTIONS")]
    public enum MFP_CREATION_OPTIONS
    {
        None = 0x00000000,
        FreeThreadedCallback = 0x00000001,
        NoMMCSS = 0x00000002,
        NoRemoteDesktopOptimization = 0x00000004
    }

    [UnmanagedName("MFP_MEDIAPLAYER_STATE")]
    public enum MFP_MEDIAPLAYER_STATE
    {
        Empty = 0x00000000,
        Stopped = 0x00000001,
        Playing = 0x00000002,
        Paused = 0x00000003,
        Shutdown = 0x00000004
    }

    [Flags, UnmanagedName("MFP_MEDIAITEM_CHARACTERISTICS")]
    public enum MFP_MEDIAITEM_CHARACTERISTICS
    {
        None = 0x00000000,
        IsLive = 0x00000001,
        CanSeek = 0x00000002,
        CanPause = 0x00000004,
        HasSlowSeek = 0x00000008
    }

    [Flags, UnmanagedName("MFP_CREDENTIAL_FLAGS")]
    public enum MFP_CREDENTIAL_FLAGS
    {
        None = 0x00000000,
        Prompt = 0x00000001,
        Save = 0x00000002,
        DoNotCache = 0x00000004,
        ClearText = 0x00000008,
        Proxy = 0x00000010,
        LoggedOnUser = 0x00000020
    }

    [UnmanagedName("MFP_EVENT_TYPE")]
    public enum MFP_EVENT_TYPE
    {
        Play = 0,
        Pause = 1,
        Stop = 2,
        PositionSet = 3,
        RateSet = 4,
        MediaItemCreated = 5,
        MediaItemSet = 6,
        FrameStep = 7,
        MediaItemCleared = 8,
        MF = 9,
        Error = 10,
        PlaybackEnded = 11,
        AcquireUserCredential = 12
    }

    [StructLayout(LayoutKind.Sequential), UnmanagedName("MFP_EVENT_HEADER")]
    public class MFP_EVENT_HEADER
    {
        public MFP_EVENT_TYPE eEventType;
        public int hrEvent;
        public IMFPMediaPlayer pMediaPlayer;
        public MFP_MEDIAPLAYER_STATE eState;
        public IPropertyStore pPropertyStore;

        public IntPtr GetPtr()
        {
            IntPtr ip;

            int iSize = Marshal.SizeOf(this);

            ip = Marshal.AllocCoTaskMem(iSize);
            Marshal.StructureToPtr(this, ip, false);

            return ip;
        }

        public static MFP_EVENT_HEADER PtrToEH(IntPtr pNativeData)
        {
            MFP_EVENT_TYPE met = (MFP_EVENT_TYPE)Marshal.ReadInt32(pNativeData);
            object mce;

            switch (met)
            {
                case MFP_EVENT_TYPE.Play:
                    mce = Marshal.PtrToStructure(pNativeData, typeof(MFP_PLAY_EVENT));
                    break;
                case MFP_EVENT_TYPE.Pause:
                    mce = Marshal.PtrToStructure(pNativeData, typeof(MFP_PAUSE_EVENT));
                    break;
                case MFP_EVENT_TYPE.Stop:
                    mce = Marshal.PtrToStructure(pNativeData, typeof(MFP_STOP_EVENT));
                    break;
                case MFP_EVENT_TYPE.PositionSet:
                    mce = Marshal.PtrToStructure(pNativeData, typeof(MFP_POSITION_SET_EVENT));
                    break;
                case MFP_EVENT_TYPE.RateSet:
                    mce = Marshal.PtrToStructure(pNativeData, typeof(MFP_RATE_SET_EVENT));
                    break;
                case MFP_EVENT_TYPE.MediaItemCreated:
                    mce = Marshal.PtrToStructure(pNativeData, typeof(MFP_MEDIAITEM_CREATED_EVENT));
                    break;
                case MFP_EVENT_TYPE.MediaItemSet:
                    mce = Marshal.PtrToStructure(pNativeData, typeof(MFP_RATE_SET_EVENT));
                    break;
                case MFP_EVENT_TYPE.FrameStep:
                    mce = Marshal.PtrToStructure(pNativeData, typeof(MFP_FRAME_STEP_EVENT));
                    break;
                case MFP_EVENT_TYPE.MediaItemCleared:
                    mce = Marshal.PtrToStructure(pNativeData, typeof(MFP_MEDIAITEM_CLEARED_EVENT));
                    break;
                case MFP_EVENT_TYPE.MF:
                    mce = Marshal.PtrToStructure(pNativeData, typeof(MFP_MF_EVENT));
                    break;
                case MFP_EVENT_TYPE.Error:
                    mce = Marshal.PtrToStructure(pNativeData, typeof(MFP_ERROR_EVENT));
                    break;
                case MFP_EVENT_TYPE.PlaybackEnded:
                    mce = Marshal.PtrToStructure(pNativeData, typeof(MFP_PLAYBACK_ENDED_EVENT));
                    break;
                case MFP_EVENT_TYPE.AcquireUserCredential:
                    mce = Marshal.PtrToStructure(pNativeData, typeof(MFP_ACQUIRE_USER_CREDENTIAL_EVENT));
                    break;
                default:
                    // Don't know what it is.  Send back the header.
                    mce = Marshal.PtrToStructure(pNativeData, typeof(MFP_EVENT_HEADER));
                    break;
            }

            return mce as MFP_EVENT_HEADER;
        }
    }

    [StructLayout(LayoutKind.Sequential), UnmanagedName("MFP_PLAY_EVENT")]
    public class MFP_PLAY_EVENT : MFP_EVENT_HEADER
    {
        public IMFPMediaItem pMediaItem;
    }

    [StructLayout(LayoutKind.Sequential), UnmanagedName("MFP_PAUSE_EVENT")]
    public class MFP_PAUSE_EVENT : MFP_EVENT_HEADER
    {
        public IMFPMediaItem pMediaItem;
    }

    [StructLayout(LayoutKind.Sequential), UnmanagedName("MFP_STOP_EVENT")]
    public class MFP_STOP_EVENT : MFP_EVENT_HEADER
    {
        public IMFPMediaItem pMediaItem;
    }

    [StructLayout(LayoutKind.Sequential), UnmanagedName("MFP_POSITION_SET_EVENT")]
    public class MFP_POSITION_SET_EVENT : MFP_EVENT_HEADER
    {
        public IMFPMediaItem pMediaItem;
    }

    [StructLayout(LayoutKind.Sequential), UnmanagedName("MFP_RATE_SET_EVENT")]
    public class MFP_RATE_SET_EVENT : MFP_EVENT_HEADER
    {
        public IMFPMediaItem pMediaItem;
        public float flRate;
    }

    [StructLayout(LayoutKind.Sequential), UnmanagedName("MFP_MEDIAITEM_CREATED_EVENT")]
    public class MFP_MEDIAITEM_CREATED_EVENT : MFP_EVENT_HEADER
    {
        public IMFPMediaItem pMediaItem;
        public IntPtr dwUserData;
    }

    [StructLayout(LayoutKind.Sequential), UnmanagedName("MFP_MEDIAITEM_SET_EVENT")]
    public class MFP_MEDIAITEM_SET_EVENT : MFP_EVENT_HEADER
    {
        public IMFPMediaItem pMediaItem;
    }

    [StructLayout(LayoutKind.Sequential), UnmanagedName("MFP_FRAME_STEP_EVENT")]
    public class MFP_FRAME_STEP_EVENT : MFP_EVENT_HEADER
    {
        public IMFPMediaItem pMediaItem;
    }

    [StructLayout(LayoutKind.Sequential), UnmanagedName("MFP_MEDIAITEM_CLEARED_EVENT")]
    public class MFP_MEDIAITEM_CLEARED_EVENT : MFP_EVENT_HEADER
    {
        public IMFPMediaItem pMediaItem;
    }

    [StructLayout(LayoutKind.Sequential), UnmanagedName("MFP_MF_EVENT")]
    public class MFP_MF_EVENT : MFP_EVENT_HEADER
    {
        public MediaEventType MFEventType;
        public IMFMediaEvent pMFMediaEvent;
        public IMFPMediaItem pMediaItem;
    }

    [StructLayout(LayoutKind.Sequential), UnmanagedName("MFP_ERROR_EVENT")]
    public class MFP_ERROR_EVENT : MFP_EVENT_HEADER
    {
    }

    [StructLayout(LayoutKind.Sequential), UnmanagedName("MFP_PLAYBACK_ENDED_EVENT")]
    public class MFP_PLAYBACK_ENDED_EVENT : MFP_EVENT_HEADER
    {
        public IMFPMediaItem pMediaItem;
    }

    [StructLayout(LayoutKind.Sequential), UnmanagedName("MFP_ACQUIRE_USER_CREDENTIAL_EVENT")]
    public class MFP_ACQUIRE_USER_CREDENTIAL_EVENT : MFP_EVENT_HEADER
    {
        public IntPtr dwUserData;
        [MarshalAs(UnmanagedType.Bool)]
        public bool fProceedWithAuthentication;
        public int hrAuthenticationStatus;
        [MarshalAs(UnmanagedType.LPWStr)]
        public string pwszURL;
        [MarshalAs(UnmanagedType.LPWStr)]
        public string pwszSite;
        [MarshalAs(UnmanagedType.LPWStr)]
        public string pwszRealm;
        [MarshalAs(UnmanagedType.LPWStr)]
        public string pwszPackage;
        public int nRetries;
        public MFP_CREDENTIAL_FLAGS flags;
        public IMFNetCredential pCredential;
    }

#endif

    #endregion

    #region Interfaces

#if ALLOW_UNTESTED_INTERFACES

    [ComImport, System.Security.SuppressUnmanagedCodeSecurity,
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown),
    Guid("A714590A-58AF-430a-85BF-44F5EC838D85")]
    public interface IMFPMediaPlayer
    {
        [PreserveSig]
        int Play();

        [PreserveSig]
        int Pause();

        [PreserveSig]
        int Stop();

        [PreserveSig]
        int FrameStep();

        [PreserveSig]
        int SetPosition(
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid guidPositionType,
            [In, MarshalAs(UnmanagedType.LPStruct)] ConstPropVariant pvPositionValue
        );

        [PreserveSig]
        int GetPosition(
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid guidPositionType,
            [In, Out, MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(PVMarshaler))] PropVariant pvPositionValue
        );

        [PreserveSig]
        int GetDuration(
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid guidPositionType,
            [In, Out, MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(PVMarshaler))] PropVariant pvPositionValue
        );

        [PreserveSig]
        int SetRate(
            float flRate
        );

        [PreserveSig]
        int GetRate(
            out float pflRate
        );

        [PreserveSig]
        int GetSupportedRates(
            [MarshalAs(UnmanagedType.Bool)] bool fForwardDirection,
            out float pflSlowestRate,
            out float pflFastestRate
        );

        [PreserveSig]
        int GetState(
            out MFP_MEDIAPLAYER_STATE peState
        );

        [PreserveSig]
        int CreateMediaItemFromURL(
            [In, MarshalAs(UnmanagedType.LPWStr)] string pwszURL,
            [MarshalAs(UnmanagedType.Bool)] bool fSync,
            IntPtr dwUserData,
            out IMFPMediaItem ppMediaItem
        );

        [PreserveSig]
        int CreateMediaItemFromObject(
            [MarshalAs(UnmanagedType.IUnknown)] object pIUnknownObj,
            [MarshalAs(UnmanagedType.Bool)] bool fSync,
            IntPtr dwUserData,
            out IMFPMediaItem ppMediaItem
        );

        [PreserveSig]
        int SetMediaItem(
            IMFPMediaItem pIMFPMediaItem
        );

        [PreserveSig]
        int ClearMediaItem();

        [PreserveSig]
        int GetMediaItem(
            out IMFPMediaItem ppIMFPMediaItem
        );

        [PreserveSig]
        int GetVolume(
            out float pflVolume
        );

        [PreserveSig]
        int SetVolume(
            float flVolume
        );

        [PreserveSig]
        int GetBalance(
            out float pflBalance
        );

        [PreserveSig]
        int SetBalance(
            float flBalance
        );

        [PreserveSig]
        int GetMute(
            [MarshalAs(UnmanagedType.Bool)] out bool pfMute
        );

        [PreserveSig]
        int SetMute(
            [MarshalAs(UnmanagedType.Bool)] bool fMute
        );

        [PreserveSig]
        int GetNativeVideoSize(
            out MFSize pszVideo,
            out MFSize pszARVideo
        );

        [PreserveSig]
        int GetIdealVideoSize(
            out MFSize pszMin,
            out MFSize pszMax
        );

        [PreserveSig]
        int SetVideoSourceRect(
            [In] MFVideoNormalizedRect pnrcSource
        );

        [PreserveSig]
        int GetVideoSourceRect(
            out MFVideoNormalizedRect pnrcSource
        );

        [PreserveSig]
        int SetAspectRatioMode(
            MFVideoAspectRatioMode dwAspectRatioMode
        );

        [PreserveSig]
        int GetAspectRatioMode(
            out MFVideoAspectRatioMode pdwAspectRatioMode
        );

        [PreserveSig]
        int GetVideoWindow(
            out IntPtr phwndVideo
        );

        [PreserveSig]
        int UpdateVideo();

        [PreserveSig]
        int SetBorderColor(
            Color Clr
        );

        [PreserveSig]
        int GetBorderColor(
            out Color pClr
        );

        [PreserveSig]
        int InsertEffect(
            [MarshalAs(UnmanagedType.IUnknown)] object pEffect,
            [MarshalAs(UnmanagedType.Bool)] bool fOptional
        );

        [PreserveSig]
        int RemoveEffect(
            [MarshalAs(UnmanagedType.IUnknown)] object pEffect
        );

        [PreserveSig]
        int RemoveAllEffects();

        [PreserveSig]
        int Shutdown();
    }

    [ComImport, System.Security.SuppressUnmanagedCodeSecurity,
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown),
    Guid("90EB3E6B-ECBF-45cc-B1DA-C6FE3EA70D57")]
    public interface IMFPMediaItem
    {
        [PreserveSig]
        int GetMediaPlayer(
            out IMFPMediaPlayer ppMediaPlayer
        );

        [PreserveSig]
        int GetURL(
            [MarshalAs(UnmanagedType.LPWStr)] out string ppwszURL
        );

        [PreserveSig]
        int GetObject(
            [MarshalAs(UnmanagedType.IUnknown)] out object ppIUnknown
        );

        [PreserveSig]
        int GetUserData(
            out IntPtr pdwUserData
        );

        [PreserveSig]
        int SetUserData(
            IntPtr dwUserData
        );

        [PreserveSig]
        int GetStartStopPosition(
            out Guid pguidStartPositionType,
            [In, Out, MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(PVMarshaler))] PropVariant pvStartValue,
            out Guid pguidStopPositionType,
            [In, Out, MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(PVMarshaler))] PropVariant pvStopValue
        );

        [PreserveSig]
        int SetStartStopPosition(
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid pguidStartPositionType,
            [In, MarshalAs(UnmanagedType.LPStruct)] ConstPropVariant pvStartValue,
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid pguidStopPositionType,
            [In, MarshalAs(UnmanagedType.LPStruct)] ConstPropVariant pvStopValue
        );

        [PreserveSig]
        int HasVideo(
            [MarshalAs(UnmanagedType.Bool)] out bool pfHasVideo,
            [MarshalAs(UnmanagedType.Bool)] out bool pfSelected
        );

        [PreserveSig]
        int HasAudio(
            [MarshalAs(UnmanagedType.Bool)] out bool pfHasAudio,
            [MarshalAs(UnmanagedType.Bool)] out bool pfSelected
        );

        [PreserveSig]
        int IsProtected(
            [MarshalAs(UnmanagedType.Bool)] out bool pfProtected
        );

        [PreserveSig]
        int GetDuration(
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid guidPositionType,
            [In, Out, MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(PVMarshaler))] PropVariant pvDurationValue
        );

        [PreserveSig]
        int GetNumberOfStreams(
            out int pdwStreamCount
        );

        [PreserveSig]
        int GetStreamSelection(
            int dwStreamIndex,
            [MarshalAs(UnmanagedType.Bool)] out bool pfEnabled
        );

        [PreserveSig]
        int SetStreamSelection(
            int dwStreamIndex,
            [MarshalAs(UnmanagedType.Bool)] bool fEnabled
        );

        [PreserveSig]
        int GetStreamAttribute(
            int dwStreamIndex,
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid guidMFAttribute,
            [In, Out, MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(PVMarshaler))] PropVariant pvValue
        );

        [PreserveSig]
        int GetPresentationAttribute(
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid guidMFAttribute,
            [In, Out, MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(PVMarshaler))] PropVariant pvValue
        );

        [PreserveSig]
        int GetCharacteristics(
            out MFP_MEDIAITEM_CHARACTERISTICS pCharacteristics
        );

        [PreserveSig]
        int SetStreamSink(
            int dwStreamIndex,
            [MarshalAs(UnmanagedType.IUnknown)] object pMediaSink
        );

        [PreserveSig]
        int GetMetadata(
            out IPropertyStore ppMetadataStore
        );
    }

    [ComImport, System.Security.SuppressUnmanagedCodeSecurity,
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown),
    Guid("766C8FFB-5FDB-4fea-A28D-B912996F51BD")]
    public interface IMFPMediaPlayerCallback
    {
        [PreserveSig]
        int OnMediaPlayerEvent(
            [In, MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(EHMarshaler))] MFP_EVENT_HEADER pEventHeader
            );
    }

    internal class EHMarshaler : ICustomMarshaler
    {
        public IntPtr MarshalManagedToNative(object managedObj)
        {
            MFP_EVENT_HEADER eh = managedObj as MFP_EVENT_HEADER;

            IntPtr ip = eh.GetPtr();

            return ip;
        }

        // Called just after invoking the COM method.  The IntPtr is the same one that just got returned
        // from MarshalManagedToNative.  The return value is unused.
        public object MarshalNativeToManaged(IntPtr pNativeData)
        {
            MFP_EVENT_HEADER eh = MFP_EVENT_HEADER.PtrToEH(pNativeData);

            return eh;
        }

        // It appears this routine is never called
        public void CleanUpManagedData(object ManagedObj)
        {
        }

        public void CleanUpNativeData(IntPtr pNativeData)
        {
            Marshal.FreeCoTaskMem(pNativeData);
        }

        // The number of bytes to marshal out - never called
        public int GetNativeDataSize()
        {
            return -1;
        }

        // This method is called by interop to create the custom marshaler.  The (optional)
        // cookie is the value specified in MarshalCookie="asdf", or "" is none is specified.
        public static ICustomMarshaler GetInstance(string cookie)
        {
            return new EHMarshaler();
        }
    }

#endif

    #endregion
}
