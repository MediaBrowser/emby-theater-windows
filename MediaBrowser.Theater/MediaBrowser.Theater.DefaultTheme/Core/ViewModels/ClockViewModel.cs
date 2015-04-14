using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;
using MediaBrowser.Theater.Api.UserInterface;
using MediaBrowser.Theater.Presentation.ViewModels;

namespace MediaBrowser.Theater.DefaultTheme.Core.ViewModels
{
    public class ClockViewModel
        : BaseViewModel
    {
        private readonly Timer _timer;
        private readonly Dispatcher _dispatcher;

        private string _timeLeft;
        public string TimeLeft
        {
            get { return _timeLeft; }

            set
            {
                var changed = !string.Equals(_timeLeft, value);

                _timeLeft = value;
                if (changed) {
                    OnPropertyChanged();
                }
            }
        }

        private string _timeRight;
        public string TimeRight
        {
            get { return _timeRight; }

            set
            {
                var changed = !string.Equals(_timeRight, value);

                _timeRight = value;
                if (changed) {
                    OnPropertyChanged();
                }
            }
        }

        private void UpdateTime()
        {
            var now = DateTime.Now;

            var nowString = now.ToShortTimeString();

            if (nowString.IndexOf("am", StringComparison.OrdinalIgnoreCase) != -1 ||
                nowString.IndexOf("pm", StringComparison.OrdinalIgnoreCase) != -1) {
                TimeLeft = now.ToString("h:mm");
                var time = now.ToString("t");
                var values = time.Split(' ');
                TimeRight = values[values.Length - 1].ToLower();
            } else {
                TimeLeft = now.ToShortTimeString();
                TimeRight = string.Empty;
            }
        }

        public ClockViewModel()
        {
            _dispatcher = Dispatcher.CurrentDispatcher;
            _timer = new Timer(ClockTimerCallback, null, 0, 10000);
        }

        private void ClockTimerCallback(object state)
        {
            UpdateTime();
        }
    }
}
