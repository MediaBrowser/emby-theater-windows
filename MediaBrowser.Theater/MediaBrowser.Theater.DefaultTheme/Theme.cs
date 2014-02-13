using System;
using System.Threading.Tasks;
using System.Windows;
using MediaBrowser.Common.Configuration;
using MediaBrowser.Common.Plugins;
using MediaBrowser.Model.Serialization;
using MediaBrowser.Theater.Api.UserInterface;
using MediaBrowser.Theater.Api.UserInterface.Navigation;
using MediaBrowser.Theater.DefaultTheme.Configuration;
using MediaBrowser.Theater.DefaultTheme.ViewModels;

namespace MediaBrowser.Theater.DefaultTheme
{
    public class Theme
        : BasePlugin<PluginConfiguration>, ITheme
    {
        private readonly RootViewModel _rootViewModel;
        private readonly TaskCompletionSource<object> _running;
        private App _application;
        private MainWindow _mainWindow;

        public Theme(IApplicationPaths applicationPaths, IXmlSerializer xmlSerializer, PresentationManager presentationManager, RootViewModel rootViewModel) : base(applicationPaths, xmlSerializer)
        {
            _rootViewModel = rootViewModel;
            _running = new TaskCompletionSource<object>();
            Presentation = presentationManager;
        }

        public override string Name
        {
            get { return "Default Theme"; }
        }

        public IPresentationManager Presentation { get; private set; }
        public INavigationService Navigation { get; private set; }

        public void Run()
        {
            _application = new App();
            ApplyPalette(_application);

            UIDispatchExtensions.ResetDispatcher();

            _mainWindow = CreateMainWindow();
            _application.Run(_mainWindow);

            Cleanup();

            _running.SetResult(null);
        }

        public Task Shutdown()
        {
            if (_application != null) {
                _application.Shutdown();
            } else {
                _running.SetResult(null);
            }

            return _running.Task;
        }

        private void Cleanup()
        {
            SaveWindowPosition();
        }

        private void SaveWindowPosition()
        {
            if (_mainWindow != null) {
                // Save window position
                Configuration.WindowState = _mainWindow.WindowState;
                Configuration.WindowTop = _mainWindow.Top;
                Configuration.WindowLeft = _mainWindow.Left;
                Configuration.WindowWidth = _mainWindow.Width;
                Configuration.WindowHeight = _mainWindow.Height;
                SaveConfiguration();
            }
        }

        private void ApplyPalette(Application app)
        {
            ResourceDictionary resources = Configuration.Palette.GetResources();
            foreach (object resource in resources.Keys) {
                object newValue = resources[resource];
                app.Resources[resource] = newValue;
            }
        }

        private MainWindow CreateMainWindow()
        {
            var window = new MainWindow {
                DataContext = _rootViewModel
            };

            // Restore window position/size
            if (Configuration.WindowState.HasValue) {
                // Set window state
                window.WindowState = Configuration.WindowState.Value;

                // Set position if not maximized
                if (Configuration.WindowState.Value != WindowState.Maximized) {
                    double left = 0;
                    double top = 0;

                    // Set left
                    if (Configuration.WindowLeft.HasValue) {
                        window.WindowStartupLocation = WindowStartupLocation.Manual;
                        window.Left = left = Math.Max(Configuration.WindowLeft.Value, 0);
                    }

                    // Set top
                    if (Configuration.WindowTop.HasValue) {
                        window.WindowStartupLocation = WindowStartupLocation.Manual;
                        window.Top = top = Math.Max(Configuration.WindowTop.Value, 0);
                    }

                    // Set width
                    if (Configuration.WindowWidth.HasValue) {
                        window.Width = Math.Min(Configuration.WindowWidth.Value, SystemParameters.VirtualScreenWidth - left);
                    }

                    // Set height
                    if (Configuration.WindowHeight.HasValue) {
                        window.Height = Math.Min(Configuration.WindowHeight.Value, SystemParameters.VirtualScreenHeight - top);
                    }
                }
            }

            return window;
        }
    }
}