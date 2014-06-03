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
using System.Collections;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Security;

using MediaFoundation.Misc;
using MediaFoundation.Transform;
using MediaFoundation.ReadWrite;
using MediaFoundation.MFPlayer;
using MediaFoundation.EVR;

namespace MediaFoundation
{
    public static class MFExtern
    {
        [DllImport("mfplat.dll", ExactSpelling = true), SuppressUnmanagedCodeSecurity]
        public static extern int MFShutdown();

        [DllImport("mfplat.dll", ExactSpelling = true), SuppressUnmanagedCodeSecurity]
        public static extern int MFStartup(
            int Version,
            MFStartup dwFlags
        );

        [DllImport("mfplat.dll", ExactSpelling = true), SuppressUnmanagedCodeSecurity]
        public static extern int MFCreateSystemTimeSource(
            out IMFPresentationTimeSource ppSystemTimeSource
        );

        [DllImport("mfplat.dll", ExactSpelling = true), SuppressUnmanagedCodeSecurity]
        public static extern int MFCreateCollection(
            out IMFCollection ppIMFCollection
        );

        [DllImport("mfplat.dll", ExactSpelling = true), SuppressUnmanagedCodeSecurity]
        public static extern int MFCreateStreamDescriptor(
            int dwStreamIdentifier,
            int cMediaTypes,
            [In, MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 1)] IMFMediaType[] apMediaTypes,
            out IMFStreamDescriptor ppDescriptor
        );

        [DllImport("mfplat.dll", ExactSpelling = true), SuppressUnmanagedCodeSecurity]
        public static extern int CreatePropertyStore(
            out IPropertyStore ppStore
        );

        [DllImport("mfplat.dll", ExactSpelling = true), SuppressUnmanagedCodeSecurity]
        public static extern int MFCreateAttributes(
            out IMFAttributes ppMFAttributes,
            int cInitialSize
        );

        [DllImport("mfplat.dll", ExactSpelling = true), SuppressUnmanagedCodeSecurity]
        public static extern int MFCreateWaveFormatExFromMFMediaType(
            IMFMediaType pMFType,
            [Out, MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(WEMarshaler))] out WaveFormatEx ppWF,
            out int pcbSize,
            MFWaveFormatExConvertFlags Flags
        );

        [DllImport("mfplat.dll", ExactSpelling = true), SuppressUnmanagedCodeSecurity]
        public static extern int MFCreateAsyncResult(
            [MarshalAs(UnmanagedType.IUnknown)] object punkObject,
            IMFAsyncCallback pCallback,
            [MarshalAs(UnmanagedType.IUnknown)] object punkState,
            out IMFAsyncResult ppAsyncResult
        );

        [DllImport("mfplat.dll", ExactSpelling = true), SuppressUnmanagedCodeSecurity]
        public static extern int MFInvokeCallback(
            IMFAsyncResult pAsyncResult
        );

        [DllImport("mfplat.dll", ExactSpelling = true), SuppressUnmanagedCodeSecurity]
        public static extern int MFCreatePresentationDescriptor(
            int cStreamDescriptors,
            [In, MarshalAs(UnmanagedType.LPArray)] IMFStreamDescriptor[] apStreamDescriptors,
            out IMFPresentationDescriptor ppPresentationDescriptor
        );

        [DllImport("mfplat.dll", ExactSpelling = true), SuppressUnmanagedCodeSecurity]
        public static extern int MFInitMediaTypeFromWaveFormatEx(
            IMFMediaType pMFType,
            [In, MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(WEMarshaler))] WaveFormatEx ppWF,
            int cbBufSize
        );

        [DllImport("mfplat.dll", ExactSpelling = true), SuppressUnmanagedCodeSecurity]
        public static extern int MFCreateEventQueue(
            out IMFMediaEventQueue ppMediaEventQueue
        );

        [DllImport("mfplat.dll", ExactSpelling = true), SuppressUnmanagedCodeSecurity]
        public static extern int MFCreateMediaType(
            out IMFMediaType ppMFType
        );

        [DllImport("mfplat.dll", ExactSpelling = true), SuppressUnmanagedCodeSecurity]
        public static extern int MFCreateMediaEvent(
            MediaEventType met,
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid guidExtendedType,
            int hrStatus,
            [In, MarshalAs(UnmanagedType.LPStruct)] ConstPropVariant pvValue,
            out IMFMediaEvent ppEvent
        );

        [DllImport("mfplat.dll", ExactSpelling = true), SuppressUnmanagedCodeSecurity]
        public static extern int MFCreateSample(
            out IMFSample ppIMFSample
        );

        [DllImport("mfplat.dll", ExactSpelling = true), SuppressUnmanagedCodeSecurity]
        public static extern int MFCreateMemoryBuffer(
            int cbMaxLength,
            out IMFMediaBuffer ppBuffer
        );

        [DllImport("mf.dll", ExactSpelling = true), SuppressUnmanagedCodeSecurity]
        public static extern int MFGetService(
            [In, MarshalAs(UnmanagedType.Interface)] object punkObject,
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid guidService,
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid riid,
            [Out, MarshalAs(UnmanagedType.Interface)] out object ppvObject
        );

        [DllImport("mf.dll", ExactSpelling = true), SuppressUnmanagedCodeSecurity]
        public static extern int MFCreateVideoRendererActivate(
            IntPtr hwndVideo,
            out IMFActivate ppActivate
        );

        [DllImport("mf.dll", ExactSpelling = true), SuppressUnmanagedCodeSecurity]
        public static extern int MFCreateTopologyNode(
            MFTopologyType NodeType,
            out IMFTopologyNode ppNode
        );

        [DllImport("mf.dll", ExactSpelling = true), SuppressUnmanagedCodeSecurity]
        public static extern int MFCreateSourceResolver(
            out IMFSourceResolver ppISourceResolver
        );

        [DllImport("mf.dll", ExactSpelling = true), SuppressUnmanagedCodeSecurity]
        public static extern int MFCreateMediaSession(
            IMFAttributes pConfiguration,
            out IMFMediaSession ppMediaSession
        );

        [DllImport("mf.dll", ExactSpelling = true), SuppressUnmanagedCodeSecurity]
        public static extern int MFCreateTopology(
            out IMFTopology ppTopo
        );

        [DllImport("mf.dll", ExactSpelling = true), SuppressUnmanagedCodeSecurity]
        public static extern int MFCreateAudioRendererActivate(
            out IMFActivate ppActivate
        );

        [DllImport("mf.dll", ExactSpelling = true), SuppressUnmanagedCodeSecurity]
        public static extern int MFCreatePresentationClock(
            out IMFPresentationClock ppPresentationClock
        );

        [DllImport("mf.dll", ExactSpelling = true), SuppressUnmanagedCodeSecurity]
        public static extern int MFEnumDeviceSources(
            IMFAttributes pAttributes,
            [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 2)] out IMFActivate[] pppSourceActivate,
            out int pcSourceActivate
        );

        [DllImport("mfplat.dll", CharSet = CharSet.Unicode, ExactSpelling = true), SuppressUnmanagedCodeSecurity]
        public static extern int MFTRegister(
            [In, MarshalAs(UnmanagedType.Struct)] Guid clsidMFT,
            [In, MarshalAs(UnmanagedType.Struct)] Guid guidCategory,
            [In, MarshalAs(UnmanagedType.LPWStr)] string pszName,
            [In] int Flags, // Must be zero
            [In] int cInputTypes,
            [In, MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(RTAMarshaler))]
            object pInputTypes, // should be MFTRegisterTypeInfo[], but .Net bug prevents in x64
            [In] int cOutputTypes,
            [In, MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(RTAMarshaler))]
            object pOutputTypes, // should be MFTRegisterTypeInfo[], but .Net bug prevents in x64
            [In] IMFAttributes pAttributes
            );

        [DllImport("mfplat.dll", ExactSpelling = true), SuppressUnmanagedCodeSecurity]
        public static extern int MFTUnregister(
            [In, MarshalAs(UnmanagedType.Struct)] Guid clsidMFT
            );

        [DllImport("mfplat.dll", CharSet = CharSet.Unicode, ExactSpelling = true), SuppressUnmanagedCodeSecurity]
        public static extern int MFTGetInfo(
            [In, MarshalAs(UnmanagedType.Struct)] Guid clsidMFT,
            [MarshalAs(UnmanagedType.LPWStr)] out string pszName,
            [In, Out, MarshalAs(UnmanagedType.CustomMarshaler, MarshalCookie = "0", MarshalTypeRef = typeof(RTIMarshaler))]
            ArrayList ppInputTypes,
            [In, Out, MarshalAs(UnmanagedType.CustomMarshaler, MarshalCookie = "0", MarshalTypeRef = typeof(RTIMarshaler))]
            MFInt pcInputTypes,
            [In, Out, MarshalAs(UnmanagedType.CustomMarshaler, MarshalCookie = "1", MarshalTypeRef = typeof(RTIMarshaler))]
            ArrayList ppOutputTypes,
            [In, Out, MarshalAs(UnmanagedType.CustomMarshaler, MarshalCookie = "1", MarshalTypeRef = typeof(RTIMarshaler))]
            MFInt pcOutputTypes,
            IntPtr ip // Must be IntPtr.Zero due to MF bug, but should be out IMFAttributes ppAttributes
            );

        [DllImport("mf.dll", ExactSpelling = true), SuppressUnmanagedCodeSecurity]
        public static extern int CreateNamedPropertyStore(
            out INamedPropertyStore ppStore
        );

        [DllImport("mfplat.dll", ExactSpelling = true), SuppressUnmanagedCodeSecurity]
        public static extern int MFLockPlatform();

        [DllImport("mfplat.dll", ExactSpelling = true), SuppressUnmanagedCodeSecurity]
        public static extern int MFUnlockPlatform();

        [DllImport("mfplat.dll", ExactSpelling = true), SuppressUnmanagedCodeSecurity]
        public static extern int MFGetTimerPeriodicity(
            out int Periodicity);

        [DllImport("mfplat.dll", CharSet = CharSet.Unicode, ExactSpelling = true), SuppressUnmanagedCodeSecurity]
        public static extern int MFCreateFile(
            MFFileAccessMode AccessMode,
            MFFileOpenMode OpenMode,
            MFFileFlags fFlags,
            [MarshalAs(UnmanagedType.LPWStr)] string pwszFileURL,
            out IMFByteStream ppIByteStream);

        [DllImport("mfplat.dll", ExactSpelling = true), SuppressUnmanagedCodeSecurity]
        public static extern int MFCreateTempFile(
            MFFileAccessMode AccessMode,
            MFFileOpenMode OpenMode,
            MFFileFlags fFlags,
            out IMFByteStream ppIByteStream);

        [DllImport("mfplat.dll", CharSet = CharSet.Unicode, ExactSpelling = true), SuppressUnmanagedCodeSecurity]
        public static extern int MFBeginCreateFile(
            [In] MFFileAccessMode AccessMode,
            [In] MFFileOpenMode OpenMode,
            [In] MFFileFlags fFlags,
            [In, MarshalAs(UnmanagedType.LPWStr)] string pwszFilePath,
            [In] IMFAsyncCallback pCallback,
            [In] [MarshalAs(UnmanagedType.IUnknown)] object pState,
            [MarshalAs(UnmanagedType.IUnknown)] out object ppCancelCookie);

        [DllImport("mfplat.dll", ExactSpelling = true), SuppressUnmanagedCodeSecurity]
        public static extern int MFEndCreateFile(
            [In] IMFAsyncResult pResult,
            out IMFByteStream ppFile);

        [DllImport("mfplat.dll", ExactSpelling = true), SuppressUnmanagedCodeSecurity]
        public static extern int MFCancelCreateFile(
            [In] [MarshalAs(UnmanagedType.IUnknown)] object pCancelCookie);

        [DllImport("mfplat.dll", ExactSpelling = true), SuppressUnmanagedCodeSecurity]
        public static extern int MFCreateAlignedMemoryBuffer(
            [In] int cbMaxLength,
            [In] int cbAligment,
            out IMFMediaBuffer ppBuffer);

        [DllImport("mfplat.dll", ExactSpelling = true), SuppressUnmanagedCodeSecurity]
        public static extern long MFGetSystemTime(
            );

        [DllImport("mf.dll", ExactSpelling = true), SuppressUnmanagedCodeSecurity]
        public static extern int MFGetSupportedSchemes(
            [In, Out, MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(PVMarshaler))] PropVariant pPropVarSchemeArray
        );

        [DllImport("mf.dll", ExactSpelling = true), SuppressUnmanagedCodeSecurity]
        public static extern int MFGetSupportedMimeTypes(
            [In, Out, MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(PVMarshaler))] PropVariant pPropVarSchemeArray
        );

        [DllImport("mf.dll", ExactSpelling = true), SuppressUnmanagedCodeSecurity]
        public static extern int MFCreateSimpleTypeHandler(
            out IMFMediaTypeHandler ppHandler
        );

        [DllImport("mf.dll", ExactSpelling = true), SuppressUnmanagedCodeSecurity]
        public static extern int MFCreateSequencerSegmentOffset(
            int dwId,
            long hnsOffset,
            [In, Out, MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(PVMarshaler))] PropVariant pvarSegmentOffset
        );

        [DllImport("mf.dll", ExactSpelling = true), SuppressUnmanagedCodeSecurity]
        public static extern int MFCreateVideoRenderer(
            [MarshalAs(UnmanagedType.LPStruct)] Guid riidRenderer,
            [MarshalAs(UnmanagedType.IUnknown)] out object ppVideoRenderer
            );

        [DllImport("mfplat.dll", ExactSpelling = true), SuppressUnmanagedCodeSecurity]
        public static extern int MFCreateMediaBufferWrapper(
            [In] IMFMediaBuffer pBuffer,
            [In] int cbOffset,
            [In] int dwLength,
            out IMFMediaBuffer ppBuffer);

        // Technically, the last param should be an IMediaBuffer.  However, that interface is
        // beyond the scope of this library.  If you are using DirectShowNet (where this *is*
        // defined), you can cast from the object to the IMediaBuffer.
        [DllImport("mfplat.dll", ExactSpelling = true), SuppressUnmanagedCodeSecurity]
        public static extern int MFCreateLegacyMediaBufferOnMFMediaBuffer(
            [In] IMFSample pSample,
            [In] IMFMediaBuffer pMFMediaBuffer,
            [In] int cbOffset,
            [MarshalAs(UnmanagedType.IUnknown)] out object ppMediaBuffer);

        [DllImport("mfplat.dll", ExactSpelling = true), SuppressUnmanagedCodeSecurity]
        public static extern int MFInitAttributesFromBlob(
            [In] IMFAttributes pAttributes,
            IntPtr pBuf,
            [In] int cbBufSize
            );

        [DllImport("mfplat.dll", ExactSpelling = true), SuppressUnmanagedCodeSecurity]
        public static extern int MFGetAttributesAsBlobSize(
            [In] IMFAttributes pAttributes,
            out int pcbBufSize
            );

        [DllImport("mfplat.dll", ExactSpelling = true), SuppressUnmanagedCodeSecurity]
        public static extern int MFGetAttributesAsBlob(
            [In] IMFAttributes pAttributes,
            IntPtr pBuf,
            [In] int cbBufSize
            );

        [DllImport("mfplat.dll", ExactSpelling = true), SuppressUnmanagedCodeSecurity]
        public static extern int MFSerializeAttributesToStream(
            IMFAttributes pAttr,
            MFAttributeSerializeOptions dwOptions,
            IStream pStm);

        [DllImport("mfplat.dll", ExactSpelling = true), SuppressUnmanagedCodeSecurity]
        public static extern int MFDeserializeAttributesFromStream(
            IMFAttributes pAttr,
            MFAttributeSerializeOptions dwOptions,
            IStream pStm);

        [DllImport("mfplat.dll", ExactSpelling = true), SuppressUnmanagedCodeSecurity]
        public static extern int MFCreateMFVideoFormatFromMFMediaType(
            [In] IMFMediaType pMFType,
            out MFVideoFormat ppMFVF,
            out int pcbSize
            );

        [DllImport("evr.dll", ExactSpelling = true), SuppressUnmanagedCodeSecurity]
        public static extern int MFGetUncompressedVideoFormat(
            [In, MarshalAs(UnmanagedType.LPStruct)] MFVideoFormat pVideoFormat
        );

        [DllImport("mfplat.dll", ExactSpelling = true), SuppressUnmanagedCodeSecurity]
        public static extern int MFInitMediaTypeFromMFVideoFormat(
            [In] IMFMediaType pMFType,
            MFVideoFormat pMFVF,
            [In] int cbBufSize
            );

        [DllImport("mfplat.dll", ExactSpelling = true), SuppressUnmanagedCodeSecurity]
        public static extern int MFInitAMMediaTypeFromMFMediaType(
            [In] IMFMediaType pMFType,
            [In, MarshalAs(UnmanagedType.Struct)] Guid guidFormatBlockType,
            [Out] AMMediaType pAMType
            );

        [DllImport("mfplat.dll", ExactSpelling = true), SuppressUnmanagedCodeSecurity]
        public static extern int MFCreateAMMediaTypeFromMFMediaType(
            [In] IMFMediaType pMFType,
            [In, MarshalAs(UnmanagedType.Struct)] Guid guidFormatBlockType,
            out AMMediaType ppAMType // delete with DeleteMediaType
            );

        [DllImport("mfplat.dll", ExactSpelling = true), SuppressUnmanagedCodeSecurity]
        public static extern int MFInitMediaTypeFromAMMediaType(
            [In] IMFMediaType pMFType,
            [In] AMMediaType pAMType
            );

        [DllImport("mfplat.dll", ExactSpelling = true), SuppressUnmanagedCodeSecurity]
        public static extern int MFInitMediaTypeFromVideoInfoHeader(
            [In] IMFMediaType pMFType,
            VideoInfoHeader pVIH,
            [In] int cbBufSize,
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid pSubtype
            );

        [DllImport("mfplat.dll", ExactSpelling = true), SuppressUnmanagedCodeSecurity]
        public static extern int MFInitMediaTypeFromVideoInfoHeader2(
            [In] IMFMediaType pMFType,
            VideoInfoHeader2 pVIH2,
            [In] int cbBufSize,
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid pSubtype
            );

        [DllImport("evr.dll", ExactSpelling = true), SuppressUnmanagedCodeSecurity]
        public static extern int MFCreateVideoMediaType(
            MFVideoFormat pVideoFormat,
            out IMFVideoMediaType ppIVideoMediaType
            );

        [DllImport("mfplat.dll", ExactSpelling = true), SuppressUnmanagedCodeSecurity]
        public static extern int MFTEnum(
            [In, MarshalAs(UnmanagedType.Struct)] Guid guidCategory,
            [In] int Flags, // Must be zero
            [In, MarshalAs(UnmanagedType.LPStruct)] MFTRegisterTypeInfo pInputType,
            [In, MarshalAs(UnmanagedType.LPStruct)] MFTRegisterTypeInfo pOutputType,
            [In] IMFAttributes pAttributes,
            [In, Out, MarshalAs(UnmanagedType.CustomMarshaler, MarshalCookie = "0", MarshalTypeRef = typeof(GAMarshaler))]
            ArrayList ppclsidMFT,
            [In, Out, MarshalAs(UnmanagedType.CustomMarshaler, MarshalCookie = "0", MarshalTypeRef = typeof(GAMarshaler))]
            MFInt pcMFTs
            );

        [DllImport("mf.dll", ExactSpelling = true), SuppressUnmanagedCodeSecurity]
        public static extern int MFCreateSequencerSource(
            [MarshalAs(UnmanagedType.IUnknown)] object pReserved,
            out IMFSequencerSource ppSequencerSource
        );

        [DllImport("mfplat.dll", ExactSpelling = true), SuppressUnmanagedCodeSecurity]
        public static extern int MFAllocateWorkQueue(
            out int pdwWorkQueue);

        [DllImport("mfplat.dll", ExactSpelling = true), SuppressUnmanagedCodeSecurity]
        public static extern int MFUnlockWorkQueue(
            [In] int dwWorkQueue);

        [DllImport("mfplat.dll", ExactSpelling = true), SuppressUnmanagedCodeSecurity]
        public static extern int MFPutWorkItem(
            int dwQueue,
            IMFAsyncCallback pCallback,
            [MarshalAs(UnmanagedType.IUnknown)] object pState);

        [DllImport("mf.dll", ExactSpelling = true), SuppressUnmanagedCodeSecurity]
        public static extern int MFCreatePMPMediaSession(
            MFPMPSessionCreationFlags dwCreationFlags,
            IMFAttributes pConfiguration,
            out IMFMediaSession ppMediaSession,
            out IMFActivate ppEnablerActivate
        );

        [DllImport("mf.dll", ExactSpelling = true), SuppressUnmanagedCodeSecurity]
        public static extern int MFCreateASFContentInfo(
            out IMFASFContentInfo ppIContentInfo);

        [DllImport("mf.dll", ExactSpelling = true), SuppressUnmanagedCodeSecurity]
        public static extern int MFCreateASFSplitter(
            out IMFASFSplitter ppISplitter);

        [DllImport("mf.dll", ExactSpelling = true), SuppressUnmanagedCodeSecurity]
        public static extern int MFCreateASFProfile(
            out IMFASFProfile ppIProfile);

        [DllImport("mf.dll", ExactSpelling = true), SuppressUnmanagedCodeSecurity]
        public static extern int MFCreateAudioRenderer(
            IMFAttributes pAudioAttributes,
            out IMFMediaSink ppSink
        );

        [DllImport("mf.dll", ExactSpelling = true), SuppressUnmanagedCodeSecurity]
        public static extern int MFCreateASFIndexer(
            out IMFASFIndexer ppIIndexer);

        [DllImport("evr.dll", ExactSpelling = true), SuppressUnmanagedCodeSecurity]
        public static extern int MFGetStrideForBitmapInfoHeader(
            int format,
            int dwWidth,
            out int pStride
            );

        [DllImport("mfplat.dll", ExactSpelling = true), SuppressUnmanagedCodeSecurity]
        public static extern int MFCalculateBitmapImageSize(
            [In, MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(BMMarshaler))] BitmapInfoHeader pBMIH,
            [In] int cbBufSize,
            out int pcbImageSize,
            out bool pbKnown
            );

        [DllImport("evr.dll", ExactSpelling = true), SuppressUnmanagedCodeSecurity]
        public static extern int MFCreateVideoSampleFromSurface(
            [In, MarshalAs(UnmanagedType.IUnknown)] object pUnkSurface,
            out IMFSample ppSample
            );

        [DllImport("mfplat.dll", ExactSpelling = true), SuppressUnmanagedCodeSecurity]
        public static extern int MFFrameRateToAverageTimePerFrame(
            [In] int unNumerator,
            [In] int unDenominator,
            out long punAverageTimePerFrame
            );

        [DllImport("mfplat.dll", ExactSpelling = true), SuppressUnmanagedCodeSecurity]
        public static extern int MFUnwrapMediaType(
            [In] IMFMediaType pWrap,
            out IMFMediaType ppOrig
            );

#if ALLOW_UNTESTED_INTERFACES

        #region Tested
        // While these methods are tested, the interfaces they use are not

        [DllImport("mf.dll", ExactSpelling = true), SuppressUnmanagedCodeSecurity]
        public static extern int MFCreateTopoLoader(
            out IMFTopoLoader ppObj
        );

        [DllImport("mf.dll", ExactSpelling = true), SuppressUnmanagedCodeSecurity]
        public static extern int MFCreateStandardQualityManager(
            out IMFQualityManager ppQualityManager
        );

        [DllImport("mf.dll", ExactSpelling = true), SuppressUnmanagedCodeSecurity]
        public static extern int MFCreatePMPServer(
            MFPMPSessionCreationFlags dwCreationFlags,
            out IMFPMPServer ppPMPServer
        );

        [DllImport("mf.dll", ExactSpelling = true), SuppressUnmanagedCodeSecurity]
        public static extern int MFCreateASFMultiplexer(
            out IMFASFMultiplexer ppIMultiplexer);

        #endregion

        #region Work Queue

        [DllImport("mfplat.dll", ExactSpelling = true), SuppressUnmanagedCodeSecurity]
        public static extern int MFPutWorkItemEx(
            int dwQueue,
            IMFAsyncResult pResult);

        [DllImport("mfplat.dll", ExactSpelling = true), SuppressUnmanagedCodeSecurity]
        public static extern int MFScheduleWorkItem(
            IMFAsyncCallback pCallback,
            [MarshalAs(UnmanagedType.IUnknown)] object pState,
            long Timeout,
            long pKey);

        [DllImport("mfplat.dll", ExactSpelling = true), SuppressUnmanagedCodeSecurity]
        public static extern int MFScheduleWorkItemEx(
            IMFAsyncResult pResult,
            long Timeout,
            long pKey);

        [DllImport("mfplat.dll", ExactSpelling = true), SuppressUnmanagedCodeSecurity]
        public static extern int MFCancelWorkItem(
            long Key);

        [DllImport("mfplat.dll", ExactSpelling = true), SuppressUnmanagedCodeSecurity]
        public static extern int MFAddPeriodicCallback(
            IntPtr Callback,
            [MarshalAs(UnmanagedType.IUnknown)] object pContext,
            out int pdwKey);

        [DllImport("mfplat.dll", ExactSpelling = true), SuppressUnmanagedCodeSecurity]
        public static extern int MFRemovePeriodicCallback(
            int dwKey);

        [DllImport("mfplat.dll", ExactSpelling = true), SuppressUnmanagedCodeSecurity]
        public static extern int MFLockWorkQueue(
            [In] int dwWorkQueue);

        [DllImport("mfplat.dll", CharSet = CharSet.Unicode, ExactSpelling = true), SuppressUnmanagedCodeSecurity]
        public static extern int MFBeginRegisterWorkQueueWithMMCSS(
            int dwWorkQueueId,
            [In, MarshalAs(UnmanagedType.LPWStr)] string wszClass,
            int dwTaskId,
            [In] IMFAsyncCallback pDoneCallback,
            [In] [MarshalAs(UnmanagedType.IUnknown)] object pDoneState);

        [DllImport("mfplat.dll", ExactSpelling = true), SuppressUnmanagedCodeSecurity]
        public static extern int MFEndRegisterWorkQueueWithMMCSS(
            [In] IMFAsyncResult pResult,
            out int pdwTaskId);

        [DllImport("mfplat.dll", ExactSpelling = true), SuppressUnmanagedCodeSecurity]
        public static extern int MFBeginUnregisterWorkQueueWithMMCSS(
            int dwWorkQueueId,
            [In] IMFAsyncCallback pDoneCallback,
            [In] [MarshalAs(UnmanagedType.IUnknown)] object pDoneState);

        [DllImport("mfplat.dll", ExactSpelling = true), SuppressUnmanagedCodeSecurity]
        public static extern int MFEndUnregisterWorkQueueWithMMCSS(
            [In] IMFAsyncResult pResult);

        [DllImport("mf.dll", CharSet = CharSet.Unicode, ExactSpelling = true), SuppressUnmanagedCodeSecurity]
        public static extern int MFSetWorkQueueClass(
            int dwWorkQueueId,
            [MarshalAs(UnmanagedType.LPWStr)] string szClass);

        [DllImport("mfplat.dll", CharSet = CharSet.Unicode, ExactSpelling = true), SuppressUnmanagedCodeSecurity]
        public static extern int MFGetWorkQueueMMCSSClass(
            int dwWorkQueueId,
            [MarshalAs(UnmanagedType.LPWStr)] out string pwszClass,
            out int pcchClass);

        [DllImport("mfplat.dll", ExactSpelling = true), SuppressUnmanagedCodeSecurity]
        public static extern int MFGetWorkQueueMMCSSTaskId(
            int dwWorkQueueId,
            out int pdwTaskId);

        #endregion

        #region Check dllimport

        [DllImport("mfplat.dll", ExactSpelling = true), SuppressUnmanagedCodeSecurity]
        public static extern int MFCreateAC3MediaSink(
            IMFByteStream pTargetByteStream,
            IMFMediaType pAudioMediaType,
            out IMFMediaSink ppMediaSink
        );

        [DllImport("mfplat.dll", ExactSpelling = true), SuppressUnmanagedCodeSecurity]
        public static extern int MFCreateADTSMediaSink(
            IMFByteStream pTargetByteStream,
            IMFMediaType pAudioMediaType,
            out IMFMediaSink ppMediaSink
        );
        
        [DllImport("mfplat.dll", ExactSpelling = true), SuppressUnmanagedCodeSecurity]
        public static extern int MFCreateMuxSink(
        [In, MarshalAs(UnmanagedType.LPStruct)] Guid guidOutputSubType,
            IMFAttributes pOutputAttributes,
            IMFByteStream pOutputByteStream,
            out IMFMediaSink ppMuxSink
        );

        [DllImport("mfplat.dll", ExactSpelling = true), SuppressUnmanagedCodeSecurity]
        public static extern int MFCreateFMPEG4MediaSink(
            IMFByteStream pIByteStream,
            IMFMediaType pVideoMediaType,
            IMFMediaType pAudioMediaType,
            out IMFMediaSink ppIMediaSink
        );

        [DllImport("mfplat.dll", ExactSpelling = true), SuppressUnmanagedCodeSecurity]
        public static extern int MFCreateTranscodeTopologyFromByteStream(
            IMFMediaSource pSrc,
            IMFByteStream pOutputStream,
            IMFTranscodeProfile pProfile,
            out IMFTopology ppTranscodeTopo
        );

        [DllImport("mfplat.dll", ExactSpelling = true), SuppressUnmanagedCodeSecurity]
        public static extern int MFCreateTrackedSample(
            out IMFTrackedSample ppMFSample
        );

        [DllImport("mfplat.dll", ExactSpelling = true), SuppressUnmanagedCodeSecurity]
        public static extern int MFCreateStreamOnMFByteStream(
            IMFByteStream pByteStream,
            out IStream ppStream
        );

        [DllImport("mfplat.dll", ExactSpelling = true), SuppressUnmanagedCodeSecurity]
        public static extern int MFCreateMFByteStreamOnStreamEx(
            [MarshalAs(UnmanagedType.IUnknown)] object punkStream,
            out IMFByteStream ppByteStream
        );

        [DllImport("mfplat.dll", ExactSpelling = true), SuppressUnmanagedCodeSecurity]
        public static extern int MFCreateStreamOnMFByteStreamEx(
            IMFByteStream pByteStream,
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid riid,
            [MarshalAs(UnmanagedType.IUnknown)] out object ppv
        );

        [DllImport("mfplat.dll", ExactSpelling = true), SuppressUnmanagedCodeSecurity]
        public static extern int MFCreateMediaTypeFromProperties(
        [MarshalAs(UnmanagedType.IUnknown)] object punkStream,
            out IMFMediaType ppMediaType
        );

        [DllImport("mfplat.dll", ExactSpelling = true), SuppressUnmanagedCodeSecurity]
        public static extern int MFCreatePropertiesFromMediaType(
            IMFMediaType pMediaType,
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid riid,
            [MarshalAs(UnmanagedType.IUnknown)] out object ppv
        );

        [DllImport("mfplat.dll", ExactSpelling = true), SuppressUnmanagedCodeSecurity]
        public static extern int MFCreateProtectedEnvironmentAccess(
            out IMFProtectedEnvironmentAccess ppAccess
        );

        [DllImport("mfplat.dll", ExactSpelling = true), SuppressUnmanagedCodeSecurity]
        public static extern int MFLoadSignedLibrary(
        [In, MarshalAs(UnmanagedType.LPWStr)] string pszName,
            out IMFSignedLibrary ppLib
        );

        [DllImport("mfplat.dll", ExactSpelling = true), SuppressUnmanagedCodeSecurity]
        public static extern int MFGetSystemId(
            out IMFSystemId ppId
        );

        [DllImport("mfplat.dll", ExactSpelling = true), SuppressUnmanagedCodeSecurity]
        public static extern int MFPutWorkItem2(
            int dwQueue,
            int Priority,
            IMFAsyncCallback pCallback,
            [In, MarshalAs(UnmanagedType.IUnknown)] object pState
        );

        [DllImport("mfplat.dll", ExactSpelling = true), SuppressUnmanagedCodeSecurity]
        public static extern int MFPutWorkItemEx2(
            int dwQueue,
            int Priority,
            IMFAsyncResult pResult
        );

        [DllImport("mfplat.dll", ExactSpelling = true), SuppressUnmanagedCodeSecurity]
        public static extern int MFPutWaitingWorkItem(
            IntPtr hEvent,
            int Priority,
            IMFAsyncResult pResult,
            out long pKey
        );

        [DllImport("mfplat.dll", ExactSpelling = true), SuppressUnmanagedCodeSecurity]
        public static extern int MFAllocateSerialWorkQueue(
            int dwWorkQueue,
            out int pdwWorkQueue
        );

        [DllImport("mfplat.dll", ExactSpelling = true), SuppressUnmanagedCodeSecurity]
        public static extern int MFBeginRegisterWorkQueueWithMMCSSEx(
            int dwWorkQueueId,
            [In, MarshalAs(UnmanagedType.LPWStr)] string wszClass,
            int dwTaskId,
            int lPriority,
            IMFAsyncCallback pDoneCallback,
            [In, MarshalAs(UnmanagedType.IUnknown)] object pDoneState
        );

        [DllImport("mfplat.dll", ExactSpelling = true), SuppressUnmanagedCodeSecurity]
        public static extern int MFRegisterPlatformWithMMCSS(
            [In, MarshalAs(UnmanagedType.LPWStr)] string wszClass,
            ref int pdwTaskId,
            int lPriority
        );

        [DllImport("mfplat.dll", ExactSpelling = true), SuppressUnmanagedCodeSecurity]
        public static extern int MFUnregisterPlatformFromMMCSS();

        [DllImport("mfplat.dll", ExactSpelling = true), SuppressUnmanagedCodeSecurity]
        public static extern int MFLockSharedWorkQueue(
            [In, MarshalAs(UnmanagedType.LPWStr)] string wszClass,
            int BasePriority,
            ref int pdwTaskId,
            out int pID
        );

        [DllImport("mfplat.dll", ExactSpelling = true), SuppressUnmanagedCodeSecurity]
        public static extern int MFGetWorkQueueMMCSSPriority(
            int dwWorkQueueId,
            out int lPriority
        );

        [DllImport("mfplat.dll", ExactSpelling = true), SuppressUnmanagedCodeSecurity]
        public static extern int MFMapDX9FormatToDXGIFormat(
            int dx9
        );

        [DllImport("mfplat.dll", ExactSpelling = true), SuppressUnmanagedCodeSecurity]
        public static extern int MFMapDXGIFormatToDX9Format(
            int dx11
        );

        [DllImport("mfplat.dll", ExactSpelling = true), SuppressUnmanagedCodeSecurity]
        public static extern int MFLockDXGIDeviceManager(
            out int pResetToken,
            out IMFDXGIDeviceManager ppManager
        );

        [DllImport("mfplat.dll", ExactSpelling = true), SuppressUnmanagedCodeSecurity]
        public static extern int MFUnlockDXGIDeviceManager();

        [DllImport("mfplat.dll", ExactSpelling = true), SuppressUnmanagedCodeSecurity]
        public static extern int MFCreateWICBitmapBuffer(
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid riid,
            [MarshalAs(UnmanagedType.Interface)] object punkSurface,
            out IMFMediaBuffer ppBuffer
        );

        [DllImport("mfplat.dll", ExactSpelling = true), SuppressUnmanagedCodeSecurity]
        public static extern int
        MFCreateDXGISurfaceBuffer(
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid riid,
            [MarshalAs(UnmanagedType.Interface)] object punkSurface,
            int uSubresourceIndex,
            bool fBottomUpWhenLinear,
            out IMFMediaBuffer ppBuffer
        );

        [DllImport("mfplat.dll", ExactSpelling = true), SuppressUnmanagedCodeSecurity]
        public static extern int MFCreateVideoSampleAllocatorEx(
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid riid,
            [MarshalAs(UnmanagedType.Interface)] out object ppSampleAllocator
        );

        [DllImport("mfplat.dll", ExactSpelling = true), SuppressUnmanagedCodeSecurity]
        public static extern int MFCreateDXGIDeviceManager(
            out int resetToken,
            out IMFDXGIDeviceManager ppDeviceManager
        );

        [DllImport("mfplat.dll", ExactSpelling = true), SuppressUnmanagedCodeSecurity]
        public static extern int MFRegisterLocalSchemeHandler(
            [In, MarshalAs(UnmanagedType.LPWStr)] string szScheme,
            IMFActivate pActivate
        );

        [DllImport("mfplat.dll", ExactSpelling = true), SuppressUnmanagedCodeSecurity]
        public static extern int MFRegisterLocalByteStreamHandler(
            [In, MarshalAs(UnmanagedType.LPWStr)] string szFileExtension,
            [In, MarshalAs(UnmanagedType.LPWStr)] string szMimeType,
            IMFActivate pActivate
        );

        [DllImport("mfplat.dll", ExactSpelling = true), SuppressUnmanagedCodeSecurity]
        public static extern int MFCreateMFByteStreamWrapper(
            IMFByteStream pStream,
            out IMFByteStream ppStreamWrapper
        );

        [DllImport("mfplat.dll", ExactSpelling = true), SuppressUnmanagedCodeSecurity]
        public static extern int MFCreateMediaExtensionActivate(
            [In, MarshalAs(UnmanagedType.LPWStr)] string szActivatableClassId,
            [MarshalAs(UnmanagedType.Interface)] object pConfiguration,
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid riid,
            [MarshalAs(UnmanagedType.Interface)] out object ppvObject
        );

        [DllImport("mfplat.dll", ExactSpelling = true), SuppressUnmanagedCodeSecurity]
        public static extern int MFCreate2DMediaBuffer(
            int dwWidth,
            int dwHeight,
            int dwFourCC,
            bool fBottomUp,
            out IMFMediaBuffer ppBuffer
        );

        [DllImport("mfplat.dll", ExactSpelling = true), SuppressUnmanagedCodeSecurity]
        public static extern int MFCreateMediaBufferFromMediaType(
            IMFMediaType pMediaType,
            long llDuration,
            int dwMinLength,
            int dwMinAlignment,
            out IMFMediaBuffer ppBuffer
        );

        [DllImport("mfplat.dll", ExactSpelling = true), SuppressUnmanagedCodeSecurity]
        public static extern int MFGetContentProtectionSystemCLSID(
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid guidProtectionSystemID,
            out Guid pclsid
        );

        #endregion

        [DllImport("MFCaptureEngine.dll", ExactSpelling = true), SuppressUnmanagedCodeSecurity]
        public static extern int MFCreateCaptureEngine(
            out IMFCaptureEngine ppCaptureEngine
        );

        [DllImport("Mfreadwrite.dll", ExactSpelling = true), SuppressUnmanagedCodeSecurity]
        public static extern int MFCreateSinkWriterFromURL(
            [In, MarshalAs(UnmanagedType.LPWStr)] string pwszOutputURL,
            IMFByteStream pByteStream,
            IMFAttributes pAttributes,
            out IMFSinkWriter ppSinkWriter
        );

        [DllImport("Mfreadwrite.dll", ExactSpelling = true), SuppressUnmanagedCodeSecurity]
        public static extern int MFCreateSinkWriterFromMediaSink(
            IMFMediaSink pMediaSink,
            IMFAttributes pAttributes,
            out IMFSinkWriter ppSinkWriter
        );

        [DllImport("Mfreadwrite.dll", ExactSpelling = true), SuppressUnmanagedCodeSecurity]
        public static extern int MFCreateSourceReaderFromURL(
            [In, MarshalAs(UnmanagedType.LPWStr)] string pwszURL,
            IMFAttributes pAttributes,
            out IMFSourceReader ppSourceReader
        );

        [DllImport("Mfreadwrite.dll", ExactSpelling = true), SuppressUnmanagedCodeSecurity]
        public static extern int MFCreateSourceReaderFromByteStream(
            IMFByteStream pByteStream,
            IMFAttributes pAttributes,
            out IMFSourceReader ppSourceReader
        );

        [DllImport("Mfreadwrite.dll", ExactSpelling = true), SuppressUnmanagedCodeSecurity]
        public static extern int MFCreateSourceReaderFromMediaSource(
            IMFMediaSource pMediaSource,
            IMFAttributes pAttributes,
            out IMFSourceReader ppSourceReader
        );

        [DllImport("mfplay.dll", ExactSpelling = true), SuppressUnmanagedCodeSecurity]
        public static extern int MFPCreateMediaPlayer(
            [In, MarshalAs(UnmanagedType.LPWStr)] string pwszURL,
            [MarshalAs(UnmanagedType.Bool)] bool fStartPlayback,
            MFP_CREATION_OPTIONS creationOptions,
            IMFPMediaPlayerCallback pCallback,
            IntPtr hWnd,
            out IMFPMediaPlayer ppMediaPlayer);

        [DllImport("mf.dll", ExactSpelling = true), SuppressUnmanagedCodeSecurity]
        public static extern int MFCreateRemoteDesktopPlugin(
            out IMFRemoteDesktopPlugin ppPlugin
        );

        [DllImport("evr.dll", ExactSpelling = true), SuppressUnmanagedCodeSecurity]
        public static extern int MFCreateDXSurfaceBuffer(
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid riid,
            [In] [MarshalAs(UnmanagedType.IUnknown)] object punkSurface,
            [In] bool fBottomUpWhenLinear,
            out IMFMediaBuffer ppBuffer);

        // --------------------------------------------------

        [DllImport("mfplat.dll", ExactSpelling = true), SuppressUnmanagedCodeSecurity]
        public static extern int MFValidateMediaTypeSize(
            [In, MarshalAs(UnmanagedType.Struct)] Guid FormatType,
            IntPtr pBlock,
            [In] int cbSize
            );

        [DllImport("mfplat.dll", ExactSpelling = true), SuppressUnmanagedCodeSecurity]
        public static extern int MFInitMediaTypeFromMPEG1VideoInfo(
            [In] IMFMediaType pMFType,
            MPEG1VideoInfo pMP1VI,
            [In] int cbBufSize,
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid pSubtype
            );

        [DllImport("mfplat.dll", ExactSpelling = true), SuppressUnmanagedCodeSecurity]
        public static extern int MFInitMediaTypeFromMPEG2VideoInfo(
            [In] IMFMediaType pMFType,
            Mpeg2VideoInfo pMP2VI,
            [In] int cbBufSize,
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid pSubtype
            );

        [DllImport("mfplat.dll", ExactSpelling = true), SuppressUnmanagedCodeSecurity]
        public static extern int MFCalculateImageSize(
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid guidSubtype,
            [In] int unWidth,
            [In] int unHeight,
            out int pcbImageSize
            );

        [DllImport("mfplat.dll", ExactSpelling = true), SuppressUnmanagedCodeSecurity]
        public static extern int MFAverageTimePerFrameToFrameRate(
            [In] long unAverageTimePerFrame,
            out int punNumerator,
            out int punDenominator
            );

        [DllImport("mfplat.dll", ExactSpelling = true), SuppressUnmanagedCodeSecurity]
        public static extern int MFCompareFullToPartialMediaType(
            [In] IMFMediaType pMFTypeFull,
            [In] IMFMediaType pMFTypePartial
            );

        [DllImport("mfplat.dll", ExactSpelling = true), SuppressUnmanagedCodeSecurity]
        public static extern int MFWrapMediaType(
            [In] IMFMediaType pOrig,
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid MajorType,
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid SubType,
            out IMFMediaType ppWrap
            );

        [DllImport("evr.dll", ExactSpelling = true), SuppressUnmanagedCodeSecurity]
        public static extern int MFCreateVideoMediaTypeFromSubtype(
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid pAMSubtype,
            out IMFVideoMediaType ppIVideoMediaType
            );

        [DllImport("evr.dll", ExactSpelling = true), SuppressUnmanagedCodeSecurity]
        public static extern bool MFIsFormatYUV(
            int Format
            );

        [DllImport("evr.dll", ExactSpelling = true), SuppressUnmanagedCodeSecurity]
        public static extern int MFGetPlaneSize(
            int format,
            int dwWidth,
            int dwHeight,
            out int pdwPlaneSize
            );

        [DllImport("evr.dll", ExactSpelling = true), SuppressUnmanagedCodeSecurity]
        public static extern int MFInitVideoFormat(
            [In] MFVideoFormat pVideoFormat,
            [In] MFStandardVideoFormat type
            );

        [DllImport("evr.dll", ExactSpelling = true), SuppressUnmanagedCodeSecurity]
        public static extern int MFInitVideoFormat_RGB(
            [In] MFVideoFormat pVideoFormat,
            [In] int dwWidth,
            [In] int dwHeight,
            [In] int D3Dfmt
            );

        [DllImport("evr.dll", ExactSpelling = true), SuppressUnmanagedCodeSecurity]
        public static extern int MFConvertColorInfoToDXVA(
            out int pdwToDXVA,
            MFVideoFormat pFromFormat
            );

        [DllImport("evr.dll", ExactSpelling = true), SuppressUnmanagedCodeSecurity]
        public static extern int MFConvertColorInfoFromDXVA(
            MFVideoFormat pToFormat,
            int dwFromDXVA
            );

        [DllImport("evr.dll", ExactSpelling = true), SuppressUnmanagedCodeSecurity]
        public static extern int MFCopyImage(
            IntPtr pDest,
            int lDestStride,
            IntPtr pSrc,
            int lSrcStride,
            int dwWidthInBytes,
            int dwLines
            );

        [DllImport("evr.dll", ExactSpelling = true), SuppressUnmanagedCodeSecurity]
        public static extern int MFConvertFromFP16Array(
            float[] pDest,
            short[] pSrc,
            int dwCount
            );

        [DllImport("evr.dll", ExactSpelling = true), SuppressUnmanagedCodeSecurity]
        public static extern int MFConvertToFP16Array(
            short[] pDest,
            float[] pSrc,
            int dwCount
            );

        [DllImport("mfplat.dll", ExactSpelling = true), SuppressUnmanagedCodeSecurity]
        public static extern int MFCreateMediaTypeFromRepresentation(
            [MarshalAs(UnmanagedType.Struct)] Guid guidRepresentation,
            IntPtr pvRepresentation,
            out IMFMediaType ppIMediaType
            );

        [DllImport("mf.dll", ExactSpelling = true), SuppressUnmanagedCodeSecurity]
        public static extern int MFRequireProtectedEnvironment(
            IMFPresentationDescriptor pPresentationDescriptor
        );

        [DllImport("mfplat.dll", ExactSpelling = true), SuppressUnmanagedCodeSecurity]
        public static extern int MFSerializePresentationDescriptor(
            IMFPresentationDescriptor pPD,
            out int pcbData,
            IntPtr ppbData
        );

        [DllImport("mfplat.dll", ExactSpelling = true), SuppressUnmanagedCodeSecurity]
        public static extern int MFDeserializePresentationDescriptor(
            int cbData,
            IntPtr pbData,
            out IMFPresentationDescriptor ppPD
        );

        [DllImport("mf.dll", ExactSpelling = true), SuppressUnmanagedCodeSecurity]
        public static extern int MFShutdownObject(
            object pUnk
        );

        [DllImport("mf.dll", ExactSpelling = true), SuppressUnmanagedCodeSecurity]
        public static extern int MFCreateSampleGrabberSinkActivate(
            IMFMediaType pIMFMediaType,
            IMFSampleGrabberSinkCallback pIMFSampleGrabberSinkCallback,
            out IMFActivate ppIActivate
        );

        [DllImport("mf.dll", CharSet = CharSet.Unicode, ExactSpelling = true), SuppressUnmanagedCodeSecurity]
        public static extern int MFCreateProxyLocator(
            [MarshalAs(UnmanagedType.LPWStr)] string pszProtocol,
            IPropertyStore pProxyConfig,
            out IMFNetProxyLocator ppProxyLocator
        );

        [DllImport("mf.dll", ExactSpelling = true), SuppressUnmanagedCodeSecurity]
        public static extern int MFCreateNetSchemePlugin(
            [MarshalAs(UnmanagedType.LPStruct)] Guid riid,
            [MarshalAs(UnmanagedType.IUnknown)] object ppvHandler
        );

        [DllImport("mf.dll", ExactSpelling = true), SuppressUnmanagedCodeSecurity]
        public static extern int MFCreateASFProfileFromPresentationDescriptor(
            [In] IMFPresentationDescriptor pIPD,
            out IMFASFProfile ppIProfile);

        [DllImport("mf.dll", ExactSpelling = true), SuppressUnmanagedCodeSecurity]
        public static extern int MFCreateASFIndexerByteStream(
            [In] IMFByteStream pIContentByteStream,
            [In] long cbIndexStartOffset,
            out IMFByteStream pIIndexByteStream);

        [DllImport("mf.dll", ExactSpelling = true), SuppressUnmanagedCodeSecurity]
        public static extern int MFCreateASFStreamSelector(
            [In] IMFASFProfile pIASFProfile,
            out IMFASFStreamSelector ppSelector);

        [DllImport("mf.dll", ExactSpelling = true), SuppressUnmanagedCodeSecurity]
        public static extern int MFCreateASFMediaSink(
            IMFByteStream pIByteStream,
            out IMFMediaSink ppIMediaSink
            );

        [DllImport("mf.dll", CharSet = CharSet.Unicode, ExactSpelling = true), SuppressUnmanagedCodeSecurity]
        public static extern int MFCreateASFMediaSinkActivate(
            [MarshalAs(UnmanagedType.LPWStr)] string pwszFileName,
            IMFASFContentInfo pContentInfo,
            out IMFActivate ppIActivate
            );

        [DllImport("mf.dll", ExactSpelling = true), SuppressUnmanagedCodeSecurity]
        public static extern int MFCreateWMVEncoderActivate(
            IMFMediaType pMediaType,
            IPropertyStore pEncodingConfigurationProperties,
            out IMFActivate ppActivate
            );

        [DllImport("mf.dll", ExactSpelling = true), SuppressUnmanagedCodeSecurity]
        public static extern int MFCreateWMAEncoderActivate(
            IMFMediaType pMediaType,
            IPropertyStore pEncodingConfigurationProperties,
            out IMFActivate ppActivate
            );

        [DllImport("mf.dll", ExactSpelling = true), SuppressUnmanagedCodeSecurity]
        public static extern int MFCreatePresentationDescriptorFromASFProfile(
            [In] IMFASFProfile pIProfile,
            out IMFPresentationDescriptor ppIPD);

        [DllImport("evr.dll", ExactSpelling = true), SuppressUnmanagedCodeSecurity]
        public static extern int MFCreateVideoPresenter(
            [In, MarshalAs(UnmanagedType.IUnknown)] object pOwner,
            [MarshalAs(UnmanagedType.LPStruct)] Guid riidDevice,
            [MarshalAs(UnmanagedType.LPStruct)] Guid riid,
            out IntPtr ppVideoPresenter
            );

        [DllImport("evr.dll", ExactSpelling = true), SuppressUnmanagedCodeSecurity]
        public static extern int MFCreateVideoMixer(
            [In, MarshalAs(UnmanagedType.IUnknown)] object pOwner,
            [MarshalAs(UnmanagedType.LPStruct)] Guid riidDevice,
            [MarshalAs(UnmanagedType.LPStruct)] Guid riid,
            [MarshalAs(UnmanagedType.IUnknown)] out object ppVideoMixer
            );

        [DllImport("evr.dll", ExactSpelling = true), SuppressUnmanagedCodeSecurity]
        public static extern int MFCreateVideoMixerAndPresenter(
            [In, MarshalAs(UnmanagedType.IUnknown)] object pMixerOwner,
            [In, MarshalAs(UnmanagedType.IUnknown)] object pPresenterOwner,
            [MarshalAs(UnmanagedType.LPStruct)] Guid riidMixer,
            out IntPtr ppvVideoMixer,
            [MarshalAs(UnmanagedType.LPStruct)] Guid riidPresenter,
            out IntPtr ppvVideoPresenter
            );

        [DllImport("mf.dll", ExactSpelling = true), SuppressUnmanagedCodeSecurity]
        public static extern int MFCreateASFStreamingMediaSink(
            IMFByteStream pIByteStream,
            out IMFMediaSink ppIMediaSink
        );

        [DllImport("mf.dll", ExactSpelling = true), SuppressUnmanagedCodeSecurity]
        public static extern int MFCreateASFStreamingMediaSinkActivate(
            IMFActivate pByteStreamActivate,
            IMFASFContentInfo pContentInfo,
            out IMFActivate ppIActivate
        );

        [DllImport("mfplat.dll", ExactSpelling = true), SuppressUnmanagedCodeSecurity]
        public static extern int MFCreateTransformActivate(
            out IMFActivate ppActivate
        );

        [DllImport("mf.dll", ExactSpelling = true), SuppressUnmanagedCodeSecurity]
        public static extern int MFCreateDeviceSource(
            IMFAttributes pAttributes,
            out IMFMediaSource ppSource
        );

        [DllImport("mf.dll", ExactSpelling = true), SuppressUnmanagedCodeSecurity]
        public static extern int MFCreateDeviceSourceActivate(
            IMFAttributes pAttributes,
            out IMFActivate ppActivate
        );

        [DllImport("mf.dll", ExactSpelling = true), SuppressUnmanagedCodeSecurity]
        public static extern int MFCreateSampleCopierMFT(out IMFTransform ppCopierMFT);

        [DllImport("mf.dll", ExactSpelling = true), SuppressUnmanagedCodeSecurity]
        public static extern int MFCreateTranscodeProfile(
            out IMFTranscodeProfile ppTranscodeProfile
        );

        [DllImport("mf.dll", ExactSpelling = true), SuppressUnmanagedCodeSecurity]
        public static extern int MFCreateTranscodeTopology(
            IMFMediaSource pSrc,
            [MarshalAs(UnmanagedType.LPWStr)] string pwszOutputFilePath,
            IMFTranscodeProfile pProfile,
            out IMFTopology ppTranscodeTopo
        );

        [DllImport("mf.dll", ExactSpelling = true), SuppressUnmanagedCodeSecurity]
        public static extern int MFTranscodeGetAudioOutputAvailableTypes(
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid guidSubType,
            int dwMFTFlags,
            IMFAttributes pCodecConfig,
            out IMFCollection ppAvailableTypes
        );

        [DllImport("mf.dll", ExactSpelling = true), SuppressUnmanagedCodeSecurity]
        public static extern int MFCreateTranscodeSinkActivate(
            out IMFActivate ppActivate
        );

        [DllImport("mfplat.dll", ExactSpelling = true), SuppressUnmanagedCodeSecurity]
        public static extern int MFCreateMFByteStreamOnStream(
            IStream pStream,
            out IMFByteStream ppByteStream
        );

        [DllImport("mf.dll", ExactSpelling = true), SuppressUnmanagedCodeSecurity]
        public static extern int MFCreateAggregateSource(
            IMFCollection pSourceCollection,
            out IMFMediaSource ppAggSource
            );

        [DllImport("mf.dll", ExactSpelling = true), SuppressUnmanagedCodeSecurity]
        public static extern int MFGetTopoNodeCurrentType(
            IMFTopologyNode pNode,
            int dwStreamIndex,
            [MarshalAs(UnmanagedType.Bool)] bool fOutput,
            out IMFMediaType ppType);

        [DllImport("mf.dll", ExactSpelling = true), SuppressUnmanagedCodeSecurity]
        public static extern int MFCreateMPEG4MediaSink(
            IMFByteStream pIByteStream,
            IMFMediaType pVideoMediaType,
            IMFMediaType pAudioMediaType,
            out IMFMediaSink ppIMediaSink
            );

        [DllImport("mf.dll", ExactSpelling = true), SuppressUnmanagedCodeSecurity]
        public static extern int MFCreate3GPMediaSink(
            IMFByteStream pIByteStream,
            IMFMediaType pVideoMediaType,
            IMFMediaType pAudioMediaType,
            out IMFMediaSink ppIMediaSink
            );

        [DllImport("mf.dll", ExactSpelling = true), SuppressUnmanagedCodeSecurity]
        public static extern int MFCreateMP3MediaSink(
            IMFByteStream pTargetByteStream,
            out IMFMediaSink ppMediaSink
        );

        [DllImport("mfplat.dll", ExactSpelling = true), SuppressUnmanagedCodeSecurity]
        public static extern int MFCreateVideoMediaTypeFromBitMapInfoHeaderEx(
            BitmapInfoHeader[] pbmihBitMapInfoHeader,
            int cbBitMapInfoHeader,
            int dwPixelAspectRatioX,
            int dwPixelAspectRatioY,
            MFVideoInterlaceMode InterlaceMode,
            long VideoFlags,
            int dwFramesPerSecondNumerator,
            int dwFramesPerSecondDenominator,
            int dwMaxBitRate,
            out IMFVideoMediaType ppIVideoMediaType
        );

        [DllImport("mfplat.dll", ExactSpelling = true), SuppressUnmanagedCodeSecurity]
        public static extern int MFGetPluginControl(
            out IMFPluginControl ppPluginControl
        );

        [DllImport("mfplat.dll", ExactSpelling = true), SuppressUnmanagedCodeSecurity]
        public static extern int MFGetMFTMerit(
            [MarshalAs(UnmanagedType.IUnknown)] object pMFT,
            int cbVerifier,
            IntPtr verifier,
            out int merit
        );

        [DllImport("mfplat.dll", ExactSpelling = true), SuppressUnmanagedCodeSecurity]
        public static extern int MFTEnumEx(
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid guidCategory,
            int Flags,
            MFTRegisterTypeInfo pInputType,
            MFTRegisterTypeInfo pOutputType,
            out IMFActivate[] pppMFTActivate,
            out int pnumMFTActivate
        );

        [DllImport("mfplat.dll", ExactSpelling = true), SuppressUnmanagedCodeSecurity]
        public static extern int MFAllocateWorkQueueEx(
            MFASYNC_WORKQUEUE_TYPE WorkQueueType,
            out int pdwWorkQueue
        );

        [DllImport("mfplat.dll", ExactSpelling = true), SuppressUnmanagedCodeSecurity]
        public static extern int MFTRegisterLocal(
            [MarshalAs(UnmanagedType.IUnknown)] object pClassFactory,
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid guidCategory,
            [MarshalAs(UnmanagedType.LPWStr)] string pszName,
            int Flags,
            int cInputTypes,
            MFTRegisterTypeInfo[] pInputTypes,
            int cOutputTypes,
            MFTRegisterTypeInfo[] pOutputTypes
        );

        [DllImport("mfplat.dll", ExactSpelling = true), SuppressUnmanagedCodeSecurity]
        public static extern int MFTUnregisterLocal(
            [MarshalAs(UnmanagedType.IUnknown)] object pClassFactory
        );

        [DllImport("mfplat.dll", ExactSpelling = true), SuppressUnmanagedCodeSecurity]
        public static extern int MFTRegisterLocalByCLSID(
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid clisdMFT,
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid guidCategory,
            [MarshalAs(UnmanagedType.LPWStr)] string pszName,
            MFT_EnumFlag Flags,
            int cInputTypes,
            MFTRegisterTypeInfo[] pInputTypes,
            int cOutputTypes,
            MFTRegisterTypeInfo[] pOutputTypes
        );

        [DllImport("mfplat.dll", ExactSpelling = true), SuppressUnmanagedCodeSecurity]
        public static extern int MFTUnregisterLocalByCLSID(
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid clsidMFT
        );

        #region Untestable

        [DllImport("mfplat.dll", ExactSpelling = true), Obsolete("This function is deprecated"), SuppressUnmanagedCodeSecurity]
        public static extern int MFCreateAudioMediaType(
            [In] WaveFormatEx pAudioFormat,
            out IMFAudioMediaType ppIAudioMediaType
            );

        [DllImport("mf.dll", ExactSpelling = true), Obsolete("The returned object doesn't support QI"), SuppressUnmanagedCodeSecurity]
        public static extern int MFCreateCredentialCache(
            out IMFNetCredentialCache ppCache
        );

        [DllImport("evr.dll", ExactSpelling = true), Obsolete("Not implemented"), SuppressUnmanagedCodeSecurity]
        public static extern int MFCreateVideoMediaTypeFromBitMapInfoHeader(
            [In, MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(BMMarshaler))] BitmapInfoHeader pbmihBitMapInfoHeader,
            int dwPixelAspectRatioX,
            int dwPixelAspectRatioY,
            MFVideoInterlaceMode InterlaceMode,
            long VideoFlags,
            long qwFramesPerSecondNumerator,
            long qwFramesPerSecondDenominator,
            int dwMaxBitRate,
            out IMFVideoMediaType ppIVideoMediaType
            );

        [DllImport("mf.dll", ExactSpelling = true), Obsolete("Interface doesn't exist"), SuppressUnmanagedCodeSecurity]
        public static extern int MFCreateQualityManager(
            out IMFQualityManager ppQualityManager
        );

        [DllImport("evr.dll", ExactSpelling = true), Obsolete("Undoc'ed"), SuppressUnmanagedCodeSecurity]
        public static extern int MFCreateVideoMediaTypeFromVideoInfoHeader(
            VideoInfoHeader pVideoInfoHeader,
            int cbVideoInfoHeader,
            int dwPixelAspectRatioX,
            int dwPixelAspectRatioY,
            MFVideoInterlaceMode InterlaceMode,
            long VideoFlags,
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid pSubtype,
            out IMFVideoMediaType ppIVideoMediaType
            );

        [DllImport("evr.dll", ExactSpelling = true), Obsolete("Undoc'ed"), SuppressUnmanagedCodeSecurity]
        public static extern int MFCreateVideoMediaTypeFromVideoInfoHeader2(
            VideoInfoHeader2 pVideoInfoHeader,
            int cbVideoInfoHeader,
            long AdditionalVideoFlags,
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid pSubtype,
            out IMFVideoMediaType ppIVideoMediaType
            );

        #endregion
#endif
    }
}
