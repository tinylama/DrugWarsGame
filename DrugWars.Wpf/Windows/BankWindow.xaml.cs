using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using DrugWars.Wpf.Models;
using DrugWars.Wpf.Utilities;
using System.Windows.Input;

namespace DrugWars.Wpf.Windows;

public partial class BankWindow : GameWindowBase, INotifyPropertyChanged
{
    private int _amount;
    private string _errorMessage = string.Empty;
    private bool _hasError;

    public event PropertyChangedEventHandler? PropertyChanged;

    public int Amount
    {
        get => _amount;
        set
        {
            _amount = value;
            OnPropertyChanged(nameof(Amount));
            OnPropertyChanged(nameof(CanDeposit));
            OnPropertyChanged(nameof(CanWithdraw));
        }
    }

    public bool CanDeposit =>
        Amount > 0 &&
        Amount <= GameEngine.Player.Cash;

    public bool CanWithdraw =>
        Amount > 0 &&
        Amount <= GameEngine.Player.Bank;

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

    public BankWindow()
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

    private void OnAmountChanged(object sender, TextChangedEventArgs e)
    {
        try
        {
            if (sender is TextBox textBox && int.TryParse(textBox.Text, out int value))
            {
                Amount = value;
                ErrorMessage = string.Empty;
            }
            else
            {
                Amount = 0;
                ErrorMessage = "Please enter a valid number";
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error in OnAmountChanged: {ex}");
            ErrorMessage = "Error processing amount";
        }
    }

    private void OnDepositClick(object sender, RoutedEventArgs e)
    {
        try
        {
            if (!CanDeposit)
            {
                ErrorMessage = "Invalid amount or insufficient funds";
                return;
            }

            GameEngine.Player.Cash -= Amount;
            GameEngine.Player.Bank += Amount;
            Close();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error in OnDepositClick: {ex}");
            ErrorMessage = "Error processing deposit";
        }
    }

    private void OnWithdrawClick(object sender, RoutedEventArgs e)
    {
        try
        {
            if (!CanWithdraw)
            {
                ErrorMessage = "Invalid amount or insufficient funds";
                return;
            }

            GameEngine.Player.Bank -= Amount;
            GameEngine.Player.Cash += Amount;
            Close();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error in OnWithdrawClick: {ex}");
            ErrorMessage = "Error processing withdrawal";
        }
    }

    private void OnCancelClick(object sender, RoutedEventArgs e)
    {
        Close();
    }

    private void ShowHelpDialog()
    {
        MessageBox.Show("Drug Wars\n\nRetro remake by Mark.\n\nBank window.\nF1: Help\nEsc: Close window\n", "About / Help", MessageBoxButton.OK, MessageBoxImage.Information);
    }
}