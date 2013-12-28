namespace MediaBrowser.Theater.Presentation.Controls.Osk.Layout
{
    public class LeftAction : KeyAction
    {
        public override string GetVisual(IVirtualKeyboard keyboard)
        {
            return "<";
        }

        public override void Execute(IVirtualKeyboard keyboard)
        {
            keyboard.Left();
        }
    }
}