using System;
using MediaBrowser.Theater.Presentation.Controls.Osk.Layout.KeySets;

namespace MediaBrowser.Theater.Presentation.Controls.Osk
{
    /// <summary>
    /// Interaction logic for VirtualKeyboardModalWindow.xaml
    /// </summary>
    public partial class VirtualKeyboardModalWindow : BaseModalWindow
    {
        public string Text
        {
            get { return Keyboard.Text; }
        }

        public event Action Accepted;

        protected virtual void OnAccepted()
        {
            CloseModal();

            Action handler = Accepted;
            if (handler != null) handler();
        }

        public void Cancel()
        {
            Keyboard.Cancel();
        }

        public VirtualKeyboardModalWindow(string title = null, string initialText = null)
        {
            DataContext = this;

            InitializeComponent();

            EmptySpace.MouseDown += (s, a) => Keyboard.Cancel();

            Keyboard.DefaultKeySet = KeySets.En();
            Keyboard.Focus();
            
            Keyboard.Title = title;
            Keyboard.Accepted += OnAccepted;
            Keyboard.Cancelled += CloseModal;

            Keyboard.Loaded += (s, e) =>
            {
                Keyboard.Text = initialText;
                Keyboard.CaretIndex = initialText == null ? 0 : initialText.Length;
            };
        }

        protected override void OnBrowserBack()
        {
            Cancel();
        }
    }
}
