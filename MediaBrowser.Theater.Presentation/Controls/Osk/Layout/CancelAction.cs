namespace MediaBrowser.Theater.Presentation.Controls.Osk.Layout
{
    public class CancelAction
        : KeyAction
    {
        public override string GetVisual(IVirtualKeyboard keyboard)
        {
            return "Cancel";
        }

        public override void Execute(IVirtualKeyboard keyboard)
        {
            keyboard.Cancel();
        }
    }
}