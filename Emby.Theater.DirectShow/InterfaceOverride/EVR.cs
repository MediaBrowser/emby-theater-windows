using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Emby.Theater.DirectShow.InterfaceOverride
{
    [ComImport, System.Security.SuppressUnmanagedCodeSecurity,
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown),
    Guid("6AB0000C-FECE-4d1f-A2AC-A9573530656E")]
    public interface IMFVideoProcessor
    {
        int GetAvailableVideoProcessorModes(
            out int lpdwNumProcessingModes,
            out IntPtr ppVideoProcessingModes);

        int GetVideoProcessorCaps(
            [In] Guid lpVideoProcessorMode,
            out MediaFoundation.EVR.DXVA2VideoProcessorCaps lpVideoProcessorCaps);

        int GetVideoProcessorMode(
            out Guid lpMode);

        int SetVideoProcessorMode(
            [In] ref Guid lpMode);

        int GetProcAmpRange(
            int dwProperty,
            out MediaFoundation.EVR.DXVA2ValueRange pPropRange);

        int GetProcAmpValues(
            MediaFoundation.EVR.DXVA2ProcAmp dwFlags,
            out MediaFoundation.EVR.DXVA2ProcAmpValues Values);

        int SetProcAmpValues(
            MediaFoundation.EVR.DXVA2ProcAmp dwFlags,
            [In] MediaFoundation.EVR.DXVA2ProcAmpValues pValues);

        int GetFilteringRange(
            MediaFoundation.EVR.DXVA2Filters dwProperty,
            out MediaFoundation.EVR.DXVA2ValueRange pPropRange);

        int GetFilteringValue(
            MediaFoundation.EVR.DXVA2Filters dwProperty,
            out int pValue);

        int SetFilteringValue(
            MediaFoundation.EVR.DXVA2Filters dwProperty,
            [In] int pValue);

        int GetBackgroundColor(
            out int lpClrBkg);

        int SetBackgroundColor(
            int ClrBkg);
    }
}
