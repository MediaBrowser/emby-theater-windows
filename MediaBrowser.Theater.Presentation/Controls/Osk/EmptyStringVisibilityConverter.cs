using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace MediaBrowser.Theater.Presentation.Controls.Osk
{
    public class EmptyStringVisibilityConverter
        : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return string.IsNullOrEmpty((string) value) ? Visibility.Collapsed : Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}