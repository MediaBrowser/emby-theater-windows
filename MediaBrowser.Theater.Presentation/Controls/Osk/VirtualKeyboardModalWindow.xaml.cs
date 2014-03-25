using System;
using System.Windows.Input;
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

        protected override void OnKeyDown(KeyEventArgs e)
        {
           // The Base.OnKeyDown event handles page back and page forward events
           // We want the Osk input text box to handle all its own key events 
           // with no post processing of back page and forward page
           // so we override here and don't call base
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
