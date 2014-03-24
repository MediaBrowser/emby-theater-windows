using System;
using System.Globalization;
using System.Windows.Data;

namespace MediaBrowser.Theater.Presentation.Converters
{
    public class NullToBooleanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var result = value != null;
            if (parameter != null)
            {
                if (bool.Parse((string) parameter))
                {
                    result = !result;
                }
            }

            return result;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new InvalidOperationException("NullToBooleanConverter can only be used OneWay.");
        }
    }
}
