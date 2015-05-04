using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Mpdn.Config;
using Mpdn.RenderScript;

namespace Mpdn.PlayerExtensions.GitHub
{
    public class RenderControl : PlayerExtension
    {
        protected static VideoRendererSettings RendererSettings
        {
            get { return PlayerControl.PlayerSettings.VideoRendererSettings;  }
        }

        public override ExtensionUiDescriptor Descriptor
        {
            get
            {
                return new ExtensionUiDescriptor
                {
                    Guid = new Guid("7563EB69-C62C-42FC-9054-0E33ABEE1D5F"),
                    Name = "Renderer Control",
                    Description = "Controls the video renderer"
                };
            }
        }

        public override IList<Verb> Verbs
        {
            get
            {
                return new[]
                {
                    GetVerb("Toggle YUV levels", "Ctrl+Shift+L", ToggleLevels),
                    GetVerb("Toggle YUV matrix", "Ctrl+Shift+M", ToggleYUV),
                };
            }
        }

        private void ToggleLevels()
        {
            var settings = RendererSettings.OutputLevels;
            switch (Renderer.Colorimetric)
            {
                case YuvColorimetric.FullRangePc601: RendererSettings.OutputLevels = YuvColorimetric.ItuBt601; break;
                case YuvColorimetric.FullRangePc709: RendererSettings.OutputLevels = YuvColorimetric.ItuBt709; break;
                case YuvColorimetric.FullRangePc2020: RendererSettings.OutputLevels = YuvColorimetric.ItuBt2020; break;
                case YuvColorimetric.ItuBt601: RendererSettings.OutputLevels = YuvColorimetric.FullRangePc601; break;
                case YuvColorimetric.ItuBt709: RendererSettings.OutputLevels = YuvColorimetric.FullRangePc709; break;
                case YuvColorimetric.ItuBt2020: RendererSettings.OutputLevels = YuvColorimetric.FullRangePc2020; break;
            }
            PlayerControl.ShowOsdText("Colour space: " + RendererSettings.OutputLevels.ToDescription());
            PlayerControl.RefreshSettings();
            RendererSettings.OutputLevels = settings;
        }

        private void ToggleYUV()
        {
            var settings = RendererSettings.OutputLevels;
            switch (Renderer.Colorimetric)
            {
                case YuvColorimetric.FullRangePc601:    RendererSettings.OutputLevels = YuvColorimetric.FullRangePc709; break;
                case YuvColorimetric.FullRangePc709:    RendererSettings.OutputLevels = YuvColorimetric.FullRangePc2020; break;
                case YuvColorimetric.FullRangePc2020:   RendererSettings.OutputLevels = YuvColorimetric.FullRangePc601; break;
                case YuvColorimetric.ItuBt601:  RendererSettings.OutputLevels = YuvColorimetric.ItuBt709; break;
                case YuvColorimetric.ItuBt709:  RendererSettings.OutputLevels = YuvColorimetric.ItuBt2020; break;
                case YuvColorimetric.ItuBt2020: RendererSettings.OutputLevels = YuvColorimetric.ItuBt601; break;
            }
            PlayerControl.ShowOsdText("Colour space: " + RendererSettings.OutputLevels.ToDescription());
            PlayerControl.RefreshSettings();
            RendererSettings.OutputLevels = settings;
        }

        private static Verb GetVerb(string menuItemText, string shortCutString, Action action)
        {
            return new Verb(Category.Play, "Renderer", menuItemText, shortCutString, string.Empty, action);
        }
    }
}