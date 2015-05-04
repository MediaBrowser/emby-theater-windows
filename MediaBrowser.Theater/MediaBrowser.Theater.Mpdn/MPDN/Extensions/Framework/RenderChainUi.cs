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
using Mpdn.Config;
using YAXLib;

namespace Mpdn.RenderScript
{
    public interface IRenderChainUi : IRenderScriptUi
    {
        RenderChain Chain { get; }
        string Category { get; }
    }

    public static class RenderChainUi
    {
        public static IRenderChainUi Identity = new IdentityRenderChainUi();

        public static bool IsIdentity(this IRenderChainUi chainUi)
        {
            return chainUi is IdentityRenderChainUi;
        }

        private class IdentityRenderChain : StaticChain
        {
            public IdentityRenderChain() : base(x => x) { }
        }

        private class IdentityRenderChainUi : RenderChainUi<IdentityRenderChain>
        {
            public override string Category
            {
                get { return "Meta"; }
            }

            public override ExtensionUiDescriptor Descriptor
            {
                get
                {
                    return new ExtensionUiDescriptor
                    {
                        Guid = Guid.Empty,
                        Name = "None",
                        Description = "Do nothing"
                    };
                }
            }
        }
    }

    public abstract class RenderChainUi<TChain> : RenderChainUi<TChain, ScriptConfigDialog<TChain>>
        where TChain : RenderChain, new()
    { }

    public abstract class RenderChainUi<TChain, TDialog> : ExtensionUi<Config.Internal.RenderScripts, TChain, TDialog>, IRenderChainUi
        where TChain : RenderChain, new()
        where TDialog : IScriptConfigDialog<TChain>, new()
    {
        public abstract string Category { get; }

        protected RenderChainUi()
        {
            Settings = new TChain();
        }

        public IRenderScript CreateRenderScript()
        {
            return new RenderChainScript(Settings);
        }

        #region Implementation

        [YAXDontSerialize]
        public RenderChain Chain
        {
            get { return Settings; }
        }

        public override void Destroy()
        {
            DisposeHelper.Dispose(Settings);
            base.Destroy();
        }

        #endregion Implementation
    }

    public static class RenderChainExtensions
    {
        public static IRenderChainUi CreateNew(this IRenderChainUi scriptUi)
        {
            var constructor = scriptUi.GetType().GetConstructor(Type.EmptyTypes);
            if (constructor == null)
            {
                throw new EntryPointNotFoundException("RenderChainUi must implement parameter-less constructor");
            }

            var renderScript = (IRenderChainUi)constructor.Invoke(new object[0]);
            return renderScript;
        }
    }
}
