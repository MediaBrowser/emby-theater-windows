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

namespace MediaFoundation
{
#if ALLOW_UNTESTED_INTERFACES

    [UnmanagedName("CLSID_MPEG2DLNASink"),
    ComImport,
    Guid("fa5fe7c5-6a1d-4b11-b41f-f959d6c76500")]
    public class MPEG2DLNASink
    {
    }

    [StructLayout(LayoutKind.Sequential), UnmanagedName("MFMPEG2DLNASINKSTATS")]
    public struct MFMPEG2DLNASINKSTATS
    {
        long cBytesWritten;
        bool fPAL;
        int fccVideo;
        int dwVideoWidth;
        int dwVideoHeight;
        long cVideoFramesReceived;
        long cVideoFramesEncoded;
        long cVideoFramesSkipped;
        long cBlackVideoFramesEncoded;
        long cVideoFramesDuplicated;
        int cAudioSamplesPerSec;
        int cAudioChannels;
        long cAudioBytesReceived;
        long cAudioFramesEncoded;
    }

    [ComImport, System.Security.SuppressUnmanagedCodeSecurity,
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown),
    Guid("0C012799-1B61-4C10-BDA9-04445BE5F561")]
    public interface IMFDLNASinkInit
    {
        [PreserveSig]
        int Initialize(
            IMFByteStream pByteStream,
            [MarshalAs(UnmanagedType.Bool)] bool fPal
        );
    }

#endif

}
