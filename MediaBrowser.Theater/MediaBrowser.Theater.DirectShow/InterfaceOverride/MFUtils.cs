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
using System.Text;
using System.Drawing;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Security;

using MediaFoundation.Misc;
using MediaFoundation.Transform;

namespace MediaFoundation.Misc
{
    #region Wrapper classes

    [StructLayout(LayoutKind.Sequential)]
    public class MfFloat
    {
        private float Value;

        public MfFloat(float v)
        {
            Value = v;
        }

        public override string ToString()
        {
            return Value.ToString();
        }

        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }

        public static implicit operator float(MfFloat l)
        {
            return l.Value;
        }

        public static implicit operator MfFloat(float l)
        {
            return new MfFloat(l);
        }
    }

    /// <summary>
    /// ConstPropVariant is used for [In] parameters.  This is important since
    /// for [In] parameters, you must *not* clear the PropVariant.  The caller
    /// will need to do that himself.
    ///
    /// Likewise, if you want to store a copy of a ConstPropVariant, you should
    /// store it to a PropVariant using the PropVariant constructor that takes a
    /// ConstPropVariant.  If you try to store the ConstPropVariant, when the
    /// caller frees his copy, yours will no longer be valid.
    /// </summary>
    [StructLayout(LayoutKind.Explicit)]
    public class ConstPropVariant : IDisposable
    {
        public enum VariantType : short
        {
            None = 0,
            Short = 2,
            Int32 = 3,
            Float = 4,
            Double = 5,
            IUnknown = 13,
            UByte = 17,
            UShort = 18,
            UInt32 = 19,
            Int64 = 20,
            UInt64 = 21,
            String = 31,
            Guid = 72,
            Blob = 0x1000 + 17,
            StringArray = 0x1000 + 31
        }

        [StructLayout(LayoutKind.Sequential), UnmanagedName("BLOB")]
        protected struct Blob
        {
            public int cbSize;
            public IntPtr pBlobData;
        }

        [StructLayout(LayoutKind.Sequential), UnmanagedName("CALPWSTR")]
        protected struct CALPWstr
        {
            public int cElems;
            public IntPtr pElems;
        }

        #region Member variables

        [FieldOffset(0)]
        protected VariantType type;

        [FieldOffset(2)]
        protected short reserved1;

        [FieldOffset(4)]
        protected short reserved2;

        [FieldOffset(6)]
        protected short reserved3;

        [FieldOffset(8)]
        protected short iVal;

        [FieldOffset(8), CLSCompliant(false)]
        protected ushort uiVal;

        [FieldOffset(8), CLSCompliant(false)]
        protected byte bVal;

        [FieldOffset(8)]
        protected int intValue;

        [FieldOffset(8), CLSCompliant(false)]
        protected uint uintVal;

        [FieldOffset(8)]
        protected float fltVal;

        [FieldOffset(8)]
        protected long longValue;

        [FieldOffset(8), CLSCompliant(false)]
        protected ulong ulongValue;

        [FieldOffset(8)]
        protected double doubleValue;

        [FieldOffset(8)]
        protected Blob blobValue;

        [FieldOffset(8)]
        protected IntPtr ptr;

        [FieldOffset(8)]
        protected CALPWstr calpwstrVal;

        #endregion

        public ConstPropVariant()
        {
            type = VariantType.None;
        }

        protected ConstPropVariant(VariantType v)
        {
            type = v;
        }

        public static explicit operator string(ConstPropVariant f)
        {
            return f.GetString();
        }

        public static explicit operator string[](ConstPropVariant f)
        {
            return f.GetStringArray();
        }

        public static explicit operator byte(ConstPropVariant f)
        {
            return f.GetUByte();
        }

        public static explicit operator short(ConstPropVariant f)
        {
            return f.GetShort();
        }

        [CLSCompliant(false)]
        public static explicit operator ushort(ConstPropVariant f)
        {
            return f.GetUShort();
        }

        public static explicit operator int(ConstPropVariant f)
        {
            return f.GetInt();
        }

        [CLSCompliant(false)]
        public static explicit operator uint(ConstPropVariant f)
        {
            return f.GetUInt();
        }

        public static explicit operator float(ConstPropVariant f)
        {
            return f.GetFloat();
        }

        public static explicit operator double(ConstPropVariant f)
        {
            return f.GetDouble();
        }

        public static explicit operator long(ConstPropVariant f)
        {
            return f.GetLong();
        }

        [CLSCompliant(false)]
        public static explicit operator ulong(ConstPropVariant f)
        {
            return f.GetULong();
        }

        public static explicit operator Guid(ConstPropVariant f)
        {
            return f.GetGuid();
        }

        public static explicit operator byte[](ConstPropVariant f)
        {
            return f.GetBlob();
        }

        // I decided not to do implicits since perf is likely to be
        // better recycling the PropVariant, and the only way I can
        // see to support Implicit is to create a new PropVariant.
        // Also, since I can't free the previous instance, IUnknowns
        // will linger until the GC cleans up.  Not what I think I
        // want.

        public MFAttributeType GetMFAttributeType()
        {
            switch (type)
            {
                case VariantType.None:
                case VariantType.UInt32:
                case VariantType.UInt64:
                case VariantType.Double:
                case VariantType.Guid:
                case VariantType.String:
                case VariantType.Blob:
                case VariantType.IUnknown:
                    {
                        return (MFAttributeType)type;
                    }
                default:
                    {
                        throw new Exception("Type is not a MFAttributeType");
                    }
            }
        }

        public VariantType GetVariantType()
        {
            return type;
        }

        public string[] GetStringArray()
        {
            if (type == VariantType.StringArray)
            {
                string[] sa;

                int iCount = calpwstrVal.cElems;
                sa = new string[iCount];

                for (int x = 0; x < iCount; x++)
                {
                    sa[x] = Marshal.PtrToStringUni(Marshal.ReadIntPtr(calpwstrVal.pElems, x * IntPtr.Size));
                }

                return sa;
            }
            throw new ArgumentException("PropVariant contents not a string array");
        }

        public string GetString()
        {
            if (type == VariantType.String)
            {
                return Marshal.PtrToStringUni(ptr);
            }
            throw new ArgumentException("PropVariant contents not a string");
        }

        public byte GetUByte()
        {
            if (type == VariantType.UByte)
            {
                return bVal;
            }
            throw new ArgumentException("PropVariant contents not a byte");
        }

        public short GetShort()
        {
            if (type == VariantType.Short)
            {
                return iVal;
            }
            throw new ArgumentException("PropVariant contents not an Short");
        }

        [CLSCompliant(false)]
        public ushort GetUShort()
        {
            if (type == VariantType.UShort)
            {
                return uiVal;
            }
            throw new ArgumentException("PropVariant contents not an UShort");
        }

        public int GetInt()
        {
            if (type == VariantType.Int32)
            {
                return intValue;
            }
            throw new ArgumentException("PropVariant contents not an int32");
        }

        [CLSCompliant(false)]
        public uint GetUInt()
        {
            if (type == VariantType.UInt32)
            {
                return uintVal;
            }
            throw new ArgumentException("PropVariant contents not an uint32");
        }

        public long GetLong()
        {
            if (type == VariantType.Int64)
            {
                return longValue;
            }
            throw new ArgumentException("PropVariant contents not an int64");
        }

        [CLSCompliant(false)]
        public ulong GetULong()
        {
            if (type == VariantType.UInt64)
            {
                return ulongValue;
            }
            throw new ArgumentException("PropVariant contents not an uint64");
        }

        public float GetFloat()
        {
            if (type == VariantType.Float)
            {
                return fltVal;
            }
            throw new ArgumentException("PropVariant contents not a Float");
        }

        public double GetDouble()
        {
            if (type == VariantType.Double)
            {
                return doubleValue;
            }
            throw new ArgumentException("PropVariant contents not a double");
        }

        public Guid GetGuid()
        {
            if (type == VariantType.Guid)
            {
                return (Guid)Marshal.PtrToStructure(ptr, typeof(Guid));
            }
            throw new ArgumentException("PropVariant contents not a Guid");
        }

        public byte[] GetBlob()
        {
            if (type == VariantType.Blob)
            {
                byte[] b = new byte[blobValue.cbSize];

                Marshal.Copy(blobValue.pBlobData, b, 0, blobValue.cbSize);

                return b;
            }
            throw new ArgumentException("PropVariant contents are not a Blob");
        }

        public object GetIUnknown()
        {
            if (type == VariantType.IUnknown)
            {
                return Marshal.GetObjectForIUnknown(ptr);
            }
            throw new ArgumentException("PropVariant contents not an IUnknown");
        }

        public override string ToString()
        {
            // This method is primarily intended for debugging so that a readable string will show
            // up in the output window
            string sRet;

            switch (type)
            {
                case VariantType.None:
                    {
                        sRet = "<Empty>";
                        break;
                    }

                case VariantType.Blob:
                    {
                        const string FormatString = "x2"; // Hex 2 digit format
                        const int MaxEntries = 16;

                        byte[] blob = GetBlob();

                        // Number of bytes we're going to format
                        int n = Math.Min(MaxEntries, blob.Length);

                        if (n == 0)
                        {
                            sRet = "<Empty Array>";
                        }
                        else
                        {
                            // Only format the first MaxEntries bytes
                            sRet = blob[0].ToString(FormatString);
                            for (int i = 1; i < n; i++)
                            {
                                sRet += ',' + blob[i].ToString(FormatString);
                            }

                            // If the string is longer, add an indicator
                            if (blob.Length > n)
                            {
                                sRet += "...";
                            }
                        }
                        break;
                    }

                case VariantType.Float:
                    {
                        sRet = GetFloat().ToString();
                        break;
                    }

                case VariantType.Double:
                    {
                        sRet = GetDouble().ToString();
                        break;
                    }

                case VariantType.Guid:
                    {
                        sRet = GetGuid().ToString();
                        break;
                    }

                case VariantType.IUnknown:
                    {
                        sRet = GetIUnknown().ToString();
                        break;
                    }

                case VariantType.String:
                    {
                        sRet = GetString();
                        break;
                    }

                case VariantType.Short:
                    {
                        sRet = GetShort().ToString();
                        break;
                    }

                case VariantType.UByte:
                    {
                        sRet = GetUByte().ToString();
                        break;
                    }

                case VariantType.UShort:
                    {
                        sRet = GetUShort().ToString();
                        break;
                    }

                case VariantType.Int32:
                    {
                        sRet = GetInt().ToString();
                        break;
                    }

                case VariantType.UInt32:
                    {
                        sRet = GetUInt().ToString();
                        break;
                    }

                case VariantType.Int64:
                    {
                        sRet = GetLong().ToString();
                        break;
                    }

                case VariantType.UInt64:
                    {
                        sRet = GetULong().ToString();
                        break;
                    }

                case VariantType.StringArray:
                    {
                        sRet = "";
                        foreach (string entry in GetStringArray())
                        {
                            sRet += (sRet.Length == 0 ? "\"" : ",\"") + entry + '\"';
                        }
                        break;
                    }
                default:
                    {
                        sRet = base.ToString();
                        break;
                    }
            }

            return sRet;
        }

        public override int GetHashCode()
        {
            // Give a (slightly) better hash value in case someone uses PropVariants
            // in a hash table.
            int iRet;

            switch (type)
            {
                case VariantType.None:
                    {
                        iRet = base.GetHashCode();
                        break;
                    }

                case VariantType.Blob:
                    {
                        iRet = GetBlob().GetHashCode();
                        break;
                    }

                case VariantType.Float:
                    {
                        iRet = GetFloat().GetHashCode();
                        break;
                    }

                case VariantType.Double:
                    {
                        iRet = GetDouble().GetHashCode();
                        break;
                    }

                case VariantType.Guid:
                    {
                        iRet = GetGuid().GetHashCode();
                        break;
                    }

                case VariantType.IUnknown:
                    {
                        iRet = GetIUnknown().GetHashCode();
                        break;
                    }

                case VariantType.String:
                    {
                        iRet = GetString().GetHashCode();
                        break;
                    }

                case VariantType.UByte:
                    {
                        iRet = GetUByte().GetHashCode();
                        break;
                    }

                case VariantType.Short:
                    {
                        iRet = GetShort().GetHashCode();
                        break;
                    }

                case VariantType.UShort:
                    {
                        iRet = GetUShort().GetHashCode();
                        break;
                    }

                case VariantType.Int32:
                    {
                        iRet = GetInt().GetHashCode();
                        break;
                    }

                case VariantType.UInt32:
                    {
                        iRet = GetUInt().GetHashCode();
                        break;
                    }

                case VariantType.Int64:
                    {
                        iRet = GetLong().GetHashCode();
                        break;
                    }

                case VariantType.UInt64:
                    {
                        iRet = GetULong().GetHashCode();
                        break;
                    }

                case VariantType.StringArray:
                    {
                        iRet = GetStringArray().GetHashCode();
                        break;
                    }
                default:
                    {
                        iRet = base.GetHashCode();
                        break;
                    }
            }

            return iRet;
        }

        public override bool Equals(object obj)
        {
            bool bRet;
            PropVariant p = obj as PropVariant;

            if ((((object)p) == null) || (p.type != type))
            {
                bRet = false;
            }
            else
            {
                switch (type)
                {
                    case VariantType.None:
                        {
                            bRet = true;
                            break;
                        }

                    case VariantType.Blob:
                        {
                            byte[] b1;
                            byte[] b2;

                            b1 = GetBlob();
                            b2 = p.GetBlob();

                            if (b1.Length == b2.Length)
                            {
                                bRet = true;
                                for (int x = 0; x < b1.Length; x++)
                                {
                                    if (b1[x] != b2[x])
                                    {
                                        bRet = false;
                                        break;
                                    }
                                }
                            }
                            else
                            {
                                bRet = false;
                            }
                            break;
                        }

                    case VariantType.Float:
                        {
                            bRet = GetFloat() == p.GetFloat();
                            break;
                        }

                    case VariantType.Double:
                        {
                            bRet = GetDouble() == p.GetDouble();
                            break;
                        }

                    case VariantType.Guid:
                        {
                            bRet = GetGuid() == p.GetGuid();
                            break;
                        }

                    case VariantType.IUnknown:
                        {
                            bRet = GetIUnknown() == p.GetIUnknown();
                            break;
                        }

                    case VariantType.String:
                        {
                            bRet = GetString() == p.GetString();
                            break;
                        }

                    case VariantType.UByte:
                        {
                            bRet = GetUByte() == p.GetUByte();
                            break;
                        }

                    case VariantType.Short:
                        {
                            bRet = GetShort() == p.GetShort();
                            break;
                        }

                    case VariantType.UShort:
                        {
                            bRet = GetUShort() == p.GetUShort();
                            break;
                        }

                    case VariantType.Int32:
                        {
                            bRet = GetInt() == p.GetInt();
                            break;
                        }

                    case VariantType.UInt32:
                        {
                            bRet = GetUInt() == p.GetUInt();
                            break;
                        }

                    case VariantType.Int64:
                        {
                            bRet = GetLong() == p.GetLong();
                            break;
                        }

                    case VariantType.UInt64:
                        {
                            bRet = GetULong() == p.GetULong();
                            break;
                        }

                    case VariantType.StringArray:
                        {
                            string[] sa1;
                            string[] sa2;

                            sa1 = GetStringArray();
                            sa2 = p.GetStringArray();

                            if (sa1.Length == sa2.Length)
                            {
                                bRet = true;
                                for (int x = 0; x < sa1.Length; x++)
                                {
                                    if (sa1[x] != sa2[x])
                                    {
                                        bRet = false;
                                        break;
                                    }
                                }
                            }
                            else
                            {
                                bRet = false;
                            }
                            break;
                        }
                    default:
                        {
                            bRet = base.Equals(obj);
                            break;
                        }
                }
            }

            return bRet;
        }

        public static bool operator ==(ConstPropVariant pv1, ConstPropVariant pv2)
        {
            // If both are null, or both are same instance, return true.
            if (System.Object.ReferenceEquals(pv1, pv2))
            {
                return true;
            }

            // If one is null, but not both, return false.
            if (((object)pv1 == null) || ((object)pv2 == null))
            {
                return false;
            }

            return pv1.Equals(pv2);
        }

        public static bool operator !=(ConstPropVariant pv1, ConstPropVariant pv2)
        {
            return !(pv1 == pv2);
        }

        #region IDisposable Members

        public void Dispose()
        {
            // If we are a ConstPropVariant, we must *not* call PropVariantClear.  That
            // would release the *caller's* copy of the data, which would probably make
            // him cranky.  If we are a PropVariant, the PropVariant.Dispose gets called
            // as well, which *does* do a PropVariantClear.
            type = VariantType.None;
#if DEBUG
            longValue = 0;
#endif
        }

        #endregion
    }

    [StructLayout(LayoutKind.Explicit)]
    public class PropVariant : ConstPropVariant
    {
        #region Declarations

        [DllImport("ole32.dll", ExactSpelling = true, PreserveSig = false), SuppressUnmanagedCodeSecurity]
        protected static extern void PropVariantCopy(
            [Out, MarshalAs(UnmanagedType.LPStruct)] PropVariant pvarDest,
            [In, MarshalAs(UnmanagedType.LPStruct)] ConstPropVariant pvarSource
            );

        [DllImport("ole32.dll", ExactSpelling = true, PreserveSig = false), SuppressUnmanagedCodeSecurity]
        protected static extern void PropVariantClear(
            [In, MarshalAs(UnmanagedType.LPStruct)] PropVariant pvar
            );

        #endregion

        public PropVariant() : base(VariantType.None)
        {
        }

        public PropVariant(string value) : base(VariantType.String)
        {
            ptr = Marshal.StringToCoTaskMemUni(value);
        }

        public PropVariant(string[] value) : base(VariantType.StringArray)
        {
            calpwstrVal.cElems = value.Length;
            calpwstrVal.pElems = Marshal.AllocCoTaskMem(IntPtr.Size * value.Length);

            for (int x = 0; x < value.Length; x++)
            {
                Marshal.WriteIntPtr(calpwstrVal.pElems, x * IntPtr.Size, Marshal.StringToCoTaskMemUni(value[x]));
            }
        }

        public PropVariant(byte value) : base(VariantType.UByte)
        {
            bVal = value;
        }

        public PropVariant(short value) : base(VariantType.Short)
        {
            iVal = value;
        }

        [CLSCompliant(false)]
        public PropVariant(ushort value) : base(VariantType.UShort)
        {
            uiVal = value;
        }

        public PropVariant(int value) : base(VariantType.Int32)
        {
            intValue = value;
        }

        [CLSCompliant(false)]
        public PropVariant(uint value) : base(VariantType.UInt32)
        {
            uintVal = value;
        }

        public PropVariant(float value) : base(VariantType.Float)
        {
            fltVal = value;
        }

        public PropVariant(double value) : base(VariantType.Double)
        {
            doubleValue = value;
        }

        public PropVariant(long value) : base(VariantType.Int64)
        {
            longValue = value;
        }

        [CLSCompliant(false)]
        public PropVariant(ulong value) : base(VariantType.UInt64)
        {
            ulongValue = value;
        }

        public PropVariant(Guid value) : base(VariantType.Guid)
        {
            ptr = Marshal.AllocCoTaskMem(Marshal.SizeOf(value));
            Marshal.StructureToPtr(value, ptr, false);
        }

        public PropVariant(byte[] value) : base(VariantType.Blob)
        {
            blobValue.cbSize = value.Length;
            blobValue.pBlobData = Marshal.AllocCoTaskMem(value.Length);
            Marshal.Copy(value, 0, blobValue.pBlobData, value.Length);
        }

        public PropVariant(object value) : base(VariantType.IUnknown)
        {
            ptr = Marshal.GetIUnknownForObject(value);
        }

        public PropVariant(IntPtr value)
        {
            Marshal.PtrToStructure(value, this);
        }

        public PropVariant(ConstPropVariant value)
        {
            if (value != null)
            {
                PropVariantCopy(this, value);
            }
            else
            {
                throw new NullReferenceException("null passed to PropVariant constructor");
            }
        }

        ~PropVariant()
        {
            Clear();
        }

        public void Copy(PropVariant pval)
        {
            if (pval == null)
            {
                throw new Exception("Null PropVariant sent to Copy");
            }

            pval.Clear();

            PropVariantCopy(pval, this);
        }

        public void Clear()
        {
            PropVariantClear(this);
        }

        #region IDisposable Members

        new public void Dispose()
        {
            Clear();
            GC.SuppressFinalize(this);
        }

        #endregion
    }

    [StructLayout(LayoutKind.Sequential)]
    public class FourCC
    {
        private int m_fourCC;

        public FourCC(string fcc)
        {
            if (fcc.Length != 4)
            {
                throw new ArgumentException(fcc + " is not a valid FourCC");
            }

            byte[] asc = Encoding.ASCII.GetBytes(fcc);

            LoadFromBytes(asc[0], asc[1], asc[2], asc[3]);
        }

        public FourCC(char a, char b, char c, char d)
        {
            LoadFromBytes((byte)a, (byte)b, (byte)c, (byte)d);
        }

        public FourCC(int fcc)
        {
            m_fourCC = fcc;
        }

        public FourCC(byte a, byte b, byte c, byte d)
        {
            LoadFromBytes(a, b, c, d);
        }

        public FourCC(Guid g)
        {
            if (!IsA4ccSubtype(g))
            {
                throw new Exception("Not a FourCC Guid");
            }

            byte[] asc;
            asc = g.ToByteArray();

            LoadFromBytes(asc[0], asc[1], asc[2], asc[3]);
        }

        public void LoadFromBytes(byte a, byte b, byte c, byte d)
        {
            m_fourCC = a | (b << 8) | (c << 16) | (d << 24);
        }

        public int ToInt32()
        {
            return m_fourCC;
        }

        public static explicit operator int(FourCC f)
        {
            return f.ToInt32();
        }

        public static explicit operator Guid(FourCC f)
        {
            return f.ToMediaSubtype();
        }

        public Guid ToMediaSubtype()
        {
            return new Guid(m_fourCC, 0x0000, 0x0010, 0x80, 0x00, 0x00, 0xaa, 0x00, 0x38, 0x9b, 0x71);
        }

        public static bool operator ==(FourCC fcc1, FourCC fcc2)
        {
            // If both are null, or both are same instance, return true.
            if (System.Object.ReferenceEquals(fcc1, fcc2))
            {
                return true;
            }

            // If one is null, but not both, return false.
            if (((object)fcc1 == null) || ((object)fcc2 == null))
            {
                return false;
            }

            return fcc1.m_fourCC == fcc2.m_fourCC;
        }

        public static bool operator !=(FourCC fcc1, FourCC fcc2)
        {
            return !(fcc1 == fcc2);
        }

        public override bool Equals(object obj)
        {
            if (!(obj is FourCC))
                return false;

            return (obj as FourCC).m_fourCC == m_fourCC;
        }

        public override int GetHashCode()
        {
            return m_fourCC.GetHashCode();
        }

        public override string ToString()
        {
            char c;
            char[] ca = new char[4];

            c = Convert.ToChar(m_fourCC & 255);
            if (!Char.IsLetterOrDigit(c))
            {
                c = ' ';
            }
            ca[0] = c;

            c = Convert.ToChar((m_fourCC >> 8) & 255);
            if (!Char.IsLetterOrDigit(c))
            {
                c = ' ';
            }
            ca[1] = c;

            c = Convert.ToChar((m_fourCC >> 16) & 255);
            if (!Char.IsLetterOrDigit(c))
            {
                c = ' ';
            }
            ca[2] = c;

            c = Convert.ToChar((m_fourCC >> 24) & 255);
            if (!Char.IsLetterOrDigit(c))
            {
                c = ' ';
            }
            ca[3] = c;

            string s = new string(ca);

            return s;
        }

        public static bool IsA4ccSubtype(Guid g)
        {
            return (g.ToString().EndsWith("-0000-0010-8000-00aa00389b71"));
        }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1), UnmanagedName("WAVEFORMATEX")]
    public class WaveFormatEx
    {
        public short wFormatTag;
        public short nChannels;
        public int nSamplesPerSec;
        public int nAvgBytesPerSec;
        public short nBlockAlign;
        public short wBitsPerSample;
        public short cbSize;

        public override string ToString()
        {
            return string.Format("{0} {1} {2} {3}", wFormatTag, nChannels, nSamplesPerSec, wBitsPerSample);
        }

        public IntPtr GetPtr()
        {
            IntPtr ip;

            // See what kind of WaveFormat object we've got
            if (this is WaveFormatExtensibleWithData)
            {
                int iExtensibleSize = Marshal.SizeOf(typeof(WaveFormatExtensible));
                int iWaveFormatExSize = Marshal.SizeOf(typeof(WaveFormatEx));

                // WaveFormatExtensibleWithData - Have to copy the byte array too
                WaveFormatExtensibleWithData pData = this as WaveFormatExtensibleWithData;

                int iExtraBytes = pData.cbSize - (iExtensibleSize - iWaveFormatExSize);

                // Account for copying the array.  This may result in us allocating more bytes than we
                // need (if cbSize < IntPtr.Size), but it prevents us from overrunning the buffer.
                int iUseSize = Math.Max(iExtraBytes, IntPtr.Size);

                // Remember, cbSize include the length of WaveFormatExtensible
                ip = Marshal.AllocCoTaskMem(iExtensibleSize + iUseSize);

                // Copies the waveformatex + waveformatextensible
                Marshal.StructureToPtr(pData, ip, false);

                // Get a pointer to the byte after the copy
                IntPtr ip2 = new IntPtr(ip.ToInt64() + iExtensibleSize);

                // Copy the extra bytes
                Marshal.Copy(pData.byteData, 0, ip2, pData.cbSize - (iExtensibleSize - iWaveFormatExSize));
            }
            else if (this is WaveFormatExtensible)
            {
                int iWaveFormatExtensibleSize = Marshal.SizeOf(typeof(WaveFormatExtensible));

                // WaveFormatExtensible - Just do a simple copy
                WaveFormatExtensible pExt = this as WaveFormatExtensible;

                ip = Marshal.AllocCoTaskMem(iWaveFormatExtensibleSize);

                Marshal.StructureToPtr(this as WaveFormatExtensible, ip, false);
            }
            else if (this is WaveFormatExWithData)
            {
                int iWaveFormatExSize = Marshal.SizeOf(typeof(WaveFormatEx));

                // WaveFormatExWithData - Have to copy the byte array too
                WaveFormatExWithData pData = this as WaveFormatExWithData;

                // Account for copying the array.  This may result in us allocating more bytes than we
                // need (if cbSize < IntPtr.Size), but it prevents us from overrunning the buffer.
                int iUseSize = Math.Max(pData.cbSize, IntPtr.Size);

                ip = Marshal.AllocCoTaskMem(iWaveFormatExSize + iUseSize);

                Marshal.StructureToPtr(pData, ip, false);

                IntPtr ip2 = new IntPtr(ip.ToInt64() + iWaveFormatExSize);
                Marshal.Copy(pData.byteData, 0, ip2, pData.cbSize);
            }
            else if (this is WaveFormatEx)
            {
                int iWaveFormatExSize = Marshal.SizeOf(typeof(WaveFormatEx));

                // WaveFormatEx - just do a copy
                ip = Marshal.AllocCoTaskMem(iWaveFormatExSize);
                Marshal.StructureToPtr(this as WaveFormatEx, ip, false);
            }
            else
            {
                // Someone added our custom marshaler to something they shouldn't have
                Debug.Assert(false, "Shouldn't ever get here");
                ip = IntPtr.Zero;
            }

            return ip;
        }

        public static WaveFormatEx PtrToWave(IntPtr pNativeData)
        {
            short wFormatTag = Marshal.ReadInt16(pNativeData);
            WaveFormatEx wfe;

            // WAVE_FORMAT_EXTENSIBLE == -2
            if (wFormatTag != -2)
            {
                short cbSize;

                // By spec, PCM has no cbSize element
                if (wFormatTag != 1)
                {
                    cbSize = Marshal.ReadInt16(pNativeData, 16);
                }
                else
                {
                    cbSize = 0;
                }

                // Does the structure contain extra data?
                if (cbSize == 0)
                {
                    // Create a simple WaveFormatEx struct
                    wfe = new WaveFormatEx();
                    Marshal.PtrToStructure(pNativeData, wfe);

                    // It probably already has the right value, but there is a special case
                    // where it might not, so, just to be safe...
                    wfe.cbSize = 0;
                }
                else
                {
                    WaveFormatExWithData dat = new WaveFormatExWithData();

                    // Manually parse the data into the structure
                    dat.wFormatTag = wFormatTag;
                    dat.nChannels = Marshal.ReadInt16(pNativeData, 2);
                    dat.nSamplesPerSec = Marshal.ReadInt32(pNativeData, 4);
                    dat.nAvgBytesPerSec = Marshal.ReadInt32(pNativeData, 8);
                    dat.nBlockAlign = Marshal.ReadInt16(pNativeData, 12);
                    dat.wBitsPerSample = Marshal.ReadInt16(pNativeData, 14);
                    dat.cbSize = cbSize;

                    dat.byteData = new byte[dat.cbSize];
                    IntPtr ip2 = new IntPtr(pNativeData.ToInt64() + 18);
                    Marshal.Copy(ip2, dat.byteData, 0, dat.cbSize);

                    wfe = dat as WaveFormatEx;
                }
            }
            else
            {
                short cbSize;
                int extrasize = Marshal.SizeOf(typeof(WaveFormatExtensible)) - Marshal.SizeOf(typeof(WaveFormatEx));

                cbSize = Marshal.ReadInt16(pNativeData, 16);
                if (cbSize == extrasize)
                {
                    WaveFormatExtensible ext = new WaveFormatExtensible();
                    Marshal.PtrToStructure(pNativeData, ext);
                    wfe = ext as WaveFormatEx;
                }
                else
                {
                    WaveFormatExtensibleWithData ext = new WaveFormatExtensibleWithData();
                    int iExtraBytes = cbSize - extrasize;

                    ext.wFormatTag = wFormatTag;
                    ext.nChannels = Marshal.ReadInt16(pNativeData, 2);
                    ext.nSamplesPerSec = Marshal.ReadInt32(pNativeData, 4);
                    ext.nAvgBytesPerSec = Marshal.ReadInt32(pNativeData, 8);
                    ext.nBlockAlign = Marshal.ReadInt16(pNativeData, 12);
                    ext.wBitsPerSample = Marshal.ReadInt16(pNativeData, 14);
                    ext.cbSize = cbSize;

                    ext.wValidBitsPerSample = Marshal.ReadInt16(pNativeData, 18);
                    ext.dwChannelMask = (WaveMask)Marshal.ReadInt16(pNativeData, 20);

                    // Read the Guid
                    byte[] byteGuid = new byte[16];
                    Marshal.Copy(new IntPtr(pNativeData.ToInt64() + 24), byteGuid, 0, 16);
                    ext.SubFormat = new Guid(byteGuid);

                    ext.byteData = new byte[iExtraBytes];
                    IntPtr ip2 = new IntPtr(pNativeData.ToInt64() + Marshal.SizeOf(typeof(WaveFormatExtensible)));
                    Marshal.Copy(ip2, ext.byteData, 0, iExtraBytes);

                    wfe = ext as WaveFormatEx;
                }
            }

            return wfe;
        }

        public static bool operator ==(WaveFormatEx a, WaveFormatEx b)
        {
            // If both are null, or both are same instance, return true.
            if (System.Object.ReferenceEquals(a, b))
            {
                return true;
            }

            // If one is null, but not both, return false.
            if (((object)a == null) || ((object)b == null))
            {
                return false;
            }

            bool bRet;
            Type t1 = a.GetType();
            Type t2 = b.GetType();

            if (t1 == t2 &&
                a.wFormatTag == b.wFormatTag &&
                a.nChannels == b.nChannels &&
                a.nSamplesPerSec == b.nSamplesPerSec &&
                a.nAvgBytesPerSec == b.nAvgBytesPerSec &&
                a.nBlockAlign == b.nBlockAlign &&
                a.wBitsPerSample == b.wBitsPerSample &&
                a.cbSize == b.cbSize)
            {
                bRet = true;
            }
            else
            {
                bRet = false;
            }

            return bRet;
        }

        public static bool operator !=(WaveFormatEx a, WaveFormatEx b)
        {
            return !(a == b);
        }

        public override bool Equals(object obj)
        {
            return this == (obj as WaveFormatEx);
        }

        public override int GetHashCode()
        {
            return nAvgBytesPerSec + wFormatTag;
        }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1), UnmanagedName("WAVEFORMATEX")]
    public class WaveFormatExWithData : WaveFormatEx
    {
        public byte[] byteData;

        public static bool operator ==(WaveFormatExWithData a, WaveFormatExWithData b)
        {
            bool bRet = ((WaveFormatEx)a) == ((WaveFormatEx)b);

            if (bRet)
            {
                if (b.byteData == null)
                {
                    bRet = a.byteData == null;
                }
                else
                {
                    if (b.byteData.Length == a.byteData.Length)
                    {
                        for (int x = 0; x < b.byteData.Length; x++)
                        {
                            if (b.byteData[x] != a.byteData[x])
                            {
                                bRet = false;
                                break;
                            }
                        }
                    }
                    else
                    {
                        bRet = false;
                    }
                }
            }

            return bRet;
        }

        public static bool operator !=(WaveFormatExWithData a, WaveFormatExWithData b)
        {
            return !(a == b);
        }

        public override bool Equals(object obj)
        {
            return this == (obj as WaveFormatExWithData);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }

    [StructLayout(LayoutKind.Explicit, Pack = 1), UnmanagedName("WAVEFORMATEXTENSIBLE")]
    public class WaveFormatExtensible : WaveFormatEx
    {
        [FieldOffset(0)]
        public short wValidBitsPerSample;
        [FieldOffset(0)]
        public short wSamplesPerBlock;
        [FieldOffset(0)]
        public short wReserved;
        [FieldOffset(2)]
        public WaveMask dwChannelMask;
        [FieldOffset(6)]
        public Guid SubFormat;

        public static bool operator ==(WaveFormatExtensible a, WaveFormatExtensible b)
        {
            bool bRet = ((WaveFormatEx)a) == ((WaveFormatEx)b);

            if (bRet)
            {
                bRet = (a.wValidBitsPerSample == b.wValidBitsPerSample &&
                    a.dwChannelMask == b.dwChannelMask &&
                    a.SubFormat == b.SubFormat);
            }

            return bRet;
        }

        public static bool operator !=(WaveFormatExtensible a, WaveFormatExtensible b)
        {
            return !(a == b);
        }

        public override bool Equals(object obj)
        {
            return this == (obj as WaveFormatExtensible);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1), UnmanagedName("WAVEFORMATEX")]
    public class WaveFormatExtensibleWithData : WaveFormatExtensible
    {
        public byte[] byteData;

        public static bool operator ==(WaveFormatExtensibleWithData a, WaveFormatExtensibleWithData b)
        {
            bool bRet = ((WaveFormatExtensible)a) == ((WaveFormatExtensible)b);

            if (bRet)
            {
                if (b.byteData == null)
                {
                    bRet = a.byteData == null;
                }
                else
                {
                    if (b.byteData.Length == a.byteData.Length)
                    {
                        for (int x = 0; x < b.byteData.Length; x++)
                        {
                            if (b.byteData[x] != a.byteData[x])
                            {
                                bRet = false;
                                break;
                            }
                        }
                    }
                    else
                    {
                        bRet = false;
                    }
                }
            }

            return bRet;
        }

        public static bool operator !=(WaveFormatExtensibleWithData a, WaveFormatExtensibleWithData b)
        {
            return !(a == b);
        }

        public override bool Equals(object obj)
        {
            return this == (obj as WaveFormatExtensibleWithData);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 4), UnmanagedName("BITMAPINFOHEADER")]
    public class BitmapInfoHeader
    {
        public int Size;
        public int Width;
        public int Height;
        public short Planes;
        public short BitCount;
        public int Compression;
        public int ImageSize;
        public int XPelsPerMeter;
        public int YPelsPerMeter;
        public int ClrUsed;
        public int ClrImportant;

        public IntPtr GetPtr()
        {
            IntPtr ip;

            // See what kind of BitmapInfoHeader object we've got
            if (this is BitmapInfoHeaderWithData)
            {
                int iBitmapInfoHeaderSize = Marshal.SizeOf(typeof(BitmapInfoHeader));

                // BitmapInfoHeaderWithData - Have to copy the array too
                BitmapInfoHeaderWithData pData = this as BitmapInfoHeaderWithData;

                // Account for copying the array.  This may result in us allocating more bytes than we
                // need (if cbSize < IntPtr.Size), but it prevents us from overrunning the buffer.
                int iUseSize = Math.Max(pData.bmiColors.Length * 4, IntPtr.Size);

                ip = Marshal.AllocCoTaskMem(iBitmapInfoHeaderSize + iUseSize);

                Marshal.StructureToPtr(pData, ip, false);

                IntPtr ip2 = new IntPtr(ip.ToInt64() + iBitmapInfoHeaderSize);
                Marshal.Copy(pData.bmiColors, 0, ip2, pData.bmiColors.Length);
            }
            else if (this is BitmapInfoHeader)
            {
                int iBitmapInfoHeaderSize = Marshal.SizeOf(typeof(BitmapInfoHeader));

                // BitmapInfoHeader - just do a copy
                ip = Marshal.AllocCoTaskMem(iBitmapInfoHeaderSize);
                Marshal.StructureToPtr(this as BitmapInfoHeader, ip, false);
            }
            else
            {
                Debug.Assert(false, "Shouldn't ever get here");
                ip = IntPtr.Zero;
            }

            return ip;
        }

        public static BitmapInfoHeader PtrToBMI(IntPtr pNativeData)
        {
            int iEntries;
            int biCompression;
            int biClrUsed;
            int biBitCount;

            biBitCount = Marshal.ReadInt16(pNativeData, 14);
            biCompression = Marshal.ReadInt32(pNativeData, 16);
            biClrUsed = Marshal.ReadInt32(pNativeData, 32);

            if (biCompression == 3) // BI_BITFIELDS
            {
                iEntries = 3;
            }
            else if (biClrUsed > 0)
            {
                iEntries = biClrUsed;
            }
            else if (biBitCount <= 8)
            {
                iEntries = 1 << biBitCount;
            }
            else
            {
                iEntries = 0;
            }

            BitmapInfoHeader bmi;

            if (iEntries == 0)
            {
                // Create a simple BitmapInfoHeader struct
                bmi = new BitmapInfoHeader();
                Marshal.PtrToStructure(pNativeData, bmi);
            }
            else
            {
                BitmapInfoHeaderWithData ext = new BitmapInfoHeaderWithData();

                ext.Size = Marshal.ReadInt32(pNativeData, 0);
                ext.Width = Marshal.ReadInt32(pNativeData, 4);
                ext.Height = Marshal.ReadInt32(pNativeData, 8);
                ext.Planes = Marshal.ReadInt16(pNativeData, 12);
                ext.BitCount = Marshal.ReadInt16(pNativeData, 14);
                ext.Compression = Marshal.ReadInt32(pNativeData, 16);
                ext.ImageSize = Marshal.ReadInt32(pNativeData, 20);
                ext.XPelsPerMeter = Marshal.ReadInt32(pNativeData, 24);
                ext.YPelsPerMeter = Marshal.ReadInt32(pNativeData, 28);
                ext.ClrUsed = Marshal.ReadInt32(pNativeData, 32);
                ext.ClrImportant = Marshal.ReadInt32(pNativeData, 36);

                bmi = ext as BitmapInfoHeader;

                ext.bmiColors = new int[iEntries];
                IntPtr ip2 = new IntPtr(pNativeData.ToInt64() + Marshal.SizeOf(typeof(BitmapInfoHeader)));
                Marshal.Copy(ip2, ext.bmiColors, 0, iEntries);
            }

            return bmi;
        }

        public void CopyFrom(BitmapInfoHeader bmi)
        {
            Size = bmi.Size;
            Width = bmi.Width;
            Height = bmi.Height;
            Planes = bmi.Planes;
            BitCount = bmi.BitCount;
            Compression = bmi.Compression;
            ImageSize = bmi.ImageSize;
            YPelsPerMeter = bmi.YPelsPerMeter;
            ClrUsed = bmi.ClrUsed;
            ClrImportant = bmi.ClrImportant;

            if (bmi is BitmapInfoHeaderWithData)
            {
                BitmapInfoHeaderWithData ext = this as BitmapInfoHeaderWithData;
                BitmapInfoHeaderWithData ext2 = bmi as BitmapInfoHeaderWithData;

                ext.bmiColors = new int[ext2.bmiColors.Length];
                ext2.bmiColors.CopyTo(ext.bmiColors, 0);
            }
        }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 4), UnmanagedName("BITMAPINFO")]
    public class BitmapInfoHeaderWithData : BitmapInfoHeader
    {
        public int[] bmiColors;
    }

    [StructLayout(LayoutKind.Sequential)]
    public class MFInt
    {
        protected int m_value;

        public MFInt(int v)
        {
            m_value = v;
        }

        public int GetValue()
        {
            return m_value;
        }

        // While I *could* enable this code, it almost certainly won't do what you
        // think it will.  Generally you don't want to create a *new* instance of
        // MFInt and assign a value to it.  You want to assign a value to an
        // existing instance.  In order to do this automatically, .Net would have
        // to support overloading operator =.  But since it doesn't, use Assign()

        //public static implicit operator MFInt(int f)
        //{
        //    return new MFInt(f);
        //}

        public static implicit operator int(MFInt f)
        {
            return f.m_value;
        }

        public int ToInt32()
        {
            return m_value;
        }

        public void Assign(int f)
        {
            m_value = f;
        }

        public override string ToString()
        {
            return m_value.ToString();
        }

        public override int GetHashCode()
        {
            return m_value.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (obj is MFInt)
            {
                return ((MFInt)obj).m_value == m_value;
            }

            return Convert.ToInt32(obj) == m_value;
        }
    }

    /// <summary>
    /// MFRect is a managed representation of the Win32 RECT structure.
    /// </summary>
    //[StructLayout(LayoutKind.Sequential)]
    //public class MFRect
    //{
    //    public int left;
    //    public int top;
    //    public int right;
    //    public int bottom;

    //    /// <summary>
    //    /// Empty contructor. Initialize all fields to 0
    //    /// </summary>
    //    public MFRect()
    //    {
    //    }

    //    /// <summary>
    //    /// A parametred constructor. Initialize fields with given values.
    //    /// </summary>
    //    /// <param name="left">the left value</param>
    //    /// <param name="top">the top value</param>
    //    /// <param name="right">the right value</param>
    //    /// <param name="bottom">the bottom value</param>
    //    public MFRect(int l, int t, int r, int b)
    //    {
    //        left = l;
    //        top = t;
    //        right = r;
    //        bottom = b;
    //    }

    //    /// <summary>
    //    /// A parametred constructor. Initialize fields with a given <see cref="System.Drawing.Rectangle"/>.
    //    /// </summary>
    //    /// <param name="rectangle">A <see cref="System.Drawing.Rectangle"/></param>
    //    /// <remarks>
    //    /// Warning, MFRect define a rectangle by defining two of his corners and <see cref="System.Drawing.Rectangle"/> define a rectangle with his upper/left corner, his width and his height.
    //    /// </remarks>
    //    public MFRect(Rectangle rectangle)
    //    {
    //        left = rectangle.Left;
    //        top = rectangle.Top;
    //        right = rectangle.Right;
    //        bottom = rectangle.Bottom;
    //    }

    //    /// <summary>
    //    /// Provide de string representation of this MFRect instance
    //    /// </summary>
    //    /// <returns>A string formated like this : [left, top - right, bottom]</returns>
    //    public override string ToString()
    //    {
    //        return string.Format("[{0}, {1}] - [{2}, {3}]", left, top, right, bottom);
    //    }

    //    public override int GetHashCode()
    //    {
    //        return left.GetHashCode() |
    //            top.GetHashCode() |
    //            right.GetHashCode() |
    //            bottom.GetHashCode();
    //    }

    //    public override bool Equals(object obj)
    //    {
    //        if (obj is MFRect)
    //        {
    //            MFRect cmp = (MFRect)obj;

    //            return right == cmp.right && bottom == cmp.bottom && left == cmp.left && top == cmp.top;
    //        }

    //        if (obj is Rectangle)
    //        {
    //            Rectangle cmp = (Rectangle)obj;

    //            return right == cmp.Right && bottom == cmp.Bottom && left == cmp.Left && top == cmp.Top;
    //        }

    //        return false;
    //    }

    //    /// <summary>
    //    /// Checks to see if the rectangle is empty
    //    /// </summary>
    //    /// <returns>Returns true if the rectangle is empty</returns>
    //    public bool IsEmpty()
    //    {
    //        return (right <= left || bottom <= top);
    //    }

    //    /// <summary>
    //    /// Define implicit cast between MFRect and System.Drawing.Rectangle for languages supporting this feature.
    //    /// VB.Net doesn't support implicit cast. <see cref="MFRect.ToRectangle"/> for similar functionality.
    //    /// <code>
    //    ///   // Define a new Rectangle instance
    //    ///   Rectangle r = new Rectangle(0, 0, 100, 100);
    //    ///   // Do implicit cast between Rectangle and MFRect
    //    ///   MFRect mfR = r;
    //    ///
    //    ///   Console.WriteLine(mfR.ToString());
    //    /// </code>
    //    /// </summary>
    //    /// <param name="r">a MFRect to be cast</param>
    //    /// <returns>A casted System.Drawing.Rectangle</returns>
    //    public static implicit operator Rectangle(MFRect r)
    //    {
    //        return r.ToRectangle();
    //    }

    //    /// <summary>
    //    /// Define implicit cast between System.Drawing.Rectangle and MFRect for languages supporting this feature.
    //    /// VB.Net doesn't support implicit cast. <see cref="MFRect.FromRectangle"/> for similar functionality.
    //    /// <code>
    //    ///   // Define a new MFRect instance
    //    ///   MFRect mfR = new MFRect(0, 0, 100, 100);
    //    ///   // Do implicit cast between MFRect and Rectangle
    //    ///   Rectangle r = mfR;
    //    ///
    //    ///   Console.WriteLine(r.ToString());
    //    /// </code>
    //    /// </summary>
    //    /// <param name="r">A System.Drawing.Rectangle to be cast</param>
    //    /// <returns>A casted MFRect</returns>
    //    public static implicit operator MFRect(Rectangle r)
    //    {
    //        return new MFRect(r);
    //    }

    //    /// <summary>
    //    /// Get the System.Drawing.Rectangle equivalent to this MFRect instance.
    //    /// </summary>
    //    /// <returns>A System.Drawing.Rectangle</returns>
    //    public Rectangle ToRectangle()
    //    {
    //        return new Rectangle(left, top, (right - left), (bottom - top));
    //    }

    //    /// <summary>
    //    /// Get a new MFRect instance for a given <see cref="System.Drawing.Rectangle"/>
    //    /// </summary>
    //    /// <param name="r">The <see cref="System.Drawing.Rectangle"/> used to initialize this new MFGuid</param>
    //    /// <returns>A new instance of MFGuid</returns>
    //    public static MFRect FromRectangle(Rectangle r)
    //    {
    //        return new MFRect(r);
    //    }

    //    /// <summary>
    //    /// Copy the members from an MFRect into this object
    //    /// </summary>
    //    /// <param name="from">The rectangle from which to copy the values.</param>
    //    public void CopyFrom(MFRect from)
    //    {
    //        left = from.left;
    //        top = from.top;
    //        right = from.right;
    //        bottom = from.bottom;
    //    }
    //}

    /// <summary>
    /// MFGuid is a wrapper class around a System.Guid value type.
    /// </summary>
    /// <remarks>
    /// This class is necessary to enable null paramters passing.
    /// </remarks>
    [StructLayout(LayoutKind.Explicit)]
    public class MFGuid
    {
        [FieldOffset(0)]
        private Guid guid;

        public static readonly MFGuid Empty = Guid.Empty;

        /// <summary>
        /// Empty constructor. 
        /// Initialize it with System.Guid.Empty
        /// </summary>
        public MFGuid()
        {
            this.guid = Empty;
        }

        /// <summary>
        /// Constructor.
        /// Initialize this instance with a given System.Guid string representation.
        /// </summary>
        /// <param name="g">A valid System.Guid as string</param>
        public MFGuid(string g)
        {
            this.guid = new Guid(g);
        }

        /// <summary>
        /// Constructor.
        /// Initialize this instance with a given System.Guid.
        /// </summary>
        /// <param name="g">A System.Guid value type</param>
        public MFGuid(Guid g)
        {
            this.guid = g;
        }

        /// <summary>
        /// Get a string representation of this MFGuid Instance.
        /// </summary>
        /// <returns>A string representing this instance</returns>
        public override string ToString()
        {
            return this.guid.ToString();
        }

        /// <summary>
        /// Get a string representation of this MFGuid Instance with a specific format.
        /// </summary>
        /// <param name="format"><see cref="System.Guid.ToString"/> for a description of the format parameter.</param>
        /// <returns>A string representing this instance according to the format parameter</returns>
        public string ToString(string format)
        {
            return this.guid.ToString(format);
        }

        public override int GetHashCode()
        {
            return this.guid.GetHashCode();
        }

        /// <summary>
        /// Define implicit cast between MFGuid and System.Guid for languages supporting this feature.
        /// VB.Net doesn't support implicit cast. <see cref="MFGuid.ToGuid"/> for similar functionality.
        /// <code>
        ///   // Define a new MFGuid instance
        ///   MFGuid mfG = new MFGuid("{33D57EBF-7C9D-435e-A15E-D300B52FBD91}");
        ///   // Do implicit cast between MFGuid and Guid
        ///   Guid g = mfG;
        ///
        ///   Console.WriteLine(g.ToString());
        /// </code>
        /// </summary>
        /// <param name="g">MFGuid to be cast</param>
        /// <returns>A casted System.Guid</returns>
        public static implicit operator Guid(MFGuid g)
        {
            return g.guid;
        }

        /// <summary>
        /// Define implicit cast between System.Guid and MFGuid for languages supporting this feature.
        /// VB.Net doesn't support implicit cast. <see cref="MFGuid.FromGuid"/> for similar functionality.
        /// <code>
        ///   // Define a new Guid instance
        ///   Guid g = new Guid("{B9364217-366E-45f8-AA2D-B0ED9E7D932D}");
        ///   // Do implicit cast between Guid and MFGuid
        ///   MFGuid mfG = g;
        ///
        ///   Console.WriteLine(mfG.ToString());
        /// </code>
        /// </summary>
        /// <param name="g">System.Guid to be cast</param>
        /// <returns>A casted MFGuid</returns>
        public static implicit operator MFGuid(Guid g)
        {
            return new MFGuid(g);
        }

        /// <summary>
        /// Get the System.Guid equivalent to this MFGuid instance.
        /// </summary>
        /// <returns>A System.Guid</returns>
        public Guid ToGuid()
        {
            return this.guid;
        }

        /// <summary>
        /// Get a new MFGuid instance for a given System.Guid
        /// </summary>
        /// <param name="g">The System.Guid to wrap into a MFGuid</param>
        /// <returns>A new instance of MFGuid</returns>
        public static MFGuid FromGuid(Guid g)
        {
            return new MFGuid(g);
        }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 4), UnmanagedName("SIZE")]
    public class MFSize
    {
        public int cx;
        public int cy;

        public MFSize()
        {
            cx = 0;
            cy = 0;
        }

        public MFSize(int iWidth, int iHeight)
        {
            cx = iWidth;
            cy = iHeight;
        }

        public int Width
        {
            get
            {
                return cx;
            }
            set
            {
                cx = value;
            }
        }
        public int Height
        {
            get
            {
                return cy;
            }
            set
            {
                cy = value;
            }
        }
    }

    #endregion

    #region Utility Classes

    public static class MFError
    {
        #region Errors

        public const int MF_E_PLATFORM_NOT_INITIALIZED = unchecked((int)0xC00D36B0);
        public const int MF_E_BUFFERTOOSMALL = unchecked((int)0xC00D36B1);
        public const int MF_E_INVALIDREQUEST = unchecked((int)0xC00D36B2);
        public const int MF_E_INVALIDSTREAMNUMBER = unchecked((int)0xC00D36B3);
        public const int MF_E_INVALIDMEDIATYPE = unchecked((int)0xC00D36B4);
        public const int MF_E_NOTACCEPTING = unchecked((int)0xC00D36B5);
        public const int MF_E_NOT_INITIALIZED = unchecked((int)0xC00D36B6);
        public const int MF_E_UNSUPPORTED_REPRESENTATION = unchecked((int)0xC00D36B7);
        public const int MF_E_NO_MORE_TYPES = unchecked((int)0xC00D36B9);
        public const int MF_E_UNSUPPORTED_SERVICE = unchecked((int)0xC00D36BA);
        public const int MF_E_UNEXPECTED = unchecked((int)0xC00D36BB);
        public const int MF_E_INVALIDNAME = unchecked((int)0xC00D36BC);
        public const int MF_E_INVALIDTYPE = unchecked((int)0xC00D36BD);
        public const int MF_E_INVALID_FILE_FORMAT = unchecked((int)0xC00D36BE);
        public const int MF_E_INVALIDINDEX = unchecked((int)0xC00D36BF);
        public const int MF_E_INVALID_TIMESTAMP = unchecked((int)0xC00D36C0);
        public const int MF_E_UNSUPPORTED_SCHEME = unchecked((int)0xC00D36C3);
        public const int MF_E_UNSUPPORTED_BYTESTREAM_TYPE = unchecked((int)0xC00D36C4);
        public const int MF_E_UNSUPPORTED_TIME_FORMAT = unchecked((int)0xC00D36C5);
        public const int MF_E_NO_SAMPLE_TIMESTAMP = unchecked((int)0xC00D36C8);
        public const int MF_E_NO_SAMPLE_DURATION = unchecked((int)0xC00D36C9);
        public const int MF_E_INVALID_STREAM_DATA = unchecked((int)0xC00D36CB);
        public const int MF_E_RT_UNAVAILABLE = unchecked((int)0xC00D36CF);
        public const int MF_E_UNSUPPORTED_RATE = unchecked((int)0xC00D36D0);
        public const int MF_E_THINNING_UNSUPPORTED = unchecked((int)0xC00D36D1);
        public const int MF_E_REVERSE_UNSUPPORTED = unchecked((int)0xC00D36D2);
        public const int MF_E_UNSUPPORTED_RATE_TRANSITION = unchecked((int)0xC00D36D3);
        public const int MF_E_RATE_CHANGE_PREEMPTED = unchecked((int)0xC00D36D4);
        public const int MF_E_NOT_FOUND = unchecked((int)0xC00D36D5);
        public const int MF_E_NOT_AVAILABLE = unchecked((int)0xC00D36D6);
        public const int MF_E_NO_CLOCK = unchecked((int)0xC00D36D7);
        public const int MF_S_MULTIPLE_BEGIN = unchecked((int)0x000D36D8);
        public const int MF_E_MULTIPLE_BEGIN = unchecked((int)0xC00D36D9);
        public const int MF_E_MULTIPLE_SUBSCRIBERS = unchecked((int)0xC00D36DA);
        public const int MF_E_TIMER_ORPHANED = unchecked((int)0xC00D36DB);
        public const int MF_E_STATE_TRANSITION_PENDING = unchecked((int)0xC00D36DC);
        public const int MF_E_UNSUPPORTED_STATE_TRANSITION = unchecked((int)0xC00D36DD);
        public const int MF_E_UNRECOVERABLE_ERROR_OCCURRED = unchecked((int)0xC00D36DE);
        public const int MF_E_SAMPLE_HAS_TOO_MANY_BUFFERS = unchecked((int)0xC00D36DF);
        public const int MF_E_SAMPLE_NOT_WRITABLE = unchecked((int)0xC00D36E0);
        public const int MF_E_INVALID_KEY = unchecked((int)0xC00D36E2);
        public const int MF_E_BAD_STARTUP_VERSION = unchecked((int)0xC00D36E3);
        public const int MF_E_UNSUPPORTED_CAPTION = unchecked((int)0xC00D36E4);
        public const int MF_E_INVALID_POSITION = unchecked((int)0xC00D36E5);
        public const int MF_E_ATTRIBUTENOTFOUND = unchecked((int)0xC00D36E6);
        public const int MF_E_PROPERTY_TYPE_NOT_ALLOWED = unchecked((int)0xC00D36E7);
        public const int MF_E_PROPERTY_TYPE_NOT_SUPPORTED = unchecked((int)0xC00D36E8);
        public const int MF_E_PROPERTY_EMPTY = unchecked((int)0xC00D36E9);
        public const int MF_E_PROPERTY_NOT_EMPTY = unchecked((int)0xC00D36EA);
        public const int MF_E_PROPERTY_VECTOR_NOT_ALLOWED = unchecked((int)0xC00D36EB);
        public const int MF_E_PROPERTY_VECTOR_REQUIRED = unchecked((int)0xC00D36EC);
        public const int MF_E_OPERATION_CANCELLED = unchecked((int)0xC00D36ED);
        public const int MF_E_BYTESTREAM_NOT_SEEKABLE = unchecked((int)0xC00D36EE);
        public const int MF_E_DISABLED_IN_SAFEMODE = unchecked((int)0xC00D36EF);
        public const int MF_E_CANNOT_PARSE_BYTESTREAM = unchecked((int)0xC00D36F0);
        public const int MF_E_SOURCERESOLVER_MUTUALLY_EXCLUSIVE_FLAGS = unchecked((int)0xC00D36F1);
        public const int MF_E_MEDIAPROC_WRONGSTATE = unchecked((int)0xC00D36F2);
        public const int MF_E_RT_THROUGHPUT_NOT_AVAILABLE = unchecked((int)0xC00D36F3);
        public const int MF_E_RT_TOO_MANY_CLASSES = unchecked((int)0xC00D36F4);
        public const int MF_E_RT_WOULDBLOCK = unchecked((int)0xC00D36F5);
        public const int MF_E_NO_BITPUMP = unchecked((int)0xC00D36F6);
        public const int MF_E_RT_OUTOFMEMORY = unchecked((int)0xC00D36F7);
        public const int MF_E_RT_WORKQUEUE_CLASS_NOT_SPECIFIED = unchecked((int)0xC00D36F8);
        public const int MF_E_INSUFFICIENT_BUFFER = unchecked((int)0xC00D7170);
        public const int MF_E_CANNOT_CREATE_SINK = unchecked((int)0xC00D36FA);
        public const int MF_E_BYTESTREAM_UNKNOWN_LENGTH = unchecked((int)0xC00D36FB);
        public const int MF_E_SESSION_PAUSEWHILESTOPPED = unchecked((int)0xC00D36FC);
        public const int MF_S_ACTIVATE_REPLACED = unchecked((int)0x000D36FD);
        public const int MF_E_FORMAT_CHANGE_NOT_SUPPORTED = unchecked((int)0xC00D36FE);
        public const int MF_E_INVALID_WORKQUEUE = unchecked((int)0xC00D36FF);
        public const int MF_E_DRM_UNSUPPORTED = unchecked((int)0xC00D3700);
        public const int MF_E_UNAUTHORIZED = unchecked((int)0xC00D3701);
        public const int MF_E_OUT_OF_RANGE = unchecked((int)0xC00D3702);
        public const int MF_E_INVALID_CODEC_MERIT = unchecked((int)0xC00D3703);
        public const int MF_E_HW_MFT_FAILED_START_STREAMING = unchecked((int)0xC00D3704);
        public const int MF_S_ASF_PARSEINPROGRESS = unchecked((int)0x400D3A98);
        public const int MF_E_ASF_PARSINGINCOMPLETE = unchecked((int)0xC00D3A98);
        public const int MF_E_ASF_MISSINGDATA = unchecked((int)0xC00D3A99);
        public const int MF_E_ASF_INVALIDDATA = unchecked((int)0xC00D3A9A);
        public const int MF_E_ASF_OPAQUEPACKET = unchecked((int)0xC00D3A9B);
        public const int MF_E_ASF_NOINDEX = unchecked((int)0xC00D3A9C);
        public const int MF_E_ASF_OUTOFRANGE = unchecked((int)0xC00D3A9D);
        public const int MF_E_ASF_INDEXNOTLOADED = unchecked((int)0xC00D3A9E);
        public const int MF_E_ASF_TOO_MANY_PAYLOADS = unchecked((int)0xC00D3A9F);
        public const int MF_E_ASF_UNSUPPORTED_STREAM_TYPE = unchecked((int)0xC00D3AA0);
        public const int MF_E_ASF_DROPPED_PACKET = unchecked((int)0xC00D3AA1);
        public const int MF_E_NO_EVENTS_AVAILABLE = unchecked((int)0xC00D3E80);
        public const int MF_E_INVALID_STATE_TRANSITION = unchecked((int)0xC00D3E82);
        public const int MF_E_END_OF_STREAM = unchecked((int)0xC00D3E84);
        public const int MF_E_SHUTDOWN = unchecked((int)0xC00D3E85);
        public const int MF_E_MP3_NOTFOUND = unchecked((int)0xC00D3E86);
        public const int MF_E_MP3_OUTOFDATA = unchecked((int)0xC00D3E87);
        public const int MF_E_MP3_NOTMP3 = unchecked((int)0xC00D3E88);
        public const int MF_E_MP3_NOTSUPPORTED = unchecked((int)0xC00D3E89);
        public const int MF_E_NO_DURATION = unchecked((int)0xC00D3E8A);
        public const int MF_E_INVALID_FORMAT = unchecked((int)0xC00D3E8C);
        public const int MF_E_PROPERTY_NOT_FOUND = unchecked((int)0xC00D3E8D);
        public const int MF_E_PROPERTY_READ_ONLY = unchecked((int)0xC00D3E8E);
        public const int MF_E_PROPERTY_NOT_ALLOWED = unchecked((int)0xC00D3E8F);
        public const int MF_E_MEDIA_SOURCE_NOT_STARTED = unchecked((int)0xC00D3E91);
        public const int MF_E_UNSUPPORTED_FORMAT = unchecked((int)0xC00D3E98);
        public const int MF_E_MP3_BAD_CRC = unchecked((int)0xC00D3E99);
        public const int MF_E_NOT_PROTECTED = unchecked((int)0xC00D3E9A);
        public const int MF_E_MEDIA_SOURCE_WRONGSTATE = unchecked((int)0xC00D3E9B);
        public const int MF_E_MEDIA_SOURCE_NO_STREAMS_SELECTED = unchecked((int)0xC00D3E9C);
        public const int MF_E_CANNOT_FIND_KEYFRAME_SAMPLE = unchecked((int)0xC00D3E9D);

        public const int MF_E_UNSUPPORTED_CHARACTERISTICS = unchecked((int)0xC00D3E9E);
        public const int MF_E_NO_AUDIO_RECORDING_DEVICE   = unchecked((int)0xC00D3E9F);
        public const int MF_E_AUDIO_RECORDING_DEVICE_IN_USE = unchecked((int)0xC00D3EA0);
        public const int MF_E_AUDIO_RECORDING_DEVICE_INVALIDATED = unchecked((int)0xC00D3EA1);
        public const int MF_E_VIDEO_RECORDING_DEVICE_INVALIDATED = unchecked((int)0xC00D3EA2);
        public const int MF_E_VIDEO_RECORDING_DEVICE_PREEMPTED = unchecked((int)0xC00D3EA3);

        public const int MF_E_NETWORK_RESOURCE_FAILURE = unchecked((int)0xC00D4268);
        public const int MF_E_NET_WRITE = unchecked((int)0xC00D4269);
        public const int MF_E_NET_READ = unchecked((int)0xC00D426A);
        public const int MF_E_NET_REQUIRE_NETWORK = unchecked((int)0xC00D426B);
        public const int MF_E_NET_REQUIRE_ASYNC = unchecked((int)0xC00D426C);
        public const int MF_E_NET_BWLEVEL_NOT_SUPPORTED = unchecked((int)0xC00D426D);
        public const int MF_E_NET_STREAMGROUPS_NOT_SUPPORTED = unchecked((int)0xC00D426E);
        public const int MF_E_NET_MANUALSS_NOT_SUPPORTED = unchecked((int)0xC00D426F);
        public const int MF_E_NET_INVALID_PRESENTATION_DESCRIPTOR = unchecked((int)0xC00D4270);
        public const int MF_E_NET_CACHESTREAM_NOT_FOUND = unchecked((int)0xC00D4271);
        public const int MF_I_MANUAL_PROXY = unchecked((int)0x400D4272);
        public const int MF_E_NET_REQUIRE_INPUT = unchecked((int)0xC00D4274);
        public const int MF_E_NET_REDIRECT = unchecked((int)0xC00D4275);
        public const int MF_E_NET_REDIRECT_TO_PROXY = unchecked((int)0xC00D4276);
        public const int MF_E_NET_TOO_MANY_REDIRECTS = unchecked((int)0xC00D4277);
        public const int MF_E_NET_TIMEOUT = unchecked((int)0xC00D4278);
        public const int MF_E_NET_CLIENT_CLOSE = unchecked((int)0xC00D4279);
        public const int MF_E_NET_BAD_CONTROL_DATA = unchecked((int)0xC00D427A);
        public const int MF_E_NET_INCOMPATIBLE_SERVER = unchecked((int)0xC00D427B);
        public const int MF_E_NET_UNSAFE_URL = unchecked((int)0xC00D427C);
        public const int MF_E_NET_CACHE_NO_DATA = unchecked((int)0xC00D427D);
        public const int MF_E_NET_EOL = unchecked((int)0xC00D427E);
        public const int MF_E_NET_BAD_REQUEST = unchecked((int)0xC00D427F);
        public const int MF_E_NET_INTERNAL_SERVER_ERROR = unchecked((int)0xC00D4280);
        public const int MF_E_NET_SESSION_NOT_FOUND = unchecked((int)0xC00D4281);
        public const int MF_E_NET_NOCONNECTION = unchecked((int)0xC00D4282);
        public const int MF_E_NET_CONNECTION_FAILURE = unchecked((int)0xC00D4283);
        public const int MF_E_NET_INCOMPATIBLE_PUSHSERVER = unchecked((int)0xC00D4284);
        public const int MF_E_NET_SERVER_ACCESSDENIED = unchecked((int)0xC00D4285);
        public const int MF_E_NET_PROXY_ACCESSDENIED = unchecked((int)0xC00D4286);
        public const int MF_E_NET_CANNOTCONNECT = unchecked((int)0xC00D4287);
        public const int MF_E_NET_INVALID_PUSH_TEMPLATE = unchecked((int)0xC00D4288);
        public const int MF_E_NET_INVALID_PUSH_PUBLISHING_POINT = unchecked((int)0xC00D4289);
        public const int MF_E_NET_BUSY = unchecked((int)0xC00D428A);
        public const int MF_E_NET_RESOURCE_GONE = unchecked((int)0xC00D428B);
        public const int MF_E_NET_ERROR_FROM_PROXY = unchecked((int)0xC00D428C);
        public const int MF_E_NET_PROXY_TIMEOUT = unchecked((int)0xC00D428D);
        public const int MF_E_NET_SERVER_UNAVAILABLE = unchecked((int)0xC00D428E);
        public const int MF_E_NET_TOO_MUCH_DATA = unchecked((int)0xC00D428F);
        public const int MF_E_NET_SESSION_INVALID = unchecked((int)0xC00D4290);
        public const int MF_E_OFFLINE_MODE = unchecked((int)0xC00D4291);
        public const int MF_E_NET_UDP_BLOCKED = unchecked((int)0xC00D4292);
        public const int MF_E_NET_UNSUPPORTED_CONFIGURATION = unchecked((int)0xC00D4293);
        public const int MF_E_NET_PROTOCOL_DISABLED = unchecked((int)0xC00D4294);
        public const int MF_E_ALREADY_INITIALIZED = unchecked((int)0xC00D4650);
        public const int MF_E_BANDWIDTH_OVERRUN = unchecked((int)0xC00D4651);
        public const int MF_E_LATE_SAMPLE = unchecked((int)0xC00D4652);
        public const int MF_E_FLUSH_NEEDED = unchecked((int)0xC00D4653);
        public const int MF_E_INVALID_PROFILE = unchecked((int)0xC00D4654);
        public const int MF_E_INDEX_NOT_COMMITTED = unchecked((int)0xC00D4655);
        public const int MF_E_NO_INDEX = unchecked((int)0xC00D4656);
        public const int MF_E_CANNOT_INDEX_IN_PLACE = unchecked((int)0xC00D4657);
        public const int MF_E_MISSING_ASF_LEAKYBUCKET = unchecked((int)0xC00D4658);
        public const int MF_E_INVALID_ASF_STREAMID = unchecked((int)0xC00D4659);
        public const int MF_E_STREAMSINK_REMOVED = unchecked((int)0xC00D4A38);
        public const int MF_E_STREAMSINKS_OUT_OF_SYNC = unchecked((int)0xC00D4A3A);
        public const int MF_E_STREAMSINKS_FIXED = unchecked((int)0xC00D4A3B);
        public const int MF_E_STREAMSINK_EXISTS = unchecked((int)0xC00D4A3C);
        public const int MF_E_SAMPLEALLOCATOR_CANCELED = unchecked((int)0xC00D4A3D);
        public const int MF_E_SAMPLEALLOCATOR_EMPTY = unchecked((int)0xC00D4A3E);
        public const int MF_E_SINK_ALREADYSTOPPED = unchecked((int)0xC00D4A3F);
        public const int MF_E_ASF_FILESINK_BITRATE_UNKNOWN = unchecked((int)0xC00D4A40);
        public const int MF_E_SINK_NO_STREAMS = unchecked((int)0xC00D4A41);
        public const int MF_S_SINK_NOT_FINALIZED = unchecked((int)0x000D4A42);
        public const int MF_E_METADATA_TOO_LONG = unchecked((int)0xC00D4A43);
        public const int MF_E_SINK_NO_SAMPLES_PROCESSED = unchecked((int)0xC00D4A44);
        public const int MF_E_VIDEO_REN_NO_PROCAMP_HW = unchecked((int)0xC00D4E20);
        public const int MF_E_VIDEO_REN_NO_DEINTERLACE_HW = unchecked((int)0xC00D4E21);
        public const int MF_E_VIDEO_REN_COPYPROT_FAILED = unchecked((int)0xC00D4E22);
        public const int MF_E_VIDEO_REN_SURFACE_NOT_SHARED = unchecked((int)0xC00D4E23);
        public const int MF_E_VIDEO_DEVICE_LOCKED = unchecked((int)0xC00D4E24);
        public const int MF_E_NEW_VIDEO_DEVICE = unchecked((int)0xC00D4E25);
        public const int MF_E_NO_VIDEO_SAMPLE_AVAILABLE = unchecked((int)0xC00D4E26);
        public const int MF_E_NO_AUDIO_PLAYBACK_DEVICE = unchecked((int)0xC00D4E84);
        public const int MF_E_AUDIO_PLAYBACK_DEVICE_IN_USE = unchecked((int)0xC00D4E85);
        public const int MF_E_AUDIO_PLAYBACK_DEVICE_INVALIDATED = unchecked((int)0xC00D4E86);
        public const int MF_E_AUDIO_SERVICE_NOT_RUNNING = unchecked((int)0xC00D4E87);
        public const int MF_E_TOPO_INVALID_OPTIONAL_NODE = unchecked((int)0xC00D520E);
        public const int MF_E_TOPO_CANNOT_FIND_DECRYPTOR = unchecked((int)0xC00D5211);
        public const int MF_E_TOPO_CODEC_NOT_FOUND = unchecked((int)0xC00D5212);
        public const int MF_E_TOPO_CANNOT_CONNECT = unchecked((int)0xC00D5213);
        public const int MF_E_TOPO_UNSUPPORTED = unchecked((int)0xC00D5214);
        public const int MF_E_TOPO_INVALID_TIME_ATTRIBUTES = unchecked((int)0xC00D5215);
        public const int MF_E_TOPO_LOOPS_IN_TOPOLOGY = unchecked((int)0xC00D5216);
        public const int MF_E_TOPO_MISSING_PRESENTATION_DESCRIPTOR = unchecked((int)0xC00D5217);
        public const int MF_E_TOPO_MISSING_STREAM_DESCRIPTOR = unchecked((int)0xC00D5218);
        public const int MF_E_TOPO_STREAM_DESCRIPTOR_NOT_SELECTED = unchecked((int)0xC00D5219);
        public const int MF_E_TOPO_MISSING_SOURCE = unchecked((int)0xC00D521A);
        public const int MF_E_TOPO_SINK_ACTIVATES_UNSUPPORTED = unchecked((int)0xC00D521B);
        public const int MF_E_SEQUENCER_UNKNOWN_SEGMENT_ID = unchecked((int)0xC00D61AC);
        public const int MF_S_SEQUENCER_CONTEXT_CANCELED = unchecked((int)0x000D61AD);
        public const int MF_E_NO_SOURCE_IN_CACHE = unchecked((int)0xC00D61AE);
        public const int MF_S_SEQUENCER_SEGMENT_AT_END_OF_STREAM = unchecked((int)0x000D61AF);
        public const int MF_E_TRANSFORM_TYPE_NOT_SET = unchecked((int)0xC00D6D60);
        public const int MF_E_TRANSFORM_STREAM_CHANGE = unchecked((int)0xC00D6D61);
        public const int MF_E_TRANSFORM_INPUT_REMAINING = unchecked((int)0xC00D6D62);
        public const int MF_E_TRANSFORM_PROFILE_MISSING = unchecked((int)0xC00D6D63);
        public const int MF_E_TRANSFORM_PROFILE_INVALID_OR_CORRUPT = unchecked((int)0xC00D6D64);
        public const int MF_E_TRANSFORM_PROFILE_TRUNCATED = unchecked((int)0xC00D6D65);
        public const int MF_E_TRANSFORM_PROPERTY_PID_NOT_RECOGNIZED = unchecked((int)0xC00D6D66);
        public const int MF_E_TRANSFORM_PROPERTY_VARIANT_TYPE_WRONG = unchecked((int)0xC00D6D67);
        public const int MF_E_TRANSFORM_PROPERTY_NOT_WRITEABLE = unchecked((int)0xC00D6D68);
        public const int MF_E_TRANSFORM_PROPERTY_ARRAY_VALUE_WRONG_NUM_DIM = unchecked((int)0xC00D6D69);
        public const int MF_E_TRANSFORM_PROPERTY_VALUE_SIZE_WRONG = unchecked((int)0xC00D6D6A);
        public const int MF_E_TRANSFORM_PROPERTY_VALUE_OUT_OF_RANGE = unchecked((int)0xC00D6D6B);
        public const int MF_E_TRANSFORM_PROPERTY_VALUE_INCOMPATIBLE = unchecked((int)0xC00D6D6C);
        public const int MF_E_TRANSFORM_NOT_POSSIBLE_FOR_CURRENT_OUTPUT_MEDIATYPE = unchecked((int)0xC00D6D6D);
        public const int MF_E_TRANSFORM_NOT_POSSIBLE_FOR_CURRENT_INPUT_MEDIATYPE = unchecked((int)0xC00D6D6E);
        public const int MF_E_TRANSFORM_NOT_POSSIBLE_FOR_CURRENT_MEDIATYPE_COMBINATION = unchecked((int)0xC00D6D6F);
        public const int MF_E_TRANSFORM_CONFLICTS_WITH_OTHER_CURRENTLY_ENABLED_FEATURES = unchecked((int)0xC00D6D70);
        public const int MF_E_TRANSFORM_NEED_MORE_INPUT = unchecked((int)0xC00D6D72);
        public const int MF_E_TRANSFORM_NOT_POSSIBLE_FOR_CURRENT_SPKR_CONFIG = unchecked((int)0xC00D6D73);
        public const int MF_E_TRANSFORM_CANNOT_CHANGE_MEDIATYPE_WHILE_PROCESSING = unchecked((int)0xC00D6D74);
        public const int MF_S_TRANSFORM_DO_NOT_PROPAGATE_EVENT = unchecked((int)0x000D6D75);
        public const int MF_E_UNSUPPORTED_D3D_TYPE = unchecked((int)0xC00D6D76);
        public const int MF_E_TRANSFORM_ASYNC_LOCKED = unchecked((int)0xC00D6D77);
        public const int MF_E_TRANSFORM_CANNOT_INITIALIZE_ACM_DRIVER = unchecked((int)0xC00D6D78L);
        public const int MF_E_LICENSE_INCORRECT_RIGHTS = unchecked((int)0xC00D7148);
        public const int MF_E_LICENSE_OUTOFDATE = unchecked((int)0xC00D7149);
        public const int MF_E_LICENSE_REQUIRED = unchecked((int)0xC00D714A);
        public const int MF_E_DRM_HARDWARE_INCONSISTENT = unchecked((int)0xC00D714B);
        public const int MF_E_NO_CONTENT_PROTECTION_MANAGER = unchecked((int)0xC00D714C);
        public const int MF_E_LICENSE_RESTORE_NO_RIGHTS = unchecked((int)0xC00D714D);
        public const int MF_E_BACKUP_RESTRICTED_LICENSE = unchecked((int)0xC00D714E);
        public const int MF_E_LICENSE_RESTORE_NEEDS_INDIVIDUALIZATION = unchecked((int)0xC00D714F);
        public const int MF_S_PROTECTION_NOT_REQUIRED = unchecked((int)0x000D7150);
        public const int MF_E_COMPONENT_REVOKED = unchecked((int)0xC00D7151);
        public const int MF_E_TRUST_DISABLED = unchecked((int)0xC00D7152);
        public const int MF_E_WMDRMOTA_NO_ACTION = unchecked((int)0xC00D7153);
        public const int MF_E_WMDRMOTA_ACTION_ALREADY_SET = unchecked((int)0xC00D7154);
        public const int MF_E_WMDRMOTA_DRM_HEADER_NOT_AVAILABLE = unchecked((int)0xC00D7155);
        public const int MF_E_WMDRMOTA_DRM_ENCRYPTION_SCHEME_NOT_SUPPORTED = unchecked((int)0xC00D7156);
        public const int MF_E_WMDRMOTA_ACTION_MISMATCH = unchecked((int)0xC00D7157);
        public const int MF_E_WMDRMOTA_INVALID_POLICY = unchecked((int)0xC00D7158);
        public const int MF_E_POLICY_UNSUPPORTED = unchecked((int)0xC00D7159);
        public const int MF_E_OPL_NOT_SUPPORTED = unchecked((int)0xC00D715A);
        public const int MF_E_TOPOLOGY_VERIFICATION_FAILED = unchecked((int)0xC00D715B);
        public const int MF_E_SIGNATURE_VERIFICATION_FAILED = unchecked((int)0xC00D715C);
        public const int MF_E_DEBUGGING_NOT_ALLOWED = unchecked((int)0xC00D715D);
        public const int MF_E_CODE_EXPIRED = unchecked((int)0xC00D715E);
        public const int MF_E_GRL_VERSION_TOO_LOW = unchecked((int)0xC00D715F);
        public const int MF_E_GRL_RENEWAL_NOT_FOUND = unchecked((int)0xC00D7160);
        public const int MF_E_GRL_EXTENSIBLE_ENTRY_NOT_FOUND = unchecked((int)0xC00D7161);
        public const int MF_E_KERNEL_UNTRUSTED = unchecked((int)0xC00D7162);
        public const int MF_E_PEAUTH_UNTRUSTED = unchecked((int)0xC00D7163);
        public const int MF_E_NON_PE_PROCESS = unchecked((int)0xC00D7165);
        public const int MF_E_REBOOT_REQUIRED = unchecked((int)0xC00D7167);
        public const int MF_S_WAIT_FOR_POLICY_SET = unchecked((int)0x000D7168);
        public const int MF_S_VIDEO_DISABLED_WITH_UNKNOWN_SOFTWARE_OUTPUT = unchecked((int)0x000D7169);
        public const int MF_E_GRL_INVALID_FORMAT = unchecked((int)0xC00D716A);
        public const int MF_E_GRL_UNRECOGNIZED_FORMAT = unchecked((int)0xC00D716B);
        public const int MF_E_ALL_PROCESS_RESTART_REQUIRED = unchecked((int)0xC00D716C);
        public const int MF_E_PROCESS_RESTART_REQUIRED = unchecked((int)0xC00D716D);
        public const int MF_E_USERMODE_UNTRUSTED = unchecked((int)0xC00D716E);
        public const int MF_E_PEAUTH_SESSION_NOT_STARTED = unchecked((int)0xC00D716F);
        public const int MF_E_PEAUTH_PUBLICKEY_REVOKED = unchecked((int)0xC00D7171);
        public const int MF_E_GRL_ABSENT = unchecked((int)0xC00D7172);
        public const int MF_S_PE_TRUSTED = unchecked((int)0x000D7173);
        public const int MF_E_PE_UNTRUSTED = unchecked((int)0xC00D7174);
        public const int MF_E_PEAUTH_NOT_STARTED = unchecked((int)0xC00D7175);
        public const int MF_E_INCOMPATIBLE_SAMPLE_PROTECTION = unchecked((int)0xC00D7176);
        public const int MF_E_PE_SESSIONS_MAXED = unchecked((int)0xC00D7177);
        public const int MF_E_HIGH_SECURITY_LEVEL_CONTENT_NOT_ALLOWED = unchecked((int)0xC00D7178);
        public const int MF_E_TEST_SIGNED_COMPONENTS_NOT_ALLOWED = unchecked((int)0xC00D7179);
        public const int MF_E_ITA_UNSUPPORTED_ACTION = unchecked((int)0xC00D717A);
        public const int MF_E_ITA_ERROR_PARSING_SAP_PARAMETERS = unchecked((int)0xC00D717B);
        public const int MF_E_POLICY_MGR_ACTION_OUTOFBOUNDS = unchecked((int)0xC00D717C);
        public const int MF_E_BAD_OPL_STRUCTURE_FORMAT = unchecked((int)0xC00D717D);
        public const int MF_E_ITA_UNRECOGNIZED_ANALOG_VIDEO_PROTECTION_GUID = unchecked((int)0xC00D717E);
        public const int MF_E_NO_PMP_HOST = unchecked((int)0xC00D717F);
        public const int MF_E_ITA_OPL_DATA_NOT_INITIALIZED = unchecked((int)0xC00D7180);
        public const int MF_E_ITA_UNRECOGNIZED_ANALOG_VIDEO_OUTPUT = unchecked((int)0xC00D7181);
        public const int MF_E_ITA_UNRECOGNIZED_DIGITAL_VIDEO_OUTPUT = unchecked((int)0xC00D7182);

        public const int MF_E_RESOLUTION_REQUIRES_PMP_CREATION_CALLBACK = unchecked((int)0xC00D7183);
        public const int MF_E_INVALID_AKE_CHANNEL_PARAMETERS = unchecked((int)0xC00D7184);
        public const int MF_E_CONTENT_PROTECTION_SYSTEM_NOT_ENABLED = unchecked((int)0xC00D7185);
        public const int MF_E_UNSUPPORTED_CONTENT_PROTECTION_SYSTEM = unchecked((int)0xC00D7186);
        public const int MF_E_DRM_MIGRATION_NOT_SUPPORTED = unchecked((int)0xC00D7187);

        public const int MF_E_CLOCK_INVALID_CONTINUITY_KEY = unchecked((int)0xC00D9C40);
        public const int MF_E_CLOCK_NO_TIME_SOURCE = unchecked((int)0xC00D9C41);
        public const int MF_E_CLOCK_STATE_ALREADY_SET = unchecked((int)0xC00D9C42);
        public const int MF_E_CLOCK_NOT_SIMPLE = unchecked((int)0xC00D9C43);
        public const int MF_S_CLOCK_STOPPED = unchecked((int)0x000D9C44);
        public const int MF_E_NO_MORE_DROP_MODES = unchecked((int)0xC00DA028);
        public const int MF_E_NO_MORE_QUALITY_LEVELS = unchecked((int)0xC00DA029);
        public const int MF_E_DROPTIME_NOT_SUPPORTED = unchecked((int)0xC00DA02A);
        public const int MF_E_QUALITYKNOB_WAIT_LONGER = unchecked((int)0xC00DA02B);
        public const int MF_E_QM_INVALIDSTATE = unchecked((int)0xC00DA02C);
        public const int MF_E_TRANSCODE_NO_CONTAINERTYPE = unchecked((int)0xC00DA410);
        public const int MF_E_TRANSCODE_PROFILE_NO_MATCHING_STREAMS = unchecked((int)0xC00DA411);
        public const int MF_E_TRANSCODE_NO_MATCHING_ENCODER = unchecked((int)0xC00DA412);

        public const int MF_E_TRANSCODE_INVALID_PROFILE = unchecked((int)0xC00DA413);

        public const int MF_E_ALLOCATOR_NOT_INITIALIZED = unchecked((int)0xC00DA7F8);
        public const int MF_E_ALLOCATOR_NOT_COMMITED = unchecked((int)0xC00DA7F9);
        public const int MF_E_ALLOCATOR_ALREADY_COMMITED = unchecked((int)0xC00DA7FA);
        public const int MF_E_STREAM_ERROR = unchecked((int)0xC00DA7FB);
        public const int MF_E_INVALID_STREAM_STATE = unchecked((int)0xC00DA7FC);
        public const int MF_E_HW_STREAM_NOT_CONNECTED = unchecked((int)0xC00DA7FD);

        public const int MF_E_NO_CAPTURE_DEVICES_AVAILABLE = unchecked((int)0xC00DABE0);
        public const int MF_E_CAPTURE_SINK_OUTPUT_NOT_SET = unchecked((int)0xC00DABE1);
        public const int MF_E_CAPTURE_SINK_MIRROR_ERROR = unchecked((int)0xC00DABE2);
        public const int MF_E_CAPTURE_SINK_ROTATE_ERROR = unchecked((int)0xC00DABE3);
        public const int MF_E_CAPTURE_ENGINE_INVALID_OP = unchecked((int)0xC00DABE4);

        public const int MF_E_DXGI_DEVICE_NOT_INITIALIZED = unchecked((int)0x80041000);
        public const int MF_E_DXGI_NEW_VIDEO_DEVICE = unchecked((int)0x80041001);
        public const int MF_E_DXGI_VIDEO_DEVICE_LOCKED = unchecked((int)0x80041002);

        #endregion

        #region externs

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, EntryPoint = "FormatMessageW", ExactSpelling = true), SuppressUnmanagedCodeSecurity]
        private static extern int FormatMessage(FormatMessageFlags dwFlags, IntPtr lpSource,
            int dwMessageId, int dwLanguageId, out IntPtr lpBuffer, int nSize, IntPtr[] Arguments);

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, EntryPoint = "LoadLibraryExW", ExactSpelling = true), SuppressUnmanagedCodeSecurity]
        private static extern IntPtr LoadLibraryEx(string lpFileName, IntPtr hFile, LoadLibraryExFlags dwFlags);

        [DllImport("kernel32.dll", ExactSpelling = true), SuppressUnmanagedCodeSecurity]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool FreeLibrary(IntPtr hFile);

        [DllImport("kernel32.dll", ExactSpelling = true), SuppressUnmanagedCodeSecurity]
        private static extern IntPtr LocalFree(IntPtr hMem);

        #endregion

        #region Declarations

        [Flags, UnmanagedName("#defines in WinBase.h")]
        private enum LoadLibraryExFlags
        {
            DontResolveDllReferences = 0x00000001,
            LoadLibraryAsDataFile = 0x00000002,
            LoadWithAlteredSearchPath = 0x00000008,
            LoadIgnoreCodeAuthzLevel = 0x00000010
        }

        [Flags, UnmanagedName("FORMAT_MESSAGE_* defines")]
        private enum FormatMessageFlags
        {
            AllocateBuffer = 0x00000100,
            IgnoreInserts = 0x00000200,
            FromString = 0x00000400,
            FromHmodule = 0x00000800,
            FromSystem = 0x00001000,
            ArgumentArray = 0x00002000,
            MaxWidthMask = 0x000000FF
        }

        #endregion

        private static IntPtr s_hModule = IntPtr.Zero;
        private const string MESSAGEFILE = "mferror.dll";

        /// <summary>
        /// Returns a string describing a MF error.  Works for both error codes
        /// (values < 0) and Status codes (values >= 0)
        /// </summary>
        /// <param name="hr">HRESULT for which to get description</param>
        /// <returns>The string, or null if no error text can be found</returns>
        public static string GetErrorText(int hr)
        {
            string sRet = null;
            int dwBufferLength;
            IntPtr ip = IntPtr.Zero;

            FormatMessageFlags dwFormatFlags =
                FormatMessageFlags.AllocateBuffer |
                FormatMessageFlags.IgnoreInserts |
                FormatMessageFlags.FromSystem |
                FormatMessageFlags.MaxWidthMask;

            // Scan both the Windows Media library, and the system library looking for the message
            dwBufferLength = FormatMessage(
                dwFormatFlags,
                s_hModule, // module to get message from (NULL == system)
                hr, // error number to get message for
                0, // default language
                out ip,
                0,
                null
                );

            // Not a system message.  In theory, you should be able to get both with one call.  In practice (at
            // least on my 64bit box), you need to make 2 calls.
            if (dwBufferLength == 0)
            {
                if (s_hModule == IntPtr.Zero)
                {
                    // Load the Media Foundation error message dll
                    s_hModule = LoadLibraryEx(MESSAGEFILE, IntPtr.Zero, LoadLibraryExFlags.LoadLibraryAsDataFile);
                }

                if (s_hModule != IntPtr.Zero)
                {
                    // If the load succeeds, make sure we look in it
                    dwFormatFlags |= FormatMessageFlags.FromHmodule;

                    // Scan both the Windows Media library, and the system library looking for the message
                    dwBufferLength = FormatMessage(
                        dwFormatFlags,
                        s_hModule, // module to get message from (NULL == system)
                        hr, // error number to get message for
                        0, // default language
                        out ip,
                        0,
                        null
                        );
                }
            }

            try
            {
                // Convert the returned buffer to a string.  If ip is null (due to not finding
                // the message), no exception is thrown.  sRet just stays null.  The
                // try/finally is for the (remote) possibility that we run out of memory
                // creating the string.
                sRet = Marshal.PtrToStringUni(ip);
            }
            finally
            {
                // Cleanup
                if (ip != IntPtr.Zero)
                {
                    LocalFree(ip);
                }
            }

            return sRet;
        }

        /// <summary>
        /// If hr has a "failed" status code (E_*), throw an exception.  Note that status
        /// messages (S_*) are not considered failure codes.  If MediaFoundation error text
        /// is available, it is used to build the exception, otherwise a generic com error
        /// is thrown.
        /// </summary>
        /// <param name="hr">The HRESULT to check</param>
        public static void ThrowExceptionForHR(int hr)
        {
            // If a severe error has occurred
            if (hr < 0)
            {
                string s = GetErrorText(hr);

                // If a string is returned, build a COM error from it
                if (s != null)
                {
                    throw new COMException(s, hr);
                }
                else
                {
                    // No string, just use standard com error
                    Marshal.ThrowExceptionForHR(hr);
                }
            }
        }
    }

    abstract public class COMBase
    {
        public const int S_Ok = 0;
        public const int S_False = 1;

        public const int E_NotImplemented = unchecked((int)0x80004001);
        public const int E_NoInterface = unchecked((int)0x80004002);
        public const int E_Pointer = unchecked((int)0x80004003);
        public const int E_Abort = unchecked((int)0x80004004);
        public const int E_Fail = unchecked((int)0x80004005);
        public const int E_Unexpected = unchecked((int)0x8000FFFF);
        public const int E_OutOfMemory = unchecked((int)0x8007000E);
        public const int E_InvalidArgument = unchecked((int)0x80070057);
        public const int E_BufferTooSmall = unchecked((int)0x8007007a);

        public static bool Succeeded(int hr)
        {
            return hr >= 0;
        }

        public static bool Failed(int hr)
        {
            return hr < 0;
        }

        public static void SafeRelease(object o)
        {
            if (o != null)
            {
                if (Marshal.IsComObject(o))
                {
                    int i = Marshal.ReleaseComObject(o);
                    Debug.Assert(i >= 0);
                }
                else
                {
                    IDisposable iDis = o as IDisposable;
                    if (iDis != null)
                    {
                        iDis.Dispose();
                    }
                    else
                    {
                        throw new Exception("What the heck was that?");
                    }
                }
            }
        }

        public static void TRACE(string s)
        {
            Debug.WriteLine(s);
        }
    }

    #endregion

    #region Internal classes

    // These classes are used internally and there is probably no reason you will ever
    // need to use them directly.

    // Class to release PropVariants on parameters that output PropVariants.  There
    // should be no reason for code to call this class directly.  It is invoked
    // automatically when the appropriate methods are called.
    internal class PVMarshaler : ICustomMarshaler
    {
        // The managed object passed in to MarshalManagedToNative
        protected PropVariant m_prop;

        public IntPtr MarshalManagedToNative(object managedObj)
        {
            IntPtr p;

            // Cast the object back to a PropVariant
            m_prop = managedObj as PropVariant;

            if (m_prop != null)
            {
                // Release any memory currently allocated
                m_prop.Clear();

                // Create an appropriately sized buffer, blank it, and send it to
                // the marshaler to make the COM call with.
                int iSize = GetNativeDataSize();
                p = Marshal.AllocCoTaskMem(iSize);

                if (IntPtr.Size == 4)
                {
                    Marshal.WriteInt64(p, 0);
                    Marshal.WriteInt64(p, 8, 0);
                }
                else
                {
                    Marshal.WriteInt64(p, 0);
                    Marshal.WriteInt64(p, 8, 0);
                    Marshal.WriteInt64(p, 16, 0);
                }
            }
            else
            {
                p = IntPtr.Zero;
            }

            return p;
        }

        // Called just after invoking the COM method.  The IntPtr is the same one that just got returned
        // from MarshalManagedToNative.  The return value is unused.
        public object MarshalNativeToManaged(IntPtr pNativeData)
        {
            Marshal.PtrToStructure(pNativeData, m_prop);
            m_prop = null;

            return m_prop;
        }

        public void CleanUpManagedData(object ManagedObj)
        {
            m_prop = null;
        }

        public void CleanUpNativeData(IntPtr pNativeData)
        {
            Marshal.FreeCoTaskMem(pNativeData);
        }

        // The number of bytes to marshal out
        public int GetNativeDataSize()
        {
            return Marshal.SizeOf(typeof(PropVariant));
        }

        // This method is called by interop to create the custom marshaler.  The (optional)
        // cookie is the value specified in MarshalCookie="asdf", or "" is none is specified.
        public static ICustomMarshaler GetInstance(string cookie)
        {
            return new PVMarshaler();
        }
    }

    // Used by MFTGetInfo
    internal class RTIMarshaler : ICustomMarshaler
    {
        private ArrayList m_array;
        private MFInt m_int;
        private IntPtr m_MFIntPtr;
        private IntPtr m_ArrayPtr;

        public IntPtr MarshalManagedToNative(object managedObj)
        {
            IntPtr p;

            // We get called twice: Once for the MFInt, and once for the array.
            // Figure out which call this is.
            if (managedObj is MFInt)
            {
                // Save off the object.  We'll need to use Assign() on this later.
                m_int = managedObj as MFInt;

                // Allocate room for the int
                p = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(MFInt)));
                m_MFIntPtr = p;
            }
            else
            {
                // Save off the object.  We'll be calling methods on this in
                // MarshalNativeToManaged.
                m_array = managedObj as ArrayList;

                if (m_array != null)
                {
                    m_array.Clear();
                }

                // All we need is room for the pointer
                p = Marshal.AllocCoTaskMem(IntPtr.Size);

                // Belt-and-suspenders.  Set this to null.
                Marshal.WriteIntPtr(p, IntPtr.Zero);
                m_ArrayPtr = p;
            }

            return p;
        }

        // Called just after invoking the COM method.  The IntPtr is the same one that just got returned
        // from MarshalManagedToNative.  The return value is unused.
        public object MarshalNativeToManaged(IntPtr pNativeData)
        {
            // When we are called with pNativeData == m_ArrayPtr, do nothing.  All the
            // work is done when:
            if (pNativeData == m_MFIntPtr)
            {
                // Read the count
                int count = Marshal.ReadInt32(pNativeData);

                // If we have an array to return things in (ie MFTGetInfo wasn't passed
                // nulls)
                if (m_array != null)
                {
                    IntPtr ip2 = Marshal.ReadIntPtr(m_ArrayPtr);

                    // I don't know why this might happen, but it seems worth the check
                    if (ip2 != IntPtr.Zero)
                    {
                        try
                        {
                            int iSize = Marshal.SizeOf(typeof(MFTRegisterTypeInfo));

                            // Size the array
                            m_array.Capacity = count;

                            // Copy in the values
                            for (int x = 0; x < count; x++)
                            {
                                MFTRegisterTypeInfo rti = new MFTRegisterTypeInfo();
                                Marshal.PtrToStructure(new IntPtr(ip2.ToInt64() + (x * iSize)), rti);
                                m_array.Add(rti);
                            }
                        }
                        finally
                        {
                            // Free the array we got back
                            Marshal.FreeCoTaskMem(ip2);
                        }
                    }
                }

                // Don't forget to assign the value
                m_int.Assign(count);

                m_int = null;
                m_array = null;
            }

            // This value isn't actually used
            return null;
        }

        public void CleanUpManagedData(object ManagedObj)
        {
        }

        public void CleanUpNativeData(IntPtr pNativeData)
        {
            Marshal.FreeCoTaskMem(pNativeData);
        }

        // The number of bytes to marshal out
        public int GetNativeDataSize()
        {
            return -1;
        }

        // When used with MFTGetInfo, there are 2 parameter pairs (ppInputTypes + pcInputTypes,
        // ppOutputTypes + pcOutputTypes).  Each need their own instance
        static RTIMarshaler[] s_rti = new RTIMarshaler[2];

        // This method is called by interop to create the custom marshaler.  The (optional)
        // cookie is the value specified in MarshalCookie="asdf", or "" is none is specified.
        public static ICustomMarshaler GetInstance(string cookie)
        {
            // Probably not an issue, but just to be safe
            lock (s_rti)
            {
                if (s_rti[0] == null)
                {
                    s_rti[0] = new RTIMarshaler();
                    s_rti[1] = new RTIMarshaler();
                }
            }

            int i = Convert.ToInt32(cookie);
            return s_rti[i];
        }
    }

    // Used by MFTRegister
    internal class RTAMarshaler : ICustomMarshaler
    {
        public IntPtr MarshalManagedToNative(object managedObj)
        {
            IntPtr p;

            int iSize = Marshal.SizeOf(typeof(MFTRegisterTypeInfo));

            // Save off the object.  We'll be calling methods on this in
            // MarshalNativeToManaged.
            MFTRegisterTypeInfo[] array = managedObj as MFTRegisterTypeInfo[];

            // All we need is room for the pointer
            p = Marshal.AllocCoTaskMem(array.Length * iSize);

            for (int x = 0; x < array.Length; x++)
            {
                Marshal.StructureToPtr(array[x], new IntPtr(p.ToInt64() + (x * iSize)), false);
            }

            return p;
        }

        public object MarshalNativeToManaged(IntPtr pNativeData)
        {
            // This value isn't actually used
            return null;
        }

        public void CleanUpManagedData(object ManagedObj)
        {
        }

        public void CleanUpNativeData(IntPtr pNativeData)
        {
            Marshal.FreeCoTaskMem(pNativeData);
        }

        // The number of bytes to marshal out
        public int GetNativeDataSize()
        {
            return -1;
        }

        // This method is called by interop to create the custom marshaler.  The (optional)
        // cookie is the value specified in MarshalCookie="asdf", or "" is none is specified.
        public static ICustomMarshaler GetInstance(string cookie)
        {
            return new RTAMarshaler();
        }
    }

    // Used by MFTEnum
    internal class GAMarshaler : ICustomMarshaler
    {
        private ArrayList m_array;
        private MFInt m_int;
        private IntPtr m_MFIntPtr;
        private IntPtr m_ArrayPtr;

        public IntPtr MarshalManagedToNative(object managedObj)
        {
            IntPtr p;

            // We get called twice: Once for the MFInt, and once for the array.
            // Figure out which call this is.
            if (managedObj is MFInt)
            {
                // Save off the object.  We'll need to use Assign() on this later.
                m_int = managedObj as MFInt;

                // Allocate room for the int
                p = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(MFInt)));
                m_MFIntPtr = p;
            }
            else
            {
                // Save off the object.  We'll be calling methods on this in
                // MarshalNativeToManaged.
                m_array = managedObj as ArrayList;

                if (m_array != null)
                {
                    m_array.Clear();
                }

                // All we need is room for the pointer
                p = Marshal.AllocCoTaskMem(IntPtr.Size);

                // Belt-and-suspenders.  Set this to null.
                Marshal.WriteIntPtr(p, IntPtr.Zero);
                m_ArrayPtr = p;
            }

            return p;
        }

        // Called just after invoking the COM method.  The IntPtr is the same one that just got returned
        // from MarshalManagedToNative.  The return value is unused.
        public object MarshalNativeToManaged(IntPtr pNativeData)
        {
            // When we are called with pNativeData == m_ArrayPtr, do nothing.  All the
            // work is done when:
            if (pNativeData == m_MFIntPtr)
            {
                // Read the count
                int count = Marshal.ReadInt32(pNativeData);

                // If we have an array to return things in (ie MFTGetInfo wasn't passed
                // nulls)
                if (m_array != null)
                {
                    IntPtr ip2 = Marshal.ReadIntPtr(m_ArrayPtr);

                    // I don't know why this might happen, but it seems worth the check
                    if (ip2 != IntPtr.Zero)
                    {
                        try
                        {
                            int iSize = Marshal.SizeOf(typeof(Guid));
                            // Size the array
                            m_array.Capacity = count;
                            byte[] b = new byte[iSize];

                            // Copy in the values
                            for (int x = 0; x < count; x++)
                            {
                                Marshal.Copy(new IntPtr(ip2.ToInt64() + (x * iSize)), b, 0, iSize);
                                m_array.Add(new Guid(b));
                            }
                        }
                        finally
                        {
                            // Free the array we got back
                            Marshal.FreeCoTaskMem(ip2);
                        }
                    }
                }

                // Don't forget to assign the value
                m_int.Assign(count);

                m_array = null;
                m_int = null;
            }

            // This value isn't actually used
            return null;
        }

        public void CleanUpManagedData(object ManagedObj)
        {
        }

        public void CleanUpNativeData(IntPtr pNativeData)
        {
            Marshal.FreeCoTaskMem(pNativeData);
        }

        // The number of bytes to marshal out
        public int GetNativeDataSize()
        {
            return -1;
        }

        // This method is called by interop to create the custom marshaler.  The (optional)
        // cookie is the value specified in MarshalCookie="asdf", or "" is none is specified.
        public static ICustomMarshaler GetInstance(string cookie)
        {
            return new GAMarshaler();
        }
    }

    // Class to handle WAVEFORMATEXTENSIBLE
    internal class WEMarshaler : ICustomMarshaler
    {
        public IntPtr MarshalManagedToNative(object managedObj)
        {
            WaveFormatEx wfe = managedObj as WaveFormatEx;

            IntPtr ip = wfe.GetPtr();

            return ip;
        }

        // Called just after invoking the COM method.  The IntPtr is the same one that just got returned
        // from MarshalManagedToNative.  The return value is unused.
        public object MarshalNativeToManaged(IntPtr pNativeData)
        {
            WaveFormatEx wfe = WaveFormatEx.PtrToWave(pNativeData);

            return wfe;
        }

        public void CleanUpManagedData(object ManagedObj)
        {
        }

        public void CleanUpNativeData(IntPtr pNativeData)
        {
            Marshal.FreeCoTaskMem(pNativeData);
        }

        // The number of bytes to marshal out - never called
        public int GetNativeDataSize()
        {
            return -1;
        }

        // This method is called by interop to create the custom marshaler.  The (optional)
        // cookie is the value specified in MarshalCookie="asdf", or "" is none is specified.
        public static ICustomMarshaler GetInstance(string cookie)
        {
            return new WEMarshaler();
        }
    }

    // Class to handle BITMAPINFO
    internal class BMMarshaler : ICustomMarshaler
    {
        protected BitmapInfoHeader m_bmi;

        public IntPtr MarshalManagedToNative(object managedObj)
        {
            m_bmi = managedObj as BitmapInfoHeader;

            IntPtr ip = m_bmi.GetPtr();

            return ip;
        }

        // Called just after invoking the COM method.  The IntPtr is the same one that just got returned
        // from MarshalManagedToNative.  The return value is unused.
        public object MarshalNativeToManaged(IntPtr pNativeData)
        {
            BitmapInfoHeader bmi = BitmapInfoHeader.PtrToBMI(pNativeData);

            // If we this call is In+Out, the return value is ignored.  If
            // this is out, then m_bmi will be null.
            if (m_bmi != null)
            {
                m_bmi.CopyFrom(bmi);
                bmi = null;
            }

            return bmi;
        }

        public void CleanUpManagedData(object ManagedObj)
        {
            m_bmi = null;
        }

        public void CleanUpNativeData(IntPtr pNativeData)
        {
            Marshal.FreeCoTaskMem(pNativeData);
        }

        // The number of bytes to marshal out - never called
        public int GetNativeDataSize()
        {
            return -1;
        }

        // This method is called by interop to create the custom marshaler.  The (optional)
        // cookie is the value specified in MarshalCookie="asdf", or "" is none is specified.
        public static ICustomMarshaler GetInstance(string cookie)
        {
            return new BMMarshaler();
        }
    }

    // Class to handle Array of Guid
    internal class GMarshaler : ICustomMarshaler
    {
        protected Guid[] m_Obj;
        protected IntPtr m_ip;

        public IntPtr MarshalManagedToNative(object managedObj)
        {
            if (m_ip == IntPtr.Zero)
            {
                // If we are being called first from managed

                m_Obj = (Guid[])managedObj;
                // Freed in CleanUpManagedData
                m_ip = Marshal.AllocCoTaskMem(IntPtr.Size);
            }
            else
            {
                // Return the value to native
                Guid [] mo = (Guid [])managedObj;

                IntPtr ip = Marshal.AllocCoTaskMem(16 * mo.Length);
                Marshal.WriteIntPtr(m_ip, ip);

                for (int x = 0; (mo[x] != Guid.Empty) && (x < mo.Length); x++)
                {
                    Marshal.StructureToPtr(mo[x], ip, false);
                    ip = new IntPtr(ip.ToInt64() + 16);
                }
            }
            return m_ip;
        }

        public object MarshalNativeToManaged(IntPtr pNativeData)
        {
            if (m_Obj != null)
            {
                // Return the value to managed
                byte[] b = new byte[16];
                IntPtr pBuff = Marshal.ReadIntPtr(pNativeData);
                for (int x = 0; x < m_Obj.Length; x++)
                {
                    Marshal.Copy(pBuff, b, 0, 16);
                    m_Obj[x] = new Guid(b);

                    pBuff = new IntPtr(pBuff.ToInt64() + 16);
                }

                Marshal.FreeCoTaskMem(Marshal.ReadIntPtr(pNativeData));
            }
            else
            {
                // If we are being called first from native
                m_ip = pNativeData;
                return new Guid[30];
            }

            return null;
        }

        public void CleanUpManagedData(object ManagedObj)
        {
            m_Obj = null;
        }

        public void CleanUpNativeData(IntPtr pNativeData)
        {
            Marshal.FreeCoTaskMem(pNativeData);
        }

        // The number of bytes to marshal out - never called
        public int GetNativeDataSize()
        {
            return -1;
        }

        // This method is called by interop to create the custom marshaler.  The (optional)
        // cookie is the value specified in MarshalCookie="asdf", or "" is none is specified.
        public static ICustomMarshaler GetInstance(string cookie)
        {
            return new GMarshaler();
        }
    }

    [AttributeUsage(AttributeTargets.Enum | AttributeTargets.Struct | AttributeTargets.Class)]
    public class UnmanagedNameAttribute : System.Attribute
    {
        private string m_Name;

        public UnmanagedNameAttribute(string s)
        {
            m_Name = s;
        }

        public override string ToString()
        {
            return m_Name;
        }
    }

    #endregion
}
