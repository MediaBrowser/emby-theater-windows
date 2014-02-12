using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace MediaBrowser.Theater.Presentation.Controls
{
    public class AnimatedStackPanel : Panel
    {
        public Orientation Orientation
        {
            get { return (Orientation)GetValue(OrientationProperty); }
            set { SetValue(OrientationProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Orientation.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty OrientationProperty =
            DependencyProperty.Register("Orientation", typeof(Orientation), typeof(AnimatedStackPanel), new PropertyMetadata(Orientation.Horizontal, OnOrientationChanged));

        private static void OnOrientationChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var p = (Panel)d;
            p.InvalidateVisual();
        }

        protected override bool HasLogicalOrientation
        {
            get { return true; }
        }

        protected override Orientation LogicalOrientation
        {
            get { return Orientation; }
        }

        protected override Size MeasureOverride(Size availableSize)
        {
            if (Orientation == Orientation.Horizontal) {
                double x = 0;
                double height = 0;

                foreach (UIElement child in Children) {
                    child.Measure(new Size(Math.Max(availableSize.Width - x, 0), availableSize.Height));
                    x += child.DesiredSize.Width;
                    height = Math.Max(height, child.DesiredSize.Height);
                }

                return new Size(x, height);
            } else {
                double y = 0;
                double width = 0;

                foreach (UIElement child in Children)
                {
                    child.Measure(new Size(availableSize.Width, Math.Max(availableSize.Height - y, 0)));
                    y += child.DesiredSize.Height;
                    width = Math.Max(width, child.DesiredSize.Height);
                }

                return new Size(width, y);
            }
        }

        protected override System.Windows.Size ArrangeOverride(System.Windows.Size finalSize)
        {
            return base.ArrangeOverride(finalSize);
        }
    }
}
