namespace MediaBrowser.Theater.Presentation.Controls.Osk.Layout
{
    public class BackspaceAction : KeyAction
    {
        public override string GetVisual(IVirtualKeyboard keyboard)
        {
            return "Backspace";
        }

        public override void Execute(IVirtualKeyboard keyboard)
        {
            keyboard.Backspace();
        }
    }
}