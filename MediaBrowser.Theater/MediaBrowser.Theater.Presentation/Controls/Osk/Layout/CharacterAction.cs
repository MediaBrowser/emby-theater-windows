using System.Globalization;

namespace MediaBrowser.Theater.Presentation.Controls.Osk.Layout
{
    public class CharacterAction : KeyAction
    {
        private readonly char _character;

        public CharacterAction(char character)
        {
            _character = character;
        }

        public override string GetVisual(IVirtualKeyboard keyboard)
        {
            return _character.ToString(CultureInfo.CurrentCulture);
        }

        public override void Execute(IVirtualKeyboard keyboard)
        {
            keyboard.EnterText(_character.ToString(CultureInfo.CurrentCulture));
        }
    }
}