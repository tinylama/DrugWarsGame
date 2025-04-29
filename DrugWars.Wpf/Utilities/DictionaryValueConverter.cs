using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Data;

namespace DrugWars.Wpf.Utilities
{
    public class DictionaryValueConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length != 2 || values[0] == null || values[1] == null)
                return 0;

            if (values[0] is Dictionary<string, int> dictionary && values[1] is string key)
            {
                return dictionary.TryGetValue(key, out int value) ? value : 0;
            }

            return 0;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
} 