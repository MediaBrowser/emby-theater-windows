using MediaBrowser.Model.ApiClient;
using MediaBrowser.Theater.Interfaces.Navigation;
using MediaBrowser.Theater.Interfaces.Presentation;
using MediaBrowser.Theater.Interfaces.System;
using MediaBrowser.Theater.Presentation.Pages;
using System;
using System.Threading;
using System.Windows;

namespace MediaBrowser.UI.StartupWizard
{
    /// <summary>
    /// Interaction logic for StartupWizardLav.xaml
    /// </summary>
    public partial class StartupWizardLav : BasePage
    {
        private readonly IPresentationManager _presentation;
        private readonly INavigationService _nav;
        private readonly IMediaFilters _mediaFilters;

        private readonly Progress<double> _installProgress = new Progress<double>();

        public StartupWizardLav(INavigationService nav, IPresentationManager presentation, IMediaFilters mediaFilters)
        {
            _nav = nav;
            _presentation = presentation;
            _mediaFilters = mediaFilters;
            InitializeComponent();
        }

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);

            Loaded += StartupWizardPage_Loaded;
            Unloaded += StartupWizardLav_Unloaded;
            BtnNext.Click += BtnNext_Click;
            BtnBack.Click += BtnBack_Click;
            BtnInstall.Click += BtnInstall_Click;
        }

        private CancellationTokenSource _cancellationTokenSource;

        async void BtnInstall_Click(object sender, RoutedEventArgs e)
        {
            _cancellationTokenSource = new CancellationTokenSource();

            BtnInstall.IsEnabled = false;

            var isOnTop = _presentation.Window.Topmost;

            if (isOnTop)
            {
                _presentation.Window.Topmost = false;
            }

            try
            {
                Progress.Visibility = Visibility.Visible;

                await _mediaFilters.InstallLavFilters(_installProgress, _cancellationTokenSource.Token);
            }
            catch (OperationCanceledException)
            {

            }
            catch
            {
                _presentation.ShowDefaultErrorMessage();
            }
            finally
            {
                if (isOnTop)
                {
                    _presentation.Window.Topmost = true;
                }

                BtnInstall.IsEnabled = true;
                DisposeCancellationTokenSource();
                UpdateNextButtonEnabled();
                Progress.Visibility = Visibility.Collapsed;
            }
        }

        async void BtnBack_Click(object sender, RoutedEventArgs e)
        {
            await _nav.NavigateBack();
        }

        async void BtnNext_Click(object sender, RoutedEventArgs e)
        {
            await _nav.Navigate(new StartupWizardXyVsFilter(_nav, _presentation, _mediaFilters));
        }

        void StartupWizardPage_Loaded(object sender, RoutedEventArgs e)
        {
            _presentation.SetDefaultPageTitle();

            UpdateNextButtonEnabled();

            _installProgress.ProgressChanged += _installProgress_ProgressChanged;
        }

        void StartupWizardLav_Unloaded(object sender, RoutedEventArgs e)
        {
            _installProgress.ProgressChanged -= _installProgress_ProgressChanged;

            DisposeCancellationTokenSource();
        }

        void _installProgress_ProgressChanged(object sender, double e)
        {
            Dispatcher.InvokeAsync(() => Progress.Value = e);
        }

        private void UpdateNextButtonEnabled()
        {
            try
            {
                var isLavSplitterInstalled = _mediaFilters.IsLavSplitterInstalled();
                var isLavAudioInstalled = _mediaFilters.IsLavAudioInstalled();
                var isLavVideoInstalled = _mediaFilters.IsLavVideoInstalled();

                BtnNext.IsEnabled = isLavSplitterInstalled && isLavAudioInstalled && isLavVideoInstalled;

                BtnInstall.Visibility = BtnNext.IsEnabled ? Visibility.Collapsed : Visibility.Visible;

                PanelSplitterInstalled.Visibility = isLavSplitterInstalled ? Visibility.Visible : Visibility.Collapsed;
                PanelAudioInstalled.Visibility = isLavAudioInstalled ? Visibility.Visible : Visibility.Collapsed;
                PanelVideoInstalled.Visibility = isLavVideoInstalled ? Visibility.Visible : Visibility.Collapsed;

                PanelSplitterNotInstalled.Visibility = isLavSplitterInstalled ? Visibility.Collapsed : Visibility.Visible;
                PanelAudioNotInstalled.Visibility = isLavAudioInstalled ? Visibility.Collapsed : Visibility.Visible;
                PanelVideoNotInstalled.Visibility = isLavVideoInstalled ? Visibility.Collapsed : Visibility.Visible;
            }
            catch
            {
                BtnNext.IsEnabled = false;
                BtnInstall.Visibility = Visibility.Visible;
                PanelSplitterInstalled.Visibility = Visibility.Collapsed;
                PanelAudioInstalled.Visibility = Visibility.Collapsed;
                PanelVideoInstalled.Visibility = Visibility.Collapsed;

                PanelSplitterNotInstalled.Visibility = Visibility.Visible;
                PanelAudioNotInstalled.Visibility = Visibility.Visible;
                PanelVideoNotInstalled.Visibility = Visibility.Visible;
            }

            if (BtnInstall.Visibility == Visibility.Visible)
            {
                BtnInstall.Focus();
            }
            else
            {
                BtnNext.Focus();
            }
        }

        private void DisposeCancellationTokenSource()
        {
            if (_cancellationTokenSource != null)
            {
                _cancellationTokenSource.Cancel();
                _cancellationTokenSource.Dispose();
                _cancellationTokenSource = null;
            }
        }
    }
}
