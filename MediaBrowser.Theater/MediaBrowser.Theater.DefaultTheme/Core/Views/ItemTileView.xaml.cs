using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using MediaBrowser.Theater.Api.Commands;

namespace MediaBrowser.Theater.DefaultTheme.Core.Views
{
    /// <summary>
    /// Interaction logic for ItemTileView.xaml
    /// </summary>
    public partial class ItemTileView : UserControl
    {
        public ItemTileView()
        {
            InitializeComponent();
        }

//        private void ReceiveCommand(object sender, CommandRoutedEventArgs e)
//        {
//            Debug.WriteLine("command received " + e.Command);
//        }
    }
}
