using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Linq;
using System.Diagnostics;

namespace DrugWars.Core.Models;

public class GameEngine : INotifyPropertyChanged
{
    private readonly Random random = new();
    private const int MAX_DAYS = 30;
    private readonly int _startingLocationIndex;
    private static readonly Random _random = new();
    private string _newsLog = string.Empty;
    private string? _lastManipulatedDrug;
    private string? _lastManipulationType;
    private readonly Dictionary<string, decimal> _averageBuyPrices = new();

    // Static reference to the current game engine - for use by converters
    private static GameEngine? _current;
    public static GameEngine? Current
    {
        get => _current;
        set => _current = value;
    }

    public Player Player { get; init; }
    public List<Drug> Drugs { get; init; }
    public List<Location> Locations { get; init; }
    public GameExpansion Expansion { get; private set; }
    public bool IsBankAvailable => Player.CurrentLocation == _startingLocationIndex;
    public bool IsLoanSharkAvailable => Player.CurrentLocation == _startingLocationIndex;
    public bool HasGun { get; set; }
    public string NewsLog
    {
        get => _newsLog;
        set { _newsLog = value; OnPropertyChanged(nameof(NewsLog)); }
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    public event EventHandler<GameEventArgs>? GameEventOccurred;
    public event EventHandler? GameOver;
    public event Func<int, bool>? TrenchcoatUpgradeRequested;
    public event Func<string, string[], string>? PlayerChoiceRequested;

    public GameEngine(GameExpansion expansion = GameExpansion.Original)
    {
        _startingLocationIndex = 0;
        Expansion = expansion;
        Player = new Player();
        Drugs = new List<Drug>();
        Locations = new List<Location>();
        HasGun = false;

        // Set as the current game engine
        Current = this;
    }

    public decimal GetAverageBuyPrice(string drugName)
    {
        return _averageBuyPrices.TryGetValue(drugName, out var avg) ? avg : 0;
    }

    public void BuyDrug(Drug drug, int quantity)
    {
        if (drug == null) throw new ArgumentNullException(nameof(drug));
        if (quantity <= 0) throw new ArgumentException("Quantity must be greater than 0", nameof(quantity));

        decimal totalCost = drug.CurrentPrice * quantity;
        if (totalCost > Player.Cash)
            throw new InvalidOperationException("Not enough cash");

        Player.Cash -= totalCost;
        if (!Player.Inventory.ContainsKey(drug.Name))
            Player.Inventory[drug.Name] = 0;
        Player.Inventory[drug.Name] += quantity;
        OnPropertyChanged(nameof(Player));
        GameEventOccurred?.Invoke(this, new GameEventArgs($"Bought {quantity} {drug.Name} for ${totalCost}"));

        if (quantity > drug.MaxPrice / 2) // Large buy
        {
            _lastManipulatedDrug = drug.Name;
            _lastManipulationType = "buy";
        }

        // Update average buy price
        if (!_averageBuyPrices.ContainsKey(drug.Name))
        {
            _averageBuyPrices[drug.Name] = drug.CurrentPrice;
        }
        else
        {
            int prevQty = Player.Inventory[drug.Name] - quantity;
            decimal prevTotal = _averageBuyPrices[drug.Name] * prevQty;
            decimal newTotal = prevTotal + (drug.CurrentPrice * quantity);
            _averageBuyPrices[drug.Name] = newTotal / Player.Inventory[drug.Name];
        }

        Player.CleanupInventory();
    }

    public decimal GetMaxLoanAmount()
    {
        // Base loan amount
        decimal baseLoan = 5000;

        // Increase loan limit based on player's net worth and reputation
        decimal netWorth = Player.Cash + Player.Bank - Player.Debt;
        decimal assetMultiplier = Math.Max(0, netWorth) / 1000;
        decimal dayMultiplier = Math.Min(2.0m, Player.Day / 15.0m);

        return Math.Min(50000, baseLoan * (1 + assetMultiplier) * dayMultiplier);
    }

    public decimal GetCurrentInterestRate()
    {
        // Base daily interest rate (5%)
        decimal baseRate = 0.05m;

        // Interest increases with debt-to-asset ratio
        decimal debtRatio = Player.Debt / Math.Max(1, Player.Cash + Player.Bank);
        decimal riskPremium = Math.Min(0.05m, debtRatio * 0.02m);

        // Interest decreases slightly as game progresses (player becomes more "trustworthy")
        decimal dayDiscount = Math.Min(0.02m, Player.Day * 0.001m);

        return Math.Max(0.03m, baseRate + riskPremium - dayDiscount);
    }

    private void ApplyDailyInterest()
    {
        if (Player.Debt > 0)
        {
            decimal rate = GetCurrentInterestRate();
            Player.Debt = (int)Math.Ceiling(Player.Debt * (1 + rate));

            // Notify player of significant interest charges
            if (rate > 0.07m)
            {
                GameEventOccurred?.Invoke(this, new GameEventArgs($"High interest rate warning! Current rate: {rate:P0}"));
            }
        }
    }

    // Additional safety check for day limit
    public void CheckGameOverConditions()
    {
        Debug.WriteLine($"[DEBUG] Checking game over conditions. Current day: {Player.Day}, MAX_DAYS: {MAX_DAYS}");
        if (Player.Day >= MAX_DAYS)
        {
            Debug.WriteLine($"[DEBUG] Day limit reached. Triggering game over.");
            GameEventOccurred?.Invoke(this, new GameEventArgs($"You've reached day {MAX_DAYS}. Time to retire!"));
            GameOver?.Invoke(this, EventArgs.Empty);
            return;
        }
        else if (Player.Health <= 0)
        {
            Debug.WriteLine($"[DEBUG] Health depleted. Triggering game over.");
            GameEventOccurred?.Invoke(this, new GameEventArgs("Your health has dropped to 0. Game over!"));
            GameOver?.Invoke(this, EventArgs.Empty);
            return;
        }

        // Check for excessive debt
        bool hasExcessiveDebt = Player.Debt > 100000 || (Player.Debt > 50000 && Player.Debt > (Player.Cash + Player.Bank) * 5);
        if (hasExcessiveDebt)
        {
            Debug.WriteLine($"[DEBUG] Excessive debt detected. Triggering game over.");
            GameEventOccurred?.Invoke(this, new GameEventArgs("Your debt has become unmanageable. The loan sharks are taking over. Game over!"));
            GameOver?.Invoke(this, EventArgs.Empty);
        }
    }

    public void AdvanceDayOrTravel()
    {
        Player.Day++;

        // Check game over conditions first
        if (Player.Day >= MAX_DAYS)
        {
            GameEventOccurred?.Invoke(this, new GameEventArgs($"You've reached day {MAX_DAYS}. Time to retire!"));
            GameOver?.Invoke(this, EventArgs.Empty);
            return;
        }

        // Apply interest before other events
        ApplyDailyInterest();

        // Loan shark events
        if (Player.Debt > 0)
        {
            // Chance of loan shark visit increases with debt ratio
            decimal debtRatio = Player.Debt / Math.Max(1, Player.Cash + Player.Bank);
            double visitChance = Math.Min(0.15, 0.05 + (double)debtRatio * 0.1);

            if (random.NextDouble() < visitChance)
            {
                // Different types of loan shark encounters
                double eventRoll = random.NextDouble();
                if (eventRoll < 0.4 && Player.Cash > 0)
                {
                    // Force partial payment
                    decimal payment = Math.Min(Player.Cash, Player.Debt * 0.3m);
                    Player.Cash -= payment;
                    Player.Debt -= payment;
                    GameEventOccurred?.Invoke(this, new GameEventArgs($"Loan shark thugs forced you to pay ${payment:N0}!"));
                }
                else if (eventRoll < 0.7)
                {
                    // Threaten player
                    Player.Health -= 20;
                    GameEventOccurred?.Invoke(this, new GameEventArgs("Loan shark thugs roughed you up! (-20 health)"));
                }
                else
                {
                    // Seize random inventory
                    var drugToSeize = Player.Inventory.Keys.OrderBy(_ => random.Next()).FirstOrDefault();
                    if (drugToSeize != null && Player.Inventory[drugToSeize] > 0)
                    {
                        int seized = Player.Inventory[drugToSeize];
                        Player.Inventory[drugToSeize] = 0;
                        GameEventOccurred?.Invoke(this, new GameEventArgs($"Loan shark thugs seized all your {drugToSeize} ({seized} units)!"));
                        Player.CleanupInventory();
                    }
                }
            }
        }

        // Rival dealer event (10% chance)
        if (random.NextDouble() < 0.1)
        {
            var drug = Drugs[random.Next(Drugs.Count)];
            bool spike = random.NextDouble() < 0.5;
            if (spike)
            {
                drug.CurrentPrice = drug.MaxPrice * 2;
                NewsLog = $"Rival dealer is buying up all the {drug.Name}! Price is spiking!";
            }
            else
            {
                drug.CurrentPrice = Math.Max(10, drug.MinPrice * 0.5m);
                NewsLog = $"Rival dealer is dumping {drug.Name}! Price is crashing!";
            }
        }
        else
        {
            NewsLog = string.Empty;
        }

        // Player market manipulation
        if (!string.IsNullOrEmpty(_lastManipulatedDrug))
        {
            var drug = Drugs.FirstOrDefault(d => d.Name == _lastManipulatedDrug);
            if (drug != null)
            {
                if (_lastManipulationType == "buy")
                {
                    drug.CurrentPrice = Math.Min(drug.MaxPrice * 2, drug.CurrentPrice * 1.5m);
                    NewsLog += $"\nYour big buy of {drug.Name} has driven the price up!";
                }
                else if (_lastManipulationType == "sell")
                {
                    drug.CurrentPrice = Math.Max(10, drug.CurrentPrice * 0.7m);
                    NewsLog += $"\nYour big sale of {drug.Name} has crashed the price!";
                }
            }
            _lastManipulatedDrug = null;
            _lastManipulationType = null;
        }

        RandomizeDrugPricesAndAvailability();
        TriggerRandomEvents();
        TriggerMarketEvent();

        // Record the final prices for all drugs after all changes
        foreach (var drug in Drugs)
        {
            drug.RecordDailyPrice();
        }

        OnPropertyChanged(nameof(Player));
        OnPropertyChanged(nameof(Drugs));
        GameEventOccurred?.Invoke(this, new GameEventArgs($"Day {Player.Day}: Arrived at {Locations[Player.CurrentLocation].Name}"));
        CheckPlayerStuck();
    }

    private void RandomizeDrugPricesAndAvailability()
    {
        // Only change price for a minority of drugs each round
        int numToChange = Math.Max(1, Drugs.Count / 3); // About 1/3 of drugs change
        var indices = Enumerable.Range(0, Drugs.Count).OrderBy(_ => random.Next()).Take(numToChange).ToHashSet();
        for (int i = 0; i < Drugs.Count; i++)
        {
            var drug = Drugs[i];
            // Scarcity (spike) or glut (crash) can still happen to any drug
            double eventRoll = random.NextDouble();
            if (eventRoll < 0.1)
            {
                drug.CurrentPrice = drug.MaxPrice * 2; // Scarcity
                continue;
            }
            else if (eventRoll < 0.18)
            {
                drug.CurrentPrice = Math.Max(10, drug.MinPrice * 0.5m); // Glut
                continue;
            }
            // Only update price if this drug is selected for change
            if (!indices.Contains(i)) continue;
            int minPrice = (int)drug.MinPrice;
            int maxPrice = (int)drug.MaxPrice;
            int range = maxPrice - minPrice;
            int basePrice = minPrice + random.Next(range + 1);
            int volatility = (int)(basePrice * 0.1);
            int price = basePrice + random.Next(-volatility, volatility + 1);
            price = (int)(Math.Round(price / 10.0) * 10);
            drug.CurrentPrice = Math.Max(minPrice, Math.Min(maxPrice, price));
        }
        OnPropertyChanged(nameof(Drugs));
    }

    private void TriggerRandomEvents()
    {
        // For first 3 rounds, block negative events
        bool allowNegativeEvents = Player.Day > 3;
        int maxTrenchcoat = 200;
        // Police bust: chance increases with day (starts at 10%, +0.5% per day)
        double policeChance = allowNegativeEvents ? 0.10 + 0.005 * Player.Day : 0;
        if (allowNegativeEvents && random.NextDouble() < policeChance)
        {
            int policeCount = 1 + Player.Day / 10; // More police as days go on
            string? choice = PlayerChoiceRequested?.Invoke($"Police bust! {policeCount} cop{(policeCount > 1 ? "s" : "")}! Do you want to run or fight?", new[] { "Run", "Fight" });
            if (string.IsNullOrEmpty(choice)) return; // Cancelled, do nothing
            bool success = false;
            if (choice == "Run")
            {
                // Run: odds decrease with more police
                double runChance = Math.Max(0.2, 0.7 - 0.1 * (policeCount - 1));
                success = random.NextDouble() < runChance;
                if (success)
                {
                    GameEventOccurred?.Invoke(this, new GameEventArgs("You ran from the police and escaped!"));
                }
                else
                {
                    int seized = 0;
                    foreach (var key in new List<string>(Player.Inventory.Keys))
                    {
                        seized += Player.Inventory[key];
                        Player.Inventory[key] = 0;
                    }
                    GameEventOccurred?.Invoke(this, new GameEventArgs($"You tried to run but got caught! All your drugs ({seized}) were seized."));
                }
            }
            else if (choice == "Fight")
            {
                // Fight: odds decrease with more police, gun helps
                double baseChance = HasGun ? 0.8 : 0.4;
                double fightChance = Math.Max(0.1, baseChance - 0.1 * (policeCount - 1));
                success = random.NextDouble() < fightChance;
                if (success)
                {
                    GameEventOccurred?.Invoke(this, new GameEventArgs("You fought the police and got away!"));
                }
                else
                {
                    int seized = 0;
                    foreach (var key in new List<string>(Player.Inventory.Keys))
                    {
                        seized += Player.Inventory[key];
                        Player.Inventory[key] = 0;
                    }
                    Player.Health -= 30;
                    GameEventOccurred?.Invoke(this, new GameEventArgs($"You lost the fight! All your drugs ({seized}) were seized and you were injured (-30 health)."));
                }
            }
            if (!success)
            {
                Player.CleanupInventory();
                CheckPlayerStuck();
            }
            return;
        }
        // Random event: offer to buy a gun (5% chance)
        if (!HasGun && random.NextDouble() < 0.05)
        {
            if (Player.Cash >= 1000)
            {
                string choice = PlayerChoiceRequested?.Invoke("A shady dealer offers you a gun for $1000. Buy it?", new[] { "Yes", "No" }) ?? "No";
                if (choice == "Yes")
                {
                    Player.Cash -= 1000;
                    HasGun = true;
                    GameEventOccurred?.Invoke(this, new GameEventArgs("You bought a gun! Your chances in a fight are much better."));
                }
            }
        }
        // Loan shark event if in debt (5% chance)
        if (allowNegativeEvents && Player.Debt > 0 && random.NextDouble() < 0.05)
        {
            decimal penalty = Math.Min(Player.Cash, Player.Debt * 0.5m);
            Player.Cash -= penalty;
            GameEventOccurred?.Invoke(this, new GameEventArgs($"Loan shark thugs took ${penalty} for unpaid debt!"));
        }
        // Offer to buy bigger coat (2% chance, only if not maxed)
        if (Player.MaxInventory < maxTrenchcoat && random.NextDouble() < 0.02)
        {
            if (Player.Cash >= 200)
            {
                bool approved = TrenchcoatUpgradeRequested?.Invoke(200) ?? true;
                if (approved)
                {
                    Player.Cash -= 200;
                    Player.MaxInventory = Math.Min(maxTrenchcoat, Player.MaxInventory + 40);
                    GameEventOccurred?.Invoke(this, new GameEventArgs($"You bought a bigger trenchcoat! Capacity is now {Player.MaxInventory}. (Max is {maxTrenchcoat})"));
                }
            }
        }
        // Mugging (chance increases with day, up to 10%)
        double mugChance = allowNegativeEvents ? Math.Min(0.05 + 0.002 * Player.Day, 0.10) : 0;
        if (allowNegativeEvents && random.NextDouble() < mugChance)
        {
            decimal loss = Math.Min(Player.Cash, 100m + (decimal)random.Next(400));
            Player.Cash -= loss;
            GameEventOccurred?.Invoke(this, new GameEventArgs($"You were mugged and lost ${loss}!"));
            Player.CleanupInventory();
            CheckPlayerStuck();
        }
        // Find cash (5% chance, always allowed)
        if (random.NextDouble() < 0.05)
        {
            int found = 100 + random.Next(400);
            Player.Cash += found;
            GameEventOccurred?.Invoke(this, new GameEventArgs($"You found ${found} on the street!"));
        }
    }

    private void TriggerMarketEvent()
    {
        if (Expansion == GameExpansion.Original)
        {
            var events = new[]
            {
                ("Cops crack down in Queens! Prices for Weed skyrocket!", "Weed", 2.0m),
                ("Bronx is flooded with cheap Ludes!", "Ludes", 0.5m),
                ("Rumor: New shipment of Cocaine hits Manhattan.", "Cocaine", 0.7m),
                ("Speed is hot in Brooklyn tonight!", "Speed", 1.8m),
                ("Acid drought in Central Park!", "Acid", 2.2m)
            };
            var evt = events[_random.Next(events.Length)];
            var drug = Drugs.FirstOrDefault(d => d.Name == evt.Item2);
            if (drug != null)
            {
                drug.CurrentPrice = (int)(drug.CurrentPrice * evt.Item3);
                AddGameLog(evt.Item1);
            }
        }
        else if (Expansion == GameExpansion.Australia)
        {
            var events = new[]
            {
                ("Pingas are going off at Bondi!", "Pingas", 2.0m),
                ("Ice drought in Kings Cross, prices up!", "Ice", 1.7m),
                ("Rack is everywhere in Melbourne CBD, prices drop!", "Rack", 0.6m),
                ("Buds are primo in Gold Coast tonight!", "Buds", 1.9m),
                ("Benzo bust in Perth, prices spike!", "Benzos", 2.3m)
            };
            var evt = events[_random.Next(events.Length)];
            var drug = Drugs.FirstOrDefault(d => d.Name == evt.Item2);
            if (drug != null)
            {
                drug.CurrentPrice = (int)(drug.CurrentPrice * evt.Item3);
                AddGameLog(evt.Item1);
            }
        }
        else if (Expansion == GameExpansion.UK)
        {
            var events = new[]
            {
                ("Charlie is proper expensive in Brixton, innit!", "Charlie", 2.1m),
                ("Mandy everywhere in Manchester, prices drop!", "Mandy", 0.5m),
                ("Brown's gone dry in Glasgow, prices up!", "Brown", 1.8m),
                ("Draw is cheap as chips in Liverpool!", "Draw", 0.7m),
                ("Ket shortage in Bristol, prices soar!", "Ket", 2.4m)
            };
            var evt = events[_random.Next(events.Length)];
            var drug = Drugs.FirstOrDefault(d => d.Name == evt.Item2);
            if (drug != null)
            {
                drug.CurrentPrice = (int)(drug.CurrentPrice * evt.Item3);
                AddGameLog(evt.Item1);
            }
        }
        else if (Expansion == GameExpansion.Medellin)
        {
            var events = new[]
            {
                ("Perico está carísimo en El Centro!", "Perico", 2.2m),
                ("Cripy barato en Envigado, aprovecha!", "Cripy", 0.6m),
                ("La policía hace redada en Laureles, Benzos suben!", "Benzos", 1.9m),
                ("Pills en oferta en El Poblado!", "Pills", 0.8m),
                ("Comuna 13 está caliente, Rack sube de precio!", "Rack", 1.7m)
            };
            var evt = events[_random.Next(events.Length)];
            var drug = Drugs.FirstOrDefault(d => d.Name == evt.Item2);
            if (drug != null)
            {
                drug.CurrentPrice = (int)(drug.CurrentPrice * evt.Item3);
                AddGameLog(evt.Item1);
            }
        }
    }

    private void AddGameLog(string message)
    {
        GameEventOccurred?.Invoke(this, new GameEventArgs(message));
    }

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    public void SellDrug(Drug drug, int quantity)
    {
        if (quantity > drug.MaxPrice / 2) // Large sell
        {
            _lastManipulatedDrug = drug.Name;
            _lastManipulationType = "sell";
        }
        // ... existing code ...
        if (Player.Inventory[drug.Name] == 0)
            _averageBuyPrices.Remove(drug.Name);
        // ... existing code ...
        Player.CleanupInventory();
        CheckPlayerStuck();
    }

    private void CheckPlayerStuck()
    {
        Player.CleanupInventory();

        // Find minimum travel cost to any location
        decimal minTravelCost = Locations
            .Where(loc => loc != Locations[Player.CurrentLocation])
            .Min(loc => loc.TravelCost);

        // Find minimum drug price
        decimal minDrugPrice = Drugs.Min(d => d.CurrentPrice);

        // Calculate minimum cash needed to do anything useful
        decimal minUsefulCash = Math.Min(minTravelCost, minDrugPrice);

        bool hasUsableCash = Player.Cash >= minUsefulCash;
        bool hasInventory = Player.Inventory.Count > 0;
        bool canTravel = Locations.Any(loc => loc != Locations[Player.CurrentLocation] && Player.Cash >= loc.TravelCost);
        bool hasExcessiveDebt = Player.Debt > 100000 || (Player.Debt > 50000 && Player.Debt > (Player.Cash + Player.Bank) * 5);

        // Check for game over conditions
        if (Player.Health <= 0)
        {
            GameEventOccurred?.Invoke(this, new GameEventArgs("Your health has dropped to 0. Game over!"));
            GameOver?.Invoke(this, EventArgs.Empty);
            return;
        }

        // Check for day limit - MAX_DAYS is inclusive (game ends when Player.Day == MAX_DAYS)
        if (Player.Day >= MAX_DAYS)
        {
            GameEventOccurred?.Invoke(this, new GameEventArgs($"You've reached day {MAX_DAYS}. Time to retire!"));
            GameOver?.Invoke(this, EventArgs.Empty);
            return;
        }

        if (hasExcessiveDebt)
        {
            GameEventOccurred?.Invoke(this, new GameEventArgs("Your debt has become unmanageable. The loan sharks are taking over. Game over!"));
            GameOver?.Invoke(this, EventArgs.Empty);
            return;
        }

        if (!hasUsableCash && !hasInventory && !canTravel)
        {
            if (!Player.HasReceivedBailout)
            {
                Player.Cash += 5000;
                Player.Debt += 5000;
                Player.HasReceivedBailout = true;
                GameEventOccurred?.Invoke(this, new GameEventArgs("You are broke and out of options! The loan shark bails you out with $5,000 (added to your debt). Don't let it happen again!"));
            }
            else
            {
                GameEventOccurred?.Invoke(this, new GameEventArgs("You are completely out of options. The loan shark won't help you again. Game over!"));
                GameOver?.Invoke(this, EventArgs.Empty);
            }
        }
    }
}