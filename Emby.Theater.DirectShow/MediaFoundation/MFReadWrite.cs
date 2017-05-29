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
using System.Security;

using MediaFoundation.Misc;
using System.Drawing;

using MediaFoundation.EVR;
using MediaFoundation.Transform;

namespace MediaFoundation.ReadWrite
{
    #region COM Class Objects

    [UnmanagedName("CLSID_MFReadWriteClassFactory"), 
    ComImport, 
    Guid("48e2ed0f-98c2-4a37-bed5-166312ddd83f")]
    public class MFReadWriteClassFactory
    {
    }

    #endregion

    #region Declarations

#if ALLOW_UNTESTED_INTERFACES

    [UnmanagedName("Unnamed enum")]
    public enum MF_SOURCE_READER_INDEX
    {
        CURRENT_TYPE_INDEX = unchecked((int)0xFFFFFFFF)
    }

    [UnmanagedName("Unnamed enum")]
    public enum MF_SINK_WRITER
    {
        InvalidStreamIndex = unchecked((int)0xFFFFFFFF),
        AllStreams = unchecked((int)0xFFFFFFFE),
        MediaSink = unchecked((int)0xFFFFFFFF)
    }

    [Flags, UnmanagedName("MF_SOURCE_READER_FLAG")]
    public enum MF_SOURCE_READER_FLAG
    {
        None = 0,
        Error = 0x00000001,
        EndOfStream = 0x00000002,
        NewStream = 0x00000004,
        NativeMediaTypeChanged = 0x00000010,
        CurrentMediaTypeChanged = 0x00000020,
        AllEffectsRemoved       = 0x00000200,
        StreamTick = 0x00000100
    }

    [UnmanagedName("Unnamed enum")]
    public enum MF_SOURCE_READER
    {
        InvalidStreamIndex = unchecked((int)0xFFFFFFFF),
        AllStreams = unchecked((int)0xFFFFFFFE),
        AnyStream = unchecked((int)0xFFFFFFFE),
        FirstAudioStream = unchecked((int)0xFFFFFFFD),
        FirstVideoStream = unchecked((int)0xFFFFFFFC),
        MediaSource = unchecked((int)0xFFFFFFFF),
    }

    [UnmanagedName("MF_SOURCE_READER_CONTROL_FLAG")]
    public enum MF_SOURCE_READER_CONTROL_FLAG
    {
        None = 0,
        Drain = 0x00000001
    }

    [StructLayout(LayoutKind.Sequential), UnmanagedName("MF_SINK_WRITER_STATISTICS")]
    public class MF_SINK_WRITER_STATISTICS
    {
        int cb;

        long llLastTimestampReceived;
        long llLastTimestampEncoded;
        long llLastTimestampProcessed;
        long llLastStreamTickReceived;
        long llLastSinkSampleRequest;

        long qwNumSamplesReceived;
        long qwNumSamplesEncoded;
        long qwNumSamplesProcessed;
        long qwNumStreamTicksReceived;

        int dwByteCountQueued;
        long qwByteCountProcessed;

        int dwNumOutstandingSinkSampleRequests;

        int dwAverageSampleRateReceived;
        int dwAverageSampleRateEncoded;
        int dwAverageSampleRateProcessed;
    }

#endif

    #endregion

    #region Interfaces

#if ALLOW_UNTESTED_INTERFACES

    [ComImport, System.Security.SuppressUnmanagedCodeSecurity,
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown),
    Guid("E7FE2E12-661C-40DA-92F9-4F002AB67627")]
    public interface IMFReadWriteClassFactory
    {
        [PreserveSig]
        int CreateInstanceFromURL(
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid clsid,
            [In, MarshalAs(UnmanagedType.LPWStr)] string pwszURL,
            IMFAttributes pAttributes,
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid riid,
            [MarshalAs(UnmanagedType.IUnknown)] out object ppvObject
        );

        [PreserveSig]
        int CreateInstanceFromObject(
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid clsid,
            [MarshalAs(UnmanagedType.IUnknown)] object punkObject,
            IMFAttributes pAttributes,
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid riid,
            [MarshalAs(UnmanagedType.IUnknown)] out object ppvObject
        );
    }

    [ComImport, System.Security.SuppressUnmanagedCodeSecurity,
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown),
    Guid("70ae66f2-c809-4e4f-8915-bdcb406b7993")]
    public interface IMFSourceReader
    {
        [PreserveSig]
        int GetStreamSelection(
            int dwStreamIndex,
            [MarshalAs(UnmanagedType.Bool)] out bool pfSelected
        );

        [PreserveSig]
        int SetStreamSelection(
            int dwStreamIndex,
            bool fSelected
        );

        [PreserveSig]
        int GetNativeMediaType(
            int dwStreamIndex,
            int dwMediaTypeIndex,
            out IMFMediaType ppMediaType
        );

        [PreserveSig]
        int GetCurrentMediaType(
            int dwStreamIndex,
            out IMFMediaType ppMediaType
        );

        [PreserveSig]
        int SetCurrentMediaType(
            int dwStreamIndex,
            ref int pdwReserved,
            IMFMediaType pMediaType
        );

        [PreserveSig]
        int SetCurrentPosition(
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid guidTimeFormat,
            [In] PropVariant varPosition
        );

        [PreserveSig]
        int ReadSample(
            int dwStreamIndex,
            int dwControlFlags,
            out int pdwActualStreamIndex,
            out int pdwStreamFlags,
            out long pllTimestamp,
            out IMFSample ppSample
        );

        [PreserveSig]
        int Flush(
            int dwStreamIndex
        );

        [PreserveSig]
        int GetServiceForStream(
            int dwStreamIndex,
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid guidService,
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid riid,
            [MarshalAs(UnmanagedType.IUnknown)] out object ppvObject
        );

        [PreserveSig]
        int GetPresentationAttribute(
            int dwStreamIndex,
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid guidAttribute,
            [In, Out, MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(PVMarshaler))] PropVariant pvarAttribute
        );
    }

    [ComImport, System.Security.SuppressUnmanagedCodeSecurity,
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown),
    Guid("deec8d99-fa1d-4d82-84c2-2c8969944867")]
    public interface IMFSourceReaderCallback
    {
        [PreserveSig]
        int OnReadSample(
            int hrStatus,
            int dwStreamIndex,
            MF_SOURCE_READER_FLAG dwStreamFlags,
            long llTimestamp,
            IMFSample pSample
        );

        [PreserveSig]
        int OnFlush(
            int dwStreamIndex
        );

        [PreserveSig]
        int OnEvent(
            int dwStreamIndex,
            IMFMediaEvent pEvent
        );
    }

    [ComImport, System.Security.SuppressUnmanagedCodeSecurity,
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown),
    Guid("3137f1cd-fe5e-4805-a5d8-fb477448cb3d")]
    public interface IMFSinkWriter
    {
        [PreserveSig]
        int AddStream(
            IMFMediaType pTargetMediaType,
            out int pdwStreamIndex
        );

        [PreserveSig]
        int SetInputMediaType(
            int dwStreamIndex,
            IMFMediaType pInputMediaType,
            IMFAttributes pEncodingParameters
        );

        [PreserveSig]
        int BeginWriting();

        [PreserveSig]
        int WriteSample(
            int dwStreamIndex,
            IMFSample pSample
        );

        [PreserveSig]
        int SendStreamTick(
            int dwStreamIndex,
            long llTimestamp
        );

        [PreserveSig]
        int PlaceMarker(
            int dwStreamIndex,
            IntPtr pvContext
        );

        [PreserveSig]
        int NotifyEndOfSegment(
            int dwStreamIndex
        );

        [PreserveSig]
        int Flush(
            int dwStreamIndex
        );

        [PreserveSig]
        int Finalize_();

        [PreserveSig]
        int GetServiceForStream(
            int dwStreamIndex,
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid guidService,
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid riid,
            [MarshalAs(UnmanagedType.IUnknown)] out object ppvObject
        );

        [PreserveSig]
        int GetStatistics(
            int dwStreamIndex,
            out MF_SINK_WRITER_STATISTICS pStats
        );
    }

    [ComImport, System.Security.SuppressUnmanagedCodeSecurity,
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown),
    Guid("666f76de-33d2-41b9-a458-29ed0a972c58")]
    public interface IMFSinkWriterCallback
    {
        [PreserveSig]
        int OnFinalize(
            int hrStatus
        );

        [PreserveSig]
        int OnMarker(
            int dwStreamIndex,
            IntPtr pvContext
        );
    }

    [ComImport, System.Security.SuppressUnmanagedCodeSecurity,
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown),
    Guid("7b981cf0-560e-4116-9875-b099895f23d7")]
    public interface IMFSourceReaderEx : IMFSourceReader
    {
        #region IMFSourceReader Methods

        [PreserveSig]
        new int GetStreamSelection(
            int dwStreamIndex,
            [MarshalAs(UnmanagedType.Bool)] out bool pfSelected
        );

        [PreserveSig]
        new int SetStreamSelection(
            int dwStreamIndex,
            bool fSelected
        );

        [PreserveSig]
        new int GetNativeMediaType(
            int dwStreamIndex,
            int dwMediaTypeIndex,
            out IMFMediaType ppMediaType
        );

        [PreserveSig]
        new int GetCurrentMediaType(
            int dwStreamIndex,
            out IMFMediaType ppMediaType
        );

        [PreserveSig]
        new int SetCurrentMediaType(
            int dwStreamIndex,
            ref int pdwReserved,
            IMFMediaType pMediaType
        );

        [PreserveSig]
        new int SetCurrentPosition(
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid guidTimeFormat,
            [In] PropVariant varPosition
        );

        [PreserveSig]
        new int ReadSample(
            int dwStreamIndex,
            int dwControlFlags,
            out int pdwActualStreamIndex,
            out int pdwStreamFlags,
            out long pllTimestamp,
            out IMFSample ppSample
        );

        [PreserveSig]
        new int Flush(
            int dwStreamIndex
        );

        [PreserveSig]
        new int GetServiceForStream(
            int dwStreamIndex,
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid guidService,
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid riid,
            [MarshalAs(UnmanagedType.IUnknown)] out object ppvObject
        );

        [PreserveSig]
        new int GetPresentationAttribute(
            int dwStreamIndex,
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid guidAttribute,
            [In, Out, MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(PVMarshaler))] PropVariant pvarAttribute
        );

#endregion

        [PreserveSig]
        int SetNativeMediaType(
            int dwStreamIndex,
            IMFMediaType pMediaType,
            out MF_SOURCE_READER_FLAG pdwStreamFlags);

        [PreserveSig]
        int AddTransformForStream(
            int dwStreamIndex,
            [MarshalAs(UnmanagedType.IUnknown)] out object pTransformOrActivate);

        [PreserveSig]
        int RemoveAllTransformsForStream(
            int dwStreamIndex);

        [PreserveSig]
        int GetTransformForStream(
            int dwStreamIndex,
            int dwTransformIndex,
            out Guid pGuidCategory,
            out IMFTransform ppTransform);
    }

    [ComImport, System.Security.SuppressUnmanagedCodeSecurity,
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown),
    Guid("588d72ab-5Bc1-496a-8714-b70617141b25")]
    public interface IMFSinkWriterEx : IMFSinkWriter
    {

        #region IMFSinkWriter methods

        [PreserveSig]
        new int AddStream(
            IMFMediaType pTargetMediaType,
            out int pdwStreamIndex
        );

        [PreserveSig]
        new int SetInputMediaType(
            int dwStreamIndex,
            IMFMediaType pInputMediaType,
            IMFAttributes pEncodingParameters
        );

        [PreserveSig]
        new int BeginWriting();

        [PreserveSig]
        new int WriteSample(
            int dwStreamIndex,
            IMFSample pSample
        );

        [PreserveSig]
        new int SendStreamTick(
            int dwStreamIndex,
            long llTimestamp
        );

        [PreserveSig]
        new int PlaceMarker(
            int dwStreamIndex,
            IntPtr pvContext
        );

        [PreserveSig]
        new int NotifyEndOfSegment(
            int dwStreamIndex
        );

        [PreserveSig]
        new int Flush(
            int dwStreamIndex
        );

        [PreserveSig]
        new int Finalize_();

        [PreserveSig]
        new int GetServiceForStream(
            int dwStreamIndex,
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid guidService,
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid riid,
            [MarshalAs(UnmanagedType.IUnknown)] out object ppvObject
        );

        [PreserveSig]
        new int GetStatistics(
            int dwStreamIndex,
            out MF_SINK_WRITER_STATISTICS pStats
        );

        #endregion

        [PreserveSig]
        int GetTransformForStream(
            int dwStreamIndex,
            int dwTransformIndex,
            out Guid pGuidCategory,
            out IMFTransform ppTransform);
    }

#endif

    #endregion
}
