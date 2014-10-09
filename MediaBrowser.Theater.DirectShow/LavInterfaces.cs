using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;
using System.Threading.Tasks;

namespace MediaBrowser.Theater.DirectShow
{
    #region Video

    public enum LAVVideoCodec
    {
        H264,
        VC1,
        MPEG1,
        MPEG2,
        MPEG4,
        MSMPEG4,
        VP8,
        WMV3,
        WMV12,
        MJPEG,
        Theora,
        FLV1,
        VP6,
        SVQ,
        H261,
        H263,
        Indeo,
        TSCC,
        Fraps,
        HuffYUV,
        QTRle,
        DV,
        Bink,
        Smacker,
        RV12,
        RV34,
        Lagarith,
        Cinepak,
        Camstudio,
        QPEG,
        ZLIB,
        QTRpza,
        PNG,
        MSRLE,
        ProRes,
        UtVideo,
        Dirac,
        DNxHD,
        MSVideo1,
        EightBPS,
        LOCO,
        ZMBV,
        VCR1,
        Snow,
        FFV1,
        v210,

        NB            // Number of entrys (do not use when dynamically linking)
    }

    // Codecs with hardware acceleration
    public enum LAVVideoHWCodec
    {
        H264 = LAVVideoCodec.H264,
        VC1 = LAVVideoCodec.VC1,
        MPEG2 = LAVVideoCodec.MPEG2,
        MPEG4 = LAVVideoCodec.MPEG4,
        MPEG2DVD,

        NB = MPEG2DVD + 1
    }

    // Flags for HW Resolution support
    public enum LAVHWResFlag
    {
        SD = 0x0001,
        HD = 0x0002,
        UHD = 0x0004
    }

    // Type of hardware accelerations
    public enum LAVHWAccel
    {
        None,
        CUDA,
        QuickSync,
        DXVA2,
        DXVA2CopyBack = DXVA2,
        DXVA2Native
    }

    // Deinterlace algorithms offered by the hardware decoders
    public enum LAVHWDeintModes
    {
        Weave,
        BOB,
        Hardware
    }

    // Software deinterlacing algorithms
    public enum LAVSWDeintModes
    {
        None,
        YADIF
    }

    // Deinterlacing processing mode
    public enum LAVDeintMode
    {
        Auto,
        Aggressive,
        Force,
        Disable
    }

    // Type of deinterlacing to perform
    // - FramePerField re-constructs one frame from every field, resulting in 50/60 fps.
    // - FramePer2Field re-constructs one frame from every 2 fields, resulting in 25/30 fps.
    // Note: Weave will always use FramePer2Field
    public enum LAVDeintOutput
    {
        FramePerField,
        FramePer2Field
    }

    // Control the field order of the deinterlacer
    public enum LAVDeintFieldOrder
    {
        Auto,
        TopFieldFirst,
        BottomFieldFirst,
    }

    // Supported output pixel formats
    public enum LAVOutPixFmts
    {
        None = -1,
        YV12,            // 4:2:0, 8bit, planar
        NV12,            // 4:2:0, 8bit, Y planar, U/V packed
        YUY2,            // 4:2:2, 8bit, packed
        UYVY,            // 4:2:2, 8bit, packed
        AYUV,            // 4:4:4, 8bit, packed
        P010,            // 4:2:0, 10bit, Y planar, U/V packed
        P210,            // 4:2:2, 10bit, Y planar, U/V packed
        Y410,            // 4:4:4, 10bit, packed
        P016,            // 4:2:0, 16bit, Y planar, U/V packed
        P216,            // 4:2:2, 16bit, Y planar, U/V packed
        Y416,            // 4:4:4, 16bit, packed
        RGB32,           // 32-bit RGB (BGRA)
        RGB24,           // 24-bit RGB (BGR)
        v210,            // 4:2:2, 10bit, packed
        v410,            // 4:4:4, 10bit, packed

        YV16,            // 4:2:2, 8-bit, planar
        YV24,

        NB               // Number of formats
    }

    public enum LAVDitherMode
    {
        Ordered,
        Random
    }

    [ComImport,
    Guid("FA40D6E9-4D38-4761-ADD2-71A9EC5FD32F"),
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface ILAVVideoSettings
    {
        // Switch to Runtime Config mode. This will reset all settings to default, and no changes to the settings will be saved
        // You can use this to programmatically configure LAV Audio without interfering with the users settings in the registry.
        // Subsequent calls to this function will reset all settings back to defaults, even if the mode does not change.
        //
        // Note that calling this function during playback is not supported and may exhibit undocumented behaviour. 
        // For smooth operations, it must be called before LAV Audio is connected to other filters.
        [PreserveSig]
        int SetRuntimeConfig(bool bRuntimeConfig);

        // Configure which codecs are enabled
        // If vCodec is invalid (possibly a version difference), Get will return FALSE, and Set E_FAIL.
        [PreserveSig]
        bool GetFormatConfiguration(LAVVideoCodec vCodec);
        [PreserveSig]
        int SetFormatConfiguration(LAVVideoCodec vCodec, bool bEnabled);

        // Set the number of threads to use for Multi-Threaded decoding (where available)
        //  0 = Auto Detect (based on number of CPU cores)
        //  1 = 1 Thread -- No Multi-Threading
        // >1 = Multi-Threading with the specified number of threads
        [PreserveSig]
        int SetNumThreads(uint dwNum);

        // Get the number of threads to use for Multi-Threaded decoding (where available)
        //  0 = Auto Detect (based on number of CPU cores)
        //  1 = 1 Thread -- No Multi-Threading
        // >1 = Multi-Threading with the specified number of threads
        [PreserveSig]
        int GetNumThreads();

        // Set wether the aspect ratio encoded in the stream should be forwarded to the renderer,
        // or the aspect ratio specified by the source filter should be kept.
        // TRUE  = AR from the Stream
        // FALSE = AR from the source filter
        [PreserveSig]
        int SetStreamAR(bool bStreamAR);

        // Get wether the aspect ratio encoded in the stream should be forwarded to the renderer,
        // or the aspect ratio specified by the source filter should be kept.
        // TRUE  = AR from the Stream
        // FALSE = AR from the source filter
        [PreserveSig]
        bool GetStreamAR();

        // Configure which pixel formats are enabled for output
        // If pixFmt is invalid, Get will return FALSE and Set E_FAIL
        [PreserveSig]
        bool GetPixelFormat(LAVOutPixFmts pixFmt);
        [PreserveSig]
        int SetPixelFormat(LAVOutPixFmts pixFmt, bool bEnabled);

        // Set the RGB output range for the YUV->RGB conversion
        // 0 = Auto (same as input), 1 = Limited (16-235), 2 = Full (0-255)
        [PreserveSig]
        int SetRGBOutputRange(uint dwRange);

        // Get the RGB output range for the YUV->RGB conversion
        // 0 = Auto (same as input), 1 = Limited (16-235), 2 = Full (0-255)
        [PreserveSig]
        int GetRGBOutputRange();

        // Set the deinterlacing field order of the hardware decoder
        [PreserveSig]
        int SetDeintFieldOrder(LAVDeintFieldOrder fieldOrder);

        // get the deinterlacing field order of the hardware decoder
        [PreserveSig]
        LAVDeintFieldOrder GetDeintFieldOrder();

        // Set wether all frames should be deinterlaced if the stream is flagged interlaced
        [PreserveSig]
        int SetDeintAggressive(bool bAggressive);

        // Get wether all frames should be deinterlaced if the stream is flagged interlaced
        [PreserveSig]
        bool GetDeintAggressive();

        // Set wether all frames should be deinterlaced, even ones marked as progressive
        [PreserveSig]
        int SetDeintForce(bool bForce);

        // Get wether all frames should be deinterlaced, even ones marked as progressive
        [PreserveSig]
        bool GetDeintForce();

        // Check if the specified HWAccel is supported
        // Note: This will usually only check the availability of the required libraries (ie. for NVIDIA if a recent enough NVIDIA driver is installed)
        // and not check actual hardware support
        // Returns: 0 = Unsupported, 1 = Supported, 2 = Currently running
        [PreserveSig]
        bool CheckHWAccelSupport(LAVHWAccel hwAccel);

        // Set which HW Accel method is used
        // See LAVHWAccel for options.
        [PreserveSig]
        int SetHWAccel(LAVHWAccel hwAccel);

        // Get which HW Accel method is active
        [PreserveSig]
        LAVHWAccel GetHWAccel();

        // Set which codecs should use HW Acceleration
        [PreserveSig]
        int SetHWAccelCodec(LAVVideoHWCodec hwAccelCodec, bool bEnabled);

        // Get which codecs should use HW Acceleration
        [PreserveSig]
        bool GetHWAccelCodec(LAVVideoHWCodec hwAccelCodec);

        // Set the deinterlacing mode used by the hardware decoder
        [PreserveSig]
        int SetHWAccelDeintMode(LAVHWDeintModes deintMode);

        // Get the deinterlacing mode used by the hardware decoder
        [PreserveSig]
        LAVHWDeintModes GetHWAccelDeintMode();

        // Set the deinterlacing output for the hardware decoder
        [PreserveSig]
        int SetHWAccelDeintOutput(LAVDeintOutput deintOutput);

        // Get the deinterlacing output for the hardware decoder
        [PreserveSig]
        LAVDeintOutput GetHWAccelDeintOutput();

        // Set wether the hardware decoder should force high-quality deinterlacing
        // Note: this option is not supported on all decoder implementations and/or all operating systems
        [PreserveSig]
        int SetHWAccelDeintHQ(bool bHQ);

        // Get wether the hardware decoder should force high-quality deinterlacing
        // Note: this option is not supported on all decoder implementations and/or all operating systems
        [PreserveSig]
        bool GetHWAccelDeintHQ();

        // Set the software deinterlacing mode used
        [PreserveSig]
        int SetSWDeintMode(LAVSWDeintModes deintMode);

        // Get the software deinterlacing mode used
        [PreserveSig]
        LAVSWDeintModes GetSWDeintMode();

        // Set the software deinterlacing output
        [PreserveSig]
        int SetSWDeintOutput(LAVDeintOutput deintOutput);

        // Get the software deinterlacing output
        [PreserveSig]
        LAVDeintOutput GetSWDeintOutput();

        // Set wether all content is treated as progressive, and any interlaced flags are ignored
        [PreserveSig]
        int SetDeintTreatAsProgressive(bool bEnabled);

        // Get wether all content is treated as progressive, and any interlaced flags are ignored
        [PreserveSig]
        bool GetDeintTreatAsProgressive();

        // Set the dithering mode used
        [PreserveSig]
        bool SetDitherMode(LAVDitherMode ditherMode);

        // Get the dithering mode used
        [PreserveSig]
        LAVDitherMode GetDitherMode();

        // Set if the MS WMV9 DMO Decoder should be used for VC-1/WMV3
        [PreserveSig]
        bool SetUseMSWMV9Decoder(bool bEnabled);

        // Get if the MS WMV9 DMO Decoder should be used for VC-1/WMV3
        [PreserveSig]
        bool GetUseMSWMV9Decoder();

        // Set if DVD Video support is enabled
        [PreserveSig]
        int SetDVDVideoSupport(bool bEnabled);

        // Get if DVD Video support is enabled
        [PreserveSig]
        bool GetDVDVideoSupport();

        // Set the HW Accel Resolution Flags
        // flags: bitmask of LAVHWResFlag flags
        [PreserveSig]
        int SetHWAccelResolutionFlags(int dwResFlags);

        // Get the HW Accel Resolution Flags
        // flags: bitmask of LAVHWResFlag flags
        [PreserveSig]
        int GetHWAccelResolutionFlags();

        // Toggle Tray Icon
        [PreserveSig]
        int SetTrayIcon(bool bEnabled);

        // Get Tray Icon
        [PreserveSig]
        bool GetTrayIcon();

        // Set the Deint Mode
        [PreserveSig]
        int SetDeinterlacingMode(LAVDeintMode deintMode);

        // Get the Deint Mode
        [PreserveSig]
        LAVDeintMode GetDeinterlacingMode();
    }

    #endregion

    #region Audio

    // Codecs supported in the LAV Audio configuration
    // Codecs not listed here cannot be turned off. You can request codecs to be added to this list, if you wish.
    public enum LAVAudioCodec
    {
        AAC,
        AC3,
        EAC3,
        DTS,
        MP2,
        MP3,
        TRUEHD,
        FLAC,
        VORBIS,
        LPCM,
        PCM,
        WAVPACK,
        TTA,
        WMA2,
        WMAPRO,
        Cook,
        RealAudio,
        WMALL,
        ALAC,
        Opus,
        AMR,
        Nellymoser,
        MSPCM,
        Truespeech,
        TAK,

        NB            // Number of entrys (do not use when dynamically linking)
    }

    // Bitstreaming Codecs supported in LAV Audio
    public enum LAVBitstreamCodec
    {
        AC3,
        EAC3,
        TRUEHD,
        DTS,
        DTSHD,

        NB        // Number of entrys (do not use when dynamically linking)
    }


    // Supported Sample Formats in LAV Audio
    public enum LAVAudioSampleFormat
    {
        Sixteen,
        TwentyFour,
        ThirtyTwo,
        U8,
        FP32,
        Bitstream,

        NB     // Number of entrys (do not use when dynamically linking)
    }

    public enum LAVAudioMixingMode
    {
        None,
        Dolby,
        DPLII,

        NB
    }

    public enum LAVAudioMixingFlag
    {
        UntouchedStereo = 0x0001,
        NormalizeMatrix = 0x0002,
        ClipProtection = 0x0004
    }

    public enum LAVAudioMixingLayout
    {
        Stereo = 3,
        Quadraphonic = 1539,
        FiveDotOne = 63,
        SixDotOne = 1807,
        SevenDotOne = 1599
    }

    [ComImport,
Guid("4158A22B-6553-45D0-8069-24716F8FF171"),
InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface ILAVAudioSettings
    {
        // Switch to Runtime Config mode. This will reset all settings to default, and no changes to the settings will be saved
        // You can use this to programmatically configure LAV Audio without interfering with the users settings in the registry.
        // Subsequent calls to this function will reset all settings back to defaults, even if the mode does not change.
        //
        // Note that calling this function during playback is not supported and may exhibit undocumented behaviour. 
        // For smooth operations, it must be called before LAV Audio is connected to other filters.
        [PreserveSig]
        int SetRuntimeConfig(bool bRuntimeConfig);

        // Dynamic Range Compression
        // pbDRCEnabled: The state of DRC
        // piDRCLevel:   The DRC strength (0-100, 100 is maximum)
        [PreserveSig]
        int GetDRC(out bool pbDRCEnabled, out int piDRCLevel);
        [PreserveSig]
        int SetDRC(bool bDRCEnabled, int iDRCLevel);

        // Configure which codecs are enabled
        // If aCodec is invalid (possibly a version difference), Get will return FALSE, and Set E_FAIL.
        [PreserveSig]
        bool GetFormatConfiguration(LAVAudioCodec aCodec);
        [PreserveSig]
        int SetFormatConfiguration(LAVAudioCodec aCodec, bool bEnabled);

        // Control Bitstreaming
        // If bsCodec is invalid (possibly a version difference), Get will return FALSE, and Set E_FAIL.
        [PreserveSig]
        bool GetBitstreamConfig(LAVBitstreamCodec bsCodec);
        [PreserveSig]
        int SetBitstreamConfig(LAVBitstreamCodec bsCodec, bool bEnabled);

        // Should "normal" DTS frames be encapsulated in DTS-HD frames when bitstreaming?
        [PreserveSig]
        bool GetDTSHDFraming();
        [PreserveSig]
        int SetDTSHDFraming(bool bHDFraming);

        // Control Auto A/V syncing
        [PreserveSig]
        bool GetAutoAVSync();
        [PreserveSig]
        int SetAutoAVSync(bool bAutoSync);

        // Convert all Channel Layouts to standard layouts
        // Standard are: Mono, Stereo, 5.1, 6.1, 7.1
        [PreserveSig]
        bool GetOutputStandardLayout();
        [PreserveSig]
        int SetOutputStandardLayout(bool bStdLayout);

        // Expand Mono to Stereo by simply doubling the audio
        [PreserveSig]
        bool GetExpandMono();
        [PreserveSig]
        int SetExpandMono(bool bExpandMono);

        // Expand 6.1 to 7.1 by doubling the back center
        [PreserveSig]
        bool GetExpand61();
        [PreserveSig]
        int SetExpand61(bool bExpand61);

        // Allow Raw PCM and SPDIF encoded input
        [PreserveSig]
        bool GetAllowRawSPDIFInput();
        [PreserveSig]
        int SetAllowRawSPDIFInput(bool bAllow);

        // Configure which sample formats are enabled
        // Note: SampleFormat_Bitstream cannot be controlled by this
        [PreserveSig]
        bool GetSampleFormat(LAVAudioSampleFormat format);
        [PreserveSig]
        int SetSampleFormat(LAVAudioSampleFormat format, bool bEnabled);

        // Configure a delay for the audio
        [PreserveSig]
        int GetAudioDelay(out bool pbEnabled, out int pDelay);
        [PreserveSig]
        int SetAudioDelay(bool bEnabled, int delay);

        // Enable/Disable Mixing
        [PreserveSig]
        int SetMixingEnabled(bool bEnabled);
        [PreserveSig]
        bool GetMixingEnabled();

        // Control Mixing Layout
        [PreserveSig]
        int SetMixingLayout(LAVAudioMixingLayout dwLayout);
        [PreserveSig]
        LAVAudioMixingLayout GetMixingLayout();

        [PreserveSig]
        int SetMixingFlags(LAVAudioMixingFlag dwFlags);
        [PreserveSig]
        LAVAudioMixingFlag GetMixingFlags();

        // Set Mixing Mode
        [PreserveSig]
        int SetMixingMode(LAVAudioMixingMode mixingMode);
        [PreserveSig]
        LAVAudioMixingMode GetMixingMode();

        // Set Mixing Levels
        [PreserveSig]
        int SetMixingLevels(int dwCenterLevel, int dwSurroundLevel, int dwLFELevel);
        [PreserveSig]
        int GetMixingLevels(out int dwCenterLevel, out int dwSurroundLevel, out int dwLFELevel);

        // Toggle Tray Icon
        [PreserveSig]
        int SetTrayIcon(bool bEnabled);
        [PreserveSig]
        bool GetTrayIcon();

        // Toggle Dithering for sample format conversion
        [PreserveSig]
        int SetSampleConvertDithering(bool bEnabled);
        [PreserveSig]
        bool GetSampleConvertDithering();
    }

    [ComImport,
   Guid("A668B8F2-BA87-4F63-9D41-768F7DE9C50E"),
   InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface ILAVAudioStatus
    {
        [PreserveSig]
        bool IsSampleFormatSupported(LAVAudioSampleFormat sfCheck);

        // Get details about the current decoding format
        [PreserveSig]
        int GetDecodeDetails(IntPtr pCodec, IntPtr pDecodeFormat, out int pnChannels, out int pSampleRate, out uint pChannelMask);

        // Get details about the current output format
        [PreserveSig]
        int GetOutputDetails(IntPtr pOutputFormat, out int pnChannels, out int pSampleRate, out uint pChannelMask);

        // Enable Volume measurements
        [PreserveSig]
        int EnableVolumeStats();

        // Disable Volume measurements
        [PreserveSig]
        int DisableVolumeStats();

        // Get Volume Average for the given channel
        [PreserveSig]
        int GetChannelVolumeAverage(ushort nChannel, out float pfDb);
    }

    #endregion

    #region Splitter

    public enum LAVSubtitleMode
    {
        NoSubs = 0,
        ForcedOnly,
        Default,
        Advanced
    };

    public enum VC1TimestampMode : short
    {
        None = 0,
        Always,
        Auto
    }

    [ComVisible(true), ComImport, SuppressUnmanagedCodeSecurity,
Guid("774A919D-EA95-4A87-8A1E-F48ABE8499C7"),
InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface ILAVSplitterSettings
    {
        // Switch to Runtime Config mode. This will reset all settings to default, and no changes to the settings will be saved
        // You can use this to programmatically configure LAV Splitter without interfering with the users settings in the registry.
        // Subsequent calls to this function will reset all settings back to defaults, even if the mode does not change.
        //
        // Note that calling this function during playback is not supported and may exhibit undocumented behaviour.
        // For smooth operations, it must be called before LAV Splitter opens a file.
        [PreserveSig]
        int SetRuntimeConfig(bool runtime);


        // Retrieve the preferred languages as ISO 639-2 language codes, comma seperated
        // If the result is NULL, no language has been set
        // Memory for the string will be allocated, and has to be free'ed by the caller with CoTaskMemFree
        [PreserveSig]
        int GetPreferredLanguages([MarshalAs(UnmanagedType.LPWStr)]out string langs);

        // Set the preferred languages as ISO 639-2 language codes, comma seperated
        // To reset to no preferred language, pass NULL or the empty string
        [PreserveSig]
        int SetPreferredLanguages([MarshalAs(UnmanagedType.LPWStr)]string langs);

        // Retrieve the preferred subtitle languages as ISO 639-2 language codes, comma seperated
        // If the result is NULL, no language has been set
        // If no subtitle language is set, the main language preference is used.
        // Memory for the string will be allocated, and has to be free'ed by the caller with CoTaskMemFree
        [PreserveSig]
        int GetPreferredSubtitleLanguages([MarshalAs(UnmanagedType.LPWStr)]out string langs);

        // Set the preferred subtitle languages as ISO 639-2 language codes, comma seperated
        // To reset to no preferred language, pass NULL or the empty string
        // If no subtitle language is set, the main language preference is used.
        [PreserveSig]
        int SetPreferredSubtitleLanguages([MarshalAs(UnmanagedType.LPWStr)]string langs);

        // Get the current subtitle mode
        // See enum for possible values
        [PreserveSig]
        LAVSubtitleMode GetSubtitleMode();

        // Set the current subtitle mode
        // See enum for possible values
        [PreserveSig]
        int SetSubtitleMode(LAVSubtitleMode mode);

        // Get the subtitle matching language flag
        // TRUE = Only subtitles with a language in the preferred list will be used; FALSE = All subtitles will be used
        // @deprecated - do not use anymore, deprecated and non-functional, replaced by advanced subtitle mode
        [PreserveSig]
        bool GetSubtitleMatchingLanguage();

        // Set the subtitle matching language flag
        // TRUE = Only subtitles with a language in the preferred list will be used; FALSE = All subtitles will be used
        // @deprecated - do not use anymore, deprecated and non-functional, replaced by advanced subtitle mode
        [PreserveSig]
        int SetSubtitleMatchingLanguage(bool mode);

        // Control wether a special "Forced Subtitles" stream will be created for PGS subs
        [PreserveSig]
        bool GetPGSForcedStream();

        // Control wether a special "Forced Subtitles" stream will be created for PGS subs
        [PreserveSig]
        int SetPGSForcedStream(bool enabled);

        // Get the PGS forced subs config
        // TRUE = only forced PGS frames will be shown, FALSE = all frames will be shown
        [PreserveSig]
        bool GetPGSOnlyForced();

        // Set the PGS forced subs config
        // TRUE = only forced PGS frames will be shown, FALSE = all frames will be shown
        [PreserveSig]
        int SetPGSOnlyForced(bool forced);

        // Get the VC-1 Timestamp Processing mode
        // 0 - No Timestamp Correction, 1 - Always Timestamp Correction, 2 - Auto (Correction for Decoders that need it)
        [PreserveSig]
        int GetVC1TimestampMode();

        // Set the VC-1 Timestamp Processing mode
        // 0 - No Timestamp Correction, 1 - Always Timestamp Correction, 2 - Auto (Correction for Decoders that need it)
        [PreserveSig]
        int SetVC1TimestampMode(short enabled);

        // Set whether substreams (AC3 in TrueHD, for example) should be shown as a seperate stream
        [PreserveSig]
        int SetSubstreamsEnabled(bool enabled);

        // Check whether substreams (AC3 in TrueHD, for example) should be shown as a seperate stream
        [PreserveSig]
        bool GetSubstreamsEnabled();

        // Set if the ffmpeg parsers should be used for video streams
        [PreserveSig]
        int SetVideoParsingEnabled(bool enabled);

        // Query if the ffmpeg parsers are being used for video streams
        [PreserveSig]
        bool GetVideoParsingEnabled();

        // Set if LAV Splitter should try to fix broken HD-PVR streams
        [PreserveSig]
        int SetFixBrokenHDPVR(bool enabled);

        // Query if LAV Splitter should try to fix broken HD-PVR streams
        [PreserveSig]
        bool GetFixBrokenHDPVR();

        // Control wether the givne format is enabled
        [PreserveSig]
        int SetFormatEnabled([MarshalAs(UnmanagedType.LPStr)]string strFormat, bool bEnabled);

        // Check if the given format is enabled
        [PreserveSig]
        bool IsFormatEnabled([MarshalAs(UnmanagedType.LPStr)]string strFormat);

        // Set if LAV Splitter should always completely remove the filter connected to its Audio Pin when the audio stream is changed
        [PreserveSig]
        int SetStreamSwitchRemoveAudio(bool enabled);

        // Query if LAV Splitter should always completely remove the filter connected to its Audio Pin when the audio stream is changed
        [PreserveSig]
        bool GetStreamSwitchRemoveAudio();

        // Advanced Subtitle configuration. Refer to the documention for details.
        // If no advanced config exists, will be NULL.
        // Memory for the string will be allocated, and has to be free'ed by the caller with CoTaskMemFree
        [PreserveSig]
        int GetAdvancedSubtitleConfig([MarshalAs(UnmanagedType.LPWStr)]out string ec);

        // Advanced Subtitle configuration. Refer to the documention for details.
        // To reset the config, pass NULL or the empty string.
        // If no subtitle language is set, the main language preference is used.
        [PreserveSig]
        int SetAdvancedSubtitleConfig([MarshalAs(UnmanagedType.LPWStr)]string config);

        // Set if LAV Splitter should prefer audio streams for the hearing or visually impaired
        [PreserveSig]
        int SetUseAudioForHearingVisuallyImpaired(bool bEnabled);

        // Get if LAV Splitter should prefer audio streams for the hearing or visually impaired
        [PreserveSig]
        bool GetUseAudioForHearingVisuallyImpaired();

        // Set the maximum queue size, in megabytes
        [PreserveSig]
        int SetMaxQueueMemSize(int dwMaxSize);

        // Get the maximum queue size, in megabytes
        [PreserveSig]
        int GetMaxQueueMemSize();

        // Toggle Tray Icon
        [PreserveSig]
        int SetTrayIcon(bool bEnabled);

        // Get Tray Icon
        [PreserveSig]
        bool GetTrayIcon();

        // Toggle whether higher quality audio streams are preferred
        [PreserveSig]
        int SetPreferHighQualityAudioStreams(bool bEnabled);

        // Toggle whether higher quality audio streams are preferred
        [PreserveSig]
        bool GetPreferHighQualityAudioStreams();

        // Toggle whether Matroska Linked Segments should be loaded from other files
        [PreserveSig]
        int SetLoadMatroskaExternalSegments(bool bEnabled);

        // Get whether Matroska Linked Segments should be loaded from other files
        [PreserveSig]
        bool GetLoadMatroskaExternalSegments();

        // Get the list of available formats
        // Memory for the string array will be allocated, and has to be free'ed by the caller with CoTaskMemFree
        [PreserveSig]
        int GetFormats(IntPtr formats, out uint nFormats);

        // Set the duration (in ms) of analysis for network streams (to find the streams and codec parameters)
        [PreserveSig]
        int SetNetworkStreamAnalysisDuration(int dwDuration);

        // Get the duration (in ms) of analysis for network streams (to find the streams and codec parameters)
        [PreserveSig]
        int GetNetworkStreamAnalysisDuration();
    }
    #endregion
}
