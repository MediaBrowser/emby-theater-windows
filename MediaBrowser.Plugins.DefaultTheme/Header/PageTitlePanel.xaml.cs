using MediaBrowser.Model.ApiClient;
using MediaBrowser.Model.Dto;
using MediaBrowser.Model.Net;
using MediaBrowser.Theater.Interfaces.Presentation;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace MediaBrowser.Plugins.DefaultTheme.Header
{
    /// <summary>
    /// Interaction logic for PageTitlePanel.xaml
    /// </summary>
    public partial class PageTitlePanel : UserControl
    {
        internal static IApiClient ApiClient { get; set; }
        internal static IImageManager ImageManager { get; set; }

        internal static PageTitlePanel Current;

        public PageTitlePanel()
        {
            Current = this;

            InitializeComponent();
        }

        /// <summary>
        /// Sets the default page title.
        /// </summary>
        public void SetDefaultPageTitle()
        {
            var panel = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 5, 0, 0) };

            var text = new TextBlock();
            text.Text = "media";
            text.SetResourceReference(TextBlock.StyleProperty, "Heading2TextBlockStyle");
            panel.Children.Add(text);

            text = new TextBlock();
            text.Text = "browser";
            text.SetResourceReference(TextBlock.StyleProperty, "Heading2TextBlockStyle");
            text.Foreground = new SolidColorBrush(Color.FromRgb(82, 181, 75));
            panel.Children.Add(text);

            SetPageTitle(panel);
        }

        /// <summary>
        /// Sets the page title.
        /// </summary>
        /// <param name="item">The item.</param>
        public async Task SetPageTitle(BaseItemDto item)
        {
            if (item.HasLogo || !string.IsNullOrEmpty(item.ParentLogoItemId))
            {
                var url = ApiClient.GetLogoImageUrl(item, new ImageOptions
                {
                });

                try
                {
                    var image = await ImageManager.GetRemoteImageAsync(url);

                    image.SetResourceReference(Image.StyleProperty, "ItemLogo");
                    SetPageTitle(image);
                }
                catch (HttpException)
                {
                    SetPageTitleText(item);
                }
            }
            else
            {
                SetPageTitleText(item);
            }
        }

        /// <summary>
        /// Sets the page title text.
        /// </summary>
        /// <param name="item">The item.</param>
        private void SetPageTitleText(BaseItemDto item)
        {
            SetPageTitle(item.SeriesName ?? item.Album ?? item.Name);
        }

        /// <summary>
        /// Sets the page title.
        /// </summary>
        /// <param name="title">The title.</param>
        public void SetPageTitle(string title)
        {
            var textblock = new TextBlock { Text = title, Margin = new Thickness(0, 5, 0, 0) };
            textblock.SetResourceReference(TextBlock.StyleProperty, "Heading2TextBlockStyle");

            SetPageTitle(textblock);
        }

        /// <summary>
        /// Sets the page title.
        /// </summary>
        /// <param name="element">The element.</param>
        public void SetPageTitle(UIElement element)
        {
            MainGrid.Children.Clear();
            MainGrid.Children.Add(element);
        }
    }
}
