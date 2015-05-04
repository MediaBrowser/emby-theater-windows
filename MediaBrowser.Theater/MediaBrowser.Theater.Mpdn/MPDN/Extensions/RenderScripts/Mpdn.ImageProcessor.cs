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
using System.ComponentModel;
using System.Linq;

namespace Mpdn.RenderScript
{
    namespace Mpdn.ImageProcessor
    {
        #region ImageProcessorUsage

        public enum ImageProcessorUsage
        {
            [Description("Always")] Always,
            [Description("Never")] Never,
            [Description("When upscaling video")] WhenUpscaling,
            [Description("When downscaling video")] WhenDownscaling,
            [Description("When not scaling video")] WhenNotScaling,
            [Description("When upscaling input")] WhenUpscalingInput,
            [Description("When downscaling input")] WhenDownscalingInput,
            [Description("When not scaling input")] WhenNotScalingInput
        }

        #endregion

        public class ImageProcessor : RenderChain
        {
            #region Settings

            private string[] m_ShaderFileNames;

            public ImageProcessor()
            {
                ImageProcessorUsage = ImageProcessorUsage.Always;
            }

            public string[] ShaderFileNames
            {
                get { return m_ShaderFileNames ?? (m_ShaderFileNames = new string[0]); }
                set { m_ShaderFileNames = value; }
            }

            public ImageProcessorUsage ImageProcessorUsage { get; set; }

            #endregion

            protected override string ShaderPath
            {
                get { return "ImageProcessingShaders"; }
            }

            public string FullShaderPath
            {
                get { return ShaderDataFilePath; }
            }

            public override IFilter CreateFilter(IFilter input)
            {
                if (UseImageProcessor(input))
                {
                    return ShaderFileNames.Aggregate((IFilter) input,
                        (current, filename) => new ShaderFilter(CompileShader(filename), current));
                }
                return input;
            }

            private bool UseImageProcessor(IFilter input)
            {
                var notscalingVideo = false;
                var upscalingVideo = false;
                var downscalingVideo = false;
                var notscalingInput = false;
                var upscalingInput = false;
                var downscalingInput = false;

                var usage = ImageProcessorUsage;
                TextureSize inputSize = Renderer.VideoSize;
                TextureSize outputSize = Renderer.TargetSize;
                if (outputSize == inputSize)
                {
                    // Not scaling video
                    notscalingVideo = true;
                }
                else if (outputSize.Width > inputSize.Width)
                {
                    // Upscaling video
                    upscalingVideo = true;
                }
                else
                {
                    // Downscaling video
                    downscalingVideo = true;
                }

                inputSize = input.OutputSize;
                if (outputSize == inputSize)
                {
                    // Not scaling input
                    notscalingInput = true;
                }
                else if (outputSize.Width > inputSize.Width)
                {
                    // Upscaling input
                    upscalingInput = true;
                }
                else
                {
                    // Downscaling input
                    downscalingInput = true;
                }

                switch (usage)
                {
                    case ImageProcessorUsage.Always:
                        return true;
                    case ImageProcessorUsage.Never:
                        return false;
                    case ImageProcessorUsage.WhenUpscaling:
                        return upscalingVideo;
                    case ImageProcessorUsage.WhenDownscaling:
                        return downscalingVideo;
                    case ImageProcessorUsage.WhenNotScaling:
                        return notscalingVideo;
                    case ImageProcessorUsage.WhenUpscalingInput:
                        return upscalingInput;
                    case ImageProcessorUsage.WhenDownscalingInput:
                        return downscalingInput;
                    case ImageProcessorUsage.WhenNotScalingInput:
                        return notscalingInput;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        public class ImageProcessorScript : RenderChainUi<ImageProcessor, ImageProcessorConfigDialog>
        {
            protected override string ConfigFileName
            {
                get { return "Mpdn.ImageProcessor"; }
            }

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
                        Guid = new Guid("50CA262F-65B6-4A0F-A8B5-5E25B6A18217"),
                        Name = "Image Processor",
                        Description = GetDescription(),
                    };
                }
            }

            private string GetDescription()
            {
                return Settings.ShaderFileNames.Length == 0
                    ? "Pixel shader pre-/post-processing filter"
                    : GetUsageString() + string.Join(" ➔ ", Settings.ShaderFileNames);
            }

            private string GetUsageString()
            {
                var usage = Settings.ImageProcessorUsage;
                string result;
                switch (usage)
                {
                    case ImageProcessorUsage.Never:
                        result = "[INACTIVE] ";
                        break;
                    case ImageProcessorUsage.WhenUpscaling:
                        result = "When upscaling video: ";
                        break;
                    case ImageProcessorUsage.WhenDownscaling:
                        result = "When downscaling video: ";
                        break;
                    case ImageProcessorUsage.WhenNotScaling:
                        result = "When not scaling video: ";
                        break;
                    case ImageProcessorUsage.WhenUpscalingInput:
                        result = "When upscaling input: ";
                        break;
                    case ImageProcessorUsage.WhenDownscalingInput:
                        result = "When downscaling input: ";
                        break;
                    case ImageProcessorUsage.WhenNotScalingInput:
                        result = "When not scaling input: ";
                        break;
                    default:
                        result = string.Empty;
                        break;
                }
                return result;
            }
        }
    }
}
