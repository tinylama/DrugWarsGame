using System;
using System.Globalization;
using System.Linq;
using System.Windows.Data;
using DrugWars.Core.Models;

namespace DrugWars.Wpf.Windows
{
    public class DrugTooltipConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Drug drug)
            {
                var prices = drug.PriceHistory.ToList();
                string spark = new PriceHistorySparklineConverter().Convert(prices, targetType, parameter, culture)?.ToString() ?? "";
                string tip = "";
                if (prices.Count > 2)
                {
                    var avg = prices.Average();
                    if (drug.CurrentPrice > avg * 1.3m)
                        tip = " (Tip: Price is unusually high!)";
                    else if (drug.CurrentPrice < avg * 0.7m)
                        tip = " (Tip: Price is unusually low!)";
                }
                return $"Current: ${drug.CurrentPrice:N0}\nMin: ${drug.MinPrice:N0}  Max: ${drug.MaxPrice:N0}\nHistory: {spark}\n{string.Join(", ", prices.Select(p => p.ToString("N0")))}{tip}";
            }
            return value?.ToString() ?? string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
} 