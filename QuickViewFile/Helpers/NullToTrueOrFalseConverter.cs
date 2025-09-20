using System.Globalization;
using System.Windows.Data;

namespace QuickViewFile.Helpers
{
    public class NullToTrueOrFalseConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var isNull = value is null;
            return isNull ? "True" : "False";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}