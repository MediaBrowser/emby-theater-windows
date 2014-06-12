using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace MediaBrowser.Theater.Presentation.Controls
{
    public enum RatingStarFill
    {
        Empty,
        Half,
        Full
    }

    public class RatingStar : Control
    {
        // Using a DependencyProperty as the backing store for FillState.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty FillStateProperty =
            DependencyProperty.Register("FillState", typeof (RatingStarFill), typeof (RatingStar), new PropertyMetadata(RatingStarFill.Empty));

        // Using a DependencyProperty as the backing store for Fill.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty FillProperty =
            DependencyProperty.Register("Fill", typeof (Brush), typeof (RatingStar), new PropertyMetadata(null));
        
        static RatingStar()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof (RatingStar), new FrameworkPropertyMetadata(typeof (RatingStar)));
        }

        public RatingStarFill FillState
        {
            get { return (RatingStarFill) GetValue(FillStateProperty); }
            set { SetValue(FillStateProperty, value); }
        }
        
        public Brush Fill
        {
            get { return (Brush) GetValue(FillProperty); }
            set { SetValue(FillProperty, value); }
        }
    }
}