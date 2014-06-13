using System.Windows;
using System.Windows.Controls;

namespace MediaBrowser.Theater.DefaultTheme.Controls
{
    public class TileButton : Button
    {
        // Using a DependencyProperty as the backing store for Header.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty HeaderProperty =
            DependencyProperty.Register("Header", typeof (object), typeof (TileButton), new PropertyMetadata(null));


        // Using a DependencyProperty as the backing store for HorizontalHeaderAlignment.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty HorizontalHeaderAlignmentProperty =
            DependencyProperty.Register("HorizontalHeaderAlignment", typeof (HorizontalAlignment), typeof (TileButton), new PropertyMetadata(HorizontalAlignment.Stretch));


        // Using a DependencyProperty as the backing store for VerticalHeaderAlignment.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty VerticalHeaderAlignmentProperty =
            DependencyProperty.Register("VerticalHeaderAlignment", typeof (VerticalAlignment), typeof (TileButton), new PropertyMetadata(VerticalAlignment.Bottom));


        // Using a DependencyProperty as the backing store for AutoHideHeading.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty AutoHideHeadingProperty =
            DependencyProperty.Register("AutoHideHeading", typeof (bool), typeof (TileButton), new PropertyMetadata(false));


        static TileButton()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof (TileButton), new FrameworkPropertyMetadata(typeof (TileButton)));
        }

        public bool AutoHideHeading
        {
            get { return (bool) GetValue(AutoHideHeadingProperty); }
            set { SetValue(AutoHideHeadingProperty, value); }
        }

        public object Header
        {
            get { return GetValue(HeaderProperty); }
            set { SetValue(HeaderProperty, value); }
        }

        public HorizontalAlignment HorizontalHeaderAlignment
        {
            get { return (HorizontalAlignment) GetValue(HorizontalHeaderAlignmentProperty); }
            set { SetValue(HorizontalHeaderAlignmentProperty, value); }
        }

        public VerticalAlignment VerticalHeaderAlignment
        {
            get { return (VerticalAlignment) GetValue(VerticalHeaderAlignmentProperty); }
            set { SetValue(VerticalHeaderAlignmentProperty, value); }
        }
    }
}