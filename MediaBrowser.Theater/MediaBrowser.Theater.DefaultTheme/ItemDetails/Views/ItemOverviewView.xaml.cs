using System.Windows.Controls;

namespace MediaBrowser.Theater.DefaultTheme.ItemDetails.Views
{
    /// <summary>
    ///     Interaction logic for ItemOverviewView.xaml
    /// </summary>
    public partial class ItemOverviewView : UserControl
    {
        public ItemOverviewView()
        {
            InitializeComponent();

            GotKeyboardFocus += (s, e) => BringIntoView();
        }
    }
}