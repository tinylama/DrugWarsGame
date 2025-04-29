using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Input;
using DrugWars.Wpf.Models;
using DrugWars.Wpf.Properties;
using DrugWars.Wpf.Services;
using DrugWars.Wpf.Utilities;
using System.Linq;
using System.IO;
using System.Windows.Threading;

namespace DrugWars.Wpf.Windows
{
    public partial class SettingsWindow : GameWindowBase, INotifyPropertyChanged
    {
        private bool _isMusicEnabled = true;
        private double _musicVolume = 50;
        private bool _isSoundEffectsEnabled = true;
        private double _effectsVolume = 50;
        private bool _isFullScreen = true;
        private bool _showFPS = false;
        private bool _showNotifications = true;
        private bool _confirmActions = true;
        private MediaPlayer? _musicPlayer;
        private string _errorMessage = string.Empty;
        private bool _hasError;
        private string _currentTrack = "";
        private string _currentArtist = "";
        private string _currentFileName = "";
        private string _currentTime = "0:00";
        private string _trackDuration = "0:00";
        private bool _isPlaying = true;
        private bool _isShuffle = false;
        private bool _isRepeat = false;
        private DispatcherTimer? _musicTimer;
        private bool _isMusicAvailable = true;

        public event PropertyChangedEventHandler? PropertyChanged;

        public bool IsMusicEnabled
        {
            get => _isMusicEnabled;
            set
            {
                if (_isMusicEnabled != value)
                {
                    _isMusicEnabled = value;
                    OnPropertyChanged(nameof(IsMusicEnabled));
                    if (_musicPlayer != null)
                    {
                        if (value)
                            _musicPlayer.Play();
                        else
                            _musicPlayer.Pause();
                    }
                }
            }
        }

        public double MusicVolume
        {
            get => _musicVolume;
            set
            {
                if (_musicVolume != value)
                {
                    _musicVolume = value;
                    OnPropertyChanged(nameof(MusicVolume));
                    if (_musicPlayer != null)
                        _musicPlayer.Volume = value / 100.0;
                }
            }
        }

        public bool IsSoundEffectsEnabled
        {
            get => _isSoundEffectsEnabled;
            set
            {
                if (_isSoundEffectsEnabled != value)
                {
                    _isSoundEffectsEnabled = value;
                    OnPropertyChanged(nameof(IsSoundEffectsEnabled));
                }
            }
        }

        public double EffectsVolume
        {
            get => _effectsVolume;
            set
            {
                if (_effectsVolume != value)
                {
                    _effectsVolume = value;
                    OnPropertyChanged(nameof(EffectsVolume));
                }
            }
        }

        public bool IsFullScreen
        {
            get => _isFullScreen;
            set
            {
                if (_isFullScreen != value)
                {
                    _isFullScreen = value;
                    OnPropertyChanged(nameof(IsFullScreen));
                }
            }
        }

        public bool ShowFPS
        {
            get => _showFPS;
            set
            {
                if (_showFPS != value)
                {
                    _showFPS = value;
                    OnPropertyChanged(nameof(ShowFPS));
                }
            }
        }

        public bool ShowNotifications
        {
            get => _showNotifications;
            set
            {
                if (_showNotifications != value)
                {
                    _showNotifications = value;
                    OnPropertyChanged(nameof(ShowNotifications));
                }
            }
        }

        public bool ConfirmActions
        {
            get => _confirmActions;
            set
            {
                if (_confirmActions != value)
                {
                    _confirmActions = value;
                    OnPropertyChanged(nameof(ConfirmActions));
                }
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

        public bool EnableSound { get; set; } = true;
        public bool EnableMusic { get; set; } = true;
        public string SelectedTheme { get; set; } = "Retro (Green/Black)";

        public string[] Themes { get; } = new[]
        {
            "Retro (Green/Black)",
            "Modern (Dark)"
        };

        public string CurrentTrack
        {
            get => _currentTrack;
            set { _currentTrack = value; OnPropertyChanged(nameof(CurrentTrack)); }
        }

        public string CurrentArtist
        {
            get => _currentArtist;
            set { _currentArtist = value; OnPropertyChanged(nameof(CurrentArtist)); }
        }

        public string CurrentFileName
        {
            get => _currentFileName;
            set { _currentFileName = value; OnPropertyChanged(nameof(CurrentFileName)); }
        }

        public string CurrentTime
        {
            get => _currentTime;
            set { _currentTime = value; OnPropertyChanged(nameof(CurrentTime)); }
        }

        public string TrackDuration
        {
            get => _trackDuration;
            set { _trackDuration = value; OnPropertyChanged(nameof(TrackDuration)); }
        }

        public double TrackDurationSeconds { get; set; }
        public double TrackPositionSeconds { get; set; }
        public string PlayPauseIcon => IsPlaying ? "⏸" : "▶";
        public bool IsPlaying
        {
            get => _isPlaying;
            set
            {
                if (_isPlaying != value)
                {
                    _isPlaying = value;
                    OnPropertyChanged(nameof(IsPlaying));
                    OnPropertyChanged(nameof(PlayPauseIcon));
                }
            }
        }
        public bool IsShuffle
        {
            get => _isShuffle;
            set
            {
                if (_isShuffle != value)
                {
                    _isShuffle = value;
                    OnPropertyChanged(nameof(IsShuffle));
                    OnPropertyChanged(nameof(PlayPauseIcon));
                }
            }
        }
        public bool IsRepeat
        {
            get => _isRepeat;
            set
            {
                if (_isRepeat != value)
                {
                    _isRepeat = value;
                    OnPropertyChanged(nameof(IsRepeat));
                    OnPropertyChanged(nameof(PlayPauseIcon));
                }
            }
        }

        public bool IsMusicAvailable { get => _isMusicAvailable; set { _isMusicAvailable = value; OnPropertyChanged(nameof(IsMusicAvailable)); } }

        public SettingsWindow()
        {
            try
            {
                InitializeComponent();
                Debug.WriteLine($"=== Initializing SettingsWindow ===");
                Debug.WriteLine($"Time: {DateTime.Now}");
                DataContext = this;
                LoadSettings();
                AudioManager.Instance.LoadSettings();
                ((App)Application.Current).ApplyTheme(Properties.Settings.Default.Theme);
                InitializeMusic();
                // Subscribe to track change event
                AudioManager.Instance.TrackChanged += (track, artist) =>
                {
                    CurrentTrack = track;
                    CurrentArtist = artist;
                    UpdateMusicUI();
                };
                // Set initial values
                CurrentTrack = AudioManager.Instance.CurrentTrack;
                CurrentArtist = AudioManager.Instance.CurrentArtist;
                CurrentFileName = AudioManager.Instance.CurrentFileName;
                CurrentTime = AudioManager.Instance.CurrentTime;
                TrackDuration = AudioManager.Instance.TrackDuration;
                TrackDurationSeconds = AudioManager.Instance.TrackDurationSeconds;
                TrackPositionSeconds = AudioManager.Instance.TrackPositionSeconds;
                IsPlaying = AudioManager.Instance.IsPlaying;
                IsShuffle = AudioManager.Instance.IsShuffle;
                IsRepeat = AudioManager.Instance.IsRepeat;
                IsMusicAvailable = AudioManager.Instance.HasMusic;
                OnPropertyChanged(nameof(PlayPauseIcon));
                // Start timer for music progress updates
                if (_musicTimer == null)
                {
                    _musicTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(500) };
                    _musicTimer.Tick += (s, e) => UpdateMusicProgress();
                }
                _musicTimer.Start();
                Debug.WriteLine("SettingsWindow initialized successfully");

                // Keyboard shortcuts
                InputBindings.Add(new KeyBinding(new RelayCommand(_ => Close()), new KeyGesture(Key.Escape)));
                InputBindings.Add(new KeyBinding(new RelayCommand(_ => ShowHelpDialog()), new KeyGesture(Key.F1)));
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error initializing SettingsWindow: {ex.Message}");
                Debug.WriteLine($"Stack Trace: {ex.StackTrace}");
                MessageBox.Show($"Error initializing settings window: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                DialogResult = false;
                Close();
            }
        }

        private static string GetMusicFolder()
        {
            // Use a path relative to the executable for portability
            return System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "background_music");
        }

        private void InitializeMusic()
        {
            try
            {
                Debug.WriteLine("Initializing music");
                if (MusicVolume <= 0) MusicVolume = 5; // much quieter default
                AudioManager.Instance.SetMusicVolume(MusicVolume);
                var musicFolder = GetMusicFolder();
                Debug.WriteLine($"Music folder: {musicFolder}");
                // Do not call PlayMusic here; music is started at app startup. Only update volume and UI.
                IsMusicAvailable = AudioManager.Instance.HasMusic;
                Debug.WriteLine("Music initialized successfully");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error initializing music: {ex.Message}");
                Debug.WriteLine($"Stack Trace: {ex.StackTrace}");
                ErrorMessage = "Error initializing music";
                IsMusicAvailable = false;
            }
        }

        private void LoadSettings()
        {
            try
            {
                Debug.WriteLine("Loading settings");
                IsMusicEnabled = Properties.Settings.Default.IsMusicEnabled;
                MusicVolume = Properties.Settings.Default.MusicVolume;
                if (MusicVolume <= 0) MusicVolume = 5; // much quieter default
                IsSoundEffectsEnabled = Properties.Settings.Default.AreSoundEffectsEnabled;
                EffectsVolume = Properties.Settings.Default.EffectsVolume;
                // Theme: default to Retro (Green/Black) if not set
                var savedTheme = Properties.Settings.Default.Theme;
                if (string.IsNullOrWhiteSpace(savedTheme))
                    SelectedTheme = "Retro (Green/Black)";
                else
                    SelectedTheme = savedTheme;
                OnPropertyChanged(nameof(IsMusicEnabled));
                OnPropertyChanged(nameof(MusicVolume));
                OnPropertyChanged(nameof(IsSoundEffectsEnabled));
                OnPropertyChanged(nameof(EffectsVolume));
                OnPropertyChanged(nameof(SelectedTheme));
                Debug.WriteLine("Settings loaded successfully");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error loading settings: {ex.Message}");
                Debug.WriteLine($"Stack Trace: {ex.StackTrace}");
                ErrorMessage = "Error loading settings";
            }
        }

        private void SaveSettings()
        {
            Properties.Settings.Default.IsMusicEnabled = IsMusicEnabled;
            Properties.Settings.Default.MusicVolume = MusicVolume;
            Properties.Settings.Default.AreSoundEffectsEnabled = IsSoundEffectsEnabled;
            Properties.Settings.Default.EffectsVolume = EffectsVolume;
            Properties.Settings.Default.Theme = SelectedTheme;
            Properties.Settings.Default.Save();
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void OnThemeChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (e.AddedItems.Count > 0)
                {
                    SelectedTheme = e.AddedItems[0]?.ToString() ?? "Classic (White/Blue)";
                    Properties.Settings.Default.Theme = SelectedTheme;
                    ((App)Application.Current).ApplyTheme(SelectedTheme);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in OnThemeChanged: {ex.Message}");
                ErrorMessage = "Error updating theme";
            }
        }

        private void OnEnableMusicChanged(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender is CheckBox checkBox)
                {
                    IsMusicEnabled = checkBox.IsChecked == true;
                }
                AudioManager.Instance.SetMusicEnabled(IsMusicEnabled);
                Properties.Settings.Default.IsMusicEnabled = IsMusicEnabled;
                Properties.Settings.Default.Save();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in OnEnableMusicChanged: {ex.Message}");
                Debug.WriteLine($"Stack Trace: {ex.StackTrace}");
                ErrorMessage = "Error updating music settings";
            }
        }

        private void OnMusicVolumeChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            try
            {
                Debug.WriteLine("Music volume changed");
                MusicVolume = e.NewValue;
                AudioManager.Instance.SetMusicVolume(MusicVolume);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in OnMusicVolumeChanged: {ex.Message}");
                Debug.WriteLine($"Stack Trace: {ex.StackTrace}");
                ErrorMessage = "Error updating music volume";
            }
        }

        private void OnEnableSoundEffectsChanged(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender is CheckBox checkBox)
                {
                    IsSoundEffectsEnabled = checkBox.IsChecked == true;
                }
                AudioManager.Instance.SetSoundEnabled(IsSoundEffectsEnabled);
                Properties.Settings.Default.AreSoundEffectsEnabled = IsSoundEffectsEnabled;
                Properties.Settings.Default.Save();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in OnEnableSoundEffectsChanged: {ex.Message}");
                Debug.WriteLine($"Stack Trace: {ex.StackTrace}");
                ErrorMessage = "Error updating sound effects settings";
            }
        }

        private void OnEffectsVolumeChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            try
            {
                Debug.WriteLine("Sound effects volume changed");
                EffectsVolume = e.NewValue;
                AudioManager.Instance.SetSoundVolume(EffectsVolume);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in OnEffectsVolumeChanged: {ex.Message}");
                Debug.WriteLine($"Stack Trace: {ex.StackTrace}");
                ErrorMessage = "Error updating sound effects volume";
            }
        }

        private void OnFullScreenChanged(object sender, RoutedEventArgs e)
        {
            try
            {
                Debug.WriteLine("Full screen setting changed");
                IsFullScreen = !IsFullScreen;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in OnFullScreenChanged: {ex.Message}");
                Debug.WriteLine($"Stack Trace: {ex.StackTrace}");
                ErrorMessage = "Error updating display settings";
            }
        }

        private void OnShowFPSChanged(object sender, RoutedEventArgs e)
        {
            try
            {
                Debug.WriteLine("Show FPS setting changed");
                ShowFPS = !ShowFPS;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in OnShowFPSChanged: {ex.Message}");
                Debug.WriteLine($"Stack Trace: {ex.StackTrace}");
                ErrorMessage = "Error updating display settings";
            }
        }

        private void OnShowNotificationsChanged(object sender, RoutedEventArgs e)
        {
            try
            {
                Debug.WriteLine("Show notifications setting changed");
                ShowNotifications = !ShowNotifications;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in OnShowNotificationsChanged: {ex.Message}");
                Debug.WriteLine($"Stack Trace: {ex.StackTrace}");
                ErrorMessage = "Error updating notification settings";
            }
        }

        private void OnConfirmActionsChanged(object sender, RoutedEventArgs e)
        {
            try
            {
                Debug.WriteLine("Confirm actions setting changed");
                ConfirmActions = !ConfirmActions;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in OnConfirmActionsChanged: {ex.Message}");
                Debug.WriteLine($"Stack Trace: {ex.StackTrace}");
                ErrorMessage = "Error updating confirmation settings";
            }
        }

        private void OnResetClick(object sender, RoutedEventArgs e)
        {
            try
            {
                Debug.WriteLine("Reset button clicked");
                IsMusicEnabled = true;
                MusicVolume = 50;
                IsSoundEffectsEnabled = true;
                EffectsVolume = 50;
                IsFullScreen = true;
                ShowFPS = false;
                ShowNotifications = true;
                ConfirmActions = true;
                ErrorMessage = string.Empty;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in OnResetClick: {ex.Message}");
                Debug.WriteLine($"Stack Trace: {ex.StackTrace}");
                ErrorMessage = "Error resetting settings";
            }
        }

        private void OnSaveClick(object sender, RoutedEventArgs e)
        {
            try
            {
                Debug.WriteLine("Save button clicked");
                SaveSettings();
                // Apply theme immediately
                ((App)Application.Current).ApplyTheme(SelectedTheme);
                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in OnSaveClick: {ex.Message}");
                Debug.WriteLine($"Stack Trace: {ex.StackTrace}");
                ErrorMessage = "Error saving settings";
            }
        }

        private void OnCancelClick(object sender, RoutedEventArgs e)
        {
            try
            {
                Debug.WriteLine("Close button clicked");
                if (_musicPlayer != null)
                {
                    _musicPlayer.Stop();
                    _musicPlayer.Close();
                }
                DialogResult = false;
                Close();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in OnCancelClick: {ex.Message}");
                Debug.WriteLine($"Stack Trace: {ex.StackTrace}");
            }
        }

        private void ShowHelpDialog()
        {
            MessageBox.Show("Drug Wars\n\nRetro remake by Mark.\n\nUse the settings to adjust sound, music, and theme.\nNavigate with keyboard or mouse.\n\nF1: Help\nEsc: Close window\n", "About / Help", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void OnNextTrackClick(object sender, RoutedEventArgs e)
        {
            AudioManager.Instance.NextTrack();
        }

        private void OnPreviousTrackClick(object sender, RoutedEventArgs e)
        {
            AudioManager.Instance.PreviousTrack();
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            if (_musicTimer != null)
            {
                _musicTimer.Stop();
                _musicTimer = null;
            }
            if (_musicPlayer != null)
            {
                _musicPlayer.Stop();
                _musicPlayer.Close();
                _musicPlayer = null;
            }
        }

        private void UpdateMusicUI()
        {
            CurrentFileName = AudioManager.Instance.CurrentFileName;
            CurrentTime = AudioManager.Instance.CurrentTime;
            TrackDuration = AudioManager.Instance.TrackDuration;
            TrackDurationSeconds = AudioManager.Instance.TrackDurationSeconds;
            TrackPositionSeconds = AudioManager.Instance.TrackPositionSeconds;
            IsPlaying = AudioManager.Instance.IsPlaying;
            IsShuffle = AudioManager.Instance.IsShuffle;
            IsRepeat = AudioManager.Instance.IsRepeat;
            IsMusicAvailable = AudioManager.Instance.HasMusic;
            OnPropertyChanged(nameof(PlayPauseIcon));
        }

        private void OnPlayPauseClick(object sender, RoutedEventArgs e)
        {
            if (IsPlaying) AudioManager.Instance.Pause();
            else AudioManager.Instance.Resume();
            UpdateMusicUI();
            if (IsPlaying) StartMusicTimer();
            else StopMusicTimer();
        }

        private void OnShuffleClick(object sender, RoutedEventArgs e)
        {
            AudioManager.Instance.ToggleShuffle();
            UpdateMusicUI();
        }

        private void OnRepeatClick(object sender, RoutedEventArgs e)
        {
            AudioManager.Instance.ToggleRepeat();
            UpdateMusicUI();
        }

        private void OnTrackSliderChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            AudioManager.Instance.Seek(e.NewValue);
        }

        private void StartMusicTimer()
        {
            if (_musicTimer == null)
            {
                _musicTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(500) };
                _musicTimer.Tick += (s, e) => UpdateMusicProgress();
            }
            _musicTimer.Start();
        }

        private void StopMusicTimer()
        {
            _musicTimer?.Stop();
        }

        private void UpdateMusicProgress()
        {
            AudioManager.Instance.UpdateTrackTimes();
            CurrentTime = AudioManager.Instance.CurrentTime;
            TrackDuration = AudioManager.Instance.TrackDuration;
            TrackPositionSeconds = AudioManager.Instance.TrackPositionSeconds;
            TrackDurationSeconds = AudioManager.Instance.TrackDurationSeconds;
            IsMusicAvailable = AudioManager.Instance.HasMusic;
            OnPropertyChanged(nameof(CurrentTime));
            OnPropertyChanged(nameof(TrackDuration));
            OnPropertyChanged(nameof(TrackPositionSeconds));
            OnPropertyChanged(nameof(TrackDurationSeconds));
            OnPropertyChanged(nameof(IsMusicAvailable));
        }
    }
} 