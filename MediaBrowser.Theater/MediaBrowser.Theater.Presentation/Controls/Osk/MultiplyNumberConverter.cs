using System;
using System.Globalization;
using System.Windows.Data;

namespace MediaBrowser.Theater.Presentation.Controls.Osk
{
    public class MultiplyNumberConverter
        : IValueConverter
    {
        public double Multiplier { get; set; }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (double) value*Multiplier;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (double) value/Multiplier;
        }
    }
}