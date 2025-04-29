using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows.Data;
using System.Diagnostics;

namespace DrugWars.Wpf.Windows
{
    /// <summary>
    /// Converts a collection of price history values into a visual sparkline representation
    /// using simple symbols: ─ for stable, ↑ for increase, ↓ for decrease
    /// </summary>
    public class PriceHistorySparklineConverter : IValueConverter
    {
        private const string STABLE = "─";   // No significant change
        private const string UP = "↑";       // Price increase
        private const string DOWN = "↓";     // Price decrease
        private const decimal SIGNIFICANT_CHANGE = 0.05m; // 5% change threshold

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is IEnumerable<decimal> prices))
            {
                Debug.WriteLine("[SPARKLINE] Input is not a decimal collection");
                return string.Empty;
            }

            var priceList = prices.ToList();
            Debug.WriteLine($"[SPARKLINE] Processing price history with {priceList.Count} prices");
            Debug.WriteLine($"[SPARKLINE] Price values: {string.Join(", ", priceList)}");

            if (priceList.Count == 0)
            {
                Debug.WriteLine("[SPARKLINE] Empty price history");
                return string.Empty;
            }

            if (priceList.Count == 1)
            {
                Debug.WriteLine("[SPARKLINE] Single price - showing NEW");
                return "NEW";
            }

            var sparkline = new System.Text.StringBuilder();

            // Build the sparkline comparing each pair of consecutive prices
            for (int i = 0; i < priceList.Count - 1; i++)
            {
                decimal currentPrice = priceList[i];
                decimal nextPrice = priceList[i + 1];
                decimal percentChange = (nextPrice - currentPrice) / currentPrice;

                Debug.WriteLine($"[SPARKLINE] Comparing prices {currentPrice} -> {nextPrice} ({percentChange:P1} change)");

                if (percentChange > SIGNIFICANT_CHANGE)
                {
                    sparkline.Append(UP);
                    Debug.WriteLine("[SPARKLINE] Added up arrow ↑");
                }
                else if (percentChange < -SIGNIFICANT_CHANGE)
                {
                    sparkline.Append(DOWN);
                    Debug.WriteLine("[SPARKLINE] Added down arrow ↓");
                }
                else
                {
                    sparkline.Append(STABLE);
                    Debug.WriteLine("[SPARKLINE] Added stable dash ─");
                }
            }

            string result = sparkline.ToString();
            Debug.WriteLine($"[SPARKLINE] Final sparkline: {result}");
            return result;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
} 