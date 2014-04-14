using System;
using System.Runtime.InteropServices;
using System.Security;
using MediaFoundation.EVR;

namespace MediaBrowser.Theater.DirectShow.InterfaceOverride
{
    [ComImport, SuppressUnmanagedCodeSecurity,
     InterfaceType(ComInterfaceType.InterfaceIsIUnknown),
     Guid("6AB0000C-FECE-4d1f-A2AC-A9573530656E")]
    public interface IMFVideoProcessor
    {
        int GetAvailableVideoProcessorModes(
            out int lpdwNumProcessingModes,
            out IntPtr ppVideoProcessingModes);

        int GetVideoProcessorCaps(
            [In] Guid lpVideoProcessorMode,
            out DXVA2VideoProcessorCaps lpVideoProcessorCaps);

        int GetVideoProcessorMode(
            out Guid lpMode);

        int SetVideoProcessorMode(
            [In] ref Guid lpMode);

        int GetProcAmpRange(
            int dwProperty,
            out DXVA2ValueRange pPropRange);

        int GetProcAmpValues(
            DXVA2ProcAmp dwFlags,
            out DXVA2ProcAmpValues Values);

        int SetProcAmpValues(
            DXVA2ProcAmp dwFlags,
            [In] DXVA2ProcAmpValues pValues);

        int GetFilteringRange(
            DXVA2Filters dwProperty,
            out DXVA2ValueRange pPropRange);

        int GetFilteringValue(
            DXVA2Filters dwProperty,
            out int pValue);

        int SetFilteringValue(
            DXVA2Filters dwProperty,
            [In] int pValue);

        int GetBackgroundColor(
            out int lpClrBkg);

        int SetBackgroundColor(
            int ClrBkg);
    }
}