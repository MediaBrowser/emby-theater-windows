using MediaBrowser.Model.Entities;
using System.Windows;
using System.Windows.Controls;

namespace MediaBrowser.Plugins.DefaultTheme.Details
{
    /// <summary>
    /// Interaction logic for CriticReviewControl.xaml
    /// </summary>
    public partial class CriticReviewControl : UserControl
    {
        public CriticReviewControl()
        {
            InitializeComponent();

            DataContextChanged += BaseItemTile_DataContextChanged;
        }

        /// <summary>
        /// Gets the view model.
        /// </summary>
        /// <value>The view model.</value>
        public ItemReview ViewModel
        {
            get { return DataContext as ItemReview; }
        }

        /// <summary>
        /// Handles the DataContextChanged event of the BaseItemTile control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="DependencyPropertyChangedEventArgs" /> instance containing the event data.</param>
        void BaseItemTile_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            OnItemChanged();
        }

        /// <summary>
        /// Called when [item changed].
        /// </summary>
        private void OnItemChanged()
        {
            var review = ViewModel;

            TxtReview.Text = "\"" + review.Caption + "\"";

            var source = string.Format("{0}. {1}", review.Publisher ?? string.Empty, review.Date.ToShortDateString());

            if (!string.IsNullOrEmpty(review.ReviewerName))
            {
                source = review.ReviewerName + ", " + source;
            }

            TxtReviewSource.Text = source;

            if (review.Likes.HasValue)
            {
                if (review.Likes.Value)
                {
                    ImgFresh.Visibility = Visibility.Visible;
                    ImgRotten.Visibility = Visibility.Collapsed;
                }
                else
                {
                    ImgFresh.Visibility = Visibility.Collapsed;
                    ImgRotten.Visibility = Visibility.Visible;
                }
            }
            else
            {
                ImgFresh.Visibility = Visibility.Collapsed;
                ImgRotten.Visibility = Visibility.Collapsed;
            }
        }
    }
}
