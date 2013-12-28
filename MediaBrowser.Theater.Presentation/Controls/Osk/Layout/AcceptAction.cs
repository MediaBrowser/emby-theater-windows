namespace MediaBrowser.Theater.Presentation.Controls.Osk.Layout
{
    public class AcceptAction
        : KeyAction
    {
        private readonly string _text;

        public AcceptAction(string text = "Accept")
        {
            _text = text;
        }

        public override string GetVisual(IVirtualKeyboard keyboard)
        {
            return _text;
        }

        public override void Execute(IVirtualKeyboard keyboard)
        {
            keyboard.Accept();
        }
    }
}
