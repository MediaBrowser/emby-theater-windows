using System;
using System.Drawing;

namespace Mpdn.RenderScript
{
    namespace Shiandow.SuperRes
    {
        public class SuperRes : RenderChain
        {
            #region Settings

            public int Passes { get; set; }

            public float Strength { get; set; }
            public float Sharpness { get; set; }
            public float AntiAliasing { get; set; }
            public float AntiRinging { get; set; }

            public bool UseNEDI { get; set; }
            public bool NoIntermediates { get; set; }

            public bool FirstPassOnly;

            #endregion

            public Func<TextureSize> TargetSize; // Not saved
            private IScaler downscaler, upscaler;

            public SuperRes()
            {
                TargetSize = () => Renderer.TargetSize;
                m_ShiftedScaler = new Scaler.Custom(new ShiftedScaler(0.5f), ScalerTaps.Six, false);

                Passes = 3;

                Strength = 0.75f;
                Sharpness = 0.5f;
                AntiAliasing = 0.25f;
                AntiRinging = 0.75f;

                UseNEDI = false;
                NoIntermediates = false;

                FirstPassOnly = false;
                upscaler = new Scaler.Jinc(ScalerTaps.Four, false);
                downscaler = new Scaler.Bilinear();
            }

            public override IFilter CreateFilter(IResizeableFilter sourceFilter)
            {
                var inputSize = sourceFilter.OutputSize;
                var targetSize = TargetSize();

                // Skip if downscaling
                if (targetSize.Width <= inputSize.Width && targetSize.Height <= inputSize.Height)
                    return sourceFilter;
                else
                    return CreateFilter(sourceFilter, sourceFilter);
            }

            public IFilter CreateFilter(IFilter original, IFilter initial)
            {
                IFilter lab, linear, result = initial;

                var inputSize = original.OutputSize;
                var currentSize = original.OutputSize;
                var targetSize = TargetSize();

                var Diff = CompileShader("Diff.hlsl");
                var SuperRes = CompileShader("SuperRes.hlsl");

                var GammaToLab = CompileShader("GammaToLab.hlsl");
                var LabToGamma = CompileShader("LabToGamma.hlsl");
                var LinearToGamma = CompileShader("LinearToGamma.hlsl");
                var GammaToLinear = CompileShader("GammaToLinear.hlsl");
                var LabToLinear = CompileShader("LabToLinear.hlsl");
                var LinearToLab = CompileShader("LinearToLab.hlsl");

                var NEDI = new Shiandow.Nedi.Nedi
                {
                    AlwaysDoubleImage = false,
                    Centered = false,
                    LumaConstants = new[] { 1.0f, 0.0f, 0.0f }
                };

                var Consts = new[] { Strength, Sharpness, AntiAliasing, AntiRinging };

                // Initial scaling
                lab = new ShaderFilter(GammaToLab, initial);
                original = new ShaderFilter(GammaToLab, original);

                for (int i = 1; i <= Passes; i++)
                {
                    IFilter res, diff;
                    bool useBilinear = (upscaler is Scaler.Bilinear) || (FirstPassOnly && !(i == 1));

                    // Calculate size
                    if (i == Passes || NoIntermediates) currentSize = targetSize;
                    else currentSize = CalculateSize(currentSize, targetSize, i);
                                        
                    // Resize
                    if (i == 1 && UseNEDI)
                        lab = new ResizeFilter(lab + NEDI, currentSize, m_ShiftedScaler, m_ShiftedScaler, m_ShiftedScaler);
                    else 
                        lab = new ResizeFilter(lab, currentSize);

                    // Downscale and Subtract
                    linear = new ShaderFilter(LabToLinear, lab);
                    res = new ResizeFilter(linear, inputSize, upscaler, downscaler); // Downscale result
                    diff = new ShaderFilter(Diff, res, original);                    // Compare with original

                    // Scale difference back
                    if (!useBilinear)
                        diff = new ResizeFilter(diff, currentSize, upscaler, downscaler);
                    
                    // Update result
                    lab = new ShaderFilter(SuperRes, useBilinear, Consts, lab, diff, original);
                    result = new ShaderFilter(LabToGamma, lab);
                }

                return result;
            }

            private TextureSize CalculateSize(TextureSize sizeA, TextureSize sizeB, int k)
            {
                double w, h;
                var MaxScale = 2.0;
                var MinScale = Math.Sqrt(MaxScale);
                
                int minW = sizeA.Width; int minH = sizeA.Height;
                int maxW = sizeB.Width; int maxH = sizeB.Height;

                int maxSteps = (int)Math.Floor  (Math.Log((double)(maxH * maxW) / (double)(minH * minW)) / (2 * Math.Log(MinScale)));
                int minSteps = (int)Math.Ceiling(Math.Log((double)(maxH * maxW) / (double)(minH * minW)) / (2 * Math.Log(MaxScale)));
                int steps = Math.Max(Math.Max(1,minSteps), Math.Min(maxSteps, Passes - (k - 1)));
                
                w = minW * Math.Pow((double)maxW / (double)minW, (double)Math.Min(k, steps) / (double)steps);
                h = minW * Math.Pow((double)maxH / (double)minH, (double)Math.Min(k, steps) / (double)steps);

                return new TextureSize(Math.Max(minW, Math.Min(maxW, (int)Math.Round(w))),
                                Math.Max(minH, Math.Min(maxH, (int)Math.Round(h))));
            }

            private IScaler m_ShiftedScaler;

            private class ShiftedScaler : ICustomLinearScaler
            {
                private float m_Offset;

                public ShiftedScaler(float offset)
                {
                    m_Offset = offset;
                }

                public Guid Guid
                {
                    get { return new Guid(); }
                }

                public string Name
                {
                    get { return ""; }
                }

                public bool AllowDeRing
                {
                    get { return false; }
                }

                public ScalerTaps MaxTapCount
                {
                    get { return ScalerTaps.Six; }
                } 

                public float GetWeight(float n, int width)
                {
                    return (float)Kernel(n + m_Offset, width);
                }

                private static double Kernel(double x, double radius)
                {
                    x = Math.Abs(x);
                    var B = 1.0/3.0;
                    var C = 1.0/3.0;

                    if (x > 2.0)
                        return 0;
                    else if (x <= 1.0)
                        return ((2 - 1.5 * B - C) * x + (-3 + 2 * B + C)) * x * x + (1 - B / 3.0);
                    else
                        return (((-B / 6.0 - C) * x + (B + 5 * C)) * x + (-2 * B - 8 * C)) * x + ((4.0 / 3.0) * B + 4 * C);
                }
            }
        }

        public class SuperResUi : RenderChainUi<SuperRes, SuperResConfigDialog>
        {
            protected override string ConfigFileName
            {
                get { return "Shiandow.SuperRes"; }
            }

            public override ExtensionUiDescriptor Descriptor
            {
                get
                {
                    return new ExtensionUiDescriptor
                    {
                        Guid = new Guid("3E7C670C-EFFB-41EB-AC19-207E650DEBD0"),
                        Name = "SuperRes",
                        Description = "SuperRes image scaling",
                        Copyright = "SuperRes by Shiandow",
                    };
                }
            }
        }
    }
}
