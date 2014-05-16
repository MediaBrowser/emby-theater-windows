using System;
using System.Globalization;
using System.Windows.Data;

namespace MediaBrowser.Theater.Presentation.Converters
{
    public class LowercaseConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var text = (string) value;
            if (text == null) {
                return null;
            }

            return text.ToLower(culture);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}