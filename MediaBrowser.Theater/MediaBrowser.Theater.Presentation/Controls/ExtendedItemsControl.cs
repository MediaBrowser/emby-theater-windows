using System.Windows;
using System.Windows.Controls;

namespace MediaBrowser.Theater.Presentation.Controls
{
    public class ExtendedItemsControl
        : ItemsControl
    {
        static ExtendedItemsControl()
        {
            FocusableProperty.OverrideMetadata(typeof (ExtendedItemsControl), new FrameworkPropertyMetadata(false));
        }

        protected override DependencyObject GetContainerForItemOverride()
        {
            return new ExtendedContentControl();
        }

        protected override void PrepareContainerForItemOverride(DependencyObject element, object item)
        {
            var contentControl = element as ExtendedContentControl;
            if (contentControl != null) {
                if (ItemContainerStyle != null && contentControl.Style == null) {
                    contentControl.SetValue(StyleProperty, ItemContainerStyle);
                }
            }
            
            base.PrepareContainerForItemOverride(element, item);
        }
    }
}