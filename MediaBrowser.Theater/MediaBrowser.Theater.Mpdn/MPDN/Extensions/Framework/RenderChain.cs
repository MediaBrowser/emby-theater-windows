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
using Mpdn.OpenCl;
using TransformFunc = System.Func<System.Drawing.Size, System.Drawing.Size>;

namespace Mpdn.RenderScript
{
    public abstract class RenderChain : IDisposable
    {
        public abstract IFilter CreateFilter(IFilter input);

        #region Operators

        public static RenderChain Identity = new StaticChain(x => x);

        public static implicit operator Func<IFilter, IFilter>(RenderChain map)
        {
            return filter => map.CreateFilter(filter);
        }

        public static implicit operator RenderChain(Func<IFilter, IFilter> map)
        {
            return new StaticChain(map);
        }

        public static IFilter operator +(IFilter filter, RenderChain map)
        {
            return filter.Apply(map);
        }

        public static RenderChain operator +(RenderChain f, RenderChain g)
        {
            return (RenderChain)(filter => (filter + f) + g);
        }

        #endregion

        #region Shader Compilation

        protected virtual string ShaderPath
        {
            get { return GetType().Name; }
        }

        protected string ShaderDataFilePath
        {
            get
            {
                var asmPath = typeof (IRenderScript).Assembly.Location;
                return Path.Combine(PathHelper.GetDirectoryName(asmPath), "Extensions", "RenderScripts", ShaderPath);
            }
        }

        protected IShader CompileShader(string shaderFileName, string entryPoint = "main", string macroDefinitions = null)
        {
            return ShaderCache.CompileShader(Path.Combine(ShaderDataFilePath, shaderFileName), entryPoint, macroDefinitions);
        }

        protected IShader11 CompileShader11(string shaderFileName, string profile, string entryPoint = "main", string macroDefinitions = null)
        {
            return ShaderCache.CompileShader11(Path.Combine(ShaderDataFilePath, shaderFileName), entryPoint, profile, macroDefinitions);
        }

        protected IKernel CompileClKernel(string sourceFileName, string entryPoint, string options = null)
        {
            return ShaderCache.CompileClKernel(Path.Combine(ShaderDataFilePath, sourceFileName), entryPoint, options);
        }

        protected IShader LoadShader(string shaderFileName)
        {
            return ShaderCache.LoadShader(Path.Combine(ShaderDataFilePath, shaderFileName));
        }

        protected IShader11 LoadShader11(string shaderFileName)
        {
            return ShaderCache.LoadShader11(Path.Combine(ShaderDataFilePath, shaderFileName));
        }

        #endregion

        #region Size Calculations

        protected bool IsDownscalingFrom(TextureSize size)
        {
            return !IsNotScalingFrom(size) && !IsUpscalingFrom(size);
        }

        protected bool IsNotScalingFrom(TextureSize size)
        {
            return size == Renderer.TargetSize;
        }

        protected bool IsUpscalingFrom(TextureSize size)
        {
            var targetSize = Renderer.TargetSize;
            return targetSize.Width > size.Width || targetSize.Height > size.Height;
        }

        protected bool IsDownscalingFrom(IFilter chain)
        {
            return IsDownscalingFrom(chain.OutputSize);
        }

        protected bool IsNotScalingFrom(IFilter chain)
        {
            return IsNotScalingFrom(chain.OutputSize);
        }

        protected bool IsUpscalingFrom(IFilter chain)
        {
            return IsUpscalingFrom(chain.OutputSize);
        }

        #endregion

        #region Resource Disposal Methods

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
        }

        public virtual void RenderScriptDisposed()
        {
        }

        ~RenderChain()
        {
            Dispose(false);
        }

        #endregion
    }

    public class StaticChain : RenderChain
    {
        private readonly Func<IFilter, IFilter> m_Compiler;

        public StaticChain(Func<IFilter, IFilter> compiler)
        {
            m_Compiler = compiler;
        }

        public override IFilter CreateFilter(IFilter input)
        {
            return m_Compiler(input);
        }
    }
}