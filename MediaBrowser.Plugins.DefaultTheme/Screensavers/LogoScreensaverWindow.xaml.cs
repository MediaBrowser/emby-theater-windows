using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using MediaBrowser.Common;
using MediaBrowser.Model.Logging;
using MediaBrowser.Theater.Interfaces.Presentation;
using System;

namespace MediaBrowser.Plugins.DefaultTheme.Screensavers
{
    /// <summary>
    /// Screen saver factory to create User Screen saver
    /// </summary>
    public class  LogoScreensaverFactory : IScreensaverFactory
    {
        private readonly IApplicationHost _applicationHost;
      
        public LogoScreensaverFactory(IApplicationHost applicationHost)
        {
            _applicationHost = applicationHost;
        }

        public IScreensaver GetScreensaver()
        {
            return (IScreensaver)_applicationHost.CreateInstance(typeof(LogoScreensaverWindow));
        }
    }
   
    /// <summary>
    /// Interaction logic for ScreensaverWindow.xaml
    /// </summary>
    public partial class LogoScreensaverWindow : ScreensaverWindowBase
    {
        public LogoScreensaverWindow(IPresentationManager presentationManager, IScreensaverManager screensaverManager, ILogger logger) : base(presentationManager, screensaverManager, logger)
        {
            InitializeComponent();
            
            DataContext = this;
            Loaded += LogoScreensaver_Loaded;
        }

        protected override string ScreensaverName() { return "Logo"; }

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
