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
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Drawing;
using Mpdn.OpenCl;
using SharpDX;
using TransformFunc = System.Func<Mpdn.RenderScript.TextureSize, Mpdn.RenderScript.TextureSize>;
using IBaseFilter = Mpdn.RenderScript.IFilter<Mpdn.IBaseTexture>;

namespace Mpdn.RenderScript
{
    public interface ITextureCache
    {
        ITexture GetTexture(TextureSize textureSize, TextureFormat textureFormat);
        void PutTexture(ITexture texture);
        void PutTempTexture(ITexture texture);
    }

    public interface IFilter<out TTexture>
        where TTexture : class, IBaseTexture
    {
        IBaseFilter[] InputFilters { get; }
        TTexture OutputTexture { get; }
        TextureSize OutputSize { get; }
        TextureFormat OutputFormat { get; }
        int FilterIndex { get; }
        int LastDependentIndex { get; }
        void Render(ITextureCache cache);
        void Reset(ITextureCache cache);
        void Initialize(int time = 1);
        IFilter<TTexture> Compile();
    }

    public interface IFilter : IFilter<ITexture>
    {
    }

    public interface IResizeableFilter : IFilter
    {
        void SetSize(TextureSize outputSize);
    }

    public struct TextureSize
    {
        public readonly int Width;
        public readonly int Height;
        public readonly int Depth;

        public bool Is3D
        {
            get { return Depth != 1; }
        }

        public bool IsEmpty
        {
            get { return (Width == 0) || (Height == 0) || (Depth == 0); }
        }

        public TextureSize(int width, int height, int depth = 1)
        {
            Width = width;
            Height = height;
            Depth = depth;
        }

        public static bool operator ==(TextureSize a, TextureSize b)
        {
            return a.Equals(b);
        }

        public static bool operator !=(TextureSize a, TextureSize b)
        {
            return !a.Equals(b);
        }

        public bool Equals(TextureSize other)
        {
            return Width == other.Width && Height == other.Height && Depth == other.Depth;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
                return false;

            return obj is TextureSize && Equals((TextureSize) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = Width;
                hashCode = (hashCode*397) ^ Height;
                hashCode = (hashCode*397) ^ Depth;
                return hashCode;
            }
        }

        public static implicit operator TextureSize(Size size)
        {
            return new TextureSize(size.Width, size.Height);
        }

        public static explicit operator Size(TextureSize size)
        {
            return new Size(size.Width, size.Height);
        }

        public static implicit operator Vector2(TextureSize size)
        {
            return new Vector2(size.Width, size.Height);
        }
    }

    public static class TextureHelper
    {
        public static TextureSize GetSize(this IBaseTexture texture)
        {
            if (texture is ITexture)
            {
                var t = texture as ITexture;
                return new TextureSize(t.Width, t.Height);
            }
            if (texture is ITexture3D)
            {
                var t = texture as ITexture3D;
                return new TextureSize(t.Width, t.Height, t.Depth);
            }
            throw new ArgumentException("Invalid texture type");
        }
    }

    public class TextureCache : ITextureCache, IDisposable
    {
        private List<ITexture> m_OldTextures = new List<ITexture>();
        private List<ITexture> m_SavedTextures = new List<ITexture>();
        private List<ITexture> m_TempTextures = new List<ITexture>();

        public ITexture GetTexture(TextureSize textureSize, TextureFormat textureFormat)
        {
            foreach (var list in new[] {m_SavedTextures, m_OldTextures})
            {
                var index = list.FindIndex(x => (x.GetSize() == textureSize) && (x.Format == textureFormat));
                if (index < 0) continue;

                var texture = list[index];
                list.RemoveAt(index);
                return texture;
            }

            return Renderer.CreateRenderTarget(textureSize.Width, textureSize.Height, textureFormat);
        }

        public void PutTempTexture(ITexture texture)
        {
            m_TempTextures.Add(texture);
            m_SavedTextures.Add(texture);
        }

        public void PutTexture(ITexture texture)
        {
            m_SavedTextures.Add(texture);
        }

        public void FlushTextures()
        {
            foreach (var texture in m_OldTextures)
            {
                DisposeHelper.Dispose(texture);
            }

            foreach (var texture in m_TempTextures)
            {
                m_SavedTextures.Remove(texture);
            }

            m_OldTextures = m_SavedTextures;
            m_TempTextures = new List<ITexture>();
            m_SavedTextures = new List<ITexture>();
        }

        public void Dispose()
        {
            FlushTextures();
            FlushTextures();
        }
    }

    public abstract class Filter : IFilter
    {
        protected Filter(params IBaseFilter[] inputFilters)
        {
            if (inputFilters == null || inputFilters.Any(f => f == null))
            {
                throw new ArgumentNullException("inputFilters");
            }

            Initialized = false;
            CompilationResult = null;
            InputFilters = inputFilters;
        }

        protected abstract void Render(IList<IBaseTexture> inputs);

        #region IFilter Implementation

        protected bool Updated { get; set; }
        protected bool Initialized { get; set; }
        protected IFilter<ITexture> CompilationResult { get; set; }

        public IBaseFilter[] InputFilters { get; private set; }
        public ITexture OutputTexture { get; private set; }

        public virtual TextureSize OutputSize
        {
            get { return InputFilters[0].OutputSize; }
        }

        public virtual TextureFormat OutputFormat
        {
            get { return Renderer.RenderQuality.GetTextureFormat(); }
        }

        public int FilterIndex { get; private set; }
        public int LastDependentIndex { get; private set; }

        public void Initialize(int time = 1)
        {
            LastDependentIndex = time;

            if (Initialized)
                return;

            for (int i = 0; i < InputFilters.Length; i++)
            {
                InputFilters[i].Initialize(LastDependentIndex);
                LastDependentIndex = InputFilters[i].LastDependentIndex;
            }

            FilterIndex = LastDependentIndex;

            foreach (var filter in InputFilters)
            {
                filter.Initialize(FilterIndex);
            }

            LastDependentIndex++;

            Initialized = true;
        }

        public IFilter<ITexture> Compile()
        {
            if (CompilationResult == null)
            {
                for (int i = 0; i < InputFilters.Length; i++)
                    InputFilters[i] = InputFilters[i].Compile();

                CompilationResult = Optimize();
            };
            
            return CompilationResult;
        }

        protected virtual IFilter<ITexture> Optimize()
        {
            return this;
        }

        public virtual void Render(ITextureCache cache)
        {
            if (Updated)
                return;

            Updated = true;

            foreach (var filter in InputFilters)
            {
                filter.Render(cache);
            }

            var inputTextures =
                InputFilters
                    .Select(f => f.OutputTexture)
                    .ToList();

            OutputTexture = cache.GetTexture(OutputSize, OutputFormat);

            Render(inputTextures);

            foreach (var filter in InputFilters)
            {
                if (filter.LastDependentIndex <= FilterIndex)
                {
                    filter.Reset(cache);
                }
            }
        }

        public virtual void Reset(ITextureCache cache)
        {
            Updated = false;

            if (OutputTexture != null)
            {
                cache.PutTexture(OutputTexture);
            }

            OutputTexture = null;
        }

        #endregion
    }

    public abstract class BaseSourceFilter<TTexture> : IFilter<TTexture>
        where TTexture : class, IBaseTexture
    {
        protected BaseSourceFilter(params IBaseFilter[] inputFilters)
        {
            InputFilters = inputFilters;
        }

        public abstract TTexture OutputTexture { get; }

        public abstract TextureSize OutputSize { get; }

        #region IFilter Implementation

        public IBaseFilter[] InputFilters { get; protected set; }

        public virtual int FilterIndex
        {
            get { return 0; }
        }

        public virtual int LastDependentIndex { get; private set; }

        public void Initialize(int time = 1)
        {
            LastDependentIndex = time;
        }

        public IFilter<TTexture> Compile()
        {
            return this;
        }

        public TextureFormat OutputFormat
        {
            get
            {
                return OutputTexture != null
                    ? OutputTexture.Format
                    : Renderer.RenderQuality.GetTextureFormat();
            }
        }

        public void NewFrame()
        {
        }

        public void Render(ITextureCache cache)
        {
        }

        public virtual void Reset(ITextureCache cache)
        {
            if (typeof (TTexture) == typeof (ITexture))
                cache.PutTempTexture(OutputTexture as ITexture);
        }

        #endregion
    }

    public abstract class BaseSourceFilter : BaseSourceFilter<ITexture>, IFilter
    {
    }

    public sealed class SourceFilter : BaseSourceFilter, IResizeableFilter
    {
        private TextureSize m_OutputSize;

        public void SetSize(TextureSize targetSize)
        {
            m_OutputSize = targetSize;
        }

        #region IFilter Implementation

        public override ITexture OutputTexture
        {
            get { return Renderer.InputRenderTarget; }
        }

        public override TextureSize OutputSize
        {
            get { return (m_OutputSize.IsEmpty ? Renderer.VideoSize : m_OutputSize); }
        }

        #endregion
    }

    public sealed class YSourceFilter : BaseSourceFilter
    {
        public override ITexture OutputTexture
        {
            get { return Renderer.TextureY; }
        }

        public override TextureSize OutputSize
        {
            get { return Renderer.LumaSize; }
        }

        public override void Reset(ITextureCache cache)
        {
        }
    }

    public sealed class USourceFilter : BaseSourceFilter
    {
        public override ITexture OutputTexture
        {
            get { return Renderer.TextureU; }
        }

        public override TextureSize OutputSize
        {
            get { return Renderer.ChromaSize; }
        }

        public override void Reset(ITextureCache cache)
        {
        }
    }

    public sealed class VSourceFilter : BaseSourceFilter
    {
        public override ITexture OutputTexture
        {
            get { return Renderer.TextureV; }
        }

        public override TextureSize OutputSize
        {
            get { return Renderer.ChromaSize; }
        }

        public override void Reset(ITextureCache cache)
        {
        }
    }

    public sealed class NullFilter : BaseSourceFilter
    {
        public override ITexture OutputTexture
        {
            get { return Renderer.OutputRenderTarget; }
        }

        public override TextureSize OutputSize
        {
            get { return Renderer.TargetSize; }
        }
    }

    public sealed class TextureSourceFilter : BaseSourceFilter
    {
        private readonly ITexture m_Texture;
        private readonly TextureSize m_Size;

        public TextureSourceFilter(ITexture texture)
        {
            m_Texture = texture;
            m_Size = new TextureSize(texture.Width, texture.Height);
        }

        public override ITexture OutputTexture
        {
            get { return m_Texture; }
        }

        public override TextureSize OutputSize
        {
            get { return m_Size; }
        }

        public override void Reset(ITextureCache cache)
        {
        }
    }

    public sealed class Texture3DSourceFilter : BaseSourceFilter<ITexture3D>
    {
        private readonly ITexture3D m_Texture;
        private readonly TextureSize m_Size;

        public Texture3DSourceFilter(ITexture3D texture)
        {
            m_Texture = texture;
            m_Size = new TextureSize(texture.Width, texture.Height, texture.Depth);
        }

        public override ITexture3D OutputTexture
        {
            get { return m_Texture; }
        }

        public override TextureSize OutputSize
        {
            get { return m_Size; }
        }

        public override void Reset(ITextureCache cache)
        {
        }
    }

    public sealed class RgbFilter : Filter
    {
        public readonly YuvColorimetric Colorimetric;
        public readonly bool OutputLimitedRange;

        public RgbFilter(IFilter inputFilter)
            : this(inputFilter, Renderer.Colorimetric, Renderer.OutputLimitedRange)
        {
        }

        public RgbFilter(IFilter inputFilter, bool limitedRange)
            : this(inputFilter, Renderer.Colorimetric, limitedRange)
        {
        }

        public RgbFilter(IFilter inputFilter, YuvColorimetric colorimetric)
            : this(inputFilter, colorimetric, Renderer.OutputLimitedRange)
        {
        }

        public RgbFilter(IFilter inputFilter, YuvColorimetric colorimetric, bool limitedRange)
            : base(inputFilter)
        {
            Colorimetric = colorimetric;
            OutputLimitedRange = limitedRange;
        }

        protected override IFilter<ITexture> Optimize()
        {
            var input = InputFilters[0] as YuvFilter;
            if (input != null && input.Colorimetric == Colorimetric && input.OutputLimitedRange == OutputLimitedRange)
                return (IFilter<ITexture>) input.InputFilters[0];

            return this;
        }

        public override TextureSize OutputSize
        {
            get { return InputFilters[0].OutputSize; }
        }

        public override TextureFormat OutputFormat
        {
            get { return InputFilters[0].OutputFormat; }
        }

        protected override void Render(IList<IBaseTexture> inputs)
        {
            var texture = inputs.OfType<ITexture>().SingleOrDefault();
            if (texture == null)
                return;

            Renderer.ConvertToRgb(OutputTexture, texture, Colorimetric, OutputLimitedRange);
        }
    }

    public sealed class YuvFilter : Filter
    {
        public readonly YuvColorimetric Colorimetric;
        public readonly bool OutputLimitedRange;

        public YuvFilter(IFilter inputFilter)
            : this(inputFilter, Renderer.Colorimetric, Renderer.OutputLimitedRange)
        {
        }

        public YuvFilter(IFilter inputFilter, bool limitedRange)
            : this(inputFilter, Renderer.Colorimetric, limitedRange)
        {
        }

        public YuvFilter(IFilter inputFilter, YuvColorimetric colorimetric)
            : this(inputFilter, colorimetric, Renderer.OutputLimitedRange)
        {
        }

        public YuvFilter(IFilter inputFilter, YuvColorimetric colorimetric, bool limitedRange)
            : base(inputFilter)
        {
            Colorimetric = colorimetric;
            OutputLimitedRange = limitedRange;
        }

        protected override IFilter<ITexture> Optimize()
        {
            var input = InputFilters[0] as RgbFilter;
            if (input != null && input.Colorimetric == Colorimetric && input.OutputLimitedRange == OutputLimitedRange)
                return (IFilter<ITexture>) input.InputFilters[0];

            return this;
        }

        public override TextureSize OutputSize
        {
            get { return InputFilters[0].OutputSize; }
        }

        public override TextureFormat OutputFormat
        {
            get { return InputFilters[0].OutputFormat; }
        }

        protected override void Render(IList<IBaseTexture> inputs)
        {
            var texture = inputs.OfType<ITexture>().SingleOrDefault();
            if (texture == null)
                return;

            Renderer.ConvertToYuv(OutputTexture, texture, Colorimetric, OutputLimitedRange);
        }
    }

    public class ResizeFilter : Filter, IResizeableFilter
    {
        private readonly IScaler m_Downscaler;
        private readonly IScaler m_Upscaler;
        private readonly IScaler m_Convolver;
        private readonly Vector2 m_Offset;
        private TextureSize m_OutputSize;
        private readonly TextureChannels m_Channels;

        public ResizeFilter(IFilter<ITexture> inputFilter)
            : this(inputFilter, inputFilter.OutputSize)
        {
        }

        public ResizeFilter(IFilter<ITexture> inputFilter, TextureSize outputSize, IScaler convolver = null)
            : this(inputFilter, outputSize, TextureChannels.All, Vector2.Zero, Renderer.LumaUpscaler, Renderer.LumaDownscaler, convolver)
        {
        }

        public ResizeFilter(IFilter<ITexture> inputFilter, TextureSize outputSize, Vector2 offset, IScaler convolver = null)
            : this(inputFilter, outputSize, TextureChannels.All, offset, Renderer.LumaUpscaler, Renderer.LumaDownscaler, convolver)
        {
        }

        public ResizeFilter(IFilter<ITexture> inputFilter, TextureSize outputSize, IScaler upscaler, IScaler downscaler, IScaler convolver = null)
            : this(inputFilter, outputSize, TextureChannels.All, Vector2.Zero, upscaler, downscaler, convolver)
        {
        }

        public ResizeFilter(IFilter<ITexture> inputFilter, TextureSize outputSize, TextureChannels channels, IScaler convolver = null)
            : this(inputFilter, outputSize, channels, Vector2.Zero, Renderer.LumaUpscaler, Renderer.LumaDownscaler, convolver)
        {
        }

        public ResizeFilter(IFilter<ITexture> inputFilter, TextureSize outputSize, TextureChannels channels, Vector2 offset, IScaler convolver = null)
            : this(inputFilter, outputSize, channels, offset, Renderer.LumaUpscaler, Renderer.LumaDownscaler, convolver)
        {
        }

        public ResizeFilter(IFilter<ITexture> inputFilter, TextureSize outputSize, TextureChannels channels, IScaler upscaler, IScaler downscaler, IScaler convolver = null)
            : this(inputFilter, outputSize, channels, Vector2.Zero, upscaler, downscaler, convolver)
        {
        }

        public ResizeFilter(IFilter<ITexture> inputFilter, TextureSize outputSize, Vector2 offset, IScaler upscaler, IScaler downscaler, IScaler convolver = null)
            : this(inputFilter, outputSize, TextureChannels.All, offset, upscaler, downscaler, convolver)
        {
        }

        public ResizeFilter(IFilter<ITexture> inputFilter, TextureSize outputSize, TextureChannels channels, Vector2 offset, IScaler upscaler, IScaler downscaler, IScaler convolver = null)
            : base(inputFilter)
        {
            m_Upscaler = upscaler;
            m_Downscaler = downscaler;
            m_Convolver = convolver;
            m_OutputSize = outputSize;
            m_Channels = channels;
            m_Offset = offset;
        }

        public void SetSize(TextureSize targetSize)
        {
            m_OutputSize = targetSize;
        }

        protected override IFilter<ITexture> Optimize()
        {
            if (InputFilters[0].OutputSize == m_OutputSize && m_Convolver == null)
            {
                return InputFilters[0] as IFilter;
            }

            return this;
        }

        public override TextureSize OutputSize
        {
            get { return m_OutputSize; }
        }

        public override TextureFormat OutputFormat
        {
            get { return InputFilters[0].OutputFormat; }
        }

        protected override void Render(IList<IBaseTexture> inputs)
        {
            var texture = inputs.OfType<ITexture>().SingleOrDefault();
            if (texture == null)
                return;

            Renderer.Scale(OutputTexture, texture, m_Channels, m_Offset, m_Upscaler, m_Downscaler, m_Convolver);
        }
    }

    public static class FilterConversions
    {
        public static IFilter ConvertToRgb(this IFilter filter)
        {
            return new RgbFilter(filter);
        }

        public static IFilter ConvertToYuv(this IFilter filter)
        {
            return new YuvFilter(filter);
        }

        public static IResizeableFilter Transform(this IResizeableFilter filter, Func<IFilter, IFilter> transformation)
        {
            return new TransformedResizeableFilter(transformation, filter);
        }

        public static IFilter Apply(this IFilter filter, Func<IFilter, IFilter> map)
        {
            return map(filter);
        }

        public static void SetSize(this IFilter filter, TextureSize size)
        {
            var resizeable = (filter as IResizeableFilter) ?? new ResizeFilter(filter);
            resizeable.SetSize(size);
        }

        #region Auxilary class(es)

        private sealed class TransformedResizeableFilter : Filter, IResizeableFilter
        {
            private readonly IResizeableFilter m_InputFilter;

            public TransformedResizeableFilter(Func<IFilter, IFilter> transformation, IResizeableFilter inputFilter)
                : base(new IBaseFilter[0])
            {
                m_InputFilter = inputFilter;
                CompilationResult = transformation(m_InputFilter);
                CheckSize();
            }

            public void SetSize(TextureSize outputSize)
            {
                m_InputFilter.SetSize(outputSize);
                CheckSize();
            }

            private void CheckSize()
            {
                if (m_InputFilter.OutputSize != CompilationResult.OutputSize)
                {
                    throw new InvalidOperationException("Transformation is not allowed to change the size.");
                }
            }

            public override TextureSize OutputSize
            {
                get { return m_InputFilter.OutputSize; }
            }

            public override TextureFormat OutputFormat
            {
                get { return m_InputFilter.OutputFormat; }
            }

            protected override void Render(IList<IBaseTexture> inputs)
            {
                throw new NotImplementedException();
            }
        }

        #endregion
    }

    public class ShaderFilterSettings<T>
    {
        public T Shader;
        public bool LinearSampling = false;
        public TransformFunc Transform = (s => s);
        public TextureFormat Format = Renderer.RenderQuality.GetTextureFormat();
        public int SizeIndex = 0;
        public float[] Args = new float[0];

        public ShaderFilterSettings(T shader)
        {
            Shader = shader;
        }

        public static implicit operator ShaderFilterSettings<T>(T shader)
        {
            return new ShaderFilterSettings<T>(shader);
        }

        public ShaderFilterSettings<T> Configure(bool? linearSampling = null, float[] arguments = null,
            TransformFunc transform = null, int? sizeIndex = null, TextureFormat? format = null)
        {
            return new ShaderFilterSettings<T>(Shader)
            {
                Transform = transform ?? Transform,
                LinearSampling = linearSampling ?? LinearSampling,
                Format = format ?? Format,
                SizeIndex = sizeIndex ?? SizeIndex,
                Args = arguments ?? Args
            };
        }
    }

    public static class ShaderFilterHelper
    {
        public static ShaderFilterSettings<IShader> Configure(this IShader shader, bool? linearSampling = null,
            float[] arguments = null, TransformFunc transform = null, int? sizeIndex = null,
            TextureFormat? format = null)
        {
            return new ShaderFilterSettings<IShader>(shader).Configure(linearSampling, arguments, transform, sizeIndex,
                format);
        }

        public static ShaderFilterSettings<IShader11> Configure(this IShader11 shader, bool? linearSampling = null,
            float[] arguments = null, TransformFunc transform = null, int? sizeIndex = null,
            TextureFormat? format = null)
        {
            return new ShaderFilterSettings<IShader11>(shader).Configure(linearSampling, arguments, transform, sizeIndex,
                format);
        }

        public static ShaderFilterSettings<IKernel> Configure(this IKernel kernel, bool? linearSampling = null,
            float[] arguments = null, TransformFunc transform = null, int? sizeIndex = null,
            TextureFormat? format = null)
        {
            return new ShaderFilterSettings<IKernel>(kernel).Configure(linearSampling, arguments, transform, sizeIndex,
                format);
        }
    }

    public abstract class GenericShaderFilter<T> : Filter where T : class
    {
        protected GenericShaderFilter(T shader, params IBaseFilter[] inputFilters)
            : this((ShaderFilterSettings<T>) shader, inputFilters)
        {
        }

        protected GenericShaderFilter(ShaderFilterSettings<T> settings, params IBaseFilter[] inputFilters)
            : base(inputFilters)
        {
            Shader = settings.Shader;
            LinearSampling = settings.LinearSampling;
            Transform = settings.Transform;
            Format = settings.Format;
            SizeIndex = settings.SizeIndex;

            if (SizeIndex < 0 || SizeIndex >= inputFilters.Length || inputFilters[SizeIndex] == null)
            {
                throw new IndexOutOfRangeException(String.Format("No valid input filter at index {0}", SizeIndex));
            }

            var arguments = settings.Args ?? new float[0];
            Args = new float[4*((arguments.Length + 3)/4)];
            arguments.CopyTo(Args, 0);
        }

        protected T Shader { get; private set; }
        protected bool LinearSampling { get; private set; }
        protected TransformFunc Transform { get; private set; }
        protected TextureFormat Format { get; private set; }
        protected int SizeIndex { get; private set; }
        protected float[] Args { get; private set; }

        public override TextureSize OutputSize
        {
            get { return Transform(InputFilters[SizeIndex].OutputSize); }
        }

        public override TextureFormat OutputFormat
        {
            get { return Format; }
        }

        protected abstract void LoadInputs(IList<IBaseTexture> inputs);
        protected abstract void Render(T shader);

        protected override void Render(IList<IBaseTexture> inputs)
        {
            LoadInputs(inputs);
            Render(Shader);
        }
    }

    public class ShaderFilter : GenericShaderFilter<IShader>
    {
        public ShaderFilter(ShaderFilterSettings<IShader> settings, params IBaseFilter[] inputFilters)
            : base(settings, inputFilters)
        {
        }

        public ShaderFilter(IShader shader, params IBaseFilter[] inputFilters)
            : base(shader, inputFilters)
        {
        }

        protected int Counter { get; private set; }

        protected override void LoadInputs(IList<IBaseTexture> inputs)
        {
            var i = 0;
            foreach (var input in inputs)
            {
                if (input as ITexture != null)
                {
                    var tex = (ITexture) input;
                    Shader.SetTextureConstant(i, tex, LinearSampling, false);
                    Shader.SetConstant(String.Format("size{0}", i),
                        new Vector4(tex.Width, tex.Height, 1.0f/tex.Width, 1.0f/tex.Height), false);
                }
                else
                {
                    var tex = (ITexture3D) input;
                    Shader.SetTextureConstant(i, tex, LinearSampling, false);
                    Shader.SetConstant(String.Format("size3d{0}", i),
                        new Vector4(tex.Width, tex.Height, tex.Depth, 0), false);
                }
                i++;
            }

            for (i = 0; 4*i < Args.Length; i++)
            {
                Shader.SetConstant(String.Format("args{0}", i),
                    new Vector4(Args[4*i], Args[4*i + 1], Args[4*i + 2], Args[4*i + 3]), false);
            }

            // Legacy constants 
            var output = OutputTexture;
            Shader.SetConstant(0, new Vector4(output.Width, output.Height, Counter++, Stopwatch.GetTimestamp()),
                false);
            Shader.SetConstant(1, new Vector4(1.0f/output.Width, 1.0f/output.Height, 0, 0), false);
        }

        protected override void Render(IShader shader)
        {
            Renderer.Render(OutputTexture, shader);
        }
    }

    public class Shader11Filter : GenericShaderFilter<IShader11>
    {
        public Shader11Filter(ShaderFilterSettings<IShader11> settings, params IBaseFilter[] inputFilters)
            : base(settings, inputFilters)
        {
        }

        public Shader11Filter(IShader11 shader, params IBaseFilter[] inputFilters)
            : base(shader, inputFilters)
        {
        }

        protected int Counter { get; private set; }

        protected override void LoadInputs(IList<IBaseTexture> inputs)
        {
            var i = 0;
            foreach (var input in inputs)
            {
                if (input as ITexture != null)
                {
                    var tex = (ITexture) input;
                    Shader.SetTextureConstant(i, tex, LinearSampling, false);
                    Shader.SetConstantBuffer(String.Format("size{0}", i),
                        new Vector4(tex.Width, tex.Height, 1.0f/tex.Width, 1.0f/tex.Height), false);
                }
                else
                {
                    var tex = (ITexture3D) input;
                    Shader.SetTextureConstant(i, tex, LinearSampling, false);
                    Shader.SetConstantBuffer(String.Format("size3d{0}", i),
                        new Vector4(tex.Width, tex.Height, tex.Depth, 0), false);
                }
                i++;
            }

            for (i = 0; 4*i < Args.Length; i++)
            {
                Shader.SetConstantBuffer(String.Format("args{0}", i),
                    new Vector4(Args[4*i], Args[4*i + 1], Args[4*i + 2], Args[4*i + 3]), false);
            }

            // Legacy constants 
            var output = OutputTexture;
            Shader.SetConstantBuffer(0, new Vector4(output.Width, output.Height, Counter++, Stopwatch.GetTimestamp()),
                false);
        }

        protected override void Render(IShader11 shader)
        {
            Renderer.Render(OutputTexture, shader);
        }
    }

    public class DirectComputeFilter : Shader11Filter
    {
        public DirectComputeFilter(ShaderFilterSettings<IShader11> settings, int threadGroupX, int threadGroupY,
            int threadGroupZ, params IBaseFilter[] inputFilters)
            : base(settings, inputFilters)
        {
            ThreadGroupX = threadGroupX;
            ThreadGroupY = threadGroupY;
            ThreadGroupZ = threadGroupZ;
        }

        public DirectComputeFilter(IShader11 shader, int threadGroupX, int threadGroupY, int threadGroupZ,
            params IBaseFilter[] inputFilters)
            : base(shader, inputFilters)
        {
            ThreadGroupX = threadGroupX;
            ThreadGroupY = threadGroupY;
            ThreadGroupZ = threadGroupZ;
        }

        protected override void Render(IShader11 shader)
        {
            Renderer.Compute(OutputTexture, shader, ThreadGroupX, ThreadGroupY, ThreadGroupZ);
        }

        public int ThreadGroupX { get; private set; }
        public int ThreadGroupY { get; private set; }
        public int ThreadGroupZ { get; private set; }
    }

    public class ClKernelFilter : GenericShaderFilter<IKernel>
    {
        public ClKernelFilter(ShaderFilterSettings<IKernel> settings, int[] globalWorkSizes, params IBaseFilter[] inputFilters)
            : base(settings, inputFilters)
        {
            GlobalWorkSizes = globalWorkSizes;
            LocalWorkSizes = null;
        }

        public ClKernelFilter(IKernel shader, int[] globalWorkSizes, params IBaseFilter[] inputFilters)
            : base(shader, inputFilters)
        {
            GlobalWorkSizes = globalWorkSizes;
            LocalWorkSizes = null;
        }

        public ClKernelFilter(ShaderFilterSettings<IKernel> settings, int[] globalWorkSizes, int[] localWorkSizes, params IBaseFilter[] inputFilters)
            : base(settings, inputFilters)
        {
            GlobalWorkSizes = globalWorkSizes;
            LocalWorkSizes = localWorkSizes;
        }

        public ClKernelFilter(IKernel shader, int[] globalWorkSizes, int[] localWorkSizes, params IBaseFilter[] inputFilters)
            : base(shader, inputFilters)
        {
            GlobalWorkSizes = globalWorkSizes;
            LocalWorkSizes = localWorkSizes;
        }

        protected virtual void LoadCustomInputs()
        {
            // override to load custom OpenCL inputs such as a weights buffer
        }

        protected override void LoadInputs(IList<IBaseTexture> inputs)
        {
            Shader.SetOutputTextureArg(0, OutputTexture); // Note: MPDN only supports one output texture per kernel

            var i = 1;
            foreach (var input in inputs)
            {
                if (input as ITexture != null)
                {
                    var tex = (ITexture) input;
                    Shader.SetInputTextureArg(i, tex, false);
                }
                else
                {
                    throw new NotSupportedException("Only 2D textures are supported in OpenCL");
                }
                i++;
            }

            foreach (var v in Args)
            {
                Shader.SetArg(i++, v, false);
            }

            LoadCustomInputs();
        }

        protected override void Render(IKernel shader)
        {
            Renderer.RunClKernel(shader, GlobalWorkSizes, LocalWorkSizes);
        }

        public int[] GlobalWorkSizes { get; private set; }
        public int[] LocalWorkSizes { get; private set; }
    }
}
