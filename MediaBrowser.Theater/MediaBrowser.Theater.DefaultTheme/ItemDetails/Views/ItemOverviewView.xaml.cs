using System.Threading;
using System.Windows.Controls;
using MediaBrowser.Theater.DefaultTheme.ItemDetails.ViewModels;

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