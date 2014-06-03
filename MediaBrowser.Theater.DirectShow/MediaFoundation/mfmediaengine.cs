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

using MediaFoundation.Misc;
using MediaFoundation.EVR;
using MediaFoundation.Transform;

namespace MediaFoundation
{

    #region Declarations

#if ALLOW_UNTESTED_INTERFACES

    [UnmanagedName("MF_MEDIA_ENGINE_ERR")]
    public enum MF_MEDIA_ENGINE_ERR : short
    {
        NoError = 0,
        Aborted = 1,
        Network = 2,
        Decode = 3,
        SrcNotSupported = 4
    }

    [UnmanagedName("MF_MEDIA_ENGINE_EVENT")]
    public enum MF_MEDIA_ENGINE_EVENT
    {
        LoadStart = 1,
        Progress = 2,
        Suspend = 3,
        Abort = 4,
        Error = 5,
        Emptied = 6,
        Stalled = 7,
        Play = 8,
        Pause = 9,
        LoadedMetadata = 10,
        LoadedData = 11,
        Waiting = 12,
        Playing = 13,
        CanPlay = 14,
        CanPlayThrough = 15,
        Seeking = 16,
        Seeked = 17,
        TimeUpdate = 18,
        Ended = 19,
        RateChange = 20,
        DurationChange = 21,
        VolumeChange = 22,

        FormatChange = 1000,
        PurgeQueuedEvents = 1001,
        TimelineMarker = 1002,
        BalanceChange = 1003,
        DownloadComplete = 1004,
        BufferingStarted = 1005,
        BufferingEnded = 1006,
        FrameStepCompleted = 1007,
        NotifyStableState = 1008
    }

    [UnmanagedName("MF_MEDIA_ENGINE_NETWORK")]
    public enum MF_MEDIA_ENGINE_NETWORK : short
    {
        Empty = 0,
        Idle = 1,
        Loading = 2,
        NoSource = 3
    }

    [UnmanagedName("MF_MEDIA_ENGINE_READY")]
    public enum MF_MEDIA_ENGINE_READY : short
    {
        HaveNothing = 0,
        HaveMetadata = 1,
        HaveCurrentData = 2,
        HaveFutureData = 3,
        HaveEnoughData = 4
    }

    [UnmanagedName("MF_MEDIA_ENGINE_CANPLAY")]
    public enum MF_MEDIA_ENGINE_CANPLAY
    {
        NotSupported = 0,
        Maybe = 1,
        Probably = 2,
    }


    [UnmanagedName("MF_MEDIA_ENGINE_PRELOAD")]
    public enum MF_MEDIA_ENGINE_PRELOAD
    {
        Missing = 0,
        Empty = 1,
        None = 2,
        Metadata = 3,
        Automatic = 4
    }


    [UnmanagedName("MF_MEDIA_ENGINE_S3D_PACKING_MODE")]
    public enum MF_MEDIA_ENGINE_S3D_PACKING_MODE
    {
        None = 0,
        SideBySide = 1,
        TopBottom = 2

    }

    [UnmanagedName("MF_MEDIA_ENGINE_STATISTIC")]
    public enum MF_MEDIA_ENGINE_STATISTIC
    {
        FramesRendered = 0,
        FramesDropped = 1,
        BytesDownloaded = 2,
        BufferProgress = 3,
        FramesPerSecond = 4

    }

    [UnmanagedName("MF_MEDIA_ENGINE_SEEK_MODE")]
    public enum MF_MEDIA_ENGINE_SEEK_MODE
    {
        Normal = 0,
        Approximate = 1
    }

    [UnmanagedName("MF_MEDIA_ENGINE_EXTENSION_TYPE")]
    public enum MF_MEDIA_ENGINE_EXTENSION_TYPE
    {
        MediaSource = 0,
        ByteStream = 1
    }

    [Flags, UnmanagedName("MF_MEDIA_ENGINE_FRAME_PROTECTION_FLAGS")]
    public enum MF_MEDIA_ENGINE_FRAME_PROTECTION_FLAGS
    {
        None = 0x0,
        Protected = 0x01,
        RequiresSurfaceProtection = 0x02,
        RequiresAntiScreenScrapeProtection = 0x04
    }

    [Flags, UnmanagedName("MF_MEDIA_ENGINE_CREATEFLAGS")]
    public enum MF_MEDIA_ENGINE_CREATEFLAGS
    {
        AudioOnly = 0x0001,
        WaitForStableState = 0x0002,
        ForceMute = 0x0004,
        RealTimeMode = 0x0008,
        DisableLocalPlugins = 0x0010,
        CreateFlagsMask = 0x001F
    }

    [Flags, UnmanagedName("MF_MEDIA_ENGINE_PROTECTION_FLAGS")]
    public enum MF_MEDIA_ENGINE_PROTECTION_FLAGS
    {
        EnableProtectedContent = 1,
        UsePMPForAllContent = 2,
        UseUnprotectedPMP = 4

    }

#endif

    #endregion

    #region Interfaces

#if ALLOW_UNTESTED_INTERFACES

    [ComImport, System.Security.SuppressUnmanagedCodeSecurity,
    Guid("fc0e10d2-ab2a-4501-a951-06bb1075184c"),
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IMFMediaError
    {
        [PreserveSig]
        MF_MEDIA_ENGINE_ERR GetErrorCode();

        [PreserveSig]
        int GetExtendedErrorCode();

        [PreserveSig]
        int SetErrorCode(
            MF_MEDIA_ENGINE_ERR error
            );

        [PreserveSig]
        int SetExtendedErrorCode(
            int error
            );
    }

    [ComImport, System.Security.SuppressUnmanagedCodeSecurity,
    Guid("db71a2fc-078a-414e-9df9-8c2531b0aa6c"),
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IMFMediaTimeRange
    {
        [PreserveSig]
        int GetLength();

        [PreserveSig]
        int GetStart(
            int index,
            out double pStart
            );

        [PreserveSig]
        int GetEnd(
            int index,
            out  double pEnd
            );

        [return: MarshalAs(UnmanagedType.Bool)]
        bool ContainsTime(
            double time
            );

        [PreserveSig]
        int AddRange(
            double startTime,
            double endTime
            );

        [PreserveSig]
        int Clear();
    }

    [ComImport, System.Security.SuppressUnmanagedCodeSecurity,
    Guid("fee7c112-e776-42b5-9bbf-0048524e2bd5"),
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IMFMediaEngineNotify
    {
        [PreserveSig]
        int EventNotify(
            MF_MEDIA_ENGINE_EVENT eventid,
            IntPtr param1,
            int param2
            );
    }

    [ComImport, System.Security.SuppressUnmanagedCodeSecurity,
    Guid("7a5e5354-b114-4c72-b991-3131d75032ea"),
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IMFMediaEngineSrcElements
    {
        [PreserveSig]
        int GetLength();

        [PreserveSig]
        int GetURL(
            int index,
            [MarshalAs(UnmanagedType.BStr)] out string pURL
            );

        [PreserveSig]
        int GetType(
            int index,
            [MarshalAs(UnmanagedType.BStr)] out string pType
            );

        [PreserveSig]
        int GetMedia(
            int index,
            [MarshalAs(UnmanagedType.BStr)] out string pMedia
            );

        [PreserveSig]
        int AddElement(
            [MarshalAs(UnmanagedType.BStr)] string pURL,
            [MarshalAs(UnmanagedType.BStr)] string pType,
            [MarshalAs(UnmanagedType.BStr)] string pMedia
            );

        [PreserveSig]
        int RemoveAllElements();
    }

    [ComImport, System.Security.SuppressUnmanagedCodeSecurity,
    Guid("98a1b0bb-03eb-4935-ae7c-93c1fa0e1c93"),
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IMFMediaEngine
    {
        [PreserveSig]
        int GetError(
            out IMFMediaError ppError
            );

        [PreserveSig]
        int SetErrorCode(
            MF_MEDIA_ENGINE_ERR error
            );

        [PreserveSig]
        int SetSourceElements(
            IMFMediaEngineSrcElements pSrcElements
            );

        [PreserveSig]
        int SetSource(
            [MarshalAs(UnmanagedType.BStr)] string pUrl
            );

        [PreserveSig]
        int GetCurrentSource(
            [MarshalAs(UnmanagedType.BStr)] out string ppUrl
            );

        [PreserveSig]
        MF_MEDIA_ENGINE_NETWORK GetNetworkState();

        [PreserveSig]
        MF_MEDIA_ENGINE_PRELOAD GetPreload();

        [PreserveSig]
        int SetPreload(
            MF_MEDIA_ENGINE_PRELOAD Preload
            );

        [PreserveSig]
        int GetBuffered(
            out IMFMediaTimeRange ppBuffered
            );

        [PreserveSig]
        int Load();

        [PreserveSig]
        int CanPlayType(
            [MarshalAs(UnmanagedType.BStr)] string type,
            out MF_MEDIA_ENGINE_CANPLAY pAnswer
            );

        [PreserveSig]
        MF_MEDIA_ENGINE_READY GetReadyState();

        [return: MarshalAs(UnmanagedType.Bool)]
        bool IsSeeking();

        [PreserveSig]
        double GetCurrentTime();

        [PreserveSig]
        int SetCurrentTime(
            double seekTime
            );

        [PreserveSig]
        double GetStartTime();

        [PreserveSig]
        double GetDuration();

        [return: MarshalAs(UnmanagedType.Bool)]
        bool IsPaused();

        [PreserveSig]
        double GetDefaultPlaybackRate();

        [PreserveSig]
        int SetDefaultPlaybackRate(
            double Rate
            );

        [PreserveSig]
        double GetPlaybackRate();

        [PreserveSig]
        int SetPlaybackRate(
            double Rate
            );

        [PreserveSig]
        int GetPlayed(
            out IMFMediaTimeRange ppPlayed
            );

        [PreserveSig]
        int GetSeekable(
            out IMFMediaTimeRange ppSeekable
            );

        [return: MarshalAs(UnmanagedType.Bool)]
        bool IsEnded();

        [return: MarshalAs(UnmanagedType.Bool)]
        bool GetAutoPlay();

        [PreserveSig]
        int SetAutoPlay(
            [MarshalAs(UnmanagedType.Bool)] bool AutoPlay
            );

        [return: MarshalAs(UnmanagedType.Bool)]
        bool GetLoop();

        [PreserveSig]
        int SetLoop(
            [MarshalAs(UnmanagedType.Bool)] bool Loop
            );

        [PreserveSig]
        int Play();

        [PreserveSig]
        int Pause();

        [return: MarshalAs(UnmanagedType.Bool)]
        bool GetMuted();

        [PreserveSig]
        int SetMuted(
            [MarshalAs(UnmanagedType.Bool)] bool Muted
            );

        [PreserveSig]
        double GetVolume();

        [PreserveSig]
        int SetVolume(
            double Volume
            );

        [return: MarshalAs(UnmanagedType.Bool)]
        bool HasVideo();

        [return: MarshalAs(UnmanagedType.Bool)]
        bool HasAudio();

        [PreserveSig]
        int GetNativeVideoSize(
            out int cx,
            out int cy
            );

        [PreserveSig]
        int GetVideoAspectRatio(
            out int cx,
            out int cy
            );

        [PreserveSig]
        int Shutdown();

        [PreserveSig]
        int TransferVideoFrame(
            [In, MarshalAs(UnmanagedType.IUnknown)] object pDstSurf,
            [In] MFVideoNormalizedRect pSrc,
            [In] MFRect pDst,
            [In] MFARGB pBorderClr
            );

        [PreserveSig]
        int OnVideoStreamTick(
            out long pPts
            );
    }

    [ComImport, System.Security.SuppressUnmanagedCodeSecurity,
    Guid("83015ead-b1e6-40d0-a98a-37145ffe1ad1"),
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IMFMediaEngineEx : IMFMediaEngine
    {

        #region IMFMediaEngine methods

        [PreserveSig]
        new int GetError(
            out IMFMediaError ppError
            );

        [PreserveSig]
        new int SetErrorCode(
            MF_MEDIA_ENGINE_ERR error
            );

        [PreserveSig]
        new int SetSourceElements(
            IMFMediaEngineSrcElements pSrcElements
            );

        [PreserveSig]
        new int SetSource(
            [MarshalAs(UnmanagedType.BStr)] string pUrl
            );

        [PreserveSig]
        new int GetCurrentSource(
            [MarshalAs(UnmanagedType.BStr)] out string ppUrl
            );

        [PreserveSig]
        new MF_MEDIA_ENGINE_NETWORK GetNetworkState();

        [PreserveSig]
        new MF_MEDIA_ENGINE_PRELOAD GetPreload();

        [PreserveSig]
        new int SetPreload(
            MF_MEDIA_ENGINE_PRELOAD Preload
            );

        [PreserveSig]
        new int GetBuffered(
            out IMFMediaTimeRange ppBuffered
            );

        [PreserveSig]
        new int Load();

        [PreserveSig]
        new int CanPlayType(
            [MarshalAs(UnmanagedType.BStr)] string type,
            out MF_MEDIA_ENGINE_CANPLAY pAnswer
            );

        [PreserveSig]
        new MF_MEDIA_ENGINE_READY GetReadyState();

        [return: MarshalAs(UnmanagedType.Bool)]
        new bool IsSeeking();

        [PreserveSig]
        new double GetCurrentTime();

        [PreserveSig]
        new int SetCurrentTime(
            double seekTime
            );

        [PreserveSig]
        new double GetStartTime();

        [PreserveSig]
        new double GetDuration();

        [return: MarshalAs(UnmanagedType.Bool)]
        new bool IsPaused();

        [PreserveSig]
        new double GetDefaultPlaybackRate();

        [PreserveSig]
        new int SetDefaultPlaybackRate(
            double Rate
            );

        [PreserveSig]
        new double GetPlaybackRate();

        [PreserveSig]
        new int SetPlaybackRate(
            double Rate
            );

        [PreserveSig]
        new int GetPlayed(
            out IMFMediaTimeRange ppPlayed
            );

        [PreserveSig]
        new int GetSeekable(
            out IMFMediaTimeRange ppSeekable
            );

        [return: MarshalAs(UnmanagedType.Bool)]
        new bool IsEnded();

        [return: MarshalAs(UnmanagedType.Bool)]
        new bool GetAutoPlay();

        [PreserveSig]
        new int SetAutoPlay(
            [MarshalAs(UnmanagedType.Bool)] bool AutoPlay
            );

        [return: MarshalAs(UnmanagedType.Bool)]
        new bool GetLoop();

        [PreserveSig]
        new int SetLoop(
            [MarshalAs(UnmanagedType.Bool)] bool Loop
            );

        [PreserveSig]
        new int Play();

        [PreserveSig]
        new int Pause();

        [return: MarshalAs(UnmanagedType.Bool)]
        new bool GetMuted();

        [PreserveSig]
        new int SetMuted(
            [MarshalAs(UnmanagedType.Bool)] bool Muted
            );

        [PreserveSig]
        new double GetVolume();

        [PreserveSig]
        new int SetVolume(
            double Volume
            );

        [return: MarshalAs(UnmanagedType.Bool)]
        new bool HasVideo();

        [return: MarshalAs(UnmanagedType.Bool)]
        new bool HasAudio();

        [PreserveSig]
        new int GetNativeVideoSize(
            out int cx,
            out int cy
            );

        [PreserveSig]
        new int GetVideoAspectRatio(
            out int cx,
            out int cy
            );

        [PreserveSig]
        new int Shutdown();

        [PreserveSig]
        new int TransferVideoFrame(
            [In, MarshalAs(UnmanagedType.IUnknown)] object pDstSurf,
            [In] MFVideoNormalizedRect pSrc,
            [In] MFRect pDst,
            [In] MFARGB pBorderClr
            );

        [PreserveSig]
        new int OnVideoStreamTick(
            out long pPts
            );

        #endregion

        [PreserveSig]
        int SetSourceFromByteStream(
            IMFByteStream pByteStream,
            [MarshalAs(UnmanagedType.BStr)] string pURL
            );

        [PreserveSig]
        int GetStatistics(
            MF_MEDIA_ENGINE_STATISTIC StatisticID,
            [In, Out, MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(PVMarshaler))] PropVariant pStatistic
            );

        [PreserveSig]
        int UpdateVideoStream(
            [In] MFVideoNormalizedRect pSrc,
            [In] MFRect pDst,
            [In] MFARGB pBorderClr
            );

        [PreserveSig]
        double GetBalance();

        [PreserveSig]
        int SetBalance(
            double balance
            );

        [return: MarshalAs(UnmanagedType.Bool)]
        bool IsPlaybackRateSupported(
            double rate
            );

        [PreserveSig]
        int FrameStep(
            [MarshalAs(UnmanagedType.Bool)] bool Forward
            );

        [PreserveSig]
        int GetResourceCharacteristics(
            out int pCharacteristics
            );

        [PreserveSig]
        int GetPresentationAttribute(
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid guidMFAttribute,
            [In, Out, MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(PVMarshaler))] PropVariant pvValue
            );

        [PreserveSig]
        int GetNumberOfStreams(
            out int pdwStreamCount
            );

        [PreserveSig]
        int GetStreamAttribute(
            int dwStreamIndex,
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid guidMFAttribute,
            [In, Out, MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(PVMarshaler))] PropVariant pvValue
            );

        [PreserveSig]
        int GetStreamSelection(
            int dwStreamIndex,
            [MarshalAs(UnmanagedType.Bool)] out bool pEnabled
            );

        [PreserveSig]
        int SetStreamSelection(
            int dwStreamIndex,
            [MarshalAs(UnmanagedType.Bool)] bool Enabled
            );

        [PreserveSig]
        int ApplyStreamSelections();

        [PreserveSig]
        int IsProtected(
            [MarshalAs(UnmanagedType.Bool)] out bool pProtected
            );

        [PreserveSig]
        int InsertVideoEffect(
            [In, MarshalAs(UnmanagedType.IUnknown)] object pEffect,
            [MarshalAs(UnmanagedType.Bool)] bool fOptional
            );

        [PreserveSig]
        int InsertAudioEffect(
            [In, MarshalAs(UnmanagedType.IUnknown)] object pEffect,
            [MarshalAs(UnmanagedType.Bool)] bool fOptional
            );

        [PreserveSig]
        int RemoveAllEffects();

        [PreserveSig]
        int SetTimelineMarkerTimer(
            double timeToFire
            );

        [PreserveSig]
        int GetTimelineMarkerTimer(
            out double pTimeToFire
            );

        [PreserveSig]
        int CancelTimelineMarkerTimer();

        [return: MarshalAs(UnmanagedType.Bool)]
        bool IsStereo3D();

        [PreserveSig]
        int GetStereo3DFramePackingMode(
            out MF_MEDIA_ENGINE_S3D_PACKING_MODE packMode
            );

        [PreserveSig]
        int SetStereo3DFramePackingMode(
            MF_MEDIA_ENGINE_S3D_PACKING_MODE packMode
            );

        [PreserveSig]
        int GetStereo3DRenderMode(
            out MF3DVideoOutputType outputType
            );

        [PreserveSig]
        int SetStereo3DRenderMode(
            MF3DVideoOutputType outputType
            );

        [PreserveSig]
        int EnableWindowlessSwapchainMode(
            [MarshalAs(UnmanagedType.Bool)] bool fEnable
            );

        [PreserveSig]
        int GetVideoSwapchainHandle(
            out IntPtr phSwapchain
            );

        [PreserveSig]
        int EnableHorizontalMirrorMode(
            [MarshalAs(UnmanagedType.Bool)] bool fEnable
            );

        [PreserveSig]
        int GetAudioStreamCategory(
            out int pCategory
            );

        [PreserveSig]
        int SetAudioStreamCategory(
            int category
            );

        [PreserveSig]
        int GetAudioEndpointRole(
            out int pRole
            );

        [PreserveSig]
        int SetAudioEndpointRole(
            int role
            );

        [PreserveSig]
        int GetRealTimeMode(
            [MarshalAs(UnmanagedType.Bool)] out bool pfEnabled
            );

        [PreserveSig]
        int SetRealTimeMode(
            [MarshalAs(UnmanagedType.Bool)] bool fEnable
            );

        [PreserveSig]
        int SetCurrentTimeEx(
            double seekTime,
            MF_MEDIA_ENGINE_SEEK_MODE seekMode
            );

        [PreserveSig]
        int EnableTimeUpdateTimer(
            [MarshalAs(UnmanagedType.Bool)] bool fEnableTimer
            );
    }

    [ComImport, System.Security.SuppressUnmanagedCodeSecurity,
    Guid("2f69d622-20b5-41e9-afdf-89ced1dda04e"),
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IMFMediaEngineExtension
    {
        [PreserveSig]
        int CanPlayType(
            [MarshalAs(UnmanagedType.Bool)] bool AudioOnly,
            [MarshalAs(UnmanagedType.BStr)] string MimeType,
            out MF_MEDIA_ENGINE_CANPLAY pAnswer
            );

        [PreserveSig]
        int BeginCreateObject(
            [MarshalAs(UnmanagedType.BStr)] string bstrURL,
            IMFByteStream pByteStream,
            MFObjectType type,
            [MarshalAs(UnmanagedType.IUnknown)] out object ppIUnknownCancelCookie,
            IMFAsyncCallback pCallback,
            [In, MarshalAs(UnmanagedType.IUnknown)] object punkState
            );

        [PreserveSig]
        int CancelObjectCreation(
            [In, MarshalAs(UnmanagedType.IUnknown)] object pIUnknownCancelCookie
            );

        [PreserveSig]
        int EndCreateObject(
            IMFAsyncResult pResult,
            [MarshalAs(UnmanagedType.IUnknown)] out object ppObject
            );
    }

    [ComImport, System.Security.SuppressUnmanagedCodeSecurity,
    Guid("9f8021e8-9c8c-487e-bb5c-79aa4779938c"),
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IMFMediaEngineProtectedContent
    {
        [PreserveSig]
        int ShareResources(
            [In, MarshalAs(UnmanagedType.IUnknown)] object pUnkDeviceContext
            );

        [PreserveSig]
        int GetRequiredProtections(
            out int pFrameProtectionFlags
            );

        [PreserveSig]
        int SetOPMWindow(
            IntPtr hwnd
            );

        [PreserveSig]
        int TransferVideoFrame(
            [In, MarshalAs(UnmanagedType.IUnknown)] object pDstSurf,
            [In] MFVideoNormalizedRect pSrc,
            [In] MFRect pDst,
            [In] MFARGB pBorderClr,
            out int pFrameProtectionFlags
            );

        [PreserveSig]
        int SetContentProtectionManager(
            IMFContentProtectionManager pCPM
            );

        [PreserveSig]
        int SetApplicationCertificate(
            IntPtr pbBlob,
            int cbBlob
            );
    }

    [ComImport, System.Security.SuppressUnmanagedCodeSecurity,
    Guid("4D645ACE-26AA-4688-9BE1-DF3516990B93"),
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IMFMediaEngineClassFactory
    {
        [PreserveSig]
        int CreateInstance(
            MF_MEDIA_ENGINE_CREATEFLAGS dwFlags,
            IMFAttributes pAttr,
            out IMFMediaEngine ppPlayer
            );

        [PreserveSig]
        int CreateTimeRange(
            out IMFMediaTimeRange ppTimeRange
            );

        [PreserveSig]
        int CreateError(
            out IMFMediaError ppError
            );
    }

#endif

    #endregion

}
