using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace MediaBrowser.Theater.Presentation.Controls
{
    /// <summary>
    /// Extends Panel by automatically sizing the children by evaluating MaxVisibleRowCount and MaxVisibleColumnCounts.
    /// Implements IScrollInfo.
    /// DependencyProperties:
    /// - Orientation Orientation
    /// - int MaxVisibleColumnCount
    /// - int MaxVisibleRowCount
    /// - double ScrollDuration
    /// - double DecelerationRatio
    /// - double AccelerationRatio
    /// </summary>
    public class UniformPanel : Panel, IScrollInfo
    {
        /// <summary>
        /// Initializes a new instance of UniformPanel.
        /// </summary>
        public UniformPanel()
            : base()
        {
            // For use in the IScrollInfo implementation
            this.RenderTransform = _trans;
            //ClipToBoundsMode = ClipToBoundsModes.Default;
        }



        //public enum ClipToBoundsModes
        //{
        //    /// <summary>
        //    /// Doesn't clip. Ignores ClipToBounds.
        //    /// </summary>
        //    Never,
        //    /// <summary>
        //    /// Uses ClipToBounds.
        //    /// </summary>
        //    Default,
        //    /// <summary>
        //    /// Clips always. Ignores ClipToBounds.
        //    /// </summary>
        //    Always,
        //    /// <summary>
        //    /// Clipping is done at the edges which don't have contact with a selected item. Ignores ClipToBounds.
        //    /// </summary>
        //    Smart
        //}

        //#region ClipToBoundsMode
        //[DefaultValue(ClipToBoundsModes.Default)]
        //public ClipToBoundsModes ClipToBoundsMode
        //{
        //    get { return (ClipToBoundsModes)GetValue(ClipToBoundsModeProperty); }
        //    set { SetValue(ClipToBoundsModeProperty, value); }
        //}

        //// Using a DependencyProperty as the backing store for ClipToBoundsMode.  This enables animation, styling, binding, etc...
        //public static readonly DependencyProperty ClipToBoundsModeProperty =
        //    DependencyProperty.Register("ClipToBoundsMode", typeof(ClipToBoundsModes), typeof(UniformPanel), new UIPropertyMetadata(ClipToBoundsModes.Default));
        //#endregion

        #region Orientation Orientation
        /// <summary>
        /// Gets or sets a value that indicates the dimension by which child elements are stacked.  
        /// </summary>
        [DefaultValue(Orientation.Vertical)]
        public Orientation Orientation
        {
            get { return (Orientation)GetValue(OrientationProperty); }
            set { SetValue(OrientationProperty, value); }
        }
        /// <summary>
        /// Identifies the Orientation dependency property.
        /// </summary>
        public static readonly DependencyProperty OrientationProperty =
            DependencyProperty.Register("Orientation", typeof(Orientation), typeof(UniformPanel),
                                        new FrameworkPropertyMetadata(Orientation.Vertical, FrameworkPropertyMetadataOptions.AffectsMeasure, OnOrientationPropertyChanged));

        private static void OnOrientationPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            UniformPanel b = d as UniformPanel;
            b.OnOrientationPropertyChanged(e);
        }

        private void OnOrientationPropertyChanged(DependencyPropertyChangedEventArgs e)
        {

            // TODO: set rows / columns
        }

        #endregion

        #region int MaxVisibleRowCount
        /// <summary>
        /// The count of rows used to arrange the child controls.
        /// </summary>
        [DefaultValue(5)]
        public int MaxVisibleRowCount
        {
            get { return (int)GetValue(VisibleRowProperty); }
            set { SetValue(VisibleRowProperty, value); }
        }
        /// <summary>
        /// Identifies the MaxVisibleRowCount dependency property.
        /// </summary>
        public static readonly DependencyProperty VisibleRowProperty =
            DependencyProperty.Register("MaxVisibleRowCount", typeof(int), typeof(UniformPanel),
                                        new FrameworkPropertyMetadata(5, OnVisibleRowPropertyChanged));

        private static void OnVisibleRowPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            UniformPanel b = d as UniformPanel;
            b.OnVisibleRowPropertyChanged((int)e.OldValue, (int)e.NewValue);
        }

        private void OnVisibleRowPropertyChanged(int OldValue, int NewValue)
        {
            // TODO: set rows / columns or refresh
        }

        #endregion

        #region int MaxVisibleColumnCount
        /// <summary>
        /// The count of columns used to arrange the child controls.
        /// </summary>
        [DefaultValue(1)]
        public int MaxVisibleColumnCount
        {
            get { return (int)GetValue(VisibleColumnProperty); }
            set { SetValue(VisibleColumnProperty, value); }
        }
        /// <summary>
        /// Identifies the MaxVisibleColumnCount dependency property.
        /// </summary>
        public static readonly DependencyProperty VisibleColumnProperty =
            DependencyProperty.Register("MaxVisibleColumnCount", typeof(int), typeof(UniformPanel),
                                        new FrameworkPropertyMetadata(1, OnVisibleColumnPropertyChanged));

        private static void OnVisibleColumnPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            UniformPanel b = d as UniformPanel;
            b.OnVisibleColumnPropertyChanged((int)e.OldValue, (int)e.NewValue);
        }

        private void OnVisibleColumnPropertyChanged(int OldValue, int NewValue)
        {
            // TODO: set rows / columns or refresh
        }

        #endregion

        private Size ChildSize
        {
            get
            {
                Size s = new Size();

                s.Height = _viewport.Height / MaxVisibleRowCount;
                s.Width = _viewport.Width / MaxVisibleColumnCount;

                if (Double.IsNaN(s.Height)) s.Height = 1000;
                if (Double.IsNaN(s.Width)) s.Width = 1000;
                return s;
            }
        }

        /// <summary>
        /// Returns a geometry for a clipping mask. The mask applies if the layout system 
        /// attempts to arrange an element that is larger than the available display space.
        /// If ClipToBounds is False, then the returned value is null, which leads to unclipped elements.
        /// </summary>
        /// <param name="layoutSlotSize">The size of the part of the element that does visual presentation.</param>
        /// <returns>The clipping geometry.</returns>
        protected override Geometry GetLayoutClip(Size layoutSlotSize)
        {
            return ClipToBounds ? base.GetLayoutClip(layoutSlotSize) : null;
        }

        ///// <summary>
        ///// Returns a geometry for a clipping mask. The mask applies if the layout system 
        ///// attempts to arrange an element that is larger than the available display space.
        ///// If ClipToBounds is False, then the returned value is null, which leads to unclipped elements.
        ///// </summary>
        ///// <param name="layoutSlotSize">The size of the part of the element that does visual presentation.</param>
        ///// <returns>The clipping geometry.</returns>
        //protected override Geometry GetLayoutClip(Size layoutSlotSize)
        //{
        //    switch (ClipToBoundsMode)
        //    {
        //        case ClipToBoundsModes.Never:
        //            return null;
        //        case ClipToBoundsModes.Default:
        //            return base.GetLayoutClip(layoutSlotSize);
        //        case ClipToBoundsModes.Always:
        //            return new RectangleGeometry(new Rect(new Point(0, 0), layoutSlotSize));
        //        case ClipToBoundsModes.Smart:
        //            double BoundTop = 0d;
        //            double BoundLeft = 0d;
        //            double BoundWidth = layoutSlotSize.Width;
        //            double BoundHeight = layoutSlotSize.Height;
        //            // find selected InternalChildren
        //            if (this.IsItemsHost)
        //            {
        //                Selector itemsControl = ItemsControl.GetItemsOwner(this) as Selector;
        //                if (itemsControl != null)
        //                {
        //                    int i = itemsControl.SelectedIndex;

        //                    if (i != -1)
        //                    {
        //                        int ChildrenPerRow = CalculateChildrenPerRow(layoutSlotSize);
        //                        int ChildrenPerColumn = CalculateChildrenPerColumn(layoutSlotSize);

        //                        int actualRow ;
        //                        int actualColumn ;


        //                        if (this.Orientation == Orientation.Vertical)
        //                        {
        //                            actualRow = i / MaxVisibleColumnCount;
        //                            actualColumn = i % MaxVisibleColumnCount;

        //                        }
        //                        else
        //                        {
        //                            // 0 3 6
        //                            // 1 4 7
        //                            // 2 5
        //                            actualRow = i % MaxVisibleRowCount;
        //                            actualColumn = i / MaxVisibleRowCount;
        //                        }

        //                        if (actualRow == 0)
        //                            BoundTop = -layoutSlotSize.Height / MaxVisibleRowCount; //double.NegativeInfinity;//double.MinValue;// double.NaN; -> all 3 lead to invisibility

        //                        if (actualRow == ChildrenPerColumn - 1)
        //                            BoundHeight = layoutSlotSize.Height - BoundTop + layoutSlotSize.Height / MaxVisibleRowCount; // double.MaxValue;
        //                        else
        //                            BoundHeight = layoutSlotSize.Height - BoundTop;

        //                        if (actualColumn == 0)
        //                            BoundLeft = -layoutSlotSize.Width / MaxVisibleColumnCount;

        //                        if (actualColumn == ChildrenPerRow - 1)
        //                            BoundWidth = layoutSlotSize.Width - BoundLeft + layoutSlotSize.Width / MaxVisibleColumnCount;
        //                        else
        //                            BoundWidth = layoutSlotSize.Width - BoundLeft;

        //                        // wenn i am rand X, dann rand X is infinite
        //                    }
        //                }
        //            }



        //           return null;

        //            //return new RectangleGeometry(new Rect(-20, -20, 100, 100));

        //           // return new RectangleGeometry(new Rect(BoundLeft, BoundTop, BoundWidth, BoundHeight));

        //            //return ClipToBounds ?  : null;
        //        default:
        //            break;
        //    }
        //    return null;
        //}

        /// <summary>
        /// Measure the children
        /// </summary>
        /// <param name="availableSize">Size available</param>
        /// <returns>Size desired</returns>
        protected override Size MeasureOverride(Size availableSize)
        {
            if (availableSize != _viewport)
            {
                _viewport = availableSize;
                if (_owner != null)
                    _owner.InvalidateScrollInfo();
            }
            UpdateScrollInfo(availableSize);

            return base.MeasureOverride(availableSize);
        }

        /// <summary>
        /// Arrange the children
        /// </summary>
        /// <param name="finalSize">Size available</param>
        /// <returns>Size used</returns>
        protected override Size ArrangeOverride(Size finalSize)
        {
            UpdateScrollInfo(finalSize);

            for (int i = 0; i < InternalChildren.Count; i++)
            {
                ArrangeChild(i, InternalChildren[i], finalSize);
            }

            return finalSize;
        }


        #region Layout specific code
        // I've isolated the layout specific code to this region. If you want to do something other than tiling, this is
        // where you'll make your changes

        /// <summary>
        /// Calculate the extent of the view based on the available size
        /// </summary>
        /// <param name="availableSize">available size</param>
        /// <param name="itemCount">number of data items</param>
        /// <returns></returns>
        private Size CalculateExtent(Size availableSize, int itemCount)
        {
            int childrenPerRow = CalculateChildrenPerRow(availableSize);
            int childrenPerColumn = CalculateChildrenPerColumn(availableSize);

            // See how big we are
            return new Size(childrenPerRow * this.ChildSize.Width,
                childrenPerColumn * this.ChildSize.Height); //    , this.ChildSize.Height * Math.Ceiling((double)itemCount / childrenPerRow));                       
        }

        /// <summary>
        /// Position a child
        /// </summary>
        /// <param name="itemIndex">The data item index of the child</param>
        /// <param name="child">The element to position</param>
        /// <param name="finalSize">The size of the panel</param>
        private void ArrangeChild(int itemIndex, UIElement child, Size finalSize)
        {
            int row;
            int column;

            if (Orientation == Orientation.Vertical)
            {
                row = itemIndex / MaxVisibleColumnCount;
                column = itemIndex % MaxVisibleColumnCount;
            }
            else
            {
                row = itemIndex % MaxVisibleRowCount;
                column = itemIndex / MaxVisibleRowCount;
            }

            child.Arrange(new Rect(column * this.ChildSize.Width, row * this.ChildSize.Height, this.ChildSize.Width, this.ChildSize.Height));
        }


        /// <summary>
        /// Helper function for tiling layout
        /// </summary>
        /// <param name="availableSize">Size available</param>
        /// <returns></returns>
        private int CalculateChildrenPerRow(Size availableSize)
        {
            int childrenPerRow;

            ItemsControl itemsControl = ItemsControl.GetItemsOwner(this);
            int itemCount = itemsControl.HasItems ? itemsControl.Items.Count : 1;

            if (this.Orientation == Orientation.Vertical)
                childrenPerRow = MaxVisibleColumnCount;
            else
                childrenPerRow = (int)Math.Ceiling((double)itemCount / (double)MaxVisibleRowCount);

            return childrenPerRow;

            // Figure out how many children fit on each row
            //int childrenPerRow;
            //if (availableSize.Width == Double.PositiveInfinity)
            //    childrenPerRow = this.Children.Count;
            //else
            //    childrenPerRow = Math.Max(1, (int)Math.Floor(availableSize.Width / this.ChildSize.Width));
            //return childrenPerRow;
        }

        /// <summary>
        /// Helper function for tiling layout
        /// </summary>
        /// <param name="availableSize">Size available</param>
        /// <returns></returns>
        private int CalculateChildrenPerColumn(Size availableSize)
        {
            int childrenPerColumn;

            ItemsControl itemsControl = ItemsControl.GetItemsOwner(this);
            int itemCount = itemsControl.HasItems ? itemsControl.Items.Count : 1;

            if (this.Orientation == Orientation.Horizontal)
                childrenPerColumn = MaxVisibleRowCount;
            else
                childrenPerColumn = (int)Math.Ceiling((double)itemCount / (double)MaxVisibleColumnCount);

            return childrenPerColumn;
        }

        #endregion


        #region double ScrollDuration
        /// <summary>
        /// Defines how many seconds (!) scrolling from one item to another item needs
        /// </summary>
        public double ScrollDuration
        {
            get { return (double)GetValue(ScrollDurationProperty); }
            set { SetValue(ScrollDurationProperty, value); }
        }

        /// <summary>
        /// Identifies the ScrollDuration dependency property.
        /// Using a DependencyProperty as the backing store enables animation, styling, binding, etc...
        /// </summary>

        public static readonly DependencyProperty ScrollDurationProperty =
            DependencyProperty.Register("ScrollDuration", typeof(double), typeof(UniformPanel), new UIPropertyMetadata(1d));

        #endregion

        #region double DecelerationRatio

        /// <summary>
        /// Gets or sets a value specifying the percentage of the animation's Duration spent decelerating the passage of time from its maximum rate to zero. 
        /// </summary>
        public double DecelerationRatio
        {
            get { return (double)GetValue(DecelerationRatioProperty); }
            set { SetValue(DecelerationRatioProperty, value); }
        }

        /// <summary>
        /// Identifies the DecelerationRatio dependency property.
        /// Using a DependencyProperty as the backing store enables animation, styling, binding, etc...
        /// </summary>

        public static readonly DependencyProperty DecelerationRatioProperty =
            DependencyProperty.Register("DecelerationRatio", typeof(double), typeof(UniformPanel), new UIPropertyMetadata(1d));

        #endregion

        #region double AccelerationRatio

        /// <summary>
        /// Gets or sets a value specifying the percentage of the animation's Duration spent accelerating the passage of time from zero to its maximum rate. 
        /// </summary>
        public double AccelerationRatio
        {
            get { return (double)GetValue(AccelerationRatioProperty); }
            set { SetValue(AccelerationRatioProperty, value); }
        }

        /// <summary>
        /// Identifies the AccelerationRatio dependency property.
        /// Using a DependencyProperty as the backing store enables animation, styling, binding, etc...
        /// </summary>
        public static readonly DependencyProperty AccelerationRatioProperty =
            DependencyProperty.Register("AccelerationRatio", typeof(double), typeof(UniformPanel), new UIPropertyMetadata(0d));

        #endregion


        #region IScrollInfo implementation
        // See Ben Constable's series of posts at http://blogs.msdn.com/bencon/

        private void UpdateScrollInfo(Size availableSize)
        {
            // See how many items there are
            //ItemsControl itemsControl = ItemsControl.GetItemsOwner(this);
            //int itemCount = itemsControl.HasItems ? itemsControl.Items.Count : 0;
            //Size extent = CalculateExtent(availableSize, itemCount);

            Size extent = CalculateExtent(availableSize, InternalChildren.Count);

            // Update extent
            if (extent != _extent)
            {
                _extent = extent;
                if (_owner != null)
                    _owner.InvalidateScrollInfo();
            }

            // Update viewport
            if (availableSize != _viewport)
            {
                _viewport = availableSize;
                if (_owner != null)
                    _owner.InvalidateScrollInfo();
            }
        }
        /// <summary>
        /// Gets or sets a ScrollViewer element that controls scrolling behavior.
        /// </summary>
        public ScrollViewer ScrollOwner
        {
            get { return _owner; }
            set { _owner = value; }
        }
        /// <summary>
        /// Gets or sets a value that indicates whether scrolling on the horizontal axis is possible.
        /// </summary>
        public bool CanHorizontallyScroll
        {
            get { return _canHScroll; }
            set { _canHScroll = value; }
        }
        /// <summary>
        /// Gets or sets a value that indicates whether scrolling on the vertical axis is possible. 
        /// </summary>
        public bool CanVerticallyScroll
        {
            get { return _canVScroll; }
            set { _canVScroll = value; }
        }
        /// <summary>
        /// Gets the horizontal offset of the scrolled content.
        /// </summary>
        public double HorizontalOffset
        {
            get { return _offset.X; }
        }
        /// <summary>
        /// Identifies the HorizontalOffset dependency property.
        /// </summary>
        public static readonly DependencyProperty HorizontalOffsetProperty =
            DependencyProperty.Register("HorizontalOffset", typeof(double), typeof(UniformPanel),
                               new FrameworkPropertyMetadata(0d, OnHorizontalOffsetPropertyChanged));

        private static void OnHorizontalOffsetPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            UniformPanel b = d as UniformPanel;
            b.OnHorizontalOffsetPropertyChanged(e);
        }

        private void OnHorizontalOffsetPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            double offset = (double)e.NewValue;

            if (offset < 0 || _viewport.Width >= _extent.Width)
            {
                offset = 0;
            }
            else
            {
                if (offset + _viewport.Width >= _extent.Width)
                {
                    offset = _extent.Width - _viewport.Width;
                }
            }

            _offset.X = offset;

            if (_owner != null)
                _owner.InvalidateScrollInfo();

            _trans.X = -offset;

            // Force us to realize the correct children
            InvalidateMeasure();
        }
        /// <summary>
        /// Gets the vertical offset of the scrolled content.
        /// </summary>
        public double VerticalOffset
        {
            get { return _offset.Y; }
        }
        /// <summary>
        /// Identifies the VerticalOffset dependnecy property.
        /// </summary>
        public static readonly DependencyProperty VerticalOffsetProperty =
            DependencyProperty.Register("VerticalOffset", typeof(double), typeof(UniformPanel),
                                       new FrameworkPropertyMetadata(0d, OnVerticalOffsetPropertyChanged));

        private static void OnVerticalOffsetPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            UniformPanel b = d as UniformPanel;
            b.OnVerticalOffsetPropertyChanged(e);
        }

        private void OnVerticalOffsetPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            double offset = (double)e.NewValue;

            if (offset < 0 || _viewport.Height >= _extent.Height)
            {
                offset = 0;
            }
            else
            {
                if (offset + _viewport.Height >= _extent.Height)
                {
                    offset = _extent.Height - _viewport.Height;
                }
            }

            _offset.Y = offset;

            if (_owner != null)
                _owner.InvalidateScrollInfo();

            _trans.Y = -offset;

            // Force us to realize the correct children
            InvalidateMeasure();
        }

        /// <summary>
        /// Gets the vertical size of the extent.
        /// </summary>
        public double ExtentHeight
        {
            get { return _extent.Height; }
        }

        /// <summary>
        /// Gets the horizontal size of the extent.
        /// </summary>
        public double ExtentWidth
        {
            get { return _extent.Width; }
        }

        /// <summary>
        /// Gets the vertical size of the viewport for this content.
        /// </summary>
        public double ViewportHeight
        {
            get { return _viewport.Height; }
        }

        /// <summary>
        /// Gets the horizontal size of the viewport for this content.
        /// </summary>
        public double ViewportWidth
        {
            get { return _viewport.Width; }
        }

        /// <summary>
        /// Scrolls up within content by one logical unit. 
        /// </summary>
        public void LineUp()
        {
            SetVerticalOffset(this.VerticalOffset - ChildSize.Height);
        }

        /// <summary>
        /// Scrolls down within content by one logical unit. 
        /// </summary>
        public void LineDown()
        {
            SetVerticalOffset(this.VerticalOffset + ChildSize.Height);
        }

        /// <summary>
        /// Scrolls up within content by one page. 
        /// </summary>
        public void PageUp()
        {
            //if (CanVerticallyScroll)
            SetVerticalOffset(this.VerticalOffset - _viewport.Height);
        }

        /// <summary>
        /// Scrolls down within content by one page. 
        /// </summary>
        public void PageDown()
        {
            //if (CanVerticallyScroll)
            SetVerticalOffset(this.VerticalOffset + _viewport.Height);
        }

        /// <summary>
        /// Scrolls up within content after a user clicks the wheel button on a mouse. 
        /// </summary>
        public void MouseWheelUp()
        {
            LineUp();
        }

        /// <summary>
        /// Scrolls down within content after a user clicks the wheel button on a mouse. 
        /// </summary>
        public void MouseWheelDown()
        {
            LineDown();
        }

        /// <summary>
        /// Scrolls left within content by one logical unit. 
        /// </summary>
        public void LineLeft()
        {
            //if( CanHorizontallyScroll)
            SetHorizontalOffset(this.HorizontalOffset - ChildSize.Width);
        }

        /// <summary>
        /// Scrolls right within content by one logical unit. 
        /// </summary>
        public void LineRight()
        {
            //if (CanHorizontallyScroll)
            SetHorizontalOffset(this.HorizontalOffset + ChildSize.Width);
        }

        /// <summary>
        /// Forces content to scroll until the coordinate space of a Visual object is visible. 
        /// </summary>
        /// <param name="visual"></param>
        /// <param name="rectangle"></param>
        /// <returns></returns>
        public Rect MakeVisible(Visual visual, Rect rectangle)
        {

            //IItemContainerGenerator igenerator = this.ItemContainerGenerator;
            //ItemContainerGenerator generator = igenerator.GetItemContainerGeneratorForPanel(this);
            //int itemIndex = generator.IndexFromContainer(visual);

            int itemIndex = InternalChildren.IndexOf(visual as UIElement);

            int row;
            int column;

            if (Orientation == Orientation.Vertical)
            {
                row = itemIndex / MaxVisibleColumnCount;
                column = itemIndex % MaxVisibleColumnCount;
            }
            else
            {
                row = itemIndex % MaxVisibleRowCount;
                column = itemIndex / MaxVisibleRowCount;
            }

            double hOffset = column * this.ChildSize.Width - (this.ViewportWidth - this.ChildSize.Width) / 2;
            double vOffset = row * this.ChildSize.Height - (this.ViewportHeight - this.ChildSize.Height) / 2;

            this.SetHorizontalOffset(hOffset);
            this.SetVerticalOffset(vOffset);

            // to allow fast keyboard navigation, the neighbours of visual are now created.
            // they'd be created in MeasureOverride anyway, but this might be too late.
            //int firstVisibleItemIndex, lastVisibleItemIndex;
            //GetVisibleRange(out firstVisibleItemIndex, out lastVisibleItemIndex, new Point(hOffset,vOffset));
            //CreateItems(firstVisibleItemIndex, lastVisibleItemIndex);

            return new Rect(hOffset, vOffset, this.ChildSize.Width, this.ChildSize.Height);
        }

        /// <summary>
        /// Scrolls left within content after a user clicks the wheel button on a mouse.
        /// </summary>
        public void MouseWheelLeft()
        {
            LineLeft();
        }

        /// <summary>
        /// Scrolls right within content after a user clicks the wheel button on a mouse. 
        /// </summary>
        public void MouseWheelRight()
        {
            LineRight();
        }

        /// <summary>
        /// Scrolls left within content by one page. 
        /// </summary>
        public void PageLeft()
        {
            SetHorizontalOffset(this.HorizontalOffset - _viewport.Width);
        }

        /// <summary>
        /// Scrolls right within content by one page. 
        /// </summary>
        public void PageRight()
        {
            SetHorizontalOffset(this.HorizontalOffset + _viewport.Width);
        }

        private Storyboard myHorizontalOffsetStoryboard;
        /// <summary>
        /// Sets the amount of horizontal offset. 
        /// </summary>
        /// <param name="offset"></param>
        public void SetHorizontalOffset(double offset)
        {
            DoubleAnimation myDoubleAnimation = new DoubleAnimation();
            myDoubleAnimation.From = _offset.X;
            myDoubleAnimation.To = offset;
            myDoubleAnimation.Duration = new Duration(TimeSpan.FromMilliseconds(ScrollDuration * 1000));
            myDoubleAnimation.AutoReverse = false;
            myDoubleAnimation.AccelerationRatio = AccelerationRatio;
            myDoubleAnimation.DecelerationRatio = DecelerationRatio;

            myHorizontalOffsetStoryboard = new Storyboard();
            myHorizontalOffsetStoryboard.Children.Add(myDoubleAnimation);
            Storyboard.SetTargetProperty(myDoubleAnimation, new PropertyPath(UniformPanel.HorizontalOffsetProperty));

            myHorizontalOffsetStoryboard.Begin(this);
        }

        private Storyboard myVerticalOffsetStoryboard;
        /// <summary>
        /// Sets the amount of vertical offset. 
        /// </summary>
        /// <param name="offset"></param>
        public void SetVerticalOffset(double offset)
        {
            DoubleAnimation myDoubleAnimation = new DoubleAnimation();
            myDoubleAnimation.From = _offset.Y;
            myDoubleAnimation.To = offset;
            myDoubleAnimation.Duration = new Duration(TimeSpan.FromMilliseconds(ScrollDuration * 1000));
            myDoubleAnimation.AutoReverse = false;
            myDoubleAnimation.AccelerationRatio = AccelerationRatio;
            myDoubleAnimation.DecelerationRatio = DecelerationRatio;

            myVerticalOffsetStoryboard = new Storyboard();
            myVerticalOffsetStoryboard.Children.Add(myDoubleAnimation);
            Storyboard.SetTargetProperty(myDoubleAnimation, new PropertyPath(UniformPanel.VerticalOffsetProperty));

            myVerticalOffsetStoryboard.Begin(this);
        }

        private TranslateTransform _trans = new TranslateTransform();
        private ScrollViewer _owner;
        private bool _canHScroll = false;
        private bool _canVScroll = false;
        private Size _extent = new Size(0, 0);
        private Size _viewport = new Size(0, 0);
        private Point _offset;

        #endregion


    }
}
