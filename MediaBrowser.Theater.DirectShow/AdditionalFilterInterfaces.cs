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

    public enum SpeakerConfig
    {
        Mono = 4,
        Stereo = 3,
        Quad = 51,
        Surround = 263,
        FiveDotOne = 63,
        FiveDotOneSurround = 1551,
        SevenDotOneSurround = 1599
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
        int GetAvailableAudioDevices(
            /*[Out, MarshalAs(UnmanagedType.LPArray)]
            out DeviceDefinition[]*/
            out IntPtr devices,
            out int count); //doesn't work
        [PreserveSig]
        int SetAudioDevice(int setting);
        [PreserveSig]
        int SetAudioDeviceById([In, MarshalAs(UnmanagedType.LPWStr)] string setting);
    }

    #endregion
}
