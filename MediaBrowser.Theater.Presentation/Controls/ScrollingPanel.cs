using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace MediaBrowser.Theater.Presentation.Controls
{
    /// <summary>
    /// This started from:
    /// http://www.switchonthecode.com/tutorials/wpf-tutorial-implementing-iscrollinfo
    /// Then, after implementing this, content was being displayed in stack panel like manner.
    /// I then reviewed the source code of ScrollContentPresenter and updated MeasureOverride and ArrangeOverride to match.
    /// </summary>
    public class ScrollingPanel : ContentControl, IScrollInfo
    {
        /// <summary>
        /// The line size
        /// </summary>
        private const double LineSize = 16;
        /// <summary>
        /// The wheel size
        /// </summary>
        private const double WheelSize = 3 * LineSize;

        private Vector _computedOffset;
        /// <summary>
        /// The _ offset
        /// </summary>
        private Vector _offset;
        /// <summary>
        /// The _ extent
        /// </summary>
        private Size _extent;
        /// <summary>
        /// The _ viewport
        /// </summary>
        private Size _viewport;

        /// <summary>
        /// The _ animation length
        /// </summary>
        private readonly TimeSpan _animationLength = TimeSpan.FromMilliseconds(500);

        public ScrollingPanel()
        {
            Focusable = false;

            DataContextChanged += ScrollingPanel_DataContextChanged;
        }

        void ScrollingPanel_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (CanHorizontallyScroll)
            {
                SetHorizontalOffset(0);
            }

            if (CanVerticallyScroll)
            {
                SetVerticalOffset(0);
            }
        }

        /// <summary>
        /// When overridden in a derived class, measures the size in layout required for child elements and determines a size for the <see cref="T:System.Windows.FrameworkElement" />-derived class.
        /// </summary>
        /// <param name="availableSize">The available size that this element can give to child elements. Infinity can be specified as a value to indicate that the element will size to whatever content is available.</param>
        /// <returns>The size that this element determines it needs during layout, based on its calculations of child element sizes.</returns>
        protected override Size MeasureOverride(Size availableSize)
        {
            if (ContentElement == null)
            {
                return availableSize;
            }

            var constraint2 = availableSize;
            if (CanHorizontallyScroll)
            {
                constraint2.Width = double.PositiveInfinity;
            }
            if (CanVerticallyScroll)
            {
                constraint2.Height = double.PositiveInfinity;
            }

            //var uiElement = Children[0];

            //uiElement.Measure(constraint2);
            var size = base.MeasureOverride(constraint2);

            VerifyScrollData(availableSize, size);

            size.Width = Math.Min(availableSize.Width, size.Width);
            size.Height = Math.Min(availableSize.Height, size.Height);

            return size;
        }

        /// <summary>
        /// Arranges the content of a <see cref="T:System.Windows.Controls.Grid" /> element.
        /// </summary>
        /// <param name="arrangeSize">Specifies the size this <see cref="T:System.Windows.Controls.Grid" /> element should use to arrange its child elements.</param>
        /// <returns><see cref="T:System.Windows.Size" /> that represents the arranged size of this Grid element and its children.</returns>
        protected override Size ArrangeOverride(Size arrangeSize)
        {
            VerifyScrollData(arrangeSize, _extent);

            var uiElement = ContentElement;

            if (uiElement == null)
            {
                return arrangeSize;
            }

            TranslateTransform trans = null;

            var finalRect = new Rect(uiElement.DesiredSize);

            // ScrollContentPresenter sets these to 0 - current offset
            // We need to set it to zero in order to make the animation work
            finalRect.X = 0;
            finalRect.Y = 0;

            finalRect.Width = Math.Max(finalRect.Width, arrangeSize.Width);
            finalRect.Height = Math.Max(finalRect.Height, arrangeSize.Height);

            trans = uiElement.RenderTransform as TranslateTransform;

            if (trans == null)
            {
                uiElement.RenderTransformOrigin = new Point(0, 0);
                trans = new TranslateTransform();
                uiElement.RenderTransform = trans;
            }

            uiElement.Arrange(finalRect);

            trans.BeginAnimation(TranslateTransform.XProperty,
              GetAnimation(0 - HorizontalOffset),
              HandoffBehavior.Compose);
            trans.BeginAnimation(TranslateTransform.YProperty,
              GetAnimation(0 - VerticalOffset),
              HandoffBehavior.Compose);

            return arrangeSize;
        }

        private UIElement ContentElement
        {
            get
            {
                return Content as UIElement;
                //if (Children == null || Children.Count == 0)
                //{
                //    return null;
                //}

                //return Children[0];
            }
        }

        /// <summary>
        /// Gets the animation.
        /// </summary>
        /// <param name="toValue">To value.</param>
        /// <returns>DoubleAnimation.</returns>
        private DoubleAnimation GetAnimation(double toValue)
        {
            var animation = new DoubleAnimation(toValue, _animationLength);

            animation.AccelerationRatio = 0;
            animation.DecelerationRatio = 1;
            animation.AutoReverse = false;

            return animation;
        }

        #region Movement Methods
        /// <summary>
        /// Scrolls down within content by one logical unit.
        /// </summary>
        public void LineDown()
        { SetVerticalOffset(VerticalOffset + LineSize); }

        /// <summary>
        /// Scrolls up within content by one logical unit.
        /// </summary>
        public void LineUp()
        { SetVerticalOffset(VerticalOffset - LineSize); }

        /// <summary>
        /// Scrolls left within content by one logical unit.
        /// </summary>
        public void LineLeft()
        { SetHorizontalOffset(HorizontalOffset - LineSize); }

        /// <summary>
        /// Scrolls right within content by one logical unit.
        /// </summary>
        public void LineRight()
        { SetHorizontalOffset(HorizontalOffset + LineSize); }

        /// <summary>
        /// Scrolls down within content after a user clicks the wheel button on a mouse.
        /// </summary>
        public void MouseWheelDown()
        { SetVerticalOffset(VerticalOffset + WheelSize); }

        /// <summary>
        /// Scrolls up within content after a user clicks the wheel button on a mouse.
        /// </summary>
        public void MouseWheelUp()
        { SetVerticalOffset(VerticalOffset - WheelSize); }

        /// <summary>
        /// Scrolls left within content after a user clicks the wheel button on a mouse.
        /// </summary>
        public void MouseWheelLeft()
        { SetHorizontalOffset(HorizontalOffset - WheelSize); }

        /// <summary>
        /// Scrolls right within content after a user clicks the wheel button on a mouse.
        /// </summary>
        public void MouseWheelRight()
        { SetHorizontalOffset(HorizontalOffset + WheelSize); }

        /// <summary>
        /// Scrolls down within content by one page.
        /// </summary>
        public void PageDown()
        { SetVerticalOffset(VerticalOffset + ViewportHeight); }

        /// <summary>
        /// Scrolls up within content by one page.
        /// </summary>
        public void PageUp()
        { SetVerticalOffset(VerticalOffset - ViewportHeight); }

        /// <summary>
        /// Scrolls left within content by one page.
        /// </summary>
        public void PageLeft()
        { SetHorizontalOffset(HorizontalOffset - ViewportWidth); }

        /// <summary>
        /// Scrolls right within content by one page.
        /// </summary>
        public void PageRight()
        { SetHorizontalOffset(HorizontalOffset + ViewportWidth); }
        #endregion

        public static readonly DependencyProperty CanHorizontallyScrollProperty = DependencyProperty.Register("CanHorizontallyScroll", typeof(bool), typeof(ScrollingPanel), new PropertyMetadata(false, new PropertyChangedCallback(ScrollingPanel.OnCanScrollPropertyChanged)));
        public static readonly DependencyProperty CanVerticallyScrollProperty = DependencyProperty.Register("CanVerticallyScroll", typeof(bool), typeof(ScrollingPanel), new PropertyMetadata(false, new PropertyChangedCallback(ScrollingPanel.OnCanScrollPropertyChanged)));

        /// <summary>
        /// Gets or sets a <see cref="T:System.Windows.Controls.ScrollViewer" /> element that controls scrolling behavior.
        /// </summary>
        /// <value>The scroll owner.</value>
        /// <returns>A <see cref="T:System.Windows.Controls.ScrollViewer" /> element that controls scrolling behavior. This property has no default value.</returns>
        public ScrollViewer ScrollOwner { get; set; }

        /// <summary>
        /// Gets or sets a value that indicates whether scrolling on the horizontal axis is possible.
        /// </summary>
        /// <value><c>true</c> if this instance can horizontally scroll; otherwise, <c>false</c>.</value>
        /// <returns>true if scrolling is possible; otherwise, false. This property has no default value.</returns>
        public bool CanHorizontallyScroll
        {
            get
            {
                return (bool)base.GetValue(ScrollingPanel.CanHorizontallyScrollProperty);
            }
            set
            {
                base.SetValue(ScrollingPanel.CanHorizontallyScrollProperty, value);
            }
        }

        /// <summary>
        /// Gets or sets a value that indicates whether scrolling on the vertical axis is possible.
        /// </summary>
        /// <value><c>true</c> if this instance can vertically scroll; otherwise, <c>false</c>.</value>
        /// <returns>true if scrolling is possible; otherwise, false. This property has no default value.</returns>
        public bool CanVerticallyScroll
        {
            get
            {
                return (bool)base.GetValue(ScrollingPanel.CanVerticallyScrollProperty);
            }
            set
            {
                base.SetValue(ScrollingPanel.CanVerticallyScrollProperty, value);
            }
        }

        private static void OnCanScrollPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            UIElement panel = d as UIElement;
            if (panel != null)
            {
                panel.InvalidateMeasure();
            }
        }

        /// <summary>
        /// Gets the vertical size of the extent.
        /// </summary>
        /// <value>The height of the extent.</value>
        /// <returns>A <see cref="T:System.Double" /> that represents, in device independent pixels, the vertical size of the extent.This property has no default value.</returns>
        public double ExtentHeight
        { get { return _extent.Height; } }

        /// <summary>
        /// Gets the horizontal size of the extent.
        /// </summary>
        /// <value>The width of the extent.</value>
        /// <returns>A <see cref="T:System.Double" /> that represents, in device independent pixels, the horizontal size of the extent. This property has no default value.</returns>
        public double ExtentWidth
        { get { return _extent.Width; } }

        /// <summary>
        /// Gets the horizontal offset of the scrolled content.
        /// </summary>
        /// <value>The horizontal offset.</value>
        /// <returns>A <see cref="T:System.Double" /> that represents, in device independent pixels, the horizontal offset. This property has no default value.</returns>
        public double HorizontalOffset
        { get { return _computedOffset.X; } }

        /// <summary>
        /// Gets the vertical offset of the scrolled content.
        /// </summary>
        /// <value>The vertical offset.</value>
        /// <returns>A <see cref="T:System.Double" /> that represents, in device independent pixels, the vertical offset of the scrolled content. Valid values are between zero and the <see cref="P:System.Windows.Controls.Primitives.IScrollInfo.ExtentHeight" /> minus the <see cref="P:System.Windows.Controls.Primitives.IScrollInfo.ViewportHeight" />. This property has no default value.</returns>
        public double VerticalOffset
        { get { return _computedOffset.Y; } }

        /// <summary>
        /// Gets the vertical size of the viewport for this content.
        /// </summary>
        /// <value>The height of the viewport.</value>
        /// <returns>A <see cref="T:System.Double" /> that represents, in device independent pixels, the vertical size of the viewport for this content. This property has no default value.</returns>
        public double ViewportHeight
        { get { return _viewport.Height; } }

        /// <summary>
        /// Gets the horizontal size of the viewport for this content.
        /// </summary>
        /// <value>The width of the viewport.</value>
        /// <returns>A <see cref="T:System.Double" /> that represents, in device independent pixels, the horizontal size of the viewport for this content. This property has no default value.</returns>
        public double ViewportWidth
        { get { return _viewport.Width; } }

        /// <summary>
        /// Forces content to scroll until the coordinate space of a <see cref="T:System.Windows.Media.Visual" /> object is visible.
        /// </summary>
        /// <param name="visual">A <see cref="T:System.Windows.Media.Visual" /> that becomes visible.</param>
        /// <param name="rectangle">A bounding rectangle that identifies the coordinate space to make visible.</param>
        /// <returns>A <see cref="T:System.Windows.Rect" /> that is visible.</returns>
        public Rect MakeVisible(Visual visual, Rect rectangle)
        {
            if (rectangle.IsEmpty || visual == null
              || visual.Equals(this) || !IsAncestorOf(visual))
            { return Rect.Empty; }

            rectangle = visual.TransformToAncestor(this).TransformBounds(rectangle);

            if (rectangle.IsEmpty)
            {
                return rectangle;
            }

            var scaleX = CanHorizontallyScroll ? (rectangle.Width * .6) : 0;
            var scaleY = CanVerticallyScroll ? (rectangle.Height * .6) : 0;
            rectangle.Inflate(scaleX, scaleY);

            var rect = new Rect(HorizontalOffset, VerticalOffset, ViewportWidth, ViewportHeight);

            rectangle.X += rect.X;
            rectangle.Y += rect.Y;

            rect.X = ComputeScrollOffsetWithMinimalScroll(rect.Left, rect.Right, rectangle.Left, rectangle.Right);
            rect.Y = ComputeScrollOffsetWithMinimalScroll(rect.Top, rect.Bottom, rectangle.Top, rectangle.Bottom);

            SetHorizontalOffset(rect.X);
            SetVerticalOffset(rect.Y);

            rectangle.Intersect(rect);
            rectangle.X -= rect.X;
            rectangle.Y -= rect.Y;

            return rectangle;
        }

        private static double ComputeScrollOffsetWithMinimalScroll(double topView, double bottomView, double topChild, double bottomChild)
        {
            bool flag = false;
            bool flag2 = false;
            return ComputeScrollOffsetWithMinimalScroll(topView, bottomView, topChild, bottomChild, ref flag, ref flag2);
        }
        private static double ComputeScrollOffsetWithMinimalScroll(double topView, double bottomView, double topChild, double bottomChild, ref bool alignTop, ref bool alignBottom)
        {
            bool flag = DoubleUtil.LessThan(topChild, topView) && DoubleUtil.LessThan(bottomChild, bottomView);
            bool flag2 = DoubleUtil.GreaterThan(bottomChild, bottomView) && DoubleUtil.GreaterThan(topChild, topView);
            bool flag3 = bottomChild - topChild > bottomView - topView;
            if ((flag && !flag3) || (flag2 && flag3) || alignTop)
            {
                alignTop = true;
                return topChild;
            }
            if (flag || flag2 || alignBottom)
            {
                alignBottom = true;
                return bottomChild - (bottomView - topView);
            }
            return topView;
        }

        /// <summary>
        /// Verifies the scroll data.
        /// </summary>
        /// <param name="viewport">The viewport.</param>
        /// <param name="extent">The extent.</param>
        protected void VerifyScrollData(Size viewport, Size extent)
        {
            bool flag = true;
            if (double.IsInfinity(viewport.Width))
            {
                viewport.Width = extent.Width;
            }
            if (double.IsInfinity(viewport.Height))
            {
                viewport.Height = extent.Height;
            }

            flag &= DoubleUtil.AreClose(viewport, _viewport);
            flag &= DoubleUtil.AreClose(extent, _extent);
            _viewport = viewport;
            _extent = extent;
            if (!(flag & this.CoerceOffsets()))
            {
                if (ScrollOwner != null)
                {
                    ScrollOwner.InvalidateScrollInfo();
                }
            }
        }

        private bool CoerceOffsets()
        {
            var vector = new Vector(CoerceOffset(this._offset.X, this._extent.Width, this._viewport.Width), CoerceOffset(this._offset.Y, this._extent.Height, this._viewport.Height));
            bool result = DoubleUtil.AreClose(this._computedOffset, vector);
            this._computedOffset = vector;
            return result;
        }
        private static double CoerceOffset(double offset, double extent, double viewport)
        {
            if (offset > extent - viewport)
            {
                offset = extent - viewport;
            }
            if (offset < 0.0)
            {
                offset = 0.0;
            }
            return offset;
        }

        /// <summary>
        /// Sets the amount of horizontal offset.
        /// </summary>
        /// <param name="offset">The degree to which content is horizontally offset from the containing viewport.</param>
        public void SetHorizontalOffset(double offset)
        {
            offset = Math.Max(0, offset);
            //offset = Math.Max(0, Math.Min(offset, ExtentWidth - ViewportWidth));

            if (!DoubleUtil.AreClose(_offset.X, offset))
            {
                _offset.X = offset;

                InvalidateArrange();
            }
        }

        /// <summary>
        /// Sets the amount of vertical offset.
        /// </summary>
        /// <param name="offset">The degree to which content is vertically offset from the containing viewport.</param>
        public void SetVerticalOffset(double offset)
        {
            offset = Math.Max(0, offset);
            //offset = Math.Max(0, Math.Min(offset, ExtentHeight - ViewportHeight));

            if (!DoubleUtil.AreClose(_offset.Y, offset))
            {
                _offset.Y = offset;
                InvalidateArrange();
            }
        }
    }
}
