using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace MediaBrowser.Theater.Presentation.Controls
{
    [TemplatePart(Name="PART_Stars", Type=typeof(ItemsControl))]
    public class FiveStarRating : Control
    {
        private ItemsControl _starsControl;
        private IEnumerable<RatingStar> _stars;

        public double Rating
        {
            get { return (double) GetValue(RatingProperty); }
            set { SetValue(RatingProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Rating.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty RatingProperty =
            DependencyProperty.Register("Rating", typeof (double), typeof (FiveStarRating), new PropertyMetadata(0.0, OnRatingChanged));

        private static void OnRatingChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var starRating = d as FiveStarRating;
            if (starRating != null) {
                starRating.CalculateRating();
            }
        }

        static FiveStarRating()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(FiveStarRating), new FrameworkPropertyMetadata(typeof(FiveStarRating)));
        }

        private ItemsControl StarsControl
        {
            get { return _starsControl; }
            set
            {
                _starsControl = value;

                if (_starsControl != null) {
                    _starsControl.ItemsSource = _stars;
                }
            }
        }

        private IEnumerable<RatingStar> Stars
        {
            get { return _stars; }
            set
            {
                _stars = value;
                if (StarsControl != null) {
                    StarsControl.ItemsSource = _stars;
                }
            }
        }

        public override void OnApplyTemplate()
        {
            StarsControl = GetTemplateChild("PART_Stars") as ItemsControl;
            base.OnApplyTemplate();
        }

        private void CalculateRating()
        {
            var rating = (int)Math.Round(Rating);
            var evenNumbers = Math.Floor(rating/2.0);

            var stars = new List<RatingStar>();

            for (int i = 0; i < evenNumbers; i++) {
                stars.Add(new RatingStar { FillState = RatingStarFill.Full });
            }

            if (rating%2 == 1) {
                stars.Add(new RatingStar { FillState = RatingStarFill.Half });
            }

            for (int i = stars.Count; i < 5; i++) {
                stars.Add(new RatingStar { FillState = RatingStarFill.Empty });
            }

            Stars = stars;
        }
    }
}
