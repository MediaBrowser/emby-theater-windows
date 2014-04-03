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
        }
    }
}