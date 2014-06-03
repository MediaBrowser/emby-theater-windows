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
using System.Text;
using System.Runtime.InteropServices;

using MediaFoundation.Misc;
using System.Drawing;

namespace MediaFoundation
{
    #region Declarations

    [Flags, UnmanagedName("MFVideoFlags")]
    public enum MFVideoFlags : long
    {
        PAD_TO_Mask = 0x0001 | 0x0002,
        PAD_TO_None = 0 * 0x0001,
        PAD_TO_4x3 = 1 * 0x0001,
        PAD_TO_16x9 = 2 * 0x0001,
        SrcContentHintMask = 0x0004 | 0x0008 | 0x0010,
        SrcContentHintNone = 0 * 0x0004,
        SrcContentHint16x9 = 1 * 0x0004,
        SrcContentHint235_1 = 2 * 0x0004,
        AnalogProtected = 0x0020,
        DigitallyProtected = 0x0040,
        ProgressiveContent = 0x0080,
        FieldRepeatCountMask = 0x0100 | 0x0200 | 0x0400,
        FieldRepeatCountShift = 8,
        ProgressiveSeqReset = 0x0800,
        PanScanEnabled = 0x20000,
        LowerFieldFirst = 0x40000,
        BottomUpLinearRep = 0x80000,
        DXVASurface = 0x100000,
        RenderTargetSurface = 0x400000,
        ForceQWORD = 0x7FFFFFFF
    }

    [UnmanagedName("MFVideoChromaSubsampling")]
    public enum MFVideoChromaSubsampling
    {
        Cosited = 7,
        DV_PAL = 6,
        ForceDWORD = 0x7fffffff,
        Horizontally_Cosited = 4,
        Last = 8,
        MPEG1 = 1,
        MPEG2 = 5,
        ProgressiveChroma = 8,
        Unknown = 0,
        Vertically_AlignedChromaPlanes = 1,
        Vertically_Cosited = 2
    }

    [UnmanagedName("MFVideoInterlaceMode")]
    public enum MFVideoInterlaceMode
    {
        FieldInterleavedLowerFirst = 4,
        FieldInterleavedUpperFirst = 3,
        FieldSingleLower = 6,
        FieldSingleUpper = 5,
        ForceDWORD = 0x7fffffff,
        Last = 8,
        MixedInterlaceOrProgressive = 7,
        Progressive = 2,
        Unknown = 0
    }

    [UnmanagedName("MFVideoTransferFunction")]
    public enum MFVideoTransferFunction
    {
        Unknown = 0,
        Func10 = 1,
        Func18 = 2,
        Func20 = 3,
        Func22 = 4,
        Func240M = 6,
        Func28 = 8,
        Func709 = 5,
        ForceDWORD = 0x7fffffff,
        Last = 9,
        sRGB = 7,
        Log_100 = 9,
        Log_316 = 10,
        x709_sym = 11 // symmetric 709
    }

    [UnmanagedName("MFVideoPrimaries")]
    public enum MFVideoPrimaries
    {
        BT470_2_SysBG = 4,
        BT470_2_SysM = 3,
        BT709 = 2,
        EBU3213 = 7,
        ForceDWORD = 0x7fffffff,
        Last = 9,
        reserved = 1,
        SMPTE_C = 8,
        SMPTE170M = 5,
        SMPTE240M = 6,
        Unknown = 0
    }

    [UnmanagedName("MFVideoTransferMatrix")]
    public enum MFVideoTransferMatrix
    {
        BT601 = 2,
        BT709 = 1,
        ForceDWORD = 0x7fffffff,
        Last = 4,
        SMPTE240M = 3,
        Unknown = 0
    }

    [UnmanagedName("MFVideoLighting")]
    public enum MFVideoLighting
    {
        Bright = 1,
        Dark = 4,
        Dim = 3,
        ForceDWORD = 0x7fffffff,
        Last = 5,
        Office = 2,
        Unknown = 0
    }

    [UnmanagedName("MFNominalRange")]
    public enum MFNominalRange
    {
        MFNominalRange_Unknown = 0,
        MFNominalRange_Normal = 1,
        MFNominalRange_Wide = 2,

        MFNominalRange_0_255 = 1,
        MFNominalRange_16_235 = 2,
        MFNominalRange_48_208 = 3,
        MFNominalRange_64_127 = 4,

        MFNominalRange_Last,
        MFNominalRange_ForceDWORD = 0x7fffffff,
    }

    [Flags, UnmanagedName("MFBYTESTREAM_SEEK_FLAG_ defines")]
    public enum MFByteStreamSeekingFlags
    {
        None = 0,
        CancelPendingIO = 1
    }

    [UnmanagedName("MFBYTESTREAM_SEEK_ORIGIN")]
    public enum MFByteStreamSeekOrigin
    {
        Begin,
        Current
    }

    [Flags, UnmanagedName("MFBYTESTREAM_* defines")]
    public enum MFByteStreamCapabilities
    {
        None = 0x00000000,
        IsReadable = 0x00000001,
        IsWritable = 0x00000002,
        IsSeekable = 0x00000004,
        IsRemote = 0x00000008,
        IsDirectory = 0x00000080,
        HasSlowSeek = 0x00000100,
        IsPartiallyDownloaded = 0x00000200,
        ShareWrite = 0x00000400
    }

    [Flags, UnmanagedName("MF_MEDIATYPE_EQUAL_* defines")]
    public enum MFMediaEqual
    {
        None = 0,
        MajorTypes = 0x00000001,
        FormatTypes = 0x00000002,
        FormatData = 0x00000004,
        FormatUserData = 0x00000008
    }

    [UnmanagedName("MFASYNC_CALLBACK_QUEUE_ defines")]
    public enum MFAsyncCallbackQueue
    {
        Undefined = 0x00000000,
        Standard = 0x00000001,
        RT = 0x00000002,
        IO = 0x00000003,
        Timer = 0x00000004,
        QueueMultiThreaded = 0x00000005,
        LongFunction = 0x00000007,
        PrivateMask = unchecked((int)0xFFFF0000),
        All = unchecked((int)0xFFFFFFFF)
    }

    [Flags, UnmanagedName("MFASYNC_* defines")]
    public enum MFASync
    {
        None = 0,
        FastIOProcessingCallback = 0x00000001,
        SignalCallback = 0x00000002,
        BlockingCallback = 0x00000004,
        ReplyCallback = 0x00000008
    }

    [UnmanagedName("MF_ATTRIBUTES_MATCH_TYPE")]
    public enum MFAttributesMatchType
    {
        OurItems,
        TheirItems,
        AllItems,
        InterSection,
        Smaller
    }

    [Flags, UnmanagedName("MF_EVENT_FLAG_* defines")]
    public enum MFEventFlag
    {
        None = 0,
        NoWait = 0x00000001
    }

    [UnmanagedName("MF_ATTRIBUTE_TYPE")]
    public enum MFAttributeType
    {
        None = 0x0,
        Blob = 0x1011,
        Double = 0x5,
        Guid = 0x48,
        IUnknown = 13,
        String = 0x1f,
        Uint32 = 0x13,
        Uint64 = 0x15
    }

    [UnmanagedName("unnamed enum")]
    public enum MediaEventType
    {
        MEUnknown = 0,
        MEError = 1,
        MEExtendedType = 2,
        MENonFatalError = 3,
        MEGenericV1Anchor = MENonFatalError,
        MESessionUnknown = 100,
        MESessionTopologySet = 101,
        MESessionTopologiesCleared = 102,
        MESessionStarted = 103,
        MESessionPaused = 104,
        MESessionStopped = 105,
        MESessionClosed = 106,
        MESessionEnded = 107,
        MESessionRateChanged = 108,
        MESessionScrubSampleComplete = 109,
        MESessionCapabilitiesChanged = 110,
        MESessionTopologyStatus = 111,
        MESessionNotifyPresentationTime = 112,
        MENewPresentation = 113,
        MELicenseAcquisitionStart = 114,
        MELicenseAcquisitionCompleted = 115,
        MEIndividualizationStart = 116,
        MEIndividualizationCompleted = 117,
        MEEnablerProgress = 118,
        MEEnablerCompleted = 119,
        MEPolicyError = 120,
        MEPolicyReport = 121,
        MEBufferingStarted = 122,
        MEBufferingStopped = 123,
        MEConnectStart = 124,
        MEConnectEnd = 125,
        MEReconnectStart = 126,
        MEReconnectEnd = 127,
        MERendererEvent = 128,
        MESessionStreamSinkFormatChanged = 129,
        MESessionV1Anchor = MESessionStreamSinkFormatChanged,
        MESourceUnknown = 200,
        MESourceStarted = 201,
        MEStreamStarted = 202,
        MESourceSeeked = 203,
        MEStreamSeeked = 204,
        MENewStream = 205,
        MEUpdatedStream = 206,
        MESourceStopped = 207,
        MEStreamStopped = 208,
        MESourcePaused = 209,
        MEStreamPaused = 210,
        MEEndOfPresentation = 211,
        MEEndOfStream = 212,
        MEMediaSample = 213,
        MEStreamTick = 214,
        MEStreamThinMode = 215,
        MEStreamFormatChanged = 216,
        MESourceRateChanged = 217,
        MEEndOfPresentationSegment = 218,
        MESourceCharacteristicsChanged = 219,
        MESourceRateChangeRequested = 220,
        MESourceMetadataChanged = 221,
        MESequencerSourceTopologyUpdated = 222,
        MESourceV1Anchor = MESequencerSourceTopologyUpdated,

        MESinkUnknown = 300,
        MEStreamSinkStarted = 301,
        MEStreamSinkStopped = 302,
        MEStreamSinkPaused = 303,
        MEStreamSinkRateChanged = 304,
        MEStreamSinkRequestSample = 305,
        MEStreamSinkMarker = 306,
        MEStreamSinkPrerolled = 307,
        MEStreamSinkScrubSampleComplete = 308,
        MEStreamSinkFormatChanged = 309,
        MEStreamSinkDeviceChanged = 310,
        MEQualityNotify = 311,
        MESinkInvalidated = 312,
        MEAudioSessionNameChanged = 313,
        MEAudioSessionVolumeChanged = 314,
        MEAudioSessionDeviceRemoved = 315,
        MEAudioSessionServerShutdown = 316,
        MEAudioSessionGroupingParamChanged = 317,
        MEAudioSessionIconChanged = 318,
        MEAudioSessionFormatChanged = 319,
        MEAudioSessionDisconnected = 320,
        MEAudioSessionExclusiveModeOverride = 321,
        MESinkV1Anchor = MEAudioSessionExclusiveModeOverride,

        MECaptureAudioSessionVolumeChanged = 322,
        MECaptureAudioSessionDeviceRemoved = 323,
        MECaptureAudioSessionFormatChanged = 324,
        MECaptureAudioSessionDisconnected = 325,
        MECaptureAudioSessionExclusiveModeOverride = 326,
        MECaptureAudioSessionServerShutdown = 327,
        MESinkV2Anchor = MECaptureAudioSessionServerShutdown,

        METrustUnknown = 400,
        MEPolicyChanged = 401,
        MEContentProtectionMessage = 402,
        MEPolicySet = 403,
        METrustV1Anchor = MEPolicySet,

        MEWMDRMLicenseBackupCompleted = 500,
        MEWMDRMLicenseBackupProgress = 501,
        MEWMDRMLicenseRestoreCompleted = 502,
        MEWMDRMLicenseRestoreProgress = 503,
        MEWMDRMLicenseAcquisitionCompleted = 506,
        MEWMDRMIndividualizationCompleted = 508,
        MEWMDRMIndividualizationProgress = 513,
        MEWMDRMProximityCompleted = 514,
        MEWMDRMLicenseStoreCleaned = 515,
        MEWMDRMRevocationDownloadCompleted = 516,
        MEWMDRMV1Anchor = MEWMDRMRevocationDownloadCompleted,

        METransformUnknown = 600,
        METransformNeedInput,
        METransformHaveOutput,
        METransformDrainComplete,
        METransformMarker,
        MEByteStreamCharacteristicsChanged = 700,
        MEVideoCaptureDeviceRemoved = 800,
        MEVideoCaptureDevicePreempted = 801,
        MEReservedMax = 10000
    }

    [UnmanagedName("MF_Plugin_Type")]
    public enum MFPluginType
    {
        MFT = 0,
        MediaSource = 1
    }

    [StructLayout(LayoutKind.Sequential, Pack = 8), UnmanagedName("MFVideoCompressedInfo")]
    public struct MFVideoCompressedInfo
    {
        public long AvgBitrate;
        public long AvgBitErrorRate;
        public int MaxKeyFrameSpacing;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 4), UnmanagedName("MFVideoSurfaceInfo")]
    public struct MFVideoSurfaceInfo
    {
        public int Format;
        public int PaletteEntries;
        public MFPaletteEntry[] Palette;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 4), UnmanagedName("MFRatio")]
    public struct MFRatio
    {
        public int Numerator;
        public int Denominator;

        public MFRatio(int n, int d)
        {
            Numerator = n;
            Denominator = d;
        }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 4), UnmanagedName("MFVideoArea")]
    public class MFVideoArea
    {
        public MFOffset OffsetX;
        public MFOffset OffsetY;
        public MFSize Area;

        public MFVideoArea()
        {
            OffsetX = new MFOffset();
            OffsetY = new MFOffset();
        }

        public MFVideoArea(float x, float y, int width, int height)
        {
            OffsetX = new MFOffset(x);
            OffsetY = new MFOffset(y);
            Area = new MFSize(width, height);
        }

        public void MakeArea(float x, float y, int width, int height)
        {
            OffsetX.MakeOffset(x);
            OffsetY.MakeOffset(y);
            Area.Width = width;
            Area.Height = height;
        }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 2), UnmanagedName("MFOffset")]
    public class MFOffset
    {
        public short fract;
        public short Value;

        public MFOffset()
        {
        }

        public MFOffset(float v)
        {
            Value = (short)v;
            fract = (short)(65536 * (v - Value));
        }

        public void MakeOffset(float v)
        {
            Value = (short)v;
            fract = (short)(65536 * (v-Value));
        }

        public float GetOffset()
        {
            return ((float)Value) + (((float)fract) / 65536.0f);
        }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 8), UnmanagedName("MFVideoInfo")]
    public struct MFVideoInfo
    {
        public int dwWidth;
        public int dwHeight;
        public MFRatio PixelAspectRatio;
        public MFVideoChromaSubsampling SourceChromaSubsampling;
        public MFVideoInterlaceMode InterlaceMode;
        public MFVideoTransferFunction TransferFunction;
        public MFVideoPrimaries ColorPrimaries;
        public MFVideoTransferMatrix TransferMatrix;
        public MFVideoLighting SourceLighting;
        public MFRatio FramesPerSecond;
        public MFNominalRange NominalRange;
        public MFVideoArea GeometricAperture;
        public MFVideoArea MinimumDisplayAperture;
        public MFVideoArea PanScanAperture;
        public MFVideoFlags VideoFlags;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 8), UnmanagedName("MFVIDEOFORMAT")]
    public class MFVideoFormat
    {
        public int dwSize;
        public MFVideoInfo videoInfo;
        public Guid guidFormat;
        public MFVideoCompressedInfo compressedInfo;
        public MFVideoSurfaceInfo surfaceInfo;
    }

#if ALLOW_UNTESTED_INTERFACES

    [UnmanagedName("MF2DBuffer_LockFlags")]
    public enum MF2DBuffer_LockFlags
    {
        None,
        LockTypeMask = 0x1 | 0x2 | 0x3,
        Read = 0x1,
        Write = 0x2,
        ReadWrite = 0x3,

        ForceDWORD = 0x7FFFFFFF
    }

    [UnmanagedName("MF_Plugin_Type")]
    public enum MF_Plugin_Type
    {
        MFT = 0,
        MediaSource = 1,
        MFT_MatchOutputType = 2,
        Other = unchecked((int)0xffffffff),
    }

    [UnmanagedName("MF_PLUGIN_CONTROL_POLICY")]
    public enum MF_PLUGIN_CONTROL_POLICY
    {
        UseAllPlugins = 0,
        UseApprovedPlugins = 1,
        UseWebPlugins = 2
    }

#endif

    #endregion

    #region Interfaces

#if ALLOW_UNTESTED_INTERFACES

    [ComImport, System.Security.SuppressUnmanagedCodeSecurity,
    Guid("C6982083-3DDC-45CB-AF5E-0F7A8CE4DE77"),
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IMFPluginControl2 : IMFPluginControl
    {
        [PreserveSig]
        int SetPolicy(
            MF_PLUGIN_CONTROL_POLICY policy
        );
    }

    [ComImport, System.Security.SuppressUnmanagedCodeSecurity,
    Guid("eb533d5d-2db6-40f8-97a9-494692014f07"),
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IMFDXGIDeviceManager
    {
        [PreserveSig]
        int CloseDeviceHandle(
            IntPtr hDevice
        );

        [PreserveSig]
        int GetVideoService(
            IntPtr hDevice,
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid riid,
            [MarshalAs(UnmanagedType.IUnknown)] out object ppService
        );

        [PreserveSig]
        int LockDevice(
            IntPtr hDevice,
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid riid,
            [MarshalAs(UnmanagedType.IUnknown)] out object ppUnkDevice,
            [MarshalAs(UnmanagedType.Bool)] bool fBlock
        );

        [PreserveSig]
        int OpenDeviceHandle(
            out IntPtr phDevice
        );

        [PreserveSig]
        int ResetDevice(
            [MarshalAs(UnmanagedType.IUnknown)] object pUnkDevice,
            int resetToken
        );

        [PreserveSig]
        int TestDevice(
            IntPtr hDevice
        );

        [PreserveSig]
        int UnlockDevice(
            IntPtr hDevice,
            [MarshalAs(UnmanagedType.Bool)]  bool fSaveState
        );
    }

    [ComImport, System.Security.SuppressUnmanagedCodeSecurity,
    Guid("a6b43f84-5c0a-42e8-a44d-b1857a76992f"),
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IMFByteStreamProxyClassFactory
    {
        [PreserveSig]
        int CreateByteStreamProxy(
            IMFByteStream pByteStream,
            IMFAttributes pAttributes,
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid riid,
            [MarshalAs(UnmanagedType.IUnknown)] out object ppvObject
        );
    }

    [ComImport, System.Security.SuppressUnmanagedCodeSecurity,
    Guid("8feed468-6f7e-440d-869a-49bdd283ad0d"),
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IMFSampleOutputStream
    {
        [PreserveSig]
        int BeginWriteSample(
            IMFSample pSample,
            IMFAsyncCallback pCallback,
            [MarshalAs(UnmanagedType.IUnknown)] object punkState
        );

        [PreserveSig]
        int EndWriteSample(
            IMFAsyncResult pResult
        );

        [PreserveSig]
        int Close();
    }

    [ComImport, System.Security.SuppressUnmanagedCodeSecurity,
    Guid("33ae5ea6-4316-436f-8ddd-d73d22f829ec"),
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IMF2DBuffer2 : IMF2DBuffer
    {
        #region IMF2DBuffer Methods

        [PreserveSig]
        new int Lock2D(
            [Out] out IntPtr pbScanline0,
            out int plPitch
            );

        [PreserveSig]
        new int Unlock2D();

        [PreserveSig]
        new int GetScanline0AndPitch(
            out IntPtr pbScanline0,
            out int plPitch
            );

        [PreserveSig]
        new int IsContiguousFormat(
            [MarshalAs(UnmanagedType.Bool)] out bool pfIsContiguous
            );

        [PreserveSig]
        new int GetContiguousLength(
            out int pcbLength
            );

        [PreserveSig]
        new int ContiguousCopyTo(
            IntPtr pbDestBuffer,
            [In] int cbDestBuffer
            );

        [PreserveSig]
        new int ContiguousCopyFrom(
            [In] IntPtr pbSrcBuffer,
            [In] int cbSrcBuffer
            );

#endregion

        [PreserveSig]
        int Lock2DSize(
            MF2DBuffer_LockFlags lockFlags,
            out IntPtr ppbScanline0,
            out int plPitch,
            out IntPtr ppbBufferStart,
            out int pcbBufferLength
        );

        [PreserveSig]
        int Copy2DTo(
            IMF2DBuffer2 pDestBuffer
        );
    }

    [ComImport, System.Security.SuppressUnmanagedCodeSecurity,
    Guid("e7174cfa-1c9e-48b1-8866-626226bfc258"),
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IMFDXGIBuffer
    {
        [PreserveSig]
        int GetResource(
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid riid,
            [MarshalAs(UnmanagedType.IUnknown)] out object ppvObject
        );

        [PreserveSig]
        int GetSubresourceIndex(
            out int puSubresource
        );

        [PreserveSig]
        int GetUnknown(
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid guid,
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid riid,
            [MarshalAs(UnmanagedType.IUnknown)] out object ppvObject
        );

        [PreserveSig]
        int SetUnknown(
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid guid,
            [MarshalAs(UnmanagedType.IUnknown)] object pUnkData
        );
    }

    [ComImport, System.Security.SuppressUnmanagedCodeSecurity,
    Guid("c7a4dca1-f5f0-47b6-b92b-bf0106d25791"),
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IMFAsyncCallbackLogging : IMFAsyncCallback
    {
        #region IMFAsyncCallback

        [PreserveSig]
        new int GetParameters(
            out MFASync pdwFlags,
            out MFAsyncCallbackQueue pdwQueue
            );

        [PreserveSig]
        new int Invoke(
            [In, MarshalAs(UnmanagedType.Interface)] IMFAsyncResult pAsyncResult
            );

        #endregion

        [PreserveSig]
        IntPtr GetObjectPointer();

        [PreserveSig]
        int GetObjectTag();
    }

    [ComImport, System.Security.SuppressUnmanagedCodeSecurity,
    Guid("a27003d0-2354-4f2a-8d6a-ab7cff15437e"),
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IMFRemoteAsyncCallback
    {
        [PreserveSig]
        int Invoke(
            int hr,
            [In, MarshalAs(UnmanagedType.IUnknown)] object pRemoteResult
            );
    }

    [ComImport, System.Security.SuppressUnmanagedCodeSecurity,
    Guid("26A0ADC3-CE26-4672-9304-69552EDD3FAF"),
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown),
    Obsolete("To get the properties of the audio format, applications should use the media type attributes. If you need to convert the media type into a WAVEFORMATEX structure, call MFCreateWaveFormatExFromMFMediaType")]
    public interface IMFAudioMediaType : IMFMediaType
    {
        #region IMFAttributes methods

        [PreserveSig]
        new int GetItem(
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
            [In, Out, MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(PVMarshaler))] PropVariant pValue
            );

        [PreserveSig]
        new int GetItemType(
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
            out MFAttributeType pType
            );

        [PreserveSig]
        new int CompareItem(
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
            [In, MarshalAs(UnmanagedType.LPStruct)] ConstPropVariant Value,
            [MarshalAs(UnmanagedType.Bool)] out bool pbResult
            );

        [PreserveSig]
        new int Compare(
            [MarshalAs(UnmanagedType.Interface)] IMFAttributes pTheirs,
            MFAttributesMatchType MatchType,
            [MarshalAs(UnmanagedType.Bool)] out bool pbResult
            );

        [PreserveSig]
        new int GetUINT32(
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
            out int punValue
            );

        [PreserveSig]
        new int GetUINT64(
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
            out long punValue
            );

        [PreserveSig]
        new int GetDouble(
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
            out double pfValue
            );

        [PreserveSig]
        new int GetGUID(
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
            out Guid pguidValue
            );

        [PreserveSig]
        new int GetStringLength(
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
            out int pcchLength
            );

        [PreserveSig]
        new int GetString(
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
            [Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pwszValue,
            int cchBufSize,
            out int pcchLength
            );

        [PreserveSig]
        new int GetAllocatedString(
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
            [MarshalAs(UnmanagedType.LPWStr)] out string ppwszValue,
            out int pcchLength
            );

        [PreserveSig]
        new int GetBlobSize(
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
            out int pcbBlobSize
            );

        [PreserveSig]
        new int GetBlob(
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
            [Out, MarshalAs(UnmanagedType.LPArray)] byte[] pBuf,
            int cbBufSize,
            out int pcbBlobSize
            );

        // Use GetBlob instead of this
        [PreserveSig]
        new int GetAllocatedBlob(
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
            out IntPtr ip,  // Read w/Marshal.Copy, Free w/Marshal.FreeCoTaskMem
            out int pcbSize
            );

        [PreserveSig]
        new int GetUnknown(
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid riid,
            [MarshalAs(UnmanagedType.IUnknown)] out object ppv
            );

        [PreserveSig]
        new int SetItem(
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
            [In, MarshalAs(UnmanagedType.LPStruct)] ConstPropVariant Value
            );

        [PreserveSig]
        new int DeleteItem(
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid guidKey
            );

        [PreserveSig]
        new int DeleteAllItems();

        [PreserveSig]
        new int SetUINT32(
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
            int unValue
            );

        [PreserveSig]
        new int SetUINT64(
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
            long unValue
            );

        [PreserveSig]
        new int SetDouble(
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
            double fValue
            );

        [PreserveSig]
        new int SetGUID(
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid guidValue
            );

        [PreserveSig]
        new int SetString(
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
            [In, MarshalAs(UnmanagedType.LPWStr)] string wszValue
            );

        [PreserveSig]
        new int SetBlob(
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
            [In, MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 2)] byte[] pBuf,
            int cbBufSize
            );

        [PreserveSig]
        new int SetUnknown(
            [MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
            [In, MarshalAs(UnmanagedType.IUnknown)] object pUnknown
            );

        [PreserveSig]
        new int LockStore();

        [PreserveSig]
        new int UnlockStore();

        [PreserveSig]
        new int GetCount(
            out int pcItems
            );

        [PreserveSig]
        new int GetItemByIndex(
            int unIndex,
            out Guid pguidKey,
            [In, Out, MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(PVMarshaler))] PropVariant pValue
            );

        [PreserveSig]
        new int CopyAllItems(
            [In, MarshalAs(UnmanagedType.Interface)] IMFAttributes pDest
            );

        #endregion

        #region IMFMediaType methods

        [PreserveSig]
        new int GetMajorType(
            out Guid pguidMajorType
            );

        [PreserveSig]
        new int IsCompressedFormat(
            [MarshalAs(UnmanagedType.Bool)] out bool pfCompressed
            );

        [PreserveSig]
        new int IsEqual(
            [In, MarshalAs(UnmanagedType.Interface)] IMFMediaType pIMediaType,
            out MFMediaEqual pdwFlags
            );

        [PreserveSig]
        new int GetRepresentation(
            [In, MarshalAs(UnmanagedType.Struct)] Guid guidRepresentation,
            out IntPtr ppvRepresentation
            );

        [PreserveSig]
        new int FreeRepresentation(
            [In, MarshalAs(UnmanagedType.Struct)] Guid guidRepresentation,
            [In] IntPtr pvRepresentation
            );

        #endregion

        [PreserveSig, Obsolete("To get the properties of the audio format, applications should use the media type attributes. If you need to convert the media type into a WAVEFORMATEX structure, call MFCreateWaveFormatExFromMFMediaType")]
        IntPtr GetAudioFormat();
    }

    [ComImport, System.Security.SuppressUnmanagedCodeSecurity,
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown),
    Guid("5C6C44BF-1DB6-435B-9249-E8CD10FDEC96")]
    public interface IMFPluginControl
    {
        [PreserveSig]
        int GetPreferredClsid(
            MFPluginType pluginType,
            [MarshalAs(UnmanagedType.LPWStr)] string selector,
            out Guid clsid
        );

        [PreserveSig]
        int GetPreferredClsidByIndex(
            MFPluginType pluginType,
            int index,
            [MarshalAs(UnmanagedType.LPWStr)] out string selector,
            out Guid clsid
        );

        [PreserveSig]
        int SetPreferredClsid(
            MFPluginType pluginType,
            [MarshalAs(UnmanagedType.LPWStr)] string selector,
            [MarshalAs(UnmanagedType.LPStruct)] MFGuid clsid
        );

        [PreserveSig]
        int IsDisabled(
            MFPluginType pluginType,
            [MarshalAs(UnmanagedType.LPStruct)] Guid clsid
        );

        [PreserveSig]
        int GetDisabledByIndex(
            MFPluginType pluginType,
            int index,
            out Guid clsid
        );

        [PreserveSig]
        int SetDisabled(
            MFPluginType pluginType,
            [MarshalAs(UnmanagedType.LPStruct)] Guid clsid,
            [MarshalAs(UnmanagedType.Bool)] bool disabled
        );
    }

#endif

    [ComImport, System.Security.SuppressUnmanagedCodeSecurity,
    Guid("36F846FC-2256-48B6-B58E-E2B638316581"),
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IMFMediaEventQueue
    {
        [PreserveSig]
        int GetEvent(
            [In] MFEventFlag dwFlags,
            [MarshalAs(UnmanagedType.Interface)] out IMFMediaEvent ppEvent
            );

        [PreserveSig]
        int BeginGetEvent(
            [In, MarshalAs(UnmanagedType.Interface)] IMFAsyncCallback pCallback,
            [In, MarshalAs(UnmanagedType.IUnknown)] object pUnkState
            );

        [PreserveSig]
        int EndGetEvent(
            [In, MarshalAs(UnmanagedType.Interface)] IMFAsyncResult pResult,
            [MarshalAs(UnmanagedType.Interface)] out IMFMediaEvent ppEvent
            );

        [PreserveSig]
        int QueueEvent(
            [In, MarshalAs(UnmanagedType.Interface)] IMFMediaEvent pEvent
            );

        [PreserveSig]
        int QueueEventParamVar(
            [In] MediaEventType met,
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid guidExtendedType,
            [In, MarshalAs(UnmanagedType.Error)] int hrStatus,
            [In, MarshalAs(UnmanagedType.LPStruct)] ConstPropVariant pvValue
            );

        [PreserveSig]
        int QueueEventParamUnk(
            [In] MediaEventType met,
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid guidExtendedType,
            [In, MarshalAs(UnmanagedType.Error)] int hrStatus,
            [In, MarshalAs(UnmanagedType.IUnknown)] object pUnk
            );

        [PreserveSig]
        int Shutdown();
    }

    [ComImport, System.Security.SuppressUnmanagedCodeSecurity,
    Guid("DF598932-F10C-4E39-BBA2-C308F101DAA3"),
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IMFMediaEvent : IMFAttributes
    {
        #region IMFAttributes methods

        [PreserveSig]
        new int GetItem(
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
            [In, Out, MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(PVMarshaler))] PropVariant pValue
            );

        [PreserveSig]
        new int GetItemType(
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
            out MFAttributeType pType
            );

        [PreserveSig]
        new int CompareItem(
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
            [In, MarshalAs(UnmanagedType.LPStruct)] ConstPropVariant Value,
            [MarshalAs(UnmanagedType.Bool)] out bool pbResult
            );

        [PreserveSig]
        new int Compare(
            [MarshalAs(UnmanagedType.Interface)] IMFAttributes pTheirs,
            MFAttributesMatchType MatchType,
            [MarshalAs(UnmanagedType.Bool)] out bool pbResult
            );

        [PreserveSig]
        new int GetUINT32(
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
            out int punValue
            );

        [PreserveSig]
        new int GetUINT64(
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
            out long punValue
            );

        [PreserveSig]
        new int GetDouble(
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
            out double pfValue
            );

        [PreserveSig]
        new int GetGUID(
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
            out Guid pguidValue
            );

        [PreserveSig]
        new int GetStringLength(
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
            out int pcchLength
            );

        [PreserveSig]
        new int GetString(
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
            [Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pwszValue,
            int cchBufSize,
            out int pcchLength
            );

        [PreserveSig]
        new int GetAllocatedString(
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
            [MarshalAs(UnmanagedType.LPWStr)] out string ppwszValue,
            out int pcchLength
            );

        [PreserveSig]
        new int GetBlobSize(
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
            out int pcbBlobSize
            );

        [PreserveSig]
        new int GetBlob(
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
            [Out, MarshalAs(UnmanagedType.LPArray)] byte[] pBuf,
            int cbBufSize,
            out int pcbBlobSize
            );

        // Use GetBlob instead of this
        [PreserveSig]
        new int GetAllocatedBlob(
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
            out IntPtr ip,  // Read w/Marshal.Copy, Free w/Marshal.FreeCoTaskMem
            out int pcbSize
            );

        [PreserveSig]
        new int GetUnknown(
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid riid,
            [MarshalAs(UnmanagedType.IUnknown)] out object ppv
            );

        [PreserveSig]
        new int SetItem(
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
            [In, MarshalAs(UnmanagedType.LPStruct)] ConstPropVariant Value
            );

        [PreserveSig]
        new int DeleteItem(
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid guidKey
            );

        [PreserveSig]
        new int DeleteAllItems();

        [PreserveSig]
        new int SetUINT32(
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
            int unValue
            );

        [PreserveSig]
        new int SetUINT64(
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
            long unValue
            );

        [PreserveSig]
        new int SetDouble(
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
            double fValue
            );

        [PreserveSig]
        new int SetGUID(
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid guidValue
            );

        [PreserveSig]
        new int SetString(
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
            [In, MarshalAs(UnmanagedType.LPWStr)] string wszValue
            );

        [PreserveSig]
        new int SetBlob(
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
            [In, MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 2)] byte[] pBuf,
            int cbBufSize
            );

        [PreserveSig]
        new int SetUnknown(
            [MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
            [In, MarshalAs(UnmanagedType.IUnknown)] object pUnknown
            );

        [PreserveSig]
        new int LockStore();

        [PreserveSig]
        new int UnlockStore();

        [PreserveSig]
        new int GetCount(
            out int pcItems
            );

        [PreserveSig]
        new int GetItemByIndex(
            int unIndex,
            out Guid pguidKey,
            [In, Out, MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(PVMarshaler))] PropVariant pValue
            );

        [PreserveSig]
        new int CopyAllItems(
            [In, MarshalAs(UnmanagedType.Interface)] IMFAttributes pDest
            );

        #endregion

        [PreserveSig]
        int GetType(
            out MediaEventType pmet
            );

        [PreserveSig]
        int GetExtendedType(
            out Guid pguidExtendedType
            );

        [PreserveSig]
        int GetStatus(
            [MarshalAs(UnmanagedType.Error)] out int phrStatus
            );

        [PreserveSig]
        int GetValue(
            [In, Out, MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(PVMarshaler))] PropVariant pvValue
            );
    }

    [ComImport, System.Security.SuppressUnmanagedCodeSecurity,
    Guid("2CD2D921-C447-44A7-A13C-4ADABFC247E3"),
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IMFAttributes
    {
        [PreserveSig]
        int GetItem(
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
            [In, Out, MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef=typeof(PVMarshaler))] PropVariant pValue
            );

        [PreserveSig]
        int GetItemType(
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
            out MFAttributeType pType
            );

        [PreserveSig]
        int CompareItem(
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
            [In, MarshalAs(UnmanagedType.LPStruct)] ConstPropVariant Value,
            [MarshalAs(UnmanagedType.Bool)] out bool pbResult
            );

        [PreserveSig]
        int Compare(
            [MarshalAs(UnmanagedType.Interface)] IMFAttributes pTheirs,
            MFAttributesMatchType MatchType,
            [MarshalAs(UnmanagedType.Bool)] out bool pbResult
            );

        [PreserveSig]
        int GetUINT32(
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
            out int punValue
            );

        [PreserveSig]
        int GetUINT64(
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
            out long punValue
            );

        [PreserveSig]
        int GetDouble(
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
            out double pfValue
            );

        [PreserveSig]
        int GetGUID(
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
            out Guid pguidValue
            );

        [PreserveSig]
        int GetStringLength(
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
            out int pcchLength
            );

        [PreserveSig]
        int GetString(
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
            [Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pwszValue,
            int cchBufSize,
            out int pcchLength
            );

        [PreserveSig]
        int GetAllocatedString(
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
            [MarshalAs(UnmanagedType.LPWStr)] out string ppwszValue,
            out int pcchLength
            );

        [PreserveSig]
        int GetBlobSize(
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
            out int pcbBlobSize
            );

        [PreserveSig]
        int GetBlob(
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
            [Out, MarshalAs(UnmanagedType.LPArray)] byte[] pBuf,
            int cbBufSize,
            out int pcbBlobSize
            );

        // Use GetBlob instead of this
        [PreserveSig]
        int GetAllocatedBlob(
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
            out IntPtr ip,  // Read w/Marshal.Copy, Free w/Marshal.FreeCoTaskMem
            out int pcbSize
            );

        [PreserveSig]
        int GetUnknown(
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid riid,
            [MarshalAs(UnmanagedType.IUnknown)] out object ppv
            );

        [PreserveSig]
        int SetItem(
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
            [In, MarshalAs(UnmanagedType.LPStruct)] ConstPropVariant Value
            );

        [PreserveSig]
        int DeleteItem(
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid guidKey
            );

        [PreserveSig]
        int DeleteAllItems();

        [PreserveSig]
        int SetUINT32(
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
            int unValue
            );

        [PreserveSig]
        int SetUINT64(
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
            long unValue
            );

        [PreserveSig]
        int SetDouble(
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
            double fValue
            );

        [PreserveSig]
        int SetGUID(
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid guidValue
            );

        [PreserveSig]
        int SetString(
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
            [In, MarshalAs(UnmanagedType.LPWStr)] string wszValue
            );

        [PreserveSig]
        int SetBlob(
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
            [In, MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 2)] byte[] pBuf,
            int cbBufSize
            );

        [PreserveSig]
        int SetUnknown(
            [MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
            [In, MarshalAs(UnmanagedType.IUnknown)] object pUnknown
            );

        [PreserveSig]
        int LockStore();

        [PreserveSig]
        int UnlockStore();

        [PreserveSig]
        int GetCount(
            out int pcItems
            );

        [PreserveSig]
        int GetItemByIndex(
            int unIndex,
            out Guid pguidKey,
            [In, Out, MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef=typeof(PVMarshaler))] PropVariant pValue
            );

        [PreserveSig]
        int CopyAllItems(
            [In, MarshalAs(UnmanagedType.Interface)] IMFAttributes pDest
            );
    }

    [ComImport, System.Security.SuppressUnmanagedCodeSecurity,
    Guid("2CD0BD52-BCD5-4B89-B62C-EADC0C031E7D"),
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IMFMediaEventGenerator
    {
        [PreserveSig]
        int GetEvent(
            [In] MFEventFlag dwFlags,
            [MarshalAs(UnmanagedType.Interface)] out IMFMediaEvent ppEvent
            );

        [PreserveSig]
        int BeginGetEvent(
            [In, MarshalAs(UnmanagedType.Interface)] IMFAsyncCallback pCallback,

            [In, MarshalAs(UnmanagedType.IUnknown)] object o
            );

        [PreserveSig]
        int EndGetEvent(
            IMFAsyncResult pResult,

            out IMFMediaEvent ppEvent
            );

        [PreserveSig]
        int QueueEvent(
            [In] MediaEventType met,
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid guidExtendedType,
            [In] int hrStatus,
            [In, MarshalAs(UnmanagedType.LPStruct)] ConstPropVariant pvValue
            );
    }

    [ComImport, System.Security.SuppressUnmanagedCodeSecurity,
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown),
    Guid("AC6B7889-0740-4D51-8619-905994A55CC6")]
    public interface IMFAsyncResult
    {
        [PreserveSig]
        int GetState(
            [MarshalAs(UnmanagedType.IUnknown)] out object ppunkState
            );

        [PreserveSig]
        int GetStatus();

        [PreserveSig]
        int SetStatus(
            [In, MarshalAs(UnmanagedType.Error)] int hrStatus
            );

        [PreserveSig]
        int GetObject(
            [MarshalAs(UnmanagedType.Interface)] out object ppObject
            );

        [PreserveSig]
        IntPtr GetStateNoAddRef();
    }

    [ComImport, System.Security.SuppressUnmanagedCodeSecurity,
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown),
    Guid("A27003CF-2354-4F2A-8D6A-AB7CFF15437E")]
    public interface IMFAsyncCallback
    {
        [PreserveSig]
        int GetParameters(
            out MFASync pdwFlags,
            out MFAsyncCallbackQueue pdwQueue
            );

        [PreserveSig]
        int Invoke(
            [In, MarshalAs(UnmanagedType.Interface)] IMFAsyncResult pAsyncResult
            );
    }

    [ComImport, System.Security.SuppressUnmanagedCodeSecurity,
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown),
    Guid("44AE0FA8-EA31-4109-8D2E-4CAE4997C555")]
    public interface IMFMediaType : IMFAttributes
    {
        #region IMFAttributes methods

        [PreserveSig]
        new int GetItem(
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
            [In, Out, MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(PVMarshaler))] PropVariant pValue
            );

        [PreserveSig]
        new int GetItemType(
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
            out MFAttributeType pType
            );

        [PreserveSig]
        new int CompareItem(
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
            [In, MarshalAs(UnmanagedType.LPStruct)] ConstPropVariant Value,
            [MarshalAs(UnmanagedType.Bool)] out bool pbResult
            );

        [PreserveSig]
        new int Compare(
            [MarshalAs(UnmanagedType.Interface)] IMFAttributes pTheirs,
            MFAttributesMatchType MatchType,
            [MarshalAs(UnmanagedType.Bool)] out bool pbResult
            );

        [PreserveSig]
        new int GetUINT32(
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
            out int punValue
            );

        [PreserveSig]
        new int GetUINT64(
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
            out long punValue
            );

        [PreserveSig]
        new int GetDouble(
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
            out double pfValue
            );

        [PreserveSig]
        new int GetGUID(
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
            out Guid pguidValue
            );

        [PreserveSig]
        new int GetStringLength(
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
            out int pcchLength
            );

        [PreserveSig]
        new int GetString(
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
            [Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pwszValue,
            int cchBufSize,
            out int pcchLength
            );

        [PreserveSig]
        new int GetAllocatedString(
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
            [MarshalAs(UnmanagedType.LPWStr)] out string ppwszValue,
            out int pcchLength
            );

        [PreserveSig]
        new int GetBlobSize(
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
            out int pcbBlobSize
            );

        [PreserveSig]
        new int GetBlob(
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
            [Out, MarshalAs(UnmanagedType.LPArray)] byte[] pBuf,
            int cbBufSize,
            out int pcbBlobSize
            );

        // Use GetBlob instead of this
        [PreserveSig]
        new int GetAllocatedBlob(
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
            out IntPtr ip,  // Read w/Marshal.Copy, Free w/Marshal.FreeCoTaskMem
            out int pcbSize
            );

        [PreserveSig]
        new int GetUnknown(
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid riid,
            [MarshalAs(UnmanagedType.IUnknown)] out object ppv
            );

        [PreserveSig]
        new int SetItem(
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
            [In, MarshalAs(UnmanagedType.LPStruct)] ConstPropVariant Value
            );

        [PreserveSig]
        new int DeleteItem(
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid guidKey
            );

        [PreserveSig]
        new int DeleteAllItems();

        [PreserveSig]
        new int SetUINT32(
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
            int unValue
            );

        [PreserveSig]
        new int SetUINT64(
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
            long unValue
            );

        [PreserveSig]
        new int SetDouble(
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
            double fValue
            );

        [PreserveSig]
        new int SetGUID(
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid guidValue
            );

        [PreserveSig]
        new int SetString(
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
            [In, MarshalAs(UnmanagedType.LPWStr)] string wszValue
            );

        [PreserveSig]
        new int SetBlob(
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
            [In, MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 2)] byte[] pBuf,
            int cbBufSize
            );

        [PreserveSig]
        new int SetUnknown(
            [MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
            [In, MarshalAs(UnmanagedType.IUnknown)] object pUnknown
            );

        [PreserveSig]
        new int LockStore();

        [PreserveSig]
        new int UnlockStore();

        [PreserveSig]
        new int GetCount(
            out int pcItems
            );

        [PreserveSig]
        new int GetItemByIndex(
            int unIndex,
            out Guid pguidKey,
            [In, Out, MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(PVMarshaler))] PropVariant pValue
            );

        [PreserveSig]
        new int CopyAllItems(
            [In, MarshalAs(UnmanagedType.Interface)] IMFAttributes pDest
            );

        #endregion

        [PreserveSig]
        int GetMajorType(
            out Guid pguidMajorType
            );

        [PreserveSig]
        int IsCompressedFormat(
            [MarshalAs(UnmanagedType.Bool)] out bool pfCompressed
            );

        [PreserveSig]
        int IsEqual(
            [In, MarshalAs(UnmanagedType.Interface)] IMFMediaType pIMediaType,
            out MFMediaEqual pdwFlags
            );

        [PreserveSig]
        int GetRepresentation(
            [In, MarshalAs(UnmanagedType.Struct)] Guid guidRepresentation,
            out IntPtr ppvRepresentation
            );

        [PreserveSig]
        int FreeRepresentation(
            [In, MarshalAs(UnmanagedType.Struct)] Guid guidRepresentation,
            [In] IntPtr pvRepresentation
            );
    }

    [ComImport, System.Security.SuppressUnmanagedCodeSecurity,
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown),
    Guid("5BC8A76B-869A-46A3-9B03-FA218A66AEBE")]
    public interface IMFCollection
    {
        [PreserveSig]
        int GetElementCount(
            out int pcElements
            );

        [PreserveSig]
        int GetElement(
            [In] int dwElementIndex,
            [MarshalAs(UnmanagedType.IUnknown)] out object ppUnkElement
            );

        [PreserveSig]
        int AddElement(
            [In, MarshalAs(UnmanagedType.IUnknown)] object pUnkElement
            );

        [PreserveSig]
        int RemoveElement(
            [In] int dwElementIndex,
            [MarshalAs(UnmanagedType.IUnknown)] out object ppUnkElement
            );

        [PreserveSig]
        int InsertElementAt(
            [In] int dwIndex,
            [In, MarshalAs(UnmanagedType.IUnknown)] object pUnknown
            );

        [PreserveSig]
        int RemoveAllElements();
    }

    [ComImport, System.Security.SuppressUnmanagedCodeSecurity,
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown),
    Guid("AD4C1B00-4BF7-422F-9175-756693D9130D")]
    public interface IMFByteStream
    {
        [PreserveSig]
        int GetCapabilities(
            out MFByteStreamCapabilities pdwCapabilities
            );

        [PreserveSig]
        int GetLength(
            out long pqwLength
            );

        [PreserveSig]
        int SetLength(
            [In] long qwLength
            );

        [PreserveSig]
        int GetCurrentPosition(
            out long pqwPosition
            );

        [PreserveSig]
        int SetCurrentPosition(
            [In] long qwPosition
            );

        [PreserveSig]
        int IsEndOfStream(
            [MarshalAs(UnmanagedType.Bool)] out bool pfEndOfStream
            );

        [PreserveSig]
        int Read(
            IntPtr pb,
            [In] int cb,
            out int pcbRead
            );

        [PreserveSig]
        int BeginRead(
            IntPtr pb,
            [In] int cb,
            [In, MarshalAs(UnmanagedType.Interface)] IMFAsyncCallback pCallback,
            [In, MarshalAs(UnmanagedType.IUnknown)] object pUnkState
            );

        [PreserveSig]
        int EndRead(
            [In, MarshalAs(UnmanagedType.Interface)] IMFAsyncResult pResult,
            out int pcbRead
            );

        [PreserveSig]
        int Write(
            IntPtr pb,
            [In] int cb,
            out int pcbWritten
            );

        [PreserveSig]
        int BeginWrite(
            IntPtr pb,
            [In] int cb,
            [In, MarshalAs(UnmanagedType.Interface)] IMFAsyncCallback pCallback,
            [In, MarshalAs(UnmanagedType.IUnknown)] object pUnkState
            );

        [PreserveSig]
        int EndWrite(
            [In, MarshalAs(UnmanagedType.Interface)] IMFAsyncResult pResult,
            out int pcbWritten
            );

        [PreserveSig]
        int Seek(
            [In] MFByteStreamSeekOrigin SeekOrigin,
            [In] long llSeekOffset,
            [In] MFByteStreamSeekingFlags dwSeekFlags,
            out long pqwCurrentPosition
            );

        [PreserveSig]
        int Flush();

        [PreserveSig]
        int Close();
    }

    [ComImport, System.Security.SuppressUnmanagedCodeSecurity,
    Guid("C40A00F2-B93A-4D80-AE8C-5A1C634F58E4"),
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IMFSample : IMFAttributes
    {
        #region IMFAttributes methods

        [PreserveSig]
        new int GetItem(
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
            [In, Out, MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(PVMarshaler))] PropVariant pValue
            );

        [PreserveSig]
        new int GetItemType(
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
            out MFAttributeType pType
            );

        [PreserveSig]
        new int CompareItem(
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
            [In, MarshalAs(UnmanagedType.LPStruct)] ConstPropVariant Value,
            [MarshalAs(UnmanagedType.Bool)] out bool pbResult
            );

        [PreserveSig]
        new int Compare(
            [MarshalAs(UnmanagedType.Interface)] IMFAttributes pTheirs,
            MFAttributesMatchType MatchType,
            [MarshalAs(UnmanagedType.Bool)] out bool pbResult
            );

        [PreserveSig]
        new int GetUINT32(
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
            out int punValue
            );

        [PreserveSig]
        new int GetUINT64(
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
            out long punValue
            );

        [PreserveSig]
        new int GetDouble(
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
            out double pfValue
            );

        [PreserveSig]
        new int GetGUID(
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
            out Guid pguidValue
            );

        [PreserveSig]
        new int GetStringLength(
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
            out int pcchLength
            );

        [PreserveSig]
        new int GetString(
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
            [Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pwszValue,
            int cchBufSize,
            out int pcchLength
            );

        [PreserveSig]
        new int GetAllocatedString(
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
            [MarshalAs(UnmanagedType.LPWStr)] out string ppwszValue,
            out int pcchLength
            );

        [PreserveSig]
        new int GetBlobSize(
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
            out int pcbBlobSize
            );

        [PreserveSig]
        new int GetBlob(
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
            [Out, MarshalAs(UnmanagedType.LPArray)] byte[] pBuf,
            int cbBufSize,
            out int pcbBlobSize
            );

        // Use GetBlob instead of this
        [PreserveSig]
        new int GetAllocatedBlob(
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
            out IntPtr ip,  // Read w/Marshal.Copy, Free w/Marshal.FreeCoTaskMem
            out int pcbSize
            );

        [PreserveSig]
        new int GetUnknown(
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid riid,
            [MarshalAs(UnmanagedType.IUnknown)] out object ppv
            );

        [PreserveSig]
        new int SetItem(
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
            [In, MarshalAs(UnmanagedType.LPStruct)] ConstPropVariant Value
            );

        [PreserveSig]
        new int DeleteItem(
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid guidKey
            );

        [PreserveSig]
        new int DeleteAllItems();

        [PreserveSig]
        new int SetUINT32(
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
            int unValue
            );

        [PreserveSig]
        new int SetUINT64(
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
            long unValue
            );

        [PreserveSig]
        new int SetDouble(
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
            double fValue
            );

        [PreserveSig]
        new int SetGUID(
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid guidValue
            );

        [PreserveSig]
        new int SetString(
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
            [In, MarshalAs(UnmanagedType.LPWStr)] string wszValue
            );

        [PreserveSig]
        new int SetBlob(
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
            [In, MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 2)] byte[] pBuf,
            int cbBufSize
            );

        [PreserveSig]
        new int SetUnknown(
            [MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
            [In, MarshalAs(UnmanagedType.IUnknown)] object pUnknown
            );

        [PreserveSig]
        new int LockStore();

        [PreserveSig]
        new int UnlockStore();

        [PreserveSig]
        new int GetCount(
            out int pcItems
            );

        [PreserveSig]
        new int GetItemByIndex(
            int unIndex,
            out Guid pguidKey,
            [In, Out, MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(PVMarshaler))] PropVariant pValue
            );

        [PreserveSig]
        new int CopyAllItems(
            [In, MarshalAs(UnmanagedType.Interface)] IMFAttributes pDest
            );

        #endregion

        [PreserveSig]
        int GetSampleFlags(
            out int pdwSampleFlags // Must be zero
            );

        [PreserveSig]
        int SetSampleFlags(
            [In] int dwSampleFlags // Must be zero
            );

        [PreserveSig]
        int GetSampleTime(
            out long phnsSampleTime
            );

        [PreserveSig]
        int SetSampleTime(
            [In] long hnsSampleTime
            );

        [PreserveSig]
        int GetSampleDuration(
            out long phnsSampleDuration
            );

        [PreserveSig]
        int SetSampleDuration(
            [In] long hnsSampleDuration
            );

        [PreserveSig]
        int GetBufferCount(
            out int pdwBufferCount
            );

        [PreserveSig]
        int GetBufferByIndex(
            [In] int dwIndex,
            [MarshalAs(UnmanagedType.Interface)] out IMFMediaBuffer ppBuffer
            );

        [PreserveSig]
        int ConvertToContiguousBuffer(
            [MarshalAs(UnmanagedType.Interface)] out IMFMediaBuffer ppBuffer
            );

        [PreserveSig]
        int AddBuffer(
            [In, MarshalAs(UnmanagedType.Interface)] IMFMediaBuffer pBuffer
            );

        [PreserveSig]
        int RemoveBufferByIndex(
            [In] int dwIndex
            );

        [PreserveSig]
        int RemoveAllBuffers();

        [PreserveSig]
        int GetTotalLength(
            out int pcbTotalLength
            );

        [PreserveSig]
        int CopyToBuffer(
            [In, MarshalAs(UnmanagedType.Interface)] IMFMediaBuffer pBuffer
            );
    }

    [ComImport, System.Security.SuppressUnmanagedCodeSecurity,
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown),
    Guid("045FA593-8799-42B8-BC8D-8968C6453507")]
    public interface IMFMediaBuffer
    {
        [PreserveSig]
        int Lock(
            out IntPtr ppbBuffer,
            out int pcbMaxLength,
            out int pcbCurrentLength
            );

        [PreserveSig]
        int Unlock();

        [PreserveSig]
        int GetCurrentLength(
            out int pcbCurrentLength
            );

        [PreserveSig]
        int SetCurrentLength(
            [In] int cbCurrentLength
            );

        [PreserveSig]
        int GetMaxLength(
            out int pcbMaxLength
            );
    }

    [ComImport, System.Security.SuppressUnmanagedCodeSecurity,
    Guid("7DC9D5F9-9ED9-44EC-9BBF-0600BB589FBB"),
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IMF2DBuffer
    {
        [PreserveSig]
        int Lock2D(
            [Out] out IntPtr pbScanline0,
            out int plPitch
            );

        [PreserveSig]
        int Unlock2D();

        [PreserveSig]
        int GetScanline0AndPitch(
            out IntPtr pbScanline0,
            out int plPitch
            );

        [PreserveSig]
        int IsContiguousFormat(
            [MarshalAs(UnmanagedType.Bool)] out bool pfIsContiguous
            );

        [PreserveSig]
        int GetContiguousLength(
            out int pcbLength
            );

        [PreserveSig]
        int ContiguousCopyTo(
            IntPtr pbDestBuffer,
            [In] int cbDestBuffer
            );

        [PreserveSig]
        int ContiguousCopyFrom(
            [In] IntPtr pbSrcBuffer,
            [In] int cbSrcBuffer
            );
    }

    [ComImport, System.Security.SuppressUnmanagedCodeSecurity,
    Guid("B99F381F-A8F9-47A2-A5AF-CA3A225A3890"),
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IMFVideoMediaType : IMFMediaType
    {
        #region IMFAttributes methods

        [PreserveSig]
        new int GetItem(
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
            [In, Out, MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(PVMarshaler))] PropVariant pValue
            );

        [PreserveSig]
        new int GetItemType(
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
            out MFAttributeType pType
            );

        [PreserveSig]
        new int CompareItem(
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
            [In, MarshalAs(UnmanagedType.LPStruct)] ConstPropVariant Value,
            [MarshalAs(UnmanagedType.Bool)] out bool pbResult
            );

        [PreserveSig]
        new int Compare(
            [MarshalAs(UnmanagedType.Interface)] IMFAttributes pTheirs,
            MFAttributesMatchType MatchType,
            [MarshalAs(UnmanagedType.Bool)] out bool pbResult
            );

        [PreserveSig]
        new int GetUINT32(
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
            out int punValue
            );

        [PreserveSig]
        new int GetUINT64(
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
            out long punValue
            );

        [PreserveSig]
        new int GetDouble(
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
            out double pfValue
            );

        [PreserveSig]
        new int GetGUID(
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
            out Guid pguidValue
            );

        [PreserveSig]
        new int GetStringLength(
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
            out int pcchLength
            );

        [PreserveSig]
        new int GetString(
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
            [Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pwszValue,
            int cchBufSize,
            out int pcchLength
            );

        [PreserveSig]
        new int GetAllocatedString(
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
            [MarshalAs(UnmanagedType.LPWStr)] out string ppwszValue,
            out int pcchLength
            );

        [PreserveSig]
        new int GetBlobSize(
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
            out int pcbBlobSize
            );

        [PreserveSig]
        new int GetBlob(
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
            [Out, MarshalAs(UnmanagedType.LPArray)] byte[] pBuf,
            int cbBufSize,
            out int pcbBlobSize
            );

        // Use GetBlob instead of this
        [PreserveSig]
        new int GetAllocatedBlob(
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
            out IntPtr ip,  // Read w/Marshal.Copy, Free w/Marshal.FreeCoTaskMem
            out int pcbSize
            );

        [PreserveSig]
        new int GetUnknown(
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid riid,
            [MarshalAs(UnmanagedType.IUnknown)] out object ppv
            );

        [PreserveSig]
        new int SetItem(
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
            [In, MarshalAs(UnmanagedType.LPStruct)] ConstPropVariant Value
            );

        [PreserveSig]
        new int DeleteItem(
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid guidKey
            );

        [PreserveSig]
        new int DeleteAllItems();

        [PreserveSig]
        new int SetUINT32(
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
            int unValue
            );

        [PreserveSig]
        new int SetUINT64(
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
            long unValue
            );

        [PreserveSig]
        new int SetDouble(
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
            double fValue
            );

        [PreserveSig]
        new int SetGUID(
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid guidValue
            );

        [PreserveSig]
        new int SetString(
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
            [In, MarshalAs(UnmanagedType.LPWStr)] string wszValue
            );

        [PreserveSig]
        new int SetBlob(
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
            [In, MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 2)] byte[] pBuf,
            int cbBufSize
            );

        [PreserveSig]
        new int SetUnknown(
            [MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
            [In, MarshalAs(UnmanagedType.IUnknown)] object pUnknown
            );

        [PreserveSig]
        new int LockStore();

        [PreserveSig]
        new int UnlockStore();

        [PreserveSig]
        new int GetCount(
            out int pcItems
            );

        [PreserveSig]
        new int GetItemByIndex(
            int unIndex,
            out Guid pguidKey,
            [In, Out, MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(PVMarshaler))] PropVariant pValue
            );

        [PreserveSig]
        new int CopyAllItems(
            [In, MarshalAs(UnmanagedType.Interface)] IMFAttributes pDest
            );

        #endregion

        #region IMFMediaType methods

        [PreserveSig]
        new int GetMajorType(
            out Guid pguidMajorType
            );

        [PreserveSig]
        new int IsCompressedFormat(
            [MarshalAs(UnmanagedType.Bool)] out bool pfCompressed
            );

        [PreserveSig]
        new int IsEqual(
            [In, MarshalAs(UnmanagedType.Interface)] IMFMediaType pIMediaType,
            out MFMediaEqual pdwFlags
            );

        [PreserveSig]
        new int GetRepresentation(
            [In, MarshalAs(UnmanagedType.Struct)] Guid guidRepresentation,
            out IntPtr ppvRepresentation
            );

        [PreserveSig]
        new int FreeRepresentation(
            [In, MarshalAs(UnmanagedType.Struct)] Guid guidRepresentation,
            [In] IntPtr pvRepresentation
            );

        #endregion

        [PreserveSig, Obsolete("This method is deprecated by MS")]
        MFVideoFormat GetVideoFormat();

        [Obsolete("This method is deprecated by MS")]
        [PreserveSig]
        int GetVideoRepresentation(
            [In, MarshalAs(UnmanagedType.Struct)] Guid guidRepresentation,
            out IntPtr ppvRepresentation,
            [In] int lStride
            );
    }

    #endregion
}
