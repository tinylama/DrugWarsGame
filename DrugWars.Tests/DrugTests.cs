using Xunit;
using DrugWars.Core.Models;
using System.Linq;
using System.Collections.Generic;
using DrugWars.Wpf.Windows;
using System;
using System.Globalization;

namespace DrugWars.Tests;

public class DrugTests
{
    [Fact]
    public void Drug_UpdateMinMaxPrices_OnPriceChange()
    {
        // Arrange
        var drug = new Drug
        {
            Name = "Test Drug",
            CurrentPrice = 100
        };

        // Act
        drug.CurrentPrice = 150;
        drug.CurrentPrice = 75;

        // Assert
        Assert.Equal(75, drug.MinPrice);
        Assert.Equal(150, drug.MaxPrice);
    }

    [Fact]
    public void Drug_PriceHistory_MaxSize()
    {
        // Arrange
        var drug = new Drug
        {
            Name = "Test Drug"
        };

        // Act - add more than 10 prices
        for (int i = 1; i <= 15; i++)
        {
            drug.CurrentPrice = i * 100;
            drug.RecordDailyPrice();
        }

        // Assert - should only have the last 10 prices
        Assert.Equal(10, drug.PriceHistory.Count());
        Assert.Equal(1500, drug.CurrentPrice);
        Assert.Equal(600, drug.PriceHistory.First());
    }

    [Fact]
    public void PriceHistorySparklineConverter_ShouldHandleNullInput()
    {
        // Arrange
        var converter = new PriceHistorySparklineConverter();

        // Act
        var result = converter.Convert(null, typeof(string), null, CultureInfo.InvariantCulture);

        // Assert
        Assert.Equal(string.Empty, result);
    }

    [Fact]
    public void PriceHistorySparklineConverter_ShouldHandleSinglePrice()
    {
        // Arrange
        var converter = new PriceHistorySparklineConverter();
        var prices = new List<decimal> { 100 };

        // Act
        var result = converter.Convert(prices, typeof(string), null, CultureInfo.InvariantCulture);

        // Assert
        Assert.Equal("NEW", result);
    }

    [Fact]
    public void PriceHistorySparklineConverter_ShouldShowStablePrices()
    {
        // Arrange
        var converter = new PriceHistorySparklineConverter();
        var prices = new List<decimal> { 100, 101, 100, 102 }; // Small changes within threshold

        // Act
        var result = converter.Convert(prices, typeof(string), null, CultureInfo.InvariantCulture) as string;

        // Assert
        Assert.NotNull(result);
        Assert.Equal("───", result); // Three stable indicators for the three transitions
    }

    [Fact]
    public void PriceHistorySparklineConverter_ShouldShowSignificantChanges()
    {
        // Arrange
        var converter = new PriceHistorySparklineConverter();
        var prices = new List<decimal> { 100, 110, 90, 100 }; // >5% changes

        // Act
        var result = converter.Convert(prices, typeof(string), null, CultureInfo.InvariantCulture) as string;

        // Assert
        Assert.NotNull(result);
        Assert.Equal("↑↓↑", result); // Up, down, up arrows for significant changes
    }

    [Fact]
    public void Drug_InitialPriceShouldSetMinMaxPrices()
    {
        // Arrange
        var drug = new Drug
        {
            Name = "Test Drug"
        };

        // Act
        drug.CurrentPrice = 100; // First price
        drug.RecordDailyPrice();

        // Assert
        Assert.Equal(100, drug.MinPrice);
        Assert.Equal(100, drug.MaxPrice);
        Assert.Single(drug.PriceHistory);
        Assert.Equal(100, drug.PriceHistory.First());
    }

    [Fact]
    public void Drug_PriceHistoryShouldRecordEveryDay()
    {
        // Arrange
        var drug = new Drug
        {
            Name = "Test Drug",
            CurrentPrice = 100
        };
        drug.RecordDailyPrice();

        // Act - Record same price for multiple days
        drug.CurrentPrice = 100; // Same price again
        drug.RecordDailyPrice();
        drug.RecordDailyPrice();

        // Assert - Should have all days recorded
        Assert.Equal(3, drug.PriceHistory.Count()); // Three days of history
        Assert.All(drug.PriceHistory, price => Assert.Equal(100, price)); // All prices should be 100
    }

    [Fact]
    public void Drug_PriceHistoryShouldNotDuplicateCurrentPrice()
    {
        // Arrange
        var drug = new Drug
        {
            Name = "Test Drug",
            CurrentPrice = 100
        };
        drug.RecordDailyPrice();

        // Act - Record same price for another day
        drug.CurrentPrice = 100; // Same price again
        drug.RecordDailyPrice();

        // Assert - Should have both days recorded
        Assert.Equal(2, drug.PriceHistory.Count()); // Two days of history
        Assert.All(drug.PriceHistory, price => Assert.Equal(100, price)); // All prices should be 100
    }

    [Fact]
    public void PriceHistorySparklineConverter_ShouldHandleExtremeChanges()
    {
        // Arrange
        var converter = new PriceHistorySparklineConverter();
        var prices = new List<decimal> { 100, 1000, 1, 100 }; // Extreme changes

        // Act
        var result = converter.Convert(prices, typeof(string), null, CultureInfo.InvariantCulture) as string;

        // Assert
        Assert.NotNull(result);
        Assert.Equal("↑↓↑", result); // Should still show directional changes
    }

    [Fact]
    public void PriceHistorySparklineConverter_ShouldShowCorrectSymbols()
    {
        // Arrange
        var converter = new PriceHistorySparklineConverter();

        // Test Case 1: Empty price history
        var emptyPrices = new List<decimal>();
        var result1 = converter.Convert(emptyPrices, typeof(string), null, CultureInfo.InvariantCulture);
        Assert.Equal(string.Empty, result1);

        // Test Case 2: Single price
        var singlePrice = new List<decimal> { 100 };
        var result2 = converter.Convert(singlePrice, typeof(string), null, CultureInfo.InvariantCulture);
        Assert.Equal("NEW", result2);

        // Test Case 3: Stable prices
        var samePrices = new List<decimal> { 100, 101, 102 }; // Small changes within threshold
        var result3 = converter.Convert(samePrices, typeof(string), null, CultureInfo.InvariantCulture);
        Assert.Equal("──", result3);

        // Test Case 4: Prices going up then down
        var mixedPrices = new List<decimal> { 100, 150, 120, 120, 80 };
        var result4 = converter.Convert(mixedPrices, typeof(string), null, CultureInfo.InvariantCulture);
        Assert.Equal("↑↓─↓", result4);
    }

    [Fact]
    public void DrugTooltipConverter_ShouldShowCorrectCurrentPrice()
    {
        // Arrange
        var converter = new DrugTooltipConverter();
        var drug = new Drug
        {
            Name = "Cocaine"
        };

        // Set some price history
        drug.CurrentPrice = 30000;
        drug.RecordDailyPrice();
        drug.CurrentPrice = 40000;
        drug.RecordDailyPrice();
        drug.CurrentPrice = 60000; // Set final price
        drug.RecordDailyPrice();

        // Act - Get the tooltip text
        var tooltipText = converter.Convert(drug, typeof(string), null, CultureInfo.InvariantCulture) as string;

        // Assert
        Assert.NotNull(tooltipText);
        Assert.Contains("Current Price: $60,000", tooltipText); // Should show current price
        Assert.Contains("Cocaine", tooltipText); // Should show drug name
        Assert.Contains("Price Range: $30,000 - $60,000", tooltipText); // Should show price range
    }
}