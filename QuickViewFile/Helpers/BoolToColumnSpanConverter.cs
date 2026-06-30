using System;
using System.Globalization;
using System.Windows.Data;

namespace QuickViewFile.Helpers
{
    public class BoolToColumnSpanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isThumbnailMode && isThumbnailMode)
            {
                return 3;
            }
            return 1;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
