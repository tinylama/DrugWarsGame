using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows.Data;
using System.Diagnostics;
using DrugWars.Core.Models;

namespace DrugWars.Wpf.Windows
{
    public class DrugTooltipConverter : IValueConverter
    {
        private readonly PriceToDisplayConverter _priceConverter = new PriceToDisplayConverter();
        private readonly PriceHistorySparklineConverter _sparklineConverter = new PriceHistorySparklineConverter();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is Drug drug))
            {
                Debug.WriteLine("[TOOLTIP] Input is not a Drug object");
                return string.Empty;
            }

            Debug.WriteLine($"[TOOLTIP] Creating tooltip for {drug.Name}");
            Debug.WriteLine($"[TOOLTIP] Current price: {drug.CurrentPrice}");
            Debug.WriteLine($"[TOOLTIP] Price history count: {drug.PriceHistory.Count()}");
            Debug.WriteLine($"[TOOLTIP] Min/Max prices: {drug.MinPrice}/{drug.MaxPrice}");

            var tooltip = new System.Text.StringBuilder();

            // Drug name and current price
            tooltip.AppendLine($"{drug.Name}");
            string currentPriceStr = _priceConverter.Convert(drug.CurrentPrice, typeof(string), null, culture).ToString();
            tooltip.AppendLine($"Current Price: {currentPriceStr}");
            Debug.WriteLine($"[TOOLTIP] Added current price: {currentPriceStr}");

            // Price trend visualization
            if (drug.PriceHistory.Any())
            {
                tooltip.AppendLine();
                tooltip.AppendLine("Price Trend:");
                string sparkline = _sparklineConverter.Convert(drug.PriceHistory, typeof(string), null, culture).ToString();
                tooltip.AppendLine(sparkline);
                Debug.WriteLine($"[TOOLTIP] Added sparkline: {sparkline}");
            }

            // Price range information
            tooltip.AppendLine();
            var priceHistoryCount = drug.PriceHistory.Count();
            if (priceHistoryCount <= 1)
            {
                string initialPrice = _priceConverter.Convert(drug.CurrentPrice, typeof(string), null, culture).ToString();
                tooltip.AppendLine($"Initial Price: {initialPrice}");
                Debug.WriteLine($"[TOOLTIP] Added initial price: {initialPrice}");
            }
            else
            {
                string minPrice = _priceConverter.Convert(drug.MinPrice, typeof(string), null, culture).ToString();
                string maxPrice = _priceConverter.Convert(drug.MaxPrice, typeof(string), null, culture).ToString();
                tooltip.AppendLine($"Price Range: {minPrice} - {maxPrice}");
                Debug.WriteLine($"[TOOLTIP] Added price range: {minPrice} - {maxPrice}");
            }

            // Recent prices (last 5)
            if (priceHistoryCount > 1)
            {
                tooltip.AppendLine();
                tooltip.AppendLine("Recent Prices:");
                var recentPrices = drug.PriceHistory.TakeLast(5).ToList();
                Debug.WriteLine($"[TOOLTIP] Adding {recentPrices.Count} recent prices");
                
                for (int i = 0; i < recentPrices.Count; i++)
                {
                    int dayOffset = priceHistoryCount - recentPrices.Count + i + 1;
                    string priceStr = _priceConverter.Convert(recentPrices[i], typeof(string), null, culture).ToString();
                    tooltip.AppendLine($"Day {dayOffset}: {priceStr}");
                    Debug.WriteLine($"[TOOLTIP] Added day {dayOffset} price: {priceStr}");
                }
            }

            // Market analysis
            if (priceHistoryCount > 1)
            {
                tooltip.AppendLine();
                tooltip.AppendLine("Market Analysis:");
                decimal avgPrice = drug.PriceHistory.Average();
                decimal currentPrice = drug.CurrentPrice;
                decimal priceRatio = currentPrice / avgPrice;

                Debug.WriteLine($"[TOOLTIP] Market analysis - Avg price: {avgPrice}, Current: {currentPrice}, Ratio: {priceRatio:F2}");

                string analysis;
                if (priceRatio < 0.8m)
                    analysis = "Great time to buy! Price is well below average.";
                else if (priceRatio < 0.95m)
                    analysis = "Good time to buy. Price is below average.";
                else if (priceRatio > 1.2m)
                    analysis = "Great time to sell! Price is well above average.";
                else if (priceRatio > 1.05m)
                    analysis = "Good time to sell. Price is above average.";
                else
                    analysis = "Price is close to average. Hold or trade based on trends.";

                tooltip.AppendLine(analysis);
                Debug.WriteLine($"[TOOLTIP] Added market analysis: {analysis}");
            }

            string result = tooltip.ToString();
            Debug.WriteLine($"[TOOLTIP] Final tooltip:\n{result}");
            return result;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
} 