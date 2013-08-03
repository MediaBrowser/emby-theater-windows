using MediaBrowser.Theater.Interfaces.Playback;
using MediaBrowser.Theater.Interfaces.Theming;
using MediaBrowser.Theater.Interfaces.UserInput;
using MediaBrowser.Theater.Presentation.Controls;
using System;
using System.Linq;
using System.Windows;

namespace MediaBrowser.Plugins.DefaultTheme.Controls
{
    /// <summary>
    /// Interaction logic for ModalWindow.xaml
    /// </summary>
    public partial class ModalWindow : BaseModalWindow
    {
        public ModalWindow(IUserInputManager userInputManager, IPlaybackManager playbackManager)
            : base(userInputManager, playbackManager)
        {
            InitializeComponent();

            Loaded += ModalWindow_Loaded;
        }

        public MessageBoxResult MessageBoxResult { get; set; }

        public string Text
        {
            set { Txt.Text = value; }
        }

        private MessageBoxButton _button;
        public MessageBoxButton Button
        {
            get { return _button; }
            set
            {
                _button = value;
                UpdateButtonVisibility();
                OnPropertyChanged("Button");
            }
        }

        private MessageBoxIcon _messageBoxImage;
        public MessageBoxIcon MessageBoxImage
        {
            get { return _messageBoxImage; }
            set
            {
                _messageBoxImage = value;
                OnPropertyChanged("MessageBoxImage");
            }
        }

        private string _caption;
        public string Caption
        {
            get { return _caption; }
            set
            {
                _caption = value;
                TxtCaption.Visibility = string.IsNullOrEmpty(value) ? Visibility.Collapsed : Visibility.Visible;
                OnPropertyChanged("Caption");
            }
        }

        void ModalWindow_Loaded(object sender, RoutedEventArgs e)
        {
            var button = new[] { BtnOk, BtnCancel, BtnYes, BtnNo }.FirstOrDefault(i => i.Visibility == Visibility.Visible);

            if (button != null)
                button.Focus();
        }

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);

            DataContext = this;
            
            BtnOk.Click += btnOk_Click;
            BtnCancel.Click += btnCancel_Click;
            BtnYes.Click += btnYes_Click;
            BtnNo.Click += btnNo_Click;
        }

        void btnNo_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult = MessageBoxResult.No;
            CloseModal();
        }

        void btnYes_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult = MessageBoxResult.Yes;
            CloseModal();
        }

        void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult = MessageBoxResult.Cancel;
            CloseModal();
        }

        void btnOk_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult = MessageBoxResult.OK;
            CloseModal();
        }

        private void UpdateButtonVisibility()
        {
            BtnYes.Visibility = Button == MessageBoxButton.YesNo || Button == MessageBoxButton.YesNoCancel
                                    ? Visibility.Visible
                                    : Visibility.Collapsed;

            BtnNo.Visibility = Button == MessageBoxButton.YesNo || Button == MessageBoxButton.YesNoCancel
                            ? Visibility.Visible
                            : Visibility.Collapsed;

            BtnOk.Visibility = Button == MessageBoxButton.OK || Button == MessageBoxButton.OKCancel
                    ? Visibility.Visible
                    : Visibility.Collapsed;

            BtnCancel.Visibility = Button == MessageBoxButton.OKCancel || Button == MessageBoxButton.YesNoCancel
            ? Visibility.Visible
            : Visibility.Collapsed;
        }
    }

}
