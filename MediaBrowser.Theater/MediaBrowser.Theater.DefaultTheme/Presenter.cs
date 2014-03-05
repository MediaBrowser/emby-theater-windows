using System;
using System.Threading.Tasks;
using System.Windows;
using MediaBrowser.Theater.Api.Events;
using MediaBrowser.Theater.Api.Navigation;
using MediaBrowser.Theater.Api.UserInterface;
using MediaBrowser.Theater.DefaultTheme.Configuration;
using MediaBrowser.Theater.DefaultTheme.Core.ViewModels;

namespace MediaBrowser.Theater.DefaultTheme
{
    public struct ShowPageEvent
    {
        public IViewModel ViewModel { get; set; }
    }

    public struct ShowNotificationEvent
    {
        public IViewModel ViewModel { get; set; }
    }

    public class WindowManager
    {
        private INavigator _navigator;
        private MainWindow _mainWindow;
        private PopupWindow _currentPopup;

        public WindowManager(INavigator navigator)
        {
            _navigator = navigator;
        }

        public MainWindow MainWindow
        {
            get { return _mainWindow; }
        }

        private void FocusMainWindow()
        {
            var rootVm = _mainWindow.DataContext as RootViewModel;
            if (rootVm != null) {
                rootVm.IsInFocus = true;
            }
        }

        private void UnfocusMainWindow()
        {
            var rootVm = _mainWindow.DataContext as RootViewModel;
            if (rootVm != null) {
                rootVm.IsInFocus = false;
            }
        }

        public async Task ClosePopup()
        {
            if (_currentPopup != null) {
                var context = _currentPopup.DataContext as IViewModel;
                if (context != null) {
                    await context.Close();
                }

                _currentPopup.Close();
                _currentPopup = null;
            }
        }

        public async Task ShowPopup(IViewModel contents)
        {
            await ClosePopup();

            _currentPopup = new PopupWindow(_navigator) {
                DataContext = contents
            };

            EventHandler windowClosed = null;
            windowClosed = (sender, args) => {
                FocusMainWindow();
                _currentPopup.Closed -= windowClosed;
            };

            _currentPopup.Closed += windowClosed;

            UnfocusMainWindow();
            _currentPopup.ShowModal(_mainWindow);
        }

        public MainWindow CreateMainWindow(PluginConfiguration config, IViewModel viewModel)
        {
            var window = new MainWindow {
                DataContext = viewModel
            };

            // Restore window position/size
            if (config.WindowState.HasValue) {
                // Set window state
                window.WindowState = config.WindowState.Value;

                // Set position if not maximized
                if (config.WindowState.Value != WindowState.Maximized) {
                    double left = 0;
                    double top = 0;

                    // Set left
                    if (config.WindowLeft.HasValue) {
                        window.WindowStartupLocation = WindowStartupLocation.Manual;
                        window.Left = left = Math.Max(config.WindowLeft.Value, 0);
                    }

                    // Set top
                    if (config.WindowTop.HasValue) {
                        window.WindowStartupLocation = WindowStartupLocation.Manual;
                        window.Top = top = Math.Max(config.WindowTop.Value, 0);
                    }

                    // Set width
                    if (config.WindowWidth.HasValue) {
                        window.Width = Math.Min(config.WindowWidth.Value, SystemParameters.VirtualScreenWidth - left);
                    }

                    // Set height
                    if (config.WindowHeight.HasValue) {
                        window.Height = Math.Min(config.WindowHeight.Value, SystemParameters.VirtualScreenHeight - top);
                    }
                }
            }

            _mainWindow = window;
            return window;
        }

        public void SaveWindowPosition(PluginConfiguration config)
        {
            if (_mainWindow != null) {
                // Save window position
                config.WindowState = _mainWindow.WindowState;
                config.WindowTop = _mainWindow.Top;
                config.WindowLeft = _mainWindow.Left;
                config.WindowWidth = _mainWindow.Width;
                config.WindowHeight = _mainWindow.Height;
            }
        }
    }

    public class Presenter
        : IPresenter
    {
        private readonly IEventBus<ShowNotificationEvent> _showNotificationEvent;
        private readonly IEventBus<ShowPageEvent> _showPageEvent;

        private readonly WindowManager _windowManager;
        
        public Presenter(IEventAggregator events, WindowManager windowManager)
        {
            _showPageEvent = events.Get<ShowPageEvent>();
            _showNotificationEvent = events.Get<ShowNotificationEvent>();
            _windowManager = windowManager;
        }

        public async Task ShowPage(IViewModel contents)
        {
            await _windowManager.ClosePopup();
            _showPageEvent.Publish(new ShowPageEvent { ViewModel = contents });
        }

        public Task ShowPopup(IViewModel contents)
        {
            return _windowManager.ShowPopup(contents);
        }

        public Task ShowNotification(IViewModel contents)
        {
            _showNotificationEvent.Publish(new ShowNotificationEvent { ViewModel = contents });
            return Task.FromResult(0);
        }
    }
}