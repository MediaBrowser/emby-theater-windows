using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace MediaBrowser.Plugins.DefaultTheme
{
    partial class AppResourcesResourceDictionary : ResourceDictionary
    { 
       public AppResourcesResourceDictionary()
       {
          InitializeComponent();
       }

        private void HeaderContentStackPanel_OnGotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            var sp = sender as StackPanel;
            if (sp != null)
            {
                var button = sp.Children.OfType<Button>().LastOrDefault(b => b.Visibility == Visibility.Visible);
                if (button != null)
                    FocusManager.SetFocusedElement(sp, button);
            }
        }
    }
}