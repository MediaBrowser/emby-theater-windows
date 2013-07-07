using System.Globalization;
using MediaBrowser.Model.Updates;
using System;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace MediaBrowser.Theater.Core.Settings
{
    /// <summary>
    /// Interaction logic for InProgressInstallation.xaml
    /// </summary>
    public partial class InProgressInstallation : UserControl
    {
        public InstallationInfo InstallationInfo { get; private set; }
        public CancellationTokenSource CancellationTokenSource { get; private set; }

        private DispatcherTimer _progressUpdateTimer;

        public InProgressInstallation(InstallationInfo installationInfo, CancellationTokenSource cancellationToken)
        {
            CancellationTokenSource = cancellationToken;
            InstallationInfo = installationInfo;
            InitializeComponent();
        }

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);

            Unloaded += InProgressInstallation_Unloaded;

            _progressUpdateTimer = new DispatcherTimer(TimeSpan.FromMilliseconds(100), DispatcherPriority.Normal, OnUpdateTimerTick,
                                                      Dispatcher);

            TxtName.Text = InstallationInfo.Name + " " + InstallationInfo.Version;
            BtnCancel.Click += BtnCancel_Click;
        }

        void InProgressInstallation_Unloaded(object sender, RoutedEventArgs e)
        {
            _progressUpdateTimer.Stop();
        }

        void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            var cancellationTokenSource = CancellationTokenSource;

            if (cancellationTokenSource != null && !cancellationTokenSource.IsCancellationRequested && InstallationInfo.PercentComplete < 100)
            {
                cancellationTokenSource.Cancel();
            }
        }

        private void OnUpdateTimerTick(object sender, EventArgs args)
        {
            var cancellationTokenSource = CancellationTokenSource;

            var percent = InstallationInfo.PercentComplete ?? 0;

            Progress.Value = percent;

            BtnCancel.Visibility = cancellationTokenSource != null && !cancellationTokenSource.IsCancellationRequested && percent < 100
                                       ? Visibility.Visible
                                       : Visibility.Collapsed;

            TxtPercent.Text = Math.Round(percent, 1).ToString(CultureInfo.CurrentCulture) + "%";
        }

        public void UpdateInstallationInfo(InstallationInfo info, CancellationTokenSource cancellationToken)
        {
            CancellationTokenSource = cancellationToken;

            InstallationInfo = info;
            TxtName.Text = InstallationInfo.Name + " " + InstallationInfo.Version;
        }
    }
}
