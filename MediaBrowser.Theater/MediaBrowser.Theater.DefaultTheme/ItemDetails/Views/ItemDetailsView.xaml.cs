using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;

namespace MediaBrowser.Theater.DefaultTheme.ItemDetails.Views
{
    /// <summary>
    ///     Interaction logic for ItemDetailsView.xaml
    /// </summary>
    public partial class ItemDetailsView : UserControl
    {
        public ItemDetailsView()
        {
            InitializeComponent();

            Loaded += (s, e) => MoveFocus(new TraversalRequest(FocusNavigationDirection.First));
            DataContextChanged += async (s, e) => {
                // horrible unreliable hack, because WPF is rebinding the control instead of recreating
                await Task.Delay(100);
                MoveFocus(new TraversalRequest(FocusNavigationDirection.First));
            };
        }
    }
}