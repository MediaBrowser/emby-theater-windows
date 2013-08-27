using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Markup;

namespace MediaBrowser.Theater.Presentation.Controls
{
    public class UpperCaseConverter : MarkupExtension, IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var str = value as string;

            return str != null ? str.ToUpper() : value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        private static UpperCaseConverter _converter;
        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            if (_converter == null)
            {
                _converter = new UpperCaseConverter();
            }
            return _converter;
        }
    }
}
