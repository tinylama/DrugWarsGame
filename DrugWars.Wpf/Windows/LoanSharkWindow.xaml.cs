using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Controls;
using DrugWars.Wpf.Models;
using DrugWars.Wpf.Utilities;
using DrugWars.Core.Models;
using System.Diagnostics;

namespace DrugWars.Wpf.Windows
{
    public partial class LoanSharkWindow : Window, INotifyPropertyChanged
    {
        public GameEngine GameEngine { get; }
        private string _borrowAmount = "";
        private string _repayAmount = "";
        private string _errorMessage = "";
        private bool _hasError;
        private bool _isBorrowing = false;
        private TextBox? _amountTextBox;
        private decimal? _maxLoanAmount;

        public decimal AvailableToBorrow => Math.Max(0, MaxLoanAmount - GameEngine.Player.Debt);

        public string BorrowAmount
        {
            get => _borrowAmount;
            set
            {
                if (_borrowAmount != value)
                {
                    _borrowAmount = value;
                    ValidateInput();
                    OnPropertyChanged();
                }
            }
        }

        public string RepayAmount
        {
            get => _repayAmount;
            set
            {
                if (_repayAmount != value)
                {
                    _repayAmount = value;
                    ValidateInput();
                    OnPropertyChanged();
                }
            }
        }

        public string ErrorMessage
        {
            get => _errorMessage;
            set
            {
                if (_errorMessage != value)
                {
                    _errorMessage = value;
                    HasError = !string.IsNullOrEmpty(value);
                    OnPropertyChanged();
                }
            }
        }

        public bool HasError
        {
            get => _hasError;
            set
            {
                if (_hasError != value)
                {
                    _hasError = value;
                    OnPropertyChanged();
                }
            }
        }

        public decimal MaxLoanAmount
        {
            get
            {
                if (!_maxLoanAmount.HasValue)
                {
                    _maxLoanAmount = GameEngine.GetMaxLoanAmount();
                }
                return _maxLoanAmount.Value;
            }
        }

        public decimal CurrentInterestRate => GameEngine.GetCurrentInterestRate();

        public bool IsBorrowing
        {
            get => _isBorrowing;
            set
            {
                _isBorrowing = value;
                OnPropertyChanged(nameof(IsBorrowing));
                OnPropertyChanged(nameof(IsPaying));
                OnPropertyChanged(nameof(ActionButtonText));
                OnPropertyChanged(nameof(AmountLabelText));
            }
        }
        
        public bool IsPaying => !_isBorrowing;
        
        public string ActionButtonText => IsBorrowing ? "Borrow" : "Pay Debt";
        
        public string AmountLabelText => IsBorrowing ? "Amount to Borrow: $" : "Amount to Pay: $";

        public int Cash => (int)GameEngine.Player.Cash;
        public int Debt => (int)GameEngine.Player.Debt;

        public bool CanRepayAll => GameEngine.Player.Cash > 0 && GameEngine.Player.Debt > 0 && GameEngine.Player.Cash >= GameEngine.Player.Debt;

        public LoanSharkWindow(GameEngine gameEngine)
        {
            GameEngine = gameEngine ?? throw new ArgumentNullException(nameof(gameEngine));
            InitializeComponent();
            DataContext = this;
            _amountTextBox = FindName("AmountTextBox") as TextBox;
            // Keyboard shortcuts
            InputBindings.Add(new KeyBinding(new RelayCommand(_ => Close()), new KeyGesture(Key.Escape)));
            InputBindings.Add(new KeyBinding(new RelayCommand(_ => ShowHelpDialog()), new KeyGesture(Key.F1)));
        }

        private void ValidateInput()
        {
            ErrorMessage = "";

            if (!string.IsNullOrEmpty(BorrowAmount))
            {
                if (!decimal.TryParse(BorrowAmount, out decimal amount))
                {
                    ErrorMessage = "Please enter a valid amount";
                    return;
                }

                if (amount <= 0)
                {
                    ErrorMessage = "Amount must be greater than 0";
                    return;
                }

                if (amount > AvailableToBorrow)
                {
                    if (AvailableToBorrow <= 0)
                    {
                        ErrorMessage = "You have reached your maximum debt limit";
                    }
                    else
                    {
                        ErrorMessage = $"You can only borrow up to ${AvailableToBorrow:N0}";
                    }
                    return;
                }
            }

            if (!string.IsNullOrEmpty(RepayAmount))
            {
                if (!decimal.TryParse(RepayAmount, out decimal amount))
                {
                    ErrorMessage = "Please enter a valid amount";
                    return;
                }

                if (amount <= 0)
                {
                    ErrorMessage = "Amount must be greater than 0";
                    return;
                }

                if (amount > GameEngine.Player.Cash)
                {
                    ErrorMessage = "You don't have enough cash";
                    return;
                }

                if (amount > GameEngine.Player.Debt)
                {
                    ErrorMessage = "Amount exceeds your debt";
                    return;
                }
            }
        }

        private void OnBorrowClick(object sender, RoutedEventArgs e)
        {
            if (!decimal.TryParse(BorrowAmount, out decimal amount) || HasError)
                return;

            if (amount > AvailableToBorrow)
            {
                if (AvailableToBorrow <= 0)
                {
                    ErrorMessage = "You have reached your maximum debt limit";
                }
                else
                {
                    ErrorMessage = $"You can only borrow up to ${AvailableToBorrow:N0}";
                }
                return;
            }

            GameEngine.Player.Cash += amount;
            GameEngine.Player.Debt += amount;
            
            DialogResult = true;
            Close();
        }

        private void OnRepayClick(object sender, RoutedEventArgs e)
        {
            if (!decimal.TryParse(RepayAmount, out decimal amount) || HasError)
                return;

            GameEngine.Player.Cash -= amount;
            GameEngine.Player.Debt -= amount;
            
            DialogResult = true;
            Close();
        }

        private void OnRepayAllClick(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!CanRepayAll)
                {
                    if (GameEngine.Player.Cash < GameEngine.Player.Debt)
                    {
                        ErrorMessage = "You don't have enough cash to repay all your debt";
                    }
                    return;
                }

                decimal amount = GameEngine.Player.Debt;
                GameEngine.Player.Cash -= amount;
                GameEngine.Player.Debt = 0;
                
                if (Owner is MainWindow mainWindow)
                {
                    mainWindow.SetStatusMessage($"Paid off all your debt: ${amount:N0}");
                }
                
                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error: {ex.Message}";
            }
        }

        private void OnCancelClick(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void NumberValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !decimal.TryParse(e.Text, out _);
        }

        private void OnToggleClick(object sender, RoutedEventArgs e)
        {
            IsBorrowing = !IsBorrowing;
            ErrorMessage = string.Empty;
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

        private void ShowHelpDialog()
        {
            MessageBox.Show("Drug Wars\n\nRetro remake by Mark.\n\nLoan Shark window.\nF1: Help\nEsc: Close window\n", "About / Help", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
} 