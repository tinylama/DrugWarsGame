using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows.Data;

namespace DrugWars.Wpf.Windows
{
    public class PriceHistorySparklineConverter : IValueConverter
    {
        private static readonly char[] SparkChars = new[] { '▁', '▂', '▃', '▄', '▅', '▆', '▇', '█' };
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is IEnumerable<decimal> prices)
            {
                var list = prices.ToList();
                if (list.Count == 0) return string.Empty;
                decimal min = list.Min();
                decimal max = list.Max();
                if (max == min) return new string(SparkChars[0], list.Count);
                return new string(list.Select(p => SparkChars[(int)((p - min) / (max - min) * (SparkChars.Length - 1))]).ToArray());
            }
            return string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
} 