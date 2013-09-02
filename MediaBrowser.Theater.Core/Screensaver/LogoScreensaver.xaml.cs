using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;

namespace MediaBrowser.Theater.Core.Screensaver
{
    /// <summary>
    /// Interaction logic for LogoScreensaver.xaml
    /// </summary>
    public partial class LogoScreensaver : UserControl
    {
        public LogoScreensaver()
        {
            InitializeComponent();

            Loaded += LogoScreensaver_Loaded;
        }

        void LogoScreensaver_Loaded(object sender, RoutedEventArgs e)
        {
            var doubleAnimation = new DoubleAnimation();
            doubleAnimation.From = -Tbmarquee.ActualWidth;
            doubleAnimation.To = CanMain.ActualWidth;
            doubleAnimation.RepeatBehavior = RepeatBehavior.Forever;
            doubleAnimation.Duration = new Duration(TimeSpan.Parse("0:0:10"));
            Tbmarquee.BeginAnimation(Canvas.LeftProperty, doubleAnimation);
        }
    }
}
