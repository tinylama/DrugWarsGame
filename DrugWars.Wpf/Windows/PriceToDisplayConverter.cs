using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows;

namespace DrugWars.Wpf.Windows
{
    public class PriceToDisplayConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // Basic currency formatting with $ sign and thousand separators
            string formattedValue = "";

            if (value is decimal dec)
            {
                formattedValue = "$" + dec.ToString("N0");
            }
            else if (value is int i)
            {
                formattedValue = "$" + i.ToString("N0");
            }
            else
            {
                formattedValue = value?.ToString() ?? string.Empty;
            }

            return formattedValue;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}