using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using MediaBrowser.Theater.Presentation.Annotations;

namespace MediaBrowser.Theater.Presentation.Controls
{
    public class IndexedItemsControl : ItemsControl, INotifyPropertyChanged
    {
        public static readonly DependencyProperty IndexSelectorProperty =
            DependencyProperty.Register("IndexSelector", typeof (Func<object, object>), typeof (IndexedItemsControl), new PropertyMetadata((Func<object, object>) (SelectByToString), SelectorChanged));

        private readonly ObservableCollection<object> _indexObjects;
        private readonly Dictionary<object, object> _indicesByItem;
        private readonly Dictionary<object, object> _itemsByIndex;
        private Panel _panel;
        private object _selectedIndex;

        public IndexedItemsControl()
        {
            _indexObjects = new ObservableCollection<object>();
            _itemsByIndex = new Dictionary<object, object>();
            _indicesByItem = new Dictionary<object, object>();

            Loaded += (s, e) => _panel = FindItemsHost();
        }

        static IndexedItemsControl()
        {
            var panelFactory = new FrameworkElementFactory(typeof(PanoramaPanel));
            var defaultPanel = new ItemsPanelTemplate {
                VisualTree = panelFactory
            };

            DefaultStyleKeyProperty.OverrideMetadata(typeof(IndexedItemsControl), new FrameworkPropertyMetadata(typeof(IndexedItemsControl)));
            FocusableProperty.OverrideMetadata(typeof(IndexedItemsControl), new FrameworkPropertyMetadata(false));
            ItemsPanelProperty.OverrideMetadata(typeof(IndexedItemsControl), new FrameworkPropertyMetadata(defaultPanel));
        }

        public ObservableCollection<object> Indices
        {
            get { return _indexObjects; }
        }

        public object SelectedIndex
        {
            get { return _selectedIndex; }
            set
            {
                if (Equals(value, _selectedIndex)) {
                    return;
                }
                _selectedIndex = value;
                OnPropertyChanged();

                if (!IsKeyboardFocusWithin) {
                    ScrollToSelectedIndex();
                }
            }
        }

        public Func<object, object> IndexSelector
        {
            get { return (Func<object, object>) GetValue(IndexSelectorProperty); }
            set { SetValue(IndexSelectorProperty, value); }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private Panel FindItemsHost()
        {
            var itemsPresenter = GetVisualChild<ItemsPresenter>(this);
            if (itemsPresenter == null) {
                return null;
            }

            var itemsPanel = VisualTreeHelper.GetChild(itemsPresenter, 0) as Panel;
            return itemsPanel;
        }

        private static T GetVisualChild<T>(DependencyObject parent) where T : Visual
        {
            T child = default(T);

            int numVisuals = VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < numVisuals; i++) {
                var v = (Visual) VisualTreeHelper.GetChild(parent, i);
                child = v as T;
                if (child == null) {
                    child = GetVisualChild<T>(v);
                }
                if (child != null) {
                    break;
                }
            }
            return child;
        }

        private void ScrollToSelectedIndex()
        {
            object item;
            if (SelectedIndex == null || !_itemsByIndex.TryGetValue(SelectedIndex, out item)) {
                return;
            }

            var virtualizedPanel = _panel as IVirtualizedPanel;
            if (virtualizedPanel != null) {
                virtualizedPanel.MakeItemVisible(item);
            } else {
                var container = ItemContainerGenerator.ContainerFromItem(item) as FrameworkElement;
                if (container != null) {
                    container.BringIntoView();
                }
            }
        }

        // Using a DependencyProperty as the backing store for IndexSelector.  This enables animation, styling, binding, etc...

        private static void SelectorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (IndexedItemsControl) d;
            control.FindIndices();
        }

        private static object SelectByToString(object item)
        {
            return item != null ? item.ToString() : string.Empty;
        }

        protected override void OnPreviewGotKeyboardFocus(KeyboardFocusChangedEventArgs e)
        {
            if (IsKeyboardFocusWithin) {
                var children = new List<DependencyObject>();
                for (var control = e.NewFocus as DependencyObject; control != null; control = VisualTreeHelper.GetParent(control)) {
                    if (Equals(control, this)) {
                        DependencyObject container = children.LastOrDefault(c => c is ExtendedContentControl);
                        if (container != null) {
                            object selectedItem = ItemContainerGenerator.ItemFromContainer(container);
                            SelectedIndex = _indicesByItem[selectedItem];
                        }

                        break;
                    }
                    children.Add(control);
                }
            }

            base.OnPreviewGotKeyboardFocus(e);
        }
        
        protected override void OnItemsChanged(NotifyCollectionChangedEventArgs e)
        {
            FindIndices();
            base.OnItemsChanged(e);
        }

        protected override bool IsItemItsOwnContainerOverride(object item)
        {
            return false;
        }

        protected override DependencyObject GetContainerForItemOverride()
        {
            return new ExtendedContentControl();
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            _panel = FindItemsHost();
        }

        protected override void PrepareContainerForItemOverride(DependencyObject element, object item)
        {
            var container = item as ExtendedContentControl;
            if (container != null) {
                container.Content = element;
            }

            base.PrepareContainerForItemOverride(element, item);
        }

        private void FindIndices()
        {
            _indexObjects.Clear();
            _indicesByItem.Clear();
            _itemsByIndex.Clear();

            Func<object, object> selector = IndexSelector;
            if (selector == null) {
                return;
            }

            object previousIndex = null;
            foreach (object item in Items) {
                object index = selector(item);
                if (!Equals(index, previousIndex)) {
                    _itemsByIndex[index] = item;
                    previousIndex = index;
                }

                _indicesByItem[item] = previousIndex;
            }

            _indexObjects.Clear();
            foreach (object index in _itemsByIndex.Keys) {
                _indexObjects.Add(index);
            }

            if (SelectedIndex == null || !_indexObjects.Contains(SelectedIndex)) {
                SelectedIndex = _indexObjects.FirstOrDefault();
            }
        }

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}