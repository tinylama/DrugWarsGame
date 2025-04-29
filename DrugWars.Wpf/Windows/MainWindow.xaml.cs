using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using DrugWars.Core.Models;
using DrugWars.Wpf.Models;
using DrugWars.Wpf.Services;
using System.Text;
using System.Windows.Input;
using System.Windows.Controls;
using System.Windows.Media;
using DrugWars.Wpf.Windows;
using System.Collections.ObjectModel;
using DrugWars.Wpf.Utilities;
using System.Windows.Threading;

namespace DrugWars.Wpf.Windows;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : GameWindowBase, INotifyPropertyChanged
{
    private static MainWindow? _instance;
    private string _currentLocationName = string.Empty;
    private string _gameLog = string.Empty;
    private string _statusMessage = string.Empty;
    private bool _isStatusBarVisible = true;
    private ScrollViewer? _gameLogScrollViewer;

    private EventHandler<GameEventArgs>? _gameEventHandler;
    private EventHandler? _gameOverHandler;
    private Func<int, bool>? _trenchcoatUpgradeHandler;
    private Func<string, string[], string>? _playerChoiceHandler;

    public event PropertyChangedEventHandler? PropertyChanged;

    public ObservableCollection<InventoryItem> InventoryList { get; set; } = new();
    public string TrenchcoatCapacity => $"{GameEngine?.Player?.CurrentInventoryTotal ?? 0}/{GameEngine?.Player?.MaxInventory ?? 0}";

    public override GameEngine GameEngine
    {
        get => base.GameEngine;
        set
        {
            Debug.WriteLine($"GameEngine property set: {value}");
            // Remove old event handlers if needed
            if (base.GameEngine != null)
            {
                // Clean up old event handlers
                base.GameEngine.GameEventOccurred -= _gameEventHandler;
                Debug.WriteLine("GameEngine.GameEventOccurred handler removed");
                base.GameEngine.GameOver -= _gameOverHandler;
                Debug.WriteLine("GameEngine.GameOver handler removed");
                base.GameEngine.TrenchcoatUpgradeRequested -= _trenchcoatUpgradeHandler;
                Debug.WriteLine("GameEngine.TrenchcoatUpgradeRequested handler removed");
                base.GameEngine.PlayerChoiceRequested -= _playerChoiceHandler;
                Debug.WriteLine("GameEngine.PlayerChoiceRequested handler removed");
            }

            base.GameEngine = value;
            
            // Set the static Current property for converters to access
            GameEngine.Current = value;
            
            // Listen to events on the new GameEngine
            WireGameEngineEvents();
            
            // Update UI
            Update();
            
            OnPropertyChanged(nameof(GameEngine));
            OnPropertyChanged(nameof(Player));
            OnPropertyChanged(nameof(Drugs));
            OnPropertyChanged(nameof(IsBankAvailable));
            OnPropertyChanged(nameof(IsLoanSharkAvailable));
            
            Debug.WriteLine($"GameEngine set complete. Player: {Player}");
        }
    }

    public string GameLog
    {
        get => _gameLog;
        set
        {
            _gameLog = value;
            OnPropertyChanged(nameof(GameLog));
            // Auto-scroll game log
            _gameLogScrollViewer?.ScrollToEnd();
        }
    }

    public string CurrentLocationName
    {
        get => _currentLocationName;
        set
        {
            _currentLocationName = value;
            OnPropertyChanged(nameof(CurrentLocationName));
        }
    }

    public Player? Player => GameEngine?.Player;
    public IEnumerable<Drug>? Drugs => GameEngine?.Drugs;
    public Dictionary<string, int>? PlayerInventory => Player?.Inventory;
    public bool IsBankAvailable => GameEngine?.IsBankAvailable ?? false;
    public bool IsLoanSharkAvailable => GameEngine?.IsLoanSharkAvailable ?? false;

    public string StatusMessage
    {
        get => _statusMessage;
        set { _statusMessage = value; OnPropertyChanged(nameof(StatusMessage)); }
    }

    public bool IsStatusBarVisible
    {
        get => _isStatusBarVisible;
        set
        {
            if (_isStatusBarVisible != value)
            {
                _isStatusBarVisible = value;
                OnPropertyChanged(nameof(IsStatusBarVisible));
            }
        }
    }

    private DispatcherTimer _statusBarTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(3) };

    public MainWindow(GameEngine gameEngine) : base(gameEngine)
    {
        try
        {
            Debug.WriteLine("MainWindow constructor started");
            Debug.WriteLine($"=== Initializing MainWindow ===");
            Debug.WriteLine($"Time: {DateTime.Now}");
            InitializeComponent();
            Debug.WriteLine("InitializeComponent completed");
            DataContext = this;
            Debug.WriteLine("DataContext set");
            Debug.WriteLine($"DataContext type: {DataContext?.GetType().FullName}");
            _instance = this;
            Debug.WriteLine("Instance set");
            Debug.WriteLine("MainWindow initialized successfully");
            Debug.WriteLine($"GameEngine (in ctor): {GameEngine}");
            Debug.WriteLine($"Player (in ctor): {Player}");
            // Keyboard shortcuts
            InputBindings.Add(new KeyBinding(new RelayCommand(_ => { Debug.WriteLine("ESC pressed, calling Close()"); Close(); }), new KeyGesture(Key.Escape)));
            InputBindings.Add(new KeyBinding(new RelayCommand(_ => ShowHelpDialog()), new KeyGesture(Key.F1)));
            _statusBarTimer.Tick += (s, e) => { Debug.WriteLine("StatusBarTimer ticked, hiding status bar"); IsStatusBarVisible = false; _statusBarTimer.Stop(); };
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error initializing MainWindow: {ex.Message}");
            Debug.WriteLine($"Stack Trace: {ex.StackTrace}");
            MessageBox.Show($"Error initializing game: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            Debug.WriteLine("Calling Close() from catch block in constructor");
            Close();
        }
    }

    protected override void OnSourceInitialized(EventArgs e)
    {
        Debug.WriteLine("OnSourceInitialized called");
        base.OnSourceInitialized(e);
        Debug.WriteLine($"OnSourceInitialized: DataContext type: {DataContext?.GetType().FullName}");
        Debug.WriteLine($"OnSourceInitialized: GameEngine: {GameEngine}");
        Debug.WriteLine($"OnSourceInitialized: Player: {Player}");
        if (GameEngine == null)
        {
            Debug.WriteLine("Error: GameEngine is null after initialization");
            MessageBox.Show("Error: Game engine not initialized", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            Close();
        }
        Debug.WriteLine("OnSourceInitialized completed");
        _gameLogScrollViewer = (ScrollViewer)this.FindName("GameLogScrollViewer");
    }

    protected override void OnActivated(EventArgs e)
    {
        Debug.WriteLine("OnActivated called");
        base.OnActivated(e);
        Debug.WriteLine("OnActivated completed");
    }

    protected override void OnDeactivated(EventArgs e)
    {
        Debug.WriteLine("OnDeactivated called");
        base.OnDeactivated(e);
        Debug.WriteLine("OnDeactivated completed");
    }

    protected override void OnClosed(EventArgs e)
    {
        Debug.WriteLine("OnClosed called");
        
        // Cleanup event handlers
        if (GameEngine != null)
        {
            if (_gameEventHandler != null)
                GameEngine.GameEventOccurred -= _gameEventHandler;
            if (_gameOverHandler != null)
                GameEngine.GameOver -= _gameOverHandler;
            if (_trenchcoatUpgradeHandler != null)
                GameEngine.TrenchcoatUpgradeRequested -= _trenchcoatUpgradeHandler;
            if (_playerChoiceHandler != null)
                GameEngine.PlayerChoiceRequested -= _playerChoiceHandler;
        }

        // Dispose timer
        _statusBarTimer?.Stop();
        _statusBarTimer = null!; // Mark as nulled after disposal

        if (_instance == this)
        {
            Debug.WriteLine("Clearing instance reference");
            _instance = null;
        }

        base.OnClosed(e);
        Debug.WriteLine("OnClosed completed");
    }

    protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
    {
        Debug.WriteLine("OnClosing called");
        Debug.WriteLine($"OnClosing: Reason: {e.ToString()}");
        Debug.WriteLine($"OnClosing: DataContext type: {DataContext?.GetType().FullName}");
        Debug.WriteLine($"OnClosing: GameEngine: {GameEngine}");
        Debug.WriteLine($"OnClosing: Player: {Player}");
        Debug.WriteLine("OnClosing stack trace: " + Environment.StackTrace);
        // Allow the window to close normally
        Debug.WriteLine("OnClosing completed");
    }

    protected virtual void OnPropertyChanged(string propertyName)
    {
        Debug.WriteLine($"OnPropertyChanged: {propertyName}");
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    private void UpdatePlayerStats()
    {
        try
        {
            Logger.Log("Updating player stats");
            CurrentLocationName = GameEngine.Locations[GameEngine.Player.CurrentLocation].Name;
            OnPropertyChanged(nameof(Player));
            OnPropertyChanged(nameof(Drugs));
            OnPropertyChanged(nameof(IsBankAvailable));
            OnPropertyChanged(nameof(IsLoanSharkAvailable));
            OnPropertyChanged(nameof(TrenchcoatCapacity));
            
            // Extra safety check for game over conditions
            GameEngine.CheckGameOverConditions();
            
            Logger.Log("Player stats updated successfully");
        }
        catch (Exception ex)
        {
            Logger.LogError("UpdatePlayerStats", ex);
        }
    }

    private void OnGameEvent(object? sender, GameEventArgs e)
    {
        try
        {
            Debug.WriteLine($"Game event occurred: {e.Message}");
            GameLog += $"{e.Message}\n";
            UpdatePlayerStats();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error in OnGameEvent: {ex.Message}");
            Debug.WriteLine($"Stack Trace: {ex.StackTrace}");
        }
    }

    private void OnGameOver(object? sender, GameEventArgs e)
    {
        try
        {
            Debug.WriteLine("Game over event triggered");
            
            // Create and show game over window
            var gameOverWindow = new GameOverWindow(GameEngine) { Owner = this };
            bool startNewGame = gameOverWindow.ShowDialog() == true;
            
            // Cleanup current window
            GameEngine = new GameEngine(); // Use empty engine instead of null
            Close();
            
            // If user wants to start a new game, do it after current window is fully closed
            if (startNewGame)
            {
                Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() =>
                {
                    try
                    {
                var mainMenu = new MainWindow(new GameEngine());
                mainMenu.Show();
            }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Failed to start new game: {ex.Message}", "Error", 
                            MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }));
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error in OnGameOver: {ex.Message}");
            Debug.WriteLine($"Stack trace: {ex.StackTrace}");
            MessageBox.Show($"An error occurred while handling game over: {ex.Message}", 
                "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void UpdateDisplay()
    {
        try
        {
            Debug.WriteLine("Updating display");
            CurrentLocationName = GameEngine.Locations[GameEngine.Player.CurrentLocation].Name;
            OnPropertyChanged(nameof(Player));
            Debug.WriteLine("Display updated successfully");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error in UpdateDisplay: {ex.Message}");
            Debug.WriteLine($"Stack Trace: {ex.StackTrace}");
        }
    }

    private void UpdateInventoryList()
    {
        InventoryList.Clear();
        if (GameEngine?.Player?.Inventory != null && GameEngine.Drugs != null)
        {
            foreach (var item in GameEngine.Player.Inventory)
            {
                var drug = GameEngine.Drugs.FirstOrDefault(d => d.Name == item.Key);
                InventoryList.Add(new InventoryItem
                {
                    DrugName = item.Key,
                    Quantity = item.Value,
                    ValuePerUnit = (int)(GameEngine.GetAverageBuyPrice(item.Key))
                });
            }
        }
        OnPropertyChanged(nameof(TrenchcoatCapacity));
    }

    private void OnBuyClick(object sender, RoutedEventArgs e)
    {
        try
        {
            Debug.WriteLine("=== Buy button clicked ===");
            Debug.WriteLine($"Player cash: ${GameEngine.Player.Cash:N0}");
            if (GameEngine.Player.Cash <= 0)
            {
                Debug.WriteLine("Player has no money");
                StatusMessage = "You do not have enough money to buy drugs.";
                return;
            }
            
            try 
            {
                var buyWindow = new BuyWindow();
                buyWindow.GameEngine = this.GameEngine; // Set GameEngine after creation
                buyWindow.Owner = this;
            buyWindow.ShowDialog();
            UpdatePlayerStats();
            UpdateInventoryList();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error creating BuyWindow: {ex.Message}");
                Debug.WriteLine($"Stack Trace: {ex.StackTrace}");
                StatusMessage = "An error occurred while opening the buy window.";
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error in OnBuyClick: {ex.Message}");
            Debug.WriteLine($"Stack Trace: {ex.StackTrace}");
            StatusMessage = $"An error occurred while opening the buy window: {ex.Message}";
        }
    }

    private void OnSellClick(object sender, RoutedEventArgs e)
    {
        try
        {
            Debug.WriteLine("=== Sell button clicked ===");
            Debug.WriteLine($"Player cash: ${GameEngine.Player.Cash:N0}");
            if (GameEngine.Player.Inventory.Count == 0)
            {
                Debug.WriteLine("Player has no drugs to sell");
                StatusMessage = "You do not have any drugs to sell.";
                return;
            }
            
            try 
            {
                var sellWindow = new SellWindow();
                sellWindow.GameEngine = this.GameEngine; // Set GameEngine after creation
                sellWindow.Owner = this;
            sellWindow.ShowDialog();
            UpdatePlayerStats();
            UpdateInventoryList();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error creating SellWindow: {ex.Message}");
                Debug.WriteLine($"Stack Trace: {ex.StackTrace}");
                StatusMessage = "An error occurred while opening the sell window.";
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error in OnSellClick: {ex.Message}");
            Debug.WriteLine($"Stack Trace: {ex.StackTrace}");
            StatusMessage = $"An error occurred while opening the sell window: {ex.Message}";
        }
    }

    private void OnTravelClick(object sender, RoutedEventArgs e)
    {
        try
        {
            Debug.WriteLine("=== Travel button clicked ===");
            Debug.WriteLine($"Player cash: ${GameEngine.Player.Cash:N0}");
            Debug.WriteLine($"Current location: {GameEngine.Locations[GameEngine.Player.CurrentLocation].Name}");
            var travelWindow = new TravelWindow { GameEngine = GameEngine, Owner = this };
            var result = travelWindow.ShowDialog();
            if (result == true)
            {
                GameEngine.AdvanceDayOrTravel();
                UpdatePlayerStats();
                UpdateInventoryList();
                OnPropertyChanged(nameof(Drugs));
                OnPropertyChanged(nameof(IsBankAvailable));
                OnPropertyChanged(nameof(IsLoanSharkAvailable));
                
                // Extra check for day limit after travel
                GameEngine.CheckGameOverConditions();
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error in OnTravelClick: {ex.Message}");
            Debug.WriteLine($"Stack Trace: {ex.StackTrace}");
            SetStatusMessage($"An error occurred while opening the travel window: {ex.Message}");
        }
    }

    private void OnBankClick(object sender, RoutedEventArgs e)
    {
        try
        {
            Debug.WriteLine("=== Bank button clicked ===");
            Debug.WriteLine($"Player cash: ${GameEngine.Player.Cash:N0}");
            Debug.WriteLine($"Player bank: ${GameEngine.Player.Bank:N0}");
            if (!GameEngine.IsBankAvailable)
            {
                Debug.WriteLine("Bank not available at current location");
                var cityName = GameEngine.Locations[0].Name;
                SetStatusMessage($"The bank is only available in the {cityName}.");
                return;
            }
            var bankWindow = new BankWindow { GameEngine = GameEngine, Owner = this };
            bankWindow.ShowDialog();
            UpdatePlayerStats();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error in OnBankClick: {ex.Message}");
            Debug.WriteLine($"Stack Trace: {ex.StackTrace}");
            SetStatusMessage($"An error occurred while opening the bank window: {ex.Message}");
        }
    }

    private void OnLoanSharkClick(object sender, RoutedEventArgs e)
    {
        try
        {
            Debug.WriteLine("=== Loan Shark button clicked ===");
            Debug.WriteLine($"Player cash: ${GameEngine.Player.Cash:N0}");
            Debug.WriteLine($"Player debt: ${GameEngine.Player.Debt:N0}");
            if (!GameEngine.IsLoanSharkAvailable)
            {
                Debug.WriteLine("Loan shark not available at current location");
                var cityName = GameEngine.Locations[0].Name;
                SetStatusMessage($"The loan shark is only available in the {cityName}.");
                return;
            }
            var loanSharkWindow = new LoanSharkWindow(GameEngine) { Owner = this };
            loanSharkWindow.ShowDialog();
            UpdatePlayerStats();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error in OnLoanSharkClick: {ex.Message}");
            Debug.WriteLine($"Stack Trace: {ex.StackTrace}");
            SetStatusMessage($"An error occurred while opening the loan shark window: {ex.Message}");
        }
    }

    private void OnHelpClick(object sender, RoutedEventArgs e)
    {
        try
        {
            Logger.Log("Help button clicked");
            string helpText = "Drug Wars\n\n" +
                             "Buy low, sell high! Travel between locations to find the best prices.\n" +
                             "Watch out for cops and loan sharks!\n\n" +
                             "Game ends after 30 days or when you're broke.";
            
            MessageBox.Show(helpText, "Help", MessageBoxButton.OK, MessageBoxImage.Information);
            Logger.Log("Help message displayed");
        }
        catch (Exception ex)
        {
            Logger.LogError("OnHelpClick", ex);
        }
    }

    private void OnKeyDown(object sender, KeyEventArgs e)
    {
        // Check for Ctrl+D for diagnostics
        if (e.Key == Key.D && (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
        {
            ShowDiagnostics();
        }
    }

    private void ShowDiagnostics()
    {
        try
        {
            if (GameEngine == null)
            {
                MessageBox.Show("Game engine is not initialized.", "Diagnostics", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Build a diagnostic report
            var sb = new StringBuilder();
            sb.AppendLine("===== DRUG WARS DIAGNOSTICS =====");
            sb.AppendLine($"Expansion: {GameEngine.Expansion}");
            sb.AppendLine($"Day: {GameEngine.Player.Day}/30");
            sb.AppendLine();
            
            sb.AppendLine("===== PLAYER =====");
            sb.AppendLine($"Cash: ${GameEngine.Player.Cash}");
            sb.AppendLine($"Bank: ${GameEngine.Player.Bank}");
            sb.AppendLine($"Debt: ${GameEngine.Player.Debt}");
            sb.AppendLine($"Health: {GameEngine.Player.Health}%");
            sb.AppendLine($"Location: {GameEngine.Locations[GameEngine.Player.CurrentLocation].Name}");
            sb.AppendLine($"Inventory: {GameEngine.Player.CurrentInventoryTotal}/{GameEngine.Player.MaxInventory}");
            sb.AppendLine();
            
            sb.AppendLine("===== INVENTORY =====");
            if (GameEngine.Player.Inventory.Count == 0)
            {
                sb.AppendLine("No drugs in inventory.");
            }
            else
            {
                foreach (var item in GameEngine.Player.Inventory)
                {
                    sb.AppendLine($"{item.Key}: {item.Value}");
                }
            }
            sb.AppendLine();
            
            sb.AppendLine("===== DRUGS AT CURRENT LOCATION =====");
            foreach (var drug in GameEngine.Drugs)
            {
                sb.AppendLine($"{drug.Name}: ${drug.CurrentPrice} (Min: ${drug.MinPrice}, Max: ${drug.MaxPrice})");
            }

            sb.AppendLine();
            
            sb.AppendLine("===== AVAILABLE LOCATIONS =====");
            for (int i = 0; i < GameEngine.Locations.Count; i++)
            {
                var loc = GameEngine.Locations[i];
                sb.AppendLine($"{loc.Name}: Travel Cost ${loc.TravelCost}, Cop Presence {loc.CopPresence}%, Price Multiplier {loc.PriceMultiplier}x");
            }

            // Display the diagnostics
            MessageBox.Show(sb.ToString(), "Drug Wars Diagnostics", MessageBoxButton.OK, MessageBoxImage.Information);
            Logger.Log("Diagnostics displayed");
        }
        catch (Exception ex)
        {
            Logger.LogError("ShowDiagnostics", ex);
            MessageBox.Show($"Error showing diagnostics: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void OnCloseClick(object sender, RoutedEventArgs e)
    {
        Debug.WriteLine("OnCloseClick called, calling Close()");
        Close();
    }

    private void OnMinimizeClick(object sender, RoutedEventArgs e)
    {
        WindowState = WindowState.Minimized;
    }

    private void OnMaximizeClick(object sender, RoutedEventArgs e)
    {
        WindowState = WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
    }
    
    private void OnSettingsClick(object sender, RoutedEventArgs e)
    {
        try
        {
            Debug.WriteLine("=== Settings button clicked ===");
            var settingsWindow = new SettingsWindow { GameEngine = GameEngine, Owner = this };
            settingsWindow.ShowDialog();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error in OnSettingsClick: {ex.Message}");
            Debug.WriteLine($"Stack Trace: {ex.StackTrace}");
            MessageBox.Show($"Error opening settings window: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void OnTitleBarMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        try
        {
            DragMove();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error in OnTitleBarMouseLeftButtonDown: {ex.Message}");
            Debug.WriteLine($"Stack Trace: {ex.StackTrace}");
        }
    }

    private void OnWindowClosing(object sender, System.ComponentModel.CancelEventArgs e)
    {
        Debug.WriteLine("OnWindowClosing called");
        // Allow the window to close normally
        Debug.WriteLine("OnWindowClosing completed");
    }

    private void ShowHelpDialog()
    {
        MessageBox.Show("Drug Wars\n\nRetro remake by Mark.\n\nNavigate with keyboard or mouse.\nF1: Help\nEsc: Close window\n", "About / Help", MessageBoxButton.OK, MessageBoxImage.Information);
    }

    public void SetStatusMessage(string message)
    {
        StatusMessage = message;
        IsStatusBarVisible = true;
        _statusBarTimer.Stop();
        _statusBarTimer.Start();
    }

    private bool ConfirmTrenchcoatUpgrade(int cost)
    {
        var result = MessageBox.Show($"Would you like to buy a bigger trenchcoat for ${cost}? (Max capacity is 200.)", "Trenchcoat Upgrade", MessageBoxButton.YesNo, MessageBoxImage.Question);
        return result == MessageBoxResult.Yes;
    }

    private string ShowPlayerChoiceDialog(string message, string[] options)
    {
        try
        {
            Debug.WriteLine($"Player choice requested: {message}");
        // Only handle police/gun events, not travel cancel
        var dialog = new PlayerChoiceDialog(message, options);
        bool? result = dialog.ShowDialog();
            
            if (result == true && !string.IsNullOrEmpty(dialog.SelectedOption))
            {
                Debug.WriteLine($"Player chose: {dialog.SelectedOption}");
            return dialog.SelectedOption;
            }
            
            // If cancelled or no selection, return empty string (safer than null)
            Debug.WriteLine("Player dialog cancelled or no selection made");
            return string.Empty;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error in ShowPlayerChoiceDialog: {ex.Message}");
            Debug.WriteLine($"Stack trace: {ex.StackTrace}");
            // Return empty string on error
            return string.Empty;
        }
    }

    private void WireGameEngineEvents()
    {
        if (GameEngine != null)
        {
            _gameEventHandler = (s, e) => OnGameEvent(s, e);
            _gameOverHandler = (s, e) => OnGameOver(s, new GameEventArgs("Game over"));
            _trenchcoatUpgradeHandler = ConfirmTrenchcoatUpgrade;
            _playerChoiceHandler = ShowPlayerChoiceDialog;

            GameEngine.GameEventOccurred += _gameEventHandler;
            GameEngine.GameOver += _gameOverHandler;
            GameEngine.TrenchcoatUpgradeRequested += _trenchcoatUpgradeHandler;
            GameEngine.PlayerChoiceRequested += _playerChoiceHandler;
        }
    }

    private void Update()
    {
        UpdatePlayerStats();
        UpdateInventoryList();
        UpdateDisplay();
    }
}