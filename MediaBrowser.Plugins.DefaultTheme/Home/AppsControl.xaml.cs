using MediaBrowser.Model.Logging;
using MediaBrowser.Theater.Interfaces.Presentation;
using MediaBrowser.Theater.Interfaces.Session;
using MediaBrowser.Theater.Presentation.Controls;
using MediaBrowser.Theater.Presentation.ViewModels;
using System;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Data;

namespace MediaBrowser.Plugins.DefaultTheme.Home
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

            var items = new RangeObservableCollection<AppViewModel>();
            var view = (ListCollectionView)CollectionViewSource.GetDefaultView(items);
            LstItems.ItemsSource = view;

            items.AddRange(_presentation.GetApps(_session.CurrentUser)
                .Select(i => new AppViewModel(_presentation, _logger) { App = i }));
        }

        void LstItems_ItemInvoked(object sender, ItemEventArgs<object> e)
        {
            var app = (AppViewModel)e.Argument;

            app.Launch();
        }
    }
}
