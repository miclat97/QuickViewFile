using System.Globalization;
using System.Windows.Controls;
using System.Windows.Data;

namespace QuickViewFile.Helpers
{
    public class ScrollBarsVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var isNull = value is null;
            return isNull ? ScrollBarVisibility.Visible : ScrollBarVisibility.Hidden;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}