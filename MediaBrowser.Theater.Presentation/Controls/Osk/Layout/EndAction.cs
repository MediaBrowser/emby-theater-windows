namespace MediaBrowser.Theater.Presentation.Controls.Osk.Layout
{
    public class EndAction : KeyAction
    {
        public override string GetVisual(IVirtualKeyboard keyboard)
        {
            return "End";
        }

        public override void Execute(IVirtualKeyboard keyboard)
        {
            keyboard.End();
        }
    }
}