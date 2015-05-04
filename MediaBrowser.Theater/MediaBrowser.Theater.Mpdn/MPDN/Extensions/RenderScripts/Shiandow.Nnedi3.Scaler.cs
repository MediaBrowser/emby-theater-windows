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
using Mpdn.RenderScript.Shiandow.NNedi3.Filters;
using SharpDX;

namespace Mpdn.RenderScript
{
    namespace Shiandow.NNedi3
    {
        public class NNedi3 : RenderChain
        {
            #region Settings

            public NNedi3()
            {
                Neurons = NNedi3Neurons.Neurons16;
                CodePath = NNedi3Path.ScalarMad;
                ForceCentered = false;
            }

            public NNedi3Neurons Neurons { get; set; }
            public NNedi3Path CodePath { get; set; }
            public bool ForceCentered { get; set; }

            #endregion

            private static readonly int[] s_NeuronCount = {16, 32, 64, 128, 256};
            private static readonly string[] s_CodePath = {"A", "B", "C", "D", "E"};

            public override IFilter CreateFilter(IFilter input)
            {
                if (!Renderer.IsDx11Avail)
                    return input; // DX11 is not available; fallback

                Func<TextureSize, TextureSize> Transformation = s => new TextureSize(2 * s.Height, s.Width);

                var NNEDI3 = LoadShader11(string.Format("NNEDI3_{0}_{1}.cso", s_NeuronCount[(int) Neurons], s_CodePath[(int) CodePath]));
                var Interleave = CompileShader("Interleave.hlsl").Configure(transform: Transformation);
                var Combine = CompileShader("Combine.hlsl").Configure(transform: Transformation);

                var sourceSize = input.OutputSize;
                if (!IsUpscalingFrom(sourceSize))
                    return input;

                var yuv = input.ConvertToYuv();

                var chroma = new ResizeFilter(yuv, new TextureSize(sourceSize.Width*2, sourceSize.Height*2),
                    TextureChannels.ChromaOnly, new Vector2(-0.25f, -0.25f), Renderer.ChromaUpscaler, Renderer.ChromaDownscaler);
                
                IFilter resultY, result;

                var pass1 = NNedi3Helpers.CreateFilter(NNEDI3, yuv, Neurons);
                resultY = new ShaderFilter(Interleave, yuv, pass1);
                var pass2 = NNedi3Helpers.CreateFilter(NNEDI3, resultY, Neurons);
                result = new ShaderFilter(Combine, resultY, pass2, chroma);

                return new ResizeFilter(result.ConvertToRgb(), result.OutputSize, new Vector2(0.5f, 0.5f),
                    Renderer.LumaUpscaler, Renderer.LumaDownscaler, ForceCentered ? Renderer.LumaUpscaler : null);
            }
        }

        public class NNedi3Scaler : RenderChainUi<NNedi3, NNedi3ConfigDialog>
        {
            protected override string ConfigFileName
            {
                get { return "Shiandow.NNedi3"; }
            }

            public override string Category
            {
                get { return "Upscaling"; }
            }

            public override ExtensionUiDescriptor Descriptor
            {
                get
                {
                    return new ExtensionUiDescriptor
                    {
                        Guid = new Guid("B210A4E6-E3F9-4FEE-9840-5D6EDB0BFE05"),
                        Name = "NNEDI3",
                        Description = "NNEDI3 image doubler",
                        Copyright = "Adapted by Shiandow for MPDN"
                    };
                }
            }
        }
    }
}
