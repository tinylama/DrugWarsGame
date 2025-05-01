using System.Globalization;
using System.Windows.Data;

namespace DrugWars.Wpf.Converters
{
    public class PriceChangeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is decimal change)
            {
                if (change == 0) return "";
                string symbol = change > 0 ? "↑" : "↓";
                return $"{symbol} {Math.Abs(change):C}";
            }
            return "";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}