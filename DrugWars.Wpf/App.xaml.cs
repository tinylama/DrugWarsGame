using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Threading;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using DrugWars.Wpf.Utilities;
using DrugWars.Wpf.Models;
using DrugWars.Core.Models;
using DrugWars.Wpf.Services;
using DrugWars.Wpf.Windows;
using DrugWars.Wpf.Properties;

namespace DrugWars.Wpf;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    private static readonly string _logFilePath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "DrugWars",
        "error_log.txt");

    private readonly TextWriterTraceListener? _traceListener;

    public App()
    {
        // Set up debug logging to file
        string logPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "debug_log.txt");
        _traceListener = new(logPath, "DrugWarsTraceListener");
        Trace.Listeners.Add(_traceListener);
        Trace.AutoFlush = true;

        InitializeErrorLogging();
        AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
        DispatcherUnhandledException += App_DispatcherUnhandledException;
        TaskScheduler.UnobservedTaskException += TaskScheduler_UnobservedTaskException;
    }

    private static void InitializeErrorLogging()
    {
        try
        {
            // Ensure directory exists
            string? logDirectory = Path.GetDirectoryName(_logFilePath);
            if (!string.IsNullOrEmpty(logDirectory) && !Directory.Exists(logDirectory))
            {
                Directory.CreateDirectory(logDirectory);
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Failed to initialize error logging: {ex.Message}");
        }
    }

    private static void LogError(string errorMessage)
    {
        try
        {
            // Append to log file
            File.AppendAllText(_logFilePath,
                $"[{DateTime.Now}] {errorMessage}{Environment.NewLine}{Environment.NewLine}");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Failed to log error: {ex.Message}");
        }
    }

    private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
        Debug.WriteLine($"=== Unhandled Domain Exception ===");
        Debug.WriteLine($"Time: {DateTime.Now}");
        if (e.ExceptionObject is Exception ex)
        {
            Debug.WriteLine($"Exception: {ex}");
            Debug.WriteLine($"Stack Trace: {ex.StackTrace}");
        }
        _traceListener?.Flush();
    }

    private void App_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
    {
        Debug.WriteLine($"=== Unhandled UI Exception ===");
        Debug.WriteLine($"Time: {DateTime.Now}");
        Debug.WriteLine($"Exception: {e.Exception}");
        Debug.WriteLine($"Stack Trace: {e.Exception.StackTrace}");
        _traceListener?.Flush();
    }

    private void TaskScheduler_UnobservedTaskException(object? sender, UnobservedTaskExceptionEventArgs e)
    {
        HandleException(e.Exception.InnerException, "TaskScheduler.UnobservedTaskException");
        e.SetObserved(); // Prevent app from crashing
    }

    private void HandleException(Exception? exception, string source)
    {
        if (exception is null) return;

        StringBuilder sb = new StringBuilder();
        sb.AppendLine($"Source: {source}");
        sb.AppendLine($"Exception Type: {exception.GetType().FullName}");
        sb.AppendLine($"Message: {exception.Message}");
        sb.AppendLine($"StackTrace: {exception.StackTrace}");

        if (exception.InnerException != null)
        {
            sb.AppendLine("Inner Exception:");
            sb.AppendLine($"Type: {exception.InnerException.GetType().FullName}");
            sb.AppendLine($"Message: {exception.InnerException.Message}");
            sb.AppendLine($"StackTrace: {exception.InnerException.StackTrace}");
        }

        string errorMessage = sb.ToString();
        Logger.Log(errorMessage);
        LogError(errorMessage);

        // Show a user-friendly message
        if (Application.Current != null)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                MessageBox.Show(
                    $"An unexpected error occurred. The error has been logged to your Desktop (DrugWars_Log.txt).\n\nError: {exception.Message}",
                    "Application Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            });
        }
    }

    protected override void OnStartup(StartupEventArgs e)
    {
        ShutdownMode = ShutdownMode.OnLastWindowClose;
        Debug.WriteLine("OnStartup called");
        base.OnStartup(e);
        Debug.WriteLine("Startup: base.OnStartup complete");
        // Apply saved theme before showing any windows
        string savedTheme = DrugWars.Wpf.Properties.Settings.Default.Theme ?? "Retro (Green/Black)";
        Debug.WriteLine($"Startup: Applying theme {savedTheme}");
        ApplyTheme(savedTheme);
        Debug.WriteLine("Startup: Theme applied");
        // Start music automatically at a quiet volume
        try
        {
            double vol = DrugWars.Wpf.Properties.Settings.Default.MusicVolume;
            if (vol <= 0) vol = 5;
            AudioManager.Instance.SetMusicVolume(vol);
            string musicFolder = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "background_music");
            AudioManager.Instance.PlayMusic(musicFolder);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error starting music on startup: {ex.Message}");
        }
        Debug.WriteLine("=== Application Starting ===");
        Debug.WriteLine($"Time: {DateTime.Now}");
        Debug.WriteLine($"Log Path: {_traceListener?.Name}");
        try
        {
            Logger.Initialize();
            Logger.Log("Application starting");
            Logger.Log("Opening SplashWindow to select expansion");
            Debug.WriteLine("Startup: Creating SplashWindow");
            var splashWindow = new SplashWindow();
            IconHelper.SetWindowIcon(splashWindow);
            Debug.WriteLine("Startup: SplashWindow created");
            splashWindow.ExpansionSelected += (sender, selectedExpansion) =>
            {
                Debug.WriteLine($"Expansion selected: {selectedExpansion}");
                splashWindow.Hide();
                Debug.WriteLine("Startup: Showing LoanSharkIntroWindow");
                var loanSharkIntro = new Windows.LoanSharkIntroWindow(selectedExpansion);
                IconHelper.SetWindowIcon(loanSharkIntro);
                loanSharkIntro.ShowDialog();
                Debug.WriteLine("Startup: LoanSharkIntroWindow closed");
                Logger.Log($"Selected expansion: {selectedExpansion}");
                Debug.WriteLine($"Startup: Selected expansion: {selectedExpansion}");
                try
                {
                    Debug.WriteLine("Startup: Creating GameEngine");
                    var gameEngine = new GameEngine(selectedExpansion)
                    {
                        Player = new Player(),
                        Locations = ExpansionConfig.GetLocations(selectedExpansion),
                        Drugs = ExpansionConfig.GetDrugs(selectedExpansion)
                    };
                    Debug.WriteLine("Startup: GameEngine created");
                    gameEngine.Player.Cash = 5000;
                    gameEngine.Player.Debt = 5000;
                    Logger.Log("GameEngine initialized successfully");
                    Debug.WriteLine("Startup: GameEngine initialized successfully");
                    gameEngine.GameEventOccurred += (s, args) =>
                    {
                        if (args is GameEventArgs gameArgs)
                        {
                            Logger.Log($"Game event: {gameArgs.Message}");
                            Debug.WriteLine($"Game event: {gameArgs.Message}");

                            // Additional check for day 30
                            if (gameEngine.Player.Day >= 30)
                            {
                                Logger.Log("Day 30 reached - enforcing game over");
                                // Show game over message
                                Application.Current.Dispatcher.Invoke(() =>
                                {
                                    MessageBox.Show("You've reached day 30. Time to retire!", "Game Over", MessageBoxButton.OK, MessageBoxImage.Information);
                                    // Handle automatic shutdown and cleanup
                                    Shutdown();
                                });
                            }
                        }
                    };
                    gameEngine.GameOver += (s, args) =>
                    {
                        if (args is GameEventArgs gameArgs)
                        {
                            Logger.Log($"Game over: {gameArgs.Message}");
                            Debug.WriteLine($"Game over: {gameArgs.Message}");
                            MessageBox.Show(gameArgs.Message, "Game Over", MessageBoxButton.OK, MessageBoxImage.Information);
                            Shutdown();
                        }
                    };
                    Logger.Log("Creating MainWindow");
                    Debug.WriteLine("Startup: Creating MainWindow");
                    var mainWindow = new Windows.MainWindow(gameEngine);
                    IconHelper.SetWindowIcon(mainWindow);
                    Debug.WriteLine("Startup: MainWindow created");
                    Logger.Log("Showing MainWindow");
                    Debug.WriteLine("Startup: Setting MainWindow property");
                    MainWindow = mainWindow;
                    Debug.WriteLine("Startup: Showing MainWindow");
                    mainWindow.Show();
                    Debug.WriteLine("Startup: MainWindow shown");
                    mainWindow.Closed += (s, e) =>
                    {
                        Debug.WriteLine("MainWindow closed, returning to splash window");
                        splashWindow.Show();
                    };
                }
                catch (Exception ex)
                {
                    Logger.LogError("GameEngine initialization", ex);
                    Debug.WriteLine($"Startup: Error initializing GameEngine: {ex.Message}");
                    Debug.WriteLine($"Startup: Stack Trace: {ex.StackTrace}");
                    MessageBox.Show($"An error occurred initializing the game: {ex.Message}",
                        "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    Shutdown(-1);
                }
            };
            splashWindow.Closed += (s, e) =>
            {
                Debug.WriteLine("SplashWindow closed, exiting application");
                Shutdown();
            };
            splashWindow.Show();
            Debug.WriteLine("Startup: SplashWindow shown");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Startup: Critical error during startup: {ex.Message}");
            Debug.WriteLine($"Startup: Stack Trace: {ex.StackTrace}");
            MessageBox.Show($"An error occurred during startup: {ex.Message}",
                "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            Shutdown(-1);
        }
        Debug.WriteLine("OnStartup completed");
    }

    private void SetApplicationIcon()
    {
        // This method is no longer needed
    }

    protected override void OnExit(ExitEventArgs e)
    {
        Debug.WriteLine("=== Application Exiting ===");
        Debug.WriteLine($"Time: {DateTime.Now}");
        Debug.WriteLine($"Exit Code: {e.ApplicationExitCode}");
        _traceListener?.Flush();
        _traceListener?.Close();
        base.OnExit(e);
    }

    protected override void OnSessionEnding(SessionEndingCancelEventArgs e)
    {
        Debug.WriteLine("OnSessionEnding called");
        e.Cancel = true;
        base.OnSessionEnding(e);
    }

    public void ApplyTheme(string theme)
    {
        string themeFile = theme switch
        {
            "Retro (Green/Black)" => "Themes/RetroTheme.xaml",
            "Classic (White/Blue)" => "Themes/ClassicTheme.xaml",
            "Modern (Dark)" => "Themes/ModernTheme.xaml",
            "Happy (Colorful)" => "Themes/HappyTheme.xaml",
            _ => "Themes/RetroTheme.xaml"
        };

        // Remove previous theme dictionaries
        var dictionariesToRemove = Resources.MergedDictionaries
            .Where(d => d.Source != null && d.Source.OriginalString.Contains("Theme.xaml"))
            .ToList();
        foreach (var dict in dictionariesToRemove)
            Resources.MergedDictionaries.Remove(dict);

        // Add the new theme dictionary
        var themeDict = new ResourceDictionary { Source = new Uri(themeFile, UriKind.Relative) };
        Resources.MergedDictionaries.Add(themeDict);
    }
}

