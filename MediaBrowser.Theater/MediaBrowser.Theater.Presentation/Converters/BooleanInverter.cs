using System;
using System.Globalization;
using System.Windows.Data;

namespace MediaBrowser.Theater.Presentation.Converters
{
    public class BooleanInverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return !(bool) value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return !(bool) value;
        }
    }
}