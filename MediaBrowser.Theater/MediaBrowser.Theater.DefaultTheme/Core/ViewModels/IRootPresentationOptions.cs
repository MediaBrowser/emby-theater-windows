using System.ComponentModel;
using System.Runtime.CompilerServices;
using MediaBrowser.Theater.DefaultTheme.Annotations;

namespace MediaBrowser.Theater.DefaultTheme.Core.ViewModels
{
    public interface IHasRootPresentationOptions
    {
        RootPresentationOptions PresentationOptions { get; }
    }

    public class RootPresentationOptions
        : INotifyPropertyChanged
    {
        private bool _showMediaBrowserLogo;
        private bool _showCommandBar;
        private bool _showClock;
        private string _title;
        private bool _isFullScreenPage;
        private double _playbackBackgroundOpacity;

        public bool IsFullScreenPage
        {
            get { return _isFullScreenPage; }
            set
            {
                if (value.Equals(_isFullScreenPage)) {
                    return;
                }

                _isFullScreenPage = value;
                OnPropertyChanged();
            }
        }

        public bool ShowMediaBrowserLogo
        {
            get { return _showMediaBrowserLogo; }
            set
            {
                if (Equals(_showMediaBrowserLogo, value)) {
                    return;
                }

                _showMediaBrowserLogo = value;
                OnPropertyChanged();
            }
        }

        public bool ShowCommandBar
        {
            get { return _showCommandBar; }
            set
            {
                if (Equals(_showCommandBar, value)) {
                    return;
                }

                _showCommandBar = value;
                OnPropertyChanged();
            }
        }

        public bool ShowClock
        {
            get { return _showClock; }
            set
            {
                if (Equals(_showClock, value)) {
                    return;
                }

                _showClock = value;
                OnPropertyChanged();
            }
        }

        public string Title
        {
            get { return _title; }
            set
            {
                if (Equals(_title, value)) {
                    return;
                }

                _title = value;
                OnPropertyChanged();
            }
        }

        public double PlaybackBackgroundOpacity
        {
            get { return _playbackBackgroundOpacity; }
            set
            {
                if (value.Equals(_playbackBackgroundOpacity)) {
                    return;
                }

                _playbackBackgroundOpacity = value;
                OnPropertyChanged();
            }
        }

        public RootPresentationOptions()
        {
            ShowMediaBrowserLogo = true;
            ShowCommandBar = true;
            ShowClock = true;
            Title = null;
            PlaybackBackgroundOpacity = 0.7;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}