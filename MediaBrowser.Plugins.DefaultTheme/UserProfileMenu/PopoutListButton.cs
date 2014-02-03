using System;
using System.Windows;
using System.Windows.Controls;

namespace MediaBrowser.Plugins.DefaultTheme.UserProfileMenu
{
    public class PopoutListButton : Button
    {
        public static readonly DependencyProperty PopoutModeEnabledProperty =
             DependencyProperty.Register("PopoutModeEnabled", typeof(bool),
             typeof(PopoutListButton), new FrameworkPropertyMetadata(false,
                 FrameworkPropertyMetadataOptions.AffectsRender));


        public bool PopoutModeEnabled
        {
            get { return (bool)GetValue(PopoutModeEnabledProperty); }
            set { SetValue(PopoutModeEnabledProperty, value); }
        }
    }
}
