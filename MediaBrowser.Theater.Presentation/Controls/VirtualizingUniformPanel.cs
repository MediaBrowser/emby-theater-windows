using System;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace MediaBrowser.Theater.Presentation.Controls
{
    /// <summary>    
    /// Inherits VirtualizingPanel
    /// Implements IScrollInfo
    /// DependencyProperties:
    /// - Orientation Orientation
    /// - int MaxVisibleColumnCount
    /// - int MaxVisibleRowCount
    /// - double ScrollDuration
    /// - double DecelerationRatio
    /// - double AccelerationRatio
    /// </summary>
    public class VirtualizingUniformPanel : VirtualizingPanel, IScrollInfo
    {
        /// <summary>
        /// The maximum count of items which are NOT visible. 
        /// If this number is exceeded, items are revirtualized - even if Panel is not idle.
        /// Set to 0 to only do a clean-up when the Panel is idle.
        /// </summary>
        private int maxObsoleteItemCount = 128;

        /// <summary>
        /// Initializes a new instance of VirtualizingUniformPanel.
        /// </summary>
        public VirtualizingUniformPanel()
            : base()
        {
            // For use in the IScrollInfo implementation
            this.RenderTransform = _trans;
        }

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
            DependencyProperty.Register("Orientation", typeof(Orientation), typeof(VirtualizingUniformPanel),
                                        new FrameworkPropertyMetadata(Orientation.Vertical, FrameworkPropertyMetadataOptions.AffectsMeasure, OnOrientationPropertyChanged));

        private static void OnOrientationPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            VirtualizingUniformPanel b = d as VirtualizingUniformPanel;
            b.OnOrientationPropertyChanged(e);
        }

        private void OnOrientationPropertyChanged(DependencyPropertyChangedEventArgs e)
        {

            // TODO: set rows / columns
        }

        #endregion

        /// <summary>
        /// The count of rows used to arrange the child controls.
        /// </summary>
        #region int MaxVisibleRowCount
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
            DependencyProperty.Register("MaxVisibleRowCount", typeof(int), typeof(VirtualizingUniformPanel),
                                        new FrameworkPropertyMetadata(5, OnVisibleRowPropertyChanged));

        private static void OnVisibleRowPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            VirtualizingUniformPanel b = d as VirtualizingUniformPanel;
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
            DependencyProperty.Register("MaxVisibleColumnCount", typeof(int), typeof(VirtualizingUniformPanel),
                                        new FrameworkPropertyMetadata(1, OnVisibleColumnPropertyChanged));

        private static void OnVisibleColumnPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            VirtualizingUniformPanel b = d as VirtualizingUniformPanel;
            b.OnVisibleColumnPropertyChanged((int)e.OldValue, (int)e.NewValue);
        }

        private void OnVisibleColumnPropertyChanged(int OldValue, int NewValue)
        {
            // TODO: set rows / columns or refresh
        }

        #endregion

        /// <summary>
        /// The computed size of a child control.
        /// </summary>
        protected virtual Size ChildSize
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

            // Figure out range that's visible based on layout algorithm
            int firstVisibleItemIndex, lastVisibleItemIndex;
            GetVisibleRange(out firstVisibleItemIndex, out lastVisibleItemIndex);

            CreateItems(firstVisibleItemIndex, lastVisibleItemIndex);

            // Note: this is deferred to idle time.
            CleanUpItemsOnIdle();

            return base.MeasureOverride(availableSize);
        }

        /// <summary>
        /// Arrange the children
        /// </summary>
        /// <param name="finalSize">Size available</param>
        /// <returns>Size used</returns>
        protected override Size ArrangeOverride(Size finalSize)
        {

            IItemContainerGenerator generator = this.ItemContainerGenerator;

            if (generator == null) // might happen if createitems is invoked and screen is closed, so it might happen here as well.
                return base.ArrangeOverride(finalSize);

            UpdateScrollInfo(finalSize);

            for (int i = 0; i < this.Children.Count; i++)
            {
                UIElement child = this.Children[i];

                // Map the child offset to an item offset
                int itemIndex = generator.IndexFromGeneratorPosition(new GeneratorPosition(i, 0));

                ArrangeChild(itemIndex, child, finalSize);
            }

            return finalSize;

        }

        /// <summary>
        /// Generates items whenever they're needed (that is: when they come into view).
        /// </summary>
        /// <param name="firstVisibleItemIndex"></param>
        /// <param name="lastVisibleItemIndex"></param>
        protected void CreateItems(int firstVisibleItemIndex, int lastVisibleItemIndex)
        {
            // We need to access InternalChildren before the generator to work around a bug
            UIElementCollection children = this.InternalChildren;
            IItemContainerGenerator generator = this.ItemContainerGenerator;
            if (generator == null) // happens if createitems is invoked and screen is closed.
                return;

            // Get the generator position of the first visible data item
            GeneratorPosition startPos = generator.GeneratorPositionFromIndex(firstVisibleItemIndex);

            // Get index where we'd insert the child for this position. If the item is realized
            // (position.Offset == 0), it's just position.Index, otherwise we have to add one to
            // insert after the corresponding child
            int childIndex = (startPos.Offset == 0) ? startPos.Index : startPos.Index + 1;

            // if availableSize has a zero component, firstVisibleItemIndex is less than zero.
            // Note that bugs behave the same!
            if (firstVisibleItemIndex >= 0)
            {
                lock (generator)
                {
                    using (generator.StartAt(startPos, GeneratorDirection.Forward, true))
                    {
                        for (int itemIndex = firstVisibleItemIndex; itemIndex <= lastVisibleItemIndex; ++itemIndex, ++childIndex)
                        {
                            bool newlyRealized;

                            // Get or create the child
                            UIElement child = generator.GenerateNext(out newlyRealized) as UIElement;
                            if (newlyRealized)
                            {
                                // Figure out if we need to insert the child at the end or somewhere in the middle
                                if (childIndex >= children.Count)
                                {
                                    base.AddInternalChild(child);
                                }
                                else
                                {
                                    base.InsertInternalChild(childIndex, child);
                                }

                                generator.PrepareItemContainer(child);
                            }
                            else
                            {
                                // The child has already been created, let's be sure it's in the right spot
                                if (childIndex < children.Count && childIndex >= 0)
                                {
                                    //System.Diagnostics.Debug.Assert(child == children[childIndex], "Wrong child was generated");
                                    //if (child != children[childIndex])
                                    //    System.Diagnostics.Trace.WriteLine("CreateItems: Wrong child was generated " + childIndex + " / " + children.Count, "Log");
                                }
                            }

                            // Measurements will depend on layout algorithm
                            if (child != null)
                                child.Measure(ChildSize);
                        }
                    }
                }

                // If there are at least maxObsoleteItemCount unneeded items, force a CleanUp.
                if (maxObsoleteItemCount > 0 && InternalChildren.Count > maxObsoleteItemCount + lastVisibleItemIndex - firstVisibleItemIndex)
                    dt_Tick(null, null);
            }
        }

        private System.Windows.Threading.DispatcherTimer dt;
        /// <summary>
        /// Revirtualize items that are no longer visible. This method waits until the system is idle plus one second before revirtualizing.
        /// </summary>
        private void CleanUpItemsOnIdle()
        {
            if (dt == null)
            {
                dt = new System.Windows.Threading.DispatcherTimer(System.Windows.Threading.DispatcherPriority.SystemIdle, this.Dispatcher);

                dt.Interval = TimeSpan.FromSeconds(1);
                dt.Tick += new EventHandler(dt_Tick);
            }

            dt.Stop();
            dt.Start();
        }

        private System.Windows.Threading.DispatcherTimer dt2;
        private void CleanUpItemsAfterNavigation()
        {
            if (dt2 == null)
            {
                dt2 = new System.Windows.Threading.DispatcherTimer(System.Windows.Threading.DispatcherPriority.Send, this.Dispatcher);

                dt2.Interval = TimeSpan.FromSeconds(1);
                dt2.Tick += new EventHandler(dt_Tick);
            }

            dt2.Stop();
            dt2.Start();
        }


        private void dt_Tick(object sender, EventArgs e)
        {
            // Figure out range that's visible based on layout algorithm
            int firstVisibleItemIndex, lastVisibleItemIndex;
            GetVisibleRange(out firstVisibleItemIndex, out lastVisibleItemIndex);

            // since CleanUpItems is slow, we only do if really needed
            if (InternalChildren.Count - 1 <= lastVisibleItemIndex - firstVisibleItemIndex)
                return;

            CleanUpItems(firstVisibleItemIndex, lastVisibleItemIndex);
        }

        /// <summary>
        /// Instantly revirtualize items that are no longer visible.
        /// </summary>
        /// <param name="minDesiredGenerated">first item index that should be visible</param>
        /// <param name="maxDesiredGenerated">last item index that should be visible</param>
        protected virtual void CleanUpItems(int minDesiredGenerated, int maxDesiredGenerated)
        {
            if (dt != null)
                dt.Stop();

            //if (InternalChildren.Count - 1 <= maxDesiredGenerated - minDesiredGenerated)
            //    return;
            UIElementCollection children = this.InternalChildren;
            IItemContainerGenerator generator = this.ItemContainerGenerator;

            for (int i = children.Count - 1; i >= 0; i--)
            {
                GeneratorPosition childGeneratorPos = new GeneratorPosition(i, 0);
                int itemIndex = generator.IndexFromGeneratorPosition(childGeneratorPos);
                if (itemIndex < minDesiredGenerated || itemIndex > maxDesiredGenerated)
                {
                    generator.Remove(childGeneratorPos, 1);
                    RemoveInternalChildRange(i, 1);
                }
            }
        }

        /// <summary>
        /// When items are removed, remove the corresponding UI if necessary
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        protected override void OnItemsChanged(object sender, ItemsChangedEventArgs args)
        {
            switch (args.Action)
            {
                case NotifyCollectionChangedAction.Remove:
                case NotifyCollectionChangedAction.Replace:
                case NotifyCollectionChangedAction.Move:
                    RemoveInternalChildRange(args.Position.Index, args.ItemUICount);
                    break;
            }
            base.OnItemsChanged(sender, args);
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
        /// Get the range of children that are visible
        /// </summary>
        /// <param name="firstVisibleItemIndex">The item index of the first visible item</param>
        /// <param name="lastVisibleItemIndex">The item index of the last visible item</param>
        /// <param name="offset"></param>
        private void GetVisibleRange(out int firstVisibleItemIndex, out int lastVisibleItemIndex, Point offset)
        {
            if (Orientation == Orientation.Vertical)
            {
                // TODO: Make this an int collection!
                int childrenPerRow = CalculateChildrenPerRow(_extent);

                firstVisibleItemIndex = (int)Math.Floor(offset.Y / this.ChildSize.Height) * childrenPerRow;
                lastVisibleItemIndex = (int)Math.Ceiling((offset.Y + _viewport.Height) / this.ChildSize.Height) * childrenPerRow - 1;
            }
            else
            {
                int childrenPerColumn = CalculateChildrenPerColumn(_extent);
                firstVisibleItemIndex = (int)Math.Floor(offset.X / this.ChildSize.Width) * childrenPerColumn;
                lastVisibleItemIndex = (int)Math.Ceiling((offset.X + _viewport.Width) / this.ChildSize.Width) * childrenPerColumn - 1;
            }

            ItemsControl itemsControl = ItemsControl.GetItemsOwner(this);
            int itemCount = (itemsControl != null && itemsControl.HasItems) ? itemsControl.Items.Count : 0;
            if (lastVisibleItemIndex >= itemCount)
                lastVisibleItemIndex = itemCount - 1;

            if (firstVisibleItemIndex >= itemCount)
                firstVisibleItemIndex = itemCount - 1;
        }

        private void GetVisibleRange(out int firstVisibleItemIndex, out int lastVisibleItemIndex)
        {
            GetVisibleRange(out firstVisibleItemIndex, out lastVisibleItemIndex, new Point(_offset.X, _offset.Y));
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

            if (child != null)
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
            int itemCount = (itemsControl != null && itemsControl.HasItems) ? itemsControl.Items.Count : 1;

            if (this.Orientation == Orientation.Vertical)
                childrenPerRow = MaxVisibleColumnCount;
            else
                childrenPerRow = (int)Math.Ceiling((double)itemCount / (double)MaxVisibleRowCount);

            return childrenPerRow;
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
            int itemCount = (itemsControl != null && itemsControl.HasItems) ? itemsControl.Items.Count : 1;

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
            DependencyProperty.Register("ScrollDuration", typeof(double), typeof(VirtualizingUniformPanel), new UIPropertyMetadata(1d));

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
            DependencyProperty.Register("DecelerationRatio", typeof(double), typeof(VirtualizingUniformPanel), new UIPropertyMetadata(1d));

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
            DependencyProperty.Register("AccelerationRatio", typeof(double), typeof(VirtualizingUniformPanel), new UIPropertyMetadata(0d));

        #endregion


        #region IScrollInfo implementation
        // See Ben Constable's series of posts at http://blogs.msdn.com/bencon/



        private void UpdateScrollInfo(Size availableSize)
        {
            // See how many items there are
            ItemsControl itemsControl = ItemsControl.GetItemsOwner(this);
            int itemCount = (itemsControl != null && itemsControl.HasItems) ? itemsControl.Items.Count : 0;

            Size extent = CalculateExtent(availableSize, itemCount);
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
            DependencyProperty.Register("HorizontalOffset", typeof(double), typeof(VirtualizingUniformPanel),
                               new FrameworkPropertyMetadata(0d, OnHorizontalOffsetPropertyChanged));

        private static void OnHorizontalOffsetPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            VirtualizingUniformPanel b = d as VirtualizingUniformPanel;
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

            CleanUpItemsAfterNavigation();
        }

        /// <summary>
        /// Gets the vertical offset of the scrolled content.
        /// </summary>
        public double VerticalOffset
        {
            get { return _offset.Y; }
        }

        /// <summary>
        /// Identifies the VerticalOffset dependency property.
        /// </summary>
        public static readonly DependencyProperty VerticalOffsetProperty =
            DependencyProperty.Register("VerticalOffset", typeof(double), typeof(VirtualizingUniformPanel),
                                       new FrameworkPropertyMetadata(0d, OnVerticalOffsetPropertyChanged));

        private static void OnVerticalOffsetPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            VirtualizingUniformPanel b = d as VirtualizingUniformPanel;
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

            CleanUpItemsAfterNavigation();
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
        /// <param name="visual">A Visual that becomes visible. </param>
        /// <param name="rectangle">A bounding rectangle that identifies the coordinate space to make visible. </param>
        /// <returns></returns>
        public Rect MakeVisible(Visual visual, Rect rectangle)
        {
            IItemContainerGenerator igenerator = this.ItemContainerGenerator;
            if (igenerator == null)
                return Rect.Empty;
            ItemContainerGenerator generator = igenerator.GetItemContainerGeneratorForPanel(this);
            if (generator == null)
                return Rect.Empty;
            int itemIndex = generator.IndexFromContainer(visual);

            // in virtualizing panels visual might already be detached from ItemContainerGenerator, itemIndex would be -1.
            if (itemIndex == -1)
            {
                // so the item's content is looked up in the base list.
                ContentControl cc = visual as ContentControl;
                if (cc != null)
                {
                    object c = cc.DataContext; // or Content? Content can still be null while DataContext is already non-null...

                    ItemsControl itemsControl = ItemsControl.GetItemsOwner(this);
                    itemIndex = (itemsControl != null && itemsControl.HasItems) ? itemsControl.Items.IndexOf(c) : -1;

                    // TODO: we now have the index of an item which was detached.
                    // this could be a hint that we don't want to scroll there, but "jump" instead.
                }
            }

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

            // TODO: add an option to only scroll if the item is NOT visible.
            // This would let it behave like a standard WPF/WinForm container.

            double hOffset = column * this.ChildSize.Width - (this.ViewportWidth - this.ChildSize.Width) / 2;
            double vOffset = row * this.ChildSize.Height - (this.ViewportHeight - this.ChildSize.Height) / 2;

            // to allow fast keyboard navigation, the neighbours of visual are now created.
            // they'd be created in MeasureOverride anyway, but this might be too late.
            int firstVisibleItemIndex, lastVisibleItemIndex;
            GetVisibleRange(out firstVisibleItemIndex, out lastVisibleItemIndex, new Point(hOffset, vOffset));
            CreateItems(firstVisibleItemIndex, lastVisibleItemIndex);

            this.SetHorizontalOffset(hOffset);
            this.SetVerticalOffset(vOffset);

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
            myDoubleAnimation.Duration = new Duration(TimeSpan.FromMilliseconds(ScrollDuration * 500));
            myDoubleAnimation.AutoReverse = false;
            myDoubleAnimation.AccelerationRatio = AccelerationRatio;
            myDoubleAnimation.DecelerationRatio = DecelerationRatio;

            myHorizontalOffsetStoryboard = new Storyboard();
            myHorizontalOffsetStoryboard.Children.Add(myDoubleAnimation);
            Storyboard.SetTargetProperty(myDoubleAnimation, new PropertyPath(VirtualizingUniformPanel.HorizontalOffsetProperty));

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
            myDoubleAnimation.Duration = new Duration(TimeSpan.FromMilliseconds(ScrollDuration * 500));
            myDoubleAnimation.AutoReverse = false;
            myDoubleAnimation.AccelerationRatio = AccelerationRatio;
            myDoubleAnimation.DecelerationRatio = DecelerationRatio;

            myVerticalOffsetStoryboard = new Storyboard();
            myVerticalOffsetStoryboard.Children.Add(myDoubleAnimation);
            Storyboard.SetTargetProperty(myDoubleAnimation, new PropertyPath(VirtualizingUniformPanel.VerticalOffsetProperty));

            myVerticalOffsetStoryboard.Begin(this);
        }

        private TranslateTransform _trans = new TranslateTransform();
        private ScrollViewer _owner;
        private bool _canHScroll = false;
        private bool _canVScroll = false;
        private Size _extent = new Size(0, 0);
        /// <summary>
        /// The Viewport backing field.
        /// </summary>
        protected Size _viewport = new Size(0, 0);
        private Point _offset;

        #endregion
    }
}
