using MediaBrowser.Model.Logging;
using MediaBrowser.Theater.Interfaces.Presentation;
using MediaBrowser.Theater.Interfaces.Session;
using MediaBrowser.Theater.Interfaces.Theming;
using MediaBrowser.Theater.Presentation.Controls;
using MediaBrowser.Theater.Presentation.ViewModels;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace MediaBrowser.Plugins.DefaultTheme.Home.Apps
{
    /// <summary>
    /// Interaction logic for AppsControl.xaml
    /// </summary>
    public partial class AppsControl : UserControl
    {
        private readonly IPresentationManager _presentation;
        private readonly ISessionManager _session;
        private readonly ILogger _logger;

        public AppsControl(IPresentationManager presentation, ISessionManager session, ILogger logger)
        {
            _presentation = presentation;
            _session = session;
            _logger = logger;
            InitializeComponent();
        }

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);

            LstItems.ItemInvoked += LstItems_ItemInvoked;

            var items = new RangeObservableCollection<ITheaterApp>();
            var view = (ListCollectionView)CollectionViewSource.GetDefaultView(items);
            LstItems.ItemsSource = view;

            items.AddRange(_presentation.GetApps(_session.CurrentUser));
        }

        async void LstItems_ItemInvoked(object sender, ItemEventArgs<object> e)
        {
            var app = (ITheaterApp) e.Argument;

            try
            {
                await app.Launch();
            }
            catch (Exception ex)
            {
                _logger.ErrorException("Error launching app {0}", ex, app.Name);

                _presentation.ShowMessage(new MessageBoxInfo
                {
                    Button = MessageBoxButton.OK,
                    Caption = "Error",
                    Icon = MessageBoxIcon.Error,
                    Text = ex.Message
                });
            }
        }
    }
}
