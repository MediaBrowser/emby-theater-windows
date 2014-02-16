using System;
using System.Threading.Tasks;
using MediaBrowser.Theater.Api.UserInterface;

namespace MediaBrowser.Theater.Presentation.ViewModels
{
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
                if (value) {
                    Task.Run(async () => {
                        await Task.Delay(_duration);
                        await Close();
                    });
                }
            }
        }
    }
}