using System;
using System.Collections.Generic;
using System.Linq;

namespace Mpdn.RenderScript
{
    namespace Mpdn.ScriptChain
    {
        public class ScriptChain : RenderChain
        {
            public List<IRenderChainUi> ScriptList { get; set; }

            public ScriptChain()
            {
                ScriptList = new List<IRenderChainUi>();
            }

            public override IFilter CreateFilter(IResizeableFilter sourceFilter)
            {
                return ScriptList.Select(pair => pair.GetChain()).Aggregate(sourceFilter, (a, b) => a + b);
            }
        }

        public class ScriptChainScript : RenderChainUi<ScriptChain, ScriptChainDialog>
        {
            protected override string ConfigFileName
            {
                get { return "Mpdn.ScriptChain"; }
            }

            public override ExtensionUiDescriptor Descriptor
            {
                get
                {
                    return new ExtensionUiDescriptor
                    {
                        Guid = new Guid("3A462015-2D92-43AC-B559-396DACF896C3"),
                        Name = "Script Chain",
                        Description = GetDescription(),
                    };
                }
            }

            private string GetDescription()
            {
                return Chain.ScriptList.Count == 0
                    ? "Chain of render scripts"
                    : string.Join(" ➔ ", Chain.ScriptList.Select(x => x.Descriptor.Name));
            }
        }
    }
}