using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using DrugWars.Core.Models;
using DrugWars.Wpf.Utilities;
using System.Windows.Input;
using System.Linq;

namespace DrugWars.Wpf.Windows;

public partial class TravelWindow : GameWindowBase, INotifyPropertyChanged
{
    private Location? _selectedLocation;
    private string _errorMessage = string.Empty;
    private bool _hasError;

    public event PropertyChangedEventHandler? PropertyChanged;

    public Location? SelectedLocation
    {
        get => _selectedLocation;
        set
        {
            _selectedLocation = value;
            OnPropertyChanged(nameof(SelectedLocation));
            OnPropertyChanged(nameof(CanTravel));
        }
    }

    public bool CanTravel => 
        SelectedLocation != null && 
        GameEngine.Player.Cash >= SelectedLocation.TravelCost;

    public string ErrorMessage
    {
        get => _errorMessage;
        set
        {
            _errorMessage = value;
            HasError = !string.IsNullOrEmpty(value);
            OnPropertyChanged(nameof(ErrorMessage));
        }
    }

    public bool HasError
    {
        get => _hasError;
        set
        {
            _hasError = value;
            OnPropertyChanged(nameof(HasError));
        }
    }

    public int Cash => (int)(GameEngine?.Player?.Cash ?? 0);
    public List<Location> Locations =>
        GameEngine == null ? new List<Location>() :
        GameEngine.Locations.Where((loc, idx) => idx != GameEngine.Player.CurrentLocation).ToList();

    public TravelWindow()
    {
        InitializeComponent();
        DataContext = this;
        // Keyboard shortcuts
        InputBindings.Add(new KeyBinding(new RelayCommand(_ => Close()), new KeyGesture(Key.Escape)));
        InputBindings.Add(new KeyBinding(new RelayCommand(_ => ShowHelpDialog()), new KeyGesture(Key.F1)));
    }

    protected virtual void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    private void OnLocationSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        try
        {
            if (sender is ListBox listBox && listBox.SelectedItem is Location location)
            {
                SelectedLocation = location;
                ErrorMessage = string.Empty;
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error in OnLocationSelectionChanged: {ex}");
            ErrorMessage = "Error selecting location";
        }
    }

    private void OnTravelClick(object sender, RoutedEventArgs e)
    {
        try
        {
            if (SelectedLocation == null || !CanTravel)
            {
                ErrorMessage = "Invalid selection or insufficient funds";
                return;
            }

            GameEngine.Player.Cash -= SelectedLocation.TravelCost;
            GameEngine.Player.CurrentLocation = GameEngine.Locations.IndexOf(SelectedLocation);
            DialogResult = true;
            Close();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error in OnTravelClick: {ex}");
            ErrorMessage = "Error processing travel";
        }
    }

    private void OnCancelClick(object sender, RoutedEventArgs e)
    {
        Close();
    }

    private void ShowHelpDialog()
    {
        MessageBox.Show("Drug Wars\n\nRetro remake by Mark.\n\nTravel window.\nF1: Help\nEsc: Close window\n", "About / Help", MessageBoxButton.OK, MessageBoxImage.Information);
    }
} 