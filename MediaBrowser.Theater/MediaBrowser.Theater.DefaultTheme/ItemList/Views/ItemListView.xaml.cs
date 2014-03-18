using System;
using System.Collections.Generic;
using System.ComponentModel;
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
using MediaBrowser.Theater.DefaultTheme.ItemList.ViewModels;

namespace MediaBrowser.Theater.DefaultTheme.ItemList.Views
{
    /// <summary>
    /// Interaction logic for ItemListView.xaml
    /// </summary>
    public partial class ItemListView : UserControl
    {
        public ItemListView()
        {
            InitializeComponent();

            // this is horrible
            Loaded += (s, e) => {
                var context = DataContext as ItemListViewModel;
                context.Items.CollectionChanged += async (sender, args) => {
                    if (args.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Reset) {
                        await Task.Delay(100);
                        MoveFocus(new TraversalRequest(FocusNavigationDirection.First));
                    }
                };
            };
        }
    }
}
