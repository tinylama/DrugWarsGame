using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using DrugWars.Wpf.Utilities;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text.Json;
using DrugWars.Core.Models;
using DrugWars.Wpf.Models;
using System.IO.IsolatedStorage;
using System.Diagnostics;

namespace DrugWars.Wpf.Windows
{
    public partial class GameOverWindow : GameWindowBase, INotifyPropertyChanged
    {
        private readonly GameEngine _gameEngine;
        private string _playerName = "";
        private bool _isTopScore;
        private readonly decimal _finalInventoryValue;

        public int Score => (int)(_gameEngine.Player.Cash + _gameEngine.Player.Bank + _finalInventoryValue - _gameEngine.Player.Debt);
        public int DaysSurvived => _gameEngine.Player.Day;
        public decimal FinalCash => _gameEngine.Player.Cash;
        public decimal FinalBank => _gameEngine.Player.Bank;
        public decimal FinalDebt => _gameEngine.Player.Debt;
        public decimal FinalInventoryValue => _finalInventoryValue;
        public decimal NetWorth => FinalCash + FinalBank + FinalInventoryValue - FinalDebt;
        public ObservableCollection<HighScore> HighScores { get; } = [];

        public string PlayerName
        {
            get => _playerName;
            set
            {
                if (_playerName != value)
                {
                    _playerName = value;
                    OnPropertyChanged();
                    CommandManager.InvalidateRequerySuggested();
                }
            }
        }

        public bool IsTopScore
        {
            get => _isTopScore;
            set
            {
                if (_isTopScore != value)
                {
                    _isTopScore = value;
                    OnPropertyChanged();
                }
            }
        }

        public ICommand SaveScoreCommand { get; }
        public ICommand MainMenuCommand { get; }

        public GameOverWindow(GameEngine gameEngine)
        {
            _gameEngine = gameEngine ?? throw new ArgumentNullException(nameof(gameEngine));

            // Calculate final inventory value at current market prices
            _finalInventoryValue = _gameEngine.Player.Inventory.Sum(item =>
            {
                var drug = _gameEngine.Drugs.FirstOrDefault(d => d.Name == item.Key);
                return drug != null ? drug.CurrentPrice * item.Value : 0;
            });

            SaveScoreCommand = new RelayCommand(
                execute: _ => SaveScore(),
                canExecute: _ => !string.IsNullOrWhiteSpace(PlayerName)
            );

            MainMenuCommand = new RelayCommand(
                execute: _ =>
                {
                    DialogResult = true;
                    Close();
                }
            );

            InitializeComponent();
            DataContext = this;
            LoadHighScores();
        }

        private void LoadHighScores()
        {
            var scores = HighScoreManager.Load();
            HighScores.Clear();

            foreach (var score in scores)
            {
                HighScores.Add(score);
            }

            // Check if current score is a high score
            var lowestScore = scores.Count > 0 ? scores.Min(s => s.Score) : 0;
            IsTopScore = scores.Count < 10 || Score > lowestScore;
        }

        private void SaveScore()
        {
            if (string.IsNullOrWhiteSpace(PlayerName))
                return;

            var newScore = new HighScore
            {
                Name = PlayerName,
                Score = Score,
                Rank = HighScores.Count + 1
            };

            HighScoreManager.Add(newScore);
            LoadHighScores(); // Reload to get updated rankings
            IsTopScore = false;
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                DragMove();
        }

        private void OnCloseClick(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class HighScore
    {
        public int Rank { get; set; }
        public string Name { get; set; } = "";
        public int Score { get; set; }
    }

    public static class HighScoreManager
    {
        private static readonly string FileName = "HighScores.json";
        private static List<HighScore> _scores = [];
        private static bool _isLoaded;

        private static void EnsureLoaded()
        {
            if (_isLoaded) return;

            try
            {
                using var storage = IsolatedStorageFile.GetUserStoreForAssembly();
                if (storage.FileExists(FileName))
                {
                    using var stream = storage.OpenFile(FileName, FileMode.Open);
                    using var reader = new StreamReader(stream);
                    var json = reader.ReadToEnd();
                    _scores = JsonSerializer.Deserialize<List<HighScore>>(json) ?? [];
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error loading high scores: {ex.Message}");
                _scores = [];
            }

            _isLoaded = true;
        }

        private static void SaveScores()
        {
            try
            {
                using var storage = IsolatedStorageFile.GetUserStoreForAssembly();
                using var stream = storage.CreateFile(FileName);
                using var writer = new StreamWriter(stream);
                var json = JsonSerializer.Serialize(_scores);
                writer.Write(json);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error saving high scores: {ex.Message}");
            }
        }

        public static List<HighScore> Load()
        {
            EnsureLoaded();
            var orderedScores = _scores
                .OrderByDescending(s => s.Score)
                .Take(10)
                .ToList();

            // Update rankings
            for (int i = 0; i < orderedScores.Count; i++)
            {
                orderedScores[i].Rank = i + 1;
            }

            return orderedScores;
        }

        public static void Add(HighScore entry)
        {
            EnsureLoaded();
            _scores.Add(entry);

            // Keep only top scores
            _scores = [.. _scores
                .OrderByDescending(s => s.Score)
                .Take(10)];

            // Update rankings
            for (int i = 0; i < _scores.Count; i++)
            {
                _scores[i].Rank = i + 1;
            }

            SaveScores();
        }
    }
}