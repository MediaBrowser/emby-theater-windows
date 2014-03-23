using System.Windows;
using System.Windows.Controls;

namespace MediaBrowser.Theater.DefaultTheme.Controls
{
    /// <summary>
    ///     Interaction logic for StarRating.xaml
    /// </summary>
    public partial class StarRating : UserControl
    {
        // Using a DependencyProperty as the backing store for Rating.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty RatingProperty =
            DependencyProperty.Register("Rating", typeof (float), typeof (StarRating), new PropertyMetadata(0f, OnRatingChanged));

        public StarRating()
        {
            InitializeComponent();
        }

        public float Rating
        {
            get { return (float) GetValue(RatingProperty); }
            set { SetValue(RatingProperty, value); }
        }

        private static void OnRatingChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (StarRating) d;
            control.UpdateStars();
        }

        private void UpdateStars()
        {
            var images = new[] { ImgCommunityRating1, ImgCommunityRating2, ImgCommunityRating3, ImgCommunityRating4, ImgCommunityRating5 };

            for (int i = 0; i < 5; i++) {
                Image img = images[i];

                int starValue = (i + 1)*2;

                if (Rating < starValue - 2) {
                    img.SetResourceReference(StyleProperty, "CommunityRatingImageEmpty");
                } else if (Rating < starValue) {
                    img.SetResourceReference(StyleProperty, "CommunityRatingImageHalf");
                } else {
                    img.SetResourceReference(StyleProperty, "CommunityRatingImageFull");
                }
            }
        }
    }
}