using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using MediaBrowser.Theater.Interfaces.Theming;

namespace MediaBrowser.UI.Controls
{
    /// <summary>
    /// Interaction logic for NotificationMessage.xaml
    /// </summary>
    public partial class NotificationMessage : UserControl, INotifyPropertyChanged
    {
        public NotificationMessage()
        {
            InitializeComponent();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public virtual void OnPropertyChanged(string name)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(name));
            }
        }
        
        public UIElement TextContent
        {
            set
            {
                pnlContent.Children.Clear();

                var textBlock = value as TextBlock;

                if (textBlock != null)
                {
                    textBlock.SetResourceReference(TextBlock.StyleProperty, "NotificationTextStyle");
                }
                pnlContent.Children.Add(value);
            }
        }

        public string Text
        {
            set { TextContent = new TextBlock { Text = value }; }
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
                OnPropertyChanged("Caption");
                txtCaption.Visibility = string.IsNullOrEmpty(value) ? Visibility.Collapsed : Visibility.Visible;
            }
        }

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);

            DataContext = this;
        }
    }
}
