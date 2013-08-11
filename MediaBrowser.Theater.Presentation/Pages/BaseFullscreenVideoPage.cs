using MediaBrowser.Theater.Interfaces.Presentation;
using MediaBrowser.Theater.Interfaces.UserInput;
using System;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Threading;

namespace MediaBrowser.Theater.Presentation.Pages
{
    public abstract class BaseFullscreenVideoPage : BasePage, IFullscreenVideoPage, ISupportsThemeMedia
    {
        private readonly IUserInputManager _userInputManager;

        private System.Threading.Timer _activityTimer;
        private DateTime _lastMouseInput;

        /// <summary>
        /// The _is mouse idle
        /// </summary>
        private bool _isMouseIdle = true;

        protected BaseFullscreenVideoPage(IUserInputManager userInputManager)
        {
            _userInputManager = userInputManager;
        }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is mouse idle.
        /// </summary>
        /// <value><c>true</c> if this instance is mouse idle; otherwise, <c>false</c>.</value>
        public bool IsMouseIdle
        {
            get { return _isMouseIdle; }
            set
            {
                var changed = _isMouseIdle != value;

                _isMouseIdle = value;

                if (changed)
                {
                    Dispatcher.InvokeAsync(OnMouseIdleChanged, DispatcherPriority.Background);
                }
            }
        }
        
        protected virtual void OnMouseIdleChanged()
        {
            
        }

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);

            Loaded += FullscreenVideoPage_Loaded;
            Unloaded += FullscreenVideoPage_Unloaded;
        }

        void _userInputManager_MouseMove(object sender, MouseEventArgs e)
        {
            _lastMouseInput = DateTime.Now;
        }

        private void TimerCallback(object state)
        {
            IsMouseIdle = (DateTime.Now - _lastMouseInput).TotalMilliseconds > 4000;
        }

        void FullscreenVideoPage_Unloaded(object sender, RoutedEventArgs e)
        {
            if (_activityTimer != null)
            {
                _activityTimer.Dispose();
                _activityTimer = null;
            }

            _userInputManager.MouseMove -= _userInputManager_MouseMove;
        }

        void FullscreenVideoPage_Loaded(object sender, RoutedEventArgs e)
        {
            _activityTimer = new System.Threading.Timer(TimerCallback, null, 100, 100);

            _userInputManager.MouseMove += _userInputManager_MouseMove;
        }
    }
}
