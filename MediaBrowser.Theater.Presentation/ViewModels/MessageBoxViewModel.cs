using MediaBrowser.Theater.Interfaces.Presentation;
using MediaBrowser.Theater.Interfaces.Theming;
using MediaBrowser.Theater.Interfaces.ViewModels;
using System;
using System.Windows;
using System.Windows.Input;

namespace MediaBrowser.Theater.Presentation.ViewModels
{
    public class MessageBoxViewModel : BaseViewModel
    {
        public ICommand YesCommand { get; private set; }
        public ICommand NoCommand { get; private set; }
        public ICommand OKCommand { get; private set; }
        public ICommand CancelCommand { get; private set; }

        private readonly Action<MessageBoxResult> _closeAction; 

        public MessageBoxViewModel(MessageBoxInfo info, Action<MessageBoxResult> closeAction)
        {
            _closeAction = closeAction;

            Caption = info.Caption;
            Text = info.Text;
            Button = info.Button;
            Icon = info.Icon;
            TimeoutMs = info.TimeoutMs;

            YesCommand = new RelayCommand(i => Close(MessageBoxResult.Yes));
            NoCommand = new RelayCommand(i => Close(MessageBoxResult.No));
            OKCommand = new RelayCommand(i => Close(MessageBoxResult.OK));
            CancelCommand = new RelayCommand(i => Close(MessageBoxResult.Cancel));
        }

        private string _caption;
        public string Caption
        {
            get { return _caption; }
            set
            {
                var changed = !string.Equals(_caption, value);

                _caption = value;

                if (changed)
                {
                    OnPropertyChanged("Caption");
                }
            }
        }

        private string _text;
        public string Text
        {
            get { return _text; }
            set
            {
                var changed = !string.Equals(_text, value);

                _text = value;

                if (changed)
                {
                    OnPropertyChanged("Text");
                }
            }
        }

        private MessageBoxButton _button;
        public MessageBoxButton Button
        {
            get { return _button; }
            set
            {
                var changed = _button != value;

                _button = value;

                if (changed)
                {
                    OnPropertyChanged("Button");
                }
            }
        }

        private MessageBoxIcon _icon;
        public MessageBoxIcon Icon
        {
            get { return _icon; }
            set
            {
                var changed = _icon != value;

                _icon = value;

                if (changed)
                {
                    OnPropertyChanged("Icon");
                }
            }
        }

        private int _timeoutMs;
        public int TimeoutMs
        {
            get { return _timeoutMs; }
            set
            {
                var changed = _timeoutMs != value;

                _timeoutMs = value;

                if (changed)
                {
                    OnPropertyChanged("TimeoutMs");
                }
            }
        }

        private void Close(MessageBoxResult result)
        {
            _closeAction(result);
        }
    }
}
