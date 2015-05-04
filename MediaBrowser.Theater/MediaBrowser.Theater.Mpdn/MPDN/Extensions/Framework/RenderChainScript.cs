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
using System.Drawing;

namespace Mpdn.RenderScript
{
    public class RenderChainScript : IRenderScript, IDisposable
    {
        private TextureCache m_Cache;
        private SourceFilter m_SourceFilter;
        private IFilter<ITexture> m_Filter;

        protected RenderChain Chain;

        public RenderChainScript(RenderChain chain)
        {
            Chain = chain;
            m_Cache = new TextureCache();
        }

        public void Dispose()
        {
            if (m_Cache == null)
                return;

            DisposeHelper.Dispose(ref m_Cache);
            Chain.RenderScriptDisposed();
            Chain = null;
        }

        public ScriptInterfaceDescriptor Descriptor
        {
            get
            {
                if (m_SourceFilter == null)
                    return null;

                return new ScriptInterfaceDescriptor
                {
                    WantYuv = true,
                    Prescale = (m_SourceFilter.LastDependentIndex > 0),
                    PrescaleSize = (Size)m_SourceFilter.OutputSize
                };
            }
        }

        public void Update()
        {
            m_SourceFilter = new SourceFilter();
            var rgbInput = m_SourceFilter.Transform(x => new RgbFilter(x));
            var result = Chain.CreateFilter(rgbInput);
            result.SetSize(Renderer.TargetSize);
            m_Filter = result.Compile();
            m_Filter.Initialize();
        }

        public void Render()
        {
            m_Cache.PutTempTexture(Renderer.OutputRenderTarget);
            m_Filter.Render(m_Cache);
            if (Renderer.OutputRenderTarget != m_Filter.OutputTexture)
            {
                Scale(Renderer.OutputRenderTarget, m_Filter.OutputTexture);
            }
            m_Filter.Reset(m_Cache);
            m_Cache.FlushTextures();
        }

        private static void Scale(ITexture output, ITexture input)
        {
            Renderer.Scale(output, input, Renderer.LumaUpscaler, Renderer.LumaDownscaler);
        }
    }
}
