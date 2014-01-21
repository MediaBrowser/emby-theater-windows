namespace MediaBrowser.Theater.Presentation.Controls.Osk.Layout
{
    public class HomeAction : KeyAction
    {
        public override string GetVisual(IVirtualKeyboard keyboard)
        {
            return "Home";
        }

        public override void Execute(IVirtualKeyboard keyboard)
        {
            keyboard.Home();
        }
    }
}