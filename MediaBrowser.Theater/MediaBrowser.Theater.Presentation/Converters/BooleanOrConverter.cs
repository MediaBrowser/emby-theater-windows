using System;
using System.Globalization;
using System.Linq;
using System.Windows.Data;

namespace MediaBrowser.Theater.Presentation.Converters
{
    public class BooleanOrConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            return values.Any(value => value is bool && (bool) value);
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}