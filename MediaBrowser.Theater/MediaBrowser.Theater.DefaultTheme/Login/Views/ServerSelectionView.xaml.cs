using System.Windows.Controls;
using System.Windows.Input;

namespace MediaBrowser.Theater.DefaultTheme.Login.Views
{
    /// <summary>
    ///     Interaction logic for ServerSelectionView.xaml
    /// </summary>
    public partial class ServerSelectionView : UserControl
    {
        public ServerSelectionView()
        {
            InitializeComponent();

            Loaded += (s, e) => MoveFocus(new TraversalRequest(FocusNavigationDirection.First));
        }
    }
}