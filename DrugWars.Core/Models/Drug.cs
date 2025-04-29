namespace DrugWars.Core.Models;
using System.ComponentModel;
using System.Collections.Generic;

public class Drug : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

    private string _name = string.Empty;
    public string Name
    {
        get => _name;
        set { if (_name != value) { _name = value; OnPropertyChanged(nameof(Name)); } }
    }

    private decimal _currentPrice;
    public decimal CurrentPrice
    {
        get => _currentPrice;
        set
        {
            if (_currentPrice != value)
            {
                _currentPrice = value;
                AddPriceToHistory(value);
                OnPropertyChanged(nameof(CurrentPrice));
                OnPropertyChanged(nameof(PriceHistory));
            }
        }
    }

    private decimal _minPrice;
    public decimal MinPrice
    {
        get => _minPrice;
        set { if (_minPrice != value) { _minPrice = value; OnPropertyChanged(nameof(MinPrice)); } }
    }

    private decimal _maxPrice;
    public decimal MaxPrice
    {
        get => _maxPrice;
        set { if (_maxPrice != value) { _maxPrice = value; OnPropertyChanged(nameof(MaxPrice)); } }
    }

    private readonly Queue<decimal> _priceHistory = new Queue<decimal>();
    public IEnumerable<decimal> PriceHistory => _priceHistory;
    private void AddPriceToHistory(decimal price)
    {
        if (_priceHistory.Count >= 10)
            _priceHistory.Dequeue();
        _priceHistory.Enqueue(price);
    }

    protected void OnPropertyChanged(string propertyName)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
} 