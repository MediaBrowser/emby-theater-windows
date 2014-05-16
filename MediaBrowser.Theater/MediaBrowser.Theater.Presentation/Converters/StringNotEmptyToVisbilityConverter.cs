using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace MediaBrowser.Theater.Presentation.Converters
{
    public class StringNotEmptyToVisbilityConverter
        : IValueConverter
    {
        public bool VisibleIfEmpty { get; set; }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (VisibleIfEmpty) {
                return string.IsNullOrEmpty(value as string) ? Visibility.Collapsed : Visibility.Visible;
            } else {
                return string.IsNullOrEmpty(value as string) ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}