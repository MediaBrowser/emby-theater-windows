using System;
using MediaBrowser.Plugins.DefaultTheme.Resources;
using MediaBrowser.UI.Pages;
using System.Windows;

namespace MediaBrowser.Plugins.DefaultTheme.Pages
{
    /// <summary>
    /// Interaction logic for InternalPlayerPage.xaml
    /// </summary>
    public partial class InternalPlayerPage : BaseInternalPlayerPage
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="InternalPlayerPage" /> class.
        /// </summary>
        public InternalPlayerPage()
        {
            InitializeComponent();
        }

        //protected override void OnInitialized(EventArgs e)
        //{
        //    base.OnInitialized(e);

        //    Unloaded += InternalPlayerPage_Unloaded;
        //}

        //void InternalPlayerPage_Unloaded(object sender, RoutedEventArgs e)
        //{
        //    AppResources.Instance.HeaderContent.Visibility = Visibility.Visible;
        //}

        ///// <summary>
        ///// Called when [loaded].
        ///// </summary>
        //protected override void OnLoaded()
        //{
        //    base.OnLoaded();

        //    AppResources.Instance.ClearPageTitle();
        //    AppResources.Instance.HeaderContent.Visibility = Visibility.Collapsed;
        //}
    }
}
