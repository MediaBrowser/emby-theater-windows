using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using MediaBrowser.Theater.Presentation.ViewModels;

namespace MediaBrowser.Theater.Presentation.Controls
{
    [TemplatePart(Name="PART_Titles", Type=typeof(ListBox))]
    public class Panorama : ItemsControl
    {
        // Using a DependencyProperty as the backing store for TitleItems.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TitleItemsProperty =
            DependencyProperty.Register("TitleItems", typeof (IEnumerable<IPanoramaPage>), typeof (Panorama), new PropertyMetadata(null));

        // Using a DependencyProperty as the backing store for SelectedTitleItem.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SelectedTitleItemProperty =
            DependencyProperty.Register("SelectedTitleItem", typeof (IPanoramaPage), typeof (Panorama), new PropertyMetadata(null, OnSelectedTitleItemChanged));


        // Using a DependencyProperty as the backing store for ItemDisplayNameStyle.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ItemDisplayNameStyleProperty =
            DependencyProperty.Register("ItemDisplayNameStyle", typeof (Style), typeof (Panorama), new PropertyMetadata(null));


        // Using a DependencyProperty as the backing store for TitleTemplate.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TitleTemplateProperty =
            DependencyProperty.Register("TitleTemplate", typeof (DataTemplate), typeof (Panorama), new PropertyMetadata(null));

        private ListBox _titlesListBox;

        private ListBox TitlesListBox
        {
            get { return _titlesListBox; }
            set
            {
                if (_titlesListBox != null) {
                    _titlesListBox.PreviewKeyDown -= _titlesListBox_PreviewKeyDown;
                }

                _titlesListBox = value;

                if (_titlesListBox != null) {
                    _titlesListBox.PreviewKeyDown += _titlesListBox_PreviewKeyDown;
                }
            }
        }

        void _titlesListBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Down) {
                var container = ItemContainerGenerator.ContainerFromItem(SelectedTitleItem) as FrameworkElement;
                if (container != null) {
                    container.MoveFocus(new TraversalRequest(FocusNavigationDirection.First));
                }

                e.Handled = true;
            }
        }

        static Panorama()
        {
            var panelFactory = new FrameworkElementFactory(typeof (StackPanel));
            panelFactory.SetValue(StackPanel.OrientationProperty, Orientation.Horizontal);

            var defaultPanel = new ItemsPanelTemplate {
                VisualTree = panelFactory
            };

            DefaultStyleKeyProperty.OverrideMetadata(typeof (Panorama), new FrameworkPropertyMetadata(typeof (Panorama)));
            FocusableProperty.OverrideMetadata(typeof (Panorama), new FrameworkPropertyMetadata(false));
            ItemsPanelProperty.OverrideMetadata(typeof(Panorama), new FrameworkPropertyMetadata(defaultPanel));
        }
        
        public override void OnApplyTemplate()
        {
            TitlesListBox = GetTemplateChild("PART_Titles") as ListBox;
            base.OnApplyTemplate();
        }

        public Panorama()
        {
            PreviewGotKeyboardFocus += Panorama_PreviewGotKeyboardFocus;
        }

        public Style ItemDisplayNameStyle
        {
            get { return (Style) GetValue(ItemDisplayNameStyleProperty); }
            set { SetValue(ItemDisplayNameStyleProperty, value); }
        }

        public DataTemplate TitleTemplate
        {
            get { return (DataTemplate) GetValue(TitleTemplateProperty); }
            set { SetValue(TitleTemplateProperty, value); }
        }

        public IEnumerable<IPanoramaPage> TitleItems
        {
            get { return (IEnumerable<IPanoramaPage>) GetValue(TitleItemsProperty); }
            set { SetValue(TitleItemsProperty, value); }
        }

        public IPanoramaPage SelectedTitleItem
        {
            get { return (IPanoramaPage) GetValue(SelectedTitleItemProperty); }
            set { SetValue(SelectedTitleItemProperty, value); }
        }

        private static void OnSelectedTitleItemChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var panorama = d as Panorama;
            if (panorama != null) {
                panorama.ScrollToSelectedTitleItem();
            }
        }

        private void Panorama_PreviewGotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            for (var control = e.NewFocus as DependencyObject; control != null; control = VisualTreeHelper.GetParent(control)) {
                if (control is PanoramaItem) {
                    var selectedItem = ItemContainerGenerator.ItemFromContainer(control) as IPanoramaPage;
                    if (TitleItems.Contains(selectedItem) && SelectedTitleItem != selectedItem) {
                        SelectedTitleItem = selectedItem;
                    }

                    break;
                }
            }
        }

        private void ScrollToSelectedTitleItem()
        {
            var container = ItemContainerGenerator.ContainerFromItem(SelectedTitleItem) as FrameworkElement;
            if (container != null) {
                container.BringIntoView();
            }
        }

        protected override DependencyObject GetContainerForItemOverride()
        {
            return new PanoramaItem();
        }

        protected override void PrepareContainerForItemOverride(DependencyObject element, object item)
        {
            var panoramaItem = element as PanoramaItem;
            if (panoramaItem != null) {
                if (ItemContainerStyle != null && panoramaItem.Style == null) {
                    panoramaItem.SetValue(StyleProperty, ItemContainerStyle);
                }

                if (ItemDisplayNameStyle != null && panoramaItem.DisplayNameStyle == null) {
                    panoramaItem.DisplayNameStyle = ItemDisplayNameStyle;
                }

                panoramaItem.Content = item;
            }

            base.PrepareContainerForItemOverride(element, item);
        }
    }

    public class PanoramaItem : Control
    {
        // Using a DependencyProperty as the backing store for DisplayName.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty DisplayNameProperty =
            DependencyProperty.Register("DisplayName", typeof (string), typeof (PanoramaItem), new PropertyMetadata(null));


        // Using a DependencyProperty as the backing store for Content.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ContentProperty =
            DependencyProperty.Register("Content", typeof (object), typeof (PanoramaItem), new PropertyMetadata(null, OnContentChanged));

        private static void OnContentChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var item = (PanoramaItem)d;
            var content = e.NewValue as IPanoramaPage;

            if (content != null) {
                var visibilityBinding = new Binding("IsVisible");
                item.SetBinding(IsContentVisibleProperty, visibilityBinding);

                var nameBinding = new Binding("DisplayName");
                item.SetBinding(DisplayNameProperty, nameBinding);
            }
        }

        // Using a DependencyProperty as the backing store for DisplayNameStyle.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty DisplayNameStyleProperty =
            DependencyProperty.Register("DisplayNameStyle", typeof (Style), typeof (PanoramaItem), new PropertyMetadata(null));
        
        public bool IsContentVisible
        {
            get { return (bool)GetValue(IsContentVisibleProperty); }
            set { SetValue(IsContentVisibleProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IsContentVisible.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsContentVisibleProperty =
            DependencyProperty.Register("IsContentVisible", typeof(bool), typeof(PanoramaItem), new PropertyMetadata(true));

        
        static PanoramaItem()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof (PanoramaItem), new FrameworkPropertyMetadata(typeof (PanoramaItem)));
        }

        public Style DisplayNameStyle
        {
            get { return (Style) GetValue(DisplayNameStyleProperty); }
            set { SetValue(DisplayNameStyleProperty, value); }
        }

        public string DisplayName
        {
            get { return (string) GetValue(DisplayNameProperty); }
            set { SetValue(DisplayNameProperty, value); }
        }

        public object Content
        {
            get { return GetValue(ContentProperty); }
            set { SetValue(ContentProperty, value); }
        }
    }
}