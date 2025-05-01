using System.Media;
using System.Windows.Media;
using DrugWars.Wpf.Properties;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System;

namespace DrugWars.Wpf.Services
{
    public class AudioManager
    {
        private static AudioManager? _instance;
        public static AudioManager Instance => _instance ??= new AudioManager();

        private MediaPlayer _musicPlayer = new MediaPlayer();
        private List<string> _playlist = new List<string>();
        private int _currentTrackIndex = -1;
        private bool _shuffle = true;
        private readonly Random _random = new();

        public bool IsMusicEnabled { get; private set; }
        public double MusicVolume { get; private set; }
        public bool IsSoundEnabled { get; private set; }
        public double SoundVolume { get; private set; }

        public event Action<string, string>? TrackChanged;
        public string CurrentTrack { get; private set; } = string.Empty;
        public string CurrentArtist { get; private set; } = string.Empty;
        public string CurrentFileName { get; private set; } = string.Empty;
        public string CurrentTime { get; private set; } = "0:00";
        public string TrackDuration { get; private set; } = "0:00";
        public double TrackDurationSeconds { get; private set; }
        public double TrackPositionSeconds { get; private set; }
        public bool IsPlaying { get; private set; }
        public bool IsShuffle { get => _shuffle; }
        public bool IsRepeat { get; private set; }
        public bool HasMusic => _playlist != null && _playlist.Count > 0;

        private AudioManager()
        {
            LoadSettings();
        }

        public void PlayMusic(string? musicFileOrFolder = null)
        {
            if (!IsMusicEnabled) return;

            if (!string.IsNullOrEmpty(musicFileOrFolder) && Directory.Exists(musicFileOrFolder))
            {
                _playlist = new List<string>(Directory.GetFiles(musicFileOrFolder, "*.mp3"));
                if (_shuffle)
                    _playlist = _playlist.OrderBy(x => _random.Next()).ToList();
                _currentTrackIndex = 0;
                if (_playlist.Count == 0)
                {
                    System.Diagnostics.Debug.WriteLine($"No music files found in {musicFileOrFolder}");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"Loaded music files: {string.Join(", ", _playlist)}");
                }
            }
            else if (!string.IsNullOrEmpty(musicFileOrFolder) && File.Exists(musicFileOrFolder))
            {
                _playlist = new List<string> { musicFileOrFolder };
                _currentTrackIndex = 0;
            }
            else
            {
                System.Diagnostics.Debug.WriteLine($"Music folder or file not found: {musicFileOrFolder}");
            }

            if (_playlist.Count == 0) return;

            PlayCurrentTrack();
        }

        private void PlayCurrentTrack()
        {
            if (_playlist.Count == 0 || _currentTrackIndex < 0 || _currentTrackIndex >= _playlist.Count) return;
            var file = _playlist[_currentTrackIndex];
            _musicPlayer.Open(new System.Uri(file, System.UriKind.RelativeOrAbsolute));
            _musicPlayer.Volume = MusicVolume / 100.0;
            _musicPlayer.MediaEnded -= OnTrackEnded;
            _musicPlayer.MediaEnded += OnTrackEnded;
            _musicPlayer.MediaOpened -= OnTrackMediaOpened;
            _musicPlayer.MediaOpened += OnTrackMediaOpened;
            // Parse track and artist from filename: "Artist - Title.mp3"
            var name = System.IO.Path.GetFileNameWithoutExtension(file);
            var parts = name.Split(" - ");
            if (parts.Length == 2)
            {
                CurrentArtist = parts[0].Trim();
                CurrentTrack = parts[1].Trim();
            }
            else
            {
                CurrentArtist = "Unknown";
                CurrentTrack = name;
            }
            CurrentFileName = name;
            // Don't update times or fire TrackChanged until MediaOpened
            _musicPlayer.Play();
            IsPlaying = true;
        }

        private void OnTrackEnded(object? sender, EventArgs e)
        {
            if (_playlist.Count == 0) return;
            if (IsRepeat)
            {
                PlayCurrentTrack();
                return;
            }
            _currentTrackIndex++;
            if (_currentTrackIndex >= _playlist.Count)
            {
                if (_shuffle)
                    _playlist = _playlist.OrderBy(x => _random.Next()).ToList();
                _currentTrackIndex = 0;
            }
            PlayCurrentTrack();
        }

        private void OnTrackMediaOpened(object? sender, EventArgs e)
        {
            UpdateTrackTimes();
            TrackChanged?.Invoke(CurrentTrack, CurrentArtist);
        }

        public void StopMusic()
        {
            _musicPlayer.Stop();
        }

        public void SetMusicEnabled(bool enabled)
        {
            IsMusicEnabled = enabled;
            if (!enabled) StopMusic();
            SaveSettings();
        }

        public void SetMusicVolume(double volume)
        {
            MusicVolume = volume;
            _musicPlayer.Volume = volume / 100.0;
            SaveSettings();
        }

        public void SetSoundEnabled(bool enabled)
        {
            IsSoundEnabled = enabled;
            SaveSettings();
        }

        public void SetSoundVolume(double volume)
        {
            SoundVolume = volume;
            SaveSettings();
        }

        public void PlaySound(string soundFile)
        {
            if (!IsSoundEnabled) return;
            var player = new SoundPlayer(soundFile);
            player.Play();
        }

        public void LoadSettings()
        {
            IsMusicEnabled = Settings.Default.IsMusicEnabled;
            MusicVolume = Settings.Default.MusicVolume;
            IsSoundEnabled = Settings.Default.AreSoundEffectsEnabled;
            SoundVolume = Settings.Default.EffectsVolume;
        }

        public void SaveSettings()
        {
            Settings.Default.IsMusicEnabled = IsMusicEnabled;
            Settings.Default.MusicVolume = MusicVolume;
            Settings.Default.AreSoundEffectsEnabled = IsSoundEnabled;
            Settings.Default.EffectsVolume = SoundVolume;
            Settings.Default.Save();
        }

        public void NextTrack()
        {
            if (_playlist.Count == 0) return;
            _currentTrackIndex++;
            if (_currentTrackIndex >= _playlist.Count)
            {
                if (_shuffle)
                    _playlist = _playlist.OrderBy(x => _random.Next()).ToList();
                _currentTrackIndex = 0;
            }
            PlayCurrentTrack();
        }

        public void PreviousTrack()
        {
            if (_playlist.Count == 0) return;
            _currentTrackIndex--;
            if (_currentTrackIndex < 0)
                _currentTrackIndex = _playlist.Count - 1;
            PlayCurrentTrack();
        }

        public void UpdateTrackTimes()
        {
            if (_musicPlayer.NaturalDuration.HasTimeSpan)
            {
                var dur = _musicPlayer.NaturalDuration.TimeSpan;
                TrackDuration = dur.ToString("m\\:ss");
                TrackDurationSeconds = dur.TotalSeconds;
                var pos = _musicPlayer.Position;
                CurrentTime = pos.ToString("m\\:ss");
                TrackPositionSeconds = pos.TotalSeconds;
            }
            else
            {
                TrackDuration = "0:00";
                TrackDurationSeconds = 0;
                CurrentTime = "0:00";
                TrackPositionSeconds = 0;
            }
        }

        public void Pause()
        {
            _musicPlayer.Pause();
            IsPlaying = false;
        }

        public void Resume()
        {
            _musicPlayer.Play();
            IsPlaying = true;
        }

        public void ToggleShuffle()
        {
            _shuffle = !_shuffle;
        }

        public void ToggleRepeat()
        {
            IsRepeat = !IsRepeat;
        }

        public void Seek(double seconds)
        {
            if (_musicPlayer.NaturalDuration.HasTimeSpan)
            {
                _musicPlayer.Position = TimeSpan.FromSeconds(seconds);
                UpdateTrackTimes();
            }
        }
    }
}