using System.Windows;

namespace MediaBrowser.Plugins.DefaultTheme.Controls
{
    /// <summary>
    /// Interaction logic for ItemInfoFooter.xaml
    /// </summary>
    public partial class ItemInfoFooter
    {
        public bool ShowResumeProgress
        {
            get { return ProgressGrid.Visibility == Visibility.Visible; }
            set { ProgressGrid.Visibility = value ? Visibility.Visible : Visibility.Collapsed; }
        }

        public bool ShowUserRating
        {
            get { return UserLikeGrid.Visibility == Visibility.Visible; }
            set
            {
                UserLikeGrid.Visibility = value ? Visibility.Visible : Visibility.Collapsed;
                UserDislikeGrid.Visibility = value ? Visibility.Visible : Visibility.Collapsed;
                UserFavoriteGrid.Visibility = value ? Visibility.Visible : Visibility.Collapsed;
            }
        }
        
        /// <summary>
        /// Initializes a new instance of the <see cref="ItemInfoFooter" /> class.
        /// </summary>
        public ItemInfoFooter()
        {
            InitializeComponent();
        }
    }
}
