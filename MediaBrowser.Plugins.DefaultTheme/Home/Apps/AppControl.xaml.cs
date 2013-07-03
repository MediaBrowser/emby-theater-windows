using MediaBrowser.Theater.Interfaces.Presentation;
using System.Windows;
using System.Windows.Controls;

namespace MediaBrowser.Plugins.DefaultTheme.Home.Apps
{
    /// <summary>
    /// Interaction logic for AppControl.xaml
    /// </summary>
    public partial class AppControl : UserControl
    {
        /// <summary>
        /// Gets the view model.
        /// </summary>
        /// <value>The view model.</value>
        public ITheaterApp ViewModel
        {
            get { return DataContext as ITheaterApp; }
        }
        
        public AppControl()
        {
            InitializeComponent();

            DataContextChanged += AppControl_DataContextChanged;
        }

        void AppControl_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            OnAppChanged();
        }

        private void OnAppChanged()
        {
            TxtName.Text = ViewModel.Name;

            PanelImage.Children.Clear();

            PanelImage.Children.Add(ViewModel.GetTileImage());
        }
    }
}
