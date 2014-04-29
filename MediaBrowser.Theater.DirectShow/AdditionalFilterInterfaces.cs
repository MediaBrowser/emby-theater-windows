using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using MediaFoundation;

namespace MediaBrowser.Theater.DirectShow
{

    #region Custom EVR Presenter

    [ComImport,
    Guid("D54059EF-CA38-46A5-9123-0249770482EE"),
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IEVRCPSettings
    {
        [PreserveSig]
        int SetNominalRange(MFNominalRange range);

        [PreserveSig]
        MFNominalRange GetNominalRange();
    }

    #endregion

    #region WASAPI Audio Renderer

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

    [ComImport, System.Security.SuppressUnmanagedCodeSecurity,
    Guid("CA0CDCD8-D26B-4F8F-B23C-D8D949B14297"),
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IMPAudioSettings
    {
        [PreserveSig]
        int GetAC3EncodingMode(out int setting);
        [PreserveSig]
        int SetAC3EncodingMode(int setting);

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
        int GetUseFilters(out bool setting);
        [PreserveSig]
        int SetUseFilters(bool setting);

        [PreserveSig]
        int GetReleaseDeviceOnStop(out bool setting);
        [PreserveSig]
        int SetReleaseDeviceOnStop(bool setting);
    }

    #endregion
}
