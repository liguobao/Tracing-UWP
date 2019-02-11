using System;
using Windows.UI.Xaml.Data;

namespace Tracing.Core.Converters
{
    public class ImageOpacityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            double f = ((double)value) / 100;
            return f;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            double f = ((double)value) * 100;
            return f;
        }
    }
}
