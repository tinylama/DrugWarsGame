using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Linq;
using DrugWars.Core.Models;
using DrugWars.Wpf.Utilities;

namespace DrugWars.Wpf.Windows
{
    /// <summary>
    /// Interaction logic for BuyWindow.xaml
    /// </summary>
    public partial class BuyWindow : GameWindowBase, INotifyPropertyChanged
    {
        private int _quantity;
        private string _errorMessage = string.Empty;
        private bool _hasError;
        private Drug? _selectedDrug;

        public event PropertyChangedEventHandler? PropertyChanged;

        public BuyWindow()
        {
            InitializeComponent();
            DataContext = this;
            // Set ComboBox default selection to first drug after DataContext is set
            Loaded += (s, e) =>
            {
                var combo = this.FindName("DrugComboBox") as ComboBox;
                if (combo != null && combo.Items.Count > 0)
                    combo.SelectedIndex = 0;
            };
            // Keyboard shortcuts
            InputBindings.Add(new KeyBinding(new RelayCommand(_ => Close()), new KeyGesture(Key.Escape)));
            InputBindings.Add(new KeyBinding(new RelayCommand(_ => ShowHelpDialog()), new KeyGesture(Key.F1)));
        }

        public IEnumerable<Drug> Drugs => GameEngine.Drugs;

        public Drug? SelectedDrug
        {
            get => _selectedDrug;
            set
            {
                if (_selectedDrug != value)
                {
                    _selectedDrug = value;
                    OnPropertyChanged(nameof(SelectedDrug));
                    OnPropertyChanged(nameof(RoundedPrice));
                    OnPropertyChanged(nameof(MaxBuyQuantity));
                    OnPropertyChanged(nameof(TotalCost));
                    OnPropertyChanged(nameof(CanBuy));
                }
            }
        }

        public int Quantity
        {
            get => _quantity;
            set
            {
                if (_quantity != value)
                {
                    _quantity = value;
                    OnPropertyChanged(nameof(Quantity));
                    OnPropertyChanged(nameof(TotalCost));
                    OnPropertyChanged(nameof(CanBuy));
                }
            }
        }

        public int MaxBuyQuantity
        {
            get
            {
                var price = RoundedPrice;
                if (SelectedDrug == null || price <= 0) return 0;
                return (int)(GameEngine.Player.Cash / price);
            }
        }

        public decimal RoundedPrice => SelectedDrug == null ? 0 : Math.Max(10, Math.Round(SelectedDrug.CurrentPrice / 10m, 0) * 10);

        public decimal TotalCost => RoundedPrice * Quantity;

        public bool CanBuy => SelectedDrug != null && Quantity > 0 && TotalCost <= GameEngine.Player.Cash && RoundedPrice > 0;

        public string ErrorMessage
        {
            get => _errorMessage;
            private set
            {
                if (_errorMessage != value)
                {
                    _errorMessage = value;
                    OnPropertyChanged(nameof(ErrorMessage));
                }
            }
        }

        public bool HasError
        {
            get => _hasError;
            private set
            {
                if (_hasError != value)
                {
                    _hasError = value;
                    OnPropertyChanged(nameof(HasError));
                }
            }
        }

        private void OnBuyClick(object sender, RoutedEventArgs e)
        {
            try
            {
                if (SelectedDrug == null)
                {
                    ErrorMessage = "Please select a drug to buy.";
                    HasError = true;
                    return;
                }

                if (Quantity <= 0)
                {
                    ErrorMessage = "Please enter a valid quantity.";
                    HasError = true;
                    return;
                }

                if (TotalCost > GameEngine.Player.Cash)
                {
                    ErrorMessage = "You don't have enough cash.";
                    HasError = true;
                    return;
                }

                GameEngine.BuyDrug(SelectedDrug, Quantity);
                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;
                HasError = true;
            }
        }

        private void OnCancelClick(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void OnDrugSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (sender is ComboBox comboBox && comboBox.SelectedItem is Drug drug)
                {
                    SelectedDrug = drug;
                    ErrorMessage = string.Empty;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in OnDrugSelectionChanged: {ex}");
                ErrorMessage = "Error selecting drug";
            }
        }

        private void OnQuantityChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                if (sender is TextBox textBox && int.TryParse(textBox.Text, out int value))
                {
                    Quantity = value;
                    ErrorMessage = string.Empty;
                }
                else
                {
                    Quantity = 0;
                    ErrorMessage = "Please enter a valid number";
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in OnQuantityChanged: {ex}");
                ErrorMessage = "Error processing quantity";
            }
        }

        private void ShowHelpDialog()
        {
            MessageBox.Show("Drug Wars\n\nRetro remake by Mark.\n\nBuy window.\nF1: Help\nEsc: Close window\n", "About / Help", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
} 