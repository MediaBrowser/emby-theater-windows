using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using MediaFoundation;

namespace Emby.Theater.DirectShow
{

    #region Custom EVR Presenter

    public enum EVRCPSetting
    {
        NOMINAL_RANGE = 0,
        SUBTITLE_ALPHA,
        USE_MF_TIME_CALC,
        FRAME_DROP_THRESHOLD,
        EVRCP_SETTING_CORRECT_AR
    }

    [ComImport, System.Security.SuppressUnmanagedCodeSecurity,
    Guid("759B5834-8F83-4855-9484-DC183B381D84"),
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IEVRCPConfig
    {
        [PreserveSig]
        int GetInt(EVRCPSetting setting, out int value);
        [PreserveSig]
        int SetInt(EVRCPSetting setting, int value);
        [PreserveSig]
        int GetFloat(EVRCPSetting setting, out float value);
        [PreserveSig]
        int SetFloat(EVRCPSetting setting, float value);
        [PreserveSig]
        int GetBool(EVRCPSetting setting, out bool value);
        [PreserveSig]
        int SetBool(EVRCPSetting setting, bool value);
        [PreserveSig]
        int GetString(EVRCPSetting setting, [Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder value);
        [PreserveSig]
        int SetString(EVRCPSetting setting, [In, MarshalAs(UnmanagedType.LPWStr)] string value);
    }

    #endregion

    #region WASAPI Audio Renderer

    public enum AC3Encoding
    {
        DISABLED = 0,
        AUTO,
        FORCED
    }

    public enum AUDCLNT_SHAREMODE
    {
        SHARED,
        EXCLUSIVE
    }
    public enum SpeakerPosition
    {
        FRONT_LEFT = 0x1,
        FRONT_RIGHT = 0x2,
        FRONT_CENTER = 0x4,
        LOW_FREQUENCY = 0x8,
        BACK_LEFT = 0x10,
        BACK_RIGHT = 0x20,
        FRONT_LEFT_OF_CENTER = 0x40,
        FRONT_RIGHT_OF_CENTER = 0x80,
        BACK_CENTER = 0x100,
        SIDE_LEFT = 0x200,
        SIDE_RIGHT = 0x400,
        TOP_CENTER = 0x800,
        TOP_FRONT_LEFT = 0x1000,
        TOP_FRONT_CENTER = 0x2000,
        TOP_FRONT_RIGHT = 0x4000,
        TOP_BACK_LEFT = 0x8000,
        TOP_BACK_CENTER = 0x10000,
        TOP_BACK_RIGHT = 0x20000,

        // Bit mask locations reserved for future use
        RESERVED = 0x7FFC0000,

        // Used to specify that any possible permutation of speaker configurations
        //ALL = 0x80000000
    }

    public enum SpeakerConfig
    {
        Mono = SpeakerPosition.FRONT_CENTER,
        Stereo = SpeakerPosition.FRONT_LEFT | SpeakerPosition.FRONT_RIGHT,
        Quad = SpeakerPosition.FRONT_LEFT | SpeakerPosition.FRONT_RIGHT | SpeakerPosition.BACK_LEFT  | SpeakerPosition.BACK_RIGHT,
        Surround = SpeakerPosition.FRONT_LEFT | SpeakerPosition.FRONT_RIGHT | SpeakerPosition.FRONT_CENTER | SpeakerPosition.BACK_CENTER,
        //FiveDotOne = SpeakerPosition.FRONT_LEFT | SpeakerPosition.FRONT_RIGHT | SpeakerPosition.FRONT_CENTER | SpeakerPosition.LOW_FREQUENCY | SpeakerPosition.BACK_LEFT  | SpeakerPosition.BACK_RIGHT, //obsolete
        FiveDotOneSurround = SpeakerPosition.FRONT_LEFT | SpeakerPosition.FRONT_RIGHT | SpeakerPosition.FRONT_CENTER | SpeakerPosition.LOW_FREQUENCY | SpeakerPosition.SIDE_LEFT  | SpeakerPosition.SIDE_RIGHT,
        SevenDotOneSurround = SpeakerPosition.FRONT_LEFT | SpeakerPosition.FRONT_RIGHT | SpeakerPosition.FRONT_CENTER | SpeakerPosition.LOW_FREQUENCY | SpeakerPosition.BACK_LEFT | SpeakerPosition.BACK_RIGHT | SpeakerPosition.SIDE_LEFT | SpeakerPosition.SIDE_RIGHT
    }

    public enum MPARUseFilters
    {
        WASAPI = 0,
        AC3ENCODER = 1,
        BIT_DEPTH_IN = 2,
        BIT_DEPTH_OUT = 4,
        TIME_STRETCH = 8,
        SAMPLE_RATE_CONVERTER = 16,
        CHANNEL_MIXER = 32,
        COMPAT = WASAPI | CHANNEL_MIXER | BIT_DEPTH_OUT | BIT_DEPTH_IN | SAMPLE_RATE_CONVERTER,
        MID = WASAPI | BIT_DEPTH_IN | BIT_DEPTH_OUT | TIME_STRETCH | SAMPLE_RATE_CONVERTER | CHANNEL_MIXER,
        ALL = WASAPI | AC3ENCODER | BIT_DEPTH_IN | BIT_DEPTH_OUT | TIME_STRETCH | SAMPLE_RATE_CONVERTER | CHANNEL_MIXER
    }

    [ComImport, System.Security.SuppressUnmanagedCodeSecurity,
    Guid("CA0CDCD8-D26B-4F8F-B23C-D8D949B14297"),
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IMPAudioSettings
    {
        [PreserveSig]
        int GetAC3EncodingMode(out AC3Encoding setting);
        [PreserveSig]
        int SetAC3EncodingMode(AC3Encoding setting);

        [PreserveSig]
        int GetLogSampleTimes(out bool setting);
        [PreserveSig]
        int SetLogSampleTimes(bool setting);

        [PreserveSig]
        int GetEnableSyncAdjustment(out bool setting);
        [PreserveSig]
        int SetEnableSyncAdjustment(bool setting);

        [PreserveSig]
        int GetWASAPIMode(out AUDCLNT_SHAREMODE setting);
        [PreserveSig]
        int SetWASAPIMode(AUDCLNT_SHAREMODE setting);

        [PreserveSig]
        int GetUseWASAPIEventMode(out bool setting);
        [PreserveSig]
        int SetUseWASAPIEventMode(bool setting);

        [PreserveSig]
        int GetUseTimeStretching(out bool setting);
        [PreserveSig]
        int SetUseTimeStretching(bool setting);

        [PreserveSig]
        int GetExpandMonoToStereo(out bool setting);
        [PreserveSig]
        int SetExpandMonoToStereo(bool setting);

        [PreserveSig]
        int GetAC3Bitrate(out int setting);
        [PreserveSig]
        int SetAC3Bitrate(int setting);

        [PreserveSig]
        int GetSpeakerConfig(out SpeakerConfig setting);
        [PreserveSig]
        int SetSpeakerConfig(SpeakerConfig setting);

        [PreserveSig]
        int GetForceChannelMixing(out bool setting);
        [PreserveSig]
        int SetForceChannelMixing(bool setting);

        [PreserveSig]
        int GetAudioDelay(out int setting);
        [PreserveSig]
        int SetAudioDelay(int setting);

        [PreserveSig]
        int GetOutputBuffer(out int setting);
        [PreserveSig]
        int SetOutputBuffer(int setting);

        [PreserveSig]
        int GetSampleRate(out int setting);
        [PreserveSig]
        int SetSampleRate(int setting);

        [PreserveSig]
        int GetBitDepth(out int setting);
        [PreserveSig]
        int SetBitDepth(int setting);

        [PreserveSig]
        int GetResamplingQuality(out int setting);
        [PreserveSig]
        int SetResamplingQuality(int setting);

        [PreserveSig]
        int SetAudioDevice(int setting);
        [PreserveSig]
        int SetAudioDeviceById([In, MarshalAs(UnmanagedType.LPWStr)] string setting);

        [PreserveSig]
        int GetSpeakerMatchOutput(out bool setting);
        [PreserveSig]
        int SetSpeakerMatchOutput(bool setting);

        [PreserveSig]
        int GetUseFilters(out int setting);
        [PreserveSig]
        int SetUseFilters(int setting);

        [PreserveSig]
        int GetAllowBitStreaming(out bool setting);
        [PreserveSig]
        int SetAllowBitStreaming(bool setting);
    }
    
    public enum MPARSetting
    {
        AC3_ENCODING = 0,           // enum (MPARAC3EncodingMode)
        LOG_SAMPLE_TIMES = 1,       // bool
        ENABLE_SYNC_ADJUSTMENT = 2, // bool
        WASAPI_MODE = 3,            // enum (MPARWASAPIMode)
        WASAPI_EVENT_DRIVEN = 4,    // bool
        ENABLE_TIME_STRETCHING = 5, // bool
        EXPAND_MONO_TO_STEREO = 6,  // bool
        AC3_BITRATE = 7,            // enum (MPARAC3Bitrate)
        SPEAKER_CONFIG = 8,         // enum (MPARSpeakerConfig)
        FORCE_CHANNEL_MIXING = 9,   // bool
        AUDIO_DELAY = 10,           // int (audio delay in ms)
        OUTPUT_BUFFER_LENGTH = 11,  // int (output buffer in ms)
        SAMPLE_RATE = 12,           // enum (MPARSampleRate)
        BIT_DEPTH = 13,             // enum (MPARBitDepth)
        LIB_RESAMPLE_QUALITY = 14,  // enum (MPARLibResampleQuality)
        USE_FILTERS = 15,           // int (flags from MPARUseFilters enum)
        SETTING_AUDIO_DEVICE = 16,  // wchar
        ALLOW_BITSTREAMING = 17     // bool
    }

    [ComImport, System.Security.SuppressUnmanagedCodeSecurity,
    Guid("CA0CDCD8-D26B-4F8F-B23C-D8D949B14297"),
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IMPAudioRendererConfig
    {
        [PreserveSig]
        int GetBool(MPARSetting setting, out bool value);
        [PreserveSig]
        int SetBool(MPARSetting setting, bool value);
        [PreserveSig]
        int GetInt(MPARSetting setting, out int value);
        [PreserveSig]
        int SetInt(MPARSetting setting, int value);
        [PreserveSig]
        int GetString(MPARSetting setting, [Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder value);
        [PreserveSig]
        int SetString(MPARSetting setting, [In, MarshalAs(UnmanagedType.LPWStr)] string value);
    }

    #endregion
}
