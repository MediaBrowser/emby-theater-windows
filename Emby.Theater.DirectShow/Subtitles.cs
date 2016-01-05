using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using DirectShowLib;
using MediaFoundation.Misc;

namespace Emby.Theater.DirectShow
{
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    public class LOGFONT
    {
        public int lfHeight;
        public int lfWidth;
        public int lfEscapement;
        public int lfOrientation;
        public int lfWeight;
        public byte lfItalic;
        public byte lfUnderline;
        public byte lfStrikeOut;
        public byte lfCharSet;
        public byte lfOutPrecision;
        public byte lfClipPrecision;
        public byte lfQuality;
        public byte lfPitchAndFamily;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
        public string lfFaceName = string.Empty;
    }

    [StructLayoutAttribute(LayoutKind.Sequential)]
    public struct NORMALIZEDRECT
    {
        public float left;
        public float top;
        public float right;
        public float bottom;
    }

    [ComVisible(true), ComImport, SuppressUnmanagedCodeSecurity,
Guid("EBE1FB08-3957-47ca-AF13-5827E5442E56"),
InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IDirectVobSub : IBaseFilter
    {
        [PreserveSig]
        int get_FileName(/*[MarshalAs(UnmanagedType.LPWStr, SizeConst = 260)]ref StringBuilder*/IntPtr fn);

        [PreserveSig]
        int put_FileName([MarshalAs(UnmanagedType.LPWStr)]string fn);

        [PreserveSig]
        int get_LanguageCount(out int nLangs);

        [PreserveSig]
        int get_LanguageName(int iLanguage, [MarshalAs(UnmanagedType.LPWStr)] out string ppName);
        [PreserveSig]
        int get_SelectedLanguage(ref int iSelected);

        [PreserveSig]
        int put_SelectedLanguage(int iSelected);

        [PreserveSig]
        int get_HideSubtitles(ref bool fHideSubtitles);

        [PreserveSig]
        int put_HideSubtitles([MarshalAsAttribute(UnmanagedType.I1)] bool fHideSubtitles);

        [PreserveSig]
        int get_PreBuffering(ref bool fDoPreBuffering);

        [PreserveSig]
        int put_PreBuffering([MarshalAsAttribute(UnmanagedType.I1)] bool fDoPreBuffering);

        [PreserveSig]
        int get_Placement(ref bool fOverridePlacement, ref int xperc, ref int yperc);

        [PreserveSig]
        int put_Placement([MarshalAsAttribute(UnmanagedType.I1)] bool fOverridePlacement, int xperc, int yperc);

        [PreserveSig]
        int get_VobSubSettings(ref bool fBuffer, ref bool fOnlyShowForcedSubs, ref bool fPolygonize);

        [PreserveSig]
        int put_VobSubSettings([MarshalAsAttribute(UnmanagedType.I1)] bool fBuffer, [MarshalAsAttribute(UnmanagedType.I1)] bool fOnlyShowForcedSubs, [MarshalAsAttribute(UnmanagedType.I1)] bool fPolygonize);

        [PreserveSig]
        int get_TextSettings(LOGFONT lf, int lflen, ref uint color, ref bool fShadow, ref bool fOutline, ref bool fAdvancedRenderer);

        [PreserveSig]
        int put_TextSettings(LOGFONT lf, int lflen, uint color, bool fShadow, bool fOutline, bool fAdvancedRenderer);

        [PreserveSig]
        int get_Flip(ref bool fPicture, ref bool fSubtitles);
        [PreserveSig]
        int put_Flip([MarshalAsAttribute(UnmanagedType.I1)] bool fPicture, [MarshalAsAttribute(UnmanagedType.I1)] bool fSubtitles);

        [PreserveSig]
        int get_OSD(ref bool fOSD);

        [PreserveSig]
        int put_OSD([MarshalAsAttribute(UnmanagedType.I1)] bool fOSD);

    }

    [ComVisible(true), ComImport, SuppressUnmanagedCodeSecurity,
    Guid("85E5D6F9-BEFB-4E01-B047-758359CDF9AB"),
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IDirectVobSubXy : IBaseFilter
    {
        [PreserveSig]
        int XyGetBool(int field, ref bool value);
        [PreserveSig]
        int XyGetInt(int field, ref int value);
        [PreserveSig]
        int XyGetSize(int field, ref MFSize value);
        [PreserveSig]
        int XyGetRect(int field, ref MFRect value);
        [PreserveSig]
        int XyGetUlonglong(int field, ref long value);
        [PreserveSig]
        int XyGetDouble(int field, ref double value);
        [PreserveSig]
        int XyGetString(int field, ref string value, ref int chars);
        [PreserveSig]
        int XyGetBin(int field, ref IntPtr value, ref int size);

        [PreserveSig]
        int XySetBool(int field, bool value);
        [PreserveSig]
        int XySetInt(int field, int value);
        [PreserveSig]
        int XySetSize(int field, MFSize value);
        [PreserveSig]
        int XySetRect(int field, MFRect value);
        [PreserveSig]
        int XySetUlonglong(int field, long value);
        [PreserveSig]
        int XySetDouble(int field, double value);
        [PreserveSig]
        int XySetString(int field, string value, int chars);
        [PreserveSig]
        int XySetBin(int field, IntPtr value, int size);

    }

    [ComVisible(true), ComImport, SuppressUnmanagedCodeSecurity,
    Guid("AB52FC9C-2415-4dca-BC1C-8DCC2EAE8151"),
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IDirectVobSub3 : IBaseFilter
    {
        [PreserveSig]
        int OpenSubtitles([MarshalAs(UnmanagedType.LPWStr)]string fn);

        [PreserveSig]
        int SkipAutoloadCheck(int boolval);
    }
}
