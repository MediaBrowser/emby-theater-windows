using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using MediaBrowser.Theater.DefaultTheme.Home.ViewModels;

namespace MediaBrowser.Theater.DefaultTheme.Controls
{
    [ContentProperty("Content")]
    public class ContentPage : UserControl
    {
        // Using a DependencyProperty as the backing store for Header.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty HeaderProperty =
            DependencyProperty.Register("Header", typeof (object), typeof (ContentPage), new PropertyMetadata(null));

        // Using a DependencyProperty as the backing store for Footer.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty FooterProperty =
            DependencyProperty.Register("Footer", typeof (object), typeof (ContentPage), new PropertyMetadata(null));


        // Using a DependencyProperty as the backing store for HeaderHeight.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty HeaderHeightProperty =
            DependencyProperty.Register("HeaderHeight", typeof (double), typeof (ContentPage), new PropertyMetadata(HomeViewModel.HeaderHeight));


        // Using a DependencyProperty as the backing store for FooterHeight.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty FooterHeightProperty =
            DependencyProperty.Register("FooterHeight", typeof (double), typeof (ContentPage), new PropertyMetadata(HomeViewModel.FooterHeight));


        static ContentPage()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof (ContentPage), new FrameworkPropertyMetadata(typeof (ContentPage)));
        }

        public double HeaderHeight
        {
            get { return (double) GetValue(HeaderHeightProperty); }
            set { SetValue(HeaderHeightProperty, value); }
        }

        public double FooterHeight
        {
            get { return (double) GetValue(FooterHeightProperty); }
            set { SetValue(FooterHeightProperty, value); }
        }

        public object Header
        {
            get { return GetValue(HeaderProperty); }
            set { SetValue(HeaderProperty, value); }
        }

        public object Footer
        {
            get { return GetValue(FooterProperty); }
            set { SetValue(FooterProperty, value); }
        }
    }
}