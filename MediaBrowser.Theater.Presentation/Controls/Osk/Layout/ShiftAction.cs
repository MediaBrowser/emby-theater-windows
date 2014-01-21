namespace MediaBrowser.Theater.Presentation.Controls.Osk.Layout
{
    public class ShiftAction : KeyAction
    {
        public override string GetVisual(IVirtualKeyboard keyboard)
        {
            return "aA";
        }

        public override void Execute(IVirtualKeyboard keyboard)
        {
            keyboard.ToggleShift();
        }
    }
}
