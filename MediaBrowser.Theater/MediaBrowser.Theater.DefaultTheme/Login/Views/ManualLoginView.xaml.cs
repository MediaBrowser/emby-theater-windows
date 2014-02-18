using System;
using System.Collections.Generic;
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

namespace MediaBrowser.Theater.DefaultTheme.Login.Views
{
    /// <summary>
    /// Interaction logic for ManualLoginView.xaml
    /// </summary>
    public partial class ManualLoginView : UserControl
    {
        public ManualLoginView()
        {
            InitializeComponent();
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (e.Key == Key.Down) {
                var focused = Keyboard.FocusedElement as FrameworkElement;
                focused.MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
                e.Handled = true;
            }

            if (e.Key == Key.Up) {
                var focused = Keyboard.FocusedElement as FrameworkElement;
                //var up = focused.PredictFocus(FocusNavigationDirection.Previous);
                focused.MoveFocus(new TraversalRequest(FocusNavigationDirection.Previous));
                e.Handled = true;
            }

            base.OnKeyDown(e);
        }
    }
}
