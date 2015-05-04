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
using System.Linq;
using System.Windows.Forms;
using Mpdn.PlayerExtensions;
using YAXLib;

namespace Mpdn.RenderScript
{
    namespace Mpdn.Presets
    {
        public interface INameable
        {
            string Name { set; }
        }

        public class Preset : RenderChain
        {
            #region Settings

            private string m_Name;
            private IRenderChainUi m_Script;

            [YAXAttributeForClass]
            public virtual string Name {
                get { return m_Name; }
                set 
                {
                    m_Name = value;
                    if (Script != null && Chain is INameable)
                        (Chain as INameable).Name = value;
                }
            }

            [YAXAttributeForClass]
            public virtual Guid Guid { get; set; }
            
            public virtual IRenderChainUi Script 
            {
                get { return m_Script; }
                set
                {
                    m_Script = value;
                    if (Script != null && Chain is INameable)
                        (Chain as INameable).Name = Name;
                }
            }

            #endregion

            #region Script Properties

            [YAXDontSerialize]
            public string Description
            {
                get { return Script.Descriptor.Description; }
            }

            [YAXDontSerialize]
            public RenderChain Chain
            {
                get { return Script.Chain; }
            }

            public bool HasConfigDialog()
            {
                if (Script != null)
                    return Script.HasConfigDialog();
                else
                    return false;
            }

            public bool ShowConfigDialog(IWin32Window owner)
            {
                if (Script != null)
                    return Script.ShowConfigDialog(owner);
                else
                    return false;
            }

            #endregion

            #region RenderChain implementation

            public Preset()
            {
                Guid = Guid.NewGuid();
            }

            protected override void Dispose(bool disposing)
            {
                base.Dispose(disposing);
                DisposeHelper.Dispose(Script);
            }

            public override IFilter CreateFilter(IFilter input)
            {
                if (Script != null)
                    return Chain.CreateFilter(input);
                else
                    return input;
            }

            #endregion

            public static Preset Make<T>(string name = null)
                where T : IRenderChainUi, new()
            {
                var script = new T();
                return new Preset() { Name = (name ?? script.Descriptor.Name), Script = script };
            }
        }

        public static class PresetHelper
        {
            public static Preset MakeNewPreset(this IRenderChainUi renderScript, string name = null)
            {
                return renderScript.CreateNew().ToPreset();
            }

            public static Preset ToPreset(this IRenderChainUi renderScript, string name = null)
            {
                return new Preset() { Name = name ?? renderScript.Descriptor.Name, Script = renderScript };
            }
        }

        public class PresetCollection : RenderChain, INameable
        {
            #region Settings

            public List<Preset> Options { get; set; }

            [YAXDontSerialize]
            public string Name { protected get; set; }

            [YAXDontSerialize]
            public virtual bool AllowRegrouping { get { return true; } }

            #endregion 

            public PresetCollection()
                : base()
            {
                Options = new List<Preset>();
            }

            protected override void Dispose(bool disposing)
            {
                base.Dispose(disposing);
                if (disposing && Options != null)
                    foreach (var option in Options)
                        option.Dispose();
            }

            public override IFilter CreateFilter(IFilter input)
            {
                throw new NotImplementedException();
            }
        }

        public class PresetChain : PresetCollection
        {
            public override IFilter CreateFilter(IFilter input)
            {
                return Options.Aggregate(input, (a, b) => a + b);
            }
        }

        public class PresetGroup : PresetCollection
        {
            #region Settings

            public int SelectedIndex { get; set; }

            public string Hotkey
            {
                get { return m_Hotkey; }
                set
                {
                    m_Hotkey = value ?? "";
                    RegisterHotkey();
                }
            }

            [YAXDontSerialize]
            public Preset SelectedOption { get { return Options.ElementAtOrDefault(SelectedIndex); } }

            #endregion

            public PresetGroup()
                : base()
            {
                SelectedIndex = 0;
                m_HotkeyGuid = Guid.NewGuid();
            }

            public override IFilter CreateFilter(IFilter input)
            {
                if (SelectedOption != null)
                    return SelectedOption.CreateFilter(input);
                else
                    return input;
            }

            #region Hotkey Handling

            private Guid m_HotkeyGuid;

            private string m_Hotkey;

            private void RegisterHotkey()
            {
                DynamicHotkeys.RegisterHotkey(m_HotkeyGuid, Hotkey, IncrementSelection);
            }

            private void IncrementSelection()
            {
                if (Options.Count > 0)
                    SelectedIndex = (SelectedIndex + 1) % Options.Count;

                if (SelectedOption != null)
                    PlayerControl.ShowOsdText(Name + ": " + SelectedOption.Name);

                PlayerControl.SetRenderScript(PlayerControl.ActiveRenderScriptGuid);
            }

            #endregion
        }

        public class PresetChainDialog : PresetDialog
        {
            public PresetChainDialog()
                : base()
            {
                Text = "Script Chain";
            }
        }

        public class PresetGroupAdvDialog : PresetDialog
        {
            public PresetGroupAdvDialog()
                : base()
            {
                Text = "Script Group";
            }

            protected override void SaveSettings()
            {
                base.SaveSettings();
                (Settings as PresetGroup).SelectedIndex = SelectedIndex;
            }

            protected override void LoadSettings()
            {
                base.LoadSettings();
                SelectedIndex = (Settings as PresetGroup).SelectedIndex;
            }
        }

        public class ScriptChainScript : RenderChainUi<PresetChain, PresetChainDialog>
        {
            protected override string ConfigFileName
            {
                get { return "Mpdn.ScriptChain"; }
            }

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
                        Guid = new Guid("3A462015-2D92-43AC-B559-396DACF896C3"),
                        Name = "Script Chain",
                        Description = Settings.Options.Count > 0
                            ? string.Join(" ➔ ", Settings.Options.Select(x => x.Name))
                            : "Chains together multiple renderscripts"
                    };
                }
            }
        }

        public class ScriptGroupScript : RenderChainUi<PresetGroup, PresetGroupDialog>
        {
            protected override string ConfigFileName
            {
                get { return "Mpdn.ScriptGroup"; }
            }

            public override string Category
            {
                get { return "Meta"; }
            }

            public string Description()
            {
                return (Settings.Options.Count > 0)
                        ? string.Join(", ",
                            Settings.Options.Select(x => 
                                (x == Settings.SelectedOption)
                                ? "[" + x.Name + "]"
                                : x.Name))
                        : "Picks one out of several renderscripts";
            }

            public override ExtensionUiDescriptor Descriptor
            {
                get
                {
                    return new ExtensionUiDescriptor
                    {
                        Guid = new Guid("57D79B2B-4303-4102-A797-17EC4D003130"),
                        Name = "Script Group",
                        Description = Description()
                    };
                }
            }
        }
    }
}
