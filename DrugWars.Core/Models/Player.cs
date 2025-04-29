namespace DrugWars.Core.Models;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Linq;

public class Player : INotifyPropertyChanged
{
    private decimal _cash;
    public decimal Cash
    {
        get => _cash;
        set { if (_cash != value) { _cash = value; OnPropertyChanged(); } }
    }

    private decimal _bank;
    public decimal Bank
    {
        get => _bank;
        set { if (_bank != value) { _bank = value; OnPropertyChanged(); } }
    }

    private decimal _debt;
    public decimal Debt
    {
        get => _debt;
        set { if (_debt != value) { _debt = value; OnPropertyChanged(); } }
    }

    private int _health = 100;
    public int Health
    {
        get => _health;
        set { if (_health != value) { _health = value; OnPropertyChanged(); } }
    }

    private int _day = 1;
    public int Day
    {
        get => _day;
        set { if (_day != value) { _day = value; OnPropertyChanged(); } }
    }

    private int _currentLocation;
    public int CurrentLocation
    {
        get => _currentLocation;
        set { if (_currentLocation != value) { _currentLocation = value; OnPropertyChanged(); } }
    }

    public Dictionary<string, int> Inventory { get; } = new();

    private int _maxInventory = 100;
    public int MaxInventory
    {
        get => _maxInventory;
        set { if (_maxInventory != value) { _maxInventory = value; OnPropertyChanged(); } }
    }

    public int CurrentInventoryTotal => Inventory.Values.Sum();

    public int GetDrugQuantity(string drugName)
    {
        return Inventory.TryGetValue(drugName, out int quantity) ? quantity : 0;
    }

    public bool HasReceivedBailout { get; set; } = false;

    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

    public void CleanupInventory()
    {
        var zeroKeys = Inventory.Where(kvp => kvp.Value <= 0).Select(kvp => kvp.Key).ToList();
        foreach (var key in zeroKeys)
            Inventory.Remove(key);
        OnPropertyChanged(nameof(Inventory));
        OnPropertyChanged(nameof(CurrentInventoryTotal));
    }
} 