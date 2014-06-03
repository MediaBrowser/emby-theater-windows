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

namespace MediaFoundation.OPM
{
#if ALLOW_UNTESTED_INTERFACES

    public class OPMExtern
    {
        [DllImport("Dxva2.dll", ExactSpelling = true), SuppressUnmanagedCodeSecurity]
        public static extern int OPMGetVideoOutputsFromHMONITOR(
            IntPtr hMonitor,
            OPM_VIDEO_OUTPUT_SEMANTICS vos,
            out int pulNumVideoOutputs,
            IntPtr pppOPMVideoOutputArray       // ISYN: IOPMVideoOutput***
            );

        [DllImport("Dxva2.dll", ExactSpelling = true), SuppressUnmanagedCodeSecurity]
        public static extern int OPMGetVideoOutputsFromIDirect3DDevice9Object(
            [MarshalAs(UnmanagedType.IUnknown)] object pDirect3DDevice9, // IDirect3DDevice9
            OPM_VIDEO_OUTPUT_SEMANTICS vos,
            out int pulNumVideoOutputs,
            IntPtr pppOPMVideoOutputArray       /// ISYN: IOPMVideoOutput***
            );
    }

    public static class MFOpmStatusRequests
    {
        public static readonly Guid OPM_GET_CURRENT_HDCP_SRM_VERSION = new Guid(0x99c5ceff, 0x5f1d, 0x4879, 0x81, 0xc1, 0xc5, 0x24, 0x43, 0xc9, 0x48, 0x2b);
        public static readonly Guid OPM_GET_CONNECTED_HDCP_DEVICE_INFORMATION = new Guid(0x0db59d74, 0xa992, 0x492e, 0xa0, 0xbd, 0xc2, 0x3f, 0xda, 0x56, 0x4e, 0x00);
        public static readonly Guid OPM_GET_ACP_AND_CGMSA_SIGNALING = new Guid(0x6629a591, 0x3b79, 0x4cf3, 0x92, 0x4a, 0x11, 0xe8, 0xe7, 0x81, 0x16, 0x71);
        public static readonly Guid OPM_GET_CONNECTOR_TYPE = new Guid(0x81d0bfd5, 0x6afe, 0x48c2, 0x99, 0xc0, 0x95, 0xa0, 0x8f, 0x97, 0xc5, 0xda);
        public static readonly Guid OPM_GET_SUPPORTED_PROTECTION_TYPES = new Guid(0x38f2a801, 0x9a6c, 0x48bb, 0x91, 0x07, 0xb6, 0x69, 0x6e, 0x6f, 0x17, 0x97);
        public static readonly Guid OPM_GET_VIRTUAL_PROTECTION_LEVEL = new Guid(0xb2075857, 0x3eda, 0x4d5d, 0x88, 0xdb, 0x74, 0x8f, 0x8c, 0x1a, 0x05, 0x49);
        public static readonly Guid OPM_GET_ACTUAL_PROTECTION_LEVEL = new Guid(0x1957210a, 0x7766, 0x452a, 0xb9, 0x9a, 0xd2, 0x7a, 0xed, 0x54, 0xf0, 0x3a);
        public static readonly Guid OPM_GET_ACTUAL_OUTPUT_FORMAT = new Guid(0xd7bf1ba3, 0xad13, 0x4f8e, 0xaf, 0x98, 0x0d, 0xcb, 0x3c, 0xa2, 0x04, 0xcc);
        public static readonly Guid OPM_GET_ADAPTER_BUS_TYPE = new Guid(0xc6f4d673, 0x6174, 0x4184, 0x8e, 0x35, 0xf6, 0xdb, 0x52, 0x0, 0xbc, 0xba);
        public static readonly Guid OPM_GET_OUTPUT_ID = new Guid(0x72cb6df3, 0x244f, 0x40ce, 0xb0, 0x9e, 0x20, 0x50, 0x6a, 0xf6, 0x30, 0x2f);
        public static readonly Guid OPM_GET_DVI_CHARACTERISTICS = new Guid(0xa470b3bb, 0x5dd7, 0x4172, 0x83, 0x9c, 0x3d, 0x37, 0x76, 0xe0, 0xeb, 0xf5);
        public static readonly Guid OPM_GET_CODEC_INFO = new Guid(0x4f374491, 0x8f5f, 0x4445, 0x9d, 0xba, 0x95, 0x58, 0x8f, 0x6b, 0x58, 0xb4);
        public static readonly Guid OPM_SET_PROTECTION_LEVEL = new Guid(0x9bb9327c, 0x4eb5, 0x4727, 0x9f, 0x00, 0xb4, 0x2b, 0x09, 0x19, 0xc0, 0xda);
        public static readonly Guid OPM_SET_ACP_AND_CGMSA_SIGNALING = new Guid(0x09a631a5, 0xd684, 0x4c60, 0x8e, 0x4d, 0xd3, 0xbb, 0x0f, 0x0b, 0xe3, 0xee);
        public static readonly Guid OPM_SET_HDCP_SRM = new Guid(0x8b5ef5d1, 0xc30d, 0x44ff, 0x84, 0xa5, 0xea, 0x71, 0xdc, 0xe7, 0x8f, 0x13);
        public static readonly Guid OPM_SET_PROTECTION_LEVEL_ACCORDING_TO_CSS_DVD = new Guid(0x39ce333e, 0x4cc0, 0x44ae, 0xbf, 0xcc, 0xda, 0x50, 0xb5, 0xf8, 0x2e, 0x72);
    }

    public static class OpmConstants
    {
        public const int OPM_OMAC_SIZE = 16;
        public const int OPM_128_BIT_RANDOM_NUMBER_SIZE = 16;
        public const int OPM_ENCRYPTED_INITIALIZATION_PARAMETERS_SIZE = 256;
        public const int OPM_CONFIGURE_SETTING_DATA_SIZE = 4056;
        public const int OPM_GET_INFORMATION_PARAMETERS_SIZE = 4056;
        public const int OPM_REQUESTED_INFORMATION_SIZE = 4076;
        public const int OPM_HDCP_KEY_SELECTION_VECTOR_SIZE = 5;
        public const int OPM_PROTECTION_TYPE_SIZE = 4;
        public const int OPM_BUS_TYPE_MASK = 0xffff;
        public const int OPM_BUS_IMPLEMENTATION_MODIFIER_MASK = 0x7fff;
    }

    public enum OPM_VIDEO_OUTPUT_SEMANTICS
    {
        OPM_VOS_COPP_SEMANTICS = 0,
        OPM_VOS_OPM_SEMANTICS = 1
    }


    public enum a
    {
        OPM_HDCP_FLAG_NONE = 0,
        OPM_HDCP_FLAG_REPEATER = 0x1
    }

    public enum b
    {
        OPM_STATUS_NORMAL = 0,
        OPM_STATUS_LINK_LOST = 0x1,
        OPM_STATUS_RENEGOTIATION_REQUIRED = 0x2,
        OPM_STATUS_TAMPERING_DETECTED = 0x4,
        OPM_STATUS_REVOKED_HDCP_DEVICE_ATTACHED = 0x8
    }

    public enum c
    {
        OPM_CONNECTOR_TYPE_OTHER = -1,
        OPM_CONNECTOR_TYPE_VGA = 0,
        OPM_CONNECTOR_TYPE_SVIDEO = 1,
        OPM_CONNECTOR_TYPE_COMPOSITE_VIDEO = 2,
        OPM_CONNECTOR_TYPE_COMPONENT_VIDEO = 3,
        OPM_CONNECTOR_TYPE_DVI = 4,
        OPM_CONNECTOR_TYPE_HDMI = 5,
        OPM_CONNECTOR_TYPE_LVDS = 6,
        OPM_CONNECTOR_TYPE_D_JPN = 8,
        OPM_CONNECTOR_TYPE_SDI = 9,
        OPM_CONNECTOR_TYPE_DISPLAYPORT_EXTERNAL = 10,
        OPM_CONNECTOR_TYPE_DISPLAYPORT_EMBEDDED = 11,
        OPM_CONNECTOR_TYPE_UDI_EXTERNAL = 12,
        OPM_CONNECTOR_TYPE_UDI_EMBEDDED = 13,
        OPM_COPP_COMPATIBLE_CONNECTOR_TYPE_INTERNAL = unchecked((int)0x80000000)
    }

    public enum d
    {
        OPM_DVI_CHARACTERISTIC_1_0 = 1,
        OPM_DVI_CHARACTERISTIC_1_1_OR_ABOVE = 2
    }

    public enum e
    {
        OPM_BUS_TYPE_OTHER = 0,
        OPM_BUS_TYPE_PCI = 0x1,
        OPM_BUS_TYPE_PCIX = 0x2,
        OPM_BUS_TYPE_PCIEXPRESS = 0x3,
        OPM_BUS_TYPE_AGP = 0x4,
        OPM_BUS_IMPLEMENTATION_MODIFIER_INSIDE_OF_CHIPSET = 0x10000,
        OPM_BUS_IMPLEMENTATION_MODIFIER_TRACKS_ON_MOTHER_BOARD_TO_CHIP = 0x20000,
        OPM_BUS_IMPLEMENTATION_MODIFIER_TRACKS_ON_MOTHER_BOARD_TO_SOCKET = 0x30000,
        OPM_BUS_IMPLEMENTATION_MODIFIER_DAUGHTER_BOARD_CONNECTOR = 0x40000,
        OPM_BUS_IMPLEMENTATION_MODIFIER_DAUGHTER_BOARD_CONNECTOR_INSIDE_OF_NUAE = 0x50000,
        OPM_BUS_IMPLEMENTATION_MODIFIER_NON_STANDARD = unchecked((int)0x80000000),
        OPM_COPP_COMPATIBLE_BUS_TYPE_INTEGRATED = unchecked((int)0x80000000)
    }
    public enum OPM_DPCP_PROTECTION_LEVEL
    {
        OPM_DPCP_OFF = 0,
        OPM_DPCP_ON = 1,
        OPM_DPCP_FORCE_ULONG = 0x7fffffff
    }

    public enum OPM_HDCP_PROTECTION_LEVEL
    {
        OPM_HDCP_OFF = 0,
        OPM_HDCP_ON = 1,
        OPM_HDCP_FORCE_ULONG = 0x7fffffff
    }

    public enum f
    {
        OPM_CGMSA_OFF = 0,
        OPM_CGMSA_COPY_FREELY = 0x1,
        OPM_CGMSA_COPY_NO_MORE = 0x2,
        OPM_CGMSA_COPY_ONE_GENERATION = 0x3,
        OPM_CGMSA_COPY_NEVER = 0x4,
        OPM_CGMSA_REDISTRIBUTION_CONTROL_REQUIRED = 0x8
    }

    public enum OPM_ACP_PROTECTION_LEVEL
    {
        OPM_ACP_OFF = 0,
        OPM_ACP_LEVEL_ONE = 1,
        OPM_ACP_LEVEL_TWO = 2,
        OPM_ACP_LEVEL_THREE = 3,
        OPM_ACP_FORCE_ULONG = 0x7fffffff
    }

    public enum g
    {
        OPM_PROTECTION_TYPE_OTHER = unchecked((int)0x80000000),
        OPM_PROTECTION_TYPE_NONE = 0,
        OPM_PROTECTION_TYPE_COPP_COMPATIBLE_HDCP = 0x1,
        OPM_PROTECTION_TYPE_ACP = 0x2,
        OPM_PROTECTION_TYPE_CGMSA = 0x4,
        OPM_PROTECTION_TYPE_HDCP = 0x8,
        OPM_PROTECTION_TYPE_DPCP = 0x10
    }

    public enum h
    {
        OPM_PROTECTION_STANDARD_OTHER = unchecked((int)0x80000000),
        OPM_PROTECTION_STANDARD_NONE = 0,
        OPM_PROTECTION_STANDARD_IEC61880_525I = 0x1,
        OPM_PROTECTION_STANDARD_IEC61880_2_525I = 0x2,
        OPM_PROTECTION_STANDARD_IEC62375_625P = 0x4,
        OPM_PROTECTION_STANDARD_EIA608B_525 = 0x8,
        OPM_PROTECTION_STANDARD_EN300294_625I = 0x10,
        OPM_PROTECTION_STANDARD_CEA805A_TYPEA_525P = 0x20,
        OPM_PROTECTION_STANDARD_CEA805A_TYPEA_750P = 0x40,
        OPM_PROTECTION_STANDARD_CEA805A_TYPEA_1125I = 0x80,
        OPM_PROTECTION_STANDARD_CEA805A_TYPEB_525P = 0x100,
        OPM_PROTECTION_STANDARD_CEA805A_TYPEB_750P = 0x200,
        OPM_PROTECTION_STANDARD_CEA805A_TYPEB_1125I = 0x400,
        OPM_PROTECTION_STANDARD_ARIBTRB15_525I = 0x800,
        OPM_PROTECTION_STANDARD_ARIBTRB15_525P = 0x1000,
        OPM_PROTECTION_STANDARD_ARIBTRB15_750P = 0x2000,
        OPM_PROTECTION_STANDARD_ARIBTRB15_1125I = 0x4000
    }

    public enum OPM_IMAGE_ASPECT_RATIO_EN300294
    {
        OPM_ASPECT_RATIO_EN300294_FULL_FORMAT_4_BY_3 = 0,
        OPM_ASPECT_RATIO_EN300294_BOX_14_BY_9_CENTER = 1,
        OPM_ASPECT_RATIO_EN300294_BOX_14_BY_9_TOP = 2,
        OPM_ASPECT_RATIO_EN300294_BOX_16_BY_9_CENTER = 3,
        OPM_ASPECT_RATIO_EN300294_BOX_16_BY_9_TOP = 4,
        OPM_ASPECT_RATIO_EN300294_BOX_GT_16_BY_9_CENTER = 5,
        OPM_ASPECT_RATIO_EN300294_FULL_FORMAT_4_BY_3_PROTECTED_CENTER = 6,
        OPM_ASPECT_RATIO_EN300294_FULL_FORMAT_16_BY_9_ANAMORPHIC = 7,
        OPM_ASPECT_RATIO_FORCE_ULONG = 0x7fffffff
    }

    public struct OPM_RANDOM_NUMBER
    {
        public Guid abRandomNumber;
    }

    public struct OPM_OMAC
    {
        public Guid abOMAC;
    }

    public struct OPM_ENCRYPTED_INITIALIZATION_PARAMETERS
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 256)]
        public byte[] abEncryptedInitializationParameters;
    }

    public struct OPM_GET_INFO_PARAMETERS
    {
        public OPM_OMAC omac;
        public OPM_RANDOM_NUMBER rnRandomNumber;
        public Guid guidInformation;
        public int ulSequenceNumber;
        public int cbParametersSize;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4056)]
        public byte[] abParameters;
    }

    public struct OPM_COPP_COMPATIBLE_GET_INFO_PARAMETERS
    {
        public OPM_RANDOM_NUMBER rnRandomNumber;
        public Guid guidInformation;
        public int ulSequenceNumber;
        public int cbParametersSize;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4056)]
        public byte[] abParameters;
    }

    public struct OPM_HDCP_KEY_SELECTION_VECTOR
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 5)]
        public byte[] abKeySelectionVector;
    }

    public struct OPM_CONNECTED_HDCP_DEVICE_INFORMATION
    {
        public OPM_RANDOM_NUMBER rnRandomNumber;
        public int ulStatusFlags;
        public int ulHDCPFlags;
        public OPM_HDCP_KEY_SELECTION_VECTOR ksvB;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 11)]
        public byte[] Reserved;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
        public byte[] Reserved2;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
        public byte[] Reserved3;
    }

    public struct OPM_REQUESTED_INFORMATION
    {
        public OPM_OMAC omac;
        public int cbRequestedInformationSize;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4076)]
        public byte[] abRequestedInformation;
    }

    public struct OPM_STANDARD_INFORMATION
    {
        public OPM_RANDOM_NUMBER rnRandomNumber;
        public int ulStatusFlags;
        public int ulInformation;
        public int ulReserved;
        public int ulReserved2;
    }

    public struct OPM_ACTUAL_OUTPUT_FORMAT
    {
        public OPM_RANDOM_NUMBER rnRandomNumber;
        public int ulStatusFlags;
        public int ulDisplayWidth;
        public int ulDisplayHeight;
        public int dsfSampleInterleaveFormat; // DXVA2_SampleFormat
        public int d3dFormat; // D3DFORMAT
        public int ulFrequencyNumerator;
        public int ulFrequencyDenominator;
    }

    public struct OPM_ACP_AND_CGMSA_SIGNALING
    {
        public OPM_RANDOM_NUMBER rnRandomNumber;
        public int ulStatusFlags;
        public int ulAvailableTVProtectionStandards;
        public int ulActiveTVProtectionStandard;
        public int ulReserved;
        public int ulAspectRatioValidMask1;
        public int ulAspectRatioData1;
        public int ulAspectRatioValidMask2;
        public int ulAspectRatioData2;
        public int ulAspectRatioValidMask3;
        public int ulAspectRatioData3;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        public int[] ulReserved2;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        public int[] ulReserved3;
    }

    public struct OPM_OUTPUT_ID_DATA
    {
        public OPM_RANDOM_NUMBER rnRandomNumber;
        public int ulStatusFlags;
        public long OutputId;
    }

    public struct OPM_CONFIGURE_PARAMETERS
    {
        public OPM_OMAC omac;
        public Guid guidSetting;
        public int ulSequenceNumber;
        public int cbParametersSize;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4056)]
        public byte[] abParameters;
    }

    public struct OPM_SET_PROTECTION_LEVEL_PARAMETERS
    {
        public int ulProtectionType;
        public int ulProtectionLevel;
        public int Reserved;
        public int Reserved2;
    }

    public struct OPM_SET_ACP_AND_CGMSA_SIGNALING_PARAMETERS
    {
        public int ulNewTVProtectionStandard;
        public int ulAspectRatioChangeMask1;
        public int ulAspectRatioData1;
        public int ulAspectRatioChangeMask2;
        public int ulAspectRatioData2;
        public int ulAspectRatioChangeMask3;
        public int ulAspectRatioData3;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        public int[] ulReserved;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        public int[] ulReserved2;
        public int ulReserved3;
    }

    public struct OPM_SET_HDCP_SRM_PARAMETERS
    {
        public int ulSRMVersion;
    }

    public struct OPM_GET_CODEC_INFO_PARAMETERS
    {
        public int cbVerifier;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4052)]
        public byte[] Verifier;
    }

    public struct OPM_GET_CODEC_INFO_INFORMATION
    {
        public OPM_RANDOM_NUMBER rnRandomNumber;
        public int Merit;
    }

    [ComImport, System.Security.SuppressUnmanagedCodeSecurity,
    Guid("0A15159D-41C7-4456-93E1-284CD61D4E8D"),
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IOPMVideoOutput
    {
        [PreserveSig]
        int StartInitialization(
            out OPM_RANDOM_NUMBER prnRandomNumber,
            out IntPtr ppbCertificate,
            out  int pulCertificateLength);

        [PreserveSig]
        int FinishInitialization(
            OPM_ENCRYPTED_INITIALIZATION_PARAMETERS pParameters);

        [PreserveSig]
        int GetInformation(
            OPM_GET_INFO_PARAMETERS pParameters,
            out OPM_REQUESTED_INFORMATION pRequestedInformation);

        [PreserveSig]
        int COPPCompatibleGetInformation(
            OPM_COPP_COMPATIBLE_GET_INFO_PARAMETERS pParameters,
            out OPM_REQUESTED_INFORMATION pRequestedInformation);

        [PreserveSig]
        int Configure(
            OPM_CONFIGURE_PARAMETERS pParameters,
            int ulAdditionalParametersSize,
            IntPtr pbAdditionalParameters);
    }

    //public Guid KSPROPSETID_OPMVideoOutput = new Guid(0x6f414bb, 0xf43a, 0x4fe2, 0xa5, 0x66, 0x77, 0x4b, 0x4c, 0x81, 0xf0, 0xdb);

    public enum KSMETHOD_OPMVIDEOOUTPUT
    {
        //  Output is OPM_RANDOM_NUMBER followed by certificate
        KSMETHOD_OPMVIDEOOUTPUT_STARTINITIALIZATION = 0,

        //  Input OPM_ENCRYPTED_INITIALIZATION_PARAMETERS
        //  Output OPM_STANDARD_INFORMATION
        KSMETHOD_OPMVIDEOOUTPUT_FINISHINITIALIZATION = 1,

        //  Input is OPM_GET_INFO_PARAMETERS, output is OPM_REQUESTED_INFORMATION
        //  Use KsMethod - both input and output in the buffer (not after the KSMETHOD structure)
        KSMETHOD_OPMVIDEOOUTPUT_GETINFORMATION = 2
    }

#endif

}
