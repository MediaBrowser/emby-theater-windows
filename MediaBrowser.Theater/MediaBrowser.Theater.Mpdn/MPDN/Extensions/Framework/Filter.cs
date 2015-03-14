using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Drawing;
using SharpDX;
using TransformFunc = System.Func<Mpdn.RenderScript.TextureSize, Mpdn.RenderScript.TextureSize>;
using IBaseFilter = Mpdn.RenderScript.IFilter<Mpdn.IBaseTexture>;

namespace Mpdn.RenderScript
{
    public interface ITextureCache
    {
        ITexture GetTexture(TextureSize textureSize);
        void PutTexture(ITexture texture);
        void PutTempTexture(ITexture texture);
    }

    public interface IFilter<out TTexture>
        where TTexture : class, IBaseTexture
    {
        IBaseFilter[] InputFilters { get; }
        TTexture OutputTexture { get; }
        TextureSize OutputSize { get; }
        int FilterIndex { get; }
        int LastDependentIndex { get; }
        void Render(ITextureCache cache);
        void Reset(ITextureCache cache);
        IFilter<TTexture> Initialize(int time = 1);
    }

    public interface IFilter : IFilter<ITexture> { }

    public interface IResizeableFilter : IFilter
    {
        void SetSize(TextureSize outputSize);
    }

    public struct TextureSize
    {
        public readonly int Width;
        public readonly int Height;
        public readonly int Depth;

        public bool Is3D { get { return Depth != 1; } }
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

            return obj is TextureSize && Equals((TextureSize)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = Width;
                hashCode = (hashCode * 397) ^ Height;
                hashCode = (hashCode * 397) ^ Depth;
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

        public ITexture GetTexture(TextureSize textureSize)
        {
            foreach (var list in new[] { m_SavedTextures, m_OldTextures })
            {
                var index = list.FindIndex(x => (x.GetSize() == textureSize));
                if (index < 0) continue;

                var texture = list[index];
                list.RemoveAt(index);
                return texture;
            }

            return Renderer.CreateRenderTarget(textureSize.Width, textureSize.Height);
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
            InputFilters = inputFilters;
        }

        protected abstract void Render(IList<IBaseTexture> inputs);

        protected virtual IFilter<ITexture> PassthroughFilter { get; set; }

        #region IFilter Implementation

        protected bool Updated { get; set; }
        protected bool Initialized { get; set; }

        public IBaseFilter[] InputFilters { get; private set; }
        public ITexture OutputTexture { get; private set; }

        public abstract TextureSize OutputSize { get; }

        public int FilterIndex { get; private set; }
        public int LastDependentIndex { get; private set; }

        public virtual IFilter<ITexture> Initialize(int time = 1)
        {
            if (PassthroughFilter != null)
            {
                PassthroughFilter = PassthroughFilter.Initialize(time);
                return PassthroughFilter;
            }

            LastDependentIndex = time;

            if (Initialized)
                return this;

            for (int i = 0; i < InputFilters.Length; i++)
            {
                InputFilters[i] = InputFilters[i].Initialize(LastDependentIndex);
                LastDependentIndex = InputFilters[i].LastDependentIndex;
            }

            FilterIndex = LastDependentIndex;

            foreach (var filter in InputFilters)
            {
                filter.Initialize(FilterIndex);
            }

            LastDependentIndex++;

            Initialized = true;
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

            OutputTexture = cache.GetTexture(OutputSize);

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

        public IFilter<TTexture> Initialize(int time = 1)
        {
            LastDependentIndex = time;
            return this;
        }

        public void NewFrame()
        {
        }

        public void Render(ITextureCache cache)
        {
        }

        public virtual void Reset(ITextureCache cache)
        {
            if (typeof(TTexture) == typeof(ITexture))
                cache.PutTempTexture(OutputTexture as ITexture);
        }

        #endregion
    }

    public abstract class BaseSourceFilter : BaseSourceFilter<ITexture>, IFilter { }

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
        public RgbFilter(IFilter inputFilter)
            : base(inputFilter)
        {
            if (inputFilter is YuvFilter)
            {
                PassthroughFilter = inputFilter.InputFilters[0] as IFilter;
            }
        }

        public override TextureSize OutputSize
        {
            get { return InputFilters[0].OutputSize; }
        }

        protected override void Render(IList<IBaseTexture> inputs)
        {
            var texture = inputs.OfType<ITexture>().SingleOrDefault();
            if (texture == null)
                return;

            Renderer.ConvertToRgb(OutputTexture, texture, Renderer.Colorimetric, Renderer.OutputLimitedRange);
        }
    }

    public sealed class YuvFilter : Filter
    {
        public YuvFilter(IFilter inputFilter)
            : base(inputFilter)
        {
            if (inputFilter is RgbFilter)
            {
                PassthroughFilter = inputFilter.InputFilters[0] as IFilter;
            }
        }

        public override TextureSize OutputSize
        {
            get { return InputFilters[0].OutputSize; }
        }

        protected override void Render(IList<IBaseTexture> inputs)
        {
            var texture = inputs.OfType<ITexture>().SingleOrDefault();
            if (texture == null)
                return;

            Renderer.ConvertToYuv(OutputTexture, texture, Renderer.Colorimetric, Renderer.OutputLimitedRange);
        }
    }

    public class ResizeFilter : Filter, IResizeableFilter
    {
        private readonly IScaler m_Downscaler;
        private readonly IScaler m_Upscaler;
        private readonly IScaler m_Convolver;
        private readonly IFilter<ITexture> m_InputFilter;
        private TextureSize m_OutputSize;

        public ResizeFilter(IFilter<ITexture> inputFilter, TextureSize outputSize, IScaler convolver = null)
            : this(inputFilter, outputSize, Renderer.LumaUpscaler, Renderer.LumaDownscaler, convolver)
        {
            m_InputFilter = inputFilter;
        }

        public ResizeFilter(IFilter<ITexture> inputFilter, TextureSize outputSize, IScaler upscaler, IScaler downscaler, IScaler convolver = null)
            : base(inputFilter)
        {
            m_Upscaler = upscaler;
            m_Downscaler = downscaler;
            m_Convolver = convolver;
            m_OutputSize = outputSize;
        }

        public void SetSize(TextureSize targetSize)
        {
            m_OutputSize = targetSize;
        }

        public override IFilter<ITexture> Initialize(int time = 1)
        {
            if (InputFilters[0].OutputSize == m_OutputSize && m_Convolver == null)
            {
                PassthroughFilter = m_InputFilter;
            }

            return base.Initialize(time);
        }

        public override TextureSize OutputSize
        {
            get { return m_OutputSize; }
        }

        protected override void Render(IList<IBaseTexture> inputs)
        {
            var texture = inputs.OfType<ITexture>().SingleOrDefault();
            if (texture == null)
                return;

            Renderer.Scale(OutputTexture, texture, m_Upscaler, m_Downscaler, m_Convolver);
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

        public static IResizeableFilter MakeResizeable(this IFilter filter)
        {
            return (filter as IResizeableFilter) ?? new ResizeFilter(filter, filter.OutputSize);
        }

        public static IResizeableFilter Transform(this IResizeableFilter filter, Func<IFilter, IFilter> transformation)
        {
            return new TransformedResizeableFilter(transformation, filter);
        }

        public static IFilter Apply(this IFilter filter, Func<IFilter, IFilter> map)
        {
            return map(filter);
        }

        public static IFilter Apply(this IFilter filter, Func<IResizeableFilter, IFilter> map)
        {
            return map(filter.MakeResizeable());
        }

        #region Auxilary class(es)

        private sealed class TransformedResizeableFilter : Filter, IResizeableFilter
        {
            private readonly IResizeableFilter m_InputFilter;

            public TransformedResizeableFilter(Func<IFilter, IFilter> transformation, IResizeableFilter inputFilter)
                : base(new IBaseFilter[0])
            {
                m_InputFilter = inputFilter;
                PassthroughFilter = transformation(m_InputFilter);
                CheckSize();
            }

            public void SetSize(TextureSize outputSize)
            {
                m_InputFilter.SetSize(outputSize);
                CheckSize();
            }

            private void CheckSize()
            {
                if (m_InputFilter.OutputSize != PassthroughFilter.OutputSize)
                {
                    throw new InvalidOperationException("Transformation is not allowed to change the size.");
                }
            }

            public override TextureSize OutputSize
            {
                get { return m_InputFilter.OutputSize; }
            }

            protected override void Render(IList<IBaseTexture> inputs)
            {
                throw new NotImplementedException();
            }
        }

        #endregion
    }

    public abstract class GenericShaderFilter<T> : Filter where T : class
    {
        protected GenericShaderFilter(T shader, TransformFunc transform, int sizeIndex, bool linearSampling, float[] arguments,
            params IBaseFilter[] inputFilters)
            : base(inputFilters)
        {
            if (sizeIndex < 0 || sizeIndex >= inputFilters.Length || inputFilters[sizeIndex] == null)
            {
                throw new IndexOutOfRangeException(String.Format("No valid input filter at index {0}", sizeIndex));
            }

            Shader = shader;
            LinearSampling = linearSampling;
            Transform = transform;
            SizeIndex = sizeIndex;

            arguments = arguments ?? new float[0];
            Args = new float[4 * ((arguments.Length + 3) / 4)];
            arguments.CopyTo(Args, 0);
        }

        protected T Shader { get; private set; }
        protected bool LinearSampling { get; private set; }
        protected TransformFunc Transform { get; private set; }
        protected int SizeIndex { get; private set; }
        protected float[] Args { get; private set; }

        public override TextureSize OutputSize
        {
            get { return Transform(InputFilters[SizeIndex].OutputSize); }
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
        public ShaderFilter(IShader shader, TransformFunc transform, int sizeIndex, bool linearSampling, float[] arguments,
            params IBaseFilter[] inputFilters)
            : base(shader, transform, sizeIndex, linearSampling, arguments, inputFilters)
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
                    var tex = (ITexture)input;
                    Shader.SetTextureConstant(i, tex, LinearSampling, false);
                    Shader.SetConstant(String.Format("size{0}", i),
                        new Vector4(tex.Width, tex.Height, 1.0f / tex.Width, 1.0f / tex.Height), false);
                }
                else
                {
                    var tex = (ITexture3D)input;
                    Shader.SetTextureConstant(i, tex, LinearSampling, false);
                    Shader.SetConstant(String.Format("size3d{0}", i),
                        new Vector4(tex.Width, tex.Height, tex.Depth, 0), false);
                }
                i++;
            }

            for (i = 0; 4 * i < Args.Length; i++)
            {
                Shader.SetConstant(String.Format("args{0}", i),
                    new Vector4(Args[4 * i], Args[4 * i + 1], Args[4 * i + 2], Args[4 * i + 3]), false);
            }

            // Legacy constants 
            var output = OutputTexture;
            Shader.SetConstant(0, new Vector4(output.Width, output.Height, Counter++, Stopwatch.GetTimestamp()),
                false);
            Shader.SetConstant(1, new Vector4(1.0f / output.Width, 1.0f / output.Height, 0, 0), false);
        }

        protected override void Render(IShader shader)
        {
            Renderer.Render(OutputTexture, shader);
        }

        #region Auxilary Constructors

        public ShaderFilter(IShader shader, params IBaseFilter[] inputFilters)
            : this(shader, false, inputFilters)
        {
        }

        public ShaderFilter(IShader shader, bool linearSampling, params IBaseFilter[] inputFilters)
            : this(shader, 0, linearSampling, inputFilters)
        {
        }

        public ShaderFilter(IShader shader, int sizeIndex, params IBaseFilter[] inputFilters)
            : this(shader, sizeIndex, false, inputFilters)
        {
        }

        public ShaderFilter(IShader shader, int sizeIndex, bool linearSampling, params IBaseFilter[] inputFilters)
            : this(shader, s => s, sizeIndex, linearSampling, new float[0], inputFilters)
        {
        }

        public ShaderFilter(IShader shader, TransformFunc transform, params IBaseFilter[] inputFilters)
            : this(shader, transform, 0, false, new float[0], inputFilters)
        {
        }

        public ShaderFilter(IShader shader, TransformFunc transform, bool linearSampling, params IBaseFilter[] inputFilters)
            : this(shader, transform, 0, linearSampling, new float[0], inputFilters)
        {
        }

        public ShaderFilter(IShader shader, TransformFunc transform, int sizeIndex, params IBaseFilter[] inputFilters)
            : this(shader, transform, sizeIndex, false, new float[0], inputFilters)
        {
        }

        public ShaderFilter(IShader shader, float[] arguments, params IBaseFilter[] inputFilters)
            : this(shader, false, arguments, inputFilters)
        {
        }

        public ShaderFilter(IShader shader, bool linearSampling, float[] arguments, params IBaseFilter[] inputFilters)
            : this(shader, 0, linearSampling, arguments, inputFilters)
        {
        }

        public ShaderFilter(IShader shader, int sizeIndex, float[] arguments, params IBaseFilter[] inputFilters)
            : this(shader, sizeIndex, false, arguments, inputFilters)
        {
        }

        public ShaderFilter(IShader shader, int sizeIndex, bool linearSampling, float[] arguments, params IBaseFilter[] inputFilters)
            : this(shader, s => s, sizeIndex, linearSampling, arguments, inputFilters)
        {
        }

        public ShaderFilter(IShader shader, TransformFunc transform, float[] arguments, params IBaseFilter[] inputFilters)
            : this(shader, transform, 0, false, arguments, inputFilters)
        {
        }

        public ShaderFilter(IShader shader, TransformFunc transform, bool linearSampling, float[] arguments, params IBaseFilter[] inputFilters)
            : this(shader, transform, 0, linearSampling, arguments, inputFilters)
        {
        }

        public ShaderFilter(IShader shader, TransformFunc transform, int sizeIndex, float[] arguments, params IBaseFilter[] inputFilters)
            : this(shader, transform, sizeIndex, false, arguments, inputFilters)
        {
        }

        #endregion
    }

    public class Shader11Filter : GenericShaderFilter<IShader11>
    {
        public Shader11Filter(IShader11 shader, TransformFunc transform, int sizeIndex, bool linearSampling,
            float[] arguments, params IBaseFilter[] inputFilters)
            : base(shader, transform, sizeIndex, linearSampling, arguments, inputFilters)
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
                    var tex = (ITexture)input;
                    Shader.SetTextureConstant(i, tex, LinearSampling, false);
                    Shader.SetConstantBuffer(String.Format("size{0}", i),
                        new Vector4(tex.Width, tex.Height, 1.0f / tex.Width, 1.0f / tex.Height), false);
                }
                else
                {
                    var tex = (ITexture3D)input;
                    Shader.SetTextureConstant(i, tex, LinearSampling, false);
                    Shader.SetConstantBuffer(String.Format("size3d{0}", i),
                        new Vector4(tex.Width, tex.Height, tex.Depth, 0), false);
                }
                i++;
            }

            for (i = 0; 4 * i < Args.Length; i++)
            {
                Shader.SetConstantBuffer(String.Format("args{0}", i),
                    new Vector4(Args[4 * i], Args[4 * i + 1], Args[4 * i + 2], Args[4 * i + 3]), false);
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

        #region Auxilary Constructors

        public Shader11Filter(IShader11 shader, params IBaseFilter[] inputFilters)
            : this(shader, false, inputFilters)
        {
        }

        public Shader11Filter(IShader11 shader, bool linearSampling, params IBaseFilter[] inputFilters)
            : this(shader, 0, linearSampling, inputFilters)
        {
        }

        public Shader11Filter(IShader11 shader, int sizeIndex, params IBaseFilter[] inputFilters)
            : this(shader, sizeIndex, false, inputFilters)
        {
        }

        public Shader11Filter(IShader11 shader, int sizeIndex, bool linearSampling, params IBaseFilter[] inputFilters)
            : this(shader, s => s, sizeIndex, linearSampling, new float[0], inputFilters)
        {
        }

        public Shader11Filter(IShader11 shader, TransformFunc transform, params IBaseFilter[] inputFilters)
            : this(shader, transform, 0, false, new float[0], inputFilters)
        {
        }

        public Shader11Filter(IShader11 shader, TransformFunc transform, bool linearSampling, params IBaseFilter[] inputFilters)
            : this(shader, transform, 0, linearSampling, new float[0], inputFilters)
        {
        }

        public Shader11Filter(IShader11 shader, TransformFunc transform, int sizeIndex, params IBaseFilter[] inputFilters)
            : this(shader, transform, sizeIndex, false, new float[0], inputFilters)
        {
        }

        public Shader11Filter(IShader11 shader, float[] arguments, params IBaseFilter[] inputFilters)
            : this(shader, false, arguments, inputFilters)
        {
        }

        public Shader11Filter(IShader11 shader, bool linearSampling, float[] arguments, params IBaseFilter[] inputFilters)
            : this(shader, 0, linearSampling, arguments, inputFilters)
        {
        }

        public Shader11Filter(IShader11 shader, int sizeIndex, float[] arguments, params IBaseFilter[] inputFilters)
            : this(shader, sizeIndex, false, arguments, inputFilters)
        {
        }

        public Shader11Filter(IShader11 shader, int sizeIndex, bool linearSampling, float[] arguments, params IBaseFilter[] inputFilters)
            : this(shader, s => s, sizeIndex, linearSampling, arguments, inputFilters)
        {
        }

        public Shader11Filter(IShader11 shader, TransformFunc transform, float[] arguments, params IBaseFilter[] inputFilters)
            : this(shader, transform, 0, false, arguments, inputFilters)
        {
        }

        public Shader11Filter(IShader11 shader, TransformFunc transform, bool linearSampling, float[] arguments, params IBaseFilter[] inputFilters)
            : this(shader, transform, 0, linearSampling, arguments, inputFilters)
        {
        }

        public Shader11Filter(IShader11 shader, TransformFunc transform, int sizeIndex, float[] arguments, params IBaseFilter[] inputFilters)
            : this(shader, transform, sizeIndex, false, arguments, inputFilters)
        {
        }

        #endregion
    }

    public class DirectComputeFilter : Shader11Filter
    {
        public DirectComputeFilter(IShader11 shader, int threadGroupX, int threadGroupY, int threadGroupZ,
            TransformFunc transform, int sizeIndex, bool linearSampling, float[] arguments,
            params IBaseFilter[] inputFilters)
            : base(shader, transform, sizeIndex, linearSampling, arguments, inputFilters)
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

        #region Auxilary Constructors

        public DirectComputeFilter(IShader11 shader, int threadGroupX, int threadGroupY, int threadGroupZ,
            params IBaseFilter[] inputFilters)
            : this(shader, threadGroupX, threadGroupY, threadGroupZ, false, inputFilters)
        {
        }

        public DirectComputeFilter(IShader11 shader, int threadGroupX, int threadGroupY, int threadGroupZ,
            bool linearSampling, params IBaseFilter[] inputFilters)
            : this(shader, threadGroupX, threadGroupY, threadGroupZ, 0, linearSampling, inputFilters)
        {
        }

        public DirectComputeFilter(IShader11 shader, int threadGroupX, int threadGroupY, int threadGroupZ, int sizeIndex,
            params IBaseFilter[] inputFilters)
            : this(shader, threadGroupX, threadGroupY, threadGroupZ, sizeIndex, false, inputFilters)
        {
        }

        public DirectComputeFilter(IShader11 shader, int threadGroupX, int threadGroupY, int threadGroupZ, int sizeIndex,
            bool linearSampling, params IBaseFilter[] inputFilters)
            : this(
                shader, threadGroupX, threadGroupY, threadGroupZ, s => s, sizeIndex, linearSampling, new float[0],
                inputFilters)
        {
        }

        public DirectComputeFilter(IShader11 shader, int threadGroupX, int threadGroupY, int threadGroupZ,
            TransformFunc transform, params IBaseFilter[] inputFilters)
            : this(shader, threadGroupX, threadGroupY, threadGroupZ, transform, 0, false, new float[0], inputFilters)
        {
        }

        public DirectComputeFilter(IShader11 shader, int threadGroupX, int threadGroupY, int threadGroupZ,
            TransformFunc transform, bool linearSampling, params IBaseFilter[] inputFilters)
            : this(
                shader, threadGroupX, threadGroupY, threadGroupZ, transform, 0, linearSampling, new float[0],
                inputFilters)
        {
        }

        public DirectComputeFilter(IShader11 shader, int threadGroupX, int threadGroupY, int threadGroupZ,
            TransformFunc transform, int sizeIndex, params IBaseFilter[] inputFilters)
            : this(
                shader, threadGroupX, threadGroupY, threadGroupZ, transform, sizeIndex, false, new float[0],
                inputFilters)
        {
        }

        public DirectComputeFilter(IShader11 shader, int threadGroupX, int threadGroupY, int threadGroupZ,
            float[] arguments, params IBaseFilter[] inputFilters)
            : this(shader, threadGroupX, threadGroupY, threadGroupZ, false, arguments, inputFilters)
        {
        }

        public DirectComputeFilter(IShader11 shader, int threadGroupX, int threadGroupY, int threadGroupZ,
            bool linearSampling, float[] arguments, params IBaseFilter[] inputFilters)
            : this(shader, threadGroupX, threadGroupY, threadGroupZ, 0, linearSampling, arguments, inputFilters)
        {
        }

        public DirectComputeFilter(IShader11 shader, int threadGroupX, int threadGroupY, int threadGroupZ, int sizeIndex,
            float[] arguments, params IBaseFilter[] inputFilters)
            : this(shader, threadGroupX, threadGroupY, threadGroupZ, sizeIndex, false, arguments, inputFilters)
        {
        }

        public DirectComputeFilter(IShader11 shader, int threadGroupX, int threadGroupY, int threadGroupZ, int sizeIndex,
            bool linearSampling, float[] arguments, params IBaseFilter[] inputFilters)
            : this(
                shader, threadGroupX, threadGroupY, threadGroupZ, s => s, sizeIndex, linearSampling, arguments,
                inputFilters)
        {
        }

        public DirectComputeFilter(IShader11 shader, int threadGroupX, int threadGroupY, int threadGroupZ,
            TransformFunc transform, float[] arguments, params IBaseFilter[] inputFilters)
            : this(shader, threadGroupX, threadGroupY, threadGroupZ, transform, 0, false, arguments, inputFilters)
        {
        }

        public DirectComputeFilter(IShader11 shader, int threadGroupX, int threadGroupY, int threadGroupZ,
            TransformFunc transform, bool linearSampling, float[] arguments, params IBaseFilter[] inputFilters)
            : this(
                shader, threadGroupX, threadGroupY, threadGroupZ, transform, 0, linearSampling, arguments, inputFilters)
        {
        }

        public DirectComputeFilter(IShader11 shader, int threadGroupX, int threadGroupY, int threadGroupZ,
            TransformFunc transform, int sizeIndex, float[] arguments, params IBaseFilter[] inputFilters)
            : this(
                shader, threadGroupX, threadGroupY, threadGroupZ, transform, sizeIndex, false, arguments, inputFilters)
        {
        }

        #endregion
    }
}