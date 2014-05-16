// http://social.msdn.microsoft.com/Forums/vstudio/en-US/d3bc0cca-7b1f-436c-8f43-631c335559cd/textbox-caret-not-visible-when-scaled-down

using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace MediaBrowser.Theater.Presentation.Controls.Osk
{
    public class RecipricalConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return 1.0 / (double)value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return 1.0 / (double)value;
        }
    }

    public class StInverseConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var stSource = value as ScaleTransform;

            if (stSource == null) return null;
            Console.Write("ST:");
            Console.WriteLine(1/stSource.ScaleY);
            return new ScaleTransform(1/stSource.ScaleX, 1/stSource.ScaleY);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return new NotImplementedException();
        }
    }

    public class FontScaleConverter : IValueConverter
    {
        public double DesiredFontSize { get; set; }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return DesiredFontSize*(double) value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return new NotImplementedException();
        }
    }
}