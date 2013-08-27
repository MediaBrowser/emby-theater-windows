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
    /// Interaction logic for StartupWizardXyVsFilter.xaml
    /// </summary>
    public partial class StartupWizardXyVsFilter : BasePage
    {
        private readonly IPresentationManager _presentation;
        private readonly INavigationService _nav;
        private readonly IMediaFilters _mediaFilters;

        private readonly Progress<double> _installProgress = new Progress<double>();

        public StartupWizardXyVsFilter(INavigationService nav, IPresentationManager presentation, IMediaFilters mediaFilters)
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

            try
            {
                Progress.Visibility = Visibility.Visible;

                await _mediaFilters.InstallXyVsFilter(_installProgress, _cancellationTokenSource.Token);
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
            await _nav.Navigate(new StartupWizardFinish(_nav, _presentation));
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
                var isInstalled = _mediaFilters.IsXyVsFilterInstalled();

                BtnNext.IsEnabled = isInstalled;
                BtnInstall.Visibility = isInstalled ? Visibility.Collapsed : Visibility.Visible;
                PanelInstalled.Visibility = isInstalled ? Visibility.Visible : Visibility.Collapsed;
            }
            catch
            {
                BtnNext.IsEnabled = false;
                BtnInstall.Visibility = Visibility.Visible;
                PanelInstalled.Visibility = Visibility.Collapsed;
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
