using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MediaBrowser.Theater.Api.UserInterface
{
    public class ButtonNotificationViewModel
        : NotificationViewModel
    {
        private ICommand _pressedCommand;

        public ButtonNotificationViewModel(TimeSpan duration) : base(duration) { }

        public ICommand PressedCommand
        {
            get { return _pressedCommand; }
            set
            {
                if (Equals(value, _pressedCommand)) {
                    return;
                }
                _pressedCommand = value;
                OnPropertyChanged();
            }
        }
    }

    public class NotificationViewModel
        : BaseViewModel
    {
        private readonly TimeSpan _duration;
        private IViewModel _contents;
        private IViewModel _icon;

        public IViewModel Contents
        {
            get { return _contents; }
            set
            {
                if (value == _contents) {
                    return;
                }
                _contents = value;
                OnPropertyChanged();
                OnPropertyChanged("HasContents");
            }
        }

        public IViewModel Icon
        {
            get { return _icon; }
            set
            {
                if (Equals(value, _icon)) {
                    return;
                }
                _icon = value;
                OnPropertyChanged();
                OnPropertyChanged("HasIcon");
            }
        }

        public bool HasContents
        {
            get { return Contents != null; }
        }

        public bool HasIcon
        {
            get { return Icon != null; }
        }

        public NotificationViewModel(TimeSpan duration)
        {
            _duration = duration;
        }

        public override bool IsActive
        {
            get { return base.IsActive; }
            protected set
            {
                base.IsActive = value;
                if (value && _duration != Timeout.InfiniteTimeSpan) {
                    Task.Run(async () => {
                        await Task.Delay(_duration);
                        await Close();
                    });
                }
            }
        }
    }
}