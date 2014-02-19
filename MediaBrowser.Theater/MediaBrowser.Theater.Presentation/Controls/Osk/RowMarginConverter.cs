using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace MediaBrowser.Theater.Presentation.Controls.Osk
{
    public class RowMarginConverter
        : IValueConverter
    {
        public double KeyWidth { get; set; }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return new Thickness((double) value*KeyWidth, 0, 0, 0);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}