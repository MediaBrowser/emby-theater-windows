using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Linq;

namespace Mpdn.PlayerExtensions
{
    public class DynamicHotkeys : PlayerExtension
    {
        protected static IList<Verb> m_Verbs = new List<Verb>();
        protected static Action Reload;

        public static void RegisterHotkey(Guid guid, string hotkey, Action action)
        {
            Keys keys;
            if (TryDecodeKeyString(hotkey, out keys))
            {
                m_Verbs.Add(new Verb(Category.Window, "Dynamic Hotkeys", guid.ToString(), hotkey, "", action));
                Reload();
            }
        }

        public static void RemoveHotkey(Guid guid)
        {
            m_Verbs = m_Verbs.Where(v => v.Caption != guid.ToString()).ToList();
        }

        public DynamicHotkeys()
        {
            Reload = LoadVerbs;
        }

        public override IList<Verb> Verbs { get { return m_Verbs; } }

        public override ExtensionUiDescriptor Descriptor
        {
            get
            {
                return new ExtensionUiDescriptor
                {
                    Guid = new Guid("29CBA419-591F-4CEB-9BC1-41D592F5F203"),
                    Name = "DynamicHotkeys",
                    Description = "Allows scripts to dynamically add and remove hotkeys.",
                    Copyright = ""
                };
            }
        }
    }
}
