using System;
using System.Windows;
using System.Windows.Controls;

namespace MediaBrowser.Theater.Presentation.Controls
{
    [TemplatePart(Name = "PART_MinimizeButton", Type = typeof (Button))]
    [TemplatePart(Name = "PART_MaximizeButton", Type = typeof (Button))]
    [TemplatePart(Name = "PART_RestoreButton", Type = typeof (Button))]
    [TemplatePart(Name = "PART_CloseButton", Type = typeof (Button))]
    [TemplateVisualState(Name = "Visible", GroupName="ButtonVisibility")]
    [TemplateVisualState(Name = "Hidden", GroupName = "ButtonVisibility")]
    public class WindowCommands : Control
    {
        private Button _closeButton;
        private Button _maximizeButton;
        private Button _minimizeButton;
        private Button _restoreButton;
        private WindowState _state;
        private Window _window;
        
        public bool ShowButtons
        {
            get { return (bool)GetValue(ShowButtonsProperty); }
            set { SetValue(ShowButtonsProperty, value); }
        }

        public static readonly DependencyProperty ShowButtonsProperty =
            DependencyProperty.Register("ShowButtons", typeof(bool), typeof(WindowCommands), new PropertyMetadata(true, OnShowButtonsChanged));

        private static void OnShowButtonsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var commands = d as WindowCommands;
            if (commands != null) {
                if ((bool) e.NewValue) {
                    VisualStateManager.GoToState(commands, "Visible", true);
                } else {
                    VisualStateManager.GoToState(commands, "Hidden", true);
                }
            }
        }

        static WindowCommands()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof (WindowCommands), new FrameworkPropertyMetadata(typeof (WindowCommands)));
        }

        public WindowCommands()
        {
            Loaded += (s, e) => {
                _window = Window.GetWindow(this);
                if (_window != null) {
                    _window.StateChanged += UpdateWindowState;

                    State = _window.WindowState;
                }
            };

            Unloaded += (s, e) => {
                if (_window != null) {
                    _window.StateChanged -= UpdateWindowState;
                }
            };
        }

        private Button CloseButton
        {
            get { return _closeButton; }
            set
            {
                if (_closeButton != null) {
                    _closeButton.Click -= _closeButton_Click;
                }

                _closeButton = value;

                if (_closeButton != null) {
                    _closeButton.Click += _closeButton_Click;
                }
            }
        }

        private Button MaximizeButton
        {
            get { return _maximizeButton; }
            set
            {
                if (_maximizeButton != null) {
                    _maximizeButton.Click -= _maximizeButton_Click;
                }

                _maximizeButton = value;

                if (_maximizeButton != null) {
                    _maximizeButton.Click += _maximizeButton_Click;
                }
            }
        }

        private Button MinimizeButton
        {
            get { return _minimizeButton; }
            set
            {
                if (_minimizeButton != null) {
                    _minimizeButton.Click -= _minimizeButton_Click;
                }

                _minimizeButton = value;

                if (_minimizeButton != null) {
                    _minimizeButton.Click += _minimizeButton_Click;
                }
            }
        }

        private Button RestoreButton
        {
            get { return _restoreButton; }
            set
            {
                if (_restoreButton != null) {
                    _restoreButton.Click -= _restoreButton_Click;
                }

                _restoreButton = value;

                if (_restoreButton != null) {
                    _restoreButton.Click += _restoreButton_Click;
                }
            }
        }

        private WindowState State
        {
            get { return _state; }
            set
            {
                _state = value;

                if (_state == WindowState.Maximized) {
                    if (MaximizeButton != null) {
                        MaximizeButton.Visibility = Visibility.Collapsed;
                    }
                    if (RestoreButton != null) {
                        RestoreButton.Visibility = Visibility.Visible;
                    }
                }

                if (_state == WindowState.Normal) {
                    if (MaximizeButton != null) {
                        MaximizeButton.Visibility = Visibility.Visible;
                    }
                    if (RestoreButton != null) {
                        RestoreButton.Visibility = Visibility.Collapsed;
                    }
                }
            }
        }

        public override void OnApplyTemplate()
        {
            CloseButton = GetTemplateChild("PART_CloseButton") as Button;
            MaximizeButton = GetTemplateChild("PART_MaximizeButton") as Button;
            MinimizeButton = GetTemplateChild("PART_MinimizeButton") as Button;
            RestoreButton = GetTemplateChild("PART_RestoreButton") as Button;

            base.OnApplyTemplate();
        }

        private void _closeButton_Click(object sender, RoutedEventArgs e)
        {
            if (_window != null) {
                _window.Close();
            }
        }

        private void _maximizeButton_Click(object sender, RoutedEventArgs e)
        {
            if (_window != null) {
                _window.WindowState = WindowState.Maximized;
            }
        }

        private void _minimizeButton_Click(object sender, RoutedEventArgs e)
        {
            if (_window != null) {
                _window.WindowState = WindowState.Minimized;
            }
        }

        private void _restoreButton_Click(object sender, RoutedEventArgs e)
        {
            if (_window != null) {
                _window.WindowState = WindowState.Normal;
            }
        }

        private void UpdateWindowState(object sender, EventArgs e)
        {
            State = _window.WindowState;
        }
    }
}