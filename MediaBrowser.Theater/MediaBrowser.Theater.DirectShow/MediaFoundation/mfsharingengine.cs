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

namespace MediaFoundation
{
#if ALLOW_UNTESTED_INTERFACES

    #region Declarations

    [UnmanagedName("MF_SHARING_ENGINE_EVENT")]
    public enum MF_SHARING_ENGINE_EVENT
    {
        Disconnect = 2000,
        LocalRenderingStarted = 2001,
        LocalRenderingEnded = 2002
    }

    [UnmanagedName("MF_MEDIA_SHARING_ENGINE_EVENT")]
    public enum MF_MEDIA_SHARING_ENGINE_EVENT
    {
        Disconnect = 2000
    }

    [Flags, UnmanagedName("PLAYTO_SOURCE_CREATEFLAGS")]
    public enum PLAYTO_SOURCE_CREATEFLAGS
    {
        None = 0x0,
        Image = 0x1,
        Audio = 0x2,
        Video = 0x4
    }

    [StructLayout(LayoutKind.Sequential), UnmanagedName("DEVICE_INFO")]
    public struct DEVICE_INFO
    {
        [MarshalAs(UnmanagedType.BStr)]
        string pFriendlyDeviceName;

        [MarshalAs(UnmanagedType.BStr)]
        string pUniqueDeviceName;

        [MarshalAs(UnmanagedType.BStr)]
        string pManufacturerName;

        [MarshalAs(UnmanagedType.BStr)]
        string pModelName;

        [MarshalAs(UnmanagedType.BStr)]
        string pIconURL;
    }

    #endregion

    #region Interfaces

    [ComImport, System.Security.SuppressUnmanagedCodeSecurity,
    Guid("2BA61F92-8305-413B-9733-FAF15F259384"),
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IMFSharingEngineClassFactory
    {
        [PreserveSig]
        int CreateInstance(
            MF_MEDIA_ENGINE_CREATEFLAGS dwFlags,
            IMFAttributes pAttr,
            [MarshalAs(UnmanagedType.IUnknown)] out object ppEngine
            );
    }

    [ComImport, System.Security.SuppressUnmanagedCodeSecurity,
    Guid("8D3CE1BF-2367-40E0-9EEE-40D377CC1B46"),
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IMFMediaSharingEngine : IMFMediaEngine
    {
        [PreserveSig]
        int GetDevice(
            out DEVICE_INFO pDevice
            );
    }

    [ComImport, System.Security.SuppressUnmanagedCodeSecurity,
    Guid("524D2BC4-B2B1-4FE5-8FAC-FA4E4512B4E0"),
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IMFMediaSharingEngineClassFactory
    {
        [PreserveSig]
        int CreateInstance(
            int dwFlags,
            IMFAttributes pAttr,
            out IMFMediaSharingEngine ppEngine
            );
    }

    [ComImport, System.Security.SuppressUnmanagedCodeSecurity,
    Guid("CFA0AE8E-7E1C-44D2-AE68-FC4C148A6354"),
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IMFImageSharingEngine
    {
        [PreserveSig]
        int SetSource(
            [MarshalAs(UnmanagedType.IUnknown)] object pStream
            );

        [PreserveSig]
        int GetDevice(
            out DEVICE_INFO pDevice
            );

        [PreserveSig]
        int Shutdown();
    }

    [ComImport, System.Security.SuppressUnmanagedCodeSecurity,
    Guid("1FC55727-A7FB-4FC8-83AE-8AF024990AF1"),
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IMFImageSharingEngineClassFactory
    {
        [PreserveSig]
        int CreateInstanceFromUDN(
            [MarshalAs(UnmanagedType.BStr)]
            string pUniqueDeviceName,
            out IMFImageSharingEngine ppEngine
            );
    }

    [ComImport, System.Security.SuppressUnmanagedCodeSecurity,
    Guid("607574EB-F4B6-45C1-B08C-CB715122901D"),
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IPlayToControl
    {
        [PreserveSig]
        int Connect(
            IMFSharingEngineClassFactory pFactory
            );

        [PreserveSig]
        int Disconnect();
    }

    [ComImport, System.Security.SuppressUnmanagedCodeSecurity,
    Guid("842B32A3-9B9B-4D1C-B3F3-49193248A554"),
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IPlayToSourceClassFactory
    {
        [PreserveSig]
        int CreateInstance(
            PLAYTO_SOURCE_CREATEFLAGS dwFlags,
            IPlayToControl pControl,
            [MarshalAs(UnmanagedType.IUnknown)] out object ppSource
            );
    }

    #endregion

#endif
}
