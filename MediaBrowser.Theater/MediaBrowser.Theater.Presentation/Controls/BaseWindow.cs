using System;
using System.ComponentModel;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;

namespace MediaBrowser.Theater.Presentation.Controls
{
    /// <summary>
    ///     Provides a base class for all Windows. Handles auto-hiding the mouse cursor and content scaling.
    /// </summary>
    public abstract class BaseWindow : Window, INotifyPropertyChanged
    {
        public static readonly DependencyProperty WindowCommandsVisibilityProperty = DependencyProperty.Register("WindowCommandsVisibility", typeof (Visibility), typeof (BaseWindow), new PropertyMetadata(Visibility.Collapsed));
        private static int _showHideCount;
        private readonly object _cursorLock = new object();
        private Timer _activityTimer;

        /// <summary>
        ///     The _content scale
        /// </summary>
        private double _contentScale = 1;

        /// <summary>
        ///     The _is mouse idle
        /// </summary>
        private bool _isMouseIdle;

        private DateTime _lastMouseInput;

        /// <summary>
        ///     The _last mouse move point
        /// </summary>
        private Point? _lastMouseMovePoint;

        /// <summary>
        ///     Initializes a new instance of the <see cref="BaseWindow" /> class.
        /// </summary>
        protected BaseWindow()
        {
            SizeChanged += MainWindow_SizeChanged;
            Loaded += BaseWindow_Loaded;
        }

        /// <summary>
        ///     Gets the content scale.
        /// </summary>
        /// <value>The content scale.</value>
        public double ContentScale
        {
            get { return _contentScale; }
            private set
            {
                bool changed = !_contentScale.Equals(value);

                _contentScale = value;

                if (changed) {
                    OnPropertyChanged("ContentScale");
                }
            }
        }

        protected virtual bool ModifyWindowsFormsCursor
        {
            get { return false; }
        }

        /// <summary>
        ///     Gets or sets a value indicating whether this instance is mouse idle.
        /// </summary>
        /// <value><c>true</c> if this instance is mouse idle; otherwise, <c>false</c>.</value>
        public bool IsMouseIdle
        {
            get { return _isMouseIdle; }
            set
            {
                bool changed = _isMouseIdle != value;

                _isMouseIdle = value;

                if (changed) {
                    if (value) {
                        HideCursor();
                    } else {
                        ShowCursor();
                    }
                }
            }
        }

        public Visibility WindowCommandsVisibility
        {
            get { return (Visibility) GetValue(WindowCommandsVisibilityProperty); }
            set { SetValue(WindowCommandsVisibilityProperty, value); }
        }

        /// <summary>
        ///     Occurs when [property changed].
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        private void BaseWindow_Loaded(object sender, RoutedEventArgs e)
        {
            IsMouseIdle = true;

            _activityTimer = new Timer(TimerCallback, null, 250, 250);
        }

        /// <summary>
        ///     Called when [property changed].
        /// </summary>
        /// <param name="info">The info.</param>
        public void OnPropertyChanged(String info)
        {
            if (PropertyChanged != null) {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }

        protected void HideCursor()
        {
            Dispatcher.InvokeAsync(() => {
                WindowCommandsVisibility = Visibility.Collapsed;
                Cursor = Cursors.None;

                if (ModifyWindowsFormsCursor) {
                    lock (_cursorLock) {
                        while (_showHideCount > -1) {
                            System.Windows.Forms.Cursor.Hide();
                            _showHideCount--;
                        }
                    }
                }

                OnPropertyChanged("IsMouseIdle");
            }, DispatcherPriority.Background);
        }

        protected void ShowCursor()
        {
            Dispatcher.InvokeAsync(() => {
                WindowCommandsVisibility = Visibility.Visible;
                Cursor = Cursors.Arrow;

                lock (_cursorLock) {
                    while (_showHideCount < 0) {
                        System.Windows.Forms.Cursor.Show();
                        _showHideCount++;
                    }
                }

                OnPropertyChanged("IsMouseIdle");
            }, DispatcherPriority.Background);
        }

        /// <summary>
        ///     Handles OnMouseMove to auto-select the item that's being moused over
        /// </summary>
        /// <param name="e">Provides data for <see cref="T:System.Windows.Input.MouseEventArgs" />.</param>
        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            // Store the last position for comparison purposes
            // Even if the mouse is not moving this event will fire as elements are showing and hiding
            Point pos = e.GetPosition(this);

            if (!_lastMouseMovePoint.HasValue) {
                _lastMouseMovePoint = pos;
                return;
            }

            if (pos == _lastMouseMovePoint.Value) {
                return;
            }

            _lastMouseMovePoint = pos;

            OnMouseMove();
        }

        protected override void OnMouseWheel(MouseWheelEventArgs e)
        {
            base.OnMouseWheel(e);

            OnMouseMove();
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            DisposeActivityTimer();

            base.OnClosing(e);
        }

        protected void OnMouseMove()
        {
            _lastMouseInput = DateTime.Now;
            IsMouseIdle = false;
        }

        private void TimerCallback(object state)
        {
            IsMouseIdle = (DateTime.Now - _lastMouseInput).TotalMilliseconds > 4000;
        }

        /// <summary>
        ///     Disposes the Activity Timer
        /// </summary>
        public void DisposeActivityTimer()
        {
            if (_activityTimer != null) {
                _activityTimer.Dispose();
            }
        }

        /// <summary>
        ///     Handles the SizeChanged event of the MainWindow control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="SizeChangedEventArgs" /> instance containing the event data.</param>
        private void MainWindow_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            ContentScale = e.NewSize.Height/1080;
        }
    }
}