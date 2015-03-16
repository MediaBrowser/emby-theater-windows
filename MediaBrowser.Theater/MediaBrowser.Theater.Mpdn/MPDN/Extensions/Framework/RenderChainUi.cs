using System;
using Mpdn.Config;
using YAXLib;

namespace Mpdn.RenderScript
{
    public interface IRenderChainUi : IRenderScriptUi
    {
        RenderChain GetChain();
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
        where TDialog : ScriptConfigDialog<TChain>, new()
    {
        [YAXSerializeAs("Settings")]
        public TChain Chain
        {
            get { return Settings; }
            set { Settings = value; }
        }

        protected RenderChainUi()
        {
            Settings = new TChain();
        }

        public IRenderScript CreateRenderScript()
        {
            return new RenderChainScript(Chain);
        }

        #region Implementation

        public RenderChain GetChain()
        {
            return Chain;
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