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
using YAXLib;
using SharpDX;

namespace Mpdn.RenderScript
{
    namespace Shiandow.SuperRes
    {
        public class SuperChromaRes : RenderChain
        {
            #region Settings

            public int Passes { get; set; }

            public float Strength { get; set; }
            public float Sharpness { get; set; }
            public float AntiAliasing { get; set; }
            public float AntiRinging { get; set; }
            public float Softness { get; set; }

            public bool FirstPassOnly;

            #endregion

            private IScaler upscaler, downscaler;

            public SuperChromaRes()
            {
                Passes = 2;

                Strength = 0.75f;
                Sharpness = 0.25f;
                AntiAliasing = 0.5f;
                AntiRinging = 0.0f;
                Softness = 0.1f;

                FirstPassOnly = true;
                upscaler = new Scaler.Bilinear();
                downscaler = new Scaler.Bilinear();
            }

            protected override string ShaderPath
            {
                get { return @"SuperRes\SuperChromaRes"; }
            }

            public override IFilter CreateFilter(IFilter input)
            {
                IFilter yuv;

                var chromaSize = (TextureSize)Renderer.ChromaSize;
                var targetSize = input.OutputSize;

                // Original values
                var yInput = new YSourceFilter();
                var uInput = new USourceFilter();
                var vInput = new VSourceFilter();

                float[] YuvConsts = new float[2];
                switch (Renderer.Colorimetric)
                {
                    case YuvColorimetric.Auto : return input;
                    case YuvColorimetric.FullRangePc601: YuvConsts = new[] { 0.114f, 0.299f, 0.0f}; break;
                    case YuvColorimetric.FullRangePc709: YuvConsts = new[] { 0.0722f, 0.2126f, 0.0f }; break;
                    case YuvColorimetric.FullRangePc2020: YuvConsts = new[] { 0.0593f, 0.2627f, 0.0f }; break;
                    case YuvColorimetric.ItuBt601: YuvConsts = new[] { 0.114f, 0.299f, 1.0f }; break;
                    case YuvColorimetric.ItuBt709: YuvConsts = new[] { 0.0722f, 0.2126f, 1.0f }; break;
                    case YuvColorimetric.ItuBt2020: YuvConsts = new[] { 0.0593f, 0.2627f, 1.0f }; break;
                }

                // Skip if downscaling
                if (targetSize.Width <= chromaSize.Width && targetSize.Height <= chromaSize.Height)
                    return input;

                Vector2 offset = Renderer.ChromaOffset;
                Vector2 adjointOffset = -offset * targetSize / chromaSize;

                var Consts = new[] { Strength, Sharpness   , AntiAliasing, AntiRinging,  
                                     Softness, YuvConsts[0], YuvConsts[1], 0.0f,
                                     offset.X, offset.Y };

                var CopyLuma = CompileShader("CopyLuma.hlsl");
                var CopyChroma = CompileShader("CopyChroma.hlsl");
                var Diff = CompileShader("Diff.hlsl").Configure(arguments: YuvConsts, format: TextureFormat.Float16);
                var SuperRes = CompileShader("SuperRes.hlsl", macroDefinitions:
                        (AntiRinging  == 0 ? "SkipAntiRinging  = 1;" : "") +
                        (AntiAliasing == 0 ? "SkipAntiAliasing = 1;" : "") +
                        (Sharpness == 0 ? "SkipSharpening = 1;" : "") +
                        (Softness  == 0 ? "SkipSoftening  = 1;" : "")
                    ).Configure(arguments: new[] { Strength, Sharpness, AntiAliasing, AntiRinging, Softness });

                var GammaToLab = CompileShader("../../Common/GammaToLab.hlsl");
                var LabToGamma = CompileShader("../../Common/LabToGamma.hlsl");
                var LinearToGamma = CompileShader("../../Common/LinearToGamma.hlsl");
                var GammaToLinear = CompileShader("../../Common/GammaToLinear.hlsl");
                var LabToLinear = CompileShader("../../Common/LabToLinear.hlsl");
                var LinearToLab = CompileShader("../../Common/LinearToLab.hlsl");

                yuv = input.ConvertToYuv();

                for (int i = 1; i <= Passes; i++)
                {
                    IFilter res, diff, linear;
                    bool useBilinear = (upscaler is Scaler.Bilinear) || (FirstPassOnly && !(i == 1));

                    // Compare to chroma
                    linear = new ShaderFilter(GammaToLinear, yuv.ConvertToRgb());
                    res = new ResizeFilter(linear, chromaSize, adjointOffset, upscaler, downscaler);
                    res = new ShaderFilter(LinearToGamma, res).ConvertToYuv();
                    diff = new ShaderFilter(Diff, res, uInput, vInput);
                    if (!useBilinear)
                        diff = new ResizeFilter(diff, targetSize, offset, upscaler, downscaler); // Scale to output size

                    // Update result
                    yuv = new ShaderFilter(SuperRes.Configure(useBilinear, arguments: Consts), yuv, diff, uInput, vInput);
                }

                return yuv.ConvertToRgb();
            }
        }

        public class SuperChromaResUi : RenderChainUi<SuperChromaRes, SuperChromaResConfigDialog>
        {
            protected override string ConfigFileName
            {
                get { return "SuperChromaRes"; }
            }

            public override string Category
            {
                get { return "Chroma Scaling"; }
            }

            public override ExtensionUiDescriptor Descriptor
            {
                get
                {
                    return new ExtensionUiDescriptor
                    {
                        Guid = new Guid("AC6F46E2-C04E-4A20-AF68-EFA8A6CA7FCD"),
                        Name = "SuperChromaRes",
                        Description = "SuperChromaRes chroma scaling",
                        Copyright = "SuperChromaRes by Shiandow",
                    };
                }
            }
        }
    }
}
