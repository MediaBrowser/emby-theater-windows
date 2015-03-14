using System;
using System.Collections.Generic;
using System.ComponentModel;
using SharpDX;
using TransformFunc = System.Func<System.Drawing.Size, System.Drawing.Size>;

namespace Mpdn.RenderScript
{
    namespace Shiandow.Chroma
    {
        #region Presets

        public enum Presets
        {
            [Description("Custom")] Custom = -1,
            [Description("Hermite")] Hermite = 0,
            [Description("Spline")] Spline = 1,
            [Description("Catmull-Rom")] CatmullRom = 2,
            [Description("Mitchell-Netravali")] MitchellNetravali = 3,
            [Description("Robidoux")] Robidoux = 4,
            [Description("Robidoux-Sharp")] RobidouxSharp = 5,
            [Description("Robidoux-Soft")] RobidouxSoft = 6
        }

        #endregion

        public class BicubicChroma : RenderChain
        {
            #region Settings

            public static readonly double[] B_CONST =
            {
                0.0, 1.0, 0.0, 1.0/3.0, 12/(19 + 9*Math.Sqrt(2)),
                6/(13 + 7*Math.Sqrt(2)), (9 - 3*Math.Sqrt(2))/7
            };

            public static readonly double[] C_CONST =
            {
                0.0, 0.0, 1.0/2.0, 1.0/3.0, 113/(58 + 216*Math.Sqrt(2)),
                7/(2 + 12*Math.Sqrt(2)), (-2 + 3*Math.Sqrt(2))/14
            };

            private Presets m_Preset;

            public BicubicChroma()
            {
                B = (float) (1.0/3.0);
                C = (float) (1.0/3.0);
                Preset = Presets.Custom;
            }

            public float B { get; set; }

            public float C { get; set; }

            public Presets Preset
            {
                get { return m_Preset; }
                set
                {
                    if (value != Presets.Custom)
                    {
                        B = (float) B_CONST[(int) value];
                        C = (float) C_CONST[(int) value];
                    }
                    m_Preset = value;
                }
            }

            #endregion

            protected override string ShaderPath
            {
                get { return "ChromaScaler"; }
            }

            public override IFilter CreateFilter(IResizeableFilter sourceFilter)
            {
                var chromaShader = CompileShader("Chroma.hlsl");

                var yInput = new YSourceFilter();
                var uInput = new USourceFilter();
                var vInput = new VSourceFilter();

                float[] offset = { 0.0f, 0.5f };

                var chroma = new ShaderFilter(chromaShader, new[] {B, C, offset[0], offset[1]}, yInput, uInput, vInput);
                var rgb = chroma.ConvertToRgb();

                return rgb;
            }
        }


        public class ChromaScaler : RenderChainUi<BicubicChroma, ChromaScalerConfigDialog>
        {
            protected override string ConfigFileName
            {
                get { return "Shiandow.Chroma"; }
            }

            public override ExtensionUiDescriptor Descriptor
            {
                get
                {
                    return new ExtensionUiDescriptor
                    {
                        Guid = new Guid("BDCC94DD-93B3-4414-BA1F-345E10E1C371"),
                        Name = "ChromaScaler",
                        Description = "Chroma Scaler",
                        Copyright = "ChromaScaler by Shiandow",
                    };
                }
            }
        }
    }
}