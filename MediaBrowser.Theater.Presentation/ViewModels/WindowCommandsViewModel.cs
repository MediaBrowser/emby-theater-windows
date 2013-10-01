using MediaBrowser.Common;
using MediaBrowser.Theater.Interfaces.Reflection;
using MediaBrowser.Theater.Interfaces.ViewModels;
using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;

namespace MediaBrowser.Theater.Presentation.ViewModels
{
    [TypeDescriptionProvider(typeof(HyperTypeDescriptionProvider))]
    public class WindowCommandsViewModel : BaseViewModel, IDisposable
    {
        private readonly Window _window;
        private readonly IApplicationHost _appHost;

        public ICommand MinimizeCommand { get; private set; }
        public ICommand MaximizeCommand { get; private set; }
        public ICommand CloseApplicationCommand { get; private set; }
        public ICommand UndoMaximizeCommand { get; private set; }
        
        public WindowCommandsViewModel(Window window, IApplicationHost appHost)
        {
            _window = window;
            _appHost = appHost;

            _window.StateChanged += _window_StateChanged;

            MinimizeCommand = new RelayCommand(Minimize);
            MaximizeCommand = new RelayCommand(Maximize);
            CloseApplicationCommand = new RelayCommand(CloseApplication);
            UndoMaximizeCommand = new RelayCommand(UndoMaximize);

            UpdateProperties();
        }

        private bool _isMaximized;
        public bool IsMaximized
        {
            get { return _isMaximized; }
            set
            {
                var changed = _isMaximized != value;

                _isMaximized = value;

                if (changed)
                {
                    OnPropertyChanged("IsMaximized");
                }
            }
        }

        private bool _isMinimized;
        public bool IsMinimized
        {
            get { return _isMinimized; }
            set
            {
                var changed = _isMinimized != value;

                _isMinimized = value;

                if (changed)
                {
                    OnPropertyChanged("IsMinimized");
                }
            }
        }

        void _window_StateChanged(object sender, EventArgs e)
        {
            UpdateProperties();
        }

        private void UpdateProperties()
        {
            var state = _window.WindowState;

            IsMaximized = state == WindowState.Maximized;
            IsMinimized = state == WindowState.Minimized;
        }

        public void Dispose()
        {
            _window.StateChanged -= _window_StateChanged;
        }

        public void Minimize(object commandParameter)
        {
            SetWindowState(WindowState.Minimized);
        }

        public void Maximize(object commandParameter)
        {
            SetWindowState(WindowState.Maximized);
        }

        public void UndoMaximize(object commandParameter)
        {
            SetWindowState(WindowState.Normal);
        }

        private void SetWindowState(WindowState state)
        {
            //var focusedElement = FocusManager.GetFocusedElement(_window);

            _window.WindowState = state;

            //if (focusedElement != null)
            {
                //focusedElement.Focus();
            }
        }

        public void CloseApplication(object commandParameter)
        {
            _appHost.Shutdown();
        }
    }
}
