// http://stackoverflow.com/questions/4793030/wpf-reset-listbox-scroll-position-when-itemssource-changes

using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;

namespace MediaBrowser.Theater.Presentation.Behaviors
{
    public static class ScrollToTopBehavior
    {
        public static readonly DependencyProperty ScrollToTopProperty =
            DependencyProperty.RegisterAttached
                (
                 "ScrollToTop",
                 typeof (bool),
                 typeof (ScrollToTopBehavior),
                 new UIPropertyMetadata(false, OnScrollToTopPropertyChanged)
                );

        public static bool GetScrollToTop(DependencyObject obj)
        {
            return (bool) obj.GetValue(ScrollToTopProperty);
        }

        public static void SetScrollToTop(DependencyObject obj, bool value)
        {
            obj.SetValue(ScrollToTopProperty, value);
        }

        private static void OnScrollToTopPropertyChanged(DependencyObject dpo,
                                                         DependencyPropertyChangedEventArgs e)
        {
            var itemsControl = dpo as ItemsControl;
            if (itemsControl != null) {
                DependencyPropertyDescriptor dependencyPropertyDescriptor =
                    DependencyPropertyDescriptor.FromProperty(ItemsControl.ItemsSourceProperty, typeof (ItemsControl));
                if (dependencyPropertyDescriptor != null) {
                    if ((bool) e.NewValue) {
                        dependencyPropertyDescriptor.AddValueChanged(itemsControl, ItemsSourceChanged);
                    } else {
                        dependencyPropertyDescriptor.RemoveValueChanged(itemsControl, ItemsSourceChanged);
                    }
                }
            }
        }

        private static void ItemsSourceChanged(object sender, EventArgs e)
        {
            var itemsControl = sender as ItemsControl;
            EventHandler eventHandler = null;

            eventHandler = delegate {
                if (itemsControl.ItemContainerGenerator.Status == GeneratorStatus.ContainersGenerated) {
                    var scrollViewer = GetVisualChild<ScrollViewer>(itemsControl);
                    scrollViewer.ScrollToTop();
                    scrollViewer.ScrollToLeftEnd();
                    itemsControl.ItemContainerGenerator.StatusChanged -= eventHandler;
                }
            };

            itemsControl.ItemContainerGenerator.StatusChanged += eventHandler;
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
    }
}