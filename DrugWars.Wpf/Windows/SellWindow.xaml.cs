using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using DrugWars.Wpf.Models;
using DrugWars.Core.Models;
using DrugWars.Wpf.Utilities;

namespace DrugWars.Wpf.Windows
{
    public partial class SellWindow : GameWindowBase, INotifyPropertyChanged
    {
        private int _quantity;
        private string? _selectedDrug;
        private string _errorMessage = string.Empty;
        private bool _hasError;
        private List<InventoryItem> _inventoryItems = new();

        public event PropertyChangedEventHandler? PropertyChanged;

        public int Quantity
        {
            get => _quantity;
            set
            {
                _quantity = value;
                OnPropertyChanged(nameof(Quantity));
                OnPropertyChanged(nameof(TotalValue));
                OnPropertyChanged(nameof(CanSell));
            }
        }

        public string? SelectedDrug
        {
            get => _selectedDrug;
            set
            {
                _selectedDrug = value;
                OnPropertyChanged(nameof(SelectedDrug));
                OnPropertyChanged(nameof(TotalValue));
                OnPropertyChanged(nameof(CanSell));
                OnPropertyChanged(nameof(MaxSellQuantity));
            }
        }

        public decimal TotalValue => 
            SelectedDrug != null && GameEngine.Drugs.FirstOrDefault(d => d.Name == SelectedDrug) is Drug drug
                ? drug.CurrentPrice * Quantity
                : 0;

        public bool CanSell =>
            SelectedDrug != null &&
            Quantity > 0 &&
            GameEngine.Player.Inventory.TryGetValue(SelectedDrug, out int owned) &&
            Quantity <= owned;

        public bool HasNoInventory => _inventoryItems.Count == 0;

        public List<InventoryItem> InventoryItems
        {
            get => _inventoryItems;
            private set
            {
                _inventoryItems = value;
                OnPropertyChanged(nameof(InventoryItems));
                OnPropertyChanged(nameof(HasNoInventory));
            }
        }

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

        public int MaxSellQuantity =>
            SelectedDrug != null && GameEngine.Player.Inventory.TryGetValue(SelectedDrug, out int owned)
                ? owned
                : 0;

        public SellWindow()
        {
            try
            {
                Debug.WriteLine($"=== Initializing SellWindow ===");
                Debug.WriteLine($"Time: {DateTime.Now}");
                InitializeComponent();
                DataContext = this;
                IconHelper.SetWindowIcon(this);
                
                // Defer loading inventory until the GameEngine property is set
                Loaded += (s, e) => 
                {
                    try 
                    {
                        LoadInventory();
                        
                        // Auto-select first item if available
                        var drugListBox = this.FindName("DrugListBox") as ListBox;
                        if (drugListBox != null && drugListBox.Items.Count > 0 && SelectedDrug == null)
                        {
                            drugListBox.SelectedIndex = 0;
                            // Set quantity to all available for the selected drug
                            if (SelectedDrug != null && GameEngine.Player.Inventory.TryGetValue(SelectedDrug, out int owned))
                            {
                                Quantity = owned;
                            }
                        }
                        
                        // If no inventory, show message and close
                        if (GameEngine.Player.Inventory.Count == 0)
                        {
                            if (Owner is MainWindow mainWindow)
                            {
                                mainWindow.SetStatusMessage("You do not have any drugs to sell.");
                            }
                            Close();
                            return;
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"Error in Loaded event: {ex.Message}");
                        Debug.WriteLine($"Stack Trace: {ex.StackTrace}");
                        if (Owner is MainWindow mainWindow)
                        {
                            mainWindow.SetStatusMessage("Error opening sell window: " + ex.Message);
                        }
                        Close();
                    }
                };
                
                // Keyboard shortcuts
                InputBindings.Add(new KeyBinding(new RelayCommand(_ => Close()), new KeyGesture(Key.Escape)));
                InputBindings.Add(new KeyBinding(new RelayCommand(_ => ShowHelpDialog()), new KeyGesture(Key.F1)));
                Debug.WriteLine("SellWindow initialized successfully");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error initializing SellWindow: {ex.Message}");
                Debug.WriteLine($"Stack Trace: {ex.StackTrace}");
                throw;
            }
        }

        private void LoadInventory()
        {
            try
            {
                Debug.WriteLine("Loading inventory");
                var items = new List<InventoryItem>();
                foreach (var drug in GameEngine.Drugs)
                {
                    var quantity = GameEngine.Player.GetDrugQuantity(drug.Name);
                    if (quantity > 0)
                    {
                        items.Add(new InventoryItem
                        {
                            DrugName = drug.Name,
                            Quantity = quantity,
                            ValuePerUnit = (int)drug.CurrentPrice
                        });
                        Debug.WriteLine($"Added {drug.Name} to inventory - Quantity: {quantity}, Value: ${drug.CurrentPrice:N0}/unit");
                    }
                }
                InventoryItems = items;
                Debug.WriteLine($"Total inventory items: {items.Count}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error loading inventory: {ex.Message}");
                Debug.WriteLine($"Stack Trace: {ex.StackTrace}");
                ErrorMessage = "Error loading inventory";
            }
        }

        private void OnQuantityChanged(object sender, TextChangedEventArgs e)
        {
            if (sender is TextBox textBox && int.TryParse(textBox.Text, out int quantity))
            {
                Quantity = quantity;
                ErrorMessage = string.Empty;
            }
            else
            {
                ErrorMessage = "Please enter a valid quantity";
            }
        }

        private void OnSellClick(object sender, RoutedEventArgs e)
        {
            try
            {
                if (SelectedDrug == null || !CanSell)
                {
                    ErrorMessage = "Invalid selection or quantity";
                    return;
                }

                GameEngine.Player.Cash = GameEngine.Player.Cash + (int)TotalValue;
                GameEngine.Player.Inventory[SelectedDrug] -= Quantity;
                
                if (GameEngine.Player.Inventory[SelectedDrug] == 0)
                    GameEngine.Player.Inventory.Remove(SelectedDrug);
                
                // Show status message in main window
                if (Owner is MainWindow mainWindow)
                {
                    mainWindow.SetStatusMessage($"Sold {Quantity} {SelectedDrug} for ${TotalValue:N0}");
                }
                Close();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in OnSellClick: {ex}");
                ErrorMessage = "Error processing sale";
            }
        }

        private void OnCancelClick(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                DragMove();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in Window_MouseLeftButtonDown: {ex.Message}");
                Debug.WriteLine($"Stack trace: {ex.StackTrace}");
            }
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void OnDrugSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (sender is ListBox listBox && listBox.SelectedItem is string drugName)
                {
                    SelectedDrug = drugName;
                    ErrorMessage = string.Empty;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in OnDrugSelectionChanged: {ex}");
                ErrorMessage = "Error selecting drug";
            }
        }

        private void ShowHelpDialog()
        {
            MessageBox.Show("Drug Wars\n\nRetro remake by Mark.\n\nSell window.\nF1: Help\nEsc: Close window\n", "About / Help", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }

    public class InventoryItem
    {
        public required string DrugName { get; init; }
        public required int Quantity { get; init; }
        public required int ValuePerUnit { get; init; }
    }
} 