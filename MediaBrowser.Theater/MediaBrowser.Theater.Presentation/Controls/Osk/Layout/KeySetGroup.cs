using System.Collections.Generic;
using System.Linq;

namespace MediaBrowser.Theater.Presentation.Controls.Osk.Layout
{
    public class KeySetGroup
        : Dictionary<string, KeySet>
    {
        public KeyValuePair<string, KeySet> FindNext(KeySet set)
        {
            KeyValuePair<string, KeySet>[] items = this.ToArray();

            for (int i = 0; i < items.Length; i++)
            {
                if (items[i].Value == set)
                {
                    return items[(i + 1)%Count];
                }
            }

            return items[0];
        }
    }

    public class KeySetAction
        : KeyAction
    {
        private readonly KeySetGroup _group;

        public KeySetAction(KeySetGroup keySetGroup)
        {
            _group = keySetGroup;
        }

        public override string GetVisual(IVirtualKeyboard keyboard)
        {
            KeyValuePair<string, KeySet> next = _group.FindNext(keyboard.KeySet);
            return next.Key;
        }

        public override void Execute(IVirtualKeyboard keyboard)
        {
            keyboard.KeySet = _group.FindNext(keyboard.KeySet).Value;
        }
    }
}