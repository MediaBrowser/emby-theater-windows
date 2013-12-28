using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace MediaBrowser.Theater.Presentation.Controls.Osk.Layout
{
    public class KeySet
        : List<KeySetRow>
    {
        public string Identifier { get; set; }

        public KeySet(string identifier)
        {
            Identifier = identifier;
        }
    }

    public class KeySetRow
        : List<Key>
    {
        public double HorizontalOffset { get; set; }

        public KeySetRow()
        {       
        }

        public KeySetRow(double horizontalOffset)
        {
            HorizontalOffset = horizontalOffset;
        }
    }

    public class Key
    {
        private double _width;
        public KeyActionPair DefaultAction { get; set; }
        public IEnumerable<KeyActionPair> AlternativeActions { get; set; }

        public double Width
        {
            get { return _width; }
            set
            {
                if (value < 1)
                    throw new ArgumentOutOfRangeException("value", @"Width must be greater than or equal to 1.");

                _width = value;
            }
        }

        public Key()
        {
            Width = 1;
        }

        public Key(char character, params char[] alternateCharacters)
            : this()
        {
            DefaultAction = new KeyActionPair(new CharacterAction(character), new CharacterAction(char.ToUpper(character, CultureInfo.CurrentCulture)));
            AlternativeActions = alternateCharacters.Select(c => new KeyActionPair(new CharacterAction(c), new CharacterAction(char.ToUpper(c, CultureInfo.CurrentCulture)))).ToArray();
        }

        public Key(KeyAction singleAction)
            : this()
        {
            DefaultAction = new KeyActionPair(singleAction, singleAction);
            AlternativeActions = Enumerable.Empty<KeyActionPair>();
        }
    }

    public class KeyActionPair
    {
        public KeyAction Default { get; set; }
        public KeyAction Alternative { get; set; }

        public KeyActionPair()
        {
        }

        public KeyActionPair(KeyAction @default, KeyAction alternative)
        {
            Default = @default;
            Alternative = alternative;
        }
    }

    public abstract class KeyAction
    {
        public abstract string GetVisual(IVirtualKeyboard keyboard);
        public abstract void Execute(IVirtualKeyboard keyboard);
    }
}
