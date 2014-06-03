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
using System.Runtime.InteropServices.ComTypes;

using MediaFoundation.Misc;
using MediaFoundation;
using System.Drawing;

namespace MediaFoundation
{
    #region GUIDs, IIDs

    [UnmanagedName("CLSID_CColorConvertDMO"),
    ComImport,
    Guid("98230571-0087-4204-b020-3282538e57d3")]
    public class CColorConvertDMO
    {
    }

    [UnmanagedName("CLSID_MFCaptureEngine"),
    ComImport,
    Guid("efce38d3-8914-4674-a7df-ae1b3d654b8a")]
    public class MFCaptureEngine
    {
    }

    [UnmanagedName("CLSID_MFCaptureEngineClassFactory"),
    ComImport,
    Guid("efce38d3-8914-4674-a7df-ae1b3d654b8a")]
    public class MFCaptureEngineClassFactory
    {
    }

    [UnmanagedName("CLSID_PlayToSourceClassFactory"),
    ComImport,
    Guid("DA17539A-3DC3-42C1-A749-A183B51F085E")]
    public class PlayToSourceClassFactory
    {
    }

    [UnmanagedName("CLSID_MFMediaSharingEngineClassFactory"),
    ComImport,
    Guid("F8E307FB-6D45-4AD3-9993-66CD5A529659")]
    public class MFMediaSharingEngineClassFactory
    {
    }

    [UnmanagedName("CLSID_MFImageSharingEngineClassFactory"),
    ComImport,
    Guid("B22C3339-87F3-4059-A0C5-037AA9707EAF")]
    public class MFImageSharingEngineClassFactory
    {
    }

    [UnmanagedName("CLSID_MFMediaEngineClassFactory"),
    ComImport,
    Guid("B44392DA-499B-446b-A4CB-005FEAD0E6D5")]
    public class MFMediaEngineClassFactory
    {
    }

    [UnmanagedName("CLSID_HttpSchemePlugin"),
    ComImport,
    Guid("44cb442b-9da9-49df-b3fd-023777b16e50")]
    public class HttpSchemePlugin
    {
    }

    [UnmanagedName("CLSID_NetSchemePlugin"),
    ComImport,
    Guid("e9f4ebab-d97b-463e-a2b1-c54ee3f9414d")]
    public class NetSchemePlugin
    {
    }

    [UnmanagedName("CLSID_CreateMediaExtensionObject"),
    ComImport,
    Guid("ef65a54d-0788-45b8-8b14-bc0f6a6b5137")]
    public class CreateMediaExtensionObject
    {
    }

    [UnmanagedName("CLSID_MPEG2ByteStreamPlugin"),
    ComImport,
    Guid("40871C59-AB40-471F-8DC3-1F259D862479")]
    public class MPEG2ByteStreamPlugin
    {
    }

    [UnmanagedName("CLSID_MFByteStreamProxyClassFactory"), 
    ComImport,
    Guid("770e8e77-4916-441c-a9a7-b342d0eebc71")]
    public class MFByteStreamProxyClassFactory
    {
    }

    [UnmanagedName("CLSID_UrlmonSchemePlugin"),
    ComImport,
    Guid("9ec4b4f9-3029-45ad-947b-344de2a249e2")]
    public class UrlmonSchemePlugin
    {
    }

    public static class MF_MEDIA_ENGINE
    {
        // MF_MEDIA_ENGINE_CALLBACK
        public static readonly Guid CALLBACK = new Guid(0xc60381b8, 0x83a4, 0x41f8, 0xa3, 0xd0, 0xde, 0x05, 0x07, 0x68, 0x49, 0xa9);

        // MF_MEDIA_ENGINE_DXGI_MANAGER
        public static readonly Guid DXGI_MANAGER = new Guid(0x065702da, 0x1094, 0x486d, 0x86, 0x17, 0xee, 0x7c, 0xc4, 0xee, 0x46, 0x48);

        // MF_MEDIA_ENGINE_EXTENSION
        public static readonly Guid EXTENSION = new Guid(0x3109fd46, 0x060d, 0x4b62, 0x8d, 0xcf, 0xfa, 0xff, 0x81, 0x13, 0x18, 0xd2);

        // MF_MEDIA_ENGINE_PLAYBACK_HWND
        public static readonly Guid PLAYBACK_HWND = new Guid(0xd988879b, 0x67c9, 0x4d92, 0xba, 0xa7, 0x6e, 0xad, 0xd4, 0x46, 0x03, 0x9d);

        // MF_MEDIA_ENGINE_OPM_HWND
        public static readonly Guid OPM_HWND = new Guid(0xa0be8ee7, 0x0572, 0x4f2c, 0xa8, 0x01, 0x2a, 0x15, 0x1b, 0xd3, 0xe7, 0x26);

        // MF_MEDIA_ENGINE_PLAYBACK_VISUAL
        public static readonly Guid PLAYBACK_VISUAL = new Guid(0x6debd26f, 0x6ab9, 0x4d7e, 0xb0, 0xee, 0xc6, 0x1a, 0x73, 0xff, 0xad, 0x15);

        // MF_MEDIA_ENGINE_COREWINDOW
        public static readonly Guid COREWINDOW = new Guid(0xfccae4dc, 0x0b7f, 0x41c2, 0x9f, 0x96, 0x46, 0x59, 0x94, 0x8a, 0xcd, 0xdc);

        // MF_MEDIA_ENGINE_VIDEO_OUTPUT_FORMAT
        public static readonly Guid VIDEO_OUTPUT_FORMAT = new Guid(0x5066893c, 0x8cf9, 0x42bc, 0x8b, 0x8a, 0x47, 0x22, 0x12, 0xe5, 0x27, 0x26);

        // MF_MEDIA_ENGINE_CONTENT_PROTECTION_FLAGS
        public static readonly Guid CONTENT_PROTECTION_FLAGS = new Guid(0xe0350223, 0x5aaf, 0x4d76, 0xa7, 0xc3, 0x06, 0xde, 0x70, 0x89, 0x4d, 0xb4);

        // MF_MEDIA_ENGINE_CONTENT_PROTECTION_MANAGER
        public static readonly Guid CONTENT_PROTECTION_MANAGER = new Guid(0xfdd6dfaa, 0xbd85, 0x4af3, 0x9e, 0x0f, 0xa0, 0x1d, 0x53, 0x9d, 0x87, 0x6a);

        // MF_MEDIA_ENGINE_AUDIO_ENDPOINT_ROLE
        public static readonly Guid AUDIO_ENDPOINT_ROLE = new Guid(0xd2cb93d1, 0x116a, 0x44f2, 0x93, 0x85, 0xf7, 0xd0, 0xfd, 0xa2, 0xfb, 0x46);

        // MF_MEDIA_ENGINE_AUDIO_CATEGORY
        public static readonly Guid AUDIO_CATEGORY = new Guid(0xc8d4c51d, 0x350e, 0x41f2, 0xba, 0x46, 0xfa, 0xeb, 0xbb, 0x08, 0x57, 0xf6);

        // MF_MEDIA_ENGINE_STREAM_CONTAINS_ALPHA_CHANNEL
        public static readonly Guid STREAM_CONTAINS_ALPHA_CHANNEL = new Guid(0x5cbfaf44, 0xd2b2, 0x4cfb, 0x80, 0xa7, 0xd4, 0x29, 0xc7, 0x4c, 0x78, 0x9d);

        // MF_MEDIA_ENGINE_BROWSER_COMPATIBILITY_MODE
        public static readonly Guid BROWSER_COMPATIBILITY_MODE = new Guid(0x4e0212e2, 0xe18f, 0x41e1, 0x95, 0xe5, 0xc0, 0xe7, 0xe9, 0x23, 0x5b, 0xc3);

        // MF_MEDIA_ENGINE_BROWSER_COMPATIBILITY_MODE_IE9
        public static readonly Guid BROWSER_COMPATIBILITY_MODE_IE9 = new Guid(0x052c2d39, 0x40c0, 0x4188, 0xab, 0x86, 0xf8, 0x28, 0x27, 0x3b, 0x75, 0x22);

        // MF_MEDIA_ENGINE_BROWSER_COMPATIBILITY_MODE_IE10
        public static readonly Guid BROWSER_COMPATIBILITY_MODE_IE10 = new Guid(0x11a47afd, 0x6589, 0x4124, 0xb3, 0x12, 0x61, 0x58, 0xec, 0x51, 0x7f, 0xc3);

        // MF_MEDIA_ENGINE_SOURCE_RESOLVER_CONFIG_STORE
        public static readonly Guid SOURCE_RESOLVER_CONFIG_STORE = new Guid(0x0ac0c497, 0xb3c4, 0x48c9, 0x9c, 0xde, 0xbb, 0x8c, 0xa2, 0x44, 0x2c, 0xa3);
    }

    public static class MFAttributesClsid
    {
        // Audio Renderer Attributes
        public static readonly Guid MF_AUDIO_RENDERER_ATTRIBUTE_ENDPOINT_ID = new Guid(0xb10aaec3, 0xef71, 0x4cc3, 0xb8, 0x73, 0x5, 0xa9, 0xa0, 0x8b, 0x9f, 0x8e);
        public static readonly Guid MF_AUDIO_RENDERER_ATTRIBUTE_ENDPOINT_ROLE = new Guid(0x6ba644ff, 0x27c5, 0x4d02, 0x98, 0x87, 0xc2, 0x86, 0x19, 0xfd, 0xb9, 0x1b);
        public static readonly Guid MF_AUDIO_RENDERER_ATTRIBUTE_FLAGS = new Guid(0xede4b5e0, 0xf805, 0x4d6c, 0x99, 0xb3, 0xdb, 0x01, 0xbf, 0x95, 0xdf, 0xab);
        public static readonly Guid MF_AUDIO_RENDERER_ATTRIBUTE_SESSION_ID = new Guid(0xede4b5e3, 0xf805, 0x4d6c, 0x99, 0xb3, 0xdb, 0x01, 0xbf, 0x95, 0xdf, 0xab);

        // Byte Stream Attributes
        public static readonly Guid MF_BYTESTREAM_ORIGIN_NAME = new Guid(0xfc358288, 0x3cb6, 0x460c, 0xa4, 0x24, 0xb6, 0x68, 0x12, 0x60, 0x37, 0x5a);
        public static readonly Guid MF_BYTESTREAM_CONTENT_TYPE = new Guid(0xfc358289, 0x3cb6, 0x460c, 0xa4, 0x24, 0xb6, 0x68, 0x12, 0x60, 0x37, 0x5a);
        public static readonly Guid MF_BYTESTREAM_DURATION = new Guid(0xfc35828a, 0x3cb6, 0x460c, 0xa4, 0x24, 0xb6, 0x68, 0x12, 0x60, 0x37, 0x5a);
        public static readonly Guid MF_BYTESTREAM_LAST_MODIFIED_TIME = new Guid(0xfc35828b, 0x3cb6, 0x460c, 0xa4, 0x24, 0xb6, 0x68, 0x12, 0x60, 0x37, 0x5a);
        public static readonly Guid MF_BYTESTREAM_IFO_FILE_URI = new Guid(0xfc35828c, 0x3cb6, 0x460c, 0xa4, 0x24, 0xb6, 0x68, 0x12, 0x60, 0x37, 0x5a);
        public static readonly Guid MF_BYTESTREAM_DLNA_PROFILE_ID = new Guid(0xfc35828d, 0x3cb6, 0x460c, 0xa4, 0x24, 0xb6, 0x68, 0x12, 0x60, 0x37, 0x5a);
        public static readonly Guid MF_BYTESTREAM_EFFECTIVE_URL = new Guid(0x9afa0209, 0x89d1, 0x42af, 0x84, 0x56, 0x1d, 0xe6, 0xb5, 0x62, 0xd6, 0x91);
        public static readonly Guid MF_BYTESTREAM_TRANSCODED = new Guid(0xb6c5c282, 0x4dc9, 0x4db9, 0xab, 0x48, 0xcf, 0x3b, 0x6d, 0x8b, 0xc5, 0xe0);

        // Enhanced Video Renderer Attributes
        public static readonly Guid MF_ACTIVATE_CUSTOM_VIDEO_MIXER_ACTIVATE = new Guid(0xba491361, 0xbe50, 0x451e, 0x95, 0xab, 0x6d, 0x4a, 0xcc, 0xc7, 0xda, 0xd8);
        public static readonly Guid MF_ACTIVATE_CUSTOM_VIDEO_MIXER_CLSID = new Guid(0xba491360, 0xbe50, 0x451e, 0x95, 0xab, 0x6d, 0x4a, 0xcc, 0xc7, 0xda, 0xd8);
        public static readonly Guid MF_ACTIVATE_CUSTOM_VIDEO_MIXER_FLAGS = new Guid(0xba491362, 0xbe50, 0x451e, 0x95, 0xab, 0x6d, 0x4a, 0xcc, 0xc7, 0xda, 0xd8);
        public static readonly Guid MF_ACTIVATE_CUSTOM_VIDEO_PRESENTER_ACTIVATE = new Guid(0xba491365, 0xbe50, 0x451e, 0x95, 0xab, 0x6d, 0x4a, 0xcc, 0xc7, 0xda, 0xd8);
        public static readonly Guid MF_ACTIVATE_CUSTOM_VIDEO_PRESENTER_CLSID = new Guid(0xba491364, 0xbe50, 0x451e, 0x95, 0xab, 0x6d, 0x4a, 0xcc, 0xc7, 0xda, 0xd8);
        public static readonly Guid MF_ACTIVATE_CUSTOM_VIDEO_PRESENTER_FLAGS = new Guid(0xba491366, 0xbe50, 0x451e, 0x95, 0xab, 0x6d, 0x4a, 0xcc, 0xc7, 0xda, 0xd8);
        public static readonly Guid MF_ACTIVATE_VIDEO_WINDOW = new Guid(0x9a2dbbdd, 0xf57e, 0x4162, 0x82, 0xb9, 0x68, 0x31, 0x37, 0x76, 0x82, 0xd3);
        public static readonly Guid MF_SA_REQUIRED_SAMPLE_COUNT = new Guid(0x18802c61, 0x324b, 0x4952, 0xab, 0xd0, 0x17, 0x6f, 0xf5, 0xc6, 0x96, 0xff);
        public static readonly Guid MF_SA_REQUIRED_SAMPLE_COUNT_PROGRESSIVE = new Guid(0xb172d58e, 0xfa77, 0x4e48, 0x8d, 0x2a, 0x1d, 0xf2, 0xd8, 0x50, 0xea, 0xc2);
        public static readonly Guid MF_SA_MINIMUM_OUTPUT_SAMPLE_COUNT = new Guid(0x851745d5, 0xc3d6, 0x476d, 0x95, 0x27, 0x49, 0x8e, 0xf2, 0xd1, 0xd, 0x18);
        public static readonly Guid MF_SA_MINIMUM_OUTPUT_SAMPLE_COUNT_PROGRESSIVE = new Guid(0xf5523a5, 0x1cb2, 0x47c5, 0xa5, 0x50, 0x2e, 0xeb, 0x84, 0xb4, 0xd1, 0x4a);
        public static readonly Guid VIDEO_ZOOM_RECT = new Guid(0x7aaa1638, 0x1b7f, 0x4c93, 0xbd, 0x89, 0x5b, 0x9c, 0x9f, 0xb6, 0xfc, 0xf0);

        // Event Attributes

        public static readonly Guid MF_EVENT_FORMAT_CHANGE_REQUEST_SOURCE_SAR = new Guid(0xb26fbdfd, 0xc32c, 0x41fe, 0x9c, 0xf0, 0x8, 0x3c, 0xd5, 0xc7, 0xf8, 0xa4);

        // MF_EVENT_DO_THINNING {321EA6FB-DAD9-46e4-B31D-D2EAE7090E30}
        public static readonly Guid MF_EVENT_DO_THINNING = new Guid(0x321ea6fb, 0xdad9, 0x46e4, 0xb3, 0x1d, 0xd2, 0xea, 0xe7, 0x9, 0xe, 0x30);

        // MF_EVENT_OUTPUT_NODE {830f1a8b-c060-46dd-a801-1c95dec9b107}
        public static readonly Guid MF_EVENT_OUTPUT_NODE = new Guid(0x830f1a8b, 0xc060, 0x46dd, 0xa8, 0x01, 0x1c, 0x95, 0xde, 0xc9, 0xb1, 0x07);

        // MF_EVENT_MFT_INPUT_STREAM_ID {F29C2CCA-7AE6-42d2-B284-BF837CC874E2}
        public static readonly Guid MF_EVENT_MFT_INPUT_STREAM_ID = new Guid(0xf29c2cca, 0x7ae6, 0x42d2, 0xb2, 0x84, 0xbf, 0x83, 0x7c, 0xc8, 0x74, 0xe2);

        // MF_EVENT_MFT_CONTEXT {B7CD31F1-899E-4b41-80C9-26A896D32977}
        public static readonly Guid MF_EVENT_MFT_CONTEXT = new Guid(0xb7cd31f1, 0x899e, 0x4b41, 0x80, 0xc9, 0x26, 0xa8, 0x96, 0xd3, 0x29, 0x77);

        // MF_EVENT_PRESENTATION_TIME_OFFSET {5AD914D1-9B45-4a8d-A2C0-81D1E50BFB07}
        public static readonly Guid MF_EVENT_PRESENTATION_TIME_OFFSET = new Guid(0x5ad914d1, 0x9b45, 0x4a8d, 0xa2, 0xc0, 0x81, 0xd1, 0xe5, 0xb, 0xfb, 0x7);

        // MF_EVENT_SCRUBSAMPLE_TIME {9AC712B3-DCB8-44d5-8D0C-37455A2782E3}
        public static readonly Guid MF_EVENT_SCRUBSAMPLE_TIME = new Guid(0x9ac712b3, 0xdcb8, 0x44d5, 0x8d, 0xc, 0x37, 0x45, 0x5a, 0x27, 0x82, 0xe3);

        // MF_EVENT_SESSIONCAPS {7E5EBCD0-11B8-4abe-AFAD-10F6599A7F42}
        public static readonly Guid MF_EVENT_SESSIONCAPS = new Guid(0x7e5ebcd0, 0x11b8, 0x4abe, 0xaf, 0xad, 0x10, 0xf6, 0x59, 0x9a, 0x7f, 0x42);

        // MF_EVENT_SESSIONCAPS_DELTA {7E5EBCD1-11B8-4abe-AFAD-10F6599A7F42}
        // Type: UINT32
        public static readonly Guid MF_EVENT_SESSIONCAPS_DELTA = new Guid(0x7e5ebcd1, 0x11b8, 0x4abe, 0xaf, 0xad, 0x10, 0xf6, 0x59, 0x9a, 0x7f, 0x42);

        // MF_EVENT_SOURCE_ACTUAL_START {a8cc55a9-6b31-419f-845d-ffb351a2434b}
        public static readonly Guid MF_EVENT_SOURCE_ACTUAL_START = new Guid(0xa8cc55a9, 0x6b31, 0x419f, 0x84, 0x5d, 0xff, 0xb3, 0x51, 0xa2, 0x43, 0x4b);

        // MF_EVENT_SOURCE_CHARACTERISTICS {47DB8490-8B22-4f52-AFDA-9CE1B2D3CFA8}
        public static readonly Guid MF_EVENT_SOURCE_CHARACTERISTICS = new Guid(0x47db8490, 0x8b22, 0x4f52, 0xaf, 0xda, 0x9c, 0xe1, 0xb2, 0xd3, 0xcf, 0xa8);

        // MF_EVENT_SOURCE_CHARACTERISTICS_OLD {47DB8491-8B22-4f52-AFDA-9CE1B2D3CFA8}
        public static readonly Guid MF_EVENT_SOURCE_CHARACTERISTICS_OLD = new Guid(0x47db8491, 0x8b22, 0x4f52, 0xaf, 0xda, 0x9c, 0xe1, 0xb2, 0xd3, 0xcf, 0xa8);

        // MF_EVENT_SOURCE_FAKE_START {a8cc55a7-6b31-419f-845d-ffb351a2434b}
        public static readonly Guid MF_EVENT_SOURCE_FAKE_START = new Guid(0xa8cc55a7, 0x6b31, 0x419f, 0x84, 0x5d, 0xff, 0xb3, 0x51, 0xa2, 0x43, 0x4b);

        // MF_EVENT_SOURCE_PROJECTSTART {a8cc55a8-6b31-419f-845d-ffb351a2434b}
        public static readonly Guid MF_EVENT_SOURCE_PROJECTSTART = new Guid(0xa8cc55a8, 0x6b31, 0x419f, 0x84, 0x5d, 0xff, 0xb3, 0x51, 0xa2, 0x43, 0x4b);

        // MF_EVENT_SOURCE_TOPOLOGY_CANCELED {DB62F650-9A5E-4704-ACF3-563BC6A73364}
        public static readonly Guid MF_EVENT_SOURCE_TOPOLOGY_CANCELED = new Guid(0xdb62f650, 0x9a5e, 0x4704, 0xac, 0xf3, 0x56, 0x3b, 0xc6, 0xa7, 0x33, 0x64);

        // MF_EVENT_START_PRESENTATION_TIME {5AD914D0-9B45-4a8d-A2C0-81D1E50BFB07}
        public static readonly Guid MF_EVENT_START_PRESENTATION_TIME = new Guid(0x5ad914d0, 0x9b45, 0x4a8d, 0xa2, 0xc0, 0x81, 0xd1, 0xe5, 0xb, 0xfb, 0x7);

        // MF_EVENT_START_PRESENTATION_TIME_AT_OUTPUT {5AD914D2-9B45-4a8d-A2C0-81D1E50BFB07}
        public static readonly Guid MF_EVENT_START_PRESENTATION_TIME_AT_OUTPUT = new Guid(0x5ad914d2, 0x9b45, 0x4a8d, 0xa2, 0xc0, 0x81, 0xd1, 0xe5, 0xb, 0xfb, 0x7);

        // MF_EVENT_TOPOLOGY_STATUS {30C5018D-9A53-454b-AD9E-6D5F8FA7C43B}
        public static readonly Guid MF_EVENT_TOPOLOGY_STATUS = new Guid(0x30c5018d, 0x9a53, 0x454b, 0xad, 0x9e, 0x6d, 0x5f, 0x8f, 0xa7, 0xc4, 0x3b);

        public static readonly Guid MF_SESSION_APPROX_EVENT_OCCURRENCE_TIME = new Guid(0x190e852f, 0x6238, 0x42d1, 0xb5, 0xaf, 0x69, 0xea, 0x33, 0x8e, 0xf8, 0x50);

        // Media Session Attributes

        public static readonly Guid MF_SESSION_CONTENT_PROTECTION_MANAGER = new Guid(0x1e83d482, 0x1f1c, 0x4571, 0x84, 0x5, 0x88, 0xf4, 0xb2, 0x18, 0x1f, 0x74);
        public static readonly Guid MF_SESSION_GLOBAL_TIME = new Guid(0x1e83d482, 0x1f1c, 0x4571, 0x84, 0x5, 0x88, 0xf4, 0xb2, 0x18, 0x1f, 0x72);
        public static readonly Guid MF_SESSION_QUALITY_MANAGER = new Guid(0x1e83d482, 0x1f1c, 0x4571, 0x84, 0x5, 0x88, 0xf4, 0xb2, 0x18, 0x1f, 0x73);
        public static readonly Guid MF_SESSION_REMOTE_SOURCE_MODE = new Guid(0xf4033ef4, 0x9bb3, 0x4378, 0x94, 0x1f, 0x85, 0xa0, 0x85, 0x6b, 0xc2, 0x44);
        public static readonly Guid MF_SESSION_SERVER_CONTEXT = new Guid(0xafe5b291, 0x50fa, 0x46e8, 0xb9, 0xbe, 0xc, 0xc, 0x3c, 0xe4, 0xb3, 0xa5);
        public static readonly Guid MF_SESSION_TOPOLOADER = new Guid(0x1e83d482, 0x1f1c, 0x4571, 0x84, 0x5, 0x88, 0xf4, 0xb2, 0x18, 0x1f, 0x71);

        // Media Type Attributes

        // {48eba18e-f8c9-4687-bf11-0a74c9f96a8f}   MF_MT_MAJOR_TYPE                {GUID}
        public static readonly Guid MF_MT_MAJOR_TYPE = new Guid(0x48eba18e, 0xf8c9, 0x4687, 0xbf, 0x11, 0x0a, 0x74, 0xc9, 0xf9, 0x6a, 0x8f);

        // {f7e34c9a-42e8-4714-b74b-cb29d72c35e5}   MF_MT_SUBTYPE                   {GUID}
        public static readonly Guid MF_MT_SUBTYPE = new Guid(0xf7e34c9a, 0x42e8, 0x4714, 0xb7, 0x4b, 0xcb, 0x29, 0xd7, 0x2c, 0x35, 0xe5);

        // {c9173739-5e56-461c-b713-46fb995cb95f}   MF_MT_ALL_SAMPLES_INDEPENDENT   {UINT32 (BOOL)}
        public static readonly Guid MF_MT_ALL_SAMPLES_INDEPENDENT = new Guid(0xc9173739, 0x5e56, 0x461c, 0xb7, 0x13, 0x46, 0xfb, 0x99, 0x5c, 0xb9, 0x5f);

        // {b8ebefaf-b718-4e04-b0a9-116775e3321b}   MF_MT_FIXED_SIZE_SAMPLES        {UINT32 (BOOL)}
        public static readonly Guid MF_MT_FIXED_SIZE_SAMPLES = new Guid(0xb8ebefaf, 0xb718, 0x4e04, 0xb0, 0xa9, 0x11, 0x67, 0x75, 0xe3, 0x32, 0x1b);

        // {3afd0cee-18f2-4ba5-a110-8bea502e1f92}   MF_MT_COMPRESSED                {UINT32 (BOOL)}
        public static readonly Guid MF_MT_COMPRESSED = new Guid(0x3afd0cee, 0x18f2, 0x4ba5, 0xa1, 0x10, 0x8b, 0xea, 0x50, 0x2e, 0x1f, 0x92);

        // {dad3ab78-1990-408b-bce2-eba673dacc10}   MF_MT_SAMPLE_SIZE               {UINT32}
        public static readonly Guid MF_MT_SAMPLE_SIZE = new Guid(0xdad3ab78, 0x1990, 0x408b, 0xbc, 0xe2, 0xeb, 0xa6, 0x73, 0xda, 0xcc, 0x10);

        // 4d3f7b23-d02f-4e6c-9bee-e4bf2c6c695d     MF_MT_WRAPPED_TYPE              {Blob}
        public static readonly Guid MF_MT_WRAPPED_TYPE = new Guid(0x4d3f7b23, 0xd02f, 0x4e6c, 0x9b, 0xee, 0xe4, 0xbf, 0x2c, 0x6c, 0x69, 0x5d);

        // {37e48bf5-645e-4c5b-89de-ada9e29b696a}   MF_MT_AUDIO_NUM_CHANNELS            {UINT32}
        public static readonly Guid MF_MT_AUDIO_NUM_CHANNELS = new Guid(0x37e48bf5, 0x645e, 0x4c5b, 0x89, 0xde, 0xad, 0xa9, 0xe2, 0x9b, 0x69, 0x6a);

        // {5faeeae7-0290-4c31-9e8a-c534f68d9dba}   MF_MT_AUDIO_SAMPLES_PER_SECOND      {UINT32}
        public static readonly Guid MF_MT_AUDIO_SAMPLES_PER_SECOND = new Guid(0x5faeeae7, 0x0290, 0x4c31, 0x9e, 0x8a, 0xc5, 0x34, 0xf6, 0x8d, 0x9d, 0xba);

        // {fb3b724a-cfb5-4319-aefe-6e42b2406132}   MF_MT_AUDIO_FLOAT_SAMPLES_PER_SECOND {double}
        public static readonly Guid MF_MT_AUDIO_FLOAT_SAMPLES_PER_SECOND = new Guid(0xfb3b724a, 0xcfb5, 0x4319, 0xae, 0xfe, 0x6e, 0x42, 0xb2, 0x40, 0x61, 0x32);

        // {1aab75c8-cfef-451c-ab95-ac034b8e1731}   MF_MT_AUDIO_AVG_BYTES_PER_SECOND    {UINT32}
        public static readonly Guid MF_MT_AUDIO_AVG_BYTES_PER_SECOND = new Guid(0x1aab75c8, 0xcfef, 0x451c, 0xab, 0x95, 0xac, 0x03, 0x4b, 0x8e, 0x17, 0x31);

        // {322de230-9eeb-43bd-ab7a-ff412251541d}   MF_MT_AUDIO_BLOCK_ALIGNMENT         {UINT32}
        public static readonly Guid MF_MT_AUDIO_BLOCK_ALIGNMENT = new Guid(0x322de230, 0x9eeb, 0x43bd, 0xab, 0x7a, 0xff, 0x41, 0x22, 0x51, 0x54, 0x1d);

        // {f2deb57f-40fa-4764-aa33-ed4f2d1ff669}   MF_MT_AUDIO_BITS_PER_SAMPLE         {UINT32}
        public static readonly Guid MF_MT_AUDIO_BITS_PER_SAMPLE = new Guid(0xf2deb57f, 0x40fa, 0x4764, 0xaa, 0x33, 0xed, 0x4f, 0x2d, 0x1f, 0xf6, 0x69);

        // {d9bf8d6a-9530-4b7c-9ddf-ff6fd58bbd06}   MF_MT_AUDIO_VALID_BITS_PER_SAMPLE   {UINT32}
        public static readonly Guid MF_MT_AUDIO_VALID_BITS_PER_SAMPLE = new Guid(0xd9bf8d6a, 0x9530, 0x4b7c, 0x9d, 0xdf, 0xff, 0x6f, 0xd5, 0x8b, 0xbd, 0x06);

        // {aab15aac-e13a-4995-9222-501ea15c6877}   MF_MT_AUDIO_SAMPLES_PER_BLOCK       {UINT32}
        public static readonly Guid MF_MT_AUDIO_SAMPLES_PER_BLOCK = new Guid(0xaab15aac, 0xe13a, 0x4995, 0x92, 0x22, 0x50, 0x1e, 0xa1, 0x5c, 0x68, 0x77);

        // {55fb5765-644a-4caf-8479-938983bb1588}`  MF_MT_AUDIO_CHANNEL_MASK            {UINT32}
        public static readonly Guid MF_MT_AUDIO_CHANNEL_MASK = new Guid(0x55fb5765, 0x644a, 0x4caf, 0x84, 0x79, 0x93, 0x89, 0x83, 0xbb, 0x15, 0x88);

        // {9d62927c-36be-4cf2-b5c4-a3926e3e8711}`  MF_MT_AUDIO_FOLDDOWN_MATRIX         {BLOB, MFFOLDDOWN_MATRIX}
        public static readonly Guid MF_MT_AUDIO_FOLDDOWN_MATRIX = new Guid(0x9d62927c, 0x36be, 0x4cf2, 0xb5, 0xc4, 0xa3, 0x92, 0x6e, 0x3e, 0x87, 0x11);

        // {0x9d62927d-36be-4cf2-b5c4-a3926e3e8711}`  MF_MT_AUDIO_WMADRC_PEAKREF         {UINT32}
        public static readonly Guid MF_MT_AUDIO_WMADRC_PEAKREF = new Guid(0x9d62927d, 0x36be, 0x4cf2, 0xb5, 0xc4, 0xa3, 0x92, 0x6e, 0x3e, 0x87, 0x11);

        // {0x9d62927e-36be-4cf2-b5c4-a3926e3e8711}`  MF_MT_AUDIO_WMADRC_PEAKTARGET        {UINT32}
        public static readonly Guid MF_MT_AUDIO_WMADRC_PEAKTARGET = new Guid(0x9d62927e, 0x36be, 0x4cf2, 0xb5, 0xc4, 0xa3, 0x92, 0x6e, 0x3e, 0x87, 0x11);

        // {0x9d62927f-36be-4cf2-b5c4-a3926e3e8711}`  MF_MT_AUDIO_WMADRC_AVGREF         {UINT32}
        public static readonly Guid MF_MT_AUDIO_WMADRC_AVGREF = new Guid(0x9d62927f, 0x36be, 0x4cf2, 0xb5, 0xc4, 0xa3, 0x92, 0x6e, 0x3e, 0x87, 0x11);

        // {0x9d629280-36be-4cf2-b5c4-a3926e3e8711}`  MF_MT_AUDIO_WMADRC_AVGTARGET      {UINT32}
        public static readonly Guid MF_MT_AUDIO_WMADRC_AVGTARGET = new Guid(0x9d629280, 0x36be, 0x4cf2, 0xb5, 0xc4, 0xa3, 0x92, 0x6e, 0x3e, 0x87, 0x11);

        // {a901aaba-e037-458a-bdf6-545be2074042}   MF_MT_AUDIO_PREFER_WAVEFORMATEX     {UINT32 (BOOL)}
        public static readonly Guid MF_MT_AUDIO_PREFER_WAVEFORMATEX = new Guid(0xa901aaba, 0xe037, 0x458a, 0xbd, 0xf6, 0x54, 0x5b, 0xe2, 0x07, 0x40, 0x42);

        // {BFBABE79-7434-4d1c-94F0-72A3B9E17188} MF_MT_AAC_PAYLOAD_TYPE       {UINT32}
        public static readonly Guid MF_MT_AAC_PAYLOAD_TYPE = new Guid(0xbfbabe79, 0x7434, 0x4d1c, 0x94, 0xf0, 0x72, 0xa3, 0xb9, 0xe1, 0x71, 0x88);

        // {7632F0E6-9538-4d61-ACDA-EA29C8C14456} MF_MT_AAC_AUDIO_PROFILE_LEVEL_INDICATION       {UINT32}
        public static readonly Guid MF_MT_AAC_AUDIO_PROFILE_LEVEL_INDICATION = new Guid(0x7632f0e6, 0x9538, 0x4d61, 0xac, 0xda, 0xea, 0x29, 0xc8, 0xc1, 0x44, 0x56);

        // {1652c33d-d6b2-4012-b834-72030849a37d}   MF_MT_FRAME_SIZE                {UINT64 (HI32(Width),LO32(Height))}
        public static readonly Guid MF_MT_FRAME_SIZE = new Guid(0x1652c33d, 0xd6b2, 0x4012, 0xb8, 0x34, 0x72, 0x03, 0x08, 0x49, 0xa3, 0x7d);

        // {c459a2e8-3d2c-4e44-b132-fee5156c7bb0}   MF_MT_FRAME_RATE                {UINT64 (HI32(Numerator),LO32(Denominator))}
        public static readonly Guid MF_MT_FRAME_RATE = new Guid(0xc459a2e8, 0x3d2c, 0x4e44, 0xb1, 0x32, 0xfe, 0xe5, 0x15, 0x6c, 0x7b, 0xb0);

        // {c6376a1e-8d0a-4027-be45-6d9a0ad39bb6}   MF_MT_PIXEL_ASPECT_RATIO        {UINT64 (HI32(Numerator),LO32(Denominator))}
        public static readonly Guid MF_MT_PIXEL_ASPECT_RATIO = new Guid(0xc6376a1e, 0x8d0a, 0x4027, 0xbe, 0x45, 0x6d, 0x9a, 0x0a, 0xd3, 0x9b, 0xb6);

        // {8772f323-355a-4cc7-bb78-6d61a048ae82}   MF_MT_DRM_FLAGS                 {UINT32 (anyof MFVideoDRMFlags)}
        public static readonly Guid MF_MT_DRM_FLAGS = new Guid(0x8772f323, 0x355a, 0x4cc7, 0xbb, 0x78, 0x6d, 0x61, 0xa0, 0x48, 0xae, 0x82);

        // {24974215-1B7B-41e4-8625-AC469F2DEDAA}   MF_MT_TIMESTAMP_CAN_BE_DTS      {UINT32 (BOOL)}
        public static readonly Guid MF_MT_TIMESTAMP_CAN_BE_DTS = new Guid(0x24974215, 0x1b7b, 0x41e4, 0x86, 0x25, 0xac, 0x46, 0x9f, 0x2d, 0xed, 0xaa);

        // {A20AF9E8-928A-4B26-AAA9-F05C74CAC47C}   MF_MT_MPEG2_STANDARD            {UINT32 (0 for default MPEG2, 1  to use ATSC standard, 2 to use DVB standard, 3 to use ARIB standard)}
        public static readonly Guid MF_MT_MPEG2_STANDARD = new Guid(0xa20af9e8, 0x928a, 0x4b26, 0xaa, 0xa9, 0xf0, 0x5c, 0x74, 0xca, 0xc4, 0x7c);

        // {5229BA10-E29D-4F80-A59C-DF4F180207D2}   MF_MT_MPEG2_TIMECODE            {UINT32 (0 for no timecode, 1 to append an 4 byte timecode to the front of each transport packet)}
        public static readonly Guid MF_MT_MPEG2_TIMECODE = new Guid(0x5229ba10, 0xe29d, 0x4f80, 0xa5, 0x9c, 0xdf, 0x4f, 0x18, 0x2, 0x7, 0xd2);

        // {825D55E4-4F12-4197-9EB3-59B6E4710F06}   MF_MT_MPEG2_CONTENT_PACKET      {UINT32 (0 for no content packet, 1 to append a 14 byte Content Packet header according to the ARIB specification to the beginning a transport packet at 200-1000 ms intervals.)}
        public static readonly Guid MF_MT_MPEG2_CONTENT_PACKET = new Guid(0x825d55e4, 0x4f12, 0x4197, 0x9e, 0xb3, 0x59, 0xb6, 0xe4, 0x71, 0xf, 0x6);

        //
        // VIDEO - H264 extra data
        //

        // {F5929986-4C45-4FBB-BB49-6CC534D05B9B}  {UINT32, UVC 1.5 H.264 format descriptor: bMaxCodecConfigDelay}
        public static readonly Guid MF_MT_H264_MAX_CODEC_CONFIG_DELAY = new Guid(0xf5929986, 0x4c45, 0x4fbb, 0xbb, 0x49, 0x6c, 0xc5, 0x34, 0xd0, 0x5b, 0x9b);

        // {C8BE1937-4D64-4549-8343-A8086C0BFDA5} {UINT32, UVC 1.5 H.264 format descriptor: bmSupportedSliceModes}
        public static readonly Guid MF_MT_H264_SUPPORTED_SLICE_MODES = new Guid(0xc8be1937, 0x4d64, 0x4549, 0x83, 0x43, 0xa8, 0x8, 0x6c, 0xb, 0xfd, 0xa5);

        // {89A52C01-F282-48D2-B522-22E6AE633199} {UINT32, UVC 1.5 H.264 format descriptor: bmSupportedSyncFrameTypes}
        public static readonly Guid MF_MT_H264_SUPPORTED_SYNC_FRAME_TYPES = new Guid(0x89a52c01, 0xf282, 0x48d2, 0xb5, 0x22, 0x22, 0xe6, 0xae, 0x63, 0x31, 0x99);

        // {E3854272-F715-4757-BA90-1B696C773457} {UINT32, UVC 1.5 H.264 format descriptor: bResolutionScaling}
        public static readonly Guid MF_MT_H264_RESOLUTION_SCALING = new Guid(0xe3854272, 0xf715, 0x4757, 0xba, 0x90, 0x1b, 0x69, 0x6c, 0x77, 0x34, 0x57);

        // {9EA2D63D-53F0-4A34-B94E-9DE49A078CB3} {UINT32, UVC 1.5 H.264 format descriptor: bSimulcastSupport}
        public static readonly Guid MF_MT_H264_SIMULCAST_SUPPORT = new Guid(0x9ea2d63d, 0x53f0, 0x4a34, 0xb9, 0x4e, 0x9d, 0xe4, 0x9a, 0x7, 0x8c, 0xb3);

        // {6A8AC47E-519C-4F18-9BB3-7EEAAEA5594D} {UINT32, UVC 1.5 H.264 format descriptor: bmSupportedRateControlModes}
        public static readonly Guid MF_MT_H264_SUPPORTED_RATE_CONTROL_MODES = new Guid(0x6a8ac47e, 0x519c, 0x4f18, 0x9b, 0xb3, 0x7e, 0xea, 0xae, 0xa5, 0x59, 0x4d);

        // {45256D30-7215-4576-9336-B0F1BCD59BB2}  {Blob of size 20 * sizeof(WORD), UVC 1.5 H.264 format descriptor: wMaxMBperSec*}
        public static readonly Guid MF_MT_H264_MAX_MB_PER_SEC = new Guid(0x45256d30, 0x7215, 0x4576, 0x93, 0x36, 0xb0, 0xf1, 0xbc, 0xd5, 0x9b, 0xb2);

        // {60B1A998-DC01-40CE-9736-ABA845A2DBDC}         {UINT32, UVC 1.5 H.264 frame descriptor: bmSupportedUsages}
        public static readonly Guid MF_MT_H264_SUPPORTED_USAGES = new Guid(0x60b1a998, 0xdc01, 0x40ce, 0x97, 0x36, 0xab, 0xa8, 0x45, 0xa2, 0xdb, 0xdc);

        // {BB3BD508-490A-11E0-99E4-1316DFD72085}         {UINT32, UVC 1.5 H.264 frame descriptor: bmCapabilities}
        public static readonly Guid MF_MT_H264_CAPABILITIES = new Guid(0xbb3bd508, 0x490a, 0x11e0, 0x99, 0xe4, 0x13, 0x16, 0xdf, 0xd7, 0x20, 0x85);

        // {F8993ABE-D937-4A8F-BBCA-6966FE9E1152}         {UINT32, UVC 1.5 H.264 frame descriptor: bmSVCCapabilities}
        public static readonly Guid MF_MT_H264_SVC_CAPABILITIES = new Guid(0xf8993abe, 0xd937, 0x4a8f, 0xbb, 0xca, 0x69, 0x66, 0xfe, 0x9e, 0x11, 0x52);

        // {359CE3A5-AF00-49CA-A2F4-2AC94CA82B61}         {UINT32, UVC 1.5 H.264 Probe/Commit Control: bUsage}
        public static readonly Guid MF_MT_H264_USAGE = new Guid(0x359ce3a5, 0xaf00, 0x49ca, 0xa2, 0xf4, 0x2a, 0xc9, 0x4c, 0xa8, 0x2b, 0x61);

        //{705177D8-45CB-11E0-AC7D-B91CE0D72085}          {UINT32, UVC 1.5 H.264 Probe/Commit Control: bmRateControlModes}
        public static readonly Guid MF_MT_H264_RATE_CONTROL_MODES = new Guid(0x705177d8, 0x45cb, 0x11e0, 0xac, 0x7d, 0xb9, 0x1c, 0xe0, 0xd7, 0x20, 0x85);

        //{85E299B2-90E3-4FE8-B2F5-C067E0BFE57A}          {UINT64, UVC 1.5 H.264 Probe/Commit Control: bmLayoutPerStream}
        public static readonly Guid MF_MT_H264_LAYOUT_PER_STREAM = new Guid(0x85e299b2, 0x90e3, 0x4fe8, 0xb2, 0xf5, 0xc0, 0x67, 0xe0, 0xbf, 0xe5, 0x7a);

        // {4d0e73e5-80ea-4354-a9d0-1176ceb028ea}   MF_MT_PAD_CONTROL_FLAGS         {UINT32 (oneof MFVideoPadFlags)}
        public static readonly Guid MF_MT_PAD_CONTROL_FLAGS = new Guid(0x4d0e73e5, 0x80ea, 0x4354, 0xa9, 0xd0, 0x11, 0x76, 0xce, 0xb0, 0x28, 0xea);

        // {68aca3cc-22d0-44e6-85f8-28167197fa38}   MF_MT_SOURCE_CONTENT_HINT       {UINT32 (oneof MFVideoSrcContentHintFlags)}
        public static readonly Guid MF_MT_SOURCE_CONTENT_HINT = new Guid(0x68aca3cc, 0x22d0, 0x44e6, 0x85, 0xf8, 0x28, 0x16, 0x71, 0x97, 0xfa, 0x38);

        // {65df2370-c773-4c33-aa64-843e068efb0c}   MF_MT_CHROMA_SITING             {UINT32 (anyof MFVideoChromaSubsampling)}
        public static readonly Guid MF_MT_VIDEO_CHROMA_SITING = new Guid(0x65df2370, 0xc773, 0x4c33, 0xaa, 0x64, 0x84, 0x3e, 0x06, 0x8e, 0xfb, 0x0c);

        // {e2724bb8-e676-4806-b4b2-a8d6efb44ccd}   MF_MT_INTERLACE_MODE            {UINT32 (oneof MFVideoInterlaceMode)}
        public static readonly Guid MF_MT_INTERLACE_MODE = new Guid(0xe2724bb8, 0xe676, 0x4806, 0xb4, 0xb2, 0xa8, 0xd6, 0xef, 0xb4, 0x4c, 0xcd);

        // {5fb0fce9-be5c-4935-a811-ec838f8eed93}   MF_MT_TRANSFER_FUNCTION         {UINT32 (oneof MFVideoTransferFunction)}
        public static readonly Guid MF_MT_TRANSFER_FUNCTION = new Guid(0x5fb0fce9, 0xbe5c, 0x4935, 0xa8, 0x11, 0xec, 0x83, 0x8f, 0x8e, 0xed, 0x93);

        // {dbfbe4d7-0740-4ee0-8192-850ab0e21935}   MF_MT_VIDEO_PRIMARIES           {UINT32 (oneof MFVideoPrimaries)}
        public static readonly Guid MF_MT_VIDEO_PRIMARIES = new Guid(0xdbfbe4d7, 0x0740, 0x4ee0, 0x81, 0x92, 0x85, 0x0a, 0xb0, 0xe2, 0x19, 0x35);

        // {47537213-8cfb-4722-aa34-fbc9e24d77b8}   MF_MT_CUSTOM_VIDEO_PRIMARIES    {BLOB (MT_CUSTOM_VIDEO_PRIMARIES)}
        public static readonly Guid MF_MT_CUSTOM_VIDEO_PRIMARIES = new Guid(0x47537213, 0x8cfb, 0x4722, 0xaa, 0x34, 0xfb, 0xc9, 0xe2, 0x4d, 0x77, 0xb8);

        // {3e23d450-2c75-4d25-a00e-b91670d12327}   MF_MT_YUV_MATRIX                {UINT32 (oneof MFVideoTransferMatrix)}
        public static readonly Guid MF_MT_YUV_MATRIX = new Guid(0x3e23d450, 0x2c75, 0x4d25, 0xa0, 0x0e, 0xb9, 0x16, 0x70, 0xd1, 0x23, 0x27);

        // {53a0529c-890b-4216-8bf9-599367ad6d20}   MF_MT_VIDEO_LIGHTING            {UINT32 (oneof MFVideoLighting)}
        public static readonly Guid MF_MT_VIDEO_LIGHTING = new Guid(0x53a0529c, 0x890b, 0x4216, 0x8b, 0xf9, 0x59, 0x93, 0x67, 0xad, 0x6d, 0x20);

        // {c21b8ee5-b956-4071-8daf-325edf5cab11}   MF_MT_VIDEO_NOMINAL_RANGE       {UINT32 (oneof MFNominalRange)}
        public static readonly Guid MF_MT_VIDEO_NOMINAL_RANGE = new Guid(0xc21b8ee5, 0xb956, 0x4071, 0x8d, 0xaf, 0x32, 0x5e, 0xdf, 0x5c, 0xab, 0x11);

        // {66758743-7e5f-400d-980a-aa8596c85696}   MF_MT_GEOMETRIC_APERTURE        {BLOB (MFVideoArea)}
        public static readonly Guid MF_MT_GEOMETRIC_APERTURE = new Guid(0x66758743, 0x7e5f, 0x400d, 0x98, 0x0a, 0xaa, 0x85, 0x96, 0xc8, 0x56, 0x96);

        // {d7388766-18fe-48c6-a177-ee894867c8c4}   MF_MT_MINIMUM_DISPLAY_APERTURE  {BLOB (MFVideoArea)}
        public static readonly Guid MF_MT_MINIMUM_DISPLAY_APERTURE = new Guid(0xd7388766, 0x18fe, 0x48c6, 0xa1, 0x77, 0xee, 0x89, 0x48, 0x67, 0xc8, 0xc4);

        // {79614dde-9187-48fb-b8c7-4d52689de649}   MF_MT_PAN_SCAN_APERTURE         {BLOB (MFVideoArea)}
        public static readonly Guid MF_MT_PAN_SCAN_APERTURE = new Guid(0x79614dde, 0x9187, 0x48fb, 0xb8, 0xc7, 0x4d, 0x52, 0x68, 0x9d, 0xe6, 0x49);

        // {4b7f6bc3-8b13-40b2-a993-abf630b8204e}   MF_MT_PAN_SCAN_ENABLED          {UINT32 (BOOL)}
        public static readonly Guid MF_MT_PAN_SCAN_ENABLED = new Guid(0x4b7f6bc3, 0x8b13, 0x40b2, 0xa9, 0x93, 0xab, 0xf6, 0x30, 0xb8, 0x20, 0x4e);

        // {20332624-fb0d-4d9e-bd0d-cbf6786c102e}   MF_MT_AVG_BITRATE               {UINT32}
        public static readonly Guid MF_MT_AVG_BITRATE = new Guid(0x20332624, 0xfb0d, 0x4d9e, 0xbd, 0x0d, 0xcb, 0xf6, 0x78, 0x6c, 0x10, 0x2e);

        // {799cabd6-3508-4db4-a3c7-569cd533deb1}   MF_MT_AVG_BIT_ERROR_RATE        {UINT32}
        public static readonly Guid MF_MT_AVG_BIT_ERROR_RATE = new Guid(0x799cabd6, 0x3508, 0x4db4, 0xa3, 0xc7, 0x56, 0x9c, 0xd5, 0x33, 0xde, 0xb1);

        // {c16eb52b-73a1-476f-8d62-839d6a020652}   MF_MT_MAX_KEYFRAME_SPACING      {UINT32}
        public static readonly Guid MF_MT_MAX_KEYFRAME_SPACING = new Guid(0xc16eb52b, 0x73a1, 0x476f, 0x8d, 0x62, 0x83, 0x9d, 0x6a, 0x02, 0x06, 0x52);

        // {644b4e48-1e02-4516-b0eb-c01ca9d49ac6}   MF_MT_DEFAULT_STRIDE            {UINT32 (INT32)} // in bytes
        public static readonly Guid MF_MT_DEFAULT_STRIDE = new Guid(0x644b4e48, 0x1e02, 0x4516, 0xb0, 0xeb, 0xc0, 0x1c, 0xa9, 0xd4, 0x9a, 0xc6);

        // {6d283f42-9846-4410-afd9-654d503b1a54}   MF_MT_PALETTE                   {BLOB (array of MFPaletteEntry - usually 256)}
        public static readonly Guid MF_MT_PALETTE = new Guid(0x6d283f42, 0x9846, 0x4410, 0xaf, 0xd9, 0x65, 0x4d, 0x50, 0x3b, 0x1a, 0x54);

        // {b6bc765f-4c3b-40a4-bd51-2535b66fe09d}   MF_MT_USER_DATA                 {BLOB}
        public static readonly Guid MF_MT_USER_DATA = new Guid(0xb6bc765f, 0x4c3b, 0x40a4, 0xbd, 0x51, 0x25, 0x35, 0xb6, 0x6f, 0xe0, 0x9d);

        // {73d1072d-1870-4174-a063-29ff4ff6c11e}
        public static readonly Guid MF_MT_AM_FORMAT_TYPE = new Guid(0x73d1072d, 0x1870, 0x4174, 0xa0, 0x63, 0x29, 0xff, 0x4f, 0xf6, 0xc1, 0x1e);

        // {91f67885-4333-4280-97cd-bd5a6c03a06e}   MF_MT_MPEG_START_TIME_CODE      {UINT32}
        public static readonly Guid MF_MT_MPEG_START_TIME_CODE = new Guid(0x91f67885, 0x4333, 0x4280, 0x97, 0xcd, 0xbd, 0x5a, 0x6c, 0x03, 0xa0, 0x6e);

        // {ad76a80b-2d5c-4e0b-b375-64e520137036}   MF_MT_MPEG2_PROFILE             {UINT32 (oneof AM_MPEG2Profile)}
        public static readonly Guid MF_MT_MPEG2_PROFILE = new Guid(0xad76a80b, 0x2d5c, 0x4e0b, 0xb3, 0x75, 0x64, 0xe5, 0x20, 0x13, 0x70, 0x36);

        // {96f66574-11c5-4015-8666-bff516436da7}   MF_MT_MPEG2_LEVEL               {UINT32 (oneof AM_MPEG2Level)}
        public static readonly Guid MF_MT_MPEG2_LEVEL = new Guid(0x96f66574, 0x11c5, 0x4015, 0x86, 0x66, 0xbf, 0xf5, 0x16, 0x43, 0x6d, 0xa7);

        // {31e3991d-f701-4b2f-b426-8ae3bda9e04b}   MF_MT_MPEG2_FLAGS               {UINT32 (anyof AMMPEG2_xxx flags)}
        public static readonly Guid MF_MT_MPEG2_FLAGS = new Guid(0x31e3991d, 0xf701, 0x4b2f, 0xb4, 0x26, 0x8a, 0xe3, 0xbd, 0xa9, 0xe0, 0x4b);

        // {3c036de7-3ad0-4c9e-9216-ee6d6ac21cb3}   MF_MT_MPEG_SEQUENCE_HEADER      {BLOB}
        public static readonly Guid MF_MT_MPEG_SEQUENCE_HEADER = new Guid(0x3c036de7, 0x3ad0, 0x4c9e, 0x92, 0x16, 0xee, 0x6d, 0x6a, 0xc2, 0x1c, 0xb3);

        // {84bd5d88-0fb8-4ac8-be4b-a8848bef98f3}   MF_MT_DV_AAUX_SRC_PACK_0        {UINT32}
        public static readonly Guid MF_MT_DV_AAUX_SRC_PACK_0 = new Guid(0x84bd5d88, 0x0fb8, 0x4ac8, 0xbe, 0x4b, 0xa8, 0x84, 0x8b, 0xef, 0x98, 0xf3);

        // {f731004e-1dd1-4515-aabe-f0c06aa536ac}   MF_MT_DV_AAUX_CTRL_PACK_0       {UINT32}
        public static readonly Guid MF_MT_DV_AAUX_CTRL_PACK_0 = new Guid(0xf731004e, 0x1dd1, 0x4515, 0xaa, 0xbe, 0xf0, 0xc0, 0x6a, 0xa5, 0x36, 0xac);

        // {720e6544-0225-4003-a651-0196563a958e}   MF_MT_DV_AAUX_SRC_PACK_1        {UINT32}
        public static readonly Guid MF_MT_DV_AAUX_SRC_PACK_1 = new Guid(0x720e6544, 0x0225, 0x4003, 0xa6, 0x51, 0x01, 0x96, 0x56, 0x3a, 0x95, 0x8e);

        // {cd1f470d-1f04-4fe0-bfb9-d07ae0386ad8}   MF_MT_DV_AAUX_CTRL_PACK_1       {UINT32}
        public static readonly Guid MF_MT_DV_AAUX_CTRL_PACK_1 = new Guid(0xcd1f470d, 0x1f04, 0x4fe0, 0xbf, 0xb9, 0xd0, 0x7a, 0xe0, 0x38, 0x6a, 0xd8);

        // {41402d9d-7b57-43c6-b129-2cb997f15009}   MF_MT_DV_VAUX_SRC_PACK          {UINT32}
        public static readonly Guid MF_MT_DV_VAUX_SRC_PACK = new Guid(0x41402d9d, 0x7b57, 0x43c6, 0xb1, 0x29, 0x2c, 0xb9, 0x97, 0xf1, 0x50, 0x09);

        // {2f84e1c4-0da1-4788-938e-0dfbfbb34b48}   MF_MT_DV_VAUX_CTRL_PACK         {UINT32}
        public static readonly Guid MF_MT_DV_VAUX_CTRL_PACK = new Guid(0x2f84e1c4, 0x0da1, 0x4788, 0x93, 0x8e, 0x0d, 0xfb, 0xfb, 0xb3, 0x4b, 0x48);

        // {5315d8a0-87c5-4697-b793-666c67c49b}         MF_MT_VIDEO_3D_FORMAT           {UINT32 (anyof MFVideo3DFormat)}
        public static readonly Guid MF_MT_VIDEO_3D_FORMAT = new Guid(0x5315d8a0, 0x87c5, 0x4697, 0xb7, 0x93, 0x66, 0x6, 0xc6, 0x7c, 0x4, 0x9b);

        // {BB077E8A-DCBF-42eb-AF60-418DF98AA495}       MF_MT_VIDEO_3D_NUM_VIEW         {UINT32}
        public static readonly Guid MF_MT_VIDEO_3D_NUM_VIEWS = new Guid(0xbb077e8a, 0xdcbf, 0x42eb, 0xaf, 0x60, 0x41, 0x8d, 0xf9, 0x8a, 0xa4, 0x95);

        // {6D4B7BFF-5629-4404-948C-C634F4CE26D4}       MF_MT_VIDEO_3D_LEFT_IS_BASE     {UINT32}
        public static readonly Guid MF_MT_VIDEO_3D_LEFT_IS_BASE = new Guid(0x6d4b7bff, 0x5629, 0x4404, 0x94, 0x8c, 0xc6, 0x34, 0xf4, 0xce, 0x26, 0xd4);

        // {EC298493-0ADA-4ea1-A4FE-CBBD36CE9331}       MF_MT_VIDEO_3D_FIRST_IS_LEFT    {UINT32 (BOOL)}
        public static readonly Guid MF_MT_VIDEO_3D_FIRST_IS_LEFT = new Guid(0xec298493, 0xada, 0x4ea1, 0xa4, 0xfe, 0xcb, 0xbd, 0x36, 0xce, 0x93, 0x31);

        public static readonly Guid MF_MT_VIDEO_ROTATION = new Guid(0xc380465d, 0x2271, 0x428c, 0x9b, 0x83, 0xec, 0xea, 0x3b, 0x4a, 0x85, 0xc1);

        // Sample Attributes

        public static readonly Guid MFSampleExtension_DecodeTimestamp = new Guid(0x73a954d4, 0x9e2, 0x4861, 0xbe, 0xfc, 0x94, 0xbd, 0x97, 0xc0, 0x8e, 0x6e);

        public static readonly Guid MFSampleExtension_VideoEncodeQP = new Guid(0xb2efe478, 0xf979, 0x4c66, 0xb9, 0x5e, 0xee, 0x2b, 0x82, 0xc8, 0x2f, 0x36);

        public static readonly Guid MFSampleExtension_VideoEncodePictureType = new Guid(0x973704e6, 0xcd14, 0x483c, 0x8f, 0x20, 0xc9, 0xfc, 0x9, 0x28, 0xba, 0xd5);

        public static readonly Guid MFSampleExtension_FrameCorruption = new Guid(0xb4dd4a8c, 0xbeb, 0x44c4, 0x8b, 0x75, 0xb0, 0x2b, 0x91, 0x3b, 0x4, 0xf0);

        // {941ce0a3-6ae3-4dda-9a08-a64298340617}   MFSampleExtension_BottomFieldFirst
        public static readonly Guid MFSampleExtension_BottomFieldFirst = new Guid(0x941ce0a3, 0x6ae3, 0x4dda, 0x9a, 0x08, 0xa6, 0x42, 0x98, 0x34, 0x06, 0x17);

        // MFSampleExtension_CleanPoint {9cdf01d8-a0f0-43ba-b077-eaa06cbd728a}
        public static readonly Guid MFSampleExtension_CleanPoint = new Guid(0x9cdf01d8, 0xa0f0, 0x43ba, 0xb0, 0x77, 0xea, 0xa0, 0x6c, 0xbd, 0x72, 0x8a);

        // {6852465a-ae1c-4553-8e9b-c3420fcb1637}   MFSampleExtension_DerivedFromTopField
        public static readonly Guid MFSampleExtension_DerivedFromTopField = new Guid(0x6852465a, 0xae1c, 0x4553, 0x8e, 0x9b, 0xc3, 0x42, 0x0f, 0xcb, 0x16, 0x37);

        // MFSampleExtension_Discontinuity {9cdf01d9-a0f0-43ba-b077-eaa06cbd728a}
        public static readonly Guid MFSampleExtension_Discontinuity = new Guid(0x9cdf01d9, 0xa0f0, 0x43ba, 0xb0, 0x77, 0xea, 0xa0, 0x6c, 0xbd, 0x72, 0x8a);

        // {b1d5830a-deb8-40e3-90fa-389943716461}   MFSampleExtension_Interlaced
        public static readonly Guid MFSampleExtension_Interlaced = new Guid(0xb1d5830a, 0xdeb8, 0x40e3, 0x90, 0xfa, 0x38, 0x99, 0x43, 0x71, 0x64, 0x61);

        // {304d257c-7493-4fbd-b149-9228de8d9a99}   MFSampleExtension_RepeatFirstField
        public static readonly Guid MFSampleExtension_RepeatFirstField = new Guid(0x304d257c, 0x7493, 0x4fbd, 0xb1, 0x49, 0x92, 0x28, 0xde, 0x8d, 0x9a, 0x99);

        // {9d85f816-658b-455a-bde0-9fa7e15ab8f9}   MFSampleExtension_SingleField
        public static readonly Guid MFSampleExtension_SingleField = new Guid(0x9d85f816, 0x658b, 0x455a, 0xbd, 0xe0, 0x9f, 0xa7, 0xe1, 0x5a, 0xb8, 0xf9);

        // MFSampleExtension_Token {8294da66-f328-4805-b551-00deb4c57a61}
        public static readonly Guid MFSampleExtension_Token = new Guid(0x8294da66, 0xf328, 0x4805, 0xb5, 0x51, 0x00, 0xde, 0xb4, 0xc5, 0x7a, 0x61);

        // MFSampleExtension_3DVideo                    {F86F97A4-DD54-4e2e-9A5E-55FC2D74A005}
        public static readonly Guid MFSampleExtension_3DVideo = new Guid(0xf86f97a4, 0xdd54, 0x4e2e, 0x9a, 0x5e, 0x55, 0xfc, 0x2d, 0x74, 0xa0, 0x05);

        // MFSampleExtension_3DVideo_SampleFormat       {08671772-E36F-4cff-97B3-D72E20987A48}
        public static readonly Guid MFSampleExtension_3DVideo_SampleFormat = new Guid(0x8671772, 0xe36f, 0x4cff, 0x97, 0xb3, 0xd7, 0x2e, 0x20, 0x98, 0x7a, 0x48);

        // Sample Grabber Sink Attributes
        public static readonly Guid MF_SAMPLEGRABBERSINK_SAMPLE_TIME_OFFSET = new Guid(0x62e3d776, 0x8100, 0x4e03, 0xa6, 0xe8, 0xbd, 0x38, 0x57, 0xac, 0x9c, 0x47);

        // Stream descriptor Attributes

        public static readonly Guid MF_SD_LANGUAGE = new Guid(0xaf2180, 0xbdc2, 0x423c, 0xab, 0xca, 0xf5, 0x3, 0x59, 0x3b, 0xc1, 0x21);
        public static readonly Guid MF_SD_PROTECTED = new Guid(0xaf2181, 0xbdc2, 0x423c, 0xab, 0xca, 0xf5, 0x3, 0x59, 0x3b, 0xc1, 0x21);
        public static readonly Guid MF_SD_SAMI_LANGUAGE = new Guid(0x36fcb98a, 0x6cd0, 0x44cb, 0xac, 0xb9, 0xa8, 0xf5, 0x60, 0xd, 0xd0, 0xbb);

        // Topology Attributes
        public static readonly Guid MF_TOPOLOGY_NO_MARKIN_MARKOUT = new Guid(0x7ed3f804, 0x86bb, 0x4b3f, 0xb7, 0xe4, 0x7c, 0xb4, 0x3a, 0xfd, 0x4b, 0x80);
        public static readonly Guid MF_TOPOLOGY_PROJECTSTART = new Guid(0x7ed3f802, 0x86bb, 0x4b3f, 0xb7, 0xe4, 0x7c, 0xb4, 0x3a, 0xfd, 0x4b, 0x80);
        public static readonly Guid MF_TOPOLOGY_PROJECTSTOP = new Guid(0x7ed3f803, 0x86bb, 0x4b3f, 0xb7, 0xe4, 0x7c, 0xb4, 0x3a, 0xfd, 0x4b, 0x80);
        public static readonly Guid MF_TOPOLOGY_RESOLUTION_STATUS = new Guid(0x494bbcde, 0xb031, 0x4e38, 0x97, 0xc4, 0xd5, 0x42, 0x2d, 0xd6, 0x18, 0xdc);

        // Topology Node Attributes
        public static readonly Guid MF_TOPONODE_CONNECT_METHOD = new Guid(0x494bbcf1, 0xb031, 0x4e38, 0x97, 0xc4, 0xd5, 0x42, 0x2d, 0xd6, 0x18, 0xdc);
        public static readonly Guid MF_TOPONODE_D3DAWARE = new Guid(0x494bbced, 0xb031, 0x4e38, 0x97, 0xc4, 0xd5, 0x42, 0x2d, 0xd6, 0x18, 0xdc);
        public static readonly Guid MF_TOPONODE_DECODER = new Guid(0x494bbd02, 0xb031, 0x4e38, 0x97, 0xc4, 0xd5, 0x42, 0x2d, 0xd6, 0x18, 0xdc);
        public static readonly Guid MF_TOPONODE_DECRYPTOR = new Guid(0x494bbcfa, 0xb031, 0x4e38, 0x97, 0xc4, 0xd5, 0x42, 0x2d, 0xd6, 0x18, 0xdc);
        public static readonly Guid MF_TOPONODE_DISABLE_PREROLL = new Guid(0x14932f9e, 0x9087, 0x4bb4, 0x84, 0x12, 0x51, 0x67, 0x14, 0x5c, 0xbe, 0x04);
        public static readonly Guid MF_TOPONODE_DISCARDABLE = new Guid(0x494bbcfb, 0xb031, 0x4e38, 0x97, 0xc4, 0xd5, 0x42, 0x2d, 0xd6, 0x18, 0xdc);
        public static readonly Guid MF_TOPONODE_DRAIN = new Guid(0x494bbce9, 0xb031, 0x4e38, 0x97, 0xc4, 0xd5, 0x42, 0x2d, 0xd6, 0x18, 0xdc);
        public static readonly Guid MF_TOPONODE_ERROR_MAJORTYPE = new Guid(0x494bbcfd, 0xb031, 0x4e38, 0x97, 0xc4, 0xd5, 0x42, 0x2d, 0xd6, 0x18, 0xdc);
        public static readonly Guid MF_TOPONODE_ERROR_SUBTYPE = new Guid(0x494bbcfe, 0xb031, 0x4e38, 0x97, 0xc4, 0xd5, 0x42, 0x2d, 0xd6, 0x18, 0xdc);
        public static readonly Guid MF_TOPONODE_ERRORCODE = new Guid(0x494bbcee, 0xb031, 0x4e38, 0x97, 0xc4, 0xd5, 0x42, 0x2d, 0xd6, 0x18, 0xdc);
        public static readonly Guid MF_TOPONODE_FLUSH = new Guid(0x494bbce8, 0xb031, 0x4e38, 0x97, 0xc4, 0xd5, 0x42, 0x2d, 0xd6, 0x18, 0xdc);
        public static readonly Guid MF_TOPONODE_LOCKED = new Guid(0x494bbcf7, 0xb031, 0x4e38, 0x97, 0xc4, 0xd5, 0x42, 0x2d, 0xd6, 0x18, 0xdc);
        public static readonly Guid MF_TOPONODE_MARKIN_HERE = new Guid(0x494bbd00, 0xb031, 0x4e38, 0x97, 0xc4, 0xd5, 0x42, 0x2d, 0xd6, 0x18, 0xdc);
        public static readonly Guid MF_TOPONODE_MARKOUT_HERE = new Guid(0x494bbd01, 0xb031, 0x4e38, 0x97, 0xc4, 0xd5, 0x42, 0x2d, 0xd6, 0x18, 0xdc);
        public static readonly Guid MF_TOPONODE_MEDIASTART = new Guid(0x835c58ea, 0xe075, 0x4bc7, 0xbc, 0xba, 0x4d, 0xe0, 0x00, 0xdf, 0x9a, 0xe6);
        public static readonly Guid MF_TOPONODE_MEDIASTOP = new Guid(0x835c58eb, 0xe075, 0x4bc7, 0xbc, 0xba, 0x4d, 0xe0, 0x00, 0xdf, 0x9a, 0xe6);
        public static readonly Guid MF_TOPONODE_NOSHUTDOWN_ON_REMOVE = new Guid(0x14932f9c, 0x9087, 0x4bb4, 0x84, 0x12, 0x51, 0x67, 0x14, 0x5c, 0xbe, 0x04);
        public static readonly Guid MF_TOPONODE_PRESENTATION_DESCRIPTOR = new Guid(0x835c58ed, 0xe075, 0x4bc7, 0xbc, 0xba, 0x4d, 0xe0, 0x00, 0xdf, 0x9a, 0xe6);
        public static readonly Guid MF_TOPONODE_PRIMARYOUTPUT = new Guid(0x6304ef99, 0x16b2, 0x4ebe, 0x9d, 0x67, 0xe4, 0xc5, 0x39, 0xb3, 0xa2, 0x59);
        public static readonly Guid MF_TOPONODE_RATELESS = new Guid(0x14932f9d, 0x9087, 0x4bb4, 0x84, 0x12, 0x51, 0x67, 0x14, 0x5c, 0xbe, 0x04);
        public static readonly Guid MF_TOPONODE_SEQUENCE_ELEMENTID = new Guid(0x835c58ef, 0xe075, 0x4bc7, 0xbc, 0xba, 0x4d, 0xe0, 0x00, 0xdf, 0x9a, 0xe6);
        public static readonly Guid MF_TOPONODE_SOURCE = new Guid(0x835c58ec, 0xe075, 0x4bc7, 0xbc, 0xba, 0x4d, 0xe0, 0x00, 0xdf, 0x9a, 0xe6);
        public static readonly Guid MF_TOPONODE_STREAM_DESCRIPTOR = new Guid(0x835c58ee, 0xe075, 0x4bc7, 0xbc, 0xba, 0x4d, 0xe0, 0x00, 0xdf, 0x9a, 0xe6);
        public static readonly Guid MF_TOPONODE_STREAMID = new Guid(0x14932f9b, 0x9087, 0x4bb4, 0x84, 0x12, 0x51, 0x67, 0x14, 0x5c, 0xbe, 0x04);
        public static readonly Guid MF_TOPONODE_TRANSFORM_OBJECTID = new Guid(0x88dcc0c9, 0x293e, 0x4e8b, 0x9a, 0xeb, 0xa, 0xd6, 0x4c, 0xc0, 0x16, 0xb0);
        public static readonly Guid MF_TOPONODE_WORKQUEUE_ID = new Guid(0x494bbcf8, 0xb031, 0x4e38, 0x97, 0xc4, 0xd5, 0x42, 0x2d, 0xd6, 0x18, 0xdc);
        public static readonly Guid MF_TOPONODE_WORKQUEUE_MMCSS_CLASS = new Guid(0x494bbcf9, 0xb031, 0x4e38, 0x97, 0xc4, 0xd5, 0x42, 0x2d, 0xd6, 0x18, 0xdc);
        public static readonly Guid MF_TOPONODE_WORKQUEUE_MMCSS_TASKID = new Guid(0x494bbcff, 0xb031, 0x4e38, 0x97, 0xc4, 0xd5, 0x42, 0x2d, 0xd6, 0x18, 0xdc);

        // Transform Attributes
        public static readonly Guid MF_ACTIVATE_MFT_LOCKED = new Guid(0xc1f6093c, 0x7f65, 0x4fbd, 0x9e, 0x39, 0x5f, 0xae, 0xc3, 0xc4, 0xfb, 0xd7);
        public static readonly Guid MF_SA_D3D_AWARE = new Guid(0xeaa35c29, 0x775e, 0x488e, 0x9b, 0x61, 0xb3, 0x28, 0x3e, 0x49, 0x58, 0x3b);
        public static readonly Guid MFT_SUPPORT_3DVIDEO = new Guid(0x93f81b1, 0x4f2e, 0x4631, 0x81, 0x68, 0x79, 0x34, 0x3, 0x2a, 0x1, 0xd3);

        // {53476A11-3F13-49fb-AC42-EE2733C96741} MFT_SUPPORT_DYNAMIC_FORMAT_CHANGE {UINT32 (BOOL)}
        public static readonly Guid MFT_SUPPORT_DYNAMIC_FORMAT_CHANGE = new Guid(0x53476a11, 0x3f13, 0x49fb, 0xac, 0x42, 0xee, 0x27, 0x33, 0xc9, 0x67, 0x41);

        // Presentation Descriptor Attributes

        public static readonly Guid MF_PD_APP_CONTEXT = new Guid(0x6c990d32, 0xbb8e, 0x477a, 0x85, 0x98, 0xd, 0x5d, 0x96, 0xfc, 0xd8, 0x8a);
        public static readonly Guid MF_PD_DURATION = new Guid(0x6c990d33, 0xbb8e, 0x477a, 0x85, 0x98, 0xd, 0x5d, 0x96, 0xfc, 0xd8, 0x8a);
        public static readonly Guid MF_PD_LAST_MODIFIED_TIME = new Guid(0x6c990d38, 0xbb8e, 0x477a, 0x85, 0x98, 0xd, 0x5d, 0x96, 0xfc, 0xd8, 0x8a);
        public static readonly Guid MF_PD_MIME_TYPE = new Guid(0x6c990d37, 0xbb8e, 0x477a, 0x85, 0x98, 0xd, 0x5d, 0x96, 0xfc, 0xd8, 0x8a);
        public static readonly Guid MF_PD_PMPHOST_CONTEXT = new Guid(0x6c990d31, 0xbb8e, 0x477a, 0x85, 0x98, 0xd, 0x5d, 0x96, 0xfc, 0xd8, 0x8a);
        public static readonly Guid MF_PD_SAMI_STYLELIST = new Guid(0xe0b73c7f, 0x486d, 0x484e, 0x98, 0x72, 0x4d, 0xe5, 0x19, 0x2a, 0x7b, 0xf8);
        public static readonly Guid MF_PD_TOTAL_FILE_SIZE = new Guid(0x6c990d34, 0xbb8e, 0x477a, 0x85, 0x98, 0xd, 0x5d, 0x96, 0xfc, 0xd8, 0x8a);
        public static readonly Guid MF_PD_AUDIO_ENCODING_BITRATE = new Guid(0x6c990d35, 0xbb8e, 0x477a, 0x85, 0x98, 0xd, 0x5d, 0x96, 0xfc, 0xd8, 0x8a);
        public static readonly Guid MF_PD_VIDEO_ENCODING_BITRATE = new Guid(0x6c990d36, 0xbb8e, 0x477a, 0x85, 0x98, 0xd, 0x5d, 0x96, 0xfc, 0xd8, 0x8a);

        // wmcontainer.h Attributes
        public static readonly Guid MF_PD_ASF_FILEPROPERTIES_FILE_ID = new Guid(0x3de649b4, 0xd76d, 0x4e66, 0x9e, 0xc9, 0x78, 0x12, 0xf, 0xb4, 0xc7, 0xe3);
        public static readonly Guid MF_PD_ASF_FILEPROPERTIES_CREATION_TIME = new Guid(0x3de649b6, 0xd76d, 0x4e66, 0x9e, 0xc9, 0x78, 0x12, 0xf, 0xb4, 0xc7, 0xe3);
        public static readonly Guid MF_PD_ASF_FILEPROPERTIES_PACKETS = new Guid(0x3de649b7, 0xd76d, 0x4e66, 0x9e, 0xc9, 0x78, 0x12, 0xf, 0xb4, 0xc7, 0xe3);
        public static readonly Guid MF_PD_ASF_FILEPROPERTIES_PLAY_DURATION = new Guid(0x3de649b8, 0xd76d, 0x4e66, 0x9e, 0xc9, 0x78, 0x12, 0xf, 0xb4, 0xc7, 0xe3);
        public static readonly Guid MF_PD_ASF_FILEPROPERTIES_SEND_DURATION = new Guid(0x3de649b9, 0xd76d, 0x4e66, 0x9e, 0xc9, 0x78, 0x12, 0xf, 0xb4, 0xc7, 0xe3);
        public static readonly Guid MF_PD_ASF_FILEPROPERTIES_PREROLL = new Guid(0x3de649ba, 0xd76d, 0x4e66, 0x9e, 0xc9, 0x78, 0x12, 0xf, 0xb4, 0xc7, 0xe3);
        public static readonly Guid MF_PD_ASF_FILEPROPERTIES_FLAGS = new Guid(0x3de649bb, 0xd76d, 0x4e66, 0x9e, 0xc9, 0x78, 0x12, 0xf, 0xb4, 0xc7, 0xe3);
        public static readonly Guid MF_PD_ASF_FILEPROPERTIES_MIN_PACKET_SIZE = new Guid(0x3de649bc, 0xd76d, 0x4e66, 0x9e, 0xc9, 0x78, 0x12, 0xf, 0xb4, 0xc7, 0xe3);
        public static readonly Guid MF_PD_ASF_FILEPROPERTIES_MAX_PACKET_SIZE = new Guid(0x3de649bd, 0xd76d, 0x4e66, 0x9e, 0xc9, 0x78, 0x12, 0xf, 0xb4, 0xc7, 0xe3);
        public static readonly Guid MF_PD_ASF_FILEPROPERTIES_MAX_BITRATE = new Guid(0x3de649be, 0xd76d, 0x4e66, 0x9e, 0xc9, 0x78, 0x12, 0xf, 0xb4, 0xc7, 0xe3);
        public static readonly Guid MF_PD_ASF_CONTENTENCRYPTION_TYPE = new Guid(0x8520fe3d, 0x277e, 0x46ea, 0x99, 0xe4, 0xe3, 0xa, 0x86, 0xdb, 0x12, 0xbe);
        public static readonly Guid MF_PD_ASF_CONTENTENCRYPTION_KEYID = new Guid(0x8520fe3e, 0x277e, 0x46ea, 0x99, 0xe4, 0xe3, 0xa, 0x86, 0xdb, 0x12, 0xbe);
        public static readonly Guid MF_PD_ASF_CONTENTENCRYPTION_SECRET_DATA = new Guid(0x8520fe3f, 0x277e, 0x46ea, 0x99, 0xe4, 0xe3, 0xa, 0x86, 0xdb, 0x12, 0xbe);
        public static readonly Guid MF_PD_ASF_CONTENTENCRYPTION_LICENSE_URL = new Guid(0x8520fe40, 0x277e, 0x46ea, 0x99, 0xe4, 0xe3, 0xa, 0x86, 0xdb, 0x12, 0xbe);
        public static readonly Guid MF_PD_ASF_CONTENTENCRYPTIONEX_ENCRYPTION_DATA = new Guid(0x62508be5, 0xecdf, 0x4924, 0xa3, 0x59, 0x72, 0xba, 0xb3, 0x39, 0x7b, 0x9d);
        public static readonly Guid MF_PD_ASF_LANGLIST = new Guid(0xf23de43c, 0x9977, 0x460d, 0xa6, 0xec, 0x32, 0x93, 0x7f, 0x16, 0xf, 0x7d);
        public static readonly Guid MF_PD_ASF_MARKER = new Guid(0x5134330e, 0x83a6, 0x475e, 0xa9, 0xd5, 0x4f, 0xb8, 0x75, 0xfb, 0x2e, 0x31);
        public static readonly Guid MF_PD_ASF_SCRIPT = new Guid(0xe29cd0d7, 0xd602, 0x4923, 0xa7, 0xfe, 0x73, 0xfd, 0x97, 0xec, 0xc6, 0x50);
        public static readonly Guid MF_PD_ASF_CODECLIST = new Guid(0xe4bb3509, 0xc18d, 0x4df1, 0xbb, 0x99, 0x7a, 0x36, 0xb3, 0xcc, 0x41, 0x19);
        public static readonly Guid MF_PD_ASF_METADATA_IS_VBR = new Guid(0x5fc6947a, 0xef60, 0x445d, 0xb4, 0x49, 0x44, 0x2e, 0xcc, 0x78, 0xb4, 0xc1);
        public static readonly Guid MF_PD_ASF_METADATA_V8_VBRPEAK = new Guid(0x5fc6947b, 0xef60, 0x445d, 0xb4, 0x49, 0x44, 0x2e, 0xcc, 0x78, 0xb4, 0xc1);
        public static readonly Guid MF_PD_ASF_METADATA_V8_BUFFERAVERAGE = new Guid(0x5fc6947c, 0xef60, 0x445d, 0xb4, 0x49, 0x44, 0x2e, 0xcc, 0x78, 0xb4, 0xc1);
        public static readonly Guid MF_PD_ASF_METADATA_LEAKY_BUCKET_PAIRS = new Guid(0x5fc6947d, 0xef60, 0x445d, 0xb4, 0x49, 0x44, 0x2e, 0xcc, 0x78, 0xb4, 0xc1);
        public static readonly Guid MF_PD_ASF_DATA_START_OFFSET = new Guid(0xe7d5b3e7, 0x1f29, 0x45d3, 0x88, 0x22, 0x3e, 0x78, 0xfa, 0xe2, 0x72, 0xed);
        public static readonly Guid MF_PD_ASF_DATA_LENGTH = new Guid(0xe7d5b3e8, 0x1f29, 0x45d3, 0x88, 0x22, 0x3e, 0x78, 0xfa, 0xe2, 0x72, 0xed);
        public static readonly Guid MF_SD_ASF_EXTSTRMPROP_LANGUAGE_ID_INDEX = new Guid(0x48f8a522, 0x305d, 0x422d, 0x85, 0x24, 0x25, 0x2, 0xdd, 0xa3, 0x36, 0x80);
        public static readonly Guid MF_SD_ASF_EXTSTRMPROP_AVG_DATA_BITRATE = new Guid(0x48f8a523, 0x305d, 0x422d, 0x85, 0x24, 0x25, 0x2, 0xdd, 0xa3, 0x36, 0x80);
        public static readonly Guid MF_SD_ASF_EXTSTRMPROP_AVG_BUFFERSIZE = new Guid(0x48f8a524, 0x305d, 0x422d, 0x85, 0x24, 0x25, 0x2, 0xdd, 0xa3, 0x36, 0x80);
        public static readonly Guid MF_SD_ASF_EXTSTRMPROP_MAX_DATA_BITRATE = new Guid(0x48f8a525, 0x305d, 0x422d, 0x85, 0x24, 0x25, 0x2, 0xdd, 0xa3, 0x36, 0x80);
        public static readonly Guid MF_SD_ASF_EXTSTRMPROP_MAX_BUFFERSIZE = new Guid(0x48f8a526, 0x305d, 0x422d, 0x85, 0x24, 0x25, 0x2, 0xdd, 0xa3, 0x36, 0x80);
        public static readonly Guid MF_SD_ASF_STREAMBITRATES_BITRATE = new Guid(0xa8e182ed, 0xafc8, 0x43d0, 0xb0, 0xd1, 0xf6, 0x5b, 0xad, 0x9d, 0xa5, 0x58);
        public static readonly Guid MF_SD_ASF_METADATA_DEVICE_CONFORMANCE_TEMPLATE = new Guid(0x245e929d, 0xc44e, 0x4f7e, 0xbb, 0x3c, 0x77, 0xd4, 0xdf, 0xd2, 0x7f, 0x8a);
        public static readonly Guid MF_PD_ASF_INFO_HAS_AUDIO = new Guid(0x80e62295, 0x2296, 0x4a44, 0xb3, 0x1c, 0xd1, 0x3, 0xc6, 0xfe, 0xd2, 0x3c);
        public static readonly Guid MF_PD_ASF_INFO_HAS_VIDEO = new Guid(0x80e62296, 0x2296, 0x4a44, 0xb3, 0x1c, 0xd1, 0x3, 0xc6, 0xfe, 0xd2, 0x3c);
        public static readonly Guid MF_PD_ASF_INFO_HAS_NON_AUDIO_VIDEO = new Guid(0x80e62297, 0x2296, 0x4a44, 0xb3, 0x1c, 0xd1, 0x3, 0xc6, 0xfe, 0xd2, 0x3c);
        public static readonly Guid MF_ASFSTREAMCONFIG_LEAKYBUCKET1 = new Guid(0xc69b5901, 0xea1a, 0x4c9b, 0xb6, 0x92, 0xe2, 0xa0, 0xd2, 0x9a, 0x8a, 0xdd);
        public static readonly Guid MF_ASFSTREAMCONFIG_LEAKYBUCKET2 = new Guid(0xc69b5902, 0xea1a, 0x4c9b, 0xb6, 0x92, 0xe2, 0xa0, 0xd2, 0x9a, 0x8a, 0xdd);

        // Arbitrary

        // {9E6BD6F5-0109-4f95-84AC-9309153A19FC}   MF_MT_ARBITRARY_HEADER          {MT_ARBITRARY_HEADER}
        public static readonly Guid MF_MT_ARBITRARY_HEADER = new Guid(0x9e6bd6f5, 0x109, 0x4f95, 0x84, 0xac, 0x93, 0x9, 0x15, 0x3a, 0x19, 0xfc);

        // {5A75B249-0D7D-49a1-A1C3-E0D87F0CADE5}   MF_MT_ARBITRARY_FORMAT          {Blob}
        public static readonly Guid MF_MT_ARBITRARY_FORMAT = new Guid(0x5a75b249, 0xd7d, 0x49a1, 0xa1, 0xc3, 0xe0, 0xd8, 0x7f, 0xc, 0xad, 0xe5);

        // Image

        // {ED062CF4-E34E-4922-BE99-934032133D7C}   MF_MT_IMAGE_LOSS_TOLERANT       {UINT32 (BOOL)}
        public static readonly Guid MF_MT_IMAGE_LOSS_TOLERANT = new Guid(0xed062cf4, 0xe34e, 0x4922, 0xbe, 0x99, 0x93, 0x40, 0x32, 0x13, 0x3d, 0x7c);

        // MPEG-4 Media Type Attributes

        // {261E9D83-9529-4B8F-A111-8B9C950A81A9}   MF_MT_MPEG4_SAMPLE_DESCRIPTION   {BLOB}
        public static readonly Guid MF_MT_MPEG4_SAMPLE_DESCRIPTION = new Guid(0x261e9d83, 0x9529, 0x4b8f, 0xa1, 0x11, 0x8b, 0x9c, 0x95, 0x0a, 0x81, 0xa9);

        // {9aa7e155-b64a-4c1d-a500-455d600b6560}   MF_MT_MPEG4_CURRENT_SAMPLE_ENTRY {UINT32}
        public static readonly Guid MF_MT_MPEG4_CURRENT_SAMPLE_ENTRY = new Guid(0x9aa7e155, 0xb64a, 0x4c1d, 0xa5, 0x00, 0x45, 0x5d, 0x60, 0x0b, 0x65, 0x60);

        // Save original format information for AVI and WAV files

        // {d7be3fe0-2bc7-492d-b843-61a1919b70c3}   MF_MT_ORIGINAL_4CC               (UINT32)
        public static readonly Guid MF_MT_ORIGINAL_4CC = new Guid(0xd7be3fe0, 0x2bc7, 0x492d, 0xb8, 0x43, 0x61, 0xa1, 0x91, 0x9b, 0x70, 0xc3);

        // {8cbbc843-9fd9-49c2-882f-a72586c408ad}   MF_MT_ORIGINAL_WAVE_FORMAT_TAG   (UINT32)
        public static readonly Guid MF_MT_ORIGINAL_WAVE_FORMAT_TAG = new Guid(0x8cbbc843, 0x9fd9, 0x49c2, 0x88, 0x2f, 0xa7, 0x25, 0x86, 0xc4, 0x08, 0xad);

        // Video Capture Media Type Attributes

        // {D2E7558C-DC1F-403f-9A72-D28BB1EB3B5E}   MF_MT_FRAME_RATE_RANGE_MIN      {UINT64 (HI32(Numerator),LO32(Denominator))}
        public static readonly Guid MF_MT_FRAME_RATE_RANGE_MIN = new Guid(0xd2e7558c, 0xdc1f, 0x403f, 0x9a, 0x72, 0xd2, 0x8b, 0xb1, 0xeb, 0x3b, 0x5e);

        // {E3371D41-B4CF-4a05-BD4E-20B88BB2C4D6}   MF_MT_FRAME_RATE_RANGE_MAX      {UINT64 (HI32(Numerator),LO32(Denominator))}
        public static readonly Guid MF_MT_FRAME_RATE_RANGE_MAX = new Guid(0xe3371d41, 0xb4cf, 0x4a05, 0xbd, 0x4e, 0x20, 0xb8, 0x8b, 0xb2, 0xc4, 0xd6);

        public static readonly Guid MF_LOW_LATENCY = new Guid(0x9c27891a, 0xed7a, 0x40e1, 0x88, 0xe8, 0xb2, 0x27, 0x27, 0xa0, 0x24, 0xee);

        public static readonly Guid MF_TOPOLOGY_DXVA_MODE = new Guid(0x1e8d34f6, 0xf5ab, 0x4e23, 0xbb, 0x88, 0x87, 0x4a, 0xa3, 0xa1, 0xa7, 0x4d);
        public static readonly Guid MF_TOPOLOGY_STATIC_PLAYBACK_OPTIMIZATIONS = new Guid(0xb86cac42, 0x41a6, 0x4b79, 0x89, 0x7a, 0x1a, 0xb0, 0xe5, 0x2b, 0x4a, 0x1b);
        public static readonly Guid MF_TOPOLOGY_PLAYBACK_MAX_DIMS = new Guid(0x5715cf19, 0x5768, 0x44aa, 0xad, 0x6e, 0x87, 0x21, 0xf1, 0xb0, 0xf9, 0xbb);

        public static readonly Guid MF_TOPOLOGY_HARDWARE_MODE = new Guid(0xd2d362fd, 0x4e4f, 0x4191, 0xa5, 0x79, 0xc6, 0x18, 0xb6, 0x67, 0x6, 0xaf);
        public static readonly Guid MF_TOPOLOGY_PLAYBACK_FRAMERATE = new Guid(0xc164737a, 0xc2b1, 0x4553, 0x83, 0xbb, 0x5a, 0x52, 0x60, 0x72, 0x44, 0x8f);
        public static readonly Guid MF_TOPOLOGY_DYNAMIC_CHANGE_NOT_ALLOWED = new Guid(0xd529950b, 0xd484, 0x4527, 0xa9, 0xcd, 0xb1, 0x90, 0x95, 0x32, 0xb5, 0xb0);
        public static readonly Guid MF_TOPOLOGY_ENUMERATE_SOURCE_TYPES = new Guid(0x6248c36d, 0x5d0b, 0x4f40, 0xa0, 0xbb, 0xb0, 0xb3, 0x05, 0xf7, 0x76, 0x98);
        public static readonly Guid MF_TOPOLOGY_START_TIME_ON_PRESENTATION_SWITCH = new Guid(0xc8cc113f, 0x7951, 0x4548, 0xaa, 0xd6, 0x9e, 0xd6, 0x20, 0x2e, 0x62, 0xb3);

        public static readonly Guid MF_PD_PLAYBACK_ELEMENT_ID = new Guid(0x6c990d39, 0xbb8e, 0x477a, 0x85, 0x98, 0xd, 0x5d, 0x96, 0xfc, 0xd8, 0x8a);
        public static readonly Guid MF_PD_PREFERRED_LANGUAGE = new Guid(0x6c990d3A, 0xbb8e, 0x477a, 0x85, 0x98, 0xd, 0x5d, 0x96, 0xfc, 0xd8, 0x8a);
        public static readonly Guid MF_PD_PLAYBACK_BOUNDARY_TIME = new Guid(0x6c990d3b, 0xbb8e, 0x477a, 0x85, 0x98, 0xd, 0x5d, 0x96, 0xfc, 0xd8, 0x8a);
        public static readonly Guid MF_PD_AUDIO_ISVARIABLEBITRATE = new Guid(0x33026ee0, 0xe387, 0x4582, 0xae, 0x0a, 0x34, 0xa2, 0xad, 0x3b, 0xaa, 0x18);

        public static readonly Guid MF_SD_STREAM_NAME = new Guid(0x4f1b099d, 0xd314, 0x41e5, 0xa7, 0x81, 0x7f, 0xef, 0xaa, 0x4c, 0x50, 0x1f);
        public static readonly Guid MF_SD_MUTUALLY_EXCLUSIVE = new Guid(0x23ef79c, 0x388d, 0x487f, 0xac, 0x17, 0x69, 0x6c, 0xd6, 0xe3, 0xc6, 0xf5);

        public static readonly Guid MF_SAMPLEGRABBERSINK_IGNORE_CLOCK = new Guid(0x0efda2c0, 0x2b69, 0x4e2e, 0xab, 0x8d, 0x46, 0xdc, 0xbf, 0xf7, 0xd2, 0x5d);
        public static readonly Guid MF_BYTESTREAMHANDLER_ACCEPTS_SHARE_WRITE = new Guid(0xa6e1f733, 0x3001, 0x4915, 0x81, 0x50, 0x15, 0x58, 0xa2, 0x18, 0xe, 0xc8);

        public static readonly Guid MF_TRANSCODE_CONTAINERTYPE = new Guid(0x150ff23f, 0x4abc, 0x478b, 0xac, 0x4f, 0xe1, 0x91, 0x6f, 0xba, 0x1c, 0xca);
        public static readonly Guid MF_TRANSCODE_SKIP_METADATA_TRANSFER = new Guid(0x4e4469ef, 0xb571, 0x4959, 0x8f, 0x83, 0x3d, 0xcf, 0xba, 0x33, 0xa3, 0x93);
        public static readonly Guid MF_TRANSCODE_TOPOLOGYMODE = new Guid(0x3e3df610, 0x394a, 0x40b2, 0x9d, 0xea, 0x3b, 0xab, 0x65, 0xb, 0xeb, 0xf2);
        public static readonly Guid MF_TRANSCODE_ADJUST_PROFILE = new Guid(0x9c37c21b, 0x60f, 0x487c, 0xa6, 0x90, 0x80, 0xd7, 0xf5, 0xd, 0x1c, 0x72);

        public static readonly Guid MF_TRANSCODE_ENCODINGPROFILE = new Guid(0x6947787c, 0xf508, 0x4ea9, 0xb1, 0xe9, 0xa1, 0xfe, 0x3a, 0x49, 0xfb, 0xc9);
        public static readonly Guid MF_TRANSCODE_QUALITYVSSPEED = new Guid(0x98332df8, 0x03cd, 0x476b, 0x89, 0xfa, 0x3f, 0x9e, 0x44, 0x2d, 0xec, 0x9f);
        public static readonly Guid MF_TRANSCODE_DONOT_INSERT_ENCODER = new Guid(0xf45aa7ce, 0xab24, 0x4012, 0xa1, 0x1b, 0xdc, 0x82, 0x20, 0x20, 0x14, 0x10);

        public static readonly Guid MF_DEVSOURCE_ATTRIBUTE_SOURCE_TYPE = new Guid(0xc60ac5fe, 0x252a, 0x478f, 0xa0, 0xef, 0xbc, 0x8f, 0xa5, 0xf7, 0xca, 0xd3);
        public static readonly Guid MF_DEVSOURCE_ATTRIBUTE_SOURCE_TYPE_VIDCAP_HW_SOURCE = new Guid(0xde7046ba, 0x54d6, 0x4487, 0xa2, 0xa4, 0xec, 0x7c, 0xd, 0x1b, 0xd1, 0x63);
        public static readonly Guid MF_DEVSOURCE_ATTRIBUTE_FRIENDLY_NAME = new Guid(0x60d0e559, 0x52f8, 0x4fa2, 0xbb, 0xce, 0xac, 0xdb, 0x34, 0xa8, 0xec, 0x1);
        public static readonly Guid MF_DEVSOURCE_ATTRIBUTE_MEDIA_TYPE = new Guid(0x56a819ca, 0xc78, 0x4de4, 0xa0, 0xa7, 0x3d, 0xda, 0xba, 0xf, 0x24, 0xd4);
        public static readonly Guid MF_DEVSOURCE_ATTRIBUTE_SOURCE_TYPE_VIDCAP_CATEGORY = new Guid(0x77f0ae69, 0xc3bd, 0x4509, 0x94, 0x1d, 0x46, 0x7e, 0x4d, 0x24, 0x89, 0x9e);
        public static readonly Guid MF_DEVSOURCE_ATTRIBUTE_SOURCE_TYPE_VIDCAP_SYMBOLIC_LINK = new Guid(0x58f0aad8, 0x22bf, 0x4f8a, 0xbb, 0x3d, 0xd2, 0xc4, 0x97, 0x8c, 0x6e, 0x2f);
        public static readonly Guid MF_DEVSOURCE_ATTRIBUTE_SOURCE_TYPE_VIDCAP_MAX_BUFFERS = new Guid(0x7dd9b730, 0x4f2d, 0x41d5, 0x8f, 0x95, 0xc, 0xc9, 0xa9, 0x12, 0xba, 0x26);
        public static readonly Guid MF_DEVSOURCE_ATTRIBUTE_SOURCE_TYPE_AUDCAP_ENDPOINT_ID = new Guid(0x30da9258, 0xfeb9, 0x47a7, 0xa4, 0x53, 0x76, 0x3a, 0x7a, 0x8e, 0x1c, 0x5f);
        public static readonly Guid MF_DEVSOURCE_ATTRIBUTE_SOURCE_TYPE_AUDCAP_ROLE = new Guid(0xbc9d118e, 0x8c67, 0x4a18, 0x85, 0xd4, 0x12, 0xd3, 0x0, 0x40, 0x5, 0x52);

        public static readonly Guid MFSampleExtension_DeviceTimestamp = new Guid(0x8f3e35e7, 0x2dcd, 0x4887, 0x86, 0x22, 0x2a, 0x58, 0xba, 0xa6, 0x52, 0xb0);

        public static readonly Guid MF_TRANSFORM_ASYNC = new Guid(0xf81a699a, 0x649a, 0x497d, 0x8c, 0x73, 0x29, 0xf8, 0xfe, 0xd6, 0xad, 0x7a);
        public static readonly Guid MF_TRANSFORM_ASYNC_UNLOCK = new Guid(0xe5666d6b, 0x3422, 0x4eb6, 0xa4, 0x21, 0xda, 0x7d, 0xb1, 0xf8, 0xe2, 0x7);
        public static readonly Guid MF_TRANSFORM_FLAGS_Attribute = new Guid(0x9359bb7e, 0x6275, 0x46c4, 0xa0, 0x25, 0x1c, 0x1, 0xe4, 0x5f, 0x1a, 0x86);
        public static readonly Guid MF_TRANSFORM_CATEGORY_Attribute = new Guid(0xceabba49, 0x506d, 0x4757, 0xa6, 0xff, 0x66, 0xc1, 0x84, 0x98, 0x7e, 0x4e);
        public static readonly Guid MFT_TRANSFORM_CLSID_Attribute = new Guid(0x6821c42b, 0x65a4, 0x4e82, 0x99, 0xbc, 0x9a, 0x88, 0x20, 0x5e, 0xcd, 0xc);
        public static readonly Guid MFT_INPUT_TYPES_Attributes = new Guid(0x4276c9b1, 0x759d, 0x4bf3, 0x9c, 0xd0, 0xd, 0x72, 0x3d, 0x13, 0x8f, 0x96);
        public static readonly Guid MFT_OUTPUT_TYPES_Attributes = new Guid(0x8eae8cf3, 0xa44f, 0x4306, 0xba, 0x5c, 0xbf, 0x5d, 0xda, 0x24, 0x28, 0x18);
        public static readonly Guid MFT_ENUM_HARDWARE_URL_Attribute = new Guid(0x2fb866ac, 0xb078, 0x4942, 0xab, 0x6c, 0x0, 0x3d, 0x5, 0xcd, 0xa6, 0x74);
        public static readonly Guid MFT_FRIENDLY_NAME_Attribute = new Guid(0x314ffbae, 0x5b41, 0x4c95, 0x9c, 0x19, 0x4e, 0x7d, 0x58, 0x6f, 0xac, 0xe3);
        public static readonly Guid MFT_CONNECTED_STREAM_ATTRIBUTE = new Guid(0x71eeb820, 0xa59f, 0x4de2, 0xbc, 0xec, 0x38, 0xdb, 0x1d, 0xd6, 0x11, 0xa4);
        public static readonly Guid MFT_CONNECTED_TO_HW_STREAM = new Guid(0x34e6e728, 0x6d6, 0x4491, 0xa5, 0x53, 0x47, 0x95, 0x65, 0xd, 0xb9, 0x12);
        public static readonly Guid MFT_PREFERRED_OUTPUTTYPE_Attribute = new Guid(0x7e700499, 0x396a, 0x49ee, 0xb1, 0xb4, 0xf6, 0x28, 0x2, 0x1e, 0x8c, 0x9d);
        public static readonly Guid MFT_PROCESS_LOCAL_Attribute = new Guid(0x543186e4, 0x4649, 0x4e65, 0xb5, 0x88, 0x4a, 0xa3, 0x52, 0xaf, 0xf3, 0x79);
        public static readonly Guid MFT_PREFERRED_ENCODER_PROFILE = new Guid(0x53004909, 0x1ef5, 0x46d7, 0xa1, 0x8e, 0x5a, 0x75, 0xf8, 0xb5, 0x90, 0x5f);
        public static readonly Guid MFT_HW_TIMESTAMP_WITH_QPC_Attribute = new Guid(0x8d030fb8, 0xcc43, 0x4258, 0xa2, 0x2e, 0x92, 0x10, 0xbe, 0xf8, 0x9b, 0xe4);
        public static readonly Guid MFT_FIELDOFUSE_UNLOCK_Attribute = new Guid(0x8ec2e9fd, 0x9148, 0x410d, 0x83, 0x1e, 0x70, 0x24, 0x39, 0x46, 0x1a, 0x8e);
        public static readonly Guid MFT_CODEC_MERIT_Attribute = new Guid(0x88a7cb15, 0x7b07, 0x4a34, 0x91, 0x28, 0xe6, 0x4c, 0x67, 0x3, 0xc4, 0xd3);
        public static readonly Guid MFT_ENUM_TRANSCODE_ONLY_ATTRIBUTE = new Guid(0x111ea8cd, 0xb62a, 0x4bdb, 0x89, 0xf6, 0x67, 0xff, 0xcd, 0xc2, 0x45, 0x8b);

        public static readonly Guid MF_MP2DLNA_USE_MMCSS = new Guid(0x54f3e2ee, 0xa2a2, 0x497d, 0x98, 0x34, 0x97, 0x3a, 0xfd, 0xe5, 0x21, 0xeb);
        public static readonly Guid MF_MP2DLNA_VIDEO_BIT_RATE = new Guid(0xe88548de, 0x73b4, 0x42d7, 0x9c, 0x75, 0xad, 0xfa, 0xa, 0x2a, 0x6e, 0x4c);
        public static readonly Guid MF_MP2DLNA_AUDIO_BIT_RATE = new Guid(0x2d1c070e, 0x2b5f, 0x4ab3, 0xa7, 0xe6, 0x8d, 0x94, 0x3b, 0xa8, 0xd0, 0x0a);
        public static readonly Guid MF_MP2DLNA_ENCODE_QUALITY = new Guid(0xb52379d7, 0x1d46, 0x4fb6, 0xa3, 0x17, 0xa4, 0xa5, 0xf6, 0x09, 0x59, 0xf8);
        public static readonly Guid MF_MP2DLNA_STATISTICS = new Guid(0x75e488a3, 0xd5ad, 0x4898, 0x85, 0xe0, 0xbc, 0xce, 0x24, 0xa7, 0x22, 0xd7);

        public static readonly Guid MF_SINK_WRITER_ASYNC_CALLBACK = new Guid(0x48cb183e, 0x7b0b, 0x46f4, 0x82, 0x2e, 0x5e, 0x1d, 0x2d, 0xda, 0x43, 0x54);
        public static readonly Guid MF_SINK_WRITER_DISABLE_THROTTLING = new Guid(0x08b845d8, 0x2b74, 0x4afe, 0x9d, 0x53, 0xbe, 0x16, 0xd2, 0xd5, 0xae, 0x4f);
        public static readonly Guid MF_SINK_WRITER_D3D_MANAGER = new Guid(0xec822da2, 0xe1e9, 0x4b29, 0xa0, 0xd8, 0x56, 0x3c, 0x71, 0x9f, 0x52, 0x69);
        public static readonly Guid MF_SINK_WRITER_ENCODER_CONFIG = new Guid(0xad91cd04, 0xa7cc, 0x4ac7, 0x99, 0xb6, 0xa5, 0x7b, 0x9a, 0x4a, 0x7c, 0x70);
        public static readonly Guid MF_READWRITE_DISABLE_CONVERTERS = new Guid(0x98d5b065, 0x1374, 0x4847, 0x8d, 0x5d, 0x31, 0x52, 0x0f, 0xee, 0x71, 0x56);
        public static readonly Guid MF_READWRITE_ENABLE_HARDWARE_TRANSFORMS = new Guid(0xa634a91c, 0x822b, 0x41b9, 0xa4, 0x94, 0x4d, 0xe4, 0x64, 0x36, 0x12, 0xb0);
        public static readonly Guid MF_READWRITE_MMCSS_CLASS = new Guid(0x39384300, 0xd0eb, 0x40b1, 0x87, 0xa0, 0x33, 0x18, 0x87, 0x1b, 0x5a, 0x53);
        public static readonly Guid MF_READWRITE_MMCSS_PRIORITY = new Guid(0x43ad19ce, 0xf33f, 0x4ba9, 0xa5, 0x80, 0xe4, 0xcd, 0x12, 0xf2, 0xd1, 0x44);
        public static readonly Guid MF_READWRITE_MMCSS_CLASS_AUDIO = new Guid(0x430847da, 0x0890, 0x4b0e, 0x93, 0x8c, 0x05, 0x43, 0x32, 0xc5, 0x47, 0xe1);
        public static readonly Guid MF_READWRITE_MMCSS_PRIORITY_AUDIO = new Guid(0x273db885, 0x2de2, 0x4db2, 0xa6, 0xa7, 0xfd, 0xb6, 0x6f, 0xb4, 0x0b, 0x61);
        public static readonly Guid MF_READWRITE_D3D_OPTIONAL = new Guid(0x216479d9, 0x3071, 0x42ca, 0xbb, 0x6c, 0x4c, 0x22, 0x10, 0x2e, 0x1d, 0x18);

        public static readonly Guid MF_SOURCE_READER_ASYNC_CALLBACK = new Guid(0x1e3dbeac, 0xbb43, 0x4c35, 0xb5, 0x07, 0xcd, 0x64, 0x44, 0x64, 0xc9, 0x65);
        public static readonly Guid MF_SOURCE_READER_D3D_MANAGER = new Guid(0xec822da2, 0xe1e9, 0x4b29, 0xa0, 0xd8, 0x56, 0x3c, 0x71, 0x9f, 0x52, 0x69);
        public static readonly Guid MF_SOURCE_READER_DISABLE_DXVA = new Guid(0xaa456cfd, 0x3943, 0x4a1e, 0xa7, 0x7d, 0x18, 0x38, 0xc0, 0xea, 0x2e, 0x35);
        public static readonly Guid MF_SOURCE_READER_MEDIASOURCE_CONFIG = new Guid(0x9085abeb, 0x0354, 0x48f9, 0xab, 0xb5, 0x20, 0x0d, 0xf8, 0x38, 0xc6, 0x8e);
        public static readonly Guid MF_SOURCE_READER_MEDIASOURCE_CHARACTERISTICS = new Guid(0x6d23f5c8, 0xc5d7, 0x4a9b, 0x99, 0x71, 0x5d, 0x11, 0xf8, 0xbc, 0xa8, 0x80);
        public static readonly Guid MF_SOURCE_READER_ENABLE_VIDEO_PROCESSING = new Guid(0xfb394f3d, 0xccf1, 0x42ee, 0xbb, 0xb3, 0xf9, 0xb8, 0x45, 0xd5, 0x68, 0x1d);
        public static readonly Guid MF_SOURCE_READER_DISCONNECT_MEDIASOURCE_ON_SHUTDOWN = new Guid(0x56b67165, 0x219e, 0x456d, 0xa2, 0x2e, 0x2d, 0x30, 0x04, 0xc7, 0xfe, 0x56);
        public static readonly Guid MF_SOURCE_READER_ENABLE_ADVANCED_VIDEO_PROCESSING = new Guid(0xf81da2c, 0xb537, 0x4672, 0xa8, 0xb2, 0xa6, 0x81, 0xb1, 0x73, 0x7, 0xa3);
        public static readonly Guid MF_SOURCE_READER_DISABLE_CAMERA_PLUGINS = new Guid(0x9d3365dd, 0x58f, 0x4cfb, 0x9f, 0x97, 0xb3, 0x14, 0xcc, 0x99, 0xc8, 0xad);
        public static readonly Guid MF_SOURCE_READER_ENABLE_TRANSCODE_ONLY_TRANSFORMS = new Guid(0xdfd4f008, 0xb5fd, 0x4e78, 0xae, 0x44, 0x62, 0xa1, 0xe6, 0x7b, 0xbe, 0x27);


        // Misc W8 attributes
        public static readonly Guid MF_ENABLE_3DVIDEO_OUTPUT = new Guid(0xbdad7bca, 0xe5f, 0x4b10, 0xab, 0x16, 0x26, 0xde, 0x38, 0x1b, 0x62, 0x93);
        public static readonly Guid MF_SA_D3D11_BINDFLAGS = new Guid(0xeacf97ad, 0x065c, 0x4408, 0xbe, 0xe3, 0xfd, 0xcb, 0xfd, 0x12, 0x8b, 0xe2);
        public static readonly Guid MF_SA_D3D11_USAGE = new Guid(0xe85fe442, 0x2ca3, 0x486e, 0xa9, 0xc7, 0x10, 0x9d, 0xda, 0x60, 0x98, 0x80);
        public static readonly Guid MF_SA_D3D11_AWARE = new Guid(0x206b4fc8, 0xfcf9, 0x4c51, 0xaf, 0xe3, 0x97, 0x64, 0x36, 0x9e, 0x33, 0xa0);
        public static readonly Guid MF_SA_D3D11_SHARED = new Guid(0x7b8f32c3, 0x6d96, 0x4b89, 0x92, 0x3, 0xdd, 0x38, 0xb6, 0x14, 0x14, 0xf3);
        public static readonly Guid MF_SA_BUFFERS_PER_SAMPLE = new Guid(0x873c5171, 0x1e3d, 0x4e25, 0x98, 0x8d, 0xb4, 0x33, 0xce, 0x04, 0x19, 0x83);
        public static readonly Guid MFT_DECODER_EXPOSE_OUTPUT_TYPES_IN_NATIVE_ORDER = new Guid(0xef80833f, 0xf8fa, 0x44d9, 0x80, 0xd8, 0x41, 0xed, 0x62, 0x32, 0x67, 0xc);
        public static readonly Guid MFT_DECODER_FINAL_VIDEO_RESOLUTION_HINT = new Guid(0xdc2f8496, 0x15c4, 0x407a, 0xb6, 0xf0, 0x1b, 0x66, 0xab, 0x5f, 0xbf, 0x53);
        public static readonly Guid MFT_ENUM_HARDWARE_VENDOR_ID_Attribute = new Guid(0x3aecb0cc, 0x35b, 0x4bcc, 0x81, 0x85, 0x2b, 0x8d, 0x55, 0x1e, 0xf3, 0xaf);
        public static readonly Guid MF_NALU_LENGTH_SET = new Guid(0xA7911D53, 0x12A4, 0x4965, 0xAE, 0x70, 0x6E, 0xAD, 0xD6, 0xFF, 0x05, 0x51);
        public static readonly Guid MF_NALU_LENGTH_INFORMATION = new Guid(0x19124E7C, 0xAD4B, 0x465F, 0xBB, 0x18, 0x20, 0x18, 0x62, 0x87, 0xB6, 0xAF);
        public static readonly Guid MF_USER_DATA_PAYLOAD = new Guid(0xd1d4985d, 0xdc92, 0x457a, 0xb3, 0xa0, 0x65, 0x1a, 0x33, 0xa3, 0x10, 0x47);
        public static readonly Guid MF_MPEG4SINK_SPSPPS_PASSTHROUGH = new Guid(0x5601a134, 0x2005, 0x4ad2, 0xb3, 0x7d, 0x22, 0xa6, 0xc5, 0x54, 0xde, 0xb2);
        public static readonly Guid MF_MPEG4SINK_MOOV_BEFORE_MDAT = new Guid(0xf672e3ac, 0xe1e6, 0x4f10, 0xb5, 0xec, 0x5f, 0x3b, 0x30, 0x82, 0x88, 0x16);
        public static readonly Guid MF_STREAM_SINK_SUPPORTS_HW_CONNECTION = new Guid(0x9b465cbf, 0x597, 0x4f9e, 0x9f, 0x3c, 0xb9, 0x7e, 0xee, 0xf9, 0x3, 0x59);
        public static readonly Guid MF_STREAM_SINK_SUPPORTS_ROTATION = new Guid(0xb3e96280, 0xbd05, 0x41a5, 0x97, 0xad, 0x8a, 0x7f, 0xee, 0x24, 0xb9, 0x12);
        public static readonly Guid MF_DISABLE_LOCALLY_REGISTERED_PLUGINS = new Guid(0x66b16da9, 0xadd4, 0x47e0, 0xa1, 0x6b, 0x5a, 0xf1, 0xfb, 0x48, 0x36, 0x34);
        public static readonly Guid MF_LOCAL_PLUGIN_CONTROL_POLICY = new Guid(0xd91b0085, 0xc86d, 0x4f81, 0x88, 0x22, 0x8c, 0x68, 0xe1, 0xd7, 0xfa, 0x04);
        public static readonly Guid MF_TOPONODE_WORKQUEUE_MMCSS_PRIORITY = new Guid(0x5001f840, 0x2816, 0x48f4, 0x93, 0x64, 0xad, 0x1e, 0xf6, 0x61, 0xa1, 0x23);
        public static readonly Guid MF_TOPONODE_WORKQUEUE_ITEM_PRIORITY = new Guid(0xa1ff99be, 0x5e97, 0x4a53, 0xb4, 0x94, 0x56, 0x8c, 0x64, 0x2c, 0x0f, 0xf3);
        public static readonly Guid MF_AUDIO_RENDERER_ATTRIBUTE_STREAM_CATEGORY = new Guid(0xa9770471, 0x92ec, 0x4df4, 0x94, 0xfe, 0x81, 0xc3, 0x6f, 0xc, 0x3a, 0x7a);
        public static readonly Guid MR_CAPTURE_POLICY_VOLUME_SERVICE = new Guid(0x24030acd, 0x107a, 0x4265, 0x97, 0x5c, 0x41, 0x4e, 0x33, 0xe6, 0x5f, 0x2a);
        public static readonly Guid MFNETSOURCE_ENABLE_PRIVATEMODE = new Guid(0x824779d8, 0xf18b, 0x4405, 0x8c, 0xf1, 0x46, 0x4f, 0xb5, 0xaa, 0x8f, 0x71);
        public static readonly Guid MFNETSOURCE_PEERMANAGER = new Guid(0x48b29adb, 0xfebf, 0x45ee, 0xa9, 0xbf, 0xef, 0xb8, 0x1c, 0x49, 0x2e, 0xfc);
        public static readonly Guid MFNETSOURCE_FRIENDLYNAME = new Guid(0x5b2a7757, 0xbc6b, 0x447e, 0xaa, 0x06, 0x0d, 0xda, 0x1c, 0x64, 0x6e, 0x2f);
        public static readonly Guid MFPROTECTION_PROTECTED_SURFACE = new Guid(0x4f5d9566, 0xe742, 0x4a25, 0x8d, 0x1f, 0xd2, 0x87, 0xb5, 0xfa, 0x0a, 0xde);
        public static readonly Guid MFPROTECTION_DISABLE_SCREEN_SCRAPE = new Guid(0xa21179a4, 0xb7cd, 0x40d8, 0x96, 0x14, 0x8e, 0xf2, 0x37, 0x1b, 0xa7, 0x8d);
        public static readonly Guid MFPROTECTION_VIDEO_FRAMES = new Guid(0x36a59cbc, 0x7401, 0x4a8c, 0xbc, 0x20, 0x46, 0xa7, 0xc9, 0xe5, 0x97, 0xf0);
        public static readonly Guid MFPROTECTIONATTRIBUTE_BEST_EFFORT = new Guid(0xc8e06331, 0x75f0, 0x4ec1, 0x8e, 0x77, 0x17, 0x57, 0x8f, 0x77, 0x3b, 0x46);
        public static readonly Guid MFPROTECTIONATTRIBUTE_FAIL_OVER = new Guid(0x8536abc5, 0x38f1, 0x4151, 0x9c, 0xce, 0xf5, 0x5d, 0x94, 0x12, 0x29, 0xac);
        public static readonly Guid MFPROTECTION_GRAPHICS_TRANSFER_AES_ENCRYPTION = new Guid(0xc873de64, 0xd8a5, 0x49e6, 0x88, 0xbb, 0xfb, 0x96, 0x3f, 0xd3, 0xd4, 0xce);
        public static readonly Guid MFTranscodeContainerType_AC3 = new Guid(0x6d8d91c3, 0x8c91, 0x4ed1, 0x87, 0x42, 0x8c, 0x34, 0x7d, 0x5b, 0x44, 0xd0);
        public static readonly Guid MFTranscodeContainerType_ADTS = new Guid(0x132fd27d, 0x0f02, 0x43de, 0xa3, 0x01, 0x38, 0xfb, 0xbb, 0xb3, 0x83, 0x4e);
        public static readonly Guid MFTranscodeContainerType_MPEG2 = new Guid(0xbfc2dbf9, 0x7bb4, 0x4f8f, 0xaf, 0xde, 0xe1, 0x12, 0xc4, 0x4b, 0xa8, 0x82);
        public static readonly Guid MFTranscodeContainerType_FMPEG4 = new Guid(0x9ba876f1, 0x419f, 0x4b77, 0xa1, 0xe0, 0x35, 0x95, 0x9d, 0x9d, 0x40, 0x4);
        public static readonly Guid MF_XVP_DISABLE_FRC = new Guid(0x2c0afa19, 0x7a97, 0x4d5a, 0x9e, 0xe8, 0x16, 0xd4, 0xfc, 0x51, 0x8d, 0x8c);
        public static readonly Guid MF_WRAPPED_OBJECT = new Guid(0x2b182c4c, 0xd6ac, 0x49f4, 0x89, 0x15, 0xf7, 0x18, 0x87, 0xdb, 0x70, 0xcd);
        public static readonly Guid MF_DEVSOURCE_ATTRIBUTE_SOURCE_TYPE_AUDCAP_SYMBOLIC_LINK = new Guid(0x98d24b5e, 0x5930, 0x4614, 0xb5, 0xa1, 0xf6, 0x0, 0xf9, 0x35, 0x5a, 0x78);
        public static readonly Guid MF_DEVICESTREAM_IMAGE_STREAM = new Guid(0xa7ffb865, 0xe7b2, 0x43b0, 0x9f, 0x6f, 0x9a, 0xf2, 0xa0, 0xe5, 0xf, 0xc0);
        public static readonly Guid MF_DEVICESTREAM_INDEPENDENT_IMAGE_STREAM = new Guid(0x3eeec7e, 0xd605, 0x4576, 0x8b, 0x29, 0x65, 0x80, 0xb4, 0x90, 0xd7, 0xd3);
        public static readonly Guid MF_DEVICESTREAM_STREAM_ID = new Guid(0x11bd5120, 0xd124, 0x446b, 0x88, 0xe6, 0x17, 0x6, 0x2, 0x57, 0xff, 0xf9);
        public static readonly Guid MF_DEVICESTREAM_STREAM_CATEGORY = new Guid(0x2939e7b8, 0xa62e, 0x4579, 0xb6, 0x74, 0xd4, 0x7, 0x3d, 0xfa, 0xbb, 0xba);
        public static readonly Guid MF_DEVICESTREAM_TRANSFORM_STREAM_ID = new Guid(0xe63937b7, 0xdaaf, 0x4d49, 0x81, 0x5f, 0xd8, 0x26, 0xf8, 0xad, 0x31, 0xe7);
        public static readonly Guid MF_DEVICESTREAM_EXTENSION_PLUGIN_CLSID = new Guid(0x048e6558, 0x60c4, 0x4173, 0xbd, 0x5b, 0x6a, 0x3c, 0xa2, 0x89, 0x6a, 0xee);
        public static readonly Guid MF_DEVICESTREAM_EXTENSION_PLUGIN_CONNECTION_POINT = new Guid(0x37f9375c, 0xe664, 0x4ea4, 0xaa, 0xe4, 0xcb, 0x6d, 0x1d, 0xac, 0xa1, 0xf4);
        public static readonly Guid MF_DEVICESTREAM_TAKEPHOTO_TRIGGER = new Guid(0x1d180e34, 0x538c, 0x4fbb, 0xa7, 0x5a, 0x85, 0x9a, 0xf7, 0xd2, 0x61, 0xa6);
        public static readonly Guid MF_DEVICESTREAM_MAX_FRAME_BUFFERS = new Guid(0x1684cebe, 0x3175, 0x4985, 0x88, 0x2c, 0x0e, 0xfd, 0x3e, 0x8a, 0xc1, 0x1e);
    }

    public static class MF_MEDIA_SHARING_ENGINE
    {
        public static readonly Guid DEVICE_NAME = new Guid(0x771e05d1, 0x862f, 0x4299, 0x95, 0xac, 0xae, 0x81, 0xfd, 0x14, 0xf3, 0xe7);
        public static readonly Guid DEVICE = new Guid(0xb461c58a, 0x7a08, 0x4b98, 0x99, 0xa8, 0x70, 0xfd, 0x5f, 0x3b, 0xad, 0xfd);
    }

    public static class MF_CAPTURE_ENGINE
    {
        public static readonly Guid INITIALIZED = new Guid(0x219992bc, 0xcf92, 0x4531, 0xa1, 0xae, 0x96, 0xe1, 0xe8, 0x86, 0xc8, 0xf1);
        public static readonly Guid PREVIEW_STARTED = new Guid(0xa416df21, 0xf9d3, 0x4a74, 0x99, 0x1b, 0xb8, 0x17, 0x29, 0x89, 0x52, 0xc4);
        public static readonly Guid PREVIEW_STOPPED = new Guid(0x13d5143c, 0x1edd, 0x4e50, 0xa2, 0xef, 0x35, 0x0a, 0x47, 0x67, 0x80, 0x60);
        public static readonly Guid RECORD_STARTED = new Guid(0xac2b027b, 0xddf9, 0x48a0, 0x89, 0xbe, 0x38, 0xab, 0x35, 0xef, 0x45, 0xc0);
        public static readonly Guid RECORD_STOPPED = new Guid(0x55e5200a, 0xf98f, 0x4c0d, 0xa9, 0xec, 0x9e, 0xb2, 0x5e, 0xd3, 0xd7, 0x73);
        public static readonly Guid PHOTO_TAKEN = new Guid(0x3c50c445, 0x7304, 0x48eb, 0x86, 0x5d, 0xbb, 0xa1, 0x9b, 0xa3, 0xaf, 0x5c);
        public static readonly Guid ERROR = new Guid(0x46b89fc6, 0x33cc, 0x4399, 0x9d, 0xad, 0x78, 0x4d, 0xe7, 0x7d, 0x58, 0x7c);
        public static readonly Guid EFFECT_ADDED = new Guid(0xaa8dc7b5, 0xa048, 0x4e13, 0x8e, 0xbe, 0xf2, 0x3c, 0x46, 0xc8, 0x30, 0xc1);
        public static readonly Guid EFFECT_REMOVED = new Guid(0xc6e8db07, 0xfb09, 0x4a48, 0x89, 0xc6, 0xbf, 0x92, 0xa0, 0x42, 0x22, 0xc9);
        public static readonly Guid ALL_EFFECTS_REMOVED = new Guid(0xfded7521, 0x8ed8, 0x431a, 0xa9, 0x6b, 0xf3, 0xe2, 0x56, 0x5e, 0x98, 0x1c);
        public static readonly Guid D3D_MANAGER = new Guid(0x76e25e7b, 0xd595, 0x4283, 0x96, 0x2c, 0xc5, 0x94, 0xaf, 0xd7, 0x8d, 0xdf);
        public static readonly Guid RECORD_SINK_VIDEO_MAX_UNPROCESSED_SAMPLES = new Guid(0xb467f705, 0x7913, 0x4894, 0x9d, 0x42, 0xa2, 0x15, 0xfe, 0xa2, 0x3d, 0xa9);
        public static readonly Guid RECORD_SINK_AUDIO_MAX_UNPROCESSED_SAMPLES = new Guid(0x1cddb141, 0xa7f4, 0x4d58, 0x98, 0x96, 0x4d, 0x15, 0xa5, 0x3c, 0x4e, 0xfe);
        public static readonly Guid RECORD_SINK_VIDEO_MAX_PROCESSED_SAMPLES = new Guid(0xe7b4a49e, 0x382c, 0x4aef, 0xa9, 0x46, 0xae, 0xd5, 0x49, 0xb, 0x71, 0x11);
        public static readonly Guid RECORD_SINK_AUDIO_MAX_PROCESSED_SAMPLES = new Guid(0xe7b4a49e, 0x382c, 0x4aef, 0xa9, 0x46, 0xae, 0xd5, 0x49, 0xb, 0x71, 0x11);
        public static readonly Guid USE_AUDIO_DEVICE_ONLY = new Guid(0x1c8077da, 0x8466, 0x4dc4, 0x8b, 0x8e, 0x27, 0x6b, 0x3f, 0x85, 0x92, 0x3b);
        public static readonly Guid USE_VIDEO_DEVICE_ONLY = new Guid(0x7e025171, 0xcf32, 0x4f2e, 0x8f, 0x19, 0x41, 0x5, 0x77, 0xb7, 0x3a, 0x66);
        public static readonly Guid DISABLE_HARDWARE_TRANSFORMS = new Guid(0xb7c42a6b, 0x3207, 0x4495, 0xb4, 0xe7, 0x81, 0xf9, 0xc3, 0x5d, 0x59, 0x91);
        public static readonly Guid DISABLE_DXVA = new Guid(0xf9818862, 0x179d, 0x433f, 0xa3, 0x2f, 0x74, 0xcb, 0xcf, 0x74, 0x46, 0x6d);
        public static readonly Guid MEDIASOURCE_CONFIG = new Guid(0xbc6989d2, 0x0fc1, 0x46e1, 0xa7, 0x4f, 0xef, 0xd3, 0x6b, 0xc7, 0x88, 0xde);
        public static readonly Guid DECODER_MFT_FIELDOFUSE_UNLOCK_Attribute = new Guid(0x2b8ad2e8, 0x7acb, 0x4321, 0xa6, 0x06, 0x32, 0x5c, 0x42, 0x49, 0xf4, 0xfc);
        public static readonly Guid ENCODER_MFT_FIELDOFUSE_UNLOCK_Attribute = new Guid(0x54c63a00, 0x78d5, 0x422f, 0xaa, 0x3e, 0x5e, 0x99, 0xac, 0x64, 0x92, 0x69);
        public static readonly Guid DISABLE_LOW_LATENCY = new Guid(0xdab5a16d, 0x1f0f, 0x44da, 0xad, 0x48, 0x82, 0x27, 0x31, 0x89, 0x43, 0xb8);
        public static readonly Guid EVENT_GENERATOR_GUID = new Guid(0xabfa8ad5, 0xfc6d, 0x4911, 0x87, 0xe0, 0x96, 0x19, 0x45, 0xf8, 0xf7, 0xce);
        public static readonly Guid EVENT_STREAM_INDEX = new Guid(0x82697f44, 0xb1cf, 0x42eb, 0x97, 0x53, 0xf8, 0x6d, 0x64, 0x9c, 0x88, 0x65);

        public static readonly Guid SOURCE_CURRENT_DEVICE_MEDIA_TYPE_SET = new Guid(0xe7e75e4c, 0x039c, 0x4410, 0x81, 0x5b, 0x87, 0x41, 0x30, 0x7b, 0x63, 0xaa);
        public static readonly Guid SINK_PREPARED = new Guid(0x7BFCE257, 0x12B1, 0x4409, 0x8C, 0x34, 0xD4, 0x45, 0xDA, 0xAB, 0x75, 0x78);
    }

    public static class MFTranscodeContainerType
    {
        public static readonly Guid ASF = new Guid(0x430f6f6e, 0xb6bf, 0x4fc1, 0xa0, 0xbd, 0x9e, 0xe4, 0x6e, 0xee, 0x2a, 0xfb);
        public static readonly Guid MPEG4 = new Guid(0xdc6cd05d, 0xb9d0, 0x40ef, 0xbd, 0x35, 0xfa, 0x62, 0x2c, 0x1a, 0xb2, 0x8a);
        public static readonly Guid MP3 = new Guid(0xe438b912, 0x83f1, 0x4de6, 0x9e, 0x3a, 0x9f, 0xfb, 0xc6, 0xdd, 0x24, 0xd1);
        public static readonly Guid x3GP = new Guid(0x34c50167, 0x4472, 0x4f34, 0x9e, 0xa0, 0xc4, 0x9f, 0xba, 0xcf, 0x03, 0x7d);
    }

    public static class MFConnector
    {
        public static readonly Guid MFCONNECTOR_AGP = new Guid(0xac3aef60, 0xce43, 0x11d9, 0x92, 0xdb, 0x00, 0x0b, 0xdb, 0x28, 0xff, 0x98);
        public static readonly Guid MFCONNECTOR_COMPONENT = new Guid(0x57cd596b, 0xce47, 0x11d9, 0x92, 0xdb, 0x00, 0x0b, 0xdb, 0x28, 0xff, 0x98);
        public static readonly Guid MFCONNECTOR_COMPOSITE = new Guid(0x57cd596a, 0xce47, 0x11d9, 0x92, 0xdb, 0x00, 0x0b, 0xdb, 0x28, 0xff, 0x98);
        public static readonly Guid MFCONNECTOR_D_JPN = new Guid(0x57cd5970, 0xce47, 0x11d9, 0x92, 0xdb, 0x00, 0x0b, 0xdb, 0x28, 0xff, 0x98);
        public static readonly Guid MFCONNECTOR_DISPLAYPORT_EMBEDDED = new Guid(0x57cd5973, 0xce47, 0x11d9, 0x92, 0xdb, 0x00, 0x0b, 0xdb, 0x28, 0xff, 0x98);
        public static readonly Guid MFCONNECTOR_DISPLAYPORT_EXTERNAL = new Guid(0x57cd5972, 0xce47, 0x11d9, 0x92, 0xdb, 0x00, 0x0b, 0xdb, 0x28, 0xff, 0x98);
        public static readonly Guid MFCONNECTOR_DVI = new Guid(0x57cd596c, 0xce47, 0x11d9, 0x92, 0xdb, 0x00, 0x0b, 0xdb, 0x28, 0xff, 0x98);
        public static readonly Guid MFCONNECTOR_HDMI = new Guid(0x57cd596d, 0xce47, 0x11d9, 0x92, 0xdb, 0x00, 0x0b, 0xdb, 0x28, 0xff, 0x98);
        public static readonly Guid MFCONNECTOR_LVDS = new Guid(0x57cd596e, 0xce47, 0x11d9, 0x92, 0xdb, 0x00, 0x0b, 0xdb, 0x28, 0xff, 0x98);
        public static readonly Guid MFCONNECTOR_PCI = new Guid(0xac3aef5d, 0xce43, 0x11d9, 0x92, 0xdb, 0x00, 0x0b, 0xdb, 0x28, 0xff, 0x98);
        public static readonly Guid MFCONNECTOR_PCI_Express = new Guid(0xac3aef5f, 0xce43, 0x11d9, 0x92, 0xdb, 0x00, 0x0b, 0xdb, 0x28, 0xff, 0x98);
        public static readonly Guid MFCONNECTOR_PCIX = new Guid(0xac3aef5e, 0xce43, 0x11d9, 0x92, 0xdb, 0x00, 0x0b, 0xdb, 0x28, 0xff, 0x98);
        public static readonly Guid MFCONNECTOR_SPDIF = new Guid(0xb94a712, 0xad3e, 0x4cee, 0x83, 0xce, 0xce, 0x32, 0xe3, 0xdb, 0x65, 0x22);
        public static readonly Guid MFCONNECTOR_SVIDEO = new Guid(0x57cd5969, 0xce47, 0x11d9, 0x92, 0xdb, 0x00, 0x0b, 0xdb, 0x28, 0xff, 0x98);
        public static readonly Guid MFCONNECTOR_UDI_EMBEDDED = new Guid(0x57cd5975, 0xce47, 0x11d9, 0x92, 0xdb, 0x00, 0x0b, 0xdb, 0x28, 0xff, 0x98);
        public static readonly Guid MFCONNECTOR_UDI_EXTERNAL = new Guid(0x57cd5974, 0xce47, 0x11d9, 0x92, 0xdb, 0x00, 0x0b, 0xdb, 0x28, 0xff, 0x98);
        public static readonly Guid MFCONNECTOR_UNKNOWN = new Guid(0xac3aef5c, 0xce43, 0x11d9, 0x92, 0xdb, 0x00, 0x0b, 0xdb, 0x28, 0xff, 0x98);
        public static readonly Guid MFCONNECTOR_VGA = new Guid(0x57cd5968, 0xce47, 0x11d9, 0x92, 0xdb, 0x00, 0x0b, 0xdb, 0x28, 0xff, 0x98);
    }

    public static class MFTransformCategory
    {
        // {d6c02d4b-6833-45b4-971a-05a4b04bab91}   MFT_CATEGORY_VIDEO_DECODER
        public static readonly Guid MFT_CATEGORY_VIDEO_DECODER = new Guid(0xd6c02d4b, 0x6833, 0x45b4, 0x97, 0x1a, 0x05, 0xa4, 0xb0, 0x4b, 0xab, 0x91);

        // {f79eac7d-e545-4387-bdee-d647d7bde42a}   MFT_CATEGORY_VIDEO_ENCODER
        public static readonly Guid MFT_CATEGORY_VIDEO_ENCODER = new Guid(0xf79eac7d, 0xe545, 0x4387, 0xbd, 0xee, 0xd6, 0x47, 0xd7, 0xbd, 0xe4, 0x2a);

        // {12e17c21-532c-4a6e-8a1c-40825a736397}   MFT_CATEGORY_VIDEO_EFFECT
        public static readonly Guid MFT_CATEGORY_VIDEO_EFFECT = new Guid(0x12e17c21, 0x532c, 0x4a6e, 0x8a, 0x1c, 0x40, 0x82, 0x5a, 0x73, 0x63, 0x97);

        // {059c561e-05ae-4b61-b69d-55b61ee54a7b}   MFT_CATEGORY_MULTIPLEXER
        public static readonly Guid MFT_CATEGORY_MULTIPLEXER = new Guid(0x059c561e, 0x05ae, 0x4b61, 0xb6, 0x9d, 0x55, 0xb6, 0x1e, 0xe5, 0x4a, 0x7b);

        // {a8700a7a-939b-44c5-99d7-76226b23b3f1}   MFT_CATEGORY_DEMULTIPLEXER
        public static readonly Guid MFT_CATEGORY_DEMULTIPLEXER = new Guid(0xa8700a7a, 0x939b, 0x44c5, 0x99, 0xd7, 0x76, 0x22, 0x6b, 0x23, 0xb3, 0xf1);

        // {9ea73fb4-ef7a-4559-8d5d-719d8f0426c7}   MFT_CATEGORY_AUDIO_DECODER
        public static readonly Guid MFT_CATEGORY_AUDIO_DECODER = new Guid(0x9ea73fb4, 0xef7a, 0x4559, 0x8d, 0x5d, 0x71, 0x9d, 0x8f, 0x04, 0x26, 0xc7);

        // {91c64bd0-f91e-4d8c-9276-db248279d975}   MFT_CATEGORY_AUDIO_ENCODER
        public static readonly Guid MFT_CATEGORY_AUDIO_ENCODER = new Guid(0x91c64bd0, 0xf91e, 0x4d8c, 0x92, 0x76, 0xdb, 0x24, 0x82, 0x79, 0xd9, 0x75);

        // {11064c48-3648-4ed0-932e-05ce8ac811b7}   MFT_CATEGORY_AUDIO_EFFECT
        public static readonly Guid MFT_CATEGORY_AUDIO_EFFECT = new Guid(0x11064c48, 0x3648, 0x4ed0, 0x93, 0x2e, 0x05, 0xce, 0x8a, 0xc8, 0x11, 0xb7);

        // {302EA3FC-AA5F-47f9-9F7A-C2188BB163021}   MFT_CATEGORY_VIDEO_PROCESSOR
        public static readonly Guid MFT_CATEGORY_VIDEO_PROCESSOR = new Guid(0x302ea3fc, 0xaa5f, 0x47f9, 0x9f, 0x7a, 0xc2, 0x18, 0x8b, 0xb1, 0x63, 0x2);

        // {90175d57-b7ea-4901-aeb3-933a8747756f}   MFT_CATEGORY_OTHER
        public static readonly Guid MFT_CATEGORY_OTHER = new Guid(0x90175d57, 0xb7ea, 0x4901, 0xae, 0xb3, 0x93, 0x3a, 0x87, 0x47, 0x75, 0x6f);

    }

    public static class MFEnabletype
    {
        public static readonly Guid MFENABLETYPE_MF_RebootRequired = new Guid(0x6d4d3d4b, 0x0ece, 0x4652, 0x8b, 0x3a, 0xf2, 0xd2, 0x42, 0x60, 0xd8, 0x87);
        public static readonly Guid MFENABLETYPE_MF_UpdateRevocationInformation = new Guid(0xe558b0b5, 0xb3c4, 0x44a0, 0x92, 0x4c, 0x50, 0xd1, 0x78, 0x93, 0x23, 0x85);
        public static readonly Guid MFENABLETYPE_MF_UpdateUntrustedComponent = new Guid(0x9879f3d6, 0xcee2, 0x48e6, 0xb5, 0x73, 0x97, 0x67, 0xab, 0x17, 0x2f, 0x16);
        public static readonly Guid MFENABLETYPE_WMDRMV1_LicenseAcquisition = new Guid(0x4ff6eeaf, 0xb43, 0x4797, 0x9b, 0x85, 0xab, 0xf3, 0x18, 0x15, 0xe7, 0xb0);
        public static readonly Guid MFENABLETYPE_WMDRMV7_Individualization = new Guid(0xacd2c84a, 0xb303, 0x4f65, 0xbc, 0x2c, 0x2c, 0x84, 0x8d, 0x1, 0xa9, 0x89);
        public static readonly Guid MFENABLETYPE_WMDRMV7_LicenseAcquisition = new Guid(0x3306df, 0x4a06, 0x4884, 0xa0, 0x97, 0xef, 0x6d, 0x22, 0xec, 0x84, 0xa3);
    }

    public static class MFRepresentation
    {
        /// <summary> AM_MEDIA_TYPE_REPRESENTATION </summary>
        public static readonly Guid AMMediaType = new Guid(0xe2e42ad2, 0x132c, 0x491e, 0xa2, 0x68, 0x3c, 0x7c, 0x2d, 0xca, 0x18, 0x1f);
        /// <summary> FORMAT_MFVideoFormat </summary>
        public static readonly Guid MFVideoFormat = new Guid(0xaed4ab2d, 0x7326, 0x43cb, 0x94, 0x64, 0xc8, 0x79, 0xca, 0xb9, 0xc4, 0x3d);
        /// <summary> FORMAT_VideoInfo </summary>
        public static readonly Guid VideoInfo = new Guid(0x05589f80, 0xc356, 0x11ce, 0xbf, 0x01, 0x00, 0xaa, 0x00, 0x55, 0x59, 0x5a);
        /// <summary> FORMAT_VideoInfo2 </summary>
        public static readonly Guid VideoInfo2 = new Guid(0xf72a76A0, 0xeb0a, 0x11d0, 0xac, 0xe4, 0x00, 0x00, 0xc0, 0xcc, 0x16, 0xba);

    }

    public static class MFProperties
    {
        // Media Foundation Properties
        public static readonly Guid MFNETSOURCE_ACCELERATEDSTREAMINGDURATION = new Guid(0x3cb1f277, 0x0505, 0x4c5d, 0xae, 0x71, 0x0a, 0x55, 0x63, 0x44, 0xef, 0xa1);
        public static readonly Guid MFNETSOURCE_AUTORECONNECTLIMIT = new Guid(0x3cb1f27a, 0x0505, 0x4c5d, 0xae, 0x71, 0x0a, 0x55, 0x63, 0x44, 0xef, 0xa1);
        public static readonly Guid MFNETSOURCE_AUTORECONNECTPROGRESS = new Guid(0x3cb1f282, 0x0505, 0x4c5d, 0xae, 0x71, 0x0a, 0x55, 0x63, 0x44, 0xef, 0xa1);
        public static readonly Guid MFNETSOURCE_BROWSERUSERAGENT = new Guid(0x3cb1f28b, 0x0505, 0x4c5d, 0xae, 0x71, 0x0a, 0x55, 0x63, 0x44, 0xef, 0xa1);
        public static readonly Guid MFNETSOURCE_BROWSERWEBPAGE = new Guid(0x3cb1f28c, 0x0505, 0x4c5d, 0xae, 0x71, 0x0a, 0x55, 0x63, 0x44, 0xef, 0xa1);
        public static readonly Guid MFNETSOURCE_BUFFERINGTIME = new Guid(0x3cb1f276, 0x0505, 0x4c5d, 0xae, 0x71, 0x0a, 0x55, 0x63, 0x44, 0xef, 0xa1);
        public static readonly Guid MFNETSOURCE_CACHEENABLED = new Guid(0x3cb1f279, 0x0505, 0x4c5d, 0xae, 0x71, 0x0a, 0x55, 0x63, 0x44, 0xef, 0xa1);
        public static readonly Guid MFNETSOURCE_CONNECTIONBANDWIDTH = new Guid(0x3cb1f278, 0x0505, 0x4c5d, 0xae, 0x71, 0x0a, 0x55, 0x63, 0x44, 0xef, 0xa1);
        public static readonly Guid MFNETSOURCE_CREDENTIAL_MANAGER = new Guid(0x3cb1f280, 0x0505, 0x4c5d, 0xae, 0x71, 0x0a, 0x55, 0x63, 0x44, 0xef, 0xa1);
        public static readonly Guid MFNETSOURCE_DRMNET_LICENSE_REPRESENTATION = new Guid(0x47eae1bd, 0xbdfe, 0x42e2, 0x82, 0xf3, 0x54, 0xa4, 0x8c, 0x17, 0x96, 0x2d);
        public static readonly Guid MFNETSOURCE_ENABLE_DOWNLOAD = new Guid(0x3cb1f29d, 0x0505, 0x4c5d, 0xae, 0x71, 0x0a, 0x55, 0x63, 0x44, 0xef, 0xa1);
        public static readonly Guid MFNETSOURCE_ENABLE_HTTP = new Guid(0x3cb1f299, 0x0505, 0x4c5d, 0xae, 0x71, 0x0a, 0x55, 0x63, 0x44, 0xef, 0xa1);
        public static readonly Guid MFNETSOURCE_ENABLE_RTSP = new Guid(0x3cb1f298, 0x0505, 0x4c5d, 0xae, 0x71, 0x0a, 0x55, 0x63, 0x44, 0xef, 0xa1);
        public static readonly Guid MFNETSOURCE_ENABLE_STREAMING = new Guid(0x3cb1f29c, 0x0505, 0x4c5d, 0xae, 0x71, 0x0a, 0x55, 0x63, 0x44, 0xef, 0xa1);
        public static readonly Guid MFNETSOURCE_ENABLE_TCP = new Guid(0x3cb1f295, 0x0505, 0x4c5d, 0xae, 0x71, 0x0a, 0x55, 0x63, 0x44, 0xef, 0xa1);
        public static readonly Guid MFNETSOURCE_ENABLE_UDP = new Guid(0x3cb1f294, 0x0505, 0x4c5d, 0xae, 0x71, 0x0a, 0x55, 0x63, 0x44, 0xef, 0xa1);
        public static readonly Guid MFNETSOURCE_HOSTEXE = new Guid(0x3cb1f28f, 0x0505, 0x4c5d, 0xae, 0x71, 0x0a, 0x55, 0x63, 0x44, 0xef, 0xa1);
        public static readonly Guid MFNETSOURCE_HOSTVERSION = new Guid(0x3cb1f291, 0x0505, 0x4c5d, 0xae, 0x71, 0x0a, 0x55, 0x63, 0x44, 0xef, 0xa1);
        public static readonly Guid MFNETSOURCE_LOGURL = new Guid(0x3cb1f293, 0x0505, 0x4c5d, 0xae, 0x71, 0x0a, 0x55, 0x63, 0x44, 0xef, 0xa1);
        public static readonly Guid MFNETSOURCE_MAXBUFFERTIMEMS = new Guid(0x408b24e6, 0x4038, 0x4401, 0xb5, 0xb2, 0xfe, 0x70, 0x1a, 0x9e, 0xbf, 0x10);
        public static readonly Guid MFNETSOURCE_MAXUDPACCELERATEDSTREAMINGDURATION = new Guid(0x4aab2879, 0xbbe1, 0x4994, 0x9f, 0xf0, 0x54, 0x95, 0xbd, 0x25, 0x1, 0x29);
        public static readonly Guid MFNETSOURCE_PLAYERID = new Guid(0x3cb1f28e, 0x0505, 0x4c5d, 0xae, 0x71, 0x0a, 0x55, 0x63, 0x44, 0xef, 0xa1);
        public static readonly Guid MFNETSOURCE_PLAYERUSERAGENT = new Guid(0x3cb1f292, 0x0505, 0x4c5d, 0xae, 0x71, 0x0a, 0x55, 0x63, 0x44, 0xef, 0xa1);
        public static readonly Guid MFNETSOURCE_PLAYERVERSION = new Guid(0x3cb1f28d, 0x0505, 0x4c5d, 0xae, 0x71, 0x0a, 0x55, 0x63, 0x44, 0xef, 0xa1);
        public static readonly Guid MFNETSOURCE_PPBANDWIDTH = new Guid(0x3cb1f281, 0x0505, 0x4c5d, 0xae, 0x71, 0x0a, 0x55, 0x63, 0x44, 0xef, 0xa1);
        public static readonly Guid MFNETSOURCE_PROTOCOL = new Guid(0x3cb1f27d, 0x0505, 0x4c5d, 0xae, 0x71, 0x0a, 0x55, 0x63, 0x44, 0xef, 0xa1);
        public static readonly Guid MFNETSOURCE_PROXYBYPASSFORLOCAL = new Guid(0x3cb1f286, 0x0505, 0x4c5d, 0xae, 0x71, 0x0a, 0x55, 0x63, 0x44, 0xef, 0xa1);
        public static readonly Guid MFNETSOURCE_PROXYEXCEPTIONLIST = new Guid(0x3cb1f285, 0x0505, 0x4c5d, 0xae, 0x71, 0x0a, 0x55, 0x63, 0x44, 0xef, 0xa1);
        public static readonly Guid MFNETSOURCE_PROXYHOSTNAME = new Guid(0x3cb1f284, 0x0505, 0x4c5d, 0xae, 0x71, 0x0a, 0x55, 0x63, 0x44, 0xef, 0xa1);
        public static readonly Guid MFNETSOURCE_PROXYINFO = new Guid(0x3cb1f29b, 0x0505, 0x4c5d, 0xae, 0x71, 0x0a, 0x55, 0x63, 0x44, 0xef, 0xa1);
        public static readonly Guid MFNETSOURCE_PROXYLOCATORFACTORY = new Guid(0x3cb1f283, 0x0505, 0x4c5d, 0xae, 0x71, 0x0a, 0x55, 0x63, 0x44, 0xef, 0xa1);
        public static readonly Guid MFNETSOURCE_PROXYPORT = new Guid(0x3cb1f288, 0x0505, 0x4c5d, 0xae, 0x71, 0x0a, 0x55, 0x63, 0x44, 0xef, 0xa1);
        public static readonly Guid MFNETSOURCE_PROXYRERUNAUTODETECTION = new Guid(0x3cb1f289, 0x0505, 0x4c5d, 0xae, 0x71, 0x0a, 0x55, 0x63, 0x44, 0xef, 0xa1);
        public static readonly Guid MFNETSOURCE_PROXYSETTINGS = new Guid(0x3cb1f287, 0x0505, 0x4c5d, 0xae, 0x71, 0x0a, 0x55, 0x63, 0x44, 0xef, 0xa1);
        public static readonly Guid MFNETSOURCE_RESENDSENABLED = new Guid(0x3cb1f27b, 0x0505, 0x4c5d, 0xae, 0x71, 0x0a, 0x55, 0x63, 0x44, 0xef, 0xa1);
        public static readonly Guid MFNETSOURCE_STATISTICS = new Guid(0x3cb1f274, 0x0505, 0x4c5d, 0xae, 0x71, 0x0a, 0x55, 0x63, 0x44, 0xef, 0xa1);
        public static readonly Guid MFNETSOURCE_THINNINGENABLED = new Guid(0x3cb1f27c, 0x0505, 0x4c5d, 0xae, 0x71, 0x0a, 0x55, 0x63, 0x44, 0xef, 0xa1);
        public static readonly Guid MFNETSOURCE_TRANSPORT = new Guid(0x3cb1f27e, 0x0505, 0x4c5d, 0xae, 0x71, 0x0a, 0x55, 0x63, 0x44, 0xef, 0xa1);
        public static readonly Guid MFNETSOURCE_UDP_PORT_RANGE = new Guid(0x3cb1f29a, 0x0505, 0x4c5d, 0xae, 0x71, 0x0a, 0x55, 0x63, 0x44, 0xef, 0xa1);

        public static readonly Guid MFNETSOURCE_SSLCERTIFICATE_MANAGER = new Guid(0x55e6cb27, 0xe69b, 0x4267, 0x94, 0x0c, 0x2d, 0x7e, 0xc5, 0xbb, 0x8a, 0x0f);

        public static readonly Guid MFNETSOURCE_PREVIEWMODEENABLED = new Guid(0x3cb1f27f, 0x0505, 0x4c5d, 0xae, 0x71, 0x0a, 0x55, 0x63, 0x44, 0xef, 0xa1);
        public static readonly Guid MFNETSOURCE_CLIENTGUID = new Guid(0x60a2c4a6, 0xf197, 0x4c14, 0xa5, 0xbf, 0x88, 0x83, 0xd, 0x24, 0x58, 0xaf);
        public static readonly Guid MFNETSOURCE_ENABLE_MSB = new Guid(0x3cb1f296, 0x0505, 0x4c5d, 0xae, 0x71, 0x0a, 0x55, 0x63, 0x44, 0xef, 0xa1);
        public static readonly Guid MFNETSOURCE_STREAM_LANGUAGE = new Guid(0x9ab44318, 0xf7cd, 0x4f2d, 0x8d, 0x6d, 0xfa, 0x35, 0xb4, 0x92, 0xce, 0xcb);
        public static readonly Guid MFNETSOURCE_LOGPARAMS = new Guid(0x64936ae8, 0x9418, 0x453a, 0x8c, 0xda, 0x3e, 0xa, 0x66, 0x8b, 0x35, 0x3b);
    }

    public static class MFServices
    {
        public static readonly Guid MF_TIMECODE_SERVICE = new Guid(0xa0d502a7, 0x0eb3, 0x4885, 0xb1, 0xb9, 0x9f, 0xeb, 0x0d, 0x08, 0x34, 0x54);
        public static readonly Guid MF_PROPERTY_HANDLER_SERVICE = new Guid(0xa3face02, 0x32b8, 0x41dd, 0x90, 0xe7, 0x5f, 0xef, 0x7c, 0x89, 0x91, 0xb5);
        public static readonly Guid MF_METADATA_PROVIDER_SERVICE = new Guid(0xdb214084, 0x58a4, 0x4d2e, 0xb8, 0x4f, 0x6f, 0x75, 0x5b, 0x2f, 0x7a, 0xd);
        public static readonly Guid MF_PMP_SERVER_CONTEXT = new Guid(0x2f00c910, 0xd2cf, 0x4278, 0x8b, 0x6a, 0xd0, 0x77, 0xfa, 0xc3, 0xa2, 0x5f);
        public static readonly Guid MF_QUALITY_SERVICES = new Guid(0xb7e2be11, 0x2f96, 0x4640, 0xb5, 0x2c, 0x28, 0x23, 0x65, 0xbd, 0xf1, 0x6c);
        public static readonly Guid MF_RATE_CONTROL_SERVICE = new Guid(0x866fa297, 0xb802, 0x4bf8, 0x9d, 0xc9, 0x5e, 0x3b, 0x6a, 0x9f, 0x53, 0xc9);
        public static readonly Guid MF_REMOTE_PROXY = new Guid(0x2f00c90e, 0xd2cf, 0x4278, 0x8b, 0x6a, 0xd0, 0x77, 0xfa, 0xc3, 0xa2, 0x5f);
        public static readonly Guid MF_SAMI_SERVICE = new Guid(0x49a89ae7, 0xb4d9, 0x4ef2, 0xaa, 0x5c, 0xf6, 0x5a, 0x3e, 0x5, 0xae, 0x4e);
        public static readonly Guid MF_SOURCE_PRESENTATION_PROVIDER_SERVICE = new Guid(0xe002aadc, 0xf4af, 0x4ee5, 0x98, 0x47, 0x05, 0x3e, 0xdf, 0x84, 0x04, 0x26);
        public static readonly Guid MF_TOPONODE_ATTRIBUTE_EDITOR_SERVICE = new Guid(0x65656e1a, 0x077f, 0x4472, 0x83, 0xef, 0x31, 0x6f, 0x11, 0xd5, 0x08, 0x7a);
        public static readonly Guid MF_WORKQUEUE_SERVICES = new Guid(0x8e37d489, 0x41e0, 0x413a, 0x90, 0x68, 0x28, 0x7c, 0x88, 0x6d, 0x8d, 0xda);
        public static readonly Guid MFNET_SAVEJOB_SERVICE = new Guid(0xb85a587f, 0x3d02, 0x4e52, 0x95, 0x65, 0x55, 0xd3, 0xec, 0x1e, 0x7f, 0xf7);
        public static readonly Guid MFNETSOURCE_STATISTICS_SERVICE = new Guid(0x3cb1f275, 0x0505, 0x4c5d, 0xae, 0x71, 0x0a, 0x55, 0x63, 0x44, 0xef, 0xa1);
        public static readonly Guid MR_AUDIO_POLICY_SERVICE = new Guid(0x911fd737, 0x6775, 0x4ab0, 0xa6, 0x14, 0x29, 0x78, 0x62, 0xfd, 0xac, 0x88);
        public static readonly Guid MR_POLICY_VOLUME_SERVICE = new Guid(0x1abaa2ac, 0x9d3b, 0x47c6, 0xab, 0x48, 0xc5, 0x95, 0x6, 0xde, 0x78, 0x4d);
        public static readonly Guid MR_STREAM_VOLUME_SERVICE = new Guid(0xf8b5fa2f, 0x32ef, 0x46f5, 0xb1, 0x72, 0x13, 0x21, 0x21, 0x2f, 0xb2, 0xc4);
        public static readonly Guid MR_VIDEO_RENDER_SERVICE = new Guid(0x1092a86c, 0xab1a, 0x459a, 0xa3, 0x36, 0x83, 0x1f, 0xbc, 0x4d, 0x11, 0xff);
        public static readonly Guid MR_VIDEO_MIXER_SERVICE = new Guid(0x73cd2fc, 0x6cf4, 0x40b7, 0x88, 0x59, 0xe8, 0x95, 0x52, 0xc8, 0x41, 0xf8);
        public static readonly Guid MR_VIDEO_ACCELERATION_SERVICE = new Guid(0xefef5175, 0x5c7d, 0x4ce2, 0xbb, 0xbd, 0x34, 0xff, 0x8b, 0xca, 0x65, 0x54);
        public static readonly Guid MR_BUFFER_SERVICE = new Guid(0xa562248c, 0x9ac6, 0x4ffc, 0x9f, 0xba, 0x3a, 0xf8, 0xf8, 0xad, 0x1a, 0x4d);
        public static readonly Guid MF_PMP_SERVICE = new Guid(0x2F00C90C, 0xD2CF, 0x4278, 0x8B, 0x6A, 0xD0, 0x77, 0xFA, 0xC3, 0xA2, 0x5F);
        public static readonly Guid MF_LOCAL_MFT_REGISTRATION_SERVICE = new Guid(0xddf5cf9c, 0x4506, 0x45aa, 0xab, 0xf0, 0x6d, 0x5d, 0x94, 0xdd, 0x1b, 0x4a);
    }

    public static class MFPKEY
    {
        public static readonly PropertyKey CATEGORY = new PropertyKey(new Guid(0xc57a84c0, 0x1a80, 0x40a3, 0x97, 0xb5, 0x92, 0x72, 0xa4, 0x3, 0xc8, 0xae), 0x02);
        public static readonly PropertyKey CLSID = new PropertyKey(new Guid(0xc57a84c0, 0x1a80, 0x40a3, 0x97, 0xb5, 0x92, 0x72, 0xa4, 0x3, 0xc8, 0xae), 0x01);
        public static readonly PropertyKey EXATTRIBUTE_SUPPORTED = new PropertyKey(new Guid(0x456fe843, 0x3c87, 0x40c0, 0x94, 0x9d, 0x14, 0x9, 0xc9, 0x7d, 0xab, 0x2c), 0x01);
        public static readonly PropertyKey SourceOpenMonitor = new PropertyKey(new Guid(0x074d4637, 0xb5ae, 0x465d, 0xaf, 0x17, 0x1a, 0x53, 0x8d, 0x28, 0x59, 0xdd), 0x02);
        public static readonly PropertyKey ASFMediaSource_ApproxSeek = new PropertyKey(new Guid(0xb4cd270f, 0x244d, 0x4969, 0xbb, 0x92, 0x3f, 0x0f, 0xb8, 0x31, 0x6f, 0x10), 0x01);
        public static readonly PropertyKey MULTICHANNEL_CHANNEL_MASK = new PropertyKey(new Guid(0x58bdaf8c, 0x3224, 0x4692, 0x86, 0xd0, 0x44, 0xd6, 0x5c, 0x5b, 0xf8, 0x2b), 0x01);
        public static readonly PropertyKey ASFMediaSource_IterativeSeekIfNoIndex = new PropertyKey(new Guid(0x170b65dc, 0x4a4e, 0x407a, 0xac, 0x22, 0x57, 0x7f, 0x50, 0xe4, 0xa3, 0x7c), 0x01);
        public static readonly PropertyKey ASFMediaSource_IterativeSeek_Max_Count = new PropertyKey(new Guid(0x170b65dc, 0x4a4e, 0x407a, 0xac, 0x22, 0x57, 0x7f, 0x50, 0xe4, 0xa3, 0x7c), 0x02);
        public static readonly PropertyKey ASFMediaSource_IterativeSeek_Tolerance_In_MilliSecond = new PropertyKey(new Guid(0x170b65dc, 0x4a4e, 0x407a, 0xac, 0x22, 0x57, 0x7f, 0x50, 0xe4, 0xa3, 0x7c), 0x03);
        public static readonly PropertyKey Content_DLNA_Profile_ID = new PropertyKey(new Guid(0xcfa31b45, 0x525d, 0x4998, 0xbb, 0x44, 0x3f, 0x7d, 0x81, 0x54, 0x2f, 0xa4), 0x01);
        public static readonly PropertyKey MediaSource_DisableReadAhead = new PropertyKey(new Guid(0x26366c14, 0xc5bf, 0x4c76, 0x88, 0x7b, 0x9f, 0x17, 0x54, 0xdb, 0x5f, 0x9), 0x01);

        public static readonly PropertyKey MFP_PKEY_StreamIndex = new PropertyKey(new Guid(0xa7cf9740, 0xe8d9, 0x4a87, 0xbd, 0x8e, 0x29, 0x67, 0x0, 0x1f, 0xd3, 0xad), 0x00);
        public static readonly PropertyKey MFP_PKEY_StreamRenderingResults = new PropertyKey(new Guid(0xa7cf9740, 0xe8d9, 0x4a87, 0xbd, 0x8e, 0x29, 0x67, 0x0, 0x1f, 0xd3, 0xad), 0x01);

        public static readonly PropertyKey SBESourceMode = new PropertyKey(new Guid(0x3fae10bb, 0xf859, 0x4192, 0xb5, 0x62, 0x18, 0x68, 0xd3, 0xda, 0x3a, 0x02), 0x01);
        public static readonly PropertyKey PMP_Creation_Callback = new PropertyKey(new Guid(0x28bb4de2, 0x26a2, 0x4870, 0xb7, 0x20, 0xd2, 0x6b, 0xbe, 0xb1, 0x49, 0x42), 0x01);
        public static readonly PropertyKey HTTP_ByteStream_Enable_Urlmon = new PropertyKey(new Guid(0xeda8afdf, 0xc171, 0x417f, 0x8d, 0x17, 0x2e, 0x09, 0x18, 0x30, 0x32, 0x92), 0x01);
        public static readonly PropertyKey HTTP_ByteStream_Urlmon_Bind_Flags = new PropertyKey(new Guid(0xeda8afdf, 0xc171, 0x417f, 0x8d, 0x17, 0x2e, 0x09, 0x18, 0x30, 0x32, 0x92), 0x02);
        public static readonly PropertyKey HTTP_ByteStream_Urlmon_Security_Id = new PropertyKey(new Guid(0xeda8afdf, 0xc171, 0x417f, 0x8d, 0x17, 0x2e, 0x09, 0x18, 0x30, 0x32, 0x92), 0x03);
        public static readonly PropertyKey HTTP_ByteStream_Urlmon_Window = new PropertyKey(new Guid(0xeda8afdf, 0xc171, 0x417f, 0x8d, 0x17, 0x2e, 0x09, 0x18, 0x30, 0x32, 0x92), 0x04);
        public static readonly PropertyKey HTTP_ByteStream_Urlmon_Callback_QueryService = new PropertyKey(new Guid(0xeda8afdf, 0xc171, 0x417f, 0x8d, 0x17, 0x2e, 0x09, 0x18, 0x30, 0x32, 0x92), 0x05);
        public static readonly PropertyKey MediaProtectionSystemId = new PropertyKey(new Guid(0x636b271d, 0xddc7, 0x49e9, 0xa6, 0xc6, 0x47, 0x38, 0x59, 0x62, 0xe5, 0xbd), 0x01);
        public static readonly PropertyKey MediaProtectionSystemContext = new PropertyKey(new Guid(0x636b271d, 0xddc7, 0x49e9, 0xa6, 0xc6, 0x47, 0x38, 0x59, 0x62, 0xe5, 0xbd), 0x02);
    }

    public static class CLSID
    {
        // Unknown
        public static readonly Guid MFASFINDEXER_TYPE_TIMECODE = new Guid(0x49815231, 0x6bad, 0x44fd, 0x81, 0xa, 0x3f, 0x60, 0x98, 0x4e, 0xc7, 0xfd );

        public static readonly Guid MF_DEVSOURCE_ATTRIBUTE_SOURCE_TYPE_AUDCAP_GUID = new Guid(0x14dd9a1c, 0x7cff, 0x41be, 0xb1, 0xb9, 0xba, 0x1a, 0xc6, 0xec, 0xb5, 0x71);
        public static readonly Guid MF_DEVSOURCE_ATTRIBUTE_SOURCE_TYPE_VIDCAP_GUID= new Guid( 0x8ac3587a, 0x4ae7, 0x42d8, 0x99, 0xe0, 0x0a, 0x60, 0x13, 0xee, 0xf9, 0x0f);

        public static readonly Guid MFCONNECTOR_SDI = new Guid(0x57cd5971, 0xce47, 0x11d9, 0x92, 0xdb, 0x00, 0x0b, 0xdb, 0x28, 0xff, 0x98);
        public static readonly Guid MFPROTECTION_ACP = new Guid(0xc3fd11c6, 0xf8b7, 0x4d20, 0xb0, 0x08, 0x1d, 0xb1, 0x7d, 0x61, 0xf2, 0xda);
        public static readonly Guid MFPROTECTION_CGMSA = new Guid(0xE57E69E9, 0x226B, 0x4d31, 0xB4, 0xE3, 0xD3, 0xDB, 0x00, 0x87, 0x36, 0xDD);
        public static readonly Guid MFPROTECTION_CONSTRICTAUDIO = new Guid(0xffc99b44, 0xdf48, 0x4e16, 0x8e, 0x66, 0x09, 0x68, 0x92, 0xc1, 0x57, 0x8a);
        public static readonly Guid MFPROTECTION_CONSTRICTVIDEO = new Guid(0x193370ce, 0xc5e4, 0x4c3a, 0x8a, 0x66, 0x69, 0x59, 0xb4, 0xda, 0x44, 0x42);
        public static readonly Guid MFPROTECTION_DISABLE = new Guid(0x8cc6d81b, 0xfec6, 0x4d8f, 0x96, 0x4b, 0xcf, 0xba, 0x0b, 0x0d, 0xad, 0x0d);
        public static readonly Guid MFPROTECTION_FFT = new Guid(0x462a56b2, 0x2866, 0x4bb6, 0x98, 0x0d, 0x6d, 0x8d, 0x9e, 0xdb, 0x1a, 0x8c);
        public static readonly Guid MFPROTECTION_HDCP = new Guid(0xAE7CC03D, 0xC828, 0x4021, 0xac, 0xb7, 0xd5, 0x78, 0xd2, 0x7a, 0xaf, 0x13);
        public static readonly Guid MFPROTECTION_TRUSTEDAUDIODRIVERS = new Guid(0x65bdf3d2, 0x0168, 0x4816, 0xa5, 0x33, 0x55, 0xd4, 0x7b, 0x02, 0x71, 0x01);
        public static readonly Guid MFPROTECTION_WMDRMOTA = new Guid(0xa267a6a1, 0x362e, 0x47d0, 0x88, 0x05, 0x46, 0x28, 0x59, 0x8a, 0x23, 0xe4);
        public static readonly Guid MFPROTECTIONATTRIBUTE_CONSTRICTVIDEO_IMAGESIZE = new Guid(0x8476fc, 0x4b58, 0x4d80, 0xa7, 0x90, 0xe7, 0x29, 0x76, 0x73, 0x16, 0x1d);
        public static readonly Guid MFPROTECTIONATTRIBUTE_HDCP_SRM = new Guid(0x6f302107, 0x3477, 0x4468, 0x8a, 0x8, 0xee, 0xf9, 0xdb, 0x10, 0xe2, 0xf);
        public static readonly Guid MFSampleExtension_DescrambleData = new Guid(0x43483be6, 0x4903, 0x4314, 0xb0, 0x32, 0x29, 0x51, 0x36, 0x59, 0x36, 0xfc);
        public static readonly Guid MFSampleExtension_SampleKeyID = new Guid(0x9ed713c8, 0x9b87, 0x4b26, 0x82, 0x97, 0xa9, 0x3b, 0x0c, 0x5a, 0x8a, 0xcc);
        public static readonly Guid MFSampleExtension_GenKeyFunc = new Guid(0x441ca1ee, 0x6b1f, 0x4501, 0x90, 0x3a, 0xde, 0x87, 0xdf, 0x42, 0xf6, 0xed);
        public static readonly Guid MFSampleExtension_GenKeyCtx = new Guid(0x188120cb, 0xd7da, 0x4b59, 0x9b, 0x3e, 0x92, 0x52, 0xfd, 0x37, 0x30, 0x1c);
        public static readonly Guid MFSampleExtension_PacketCrossOffsets = new Guid(0x2789671d, 0x389f, 0x40bb, 0x90, 0xd9, 0xc2, 0x82, 0xf7, 0x7f, 0x9a, 0xbd);
        public static readonly Guid MF_SampleProtectionSalt = new Guid(0x5403deee, 0xb9ee, 0x438f, 0xaa, 0x83, 0x38, 0x4, 0x99, 0x7e, 0x56, 0x9d);
        public static readonly Guid CLSID_MFSourceResolver = new Guid(0x90eab60f, 0xe43a, 0x4188, 0xbc, 0xc4, 0xe4, 0x7f, 0xdf, 0x04, 0x86, 0x8c);

        // Generic

        public static readonly Guid CLSID_MFSinkWriter = new Guid(0xa3bbfb17, 0x8273, 0x4e52, 0x9e, 0x0e, 0x97, 0x39, 0xdc, 0x88, 0x79, 0x90);
        public static readonly Guid CLSID_MFSourceReader = new Guid(0x1777133c, 0x0881, 0x411b, 0xa5, 0x77, 0xad, 0x54, 0x5f, 0x07, 0x14, 0xc4);

        public static readonly Guid MF_QUALITY_NOTIFY_PROCESSING_LATENCY = new Guid(0xf6b44af8, 0x604d, 0x46fe, 0xa9, 0x5d, 0x45, 0x47, 0x9b, 0x10, 0xc9, 0xbc);
        public static readonly Guid MF_QUALITY_NOTIFY_SAMPLE_LAG = new Guid(0x30d15206, 0xed2a, 0x4760, 0xbe, 0x17, 0xeb, 0x4a, 0x9f, 0x12, 0x29, 0x5c);
        public static readonly Guid MF_TIME_FORMAT_SEGMENT_OFFSET = new Guid(0xc8b8be77, 0x869c, 0x431d, 0x81, 0x2e, 0x16, 0x96, 0x93, 0xf6, 0x5a, 0x39);
        public static readonly Guid MF_TIME_FORMAT_ENTRY_RELATIVE = new Guid(0x4399f178, 0x46d3, 0x4504, 0xaf, 0xda, 0x20, 0xd3, 0x2e, 0x9b, 0xa3, 0x60);
        public static readonly Guid MFP_POSITIONTYPE_100NS = Guid.Empty;
    }

    public static class MFMediaType
    {
        /// <summary> From MFMediaType_Default </summary>
        public static readonly Guid Default = new Guid(0x81A412E6, 0x8103, 0x4B06, 0x85, 0x7F, 0x18, 0x62, 0x78, 0x10, 0x24, 0xAC);
        /// <summary> From MFMediaType_Audio </summary>
        public static readonly Guid Audio = new Guid(0x73647561, 0x0000, 0x0010, 0x80, 0x00, 0x00, 0xAA, 0x00, 0x38, 0x9B, 0x71);
        /// <summary> From MFMediaType_Video </summary>
        public static readonly Guid Video = new Guid(0x73646976, 0x0000, 0x0010, 0x80, 0x00, 0x00, 0xAA, 0x00, 0x38, 0x9B, 0x71);
        /// <summary> From MFMediaType_Protected </summary>
        public static readonly Guid Protected = new Guid(0x7b4b6fe6, 0x9d04, 0x4494, 0xbe, 0x14, 0x7e, 0x0b, 0xd0, 0x76, 0xc8, 0xe4);
        /// <summary> From MFMediaType_SAMI </summary>
        public static readonly Guid SAMI = new Guid(0xe69669a0, 0x3dcd, 0x40cb, 0x9e, 0x2e, 0x37, 0x08, 0x38, 0x7c, 0x06, 0x16);
        /// <summary> From MFMediaType_Script </summary>
        public static readonly Guid Script = new Guid(0x72178C22, 0xE45B, 0x11D5, 0xBC, 0x2A, 0x00, 0xB0, 0xD0, 0xF3, 0xF4, 0xAB);
        /// <summary> From MFMediaType_Image </summary>
        public static readonly Guid Image = new Guid(0x72178C23, 0xE45B, 0x11D5, 0xBC, 0x2A, 0x00, 0xB0, 0xD0, 0xF3, 0xF4, 0xAB);
        /// <summary> From MFMediaType_HTML </summary>
        public static readonly Guid HTML = new Guid(0x72178C24, 0xE45B, 0x11D5, 0xBC, 0x2A, 0x00, 0xB0, 0xD0, 0xF3, 0xF4, 0xAB);
        /// <summary> From MFMediaType_Binary </summary>
        public static readonly Guid Binary = new Guid(0x72178C25, 0xE45B, 0x11D5, 0xBC, 0x2A, 0x00, 0xB0, 0xD0, 0xF3, 0xF4, 0xAB);
        /// <summary> From MFMediaType_FileTransfer </summary>
        public static readonly Guid FileTransfer = new Guid(0x72178C26, 0xE45B, 0x11D5, 0xBC, 0x2A, 0x00, 0xB0, 0xD0, 0xF3, 0xF4, 0xAB);
        /// <summary> From MFMediaType_Stream </summary>
        public static readonly Guid Stream = new Guid(0xe436eb83, 0x524f, 0x11ce, 0x9f, 0x53, 0x00, 0x20, 0xaf, 0x0b, 0xa7, 0x70);

        public static readonly Guid Base = new Guid(0x00000000, 0x0000, 0x0010, 0x80, 0x00, 0x00, 0xaa, 0x00, 0x38, 0x9b, 0x71);
        public static readonly Guid PCM = new Guid(0x00000001, 0x0000, 0x0010, 0x80, 0x00, 0x00, 0xaa, 0x00, 0x38, 0x9b, 0x71);
        public static readonly Guid Float = new Guid(0x0003, 0x0000, 0x0010, 0x80, 0x00, 0x00, 0xaa, 0x00, 0x38, 0x9b, 0x71);
        public static readonly Guid DTS = new Guid(0x0008, 0x0000, 0x0010, 0x80, 0x00, 0x00, 0xaa, 0x00, 0x38, 0x9b, 0x71);
        public static readonly Guid Dolby_AC3_SPDIF = new Guid(0x0092, 0x0000, 0x0010, 0x80, 0x00, 0x00, 0xaa, 0x00, 0x38, 0x9b, 0x71);
        public static readonly Guid DRM = new Guid(0x0009, 0x0000, 0x0010, 0x80, 0x00, 0x00, 0xaa, 0x00, 0x38, 0x9b, 0x71);
        public static readonly Guid WMAudioV8 = new Guid(0x0161, 0x0000, 0x0010, 0x80, 0x00, 0x00, 0xaa, 0x00, 0x38, 0x9b, 0x71);
        public static readonly Guid WMAudioV9 = new Guid(0x0162, 0x0000, 0x0010, 0x80, 0x00, 0x00, 0xaa, 0x00, 0x38, 0x9b, 0x71);
        public static readonly Guid WMAudio_Lossless = new Guid(0x0163, 0x0000, 0x0010, 0x80, 0x00, 0x00, 0xaa, 0x00, 0x38, 0x9b, 0x71);
        public static readonly Guid WMASPDIF = new Guid(0x0164, 0x0000, 0x0010, 0x80, 0x00, 0x00, 0xaa, 0x00, 0x38, 0x9b, 0x71);
        public static readonly Guid MSP1 = new Guid(0x000A, 0x0000, 0x0010, 0x80, 0x00, 0x00, 0xaa, 0x00, 0x38, 0x9b, 0x71);
        public static readonly Guid MP3 = new Guid(0x0055, 0x0000, 0x0010, 0x80, 0x00, 0x00, 0xaa, 0x00, 0x38, 0x9b, 0x71);
        public static readonly Guid MPEG = new Guid(0x0050, 0x0000, 0x0010, 0x80, 0x00, 0x00, 0xaa, 0x00, 0x38, 0x9b, 0x71);
        public static readonly Guid AAC = new Guid(0x1610, 0x0000, 0x0010, 0x80, 0x00, 0x00, 0xaa, 0x00, 0x38, 0x9b, 0x71);
        public static readonly Guid ADTS = new Guid(0x1600, 0x0000, 0x0010, 0x80, 0x00, 0x00, 0xaa, 0x00, 0x38, 0x9b, 0x71);

        // {00000000-767a-494d-b478-f29d25dc9037}       MFMPEG4Format_Base
        public static readonly Guid MFMPEG4Format = new Guid(0x00000000, 0x767a, 0x494d, 0xb4, 0x78, 0xf2, 0x9d, 0x25, 0xdc, 0x90, 0x37);

        public static readonly Guid RGB32 = new Guid(22, 0x0000, 0x0010, 0x80, 0x00, 0x00, 0xaa, 0x00, 0x38, 0x9b, 0x71);
        public static readonly Guid ARGB32 = new Guid(21, 0x0000, 0x0010, 0x80, 0x00, 0x00, 0xaa, 0x00, 0x38, 0x9b, 0x71);
        public static readonly Guid RGB24 = new Guid(20, 0x0000, 0x0010, 0x80, 0x00, 0x00, 0xaa, 0x00, 0x38, 0x9b, 0x71);
        public static readonly Guid RGB555 = new Guid(24, 0x0000, 0x0010, 0x80, 0x00, 0x00, 0xaa, 0x00, 0x38, 0x9b, 0x71);
        public static readonly Guid RGB565 = new Guid(23, 0x0000, 0x0010, 0x80, 0x00, 0x00, 0xaa, 0x00, 0x38, 0x9b, 0x71);
        public static readonly Guid RGB8 = new Guid(41, 0x0000, 0x0010, 0x80, 0x00, 0x00, 0xaa, 0x00, 0x38, 0x9b, 0x71);
        public static readonly Guid AI44 = new FourCC("AI44").ToMediaSubtype();
        public static readonly Guid AYUV = new FourCC("AYUV").ToMediaSubtype();
        public static readonly Guid YUY2 = new FourCC("YUY2").ToMediaSubtype();
        public static readonly Guid YVYU = new FourCC("YVYU").ToMediaSubtype();
        public static readonly Guid YVU9 = new FourCC("YVU9").ToMediaSubtype();
        public static readonly Guid UYVY = new FourCC("UYVY").ToMediaSubtype();
        public static readonly Guid NV11 = new FourCC("NV11").ToMediaSubtype();
        public static readonly Guid NV12 = new FourCC("NV12").ToMediaSubtype();
        public static readonly Guid YV12 = new FourCC("YV12").ToMediaSubtype();
        public static readonly Guid I420 = new FourCC("I420").ToMediaSubtype();
        public static readonly Guid IYUV = new FourCC("IYUV").ToMediaSubtype();
        public static readonly Guid Y210 = new FourCC("Y210").ToMediaSubtype();
        public static readonly Guid Y216 = new FourCC("Y216").ToMediaSubtype();
        public static readonly Guid Y410 = new FourCC("Y410").ToMediaSubtype();
        public static readonly Guid Y416 = new FourCC("Y416").ToMediaSubtype();
        public static readonly Guid Y41P = new FourCC("Y41P").ToMediaSubtype();
        public static readonly Guid Y41T = new FourCC("Y41T").ToMediaSubtype();
        public static readonly Guid Y42T = new FourCC("Y42T").ToMediaSubtype();
        public static readonly Guid P210 = new FourCC("P210").ToMediaSubtype();
        public static readonly Guid P216 = new FourCC("P216").ToMediaSubtype();
        public static readonly Guid P010 = new FourCC("P010").ToMediaSubtype();
        public static readonly Guid P016 = new FourCC("P016").ToMediaSubtype();
        public static readonly Guid v210 = new FourCC("v210").ToMediaSubtype();
        public static readonly Guid v216 = new FourCC("v216").ToMediaSubtype();
        public static readonly Guid v410 = new FourCC("v410").ToMediaSubtype();
        public static readonly Guid MP43 = new FourCC("MP43").ToMediaSubtype();
        public static readonly Guid MP4S = new FourCC("MP4S").ToMediaSubtype();
        public static readonly Guid M4S2 = new FourCC("M4S2").ToMediaSubtype();
        public static readonly Guid MP4V = new FourCC("MP4V").ToMediaSubtype();
        public static readonly Guid WMV1 = new FourCC("WMV1").ToMediaSubtype();
        public static readonly Guid WMV2 = new FourCC("WMV2").ToMediaSubtype();
        public static readonly Guid WMV3 = new FourCC("WMV3").ToMediaSubtype();
        public static readonly Guid WVC1 = new FourCC("WVC1").ToMediaSubtype();
        public static readonly Guid MSS1 = new FourCC("MSS1").ToMediaSubtype();
        public static readonly Guid MSS2 = new FourCC("MSS2").ToMediaSubtype();
        public static readonly Guid MPG1 = new FourCC("MPG1").ToMediaSubtype();
        public static readonly Guid DVSL = new FourCC("dvsl").ToMediaSubtype();
        public static readonly Guid DVSD = new FourCC("dvsd").ToMediaSubtype();
        public static readonly Guid DVHD = new FourCC("dvhd").ToMediaSubtype();
        public static readonly Guid DV25 = new FourCC("dv25").ToMediaSubtype();
        public static readonly Guid DV50 = new FourCC("dv50").ToMediaSubtype();
        public static readonly Guid DVH1 = new FourCC("dvh1").ToMediaSubtype();
        public static readonly Guid DVC = new FourCC("dvc ").ToMediaSubtype();
        public static readonly Guid H264 = new FourCC("H264").ToMediaSubtype();
        public static readonly Guid MJPG = new FourCC("MJPG").ToMediaSubtype();
        public static readonly Guid O420 = new FourCC("420O").ToMediaSubtype();

        public static readonly Guid MPEG2 = new Guid(0xe06d8026, 0xdb46, 0x11cf, 0xb4, 0xd1, 0x00, 0x80, 0x5f, 0x6c, 0xbb, 0xea);
        public static readonly Guid MFVideoFormat_H264_ES = new Guid(0x3f40f4f0, 0x5622, 0x4ff8, 0xb6, 0xd8, 0xa1, 0x7a, 0x58, 0x4b, 0xee, 0x5e);
        public static readonly Guid MFAudioFormat_Dolby_AC3 = new Guid(0xe06d802c, 0xdb46, 0x11cf, 0xb4, 0xd1, 0x00, 0x80, 0x05f, 0x6c, 0xbb, 0xea);
        public static readonly Guid MFAudioFormat_Dolby_DDPlus = new Guid(0xa7fb87af, 0x2d02, 0x42fb, 0xa4, 0xd4, 0x5, 0xcd, 0x93, 0x84, 0x3b, 0xdd);
        public static readonly Guid MFAudioFormat_QCELP = new Guid(0x5E7F6D41, 0xB115, 0x11D0, 0xBA, 0x91, 0x00, 0x80, 0x5F, 0xB4, 0xB9, 0x7E);
        public static readonly Guid MF_MT_VIDEO_3D = new Guid(0xcb5e88cf, 0x7b5b, 0x476b, 0x85, 0xaa, 0x1c, 0xa5, 0xae, 0x18, 0x75, 0x55);
    }

    public static class MFImageFormat
    {
        public static readonly Guid JPEG = new Guid(0x19e4a5aa, 0x5662, 0x4fc5, 0xa0, 0xc0, 0x17, 0x58, 0x02, 0x8e, 0x10, 0x57);
        public static readonly Guid RGB32 = new Guid(0x00000016, 0x0000, 0x0010, 0x80, 0x00, 0x00, 0xaa, 0x00, 0x38, 0x9b, 0x71);
    }

    public static class MFStreamFormat
    {
        public static readonly Guid MPEG2Transport = new Guid(0xe06d8023, 0xdb46, 0x11cf, 0xb4, 0xd1, 0x00, 0x80, 0x5f, 0x6c, 0xbb, 0xea);
        public static readonly Guid MPEG2Program = new Guid(0x263067d1, 0xd330, 0x45dc, 0xb6, 0x69, 0x34, 0xd9, 0x86, 0xe4, 0xe3, 0xe1);
    }

    #endregion

    #region Declarations

#if ALLOW_UNTESTED_INTERFACES

    [Flags, UnmanagedName("Unnamed enum")]
    public enum MF_RESOLUTION
    {
        None = 0,
        MediaSource = 0x1,
        ByteStream = 0x2,
        ContentDoesNotHaveToMatchExtensionOrMimeType = 0x10,
        KeepByteStreamAliveOnFail = 0x20,
        DisableLocalPlugins = 0x40,
        PluginControlPolicyApprovedOnly = 0x80,
        PluginControlPolicyWebOnly = 0x100,
        Read = 0x10000,
        Write = 0x20000
    }

    [UnmanagedName("MF_VIDEO_PROCESSOR_ROTATION")]
    public enum MF_VIDEO_PROCESSOR_ROTATION
    {
        None = 0,
        Normal = 1
    }

    [UnmanagedName("MF_VIDEO_PROCESSOR_MIRROR")]
    public enum MF_VIDEO_PROCESSOR_MIRROR
    {
        None = 0,
        Horizontal = 1,
        Vertical = 2
    }

    [StructLayout(LayoutKind.Sequential), UnmanagedName("MF_BYTE_STREAM_CACHE_RANGE")]
    public class MF_BYTE_STREAM_CACHE_RANGE
    {
        long qwStartOffset;
        long qwEndOffset;
    }

    [UnmanagedName("MFAudioConstriction")]
    public enum MFAudioConstriction
    {
        Off = 0,
        C48_16 = (Off + 1),
        C44_16 = (C48_16 + 1),
        C14_14 = (C44_16 + 1),
        Mute = (C14_14 + 1)
    }

    [UnmanagedName("MFVideoRotationFormat")]
    public enum MFVideoRotationFormat
    {
        R0 = 0,
        R90 = 90,
        R180 = 180,
        R270 = 270,
    }

    [UnmanagedName("MFVideo3DSampleFormat")]
    public enum MFVideo3DSampleFormat
    {
        Packed = 0,
        MultiView = 1
    }

    [UnmanagedName("MFVideo3DFormat")]
    public enum MFVideo3DFormat
    {
        BaseView = 0,
        MultiView = 1,
        PackedLeftRight = 2,
        PackedTopBottom = 3
    }

    [UnmanagedName("MFNETSOURCE_STATISTICS_IDS")]
    public enum MFNETSOURCE_STATISTICS_IDS
    {
        RECVPACKETS_ID = 0,
        LOSTPACKETS_ID,
        RESENDSREQUESTED_ID,
        RESENDSRECEIVED_ID,
        RECOVEREDBYECCPACKETS_ID,
        RECOVEREDBYRTXPACKETS_ID,
        OUTPACKETS_ID,
        RECVRATE_ID,
        AVGBANDWIDTHBPS_ID,
        BYTESRECEIVED_ID,
        PROTOCOL_ID,
        TRANSPORT_ID,
        CACHE_STATE_ID,
        LINKBANDWIDTH_ID,
        CONTENTBITRATE_ID,
        SPEEDFACTOR_ID,
        BUFFERSIZE_ID,
        BUFFERPROGRESS_ID,
        LASTBWSWITCHTS_ID,
        SEEKRANGESTART_ID,
        SEEKRANGEEND_ID,
        BUFFERINGCOUNT_ID,
        INCORRECTLYSIGNEDPACKETS_ID,
        SIGNEDSESSION_ID,
        MAXBITRATE_ID,
        RECEPTION_QUALITY_ID,
        RECOVEREDPACKETS_ID,
        VBR_ID,
        DOWNLOADPROGRESS_ID,
        UNPREDEFINEDPROTOCOLNAME_ID
    }

    [Flags, UnmanagedName("MFOUTPUTATTRIBUTE_ *")]
    public enum MFOutputAttribute
    {
        None = 0,
        Digital = 0x00000001,
        NonstandardImplementation = 0x00000002,
        Video = 0x00000004,
        Compressed = 0x00000008,
        Software = 0x00000010,
        Bus = 0x00000020,
        BusImplementation = 0x0000FF00
    }

    [Flags, UnmanagedName("MFNetAuthenticationFlags")]
    public enum MFNetAuthenticationFlags
    {
        None = 0,
        Proxy = 0x00000001,
        ClearText = 0x00000002,
        LoggedOnUser = 0x00000004
    }

    [Flags, UnmanagedName("MFNetCredentialOptions")]
    public enum MFNetCredentialOptions
    {
        None = 0,
        Save = 0x00000001,
        DontCache = 0x00000002,
        AllowClearText = 0x00000004,
    }

    [UnmanagedName("MFStandardVideoFormat")]
    public enum MFStandardVideoFormat
    {
        reserved = 0,
        NTSC = (reserved + 1),
        PAL = (NTSC + 1),
        DVD_NTSC = (PAL + 1),
        DVD_PAL = (DVD_NTSC + 1),
        DV_PAL = (DVD_PAL + 1),
        DV_NTSC = (DV_PAL + 1),
        ATSC_SD480i = (DV_NTSC + 1),
        ATSC_HD1080i = (ATSC_SD480i + 1),
        ATSC_HD720p = (ATSC_HD1080i + 1)
    }

    [UnmanagedName("MFNetCredentialRequirements")]
    public enum MFNetCredentialRequirements
    {
        None = 0,
        RequirePrompt = 0x00000001,
        RequireSaveSelected = 0x00000002
    }

    [UnmanagedName("MF_QUALITY_DROP_MODE")]
    public enum MFQualityDropMode
    {
        None,
        Mode1,
        Mode2,
        Mode3,
        Mode4,
        Mode5,
        NumDropModes
    }

    [UnmanagedName("MF_QUALITY_LEVEL")]
    public enum MFQualityLevel
    {
        Normal,
        NormalMinus1,
        NormalMinus2,
        NormalMinus3,
        NormalMinus4,
        NormalMinus5,
        NumQualityLevels
    }

    [UnmanagedName("MFNETSOURCE_PROTOCOL_TYPE")]
    public enum MFNetSourceProtocolType
    {
        Undefined,
        Http,
        Rtsp,
        File,
        MultiCast
    }

    [UnmanagedName("MFPolicyManagerAction")]
    public enum MFPolicyManagerAction
    {
        Copy = 2,
        Export = 3,
        Extract = 4,
        Last = 7,
        No = 0,
        Play = 1,
        Reserved1 = 5,
        Reserved2 = 6,
        Reserved3 = 7
    }

    [Flags, UnmanagedName("MF_CONNECT_METHOD")]
    public enum MF_CONNECT_METHOD
    {
        Direct = 0x00000000,
        AllowConverter = 0x00000001,
        AllowDecoder = 0x00000003,
        ResolveIndependentOutputTypes = 0x00000004,
        AsOptional = 0x00010000,
        AsOptionalBranch = 0x00020000,
    }

    [Flags, UnmanagedName("MF_AUDIO_RENDERER_ATTRIBUTE_FLAGS_* defines")]
    public enum MF_AUDIO_RENDERER_ATTRIBUTE_FLAGS
    {
        None = 0x00000000,
        CrossProcess = 0x00000001,
        NoPersist = 0x00000002,
        DontAllowFormatChanges = 0x00000004
    }

    [Flags, UnmanagedName("MF_TRANSCODE_ADJUST_PROFILE_FLAGS")]
    public enum MF_TRANSCODE_ADJUST_PROFILE_FLAGS
    {
        Default = 0,
        UseSourceAttributes = 1
    }

    [Flags, UnmanagedName("MF_TRANSCODE_TOPOLOGYMODE_FLAGS")]
    public enum MF_TRANSCODE_TOPOLOGYMODE_FLAGS
    {
        SoftwareOnly = 0,
        HardwareAllowed = 1
    }

    [Flags, UnmanagedName("MFTOPOLOGY_DXVA_MODE")]
    public enum MFTOPOLOGY_DXVA_MODE
    {
        Default = 0,
        None = 1,
        Full = 2
    }

    [UnmanagedName("MFTOPOLOGY_HARDWARE_MODE")]
    public enum MFTOPOLOGY_HARDWARE_MODE
    {
        SoftwareOnly = 0,
        UseHardware = 1,
    }

    [Flags, UnmanagedName("MF_QUALITY_ADVISE_FLAGS")]
    public enum MFQualityAdviseFlags
    {
        None = 0,
        CannotKeepUp = 0x1
    }

    [UnmanagedName("MFASYNC_WORKQUEUE_TYPE")]
    public enum MFASYNC_WORKQUEUE_TYPE
    {
        StandardWorkqueue = 0,
        WindowWorkqueue = 1,
        MultiThreadedWorkqueue = 2
    }

    [StructLayout(LayoutKind.Sequential), UnmanagedName("MPEG2VIDEOINFO")]
    public struct Mpeg2VideoInfo
    {
        VideoInfoHeader2 hdr;
        int dwStartTimeCode;        //  ?? not used for DVD ??
        int cbSequenceHeader;       // is 0 for DVD (no sequence header)
        int dwProfile;              // use enum MPEG2Profile
        int dwLevel;                // use enum MPEG2Level
        int dwFlags;                // use AMMPEG2_* defines.  Reject connection if undefined bits are not 0
        int[] dwSequenceHeader;     // DWORD instead of Byte for alignment purposes
        //   For MPEG-2, if a sequence_header is included, the sequence_extension
        //   should also be included
    }

    [StructLayout(LayoutKind.Explicit, Pack = 8), UnmanagedName("unnamed internal struct")]
    public struct Unnamed1
    {
        [FieldOffset(0)]
        public double d;
        [FieldOffset(0)]
        public int u32;
        [FieldOffset(0)]
        public long u64;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 4), UnmanagedName("MF_LEAKY_BUCKET_PAIR")]
    public struct MF_LeakyBucketPair
    {
        public int dwBitrate;
        public int msBufferWindow;
    }

    [StructLayout(LayoutKind.Sequential), UnmanagedName("MFBYTESTREAM_BUFFERING_PARAMS")]
    public struct MFByteStreamBufferingParams
    {
        public long cbTotalFileSize;
        public long cbPlayableDataSize;
        public IntPtr prgBuckets;
        public int cBuckets;
        public long qwNetBufferingTime;
        public long qwExtraBufferingTimeDuringSeek;
        public long qwPlayDuration;
        public float dRate;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 4), UnmanagedName("MFINPUTTRUSTAUTHORITY_ACCESS_PARAMS")]
    public struct MFInputTrustAuthorityAccessParams
    {
        public int dwSize;
        public int dwVer;
        public int cbSignatureOffset;
        public int cbSignatureSize;
        public int cbExtensionOffset;
        public int cbExtensionSize;
        public int cActions;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 1)]
        public MFInputTrustAuthorityAction[] rgOutputActions;
    }

    [StructLayout(LayoutKind.Sequential), UnmanagedName("MFINPUTTRUSTAUTHORITY_ACTION")]
    public struct MFInputTrustAuthorityAction
    {
        public MFPolicyManagerAction Action;
        public IntPtr pbTicket;
        public int cbTicket;
    }

    [StructLayout(LayoutKind.Sequential), UnmanagedName("MFNetCredentialManagerGetParam")]
    public struct MFNetCredentialManagerGetParam
    {
        [MarshalAs(UnmanagedType.Error)]
        public int hrOp;
        public int fAllowLoggedOnUser;
        public int fClearTextPackage;
        [MarshalAs(UnmanagedType.LPWStr)]
        public string pszUrl;
        [MarshalAs(UnmanagedType.LPWStr)]
        public string pszSite;
        [MarshalAs(UnmanagedType.LPWStr)]
        public string pszRealm;
        [MarshalAs(UnmanagedType.LPWStr)]
        public string pszPackage;
        public int nRetries;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 8), UnmanagedName("MFTOPONODE_ATTRIBUTE_UPDATE")]
    public struct MFTopoNodeAttributeUpdate
    {
        public long NodeId;
        public Guid guidAttributeKey;
        public MFAttributeType attrType;
        public Unnamed1 u1;
    }

    [StructLayout(LayoutKind.Sequential), UnmanagedName("MF_TRANSCODE_SINK_INFO")]
    public struct MFTranscodeSinkInfo
    {
        public int dwVideoStreamID;
        public IMFMediaType pVideoMediaType;
        public int dwAudioStreamID;
        public IMFMediaType pAudioMediaType;
    }

    [StructLayout(LayoutKind.Sequential), UnmanagedName("MT_ARBITRARY_HEADER")]
    public struct MT_ARBITRARY_HEADER
    {
        public Guid majortype;
        public Guid subtype;
        public bool bFixedSizeSamples;
        public bool bTemporalCompression;
        public int lSampleSize;
        public Guid formattype;
    }

#endif

    [UnmanagedName("MFRATE_DIRECTION")]
    public enum MFRateDirection
    {
        Forward = 0,
        Reverse
    }

    [UnmanagedName("MFSTREAMSINK_MARKER_TYPE")]
    public enum MFStreamSinkMarkerType
    {
        Default,
        EndOfSegment,
        Tick,
        Event
    }

    [Flags, UnmanagedName("MFSequencerTopologyFlags")]
    public enum MFSequencerTopologyFlags
    {
        None = 0,
        Last = 0x00000001
    }

    [Flags, UnmanagedName("MFSESSION_GETFULLTOPOLOGY_FLAGS")]
    public enum MFSessionGetFullTopologyFlags
    {
        None = 0x0,
        Current = 0x1
    }

    [Flags, UnmanagedName("MFSESSIONCAP_* defines")]
    public enum MFSessionCapabilities
    {
        None = 0x00000000,
        Start = 0x00000001,
        Seek = 0x00000002,
        Pause = 0x00000004,
        RateForward = 0x00000010,
        RateReverse = 0x00000020
    }

    [Flags, UnmanagedName("MFSESSION_SETTOPOLOGY_FLAGS")]
    public enum MFSessionSetTopologyFlags
    {
        None = 0x0,
        Immediate = 0x1,
        NoResolution = 0x2,
        ClearCurrent = 0x4
    }

    [UnmanagedName("MFWaveFormatExConvertFlags")]
    public enum MFWaveFormatExConvertFlags
    {
        Normal = 0,
        ForceExtensible = 1
    }

    [UnmanagedName("MF_OBJECT_TYPE")]
    public enum MFObjectType
    {
        MediaSource,
        ByteStream,
        Invalid
    }

    [Flags, UnmanagedName("unnamed enum")]
    public enum MFResolution
    {
        None = 0x0,
        MediaSource = 0x00000001,
        ByteStream = 0x00000002,
        ContentDoesNotHaveToMatchExtensionOrMimeType = 0x00000010,
        KeepByteStreamAliveOnFail = 0x00000020,
        Read = 0x00010000,
        Write = 0x00020000
    }

    [UnmanagedName("MF_TOPOSTATUS")]
    public enum MFTopoStatus
    {
        // MF_TOPOSTATUS_INVALID: Invalid value; will not be sent
        Invalid = 0,

        // READY: The topology has been put in place and is
        // ready to start.  All GetService calls to the Media Session will use
        // this topology.
        Ready = 100,

        // STARTED_SOURCE: The Media Session has started to read
        // and process data from the Media Source(s) in this topology.
        StartedSource = 200,

        // MF_TOPOSTATUS_DYNAMIC_CHANGED: The topology has been dynamic changed
        // due to the format change.
        DynamicChanged = 210,

        // SINK_SWITCHED: The Media Sinks in the pipeline have
        // switched from a previous topology to this topology.
        // Note that this status does not get sent for the first topology;
        // applications can assume that the sinks are playing the first
        // topology when they receive MESessionStarted.
        SinkSwitched = 300,

        // ENDED: Playback of this topology is complete.
        // Before deleting this topology, however, the application should wait
        // for either MESessionEnded or the STARTED_SOURCE status
        // on the next topology to ensure that the Media Session is no longer
        // using this topology.
        Ended = 400,
    }

    [UnmanagedName("MFSTARTUP_* defines")]
    public enum MFStartup
    {
        NoSocket = 0x1,
        Lite = 0x1,
        Full = 0
    }

    [UnmanagedName("MFCLOCK_STATE")]
    public enum MFClockState
    {
        Invalid,
        Running,
        Stopped,
        Paused
    }

    [Flags, UnmanagedName("MFCLOCK_CHARACTERISTICS_FLAGS")]
    public enum MFClockCharacteristicsFlags
    {
        None = 0,
        Frequency10Mhz = 0x2,
        AlwaysRunning = 0x4,
        IsSystemClock = 0x8
    }

    [UnmanagedName("MF_TOPOLOGY_TYPE")]
    public enum MFTopologyType
    {
        Max = -1,
        OutputNode = 0,
        SourcestreamNode = 1,
        TeeNode = 3,
        TransformNode = 2
    }

    [Flags, UnmanagedName("MFMEDIASOURCE_CHARACTERISTICS")]
    public enum MFMediaSourceCharacteristics
    {
        None = 0,
        IsLive = 0x1,
        CanSeek = 0x2,
        CanPause = 0x4,
        HasSlowSeek = 0x8,
        HasMultiplePresentations = 0x10,
        CanSkipForward = 0x20,
        CanSkipBackward = 0x40
    }

    [Flags, UnmanagedName("MEDIASINK_ defines")]
    public enum MFMediaSinkCharacteristics
    {
        None = 0,
        FixedStreams = 0x00000001,
        CannotMatchClock = 0x00000002,
        Rateless = 0x00000004,
        ClockRequired = 0x00000008,
        CanPreroll = 0x00000010,
        RequireReferenceMediaType = 0x00000020
    }

    [UnmanagedName("MF_ATTRIBUTE_SERIALIZE_OPTIONS"), Flags]
    public enum MFAttributeSerializeOptions
    {
        None = 0,
        UnknownByRef = 0x00000001
    }

    [UnmanagedName("MF_FILE_ACCESSMODE")]
    public enum MFFileAccessMode
    {
        None = 0,
        Read = 1,
        Write = 2,
        ReadWrite = 3
    }

    [UnmanagedName("MF_FILE_OPENMODE")]
    public enum MFFileOpenMode
    {
        FailIfNotExist = 0,
        FailIfExist = 1,
        ResetIfExist = 2,
        AppendIfExist = 3,
        DeleteIfExist = 4
    }

    [Flags, UnmanagedName("MF_FILE_FLAGS")]
    public enum MFFileFlags
    {
        None = 0,
        NoBuffering = 0x1,
        AllowWriteSharing = 0x2
    }

    [UnmanagedName("MF_URL_TRUST_STATUS")]
    public enum MFURLTrustStatus
    {
        Untrusted,
        Trusted,
        Tampered
    }

    [Flags, UnmanagedName("MFPMPSESSION_CREATION_FLAGS")]
    public enum MFPMPSessionCreationFlags
    {
        None = 0,
        UnprotectedProcess = 0x1
    }

    [Flags, UnmanagedName("MFCLOCK_RELATIONAL_FLAGS")]
    public enum MFClockRelationalFlags
    {
        None = 0,
        JitterNeverAhead = 0x1
    }

    [UnmanagedName("MFSHUTDOWN_STATUS")]
    public enum MFShutdownStatus
    {
        Initiated,
        Completed
    }

    [Flags, UnmanagedName("MFTIMER_FLAGS")]
    public enum MFTimeFlags
    {
        None = 0,
        Relative = 0x00000001
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1), UnmanagedName("MFAYUVSample")]
    public struct MFAYUVSample
    {
        public byte bCrValue;
        public byte bCbValue;
        public byte bYValue;
        public byte bSampleAlpha8;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1), UnmanagedName("MFARGB")]
    public struct MFARGB
    {
        public byte rgbBlue;
        public byte rgbGreen;
        public byte rgbRed;
        public byte rgbAlpha;
    }

    [StructLayout(LayoutKind.Explicit, Pack = 1), UnmanagedName("MFPaletteEntry")]
    public struct MFPaletteEntry
    {
        [FieldOffset(0)]
        public MFARGB ARGB;
        [FieldOffset(0)]
        public MFAYUVSample AYCbCr;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 8), UnmanagedName("MFCLOCK_PROPERTIES")]
    public struct MFClockProperties
    {
        public long qwCorrelationRate;
        public Guid guidClockId;
        public MFClockRelationalFlags dwClockFlags;
        public long qwClockFrequency;
        public int dwClockTolerance;
        public int dwClockJitter;
    }

    #endregion

    #region Interfaces

#if ALLOW_UNTESTED_INTERFACES

    [ComImport, System.Security.SuppressUnmanagedCodeSecurity,
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown),
    Guid("3C9B2EB9-86D5-4514-A394-F56664F9F0D8")]
    public interface IMFMediaSourceEx : IMFMediaSource
    {
        #region IMFMediaEventGenerator methods

        [PreserveSig]
        new int GetEvent(
            [In] MFEventFlag dwFlags,
            [MarshalAs(UnmanagedType.Interface)] out IMFMediaEvent ppEvent
        );

        [PreserveSig]
        new int BeginGetEvent(
            [In, MarshalAs(UnmanagedType.Interface)] IMFAsyncCallback pCallback,
            [In, MarshalAs(UnmanagedType.IUnknown)] object o
        );

        [PreserveSig]
        new int EndGetEvent(
            IMFAsyncResult pResult,
            out IMFMediaEvent ppEvent
        );

        [PreserveSig]
        new int QueueEvent(
            [In] MediaEventType met,
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid guidExtendedType,
            [In] int hrStatus,
            [In, MarshalAs(UnmanagedType.LPStruct)] ConstPropVariant pvValue
        );

        #endregion

        #region IMFMediaSource methods

        [PreserveSig]
        new int GetCharacteristics(
            out MFMediaSourceCharacteristics pdwCharacteristics
        );

        [PreserveSig]
        new int CreatePresentationDescriptor(
            out IMFPresentationDescriptor ppPresentationDescriptor
        );

        [PreserveSig]
        new int Start(
            [In, MarshalAs(UnmanagedType.Interface)] IMFPresentationDescriptor pPresentationDescriptor,
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid pguidTimeFormat,
            [In, MarshalAs(UnmanagedType.LPStruct)] ConstPropVariant pvarStartPosition
        );

        [PreserveSig]
        new int Stop();

        [PreserveSig]
        new int Pause();

        [PreserveSig]
        new int Shutdown();

        #endregion

        [PreserveSig]
        int GetSourceAttributes( 
            out IMFAttributes ppAttributes
        );
        
        [PreserveSig]
        int GetStreamAttributes( 
            int dwStreamIdentifier,
            out IMFAttributes ppAttributes
        );
        
        [PreserveSig]
        int SetD3DManager( 
            [MarshalAs(UnmanagedType.IUnknown)] object pManager
        );
    }

    [ComImport, System.Security.SuppressUnmanagedCodeSecurity,
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown),
    Guid("545b3a48-3283-4f62-866f-a62d8f598f9f")]
    public interface IMFVideoSampleAllocatorEx : IMFVideoSampleAllocator
    {
        #region IMFVideoSampleAllocator methods

        [PreserveSig]
        new int SetDirectXManager(
            [In, MarshalAs(UnmanagedType.IUnknown)] object pManager
        );

        [PreserveSig]
        new int UninitializeSampleAllocator();

        [PreserveSig]
        new int InitializeSampleAllocator(
            [In] int cRequestedFrames,
            [In, MarshalAs(UnmanagedType.Interface)] IMFMediaType pMediaType
        );

        [PreserveSig]
        new int AllocateSample(
            [MarshalAs(UnmanagedType.Interface)] out IMFSample ppSample
        );

        #endregion

        [PreserveSig]
        int InitializeSampleAllocatorEx( 
            int cInitialSamples,
            int cMaximumSamples,
            IMFAttributes pAttributes,
            IMFMediaType pMediaType
        );        
    }

    [ComImport, System.Security.SuppressUnmanagedCodeSecurity,
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown),
    Guid("A3F675D5-6119-4f7f-A100-1D8B280F0EFB")]
    public interface IMFVideoProcessorControl
    {
        [PreserveSig]
        int SetBorderColor( 
            MFARGB pBorderColor
        );
        
        [PreserveSig]
        int SetSourceRectangle( 
            Rectangle pSrcRect
        );
        
        [PreserveSig]
        int SetDestinationRectangle( 
            Rectangle pDstRect
        );
        
        [PreserveSig]
        int SetMirror( 
            MF_VIDEO_PROCESSOR_MIRROR eMirror
        );
        
        [PreserveSig]
        int SetRotation( 
            MF_VIDEO_PROCESSOR_ROTATION eRotation
        );
        
        [PreserveSig]
        int SetConstrictionSize(
            MFSize pConstrictionSize
        );
    }

    [ComImport, System.Security.SuppressUnmanagedCodeSecurity,
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown),
    Guid("26AFEA53-D9ED-42B5-AB80-E64F9EE34779")]
    public interface IMFSeekInfo
    {
        [PreserveSig]
        int GetNearestKeyFrames( 
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid pguidTimeFormat,
            [In, MarshalAs(UnmanagedType.LPStruct)] ConstPropVariant pvarStartPosition,
            [In, Out, MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(PVMarshaler))] PropVariant pvarPreviousKeyFrame,
            [In, Out, MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(PVMarshaler))] PropVariant pvarNextKeyFrame
        );        
    }

    [ComImport, System.Security.SuppressUnmanagedCodeSecurity,
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown),
    Guid("96bf961b-40fe-42f1-ba9d-320238b49700")]
    public interface IMFWorkQueueServicesEx : IMFWorkQueueServices
    {
        #region IMFWorkQueueServices methods

        [PreserveSig]
        new int BeginRegisterTopologyWorkQueuesWithMMCSS(
            [In, MarshalAs(UnmanagedType.Interface)] IMFAsyncCallback pCallback,
            [MarshalAs(UnmanagedType.IUnknown)] object pState
        );

        [PreserveSig]
        new int EndRegisterTopologyWorkQueuesWithMMCSS(
            IMFAsyncResult pResult
        );

        [PreserveSig]
        new int BeginUnregisterTopologyWorkQueuesWithMMCSS(
            [In, MarshalAs(UnmanagedType.Interface)] IMFAsyncCallback pCallback,
            [MarshalAs(UnmanagedType.IUnknown)] object pState
        );

        [PreserveSig]
        new int EndUnregisterTopologyWorkQueuesWithMMCSS(
            IMFAsyncResult pResult
        );

        [PreserveSig]
        new int GetTopologyWorkQueueMMCSSClass(
            [In] int dwTopologyWorkQueueId,
            [Out, MarshalAs(UnmanagedType.LPWStr)] string pwszClass,
            [In, Out] ref int pcchClass
        );

        [PreserveSig]
        new int GetTopologyWorkQueueMMCSSTaskId(
            [In] int dwTopologyWorkQueueId,
            out int pdwTaskId
        );

        [PreserveSig]
        new int BeginRegisterPlatformWorkQueueWithMMCSS(
            [In] int dwPlatformWorkQueue,
            [In, MarshalAs(UnmanagedType.LPWStr)] string wszClass,
            [In] int dwTaskId,
            [In, MarshalAs(UnmanagedType.Interface)] IMFAsyncCallback pCallback,
            [MarshalAs(UnmanagedType.IUnknown)] object pState
        );

        [PreserveSig]
        new int EndRegisterPlatformWorkQueueWithMMCSS(
            IMFAsyncResult pResult,
            out int pdwTaskId
        );

        [PreserveSig]
        new int BeginUnregisterPlatformWorkQueueWithMMCSS(
            [In] int dwPlatformWorkQueue,
            [In, MarshalAs(UnmanagedType.Interface)] IMFAsyncCallback pCallback,
            [MarshalAs(UnmanagedType.IUnknown)] object pState
        );

        [PreserveSig]
        new int EndUnregisterPlatformWorkQueueWithMMCSS(
            IMFAsyncResult pResult
        );

        [PreserveSig]
        new int GetPlaftormWorkQueueMMCSSClass(
            [In] int dwPlatformWorkQueueId,
            [Out, MarshalAs(UnmanagedType.LPWStr)] string pwszClass,
            [In, Out] ref int pcchClass
        );

        [PreserveSig]
        new int GetPlatformWorkQueueMMCSSTaskId(
            [In] int dwPlatformWorkQueueId,
            out int pdwTaskId
        );

        #endregion

        [PreserveSig]
        int GetTopologyWorkQueueMMCSSPriority( 
            int dwTopologyWorkQueueId,
            out int plPriority
        );
        
        [PreserveSig]
        int BeginRegisterPlatformWorkQueueWithMMCSSEx( 
            int dwPlatformWorkQueue,
            [Out, MarshalAs(UnmanagedType.LPWStr)] string wszClass,
            int dwTaskId,
            int lPriority,
            IMFAsyncCallback pCallback,
            [MarshalAs(UnmanagedType.IUnknown)] object pState
        );
        
        [PreserveSig]
        int GetPlatformWorkQueueMMCSSPriority( 
            int dwPlatformWorkQueueId,
            out int plPriority
        );        
    }

    [ComImport, System.Security.SuppressUnmanagedCodeSecurity,
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown),
    Guid("03910848-AB16-4611-B100-17B88AE2F248")]
    public interface IMFRealTimeClientEx
    {
        [PreserveSig]
        int RegisterThreadsEx( 
            ref int pdwTaskIndex,
            [In, MarshalAs(UnmanagedType.LPWStr)] string wszClassName,
            int lBasePriority
        );
        
        [PreserveSig]
        int UnregisterThreads( );
        
        [PreserveSig]
        int SetWorkQueueEx( 
            int dwMultithreadedWorkQueueId,
            int lWorkItemBasePriority
        );        
    }

    [ComImport, System.Security.SuppressUnmanagedCodeSecurity,
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown),
    Guid("64976BFA-FB61-4041-9069-8C9A5F659BEB")]
    public interface IMFByteStreamTimeSeek
    {
        [PreserveSig]
        int IsTimeSeekSupported( 
            out bool pfTimeSeekIsSupported
        );
        
        [PreserveSig]
        int TimeSeek( 
            long qwTimePosition
        );
        
        [PreserveSig]
        int GetTimeSeekResult( 
            out long pqwStartTime,
            out long pqwStopTime,
            out long pqwDuration
        );        
    }

    [ComImport, System.Security.SuppressUnmanagedCodeSecurity,
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown),
    Guid("71CE469C-F34B-49EA-A56B-2D2A10E51149")]
    public interface IMFByteStreamCacheControl2 : IMFByteStreamCacheControl
    {
        #region IMFByteStreamCacheControl methods

        [PreserveSig]
        new int StopBackgroundTransfer();

        #endregion

        [PreserveSig]
        int GetByteRanges( 
            out int pcRanges,
            [Out, MarshalAs(UnmanagedType.LPArray, SizeParamIndex=0)] MF_BYTE_STREAM_CACHE_RANGE[] ppRanges
        );
        
        [PreserveSig]
        int SetCacheLimit( 
            long qwBytes
        );
        
        [PreserveSig]
        int IsBackgroundTransferActive( 
            out bool pfActive
        );
    }

    [ComImport, System.Security.SuppressUnmanagedCodeSecurity,
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown),
    Guid("091878a3-bf11-4a5c-bc9f-33995b06ef2d")]
    public interface IMFNetResourceFilter
    {
        [PreserveSig]
        int OnRedirect( 
            [In, MarshalAs(UnmanagedType.LPWStr)] string pszUrl,
            [MarshalAs(UnmanagedType.VariantBool)] out bool pvbCancel
        );
        
        [PreserveSig]
        int OnSendingRequest( 
            [In, MarshalAs(UnmanagedType.LPWStr)] string pszUrl
        );
    }

    [ComImport, System.Security.SuppressUnmanagedCodeSecurity,
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown),
    Guid("84d2054a-3aa1-4728-a3b0-440a418cf49c")]
    public interface IMFPMPHostApp
    {
        [PreserveSig]
        int LockProcess();
        
        [PreserveSig]
        int UnlockProcess();
        
        [PreserveSig]
        int ActivateClassById( 
            [In, MarshalAs(UnmanagedType.LPWStr)] string id,
            IStream pStream,
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid riid,
            [MarshalAs(UnmanagedType.IUnknown)] out object ppv
        );        
    }

    [ComImport, System.Security.SuppressUnmanagedCodeSecurity,
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown),
    Guid("c004f646-be2c-48f3-93a2-a0983eba1108")]
    public interface IMFPMPClientApp
    {
        [PreserveSig]
        int SetPMPHost( 
            IMFPMPHostApp pPMPHost
        );        
    }

    [ComImport, System.Security.SuppressUnmanagedCodeSecurity,
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown),
    Guid("ef5dc845-f0d9-4ec9-b00c-cb5183d38434")]
    public interface IMFProtectedEnvironmentAccess
    {
        [PreserveSig]
        int Call( 
            int inputLength,
            IntPtr input,
            int outputLength,
            IntPtr output
        );        
    }

    [ComImport, System.Security.SuppressUnmanagedCodeSecurity,
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown),
    Guid("4a724bca-ff6a-4c07-8e0d-7a358421cf06")]
    public interface IMFSignedLibrary
    {
        [PreserveSig]
        int GetProcedureAddress( 
            [In, MarshalAs(UnmanagedType.LPWStr)] string name,
            out IntPtr address
        );        
    }
    
    [ComImport, System.Security.SuppressUnmanagedCodeSecurity,
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown),
    Guid("fff4af3a-1fc1-4ef9-a29b-d26c49e2f31a")]
    public interface IMFSystemId
    {
        [PreserveSig]
        int GetData( 
            out int size,
            out IntPtr data
        );
        
        [PreserveSig]
        int Setup( 
            int stage,
            int cbIn,
            IntPtr pbIn,
            out int pcbOut,
            out IntPtr ppbOut
        );        
    }

    [ComImport, System.Security.SuppressUnmanagedCodeSecurity,
    Guid("86CBC910-E533-4751-8E3B-F19B5B806A03"),
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IMFVideoSampleAllocator
    {
        [PreserveSig]
        int SetDirectXManager(
            [In, MarshalAs(UnmanagedType.IUnknown)] object pManager
            );

        [PreserveSig]
        int UninitializeSampleAllocator();

        [PreserveSig]
        int InitializeSampleAllocator(
            [In] int cRequestedFrames,
            [In, MarshalAs(UnmanagedType.Interface)] IMFMediaType pMediaType
            );

        [PreserveSig]
        int AllocateSample(
            [MarshalAs(UnmanagedType.Interface)] out IMFSample ppSample
            );
    }

    [ComImport, System.Security.SuppressUnmanagedCodeSecurity,
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown),
    Guid("A0638C2B-6465-4395-9AE7-A321A9FD2856")]
    public interface IMFAudioPolicy
    {
        [PreserveSig]
        int SetGroupingParam(
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid rguidClass
            );

        [PreserveSig]
        int GetGroupingParam(
            out Guid pguidClass
            );

        [PreserveSig]
        int SetDisplayName(
            [In, MarshalAs(UnmanagedType.LPWStr)] string pszName
            );

        [PreserveSig]
        int GetDisplayName(
            [MarshalAs(UnmanagedType.LPWStr)] out string pszName
            );

        [PreserveSig]
        int SetIconPath(
            [In, MarshalAs(UnmanagedType.LPWStr)] string pszPath
            );

        [PreserveSig]
        int GetIconPath(
            [MarshalAs(UnmanagedType.LPWStr)] out string pszPath
            );
    }

    [ComImport, System.Security.SuppressUnmanagedCodeSecurity,
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown),
    Guid("76B1BBDB-4EC8-4F36-B106-70A9316DF593")]
    public interface IMFAudioStreamVolume
    {
        [PreserveSig]
        int GetChannelCount(
            out int pdwCount
            );

        [PreserveSig]
        int SetChannelVolume(
            [In] int dwIndex,
            [In] float fLevel
            );

        [PreserveSig]
        int GetChannelVolume(
            [In] int dwIndex,
            out float pfLevel
            );

        [PreserveSig]
        int SetAllVolumes(
            [In] int dwCount,
            [In] ref float pfVolumes
            );

        [PreserveSig]
        int GetAllVolumes(
            [In] int dwCount,
            out float pfVolumes
            );
    }

    [ComImport, System.Security.SuppressUnmanagedCodeSecurity,
    Guid("6D66D782-1D4F-4DB7-8C63-CB8C77F1EF5E"),
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IMFByteStreamBuffering
    {
        [PreserveSig]
        int SetBufferingParams(
            [In] ref MFByteStreamBufferingParams pParams
            );

        [PreserveSig]
        int EnableBuffering(
            [In] int fEnable
            );

        [PreserveSig]
        int StopBuffering();
    }

    [ComImport, System.Security.SuppressUnmanagedCodeSecurity,
    Guid("D19F8E98-B126-4446-890C-5DCB7AD71453"),
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IMFInputTrustAuthority
    {
        [PreserveSig]
        int GetDecrypter(
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid riid,
            [MarshalAs(UnmanagedType.Interface)] out object ppv
            );

        [PreserveSig]
        int RequestAccess(
            [In] MFPolicyManagerAction Action,
            [MarshalAs(UnmanagedType.Interface)] out IMFActivate ppContentEnablerActivate
            );

        [PreserveSig]
        int GetPolicy(
            [In] MFPolicyManagerAction Action,
            [MarshalAs(UnmanagedType.Interface)] out IMFOutputPolicy ppPolicy
            );

        [PreserveSig]
        int BindAccess(
            [In] ref MFInputTrustAuthorityAccessParams pParam
            );

        [PreserveSig]
        int UpdateAccess(
            [In] ref MFInputTrustAuthorityAccessParams pParam
            );

        [PreserveSig]
        int Reset();
    }

    [ComImport, System.Security.SuppressUnmanagedCodeSecurity,
    Guid("5DFD4B2A-7674-4110-A4E6-8A68FD5F3688"),
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IMFMediaSinkPreroll
    {
        [PreserveSig]
        int NotifyPreroll(
            [In] long hnsUpcomingStartTime
            );
    }

    [ComImport, System.Security.SuppressUnmanagedCodeSecurity,
    Guid("0E1D600A-C9F3-442D-8C51-A42D2D49452F"),
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IMFMediaSourcePresentationProvider
    {
        [PreserveSig]
        int ForceEndOfPresentation(
            [In, MarshalAs(UnmanagedType.Interface)] IMFPresentationDescriptor pPresentationDescriptor
            );
    }

    [ComImport, System.Security.SuppressUnmanagedCodeSecurity,
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown),
    Guid("5B87EF6A-7ED8-434F-BA0E-184FAC1628D1")]
    public interface IMFNetCredential
    {
        [PreserveSig]
        int SetUser(
            [In] ref byte pbData,
            [In] int cbData,
            [In] int fDataIsEncrypted
            );

        [PreserveSig]
        int SetPassword(
            [In] ref byte pbData,
            [In] int cbData,
            [In] int fDataIsEncrypted
            );

        [PreserveSig]
        int GetUser(
            out byte pbData,
            [In, Out] ref int pcbData,
            [In] int fEncryptData
            );

        [PreserveSig]
        int GetPassword(
            out byte pbData,
            [In, Out] ref int pcbData,
            [In, MarshalAs(UnmanagedType.Bool)] bool fEncryptData
            );

        [PreserveSig]
        int LoggedOnUser(
            [MarshalAs(UnmanagedType.Bool)] out bool pfLoggedOnUser
            );
    }

    [ComImport, System.Security.SuppressUnmanagedCodeSecurity,
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown),
    Guid("5B87EF6C-7ED8-434F-BA0E-184FAC1628D1")]
    public interface IMFNetCredentialCache
    {
        [PreserveSig]
        int GetCredential(
            [In, MarshalAs(UnmanagedType.LPWStr)] string pszUrl,
            [In, MarshalAs(UnmanagedType.LPWStr)] string pszRealm,
            [In] MFNetAuthenticationFlags dwAuthenticationFlags,
            [MarshalAs(UnmanagedType.Interface)] out IMFNetCredential ppCred,
            out MFNetCredentialRequirements pdwRequirementsFlags
            );

        [PreserveSig]
        int SetGood(
            [In, MarshalAs(UnmanagedType.Interface)] IMFNetCredential pCred,
            [In] bool fGood
            );

        [PreserveSig]
        int SetUserOptions(
            [In, MarshalAs(UnmanagedType.Interface)] IMFNetCredential pCred,
            [In] MFNetCredentialOptions dwOptionsFlags
            );
    }

    [ComImport, System.Security.SuppressUnmanagedCodeSecurity,
    Guid("5B87EF6B-7ED8-434F-BA0E-184FAC1628D1"),
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IMFNetCredentialManager
    {
        [PreserveSig]
        int BeginGetCredentials(
            [In] ref MFNetCredentialManagerGetParam pParam,
            [In, MarshalAs(UnmanagedType.Interface)] IMFAsyncCallback pCallback,
            [In, MarshalAs(UnmanagedType.IUnknown)] object pState
            );

        [PreserveSig]
        int EndGetCredentials(
            [In, MarshalAs(UnmanagedType.Interface)] IMFAsyncResult pResult,
            [MarshalAs(UnmanagedType.Interface)] out IMFNetCredential ppCred
            );

        [PreserveSig]
        int SetGood(
            [In, MarshalAs(UnmanagedType.Interface)] IMFNetCredential pCred,
            [In] bool fGood
            );
    }

    [ComImport, System.Security.SuppressUnmanagedCodeSecurity,
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown),
    Guid("E9CD0383-A268-4BB4-82DE-658D53574D41")]
    public interface IMFNetProxyLocator
    {
        [PreserveSig]
        int FindFirstProxy(
            [In, MarshalAs(UnmanagedType.LPWStr)] string pszHost,
            [In, MarshalAs(UnmanagedType.LPWStr)] string pszUrl,
            [In, MarshalAs(UnmanagedType.Bool)] bool fReserved
            );

        [PreserveSig]
        int FindNextProxy();

        [PreserveSig]
        int RegisterProxyResult(
            [In, MarshalAs(UnmanagedType.Error)] int hrOp
            );

        [PreserveSig]
        int GetCurrentProxy(
            [Out, MarshalAs(UnmanagedType.LPWStr)] string pszStr,
            [In, Out] ref int pcchStr
            );

        [PreserveSig]
        int Clone(
            [MarshalAs(UnmanagedType.Interface)] out IMFNetProxyLocator ppProxyLocator
            );
    }

    [ComImport, System.Security.SuppressUnmanagedCodeSecurity,
    Guid("E9CD0384-A268-4BB4-82DE-658D53574D41"),
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IMFNetProxyLocatorFactory
    {
        [PreserveSig]
        int CreateProxyLocator(
            [In, MarshalAs(UnmanagedType.LPWStr)] string pszProtocol,
            [MarshalAs(UnmanagedType.Interface)] out IMFNetProxyLocator ppProxyLocator
            );
    }

    [ComImport, System.Security.SuppressUnmanagedCodeSecurity,
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown),
    Guid("7BE19E73-C9BF-468A-AC5A-A5E8653BEC87")]
    public interface IMFNetSchemeHandlerConfig
    {
        [PreserveSig]
        int GetNumberOfSupportedProtocols(
            out int pcProtocols
            );

        [PreserveSig]
        int GetSupportedProtocolType(
            [In] int nProtocolIndex,
            out MFNetSourceProtocolType pnProtocolType
            );

        [PreserveSig]
        int ResetProtocolRolloverSettings();
    }

    [ComImport, System.Security.SuppressUnmanagedCodeSecurity,
    Guid("09EF5BE3-C8A7-469E-8B70-73BF25BB193F"),
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IMFObjectReferenceStream
    {
        [PreserveSig]
        int SaveReference(
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid riid,
            [In, MarshalAs(UnmanagedType.IUnknown)] object pUnk
            );

        [PreserveSig]
        int LoadReference(
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid riid,
            [MarshalAs(UnmanagedType.Interface)] out object ppv
            );
    }

    [ComImport, System.Security.SuppressUnmanagedCodeSecurity,
    Guid("7F00F10A-DAED-41AF-AB26-5FDFA4DFBA3C"),
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IMFOutputPolicy : IMFAttributes
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
        int GenerateRequiredSchemas(
            [In] MFOutputAttribute dwAttributes,
            [In, MarshalAs(UnmanagedType.Struct)] Guid guidOutputSubType,
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid rgGuidProtectionSchemasSupported,
            [In] int cProtectionSchemasSupported,
            [MarshalAs(UnmanagedType.Interface)] out IMFCollection ppRequiredProtectionSchemas
            );

        [PreserveSig]
        int GetOriginatorID(
            out Guid pguidOriginatorID
            );

        [PreserveSig]
        int GetMinimumGRLVersion(
            out int pdwMinimumGRLVersion
            );
    }

    [ComImport, System.Security.SuppressUnmanagedCodeSecurity,
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown),
    Guid("7BE0FC5B-ABD9-44FB-A5C8-F50136E71599")]
    public interface IMFOutputSchema : IMFAttributes
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
        int GetSchemaType(
            out Guid pguidSchemaType
            );

        [PreserveSig]
        int GetConfigurationData(
            out int pdwVal
            );

        [PreserveSig]
        int GetOriginatorID(
            out Guid pguidOriginatorID
            );
    }

    [ComImport, System.Security.SuppressUnmanagedCodeSecurity,
    Guid("D19F8E94-B126-4446-890C-5DCB7AD71453"),
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IMFOutputTrustAuthority
    {
        [PreserveSig]
        int GetAction(
            out MFPolicyManagerAction pAction
            );

        [PreserveSig]
        int SetPolicy(
            [In, MarshalAs(UnmanagedType.Interface)] ref IMFOutputPolicy ppPolicy,
            [In] int nPolicy,
            [Out] IntPtr ppbTicket,
            out int pcbTicket
            );
    }

    [ComImport, System.Security.SuppressUnmanagedCodeSecurity,
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown),
    Guid("6C4E655D-EAD8-4421-B6B9-54DCDBBDF820")]
    public interface IMFPMPClient
    {
        [PreserveSig]
        int SetPMPHost(
            [In, MarshalAs(UnmanagedType.Interface)] IMFPMPHost pPMPHost
            );
    }

    [ComImport, System.Security.SuppressUnmanagedCodeSecurity,
    Guid("F70CA1A9-FDC7-4782-B994-ADFFB1C98606"),
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IMFPMPHost
    {
        [PreserveSig]
        int LockProcess();

        [PreserveSig]
        int UnlockProcess();

        [PreserveSig]
        int CreateObjectByCLSID(
            [MarshalAs(UnmanagedType.LPStruct)] Guid clsid,
            IStream pStream,
            [MarshalAs(UnmanagedType.LPStruct)] Guid riid,
            [MarshalAs(UnmanagedType.Interface)] object ppv
            );
    }

    [ComImport, System.Security.SuppressUnmanagedCodeSecurity,
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown),
    Guid("994E23AF-1CC2-493C-B9FA-46F1CB040FA4")]
    public interface IMFPMPServer
    {
        [PreserveSig]
        int LockProcess();

        [PreserveSig]
        int UnlockProcess();

        [PreserveSig]
        int CreateObjectByCLSID(
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid clsid,
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid riid,
            [MarshalAs(UnmanagedType.Interface)] out object ppObject
            );
    }

    [ComImport, System.Security.SuppressUnmanagedCodeSecurity,
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown),
    Guid("EC15E2E9-E36B-4F7C-8758-77D452EF4CE7")]
    public interface IMFQualityAdvise
    {
        [PreserveSig]
        int SetDropMode(
            [In] MFQualityDropMode eDropMode
            );

        [PreserveSig]
        int SetQualityLevel(
            [In] MFQualityLevel eQualityLevel
            );

        [PreserveSig]
        int GetDropMode(
            out MFQualityDropMode peDropMode
            );

        [PreserveSig]
        int GetQualityLevel(
            out MFQualityLevel peQualityLevel
            );

        [PreserveSig]
        int DropTime(
            [In] long hnsAmountToDrop
            );
    }

    [ComImport, System.Security.SuppressUnmanagedCodeSecurity,
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown),
    Guid("8D009D86-5B9F-4115-B1FC-9F80D52AB8AB")]
    public interface IMFQualityManager
    {
        [PreserveSig]
        int NotifyTopology(
            [In, MarshalAs(UnmanagedType.Interface)] IMFTopology pTopology
            );

        [PreserveSig]
        int NotifyPresentationClock(
            [In, MarshalAs(UnmanagedType.Interface)] IMFPresentationClock pClock
            );

        [PreserveSig]
        int NotifyProcessInput(
            [In, MarshalAs(UnmanagedType.Interface)] IMFTopologyNode pNode,
            [In] int lInputIndex,
            [In, MarshalAs(UnmanagedType.Interface)] IMFSample pSample
            );

        [PreserveSig]
        int NotifyProcessOutput(
            [In, MarshalAs(UnmanagedType.Interface)] IMFTopologyNode pNode,
            [In] int lOutputIndex,
            [In, MarshalAs(UnmanagedType.Interface)] IMFSample pSample
            );

        [PreserveSig]
        int NotifyQualityEvent(
            [In, MarshalAs(UnmanagedType.IUnknown)] object pObject,
            [In, MarshalAs(UnmanagedType.Interface)] IMFMediaEvent pEvent
            );

        [PreserveSig]
        int Shutdown();
    }

    [ComImport, System.Security.SuppressUnmanagedCodeSecurity,
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown),
    Guid("2347D60B-3FB5-480C-8803-8DF3ADCD3EF0")]
    public interface IMFRealTimeClient
    {
        [PreserveSig]
        int RegisterThreads(
            [In] int dwTaskIndex,
            [In, MarshalAs(UnmanagedType.LPWStr)] string wszClass
            );

        [PreserveSig]
        int UnregisterThreads();

        [PreserveSig]
        int SetWorkQueue(
            [In] int dwWorkQueueId
            );
    }

    [ComImport, System.Security.SuppressUnmanagedCodeSecurity,
    Guid("1CDE6309-CAE0-4940-907E-C1EC9C3D1D4A"),
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IMFRemoteDesktopPlugin
    {
        [PreserveSig]
        int UpdateTopology(
            [In, Out, MarshalAs(UnmanagedType.Interface)] IMFTopology pTopology
            );
    }

    [ComImport, System.Security.SuppressUnmanagedCodeSecurity,
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown),
    Guid("994E23AD-1CC2-493C-B9FA-46F1CB040FA4")]
    public interface IMFRemoteProxy
    {
        [PreserveSig]
        int GetRemoteObject(
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid riid,
            [MarshalAs(UnmanagedType.Interface)] out object ppv
            );

        [PreserveSig]
        int GetRemoteHost(
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid riid,
            [MarshalAs(UnmanagedType.Interface)] out object ppv
            );
    }

    [ComImport, System.Security.SuppressUnmanagedCodeSecurity,
    Guid("A7E025DD-5303-4A62-89D6-E747E1EFAC73"),
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IMFSAMIStyle
    {
        [PreserveSig]
        int GetStyleCount(
            out int pdwCount
            );

        [PreserveSig]
        int GetStyles(
            [In, Out, MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(PVMarshaler))] PropVariant pPropVarStyleArray
            );

        [PreserveSig]
        int SetSelectedStyle(
            [In, MarshalAs(UnmanagedType.LPWStr)] string pwszStyle
            );

        [PreserveSig]
        int GetSelectedStyle(
            [MarshalAs(UnmanagedType.LPWStr)] out string ppwszStyle
            );
    }

    [ComImport, System.Security.SuppressUnmanagedCodeSecurity,
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown),
    Guid("8C7B80BF-EE42-4B59-B1DF-55668E1BDCA8")]
    public interface IMFSampleGrabberSinkCallback : IMFClockStateSink
    {
        #region IMFClockStateSink methods

        [PreserveSig]
        new int OnClockStart(
            [In] long hnsSystemTime,
            [In] long llClockStartOffset
            );

        [PreserveSig]
        new int OnClockStop(
            [In] long hnsSystemTime
            );

        [PreserveSig]
        new int OnClockPause(
            [In] long hnsSystemTime
            );

        [PreserveSig]
        new int OnClockRestart(
            [In] long hnsSystemTime
            );

        [PreserveSig]
        new int OnClockSetRate(
            [In] long hnsSystemTime,
            [In] float flRate
            );

        #endregion

        [PreserveSig]
        int OnSetPresentationClock(
            [In, MarshalAs(UnmanagedType.Interface)] IMFPresentationClock pPresentationClock
            );

        [PreserveSig]
        int OnProcessSample(
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid guidMajorMediaType,
            [In] int dwSampleFlags, // must be zero
            [In] long llSampleTime,
            [In] long llSampleDuration,
            [In] ref byte pSampleBuffer,
            [In] int dwSampleSize
            );

        [PreserveSig]
        int OnShutdown();
    }

    [ComImport, System.Security.SuppressUnmanagedCodeSecurity,
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown),
    Guid("8E36395F-C7B9-43C4-A54D-512B4AF63C95")]
    public interface IMFSampleProtection
    {
        [PreserveSig]
        int GetInputProtectionVersion(
            out int pdwVersion
            );

        [PreserveSig]
        int GetOutputProtectionVersion(
            out int pdwVersion
            );

        [PreserveSig]
        int GetProtectionCertificate(
            [In] int dwVersion,
            [Out] IntPtr ppCert,
            out int pcbCert
            );

        [PreserveSig]
        int InitOutputProtection(
            [In] int dwVersion,
            [In] int dwOutputId,
            [In] ref byte pbCert,
            [In] int cbCert,
            [Out] IntPtr ppbSeed,
            out int pcbSeed
            );

        [PreserveSig]
        int InitInputProtection(
            [In] int dwVersion,
            [In] int dwInputId,
            [In] ref byte pbSeed,
            [In] int cbSeed
            );
    }

    [ComImport, System.Security.SuppressUnmanagedCodeSecurity,
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown),
    Guid("E9931663-80BF-4C6E-98AF-5DCF58747D1F")]
    public interface IMFSaveJob
    {
        [PreserveSig]
        int BeginSave(
            [In, MarshalAs(UnmanagedType.Interface)] IMFByteStream pStream,
            [In, MarshalAs(UnmanagedType.Interface)] IMFAsyncCallback pCallback,
            [In, MarshalAs(UnmanagedType.IUnknown)] object pState
            );

        [PreserveSig]
        int EndSave(
            [In, MarshalAs(UnmanagedType.Interface)] IMFAsyncResult pResult
            );

        [PreserveSig]
        int CancelSave();

        [PreserveSig]
        int GetProgress(
            out int pdwPercentComplete
            );
    }

    [ComImport, System.Security.SuppressUnmanagedCodeSecurity,
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown),
    Guid("6D4C7B74-52A0-4BB7-B0DB-55F29F47A668")]
    public interface IMFSchemeHandler
    {
        [PreserveSig]
        int BeginCreateObject(
            [In, MarshalAs(UnmanagedType.LPWStr)] string pwszURL,
            [In] MFResolution dwFlags,
            [In, MarshalAs(UnmanagedType.Interface)] IPropertyStore pProps,
            [MarshalAs(UnmanagedType.IUnknown)] out object ppIUnknownCancelCookie,
            [In, MarshalAs(UnmanagedType.Interface)] IMFAsyncCallback pCallback,
            [In, MarshalAs(UnmanagedType.IUnknown)] object pUnkState
            );

        [PreserveSig]
        int EndCreateObject(
            [In, MarshalAs(UnmanagedType.Interface)] IMFAsyncResult pResult,
            out MFObjectType pObjectType,
            [MarshalAs(UnmanagedType.IUnknown)] out object ppObject
            );

        [PreserveSig]
        int CancelObjectCreation(
            [In, MarshalAs(UnmanagedType.IUnknown)] object pIUnknownCancelCookie
            );
    }

    [ComImport, System.Security.SuppressUnmanagedCodeSecurity,
    Guid("D0AE555D-3B12-4D97-B060-0990BC5AEB67"),
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IMFSecureChannel
    {
        [PreserveSig]
        int GetCertificate(
            [Out] IntPtr ppCert,
            out int pcbCert
            );

        [PreserveSig]
        int SetupSession(
            [In] ref byte pbEncryptedSessionKey,
            [In] int cbSessionKey
            );
    }

    [ComImport, System.Security.SuppressUnmanagedCodeSecurity,
    Guid("089EDF13-CF71-4338-8D13-9E569DBDC319"),
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IMFSimpleAudioVolume
    {
        [PreserveSig]
        int SetMasterVolume(
            [In] float fLevel
            );

        [PreserveSig]
        int GetMasterVolume(
            out float pfLevel
            );

        [PreserveSig]
        int SetMute(
            [In, MarshalAs(UnmanagedType.Bool)] bool bMute
            );

        [PreserveSig]
        int GetMute(
            [MarshalAs(UnmanagedType.Bool)] out bool pbMute
            );
    }

    [ComImport, System.Security.SuppressUnmanagedCodeSecurity,
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown),
    Guid("DE9A6157-F660-4643-B56A-DF9F7998C7CD")]
    public interface IMFTopoLoader
    {
        [PreserveSig]
        int Load(
            [In, MarshalAs(UnmanagedType.Interface)] IMFTopology pInputTopo,
            [MarshalAs(UnmanagedType.Interface)] out IMFTopology ppOutputTopo,
            [In, MarshalAs(UnmanagedType.Interface)] IMFTopology pCurrentTopo
            );
    }

    [ComImport, System.Security.SuppressUnmanagedCodeSecurity,
    Guid("676AA6DD-238A-410D-BB99-65668D01605A"),
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IMFTopologyNodeAttributeEditor
    {
        [PreserveSig]
        int UpdateNodeAttributes(
            [In] long TopoId,
            [In] int cUpdates,
            [In] ref MFTopoNodeAttributeUpdate pUpdates
            );
    }

    [ComImport, System.Security.SuppressUnmanagedCodeSecurity,
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown),
    Guid("542612C4-A1B8-4632-B521-DE11EA64A0B0")]
    public interface IMFTrustedInput
    {
        [PreserveSig]
        int GetInputTrustAuthority(
            [In] int dwStreamID,
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid riid,
            [MarshalAs(UnmanagedType.IUnknown)] out object ppunkObject
            );
    }

    [ComImport, System.Security.SuppressUnmanagedCodeSecurity,
    Guid("D19F8E95-B126-4446-890C-5DCB7AD71453"),
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IMFTrustedOutput
    {
        [PreserveSig]
        int GetOutputTrustAuthorityCount(
            out int pcOutputTrustAuthorities
            );

        [PreserveSig]
        int GetOutputTrustAuthorityByIndex(
            [In] int dwIndex,
            [MarshalAs(UnmanagedType.Interface)] out IMFOutputTrustAuthority ppauthority
            );

        [PreserveSig]
        int IsFinal(
            [MarshalAs(UnmanagedType.Bool)] out bool pfIsFinal
            );
    }

    [ComImport, System.Security.SuppressUnmanagedCodeSecurity,
    Guid("35FE1BB8-A3A9-40FE-BBEC-EB569C9CCCA3"),
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IMFWorkQueueServices
    {
        [PreserveSig]
        int BeginRegisterTopologyWorkQueuesWithMMCSS(
            [In, MarshalAs(UnmanagedType.Interface)] IMFAsyncCallback pCallback,
            [MarshalAs(UnmanagedType.IUnknown)] object pState
            );

        [PreserveSig]
        int EndRegisterTopologyWorkQueuesWithMMCSS(
            IMFAsyncResult pResult
            );

        [PreserveSig]
        int BeginUnregisterTopologyWorkQueuesWithMMCSS(
            [In, MarshalAs(UnmanagedType.Interface)] IMFAsyncCallback pCallback,
            [MarshalAs(UnmanagedType.IUnknown)] object pState
            );

        [PreserveSig]
        int EndUnregisterTopologyWorkQueuesWithMMCSS(
            IMFAsyncResult pResult
            );

        [PreserveSig]
        int GetTopologyWorkQueueMMCSSClass(
            [In] int dwTopologyWorkQueueId,
            [Out, MarshalAs(UnmanagedType.LPWStr)] string pwszClass,
            [In, Out] ref int pcchClass
            );

        [PreserveSig]
        int GetTopologyWorkQueueMMCSSTaskId(
            [In] int dwTopologyWorkQueueId,
            out int pdwTaskId
            );

        [PreserveSig]
        int BeginRegisterPlatformWorkQueueWithMMCSS(
            [In] int dwPlatformWorkQueue,
            [In, MarshalAs(UnmanagedType.LPWStr)] string wszClass,
            [In] int dwTaskId,
            [In, MarshalAs(UnmanagedType.Interface)] IMFAsyncCallback pCallback,
            [MarshalAs(UnmanagedType.IUnknown)] object pState
            );

        [PreserveSig]
        int EndRegisterPlatformWorkQueueWithMMCSS(
            IMFAsyncResult pResult,
            out int pdwTaskId
            );

        [PreserveSig]
        int BeginUnregisterPlatformWorkQueueWithMMCSS(
            [In] int dwPlatformWorkQueue,
            [In, MarshalAs(UnmanagedType.Interface)] IMFAsyncCallback pCallback,
            [MarshalAs(UnmanagedType.IUnknown)] object pState
            );

        [PreserveSig]
        int EndUnregisterPlatformWorkQueueWithMMCSS(
            IMFAsyncResult pResult
            );

        [PreserveSig]
        int GetPlaftormWorkQueueMMCSSClass(
            [In] int dwPlatformWorkQueueId,
            [Out, MarshalAs(UnmanagedType.LPWStr)] string pwszClass,
            [In, Out] ref int pcchClass);

        [PreserveSig]
        int GetPlatformWorkQueueMMCSSTaskId(
            [In] int dwPlatformWorkQueueId,
            out int pdwTaskId
            );
    }

    [ComImport, System.Security.SuppressUnmanagedCodeSecurity,
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown),
    Guid("F5042EA4-7A96-4A75-AA7B-2BE1EF7F88D5")]
    public interface IMFByteStreamCacheControl
    {
        [PreserveSig]
        int StopBackgroundTransfer();
    }

    [ComImport, System.Security.SuppressUnmanagedCodeSecurity,
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown),
    Guid("508E71D3-EC66-4FC3-8775-B4B9ED6BA847")]
    public interface IMFFieldOfUseMFTUnlock
    {
        [PreserveSig]
        int Unlock(
            [In, MarshalAs(UnmanagedType.IUnknown)] object pUnkMFT
        );
    }

    [ComImport, System.Security.SuppressUnmanagedCodeSecurity,
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown),
    Guid("F3706F0D-8EA2-4886-8000-7155E9EC2EAE")]
    public interface IMFQualityAdvise2 : IMFQualityAdvise
    {
        #region IMFQualityAdvise methods

        [PreserveSig]
        new int SetDropMode(
            [In] MFQualityDropMode eDropMode
            );

        [PreserveSig]
        new int SetQualityLevel(
            [In] MFQualityLevel eQualityLevel
            );

        [PreserveSig]
        new int GetDropMode(
            out MFQualityDropMode peDropMode
            );

        [PreserveSig]
        new int GetQualityLevel(
            out MFQualityLevel peQualityLevel
            );

        [PreserveSig]
        new int DropTime(
            [In] long hnsAmountToDrop
            );

        #endregion

        [PreserveSig]
        int NotifyQualityEvent(
            IMFMediaEvent pEvent,
            out MFQualityAdviseFlags pdwFlags
        );
    }

    [ComImport, System.Security.SuppressUnmanagedCodeSecurity,
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown),
    Guid("DFCD8E4D-30B5-4567-ACAA-8EB5B7853DC9")]
    public interface IMFQualityAdviseLimits
    {
        [PreserveSig]
        int GetMaximumDropMode(
            out MFQualityDropMode peDropMode
        );

        [PreserveSig]
        int GetMinimumQualityLevel(
            out MFQualityLevel peQualityLevel
        );
    }

    [ComImport, System.Security.SuppressUnmanagedCodeSecurity,
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown),
    Guid("CA86AA50-C46E-429E-AB27-16D6AC6844CB")]
    public interface IMFSampleGrabberSinkCallback2 : IMFSampleGrabberSinkCallback
    {
        #region IMFClockStateSink methods

        [PreserveSig]
        new int OnClockStart(
            [In] long hnsSystemTime,
            [In] long llClockStartOffset
            );

        [PreserveSig]
        new int OnClockStop(
            [In] long hnsSystemTime
            );

        [PreserveSig]
        new int OnClockPause(
            [In] long hnsSystemTime
            );

        [PreserveSig]
        new int OnClockRestart(
            [In] long hnsSystemTime
            );

        [PreserveSig]
        new int OnClockSetRate(
            [In] long hnsSystemTime,
            [In] float flRate
            );

        #endregion

        #region IMFSampleGrabberSinkCallback methods

        [PreserveSig]
        new int OnSetPresentationClock(
            [In, MarshalAs(UnmanagedType.Interface)] IMFPresentationClock pPresentationClock
            );

        [PreserveSig]
        new int OnProcessSample(
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid guidMajorMediaType,
            [In] int dwSampleFlags, // must be zero
            [In] long llSampleTime,
            [In] long llSampleDuration,
            [In] ref byte pSampleBuffer,
            [In] int dwSampleSize
            );

        [PreserveSig]
        new int OnShutdown();

        #endregion

        [PreserveSig]
        int OnProcessSampleEx(
            [MarshalAs(UnmanagedType.LPStruct)] Guid guidMajorMediaType,
            [In] int dwSampleFlags, // No flags are defined
            [In] long llSampleTime,
            [In] long llSampleDuration,
            [In] IntPtr pSampleBuffer,
            [In] int dwSampleSize,
            IMFAttributes pAttributes
        );
    }

    [ComImport, System.Security.SuppressUnmanagedCodeSecurity,
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown),
    Guid("61F7D887-1230-4A8B-AEBA-8AD434D1A64D")]
    public interface IMFSSLCertificateManager
    {
        [PreserveSig]
        int GetClientCertificate(
            [In, MarshalAs(UnmanagedType.LPWStr)] string pszUrl,
            [Out] IntPtr ppbData,
            out int pcbData
        );

        [PreserveSig]
        int BeginGetClientCertificate(
            [In, MarshalAs(UnmanagedType.LPWStr)] string pszUrl,
            IMFAsyncCallback pCallback,
            [In, MarshalAs(UnmanagedType.IUnknown)] object pState
        );

        [PreserveSig]
        int EndGetClientCertificate(
            IMFAsyncResult pResult,
            [Out] IntPtr ppbData,
            out int pcbData
        );

        [PreserveSig]
        int GetCertificatePolicy(
            [In, MarshalAs(UnmanagedType.LPWStr)] string pszUrl,
            [MarshalAs(UnmanagedType.Bool)] out bool pfOverrideAutomaticCheck,
            [MarshalAs(UnmanagedType.Bool)] out bool pfClientCertificateAvailable
        );

        [PreserveSig]
        int OnServerCertificate(
            [In, MarshalAs(UnmanagedType.LPWStr)] string pszUrl,
            [In] IntPtr pbData,
            [In] int cbData,
            [MarshalAs(UnmanagedType.Bool)] out bool pfIsGood
        );
    }

    [ComImport, System.Security.SuppressUnmanagedCodeSecurity,
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown),
    Guid("9DB7AA41-3CC5-40D4-8509-555804AD34CC")]
    public interface IMFStreamingSinkConfig
    {
        [PreserveSig]
        int StartStreaming(
            [MarshalAs(UnmanagedType.Bool)] bool fSeekOffsetIsByteOffset,
            [In] long qwSeekOffset
        );
    }

    [ComImport, System.Security.SuppressUnmanagedCodeSecurity,
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown),
    Guid("AB9D8661-F7E8-4EF4-9861-89F334F94E74")]
    public interface IMFTimecodeTranslate
    {
        [PreserveSig]
        int BeginConvertTimecodeToHNS(
            [In, MarshalAs(UnmanagedType.LPStruct)] ConstPropVariant pPropVarTimecode,
            IMFAsyncCallback pCallback,
            [In, MarshalAs(UnmanagedType.IUnknown)] object punkState
        );

        [PreserveSig]
        int EndConvertTimecodeToHNS(
            IMFAsyncResult pResult,
            out long phnsTime
        );

        [PreserveSig]
        int BeginConvertHNSToTimecode(
            [In] long hnsTime,
            IMFAsyncCallback pCallback,
            [In, MarshalAs(UnmanagedType.IUnknown)] object punkState
        );

        [PreserveSig]
        int EndConvertHNSToTimecode(
            IMFAsyncResult pResult,
            [In, Out, MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(PVMarshaler))] PropVariant pPropVarTimecode
        );
    }

    [ComImport, System.Security.SuppressUnmanagedCodeSecurity,
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown),
    Guid("4ADFDBA3-7AB0-4953-A62B-461E7FF3DA1E")]
    public interface IMFTranscodeProfile
    {
        [PreserveSig]
        int SetAudioAttributes(
            IMFAttributes pAttrs
        );

        [PreserveSig]
        int GetAudioAttributes(
            out IMFAttributes ppAttrs
        );

        [PreserveSig]
        int SetVideoAttributes(
            IMFAttributes pAttrs
        );

        [PreserveSig]
        int GetVideoAttributes(
            out IMFAttributes ppAttrs
        );

        [PreserveSig]
        int SetContainerAttributes(
            IMFAttributes pAttrs
        );

        [PreserveSig]
        int GetContainerAttributes(
            out IMFAttributes ppAttrs
        );
    }

    [ComImport, System.Security.SuppressUnmanagedCodeSecurity,
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown),
    Guid("8CFFCD2E-5A03-4A3A-AFF7-EDCD107C620E")]
    public interface IMFTranscodeSinkInfoProvider
    {
        [PreserveSig]
        int SetOutputFile(
            [In, MarshalAs(UnmanagedType.LPWStr)] string pwszFileName
        );

        [PreserveSig]
        int SetOutputByteStream(
            IMFActivate pByteStreamActivate
        );

        [PreserveSig]
        int SetProfile(
            IMFTranscodeProfile pProfile
        );

        [PreserveSig]
        int GetSinkInfo(
            out MFTranscodeSinkInfo pSinkInfo
        );
    }

    [ComImport, System.Security.SuppressUnmanagedCodeSecurity,
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown),
    Guid("992388B4-3372-4F67-8B6F-C84C071F4751")]
    public interface IMFVideoSampleAllocatorCallback
    {
        [PreserveSig]
        int SetCallback(
            IMFVideoSampleAllocatorNotify pNotify
        );

        [PreserveSig]
        int GetFreeSampleCount(
            out int plSamples
        );
    }

    [ComImport, System.Security.SuppressUnmanagedCodeSecurity,
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown),
    Guid("A792CDBE-C374-4E89-8335-278E7B9956A4")]
    public interface IMFVideoSampleAllocatorNotify
    {
        [PreserveSig]
        int NotifyRelease();
    }

    #endif

    [ComImport, System.Security.SuppressUnmanagedCodeSecurity,
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown),
    Guid("279A808D-AEC7-40C8-9C6B-A6B492C78A66")]
    public interface IMFMediaSource : IMFMediaEventGenerator
    {
        #region IMFMediaEventGenerator methods

        [PreserveSig]
        new int GetEvent(
            [In] MFEventFlag dwFlags,
            [MarshalAs(UnmanagedType.Interface)] out IMFMediaEvent ppEvent
            );

        [PreserveSig]
        new int BeginGetEvent(
            [In, MarshalAs(UnmanagedType.Interface)] IMFAsyncCallback pCallback,
            [In, MarshalAs(UnmanagedType.IUnknown)] object o
            );

        [PreserveSig]
        new int EndGetEvent(
            IMFAsyncResult pResult,
            out IMFMediaEvent ppEvent
            );

        [PreserveSig]
        new int QueueEvent(
            [In] MediaEventType met,
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid guidExtendedType,
            [In] int hrStatus,
            [In, MarshalAs(UnmanagedType.LPStruct)] ConstPropVariant pvValue
            );

        #endregion

        [PreserveSig]
        int GetCharacteristics(
            out MFMediaSourceCharacteristics pdwCharacteristics
            );

        [PreserveSig]
        int CreatePresentationDescriptor(
            out IMFPresentationDescriptor ppPresentationDescriptor
            );

        [PreserveSig]
        int Start(
            [In, MarshalAs(UnmanagedType.Interface)] IMFPresentationDescriptor pPresentationDescriptor,
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid pguidTimeFormat,
            [In, MarshalAs(UnmanagedType.LPStruct)] ConstPropVariant pvarStartPosition
            );

        [PreserveSig]
        int Stop();

        [PreserveSig]
        int Pause();

        [PreserveSig]
        int Shutdown();
    }

    [ComImport, System.Security.SuppressUnmanagedCodeSecurity,
    Guid("03CB2711-24D7-4DB6-A17F-F3A7A479A536"),
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IMFPresentationDescriptor : IMFAttributes
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
        int GetStreamDescriptorCount(
            out int pdwDescriptorCount
            );

        [PreserveSig]
        int GetStreamDescriptorByIndex(
            [In] int dwIndex,
            [MarshalAs(UnmanagedType.Bool)] out bool pfSelected,
            [MarshalAs(UnmanagedType.Interface)] out IMFStreamDescriptor ppDescriptor
            );

        [PreserveSig]
        int SelectStream(
            [In] int dwDescriptorIndex
            );

        [PreserveSig]
        int DeselectStream(
            [In] int dwDescriptorIndex
            );

        [PreserveSig]
        int Clone(
            [MarshalAs(UnmanagedType.Interface)] out IMFPresentationDescriptor ppPresentationDescriptor
            );
    }

    [ComImport, System.Security.SuppressUnmanagedCodeSecurity,
    Guid("56C03D9C-9DBB-45F5-AB4B-D80F47C05938"),
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IMFStreamDescriptor : IMFAttributes
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
        int GetStreamIdentifier(
            out int pdwStreamIdentifier
            );

        [PreserveSig]
        int GetMediaTypeHandler(
            [MarshalAs(UnmanagedType.Interface)] out IMFMediaTypeHandler ppMediaTypeHandler
            );
    }

    [ComImport, System.Security.SuppressUnmanagedCodeSecurity,
    Guid("E93DCF6C-4B07-4E1E-8123-AA16ED6EADF5"),
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IMFMediaTypeHandler
    {
        [PreserveSig]
        int IsMediaTypeSupported(
            [In, MarshalAs(UnmanagedType.Interface)] IMFMediaType pMediaType,
            IntPtr ppMediaType  //[MarshalAs(UnmanagedType.Interface)] out IMFMediaType ppMediaType
            );

        [PreserveSig]
        int GetMediaTypeCount(
            out int pdwTypeCount
            );

        [PreserveSig]
        int GetMediaTypeByIndex(
            [In] int dwIndex,
            [MarshalAs(UnmanagedType.Interface)] out IMFMediaType ppType
            );

        [PreserveSig]
        int SetCurrentMediaType(
            [In, MarshalAs(UnmanagedType.Interface)] IMFMediaType pMediaType
            );

        [PreserveSig]
        int GetCurrentMediaType(
            [MarshalAs(UnmanagedType.Interface)] out IMFMediaType ppMediaType
            );

        [PreserveSig]
        int GetMajorType(
            out Guid pguidMajorType
            );
    }

    [ComImport, System.Security.SuppressUnmanagedCodeSecurity,
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown),
    Guid("83CF873A-F6DA-4BC8-823F-BACFD55DC433")]
    public interface IMFTopology : IMFAttributes
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
        int GetTopologyID(
            out long pID
            );

        [PreserveSig]
        int AddNode(
            [In, MarshalAs(UnmanagedType.Interface)] IMFTopologyNode pNode
            );

        [PreserveSig]
        int RemoveNode(
            [In, MarshalAs(UnmanagedType.Interface)] IMFTopologyNode pNode
            );

        [PreserveSig]
        int GetNodeCount(
            out short pwNodes
            );

        [PreserveSig]
        int GetNode(
            [In] short wIndex,
            [MarshalAs(UnmanagedType.Interface)] out IMFTopologyNode ppNode
            );

        [PreserveSig]
        int Clear();

        [PreserveSig]
        int CloneFrom(
            [In, MarshalAs(UnmanagedType.Interface)] IMFTopology pTopology
            );

        [PreserveSig]
        int GetNodeByID(
            [In] long qwTopoNodeID,
            [MarshalAs(UnmanagedType.Interface)] out IMFTopologyNode ppNode
            );

        [PreserveSig]
        int GetSourceNodeCollection(
            [MarshalAs(UnmanagedType.Interface)] out IMFCollection ppCollection
            );

        [PreserveSig]
        int GetOutputNodeCollection(
            [MarshalAs(UnmanagedType.Interface)] out IMFCollection ppCollection
            );
    }

    [ComImport, System.Security.SuppressUnmanagedCodeSecurity,
    Guid("83CF873A-F6DA-4BC8-823F-BACFD55DC430"),
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IMFTopologyNode : IMFAttributes
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
        int SetObject(
            [In, MarshalAs(UnmanagedType.IUnknown)] object pObject
            );

        [PreserveSig]
        int GetObject(
            [MarshalAs(UnmanagedType.IUnknown)] out object ppObject
            );

        [PreserveSig]
        int GetNodeType(
            out MFTopologyType pType
            );

        [PreserveSig]
        int GetTopoNodeID(
            out long pID
            );

        [PreserveSig]
        int SetTopoNodeID(
            [In] long ullTopoID
            );

        [PreserveSig]
        int GetInputCount(
            out int pcInputs
            );

        [PreserveSig]
        int GetOutputCount(
            out int pcOutputs
            );

        [PreserveSig]
        int ConnectOutput(
            [In] int dwOutputIndex,
            [In, MarshalAs(UnmanagedType.Interface)] IMFTopologyNode pDownstreamNode,
            [In] int dwInputIndexOnDownstreamNode
            );

        [PreserveSig]
        int DisconnectOutput(
            [In] int dwOutputIndex
            );

        [PreserveSig]
        int GetInput(
            [In] int dwInputIndex,
            [MarshalAs(UnmanagedType.Interface)] out IMFTopologyNode ppUpstreamNode,
            out int pdwOutputIndexOnUpstreamNode
            );

        [PreserveSig]
        int GetOutput(
            [In] int dwOutputIndex,
            [MarshalAs(UnmanagedType.Interface)] out IMFTopologyNode ppDownstreamNode,
            out int pdwInputIndexOnDownstreamNode
            );

        [PreserveSig]
        int SetOutputPrefType(
            [In] int dwOutputIndex,
            [In, MarshalAs(UnmanagedType.Interface)] IMFMediaType pType
            );

        [PreserveSig]
        int GetOutputPrefType(
            [In] int dwOutputIndex,
            out IMFMediaType ppType
            );

        [PreserveSig]
        int SetInputPrefType(
            [In] int dwInputIndex,
            [In, MarshalAs(UnmanagedType.Interface)] IMFMediaType pType
            );

        [PreserveSig]
        int GetInputPrefType(
            [In] int dwInputIndex,
            out IMFMediaType ppType
            );

        [PreserveSig]
        int CloneFrom(
            [In, MarshalAs(UnmanagedType.Interface)] IMFTopologyNode pNode
            );
    }

    [ComImport, System.Security.SuppressUnmanagedCodeSecurity,
    Guid("90377834-21D0-4DEE-8214-BA2E3E6C1127"),
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IMFMediaSession : IMFMediaEventGenerator
    {
        #region IMFMediaEventGenerator methods

        [PreserveSig]
        new int GetEvent(
            [In] MFEventFlag dwFlags,
            [MarshalAs(UnmanagedType.Interface)] out IMFMediaEvent ppEvent
            );

        [PreserveSig]
        new int BeginGetEvent(
            [In, MarshalAs(UnmanagedType.Interface)] IMFAsyncCallback pCallback,
            [In, MarshalAs(UnmanagedType.IUnknown)] object o);

        [PreserveSig]
        new int EndGetEvent(
            IMFAsyncResult pResult,
            out IMFMediaEvent ppEvent);


        [PreserveSig]
        new int QueueEvent(
            [In] MediaEventType met,
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid guidExtendedType,
            [In] int hrStatus,
            [In, MarshalAs(UnmanagedType.LPStruct)] ConstPropVariant pvValue
            );

        #endregion

        [PreserveSig]
        int SetTopology(
            [In] MFSessionSetTopologyFlags dwSetTopologyFlags,
            [In, MarshalAs(UnmanagedType.Interface)] IMFTopology pTopology
            );

        [PreserveSig]
        int ClearTopologies();

        [PreserveSig]
        int Start(
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid pguidTimeFormat,
            [In, MarshalAs(UnmanagedType.LPStruct)] ConstPropVariant pvarStartPosition
            );

        [PreserveSig]
        int Pause();

        [PreserveSig]
        int Stop();

        [PreserveSig]
        int Close();

        [PreserveSig]
        int Shutdown();

        [PreserveSig]
        int GetClock(
            [MarshalAs(UnmanagedType.Interface)] out IMFClock ppClock
            );

        [PreserveSig]
        int GetSessionCapabilities(
            out MFSessionCapabilities pdwCaps
            );

        [PreserveSig]
        int GetFullTopology(
            [In] MFSessionGetFullTopologyFlags dwGetFullTopologyFlags,
            [In] long TopoId,
            [MarshalAs(UnmanagedType.Interface)] out IMFTopology ppFullTopology
            );
    }

    [ComImport, System.Security.SuppressUnmanagedCodeSecurity,
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown),
    Guid("D182108F-4EC6-443F-AA42-A71106EC825F")]
    public interface IMFMediaStream : IMFMediaEventGenerator
    {
        #region IMFMediaEventGenerator methods

        [PreserveSig]
        new int GetEvent(
            [In] MFEventFlag dwFlags,
            [MarshalAs(UnmanagedType.Interface)] out IMFMediaEvent ppEvent
            );

        [PreserveSig]
        new int BeginGetEvent(
            [In, MarshalAs(UnmanagedType.Interface)] IMFAsyncCallback pCallback,
            [In, MarshalAs(UnmanagedType.IUnknown)] object o
            );

        [PreserveSig]
        new int EndGetEvent(
            IMFAsyncResult pResult,
            out IMFMediaEvent ppEvent);

        [PreserveSig]
        new int QueueEvent(
            [In] MediaEventType met,
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid guidExtendedType,
            [In] int hrStatus,
            [In, MarshalAs(UnmanagedType.LPStruct)] ConstPropVariant pvValue
            );

        #endregion

        [PreserveSig]
        int GetMediaSource(
            [MarshalAs(UnmanagedType.Interface)] out IMFMediaSource ppMediaSource
            );

        [PreserveSig]
        int GetStreamDescriptor(
            [MarshalAs(UnmanagedType.Interface)] out IMFStreamDescriptor ppStreamDescriptor
            );

        [PreserveSig]
        int RequestSample(
            [In, MarshalAs(UnmanagedType.IUnknown)] object pToken
            );
    }

    [ComImport, System.Security.SuppressUnmanagedCodeSecurity,
    Guid("FBE5A32D-A497-4B61-BB85-97B1A848A6E3"),
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IMFSourceResolver
    {
        [PreserveSig]
        int CreateObjectFromURL(
            [In, MarshalAs(UnmanagedType.LPWStr)] string pwszURL,
            [In] MFResolution dwFlags,
            IPropertyStore pProps,
            out MFObjectType pObjectType,
            [MarshalAs(UnmanagedType.IUnknown)] out object ppObject
            );

        [PreserveSig]
        int CreateObjectFromByteStream(
            [In, MarshalAs(UnmanagedType.Interface)] IMFByteStream pByteStream,
            [In, MarshalAs(UnmanagedType.LPWStr)] string pwszURL,
            [In] MFResolution dwFlags,
            [In, MarshalAs(UnmanagedType.Interface)] IPropertyStore pProps,
            out MFObjectType pObjectType,
            [MarshalAs(UnmanagedType.IUnknown)] out object ppObject
            );

        [PreserveSig]
        int BeginCreateObjectFromURL(
            [In, MarshalAs(UnmanagedType.LPWStr)] string pwszURL,
            MFResolution dwFlags,
            IPropertyStore pProps,
            [MarshalAs(UnmanagedType.IUnknown)] out object ppIUnknownCancelCookie,
            IMFAsyncCallback pCallback,
            [In, MarshalAs(UnmanagedType.IUnknown)] object punkState
            );

        [PreserveSig]
        int EndCreateObjectFromURL(
            IMFAsyncResult pResult,
            out MFObjectType pObjectType,
            [MarshalAs(UnmanagedType.Interface)] out object ppObject
            );

        [PreserveSig]
        int BeginCreateObjectFromByteStream(
            [In, MarshalAs(UnmanagedType.Interface)] IMFByteStream pByteStream,
            [In, MarshalAs(UnmanagedType.LPWStr)] string pwszURL,
            [In] MFResolution dwFlags,
            IPropertyStore pProps,
            [MarshalAs(UnmanagedType.IUnknown)] out object ppIUnknownCancelCookie,
            IMFAsyncCallback pCallback,
            [MarshalAs(UnmanagedType.IUnknown)] object punkState
           );

        [PreserveSig]
        int EndCreateObjectFromByteStream(
            IMFAsyncResult pResult,
            out MFObjectType pObjectType,
            [MarshalAs(UnmanagedType.IUnknown)] out object ppObject
            );

        [PreserveSig]
        int CancelObjectCreation(
            [In, MarshalAs(UnmanagedType.IUnknown)] object pIUnknownCancelCookie
            );
    }

    [ComImport, System.Security.SuppressUnmanagedCodeSecurity,
    Guid("7FEE9E9A-4A89-47A6-899C-B6A53A70FB67"),
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IMFActivate : IMFAttributes
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
        int ActivateObject(
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid riid,
            [MarshalAs(UnmanagedType.Interface)] out object ppv
            );

        [PreserveSig]
        int ShutdownObject();

        [PreserveSig]
        int DetachObject();
    }

    [ComImport, System.Security.SuppressUnmanagedCodeSecurity,
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown),
    Guid("FA993888-4383-415A-A930-DD472A8CF6F7")]
    public interface IMFGetService
    {
        [PreserveSig]
        int GetService(
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid guidService,
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid riid,
            [MarshalAs(UnmanagedType.Interface)] out object ppvObject
            );
    }

    [ComImport, System.Security.SuppressUnmanagedCodeSecurity,
    Guid("BB420AA4-765B-4A1F-91FE-D6A8A143924C"),
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IMFByteStreamHandler
    {
        [PreserveSig]
        int BeginCreateObject(
            [In, MarshalAs(UnmanagedType.Interface)] IMFByteStream pByteStream,
            [In, MarshalAs(UnmanagedType.LPWStr)] string pwszURL,
            [In] MFResolution dwFlags,
            [In, MarshalAs(UnmanagedType.Interface)] IPropertyStore pProps,
            [MarshalAs(UnmanagedType.IUnknown)] out object ppIUnknownCancelCookie,
            [In, MarshalAs(UnmanagedType.Interface)] IMFAsyncCallback pCallback,
            [In, MarshalAs(UnmanagedType.IUnknown)] object pUnkState
            );

        [PreserveSig]
        int EndCreateObject(
            [In, MarshalAs(UnmanagedType.Interface)] IMFAsyncResult pResult,
            out MFObjectType pObjectType,
            [MarshalAs(UnmanagedType.IUnknown)] out object ppObject
            );

        [PreserveSig]
        int CancelObjectCreation(
            [In, MarshalAs(UnmanagedType.IUnknown)] object pIUnknownCancelCookie
            );

        [PreserveSig]
        int GetMaxNumberOfBytesRequiredForResolution(
            out long pqwBytes
            );
    }

    [ComImport, System.Security.SuppressUnmanagedCodeSecurity,
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown),
    Guid("F6696E82-74F7-4F3D-A178-8A5E09C3659F")]
    public interface IMFClockStateSink
    {
        [PreserveSig]
        int OnClockStart(
            [In] long hnsSystemTime,
            [In] long llClockStartOffset
            );

        [PreserveSig]
        int OnClockStop(
            [In] long hnsSystemTime
            );

        [PreserveSig]
        int OnClockPause(
            [In] long hnsSystemTime
            );

        [PreserveSig]
        int OnClockRestart(
            [In] long hnsSystemTime
            );

        [PreserveSig]
        int OnClockSetRate(
            [In] long hnsSystemTime,
            [In] float flRate
            );
    }

    [ComImport, System.Security.SuppressUnmanagedCodeSecurity,
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown),
    Guid("2EB1E945-18B8-4139-9B1A-D5D584818530")]
    public interface IMFClock
    {
        [PreserveSig]
        int GetClockCharacteristics(
            out MFClockCharacteristicsFlags pdwCharacteristics
            );

        [PreserveSig]
        int GetCorrelatedTime(
            [In] int dwReserved,
            out long pllClockTime,
            out long phnsSystemTime
            );

        [PreserveSig]
        int GetContinuityKey(
            out int pdwContinuityKey
            );

        [PreserveSig]
        int GetState(
            [In] int dwReserved,
            out MFClockState peClockState
            );

        [PreserveSig]
        int GetProperties(
            out MFClockProperties pClockProperties
            );
    }

    [ComImport, System.Security.SuppressUnmanagedCodeSecurity,
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown),
    Guid("868CE85C-8EA9-4F55-AB82-B009A910A805")]
    public interface IMFPresentationClock : IMFClock
    {
        #region IMFClock methods

        [PreserveSig]
        new int GetClockCharacteristics(
            out MFClockCharacteristicsFlags pdwCharacteristics
            );

        [PreserveSig]
        new int GetCorrelatedTime(
            [In] int dwReserved,
            out long pllClockTime,
            out long phnsSystemTime
            );

        [PreserveSig]
        new int GetContinuityKey(
            out int pdwContinuityKey
            );

        [PreserveSig]
        new int GetState(
            [In] int dwReserved,
            out MFClockState peClockState
            );

        [PreserveSig]
        new int GetProperties(
            out MFClockProperties pClockProperties
            );

        #endregion

        [PreserveSig]
        int SetTimeSource(
            [In, MarshalAs(UnmanagedType.Interface)] IMFPresentationTimeSource pTimeSource
            );

        [PreserveSig]
        int GetTimeSource(
            [MarshalAs(UnmanagedType.Interface)] out IMFPresentationTimeSource ppTimeSource
            );

        [PreserveSig]
        int GetTime(
            out long phnsClockTime
            );

        [PreserveSig]
        int AddClockStateSink(
            [In, MarshalAs(UnmanagedType.Interface)] IMFClockStateSink pStateSink
            );

        [PreserveSig]
        int RemoveClockStateSink(
            [In, MarshalAs(UnmanagedType.Interface)] IMFClockStateSink pStateSink
            );

        [PreserveSig]
        int Start(
            [In] long llClockStartOffset
            );

        [PreserveSig]
        int Stop();

        [PreserveSig]
        int Pause();
    }

    [ComImport, System.Security.SuppressUnmanagedCodeSecurity,
    Guid("7FF12CCE-F76F-41C2-863B-1666C8E5E139"),
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IMFPresentationTimeSource : IMFClock
    {
        #region IMFClock methods

        [PreserveSig]
        new int GetClockCharacteristics(
            out MFClockCharacteristicsFlags pdwCharacteristics
            );

        [PreserveSig]
        new int GetCorrelatedTime(
            [In] int dwReserved,
            out long pllClockTime,
            out long phnsSystemTime
            );

        [PreserveSig]
        new int GetContinuityKey(
            out int pdwContinuityKey
            );

        [PreserveSig]
        new int GetState(
            [In] int dwReserved,
            out MFClockState peClockState
            );

        [PreserveSig]
        new int GetProperties(
            out MFClockProperties pClockProperties
            );

        #endregion

        [PreserveSig]
        int GetUnderlyingClock(
            [MarshalAs(UnmanagedType.Interface)] out IMFClock ppClock
            );
    }

    [ComImport, System.Security.SuppressUnmanagedCodeSecurity,
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown),
    Guid("0E1D6009-C9F3-442D-8C51-A42D2D49452F")]
    public interface IMFMediaSourceTopologyProvider
    {
        [PreserveSig]
        int GetMediaSourceTopology(
            [In, MarshalAs(UnmanagedType.Interface)] IMFPresentationDescriptor pPresentationDescriptor,
            [MarshalAs(UnmanagedType.Interface)] out IMFTopology ppTopology
            );
    }

    [ComImport, System.Security.SuppressUnmanagedCodeSecurity,
    Guid("197CD219-19CB-4DE1-A64C-ACF2EDCBE59E"),
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IMFSequencerSource
    {
        [PreserveSig]
        int AppendTopology(
            [In, MarshalAs(UnmanagedType.Interface)] IMFTopology pTopology,
            [In] MFSequencerTopologyFlags dwFlags,
            out int pdwId
            );

        [PreserveSig]
        int DeleteTopology(
            [In] int dwId
            );

        [PreserveSig]
        int GetPresentationContext(
            [In, MarshalAs(UnmanagedType.Interface)] IMFPresentationDescriptor pPD,
            out int pID,
            [MarshalAs(UnmanagedType.Interface)] out IMFTopology ppTopology
            );

        [PreserveSig]
        int UpdateTopology(
            [In] int dwId,
            [In, MarshalAs(UnmanagedType.Interface)] IMFTopology pTopology
            );

        [PreserveSig]
        int UpdateTopologyFlags(
            [In] int dwId,
            [In] MFSequencerTopologyFlags dwFlags
            );
    }

    [ComImport, System.Security.SuppressUnmanagedCodeSecurity,
    Guid("ACF92459-6A61-42BD-B57C-B43E51203CB0"),
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IMFContentProtectionManager
    {
        [PreserveSig]
        int BeginEnableContent(
            IMFActivate pEnablerActivate,
            IMFTopology pTopo,
            IMFAsyncCallback pCallback,
            [MarshalAs(UnmanagedType.Interface)] object punkState
            );

        [PreserveSig]
        int EndEnableContent(
            IMFAsyncResult pResult
            );
    }

    [ComImport, System.Security.SuppressUnmanagedCodeSecurity,
    Guid("D3C4EF59-49CE-4381-9071-D5BCD044C770"),
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IMFContentEnabler
    {
        [PreserveSig]
        int GetEnableType(out Guid pType);

        [PreserveSig]
        int GetEnableURL(
            [MarshalAs(UnmanagedType.LPWStr)] out string ppwszURL,
            out int pcchURL,
            out MFURLTrustStatus pTrustStatus
            );

        [PreserveSig]
        int GetEnableData(
            [Out] out IntPtr ppbData,
            out int pcbData);

        [PreserveSig]
        int IsAutomaticSupported(
            [MarshalAs(UnmanagedType.Bool)] out bool pfAutomatic
            );

        [PreserveSig]
        int AutomaticEnable();

        [PreserveSig]
        int MonitorEnable();

        [PreserveSig]
        int Cancel();
    }

    [ComImport, System.Security.SuppressUnmanagedCodeSecurity,
    Guid("6EF2A660-47C0-4666-B13D-CBB717F2FA2C"),
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IMFMediaSink
    {
        [PreserveSig]
        int GetCharacteristics(
            out MFMediaSinkCharacteristics pdwCharacteristics
            );

        [PreserveSig]
        int AddStreamSink(
            [In] int dwStreamSinkIdentifier,
            [In, MarshalAs(UnmanagedType.Interface)] IMFMediaType pMediaType,
            [MarshalAs(UnmanagedType.Interface)] out IMFStreamSink ppStreamSink
            );

        [PreserveSig]
        int RemoveStreamSink(
            [In] int dwStreamSinkIdentifier
            );

        [PreserveSig]
        int GetStreamSinkCount(
            out int pcStreamSinkCount
            );

        [PreserveSig]
        int GetStreamSinkByIndex(
            [In] int dwIndex,
            [MarshalAs(UnmanagedType.Interface)] out IMFStreamSink ppStreamSink
            );

        [PreserveSig]
        int GetStreamSinkById(
            [In] int dwStreamSinkIdentifier,
            [MarshalAs(UnmanagedType.Interface)] out IMFStreamSink ppStreamSink
            );

        [PreserveSig]
        int SetPresentationClock(
            [In, MarshalAs(UnmanagedType.Interface)] IMFPresentationClock pPresentationClock
            );

        [PreserveSig]
        int GetPresentationClock(
            [MarshalAs(UnmanagedType.Interface)] out IMFPresentationClock ppPresentationClock
            );

        [PreserveSig]
        int Shutdown();
    }

    [ComImport, System.Security.SuppressUnmanagedCodeSecurity,
    Guid("0A97B3CF-8E7C-4A3D-8F8C-0C843DC247FB"),
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IMFStreamSink : IMFMediaEventGenerator
    {
        #region IMFMediaEventGenerator methods

        [PreserveSig]
        new int GetEvent(
            [In] MFEventFlag dwFlags,
            [MarshalAs(UnmanagedType.Interface)] out IMFMediaEvent ppEvent
            );

        [PreserveSig]
        new int BeginGetEvent(
            [In, MarshalAs(UnmanagedType.Interface)] IMFAsyncCallback pCallback,
            [In, MarshalAs(UnmanagedType.IUnknown)] object o);

        [PreserveSig]
        new int EndGetEvent(
            IMFAsyncResult pResult,
            out IMFMediaEvent ppEvent);

        [PreserveSig]
        new int QueueEvent(
            [In] MediaEventType met,
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid guidExtendedType,
            [In] int hrStatus,
            [In, MarshalAs(UnmanagedType.LPStruct)] ConstPropVariant pvValue
            );

        #endregion

        [PreserveSig]
        int GetMediaSink(
            [MarshalAs(UnmanagedType.Interface)] out IMFMediaSink ppMediaSink
            );

        [PreserveSig]
        int GetIdentifier(
            out int pdwIdentifier
            );

        [PreserveSig]
        int GetMediaTypeHandler(
            [MarshalAs(UnmanagedType.Interface)] out IMFMediaTypeHandler ppHandler
            );

        [PreserveSig]
        int ProcessSample(
            [In, MarshalAs(UnmanagedType.Interface)] IMFSample pSample
            );

        [PreserveSig]
        int PlaceMarker(
            [In] MFStreamSinkMarkerType eMarkerType,
            [In, MarshalAs(UnmanagedType.LPStruct)] ConstPropVariant pvarMarkerValue,
            [In, MarshalAs(UnmanagedType.LPStruct)] ConstPropVariant pvarContextValue
            );

        [PreserveSig]
        int Flush();
    }

    [ComImport, System.Security.SuppressUnmanagedCodeSecurity,
    Guid("EAECB74A-9A50-42CE-9541-6A7F57AA4AD7"),
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IMFFinalizableMediaSink : IMFMediaSink
    {
        #region IMFMediaSink methods

        [PreserveSig]
        new int GetCharacteristics(
            out MFMediaSinkCharacteristics pdwCharacteristics);

        [PreserveSig]
        new int AddStreamSink(
            [In] int dwStreamSinkIdentifier,
            [In, MarshalAs(UnmanagedType.Interface)] IMFMediaType pMediaType,
            [MarshalAs(UnmanagedType.Interface)] out IMFStreamSink ppStreamSink
            );

        [PreserveSig]
        new int RemoveStreamSink(
            [In] int dwStreamSinkIdentifier
            );

        [PreserveSig]
        new int GetStreamSinkCount(
            out int pcStreamSinkCount
            );

        [PreserveSig]
        new int GetStreamSinkByIndex(
            [In] int dwIndex,
            [MarshalAs(UnmanagedType.Interface)] out IMFStreamSink ppStreamSink
            );

        [PreserveSig]
        new int GetStreamSinkById(
            [In] int dwStreamSinkIdentifier,
            [MarshalAs(UnmanagedType.Interface)] out IMFStreamSink ppStreamSink
            );

        [PreserveSig]
        new int SetPresentationClock(
            [In, MarshalAs(UnmanagedType.Interface)] IMFPresentationClock pPresentationClock
            );

        [PreserveSig]
        new int GetPresentationClock(
            [MarshalAs(UnmanagedType.Interface)] out IMFPresentationClock ppPresentationClock
            );

        [PreserveSig]
        new int Shutdown();

        #endregion

        [PreserveSig]
        int BeginFinalize(
            [In, MarshalAs(UnmanagedType.Interface)] IMFAsyncCallback pCallback,
            [In, MarshalAs(UnmanagedType.IUnknown)] object pUnkState
            );

        [PreserveSig]
        int EndFinalize(
            [In, MarshalAs(UnmanagedType.Interface)] IMFAsyncResult pResult
            );
    }

    [ComImport, System.Security.SuppressUnmanagedCodeSecurity,
    Guid("0A9CCDBC-D797-4563-9667-94EC5D79292D"),
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IMFRateSupport
    {
        [PreserveSig]
        int GetSlowestRate(
            [In] MFRateDirection eDirection,
            [In, MarshalAs(UnmanagedType.Bool)] bool fThin,
            out float pflRate
            );

        [PreserveSig]
        int GetFastestRate(
            [In] MFRateDirection eDirection,
            [In, MarshalAs(UnmanagedType.Bool)] bool fThin,
            out float pflRate
            );

        [PreserveSig]
        int IsRateSupported(
            [In, MarshalAs(UnmanagedType.Bool)] bool fThin,
            [In] float flRate,
            [In, Out] MfFloat pflNearestSupportedRate
            );
    }

    [ComImport, System.Security.SuppressUnmanagedCodeSecurity,
    Guid("059054B3-027C-494C-A27D-9113291CF87F"),
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IMFSourceOpenMonitor
    {
        [PreserveSig]
        int OnSourceEvent(
            [In, MarshalAs(UnmanagedType.Interface)] IMFMediaEvent pEvent
            );
    }

    [ComImport, System.Security.SuppressUnmanagedCodeSecurity,
    Guid("97EC2EA4-0E42-4937-97AC-9D6D328824E1"),
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IMFShutdown
    {
        [PreserveSig]
        int Shutdown();

        [PreserveSig]
        int GetShutdownStatus(
            out MFShutdownStatus pStatus
            );
    }

    [ComImport, System.Security.SuppressUnmanagedCodeSecurity,
    Guid("F88CFB8C-EF16-4991-B450-CB8C69E51704"),
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IMFMetadata
    {
        [PreserveSig]
        int SetLanguage(
            [In, MarshalAs(UnmanagedType.LPWStr)] string pwszRFC1766
            );

        [PreserveSig]
        int GetLanguage(
            [MarshalAs(UnmanagedType.LPWStr)] out string ppwszRFC1766
            );

        [PreserveSig]
        int GetAllLanguages(
            [In, Out, MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(PVMarshaler))] PropVariant ppvLanguages
            );

        [PreserveSig]
        int SetProperty(
            [In, MarshalAs(UnmanagedType.LPWStr)] string pwszName,
            [In, MarshalAs(UnmanagedType.LPStruct)] ConstPropVariant ppvValue
            );

        [PreserveSig]
        int GetProperty(
            [In, MarshalAs(UnmanagedType.LPWStr)] string pwszName,
            [In, Out, MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(PVMarshaler))] PropVariant ppvValue
            );

        [PreserveSig]
        int DeleteProperty(
            [In, MarshalAs(UnmanagedType.LPWStr)] string pwszName
            );

        [PreserveSig]
        int GetAllPropertyNames(
            [In, Out, MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(PVMarshaler))] PropVariant ppvNames
            );
    }

    [ComImport, System.Security.SuppressUnmanagedCodeSecurity,
    Guid("88DDCD21-03C3-4275-91ED-55EE3929328F"),
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IMFRateControl
    {
        [PreserveSig]
        int SetRate(
            [In, MarshalAs(UnmanagedType.Bool)] bool fThin,
            [In] float flRate
            );

        [PreserveSig]
        int GetRate(
            [In, Out, MarshalAs(UnmanagedType.Bool)] ref bool pfThin,
            out float pflRate
            );
    }

    [ComImport, System.Security.SuppressUnmanagedCodeSecurity,
    Guid("E56E4CBD-8F70-49D8-A0F8-EDB3D6AB9BF2"),
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IMFTimer
    {
        [PreserveSig]
        int SetTimer(
            [In] MFTimeFlags dwFlags,
            [In] long llClockTime,
            [In, MarshalAs(UnmanagedType.Interface)] IMFAsyncCallback pCallback,
            [In, MarshalAs(UnmanagedType.IUnknown)] object pUnkState,
            [MarshalAs(UnmanagedType.IUnknown)] out object ppunkKey
            );

        [PreserveSig]
        int CancelTimer(
            [In, MarshalAs(UnmanagedType.IUnknown)] object punkKey
            );
    }

    [ComImport, System.Security.SuppressUnmanagedCodeSecurity,
    Guid("56181D2D-E221-4ADB-B1C8-3CEE6A53F76F"),
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IMFMetadataProvider
    {
        [PreserveSig]
        int GetMFMetadata(
            [In, MarshalAs(UnmanagedType.Interface)]
            IMFPresentationDescriptor pPresentationDescriptor,
            [In] int dwStreamIdentifier,
            [In] int dwFlags, // must be zero
            [MarshalAs(UnmanagedType.Interface)] out IMFMetadata ppMFMetadata
            );
    }

    #endregion

}
