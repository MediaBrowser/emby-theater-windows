using System;
using System.Drawing;

namespace Mpdn.RenderScript
{
    namespace Shiandow.Nedi
    {
        public class Nedi : RenderChain
        {
            #region Settings

            public Nedi()
            {
                AlwaysDoubleImage = false;
                Centered = true;
            }

            public bool AlwaysDoubleImage { get; set; }
            public bool Centered { get; set; }

            #endregion

            public float[] LumaConstants = {0.2126f, 0.7152f, 0.0722f};

            private bool UseNedi(IFilter sourceFilter)
            {
                var size = sourceFilter.OutputSize;
                if (size.IsEmpty)
                    return false;

                if (AlwaysDoubleImage)
                    return true;

                return Renderer.TargetSize.Width > size.Width ||
                       Renderer.TargetSize.Height > size.Height;
            }

            public override IFilter CreateFilter(IResizeableFilter sourceFilter)
            {
                var nedi1Shader = CompileShader("NEDI-I.hlsl");
                var nedi2Shader = CompileShader("NEDI-II.hlsl");
                var nediHInterleaveShader = CompileShader("NEDI-HInterleave.hlsl");
                var nediVInterleaveShader = CompileShader("NEDI-VInterleave.hlsl");

                Func<TextureSize, TextureSize> transformWidth;
                Func<TextureSize, TextureSize> transformHeight;
                if (Centered)
                {
                    transformWidth = s => new TextureSize(2 * s.Width - 1, s.Height);
                    transformHeight = s => new TextureSize(s.Width, 2 * s.Height - 1);
                } else {
                    transformWidth = s => new TextureSize(2 * s.Width, s.Height);
                    transformHeight = s => new TextureSize(s.Width, 2 * s.Height);
                }

                if (!UseNedi(sourceFilter))
                    return sourceFilter;

                var nedi1 = new ShaderFilter(nedi1Shader, LumaConstants, sourceFilter);
                var nediH = new ShaderFilter(nediHInterleaveShader, transformWidth, sourceFilter, nedi1);
                var nedi2 = new ShaderFilter(nedi2Shader, LumaConstants, nediH);
                var nediV = new ShaderFilter(nediVInterleaveShader, transformHeight, nediH, nedi2);

                return nediV;
            }
        }

        public class NediScaler : RenderChainUi<Nedi, NediConfigDialog>
        {
            protected override string ConfigFileName
            {
                get { return "Shiandow.Nedi"; }
            }

            public override ExtensionUiDescriptor Descriptor
            {
                get
                {
                    return new ExtensionUiDescriptor
                    {
                        Guid = new Guid("B8E439B7-7DC2-4FC1-94E2-608A39756FB0"),
                        Name = "NEDI",
                        Description = GetDescription(),
                        Copyright = "NEDI by Shiandow",
                    };
                }
            }

            private string GetDescription()
            {
                var options = string.Format("{0}", Settings.AlwaysDoubleImage ? " (forced)" : string.Empty);
                return string.Format("NEDI image doubler{0}", options);
            }
        }
    }
}