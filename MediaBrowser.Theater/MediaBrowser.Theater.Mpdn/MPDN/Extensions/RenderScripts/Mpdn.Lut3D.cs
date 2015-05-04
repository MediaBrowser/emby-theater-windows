// This file is a part of MPDN Extensions.
// https://github.com/zachsaw/MPDN_Extensions
//
// This library is free software; you can redistribute it and/or
// modify it under the terms of the GNU Lesser General Public
// License as published by the Free Software Foundation; either
// version 3.0 of the License, or (at your option) any later version.
// 
// This library is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
// Lesser General Public License for more details.
// 
// You should have received a copy of the GNU Lesser General Public
// License along with this library.
// 
using System;
using System.IO;
using System.Runtime.InteropServices;
using SharpDX;

namespace Mpdn.RenderScript
{
    namespace Mpdn.Lut3D
    {
        [StructLayout(LayoutKind.Sequential)]
        public struct Lut3DHeader
        {
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)] public char[] Signature;
                // file signature; must be: '3DLT'

            public int FileVersion; // file format version number (currently "1")

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)] public char[] ProgramName;
                // name of the program that created the file

            public long ProgramVersion; // version number of the program that created the file

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)] public int[] InputBitDepth;
                // input bit depth per component (Y,Cb,Cr or R,G,B)

            public int InputColorEncoding; // input color encoding standard
            public int OutputBitDepth; // output bit depth for all components (valid values are 8, 16 and 32)
            public int OutputColorEncoding; // output color encoding standard

            public int ParametersFileOffset;
                // number of bytes between the beginning of the file and array parametersData

            public int ParametersSize; // size in bytes of the array parametersData
            public int LutFileOffset; // number of bytes between the beginning of the file and array lutData
            public int LutCompressionMethod; // type of compression used if any (0 = none, ...)

            public int LutCompressedSize;
                // size in bytes of the array lutData inside the file, whether compressed or not

            public int LutUncompressedSize;
                // true size in bytes of the array lutData when in memory for usage (outside the file)

            // This header is followed by the char array 'parametersData', of length 'parametersSize',
            // and by the array 'lutDataxx', of length 'lutCompressedSize'.
        };

        public class Lut3D : RenderChain
        {
            #region Settings

            public bool Activate { get; set; }
            public string FileName { get; set; }

            #endregion

            private ITexture3D m_Texture3D;

            protected override string ShaderPath
            {
                get { return "Lut3D"; }
            }

            public override IFilter CreateFilter(IFilter input)
            {
                if (!Activate || !File.Exists(FileName))
                    return input;

                Create3DTexture();
                var shader = CompileShader("Lut3D.hlsl").Configure(linearSampling : true);
                return new ShaderFilter(shader, input, new Texture3DSourceFilter(m_Texture3D));
            }

            public override void RenderScriptDisposed()
            {
                DiscardTextures();

                base.RenderScriptDisposed();
            }

            private void DiscardTextures()
            {
                DisposeHelper.Dispose(ref m_Texture3D);
            }

            private void Create3DTexture()
            {
                if (m_Texture3D != null)
                    return;

                Create3DLut(FileName);
            }

            private void Create3DLut(string fileName)
            {
                using (var sr = File.OpenRead(fileName))
                {
                    byte[] lutBuffer;
                    var header = Load3DLut(sr, out lutBuffer);
                    Upload3DLut(header, lutBuffer);
                }
            }

            private unsafe void Upload3DLut(Lut3DHeader header, byte[] lutBuffer)
            {
                int inputBitsR = header.InputBitDepth[2];
                int inputBitsG = header.InputBitDepth[1];
                int inputBitsB = header.InputBitDepth[0];

                int rSize = 1 << inputBitsR;
                int gSize = 1 << inputBitsG;
                int bSize = 1 << inputBitsB;

                const int channelCount = 4;
                var data = new ushort[bSize, gSize, rSize*channelCount];

                fixed (void* lutByte = lutBuffer)
                {
                    var lut = (ushort*) lutByte;
                    for (int b = 0; b < bSize; b++)
                    for (int g = 0; g < gSize; g++)
                    for (int r = 0; r < rSize; r++)
                    {
                        var lutOffset = ((r << (inputBitsG + inputBitsB)) + (g << inputBitsB) + b)*3;
                        var max = (1 << header.OutputBitDepth) - 1;
                        var n = ushort.MaxValue/max;

                        data[b, g, r*channelCount + 0] = (ushort) (lut[lutOffset + 2]*n);
                        data[b, g, r*channelCount + 1] = (ushort) (lut[lutOffset + 1]*n);
                        data[b, g, r*channelCount + 2] = (ushort) (lut[lutOffset + 0]*n);
                        data[b, g, r*channelCount + 3] = ushort.MaxValue;
                    }
                }

                m_Texture3D = Renderer.CreateTexture3D(bSize, gSize, rSize, TextureFormat.Unorm16);
                Renderer.UpdateTexture3D(m_Texture3D, data);
            }

            private static Lut3DHeader Load3DLut(FileStream sr, out byte[] lutBuffer)
            {
                var headerLength = Marshal.SizeOf(typeof (Lut3DHeader));
                var buffer = new byte[headerLength];
                sr.Read(buffer, 0, headerLength);
                var header = BytesToStruct<Lut3DHeader>(ref buffer);
                sr.Seek(header.LutFileOffset, SeekOrigin.Begin);
                lutBuffer = new byte[header.LutUncompressedSize];
                if (sr.Read(lutBuffer, 0, lutBuffer.Length) != lutBuffer.Length)
                {
                    throw new FileLoadException("Unexpected EOF!");
                }
                return header;
            }

            private static T BytesToStruct<T>(ref byte[] rawData) where T : struct
            {
                T result;
                var handle = GCHandle.Alloc(rawData, GCHandleType.Pinned);
                try
                {
                    var rawDataPtr = handle.AddrOfPinnedObject();
                    result = (T) Marshal.PtrToStructure(rawDataPtr, typeof (T));
                }
                finally
                {
                    handle.Free();
                }
                return result;
            }
        }

        public class Lut3DUi : RenderChainUi<Lut3D, Lut3DConfigDialog>
        {
            public override string Category
            {
                get { return "Processing"; }
            }

            public override ExtensionUiDescriptor Descriptor
            {
                get
                {
                    return new ExtensionUiDescriptor
                    {
                        Name = "3DLut",
                        Description = "Output correction via 3D LUT",
                        Guid = new Guid("A8080688-A04D-4E46-BCCD-F6FBA4EC9326"),
                        Copyright = "" // Optional field
                    };
                }
            }
        }
    }
}
