using System;
using System.Globalization;
using System.Windows.Data;

namespace DrugWars.Wpf.Windows
{
    public class PriceToDisplayConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is decimal dec)
            {
                return "$" + dec.ToString("N0");
            }
            if (value is int i)
            {
                return "$" + i.ToString("N0");
            }
            return value?.ToString() ?? string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
} 