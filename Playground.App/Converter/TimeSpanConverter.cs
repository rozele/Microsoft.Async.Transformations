using System;
using System.Globalization;
using Windows.UI.Xaml.Data;

namespace Playground.App.Converter
{
    class TimeSpanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var t = value as TimeSpan?;
            return t.HasValue 
                ? t.Value.ToString("c", CultureInfo.InvariantCulture) 
                : null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            var s = value as string;
            return s != null 
                ? (TimeSpan?)TimeSpan.Parse(s) 
                : null;
        }
    }
}
