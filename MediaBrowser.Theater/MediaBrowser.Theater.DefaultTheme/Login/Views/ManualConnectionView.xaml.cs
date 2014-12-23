using System.Windows.Controls;

namespace MediaBrowser.Theater.DefaultTheme.Login.Views
{
    /// <summary>
    ///     Interaction logic for ManualConnectionView.xaml
    /// </summary>
    public partial class ManualConnectionView : UserControl
    {
        public ManualConnectionView()
        {
            InitializeComponent();

            Loaded += (s, e) => AddressTextField.Focus();
        }
    }
}