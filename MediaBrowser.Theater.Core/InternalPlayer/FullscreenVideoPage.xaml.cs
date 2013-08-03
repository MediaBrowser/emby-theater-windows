using MediaBrowser.Theater.Interfaces.Presentation;
using MediaBrowser.Theater.Interfaces.Theming;
using MediaBrowser.Theater.Interfaces.UserInput;
using MediaBrowser.Theater.Presentation.Pages;
using System;
using System.Windows;
using System.Windows.Forms;

namespace MediaBrowser.Theater.Core.InternalPlayer
{
    /// <summary>
    /// Interaction logic for FullscreenVideoPage.xaml
    /// </summary>
    public partial class FullscreenVideoPage : BasePage, ISupportsThemeMedia, IFullscreenVideoPage
    {
        private readonly IThemeManager _themeManager;
        private readonly IUserInputManager _userInputManager;

        private System.Threading.Timer _activityTimer;
        
        public FullscreenVideoPage(IThemeManager themeManager, IUserInputManager userInputManager)
        {
            _themeManager = themeManager;
            _userInputManager = userInputManager;
            InitializeComponent();

            Loaded += FullscreenVideoPage_Loaded;
            Unloaded += FullscreenVideoPage_Unloaded;
        }

        private DateTime _lastMouseInput;

        /// <summary>
        /// The _is mouse idle
        /// </summary>
        private bool _isMouseIdle = true;
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
                    if (value)
                    {
                        Dispatcher.InvokeAsync(System.Windows.Forms.Cursor.Hide);
                    }
                    else
                    {
                        Dispatcher.InvokeAsync(System.Windows.Forms.Cursor.Show);
                    }
                }
            }
        }

        void _userInputManager_MouseMove(object sender, MouseEventArgs e)
        {
            _lastMouseInput = DateTime.Now;
        }

        private void TimerCallback(object state)
        {
            IsMouseIdle = (DateTime.Now - _lastMouseInput).TotalMilliseconds > 5000;
        }

        void FullscreenVideoPage_Unloaded(object sender, RoutedEventArgs e)
        {
            if (_activityTimer != null)
            {
                _activityTimer.Dispose();
                _activityTimer = null;
            }

            _userInputManager.MouseMove -= _userInputManager_MouseMove;
            System.Windows.Forms.Cursor.Show();

            _themeManager.CurrentTheme.SetGlobalContentVisibility(true);
        }

        void FullscreenVideoPage_Loaded(object sender, RoutedEventArgs e)
        {
            _activityTimer = new System.Threading.Timer(TimerCallback, null, 100, 100);

            _userInputManager.MouseMove += _userInputManager_MouseMove;
            System.Windows.Forms.Cursor.Hide();

            _themeManager.CurrentTheme.SetGlobalContentVisibility(false);
        }
    }
}
