namespace DrugWars.Core.Models;
using System.ComponentModel;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;

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
            if (_currentPrice == value) return;

            Debug.WriteLine($"[PRICE] Drug {_name}: Setting price from {_currentPrice} to {value}");

            _currentPrice = value;
            OnPropertyChanged(nameof(CurrentPrice));

            // Update min/max prices when price changes
            if (value < MinPrice || MinPrice == 0)
            {
                MinPrice = value;
            }
            if (value > MaxPrice)
            {
                MaxPrice = value;
            }
        }
    }

    private decimal _minPrice;
    public decimal MinPrice
    {
        get => _minPrice;
        set
        {
            if (_minPrice != value)
            {
                _minPrice = value;
                OnPropertyChanged(nameof(MinPrice));
            }
        }
    }

    private decimal _maxPrice;
    public decimal MaxPrice
    {
        get => _maxPrice;
        set
        {
            if (_maxPrice != value)
            {
                _maxPrice = value;
                OnPropertyChanged(nameof(MaxPrice));
            }
        }
    }

    private readonly List<decimal> _priceHistory = new List<decimal>();
    private int _currentDay = 0;
    private const int MAX_HISTORY_SIZE = 10;

    /// <summary>
    /// Gets the price history as a list with most recent prices at the end
    /// </summary>
    public IEnumerable<decimal> PriceHistory => _priceHistory.ToList();

    /// <summary>
    /// Default constructor that ensures price history is initialized correctly
    /// </summary>
    public Drug()
    {
        // Empty constructor - initialization will happen when properties are set
    }

    /// <summary>
    /// Called after all properties are set to ensure price history contains at least the initial price
    /// </summary>
    public void InitializeHistory()
    {
        Debug.WriteLine($"[INIT] Drug {_name}: InitializeHistory called, CurrentPrice={_currentPrice}");

        // Clear any existing history and add the initial price
        _priceHistory.Clear();
        _priceHistory.Add(_currentPrice);
        _currentDay = 1;

        // Set initial min/max prices
        MinPrice = _currentPrice;
        MaxPrice = _currentPrice;

        Debug.WriteLine($"[INIT] Added initial price {_currentPrice} to history");
        OnPropertyChanged(nameof(PriceHistory));
    }

    /// <summary>
    /// Records the current price for the day in history
    /// </summary>
    public void RecordDailyPrice()
    {
        // Initialize if this is the first recording
        if (_currentDay == 0)
        {
            Debug.WriteLine($"[DAILY] Drug {_name}: First recording, initializing history");
            _currentDay = 1;
            _priceHistory.Clear();
            _priceHistory.Add(_currentPrice);
            MinPrice = _currentPrice;
            MaxPrice = _currentPrice;
            OnPropertyChanged(nameof(PriceHistory));
            return;
        }

        _currentDay++;

        Debug.WriteLine($"[DAILY] Drug {_name}: Recording day {_currentDay} price {_currentPrice}");
        Debug.WriteLine($"[DAILY] Current history: {string.Join(", ", _priceHistory)}");

        // Add the current price to history
        _priceHistory.Add(_currentPrice);

        // Remove oldest price if we exceed the maximum size
        if (_priceHistory.Count > MAX_HISTORY_SIZE)
        {
            _priceHistory.RemoveAt(0);
            Debug.WriteLine($"[DAILY] Removed oldest price to maintain max size of {MAX_HISTORY_SIZE}");
        }

        Debug.WriteLine($"[DAILY] Updated history: {string.Join(", ", _priceHistory)}");
        OnPropertyChanged(nameof(PriceHistory));
    }

    /// <summary>
    /// Notifies subscribers that a property has changed
    /// </summary>
    protected void OnPropertyChanged(string propertyName)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}