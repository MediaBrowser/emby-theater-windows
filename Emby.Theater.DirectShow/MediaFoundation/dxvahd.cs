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

// This file is a mess.  While technically part of MF, I'm in no hurry 
// to try to get this to work.

using System;
using System.Runtime.InteropServices;
using System.Security;

using MediaFoundation.Misc;
using System.Drawing;

namespace MediaFoundation.dxvahd
{
#if ALLOW_UNTESTED_INTERFACES

    //DEFINE_GUID(DXVAHD_STREAM_STATE_PRIVATE_IVTC, 0x9c601e3c,0x0f33,0x414c,0xa7,0x39,0x99,0x54,0x0e,0xe4,0x2d,0xa5);

    public interface IDirect3DSurface9
    {
    }

    public interface IDirect3DDevice9Ex
    {
    }

    [UnmanagedName("DXVAHD_FRAME_FORMAT")]
    public enum DXVAHD_FRAME_FORMAT
    {
        Progressive = 0,
        InterlacedTopFieldFirst = 1,
        InterlacedBottomFieldFirst = 2
    }

    [UnmanagedName("DXVAHD_DEVICE_USAGE")]
    public enum DXVAHD_DEVICE_USAGE
    {
        PlaybackNormal = 0,
        OptimalSpeed = 1,
        OptimalQuality = 2
    }

    [UnmanagedName("DXVAHD_SURFACE_TYPE")]
    public enum DXVAHD_SURFACE_TYPE
    {
        VideoInput = 0,
        VideoInputPrivate = 1,
        VideoOutput = 2
    }

    [UnmanagedName("DXVAHD_DEVICE_TYPE")]
    public enum DXVAHD_DEVICE_TYPE
    {
        Hardware = 0,
        Software = 1,
        Reference = 2,
        Other = 3
    }

    [Flags, UnmanagedName("DXVAHD_DEVICE_CAPS")]
    public enum DXVAHD_DEVICE_CAPS
    {
        LinearSpace = 0x1,
        xvYCC = 0x2,
        RGBRangeConversion = 0x4,
        YCbCrMatrixConversion = 0x8
    }

    [Flags, UnmanagedName("DXVAHD_FEATURE_CAPS")]
    public enum DXVAHD_FEATURE_CAPS
    {
        AlphaFill = 0x1,
        Constriction = 0x2,
        LumaKey = 0x4,
        AlphaPalette = 0x8
    }

    [Flags, UnmanagedName("DXVAHD_FILTER_CAPS")]
    public enum DXVAHD_FILTER_CAPS
    {
        Brightness = 0x1,
        Contrast = 0x2,
        Hue = 0x4,
        Saturation = 0x8,
        NoiseReduction = 0x10,
        EdgeEnhancement = 0x20,
        AnamorphicScaling = 0x40
    }

    [Flags, UnmanagedName("DXVAHD_INPUT_FORMAT_CAPS")]
    public enum DXVAHD_INPUT_FORMAT_CAPS
    {
        RGBInterlaced = 0x1,
        RGBProcAmp = 0x2,
        RGBLumaKey = 0x4,
        PaletteInterlaced = 0x8
    }

    [Flags, UnmanagedName("DXVAHD_PROCESSOR_CAPS")]
    public enum DXVAHD_PROCESSOR_CAPS
    {
        DeinterlaceBland = 0x1,
        DeinterlaceBob = 0x2,
        DeinterlaceAdaptive = 0x4,
        DeinterlaceMotionCompensation = 0x8,
        InverseTelecine = 0x10,
        FrameRateConversion = 0x20
    }

    [Flags, UnmanagedName("DXVAHD_ITELECINE_CAPS")]
    public enum DXVAHD_ITELECINE_CAPS
    {
        CAPS_32 = 0x1,
        CAPS_22 = 0x2,
        CAPS_2224 = 0x4,
        CAPS_2332 = 0x8,
        CAPS_32322 = 0x10,
        CAPS_55 = 0x20,
        CAPS_64 = 0x40,
        CAPS_87 = 0x80,
        CAPS_222222222223 = 0x100,
        CAPS_OTHER = unchecked((int)0x80000000)
    }

    [UnmanagedName("DXVAHD_FILTER")]
    public enum DXVAHD_FILTER
    {
        Brightness = 0,
        Contrast = 1,
        Hue = 2,
        Saturation = 3,
        NoiseReduction = 4,
        EdgeEnhancement = 5,
        AnamorphicScaling = 6
    }

    [UnmanagedName("DXVAHD_BLT_STATE")]
    public enum DXVAHD_BLT_STATE
    {
        TargetRect = 0,
        BackgroundColor = 1,
        OutputColorSpace = 2,
        AlphaFill = 3,
        Constriction = 4,
        Private = 1000
    }

    [UnmanagedName("DXVAHD_ALPHA_FILL_MODE")]
    public enum DXVAHD_ALPHA_FILL_MODE
    {
        Opaque = 0,
        Background = 1,
        Destination = 2,
        SourceStream = 3
    }

    [UnmanagedName("DXVAHD_STREAM_STATE")]
    public enum DXVAHD_STREAM_STATE
    {
        D3DFormat = 0,
        FrameFormat = 1,
        InputColorSpace = 2,
        OutputRate = 3,
        SourceRect = 4,
        DestinationRect = 5,
        Alpha = 6,
        Palette = 7,
        LumaKey = 8,
        AspectRatio = 9,
        FilterBrightness = 100,
        FilterContrast = 101,
        FilterHue = 102,
        FilterSaturation = 103,
        FilterNoiseReduction = 104,
        FilterEdgeEnhancement = 105,
        FilterAnamorphicScaling = 106,
        Private = 1000
    }

    [UnmanagedName("DXVAHD_OUTPUT_RATE")]
    public enum DXVAHD_OUTPUT_RATE
    {
        Normal = 0,
        Half = 1,
        Custom = 2
    }

    [UnmanagedName("DXVAHD_RATIONAL")]
    public struct DXVAHD_RATIONAL
    {
        public int Numerator;
        public int Denominator;
    }

    [UnmanagedName("DXVAHD_COLOR_RGBA")]
    public struct DXVAHD_COLOR_RGBA
    {
        public float R;
        public float G;
        public float B;
        public float A;
    }

    [UnmanagedName("DXVAHD_COLOR_YCbCrA")]
    public struct DXVAHD_COLOR_YCbCrA
    {
        public float Y;
        public float Cb;
        public float Cr;
        public float A;
    }

    [StructLayout(LayoutKind.Explicit), UnmanagedName("DXVAHD_COLOR")]
    public struct DXVAHD_COLOR
    {
        [FieldOffset(0)]
        public DXVAHD_COLOR_RGBA RGB;
        [FieldOffset(0)]
        public DXVAHD_COLOR_YCbCrA YCbCr;
    }

    [StructLayout(LayoutKind.Sequential), UnmanagedName("DXVAHD_CONTENT_DESC")]
    public struct DXVAHD_CONTENT_DESC
    {
        public DXVAHD_FRAME_FORMAT InputFrameFormat;
        public DXVAHD_RATIONAL InputFrameRate;
        public int InputWidth;
        public int InputHeight;
        public DXVAHD_RATIONAL OutputFrameRate;
        public int OutputWidth;
        public int OutputHeight;
    }

    [StructLayout(LayoutKind.Sequential), UnmanagedName("DXVAHD_VPDEVCAPS")]
    public struct DXVAHD_VPDEVCAPS
    {
        public DXVAHD_DEVICE_TYPE DeviceType;
        public int DeviceCaps;
        public int FeatureCaps;
        public int FilterCaps;
        public int InputFormatCaps;
        public int InputPool;
        public int OutputFormatCount;
        public int InputFormatCount;
        public int VideoProcessorCount;
        public int MaxInputStreams;
        public int MaxStreamStates;
    }

    [StructLayout(LayoutKind.Sequential), UnmanagedName("DXVAHD_VPCAPS")]
    public struct DXVAHD_VPCAPS
    {
        public Guid VPGuid;
        public int PastFrames;
        public int FutureFrames;
        public int ProcessorCaps;
        public int ITelecineCaps;
        public int CustomRateCount;
    }

    [StructLayout(LayoutKind.Sequential), UnmanagedName("DXVAHD_CUSTOM_RATE_DATA")]
    public struct DXVAHD_CUSTOM_RATE_DATA
    {
        public DXVAHD_RATIONAL CustomRate;
        public int OutputFrames;
        [MarshalAs(UnmanagedType.Bool)]
        public bool InputInterlaced;
        public int InputFramesOrFields;
    }

    [StructLayout(LayoutKind.Sequential), UnmanagedName("DXVAHD_FILTER_RANGE_DATA")]
    public struct DXVAHD_FILTER_RANGE_DATA
    {
        public int Minimum;
        public int Maximum;
        public int Default;
        public float Multiplier;
    }

    [StructLayout(LayoutKind.Sequential), UnmanagedName("DXVAHD_BLT_STATE_TARGET_RECT_DATA")]
    public struct DXVAHD_BLT_STATE_TARGET_RECT_DATA
    {
        [MarshalAs(UnmanagedType.Bool)]
        public bool Enable;
        public MFRect TargetRect;
    }

    [StructLayout(LayoutKind.Sequential), UnmanagedName("DXVAHD_BLT_STATE_BACKGROUND_COLOR_DATA")]
    public struct DXVAHD_BLT_STATE_BACKGROUND_COLOR_DATA
    {
        [MarshalAs(UnmanagedType.Bool)]
        public bool YCbCr;
        public DXVAHD_COLOR BackgroundColor;
    }

    [StructLayout(LayoutKind.Sequential), UnmanagedName("DXVAHD_BLT_STATE_OUTPUT_COLOR_SPACE_DATA")]
    public struct DXVAHD_BLT_STATE_OUTPUT_COLOR_SPACE_DATA
    {
        public int Value;
    }

    [StructLayout(LayoutKind.Sequential), UnmanagedName("DXVAHD_BLT_STATE_ALPHA_FILL_DATA")]
    public struct DXVAHD_BLT_STATE_ALPHA_FILL_DATA
    {
        public DXVAHD_ALPHA_FILL_MODE Mode;
        public int StreamNumber;
    }

    [StructLayout(LayoutKind.Sequential), UnmanagedName("DXVAHD_BLT_STATE_CONSTRICTION_DATA")]
    public struct DXVAHD_BLT_STATE_CONSTRICTION_DATA
    {
        [MarshalAs(UnmanagedType.Bool)]
        public bool Enable;
        public MFSize xSize;
    }

    [StructLayout(LayoutKind.Sequential), UnmanagedName("DXVAHD_BLT_STATE_PRIVATE_DATA")]
    public struct DXVAHD_BLT_STATE_PRIVATE_DATA
    {
        public Guid Guid;
        public int DataSize;
        public IntPtr pData;
    }

    [StructLayout(LayoutKind.Sequential), UnmanagedName("DXVAHD_STREAM_STATE_D3DFORMAT_DATA")]
    public struct DXVAHD_STREAM_STATE_D3DFORMAT_DATA
    {
        public int Format; // D3DFORMAT
    }

    [StructLayout(LayoutKind.Sequential), UnmanagedName("DXVAHD_STREAM_STATE_FRAME_FORMAT_DATA")]
    public struct DXVAHD_STREAM_STATE_FRAME_FORMAT_DATA
    {
        public DXVAHD_FRAME_FORMAT FrameFormat;
    }

    [StructLayout(LayoutKind.Sequential), UnmanagedName("DXVAHD_STREAM_STATE_INPUT_COLOR_SPACE_DATA")]
    public struct DXVAHD_STREAM_STATE_INPUT_COLOR_SPACE_DATA
    {
        public int Value;
    }

    [StructLayout(LayoutKind.Sequential), UnmanagedName("DXVAHD_STREAM_STATE_OUTPUT_RATE_DATA")]
    public struct DXVAHD_STREAM_STATE_OUTPUT_RATE_DATA
    {
        [MarshalAs(UnmanagedType.Bool)]
        public bool RepeatFrame;
        public DXVAHD_OUTPUT_RATE OutputRate;
        public DXVAHD_RATIONAL CustomRate;
    }

    [StructLayout(LayoutKind.Sequential), UnmanagedName("DXVAHD_STREAM_STATE_SOURCE_RECT_DATA")]
    public struct DXVAHD_STREAM_STATE_SOURCE_RECT_DATA
    {
        [MarshalAs(UnmanagedType.Bool)]
        public bool Enable;
        public MFRect SourceRect;
    }

    [StructLayout(LayoutKind.Sequential), UnmanagedName("DXVAHD_STREAM_STATE_DESTINATION_RECT_DATA")]
    public struct DXVAHD_STREAM_STATE_DESTINATION_RECT_DATA
    {
        [MarshalAs(UnmanagedType.Bool)]
        public bool Enable;
        public MFRect DestinationRect;
    }

    [StructLayout(LayoutKind.Sequential), UnmanagedName("DXVAHD_STREAM_STATE_ALPHA_DATA")]
    public struct DXVAHD_STREAM_STATE_ALPHA_DATA
    {
        [MarshalAs(UnmanagedType.Bool)]
        public bool Enable;
        public float Alpha;
    }

    [StructLayout(LayoutKind.Sequential), UnmanagedName("DXVAHD_STREAM_STATE_PALETTE_DATA")]
    public struct DXVAHD_STREAM_STATE_PALETTE_DATA
    {
        public int Count;
        public int[] pEntries; // D3DCOLOR
    }

    [StructLayout(LayoutKind.Sequential), UnmanagedName("DXVAHD_STREAM_STATE_LUMA_KEY_DATA")]
    public struct DXVAHD_STREAM_STATE_LUMA_KEY_DATA
    {
        [MarshalAs(UnmanagedType.Bool)]
        public bool Enable;
        public float Lower;
        public float Upper;
    }

    [StructLayout(LayoutKind.Sequential), UnmanagedName("DXVAHD_STREAM_STATE_ASPECT_RATIO_DATA")]
    public struct DXVAHD_STREAM_STATE_ASPECT_RATIO_DATA
    {
        [MarshalAs(UnmanagedType.Bool)]
        public bool Enable;
        public DXVAHD_RATIONAL SourceAspectRatio;
        public DXVAHD_RATIONAL DestinationAspectRatio;
    }

    [StructLayout(LayoutKind.Sequential), UnmanagedName("DXVAHD_STREAM_STATE_FILTER_DATA")]
    public struct DXVAHD_STREAM_STATE_FILTER_DATA
    {
        [MarshalAs(UnmanagedType.Bool)]
        public bool Enable;
        public int Level;
    }

    [StructLayout(LayoutKind.Sequential), UnmanagedName("DXVAHD_STREAM_STATE_PRIVATE_DATA")]
    public struct DXVAHD_STREAM_STATE_PRIVATE_DATA
    {
        public Guid Guid;
        public int DataSize;
        public IntPtr pData;
    }

    [StructLayout(LayoutKind.Sequential), UnmanagedName("DXVAHD_STREAM_DATA")]
    public struct DXVAHD_STREAM_DATA
    {
        [MarshalAs(UnmanagedType.Bool)]
        public bool Enable;
        public int OutputIndex;
        public int InputFrameOrField;
        public int PastFrames;
        public int FutureFrames;
        public IDirect3DSurface9[] ppPastSurfaces;
        public IDirect3DSurface9 pInputSurface;
        public IDirect3DSurface9[] ppFutureSurfaces;
    }

    [StructLayout(LayoutKind.Sequential), UnmanagedName("DXVAHD_STREAM_STATE_PRIVATE_IVTC_DATA")]
    public struct DXVAHD_STREAM_STATE_PRIVATE_IVTC_DATA
    {
        [MarshalAs(UnmanagedType.Bool)]
        public bool Enable;
        public int ITelecineFlags;
        public int Frames;
        public int InputField;
    }

    [ComImport, System.Security.SuppressUnmanagedCodeSecurity,
    Guid("95f12dfd-d77e-49be-815f-57d579634d6d"),
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IDXVAHD_Device
    {
        [PreserveSig]
        int CreateVideoSurface(
            int Width,
            int Height,
            int Format, // D3DFORMAT 
            int Pool, // D3DPOOL
            int Usage,
            DXVAHD_SURFACE_TYPE Type,
            int NumSurfaces,
            out IDirect3DSurface9[] ppSurfaces,
            ref  IntPtr pSharedHandle
            );

        [PreserveSig]
        int GetVideoProcessorDeviceCaps(
            out DXVAHD_VPDEVCAPS pCaps
            );

        [PreserveSig]
        int GetVideoProcessorOutputFormats(
            int Count,
            out int[] pFormats // D3DFORMAT 
            );

        [PreserveSig]
        int GetVideoProcessorInputFormats(
            int Count,
            out int[] pFormats // D3DFORMAT 
            );

        [PreserveSig]
        int GetVideoProcessorCaps(
            int Count,
            out DXVAHD_VPCAPS[] pCaps
            );

        [PreserveSig]
        int GetVideoProcessorCustomRates(
            Guid pVPGuid,
            int Count,
            out DXVAHD_CUSTOM_RATE_DATA[] pRates
            );

        [PreserveSig]
        int GetVideoProcessorFilterRange(
            DXVAHD_FILTER Filter,
            out DXVAHD_FILTER_RANGE_DATA pRange
            );

        [PreserveSig]
        int CreateVideoProcessor(
            Guid pVPGuid,
            out IDXVAHD_VideoProcessor ppVideoProcessor
            );
    }

    [ComImport, System.Security.SuppressUnmanagedCodeSecurity,
    Guid("95f4edf4-6e03-4cd7-be1b-3075d665aa52"),
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IDXVAHD_VideoProcessor
    {
        [PreserveSig]
        int SetVideoProcessBltState(
            DXVAHD_BLT_STATE State,
            int DataSize,
            IntPtr pData
            );

        [PreserveSig]
        int GetVideoProcessBltState(
            DXVAHD_BLT_STATE State,
            int DataSize,
            IntPtr pData
            );

        [PreserveSig]
        int SetVideoProcessStreamState(
            int StreamNumber,
            DXVAHD_STREAM_STATE State,
            int DataSize,
            IntPtr pData
            );

        [PreserveSig]
        int GetVideoProcessStreamState(
            int StreamNumber,
            DXVAHD_STREAM_STATE State,
            int DataSize,
            IntPtr pData
            );

        [PreserveSig]
        int VideoProcessBltHD(
            IDirect3DSurface9 pOutputSurface,
            int OutputFrame,
            int StreamCount,
            DXVAHD_STREAM_DATA[] pStreams
            );
    }

    public delegate int PDXVAHDSW_CreateDevice(
        IDirect3DDevice9Ex pD3DDevice,
        out IntPtr phDevice
        );

    public delegate int PDXVAHDSW_ProposeVideoPrivateFormat(
        IntPtr hDevice,
        ref int pFormat // D3DFORMAT
        );

    public delegate int PDXVAHDSW_GetVideoProcessorDeviceCaps(
        IntPtr hDevice,
        DXVAHD_CONTENT_DESC pContentDesc,
        DXVAHD_DEVICE_USAGE Usage,
        out DXVAHD_VPDEVCAPS pCaps
        );

    public delegate int PDXVAHDSW_GetVideoProcessorOutputFormats(
        IntPtr hDevice,
        DXVAHD_CONTENT_DESC pContentDesc,
        DXVAHD_DEVICE_USAGE Usage,
        int Count,
        int[] pFormats // D3DFORMAT
        );

    public delegate int PDXVAHDSW_GetVideoProcessorInputFormats(
        IntPtr hDevice,
        DXVAHD_CONTENT_DESC pContentDesc,
        DXVAHD_DEVICE_USAGE Usage,
        int Count,
        int[] pFormats // D3DFORMAT
        );

    public delegate int PDXVAHDSW_GetVideoProcessorCaps(
        IntPtr hDevice,
        DXVAHD_CONTENT_DESC pContentDesc,
        DXVAHD_DEVICE_USAGE Usage,
        int Count,
        DXVAHD_VPCAPS[] pCaps
        );

    public delegate int PDXVAHDSW_GetVideoProcessorCustomRates(
        IntPtr hDevice,
        Guid pVPGuid,
        int Count,
        DXVAHD_CUSTOM_RATE_DATA[] pRates
        );

    public delegate int PDXVAHDSW_GetVideoProcessorFilterRange(
        IntPtr hDevice,
        DXVAHD_FILTER Filter,
        out DXVAHD_FILTER_RANGE_DATA pRange
        );

    public delegate int PDXVAHDSW_DestroyDevice(
        IntPtr hDevice
        );

    public delegate int PDXVAHDSW_CreateVideoProcessor(
        IntPtr hDevice,
        Guid pVPGuid,
        out IntPtr phVideoProcessor
        );

    public delegate int PDXVAHDSW_SetVideoProcessBltState(
        IntPtr hVideoProcessor,
        DXVAHD_BLT_STATE State,
        int DataSize,
        IntPtr pData
        );

    public delegate int PDXVAHDSW_GetVideoProcessBltStatePrivate(
        IntPtr hVideoProcessor,
        ref DXVAHD_BLT_STATE_PRIVATE_DATA pData
        );

    public delegate int PDXVAHDSW_SetVideoProcessStreamState(
        IntPtr hVideoProcessor,
        int StreamNumber,
        DXVAHD_STREAM_STATE State,
        int DataSize,
        IntPtr pData
        );

    public delegate int PDXVAHDSW_GetVideoProcessStreamStatePrivate(
        IntPtr hVideoProcessor,
        int StreamNumber,
        ref DXVAHD_STREAM_STATE_PRIVATE_DATA pData
        );

    public delegate int PDXVAHDSW_VideoProcessBltHD(
        IntPtr hVideoProcessor,
        IDirect3DSurface9 pOutputSurface,
        int OutputFrame,
        int StreamCount,
        DXVAHD_STREAM_DATA[] pStreams
        );

    public delegate int PDXVAHDSW_DestroyVideoProcessor(
        IntPtr hVideoProcessor
        );

    [StructLayout(LayoutKind.Sequential), UnmanagedName("DXVAHDSW_CALLBACKS")]
    public struct DXVAHDSW_CALLBACKS
    {
        public PDXVAHDSW_CreateDevice CreateDevice;
        public PDXVAHDSW_ProposeVideoPrivateFormat ProposeVideoPrivateFormat;
        public PDXVAHDSW_GetVideoProcessorDeviceCaps GetVideoProcessorDeviceCaps;
        public PDXVAHDSW_GetVideoProcessorOutputFormats GetVideoProcessorOutputFormats;
        public PDXVAHDSW_GetVideoProcessorInputFormats GetVideoProcessorInputFormats;
        public PDXVAHDSW_GetVideoProcessorCaps GetVideoProcessorCaps;
        public PDXVAHDSW_GetVideoProcessorCustomRates GetVideoProcessorCustomRates;
        public PDXVAHDSW_GetVideoProcessorFilterRange GetVideoProcessorFilterRange;
        public PDXVAHDSW_DestroyDevice DestroyDevice;
        public PDXVAHDSW_CreateVideoProcessor CreateVideoProcessor;
        public PDXVAHDSW_SetVideoProcessBltState SetVideoProcessBltState;
        public PDXVAHDSW_GetVideoProcessBltStatePrivate GetVideoProcessBltStatePrivate;
        public PDXVAHDSW_SetVideoProcessStreamState SetVideoProcessStreamState;
        public PDXVAHDSW_GetVideoProcessStreamStatePrivate GetVideoProcessStreamStatePrivate;
        public PDXVAHDSW_VideoProcessBltHD VideoProcessBltHD;
        public PDXVAHDSW_DestroyVideoProcessor DestroyVideoProcessor;
    }

    public delegate int PDXVAHDSW_Plugin(
        int Size,
        IntPtr[] pCallbacks
        );

    //DEFINE_GUID(DXVAHDControlGuid, 0xa0386e75,0xf70c,0x464c,0xa9,0xce,0x33,0xc4,0x4e,0x09,0x16,0x23); 

    public static class DXVAHDETWGUID
    {
        public static readonly Guid CREATEVIDEOPROCESSOR = new Guid(0x681e3d1e, 0x5674, 0x4fb3, 0xa5, 0x03, 0x2f, 0x20, 0x55, 0xe9, 0x1f, 0x60);
        public static readonly Guid VIDEOPROCESSBLTSTATE = new Guid(0x76c94b5a, 0x193f, 0x4692, 0x94, 0x84, 0xa4, 0xd9, 0x99, 0xda, 0x81, 0xa8);
        public static readonly Guid VIDEOPROCESSSTREAMSTATE = new Guid(0x262c0b02, 0x209d, 0x47ed, 0x94, 0xd8, 0x82, 0xae, 0x02, 0xb8, 0x4a, 0xa7);
        public static readonly Guid VIDEOPROCESSBLTHD = new Guid(0xbef3d435, 0x78c7, 0x4de3, 0x97, 0x07, 0xcd, 0x1b, 0x08, 0x3b, 0x16, 0x0a);
        public static readonly Guid VIDEOPROCESSBLTHD_STREAM = new Guid(0x27ae473e, 0xa5fc, 0x4be5, 0xb4, 0xe3, 0xf2, 0x49, 0x94, 0xd3, 0xc4, 0x95);
        public static readonly Guid DESTROYVIDEOPROCESSOR = new Guid(0xf943f0a0, 0x3f16, 0x43e0, 0x80, 0x93, 0x10, 0x5a, 0x98, 0x6a, 0xa5, 0xf1);
   }

    [StructLayout(LayoutKind.Sequential), UnmanagedName("DXVAHDETW_CREATEVIDEOPROCESSOR")]
    public struct DXVAHDETW_CREATEVIDEOPROCESSOR
    {
        public long pObject;
        public long pD3D9Ex;
        public Guid VPGuid;
    }

    [StructLayout(LayoutKind.Sequential), UnmanagedName("DXVAHDETW_VIDEOPROCESSBLTSTATE")]
    public struct DXVAHDETW_VIDEOPROCESSBLTSTATE
    {
        public long pObject;
        public DXVAHD_BLT_STATE State;
        public int DataSize;
        [MarshalAs(UnmanagedType.Bool)]
        public bool SetState;
    }

    [StructLayout(LayoutKind.Sequential), UnmanagedName("DXVAHDETW_VIDEOPROCESSSTREAMSTATE")]
    public struct DXVAHDETW_VIDEOPROCESSSTREAMSTATE
    {
        public long pObject;
        public int StreamNumber;
        public DXVAHD_STREAM_STATE State;
        public int DataSize;
        [MarshalAs(UnmanagedType.Bool)]
        public bool SetState;
    }

    [StructLayout(LayoutKind.Sequential), UnmanagedName("DXVAHDETW_VIDEOPROCESSBLTHD")]
    public struct DXVAHDETW_VIDEOPROCESSBLTHD
    {
        public long pObject;
        public long pOutputSurface;
        public MFRect TargetRect;
        public int OutputFormat; // D3DFORMAT
        public int ColorSpace;
        public int OutputFrame;
        public int StreamCount;
        [MarshalAs(UnmanagedType.Bool)]
        public bool Enter;
    }

    [StructLayout(LayoutKind.Sequential), UnmanagedName("DXVAHDETW_VIDEOPROCESSBLTHD_STREAM")]
    public struct DXVAHDETW_VIDEOPROCESSBLTHD_STREAM
    {
        public long pObject;
        public long pInputSurface;
        public MFRect SourceRect;
        public MFRect DestinationRect;
        public int InputFormat; // D3DFORMAT
        public DXVAHD_FRAME_FORMAT FrameFormat;
        public int ColorSpace;
        public int StreamNumber;
        public int OutputIndex;
        public int InputFrameOrField;
        public int PastFrames;
        public int FutureFrames;
    }

    [StructLayout(LayoutKind.Sequential), UnmanagedName("DXVAHDETW_DESTROYVIDEOPROCESSOR")]
    public struct DXVAHDETW_DESTROYVIDEOPROCESSOR
    {
        public long pObject;
    }

    public static class OPMExtern
    {
        [DllImport("Dxva2.dll", ExactSpelling = true), SuppressUnmanagedCodeSecurity]
        public static extern int DXVAHD_CreateDevice(
            IDirect3DDevice9Ex pD3DDevice,
            DXVAHD_CONTENT_DESC pContentDesc,
            DXVAHD_DEVICE_USAGE Usage,
            PDXVAHDSW_Plugin pPlugin,
            out IDXVAHD_Device ppDevice
        );
    }

    public delegate int PDXVAHD_CreateDevice(
        IDirect3DDevice9Ex pD3DDevice,
        DXVAHD_CONTENT_DESC pContentDesc,
        DXVAHD_DEVICE_USAGE Usage,
        PDXVAHDSW_Plugin pPlugin,
        out IDXVAHD_Device ppDevice
        );

#endif

}
