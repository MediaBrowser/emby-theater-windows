using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;
using MediaBrowser.Theater.DefaultTheme.ItemList.ViewModels;

namespace MediaBrowser.Theater.DefaultTheme.ItemList.Views
{
    /// <summary>
    ///     Interaction logic for ItemListView.xaml
    /// </summary>
    public partial class ItemListView
    {
        public ItemListView()
        {
            InitializeComponent();

            // this is horrible
            Loaded += (s, e) => {
                MoveFocus(new TraversalRequest(FocusNavigationDirection.First));

                var context = DataContext as ItemListViewModel;
                
                context.Items.CollectionChanged += async (sender, args) => {
                    if (IsKeyboardFocusWithin && args.Action == NotifyCollectionChangedAction.Reset) {
                        await Task.Delay(100);
                        MoveFocus(new TraversalRequest(FocusNavigationDirection.First));
                    }
                };
            };

            Panorama.PropertyChanged += (sender, arg) => {
                if (arg.PropertyName == "Indices") {
                    var first = Panorama.Indices.FirstOrDefault();
                    if (first != null) {
                        Index.ScrollIntoView(first);
                    }
                }
            };
        }
    }
}