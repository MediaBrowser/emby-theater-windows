using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace MediaBrowser.Theater.Presentation.Controls
{
    public class ExtendedItemsControl
        : ItemsControl
    {
        protected override DependencyObject GetContainerForItemOverride()
        {
            return new ExtendedContentControl();
        }
    }
}