using System;
using System.IO;

namespace DrugWars.Wpf.Services
{
    public static class Logger
    {
        private static string _logFilePath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
            "DrugWars_Log.txt");

        public static void Initialize()
        {
            // Create or clear the log file
            File.WriteAllText(_logFilePath, $"DrugWars Log - {DateTime.Now}\n");
        }

        public static void Log(string message)
        {
            try
            {
                string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                string logMessage = $"[{timestamp}] {message}\n";
                File.AppendAllText(_logFilePath, logMessage);
            }
            catch (Exception ex)
            {
                // If we can't log, there's not much we can do
                System.Diagnostics.Debug.WriteLine($"Failed to log message: {ex.Message}");
            }
        }

        public static void LogError(string message, Exception ex)
        {
            try
            {
                string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                string logMessage = $"[{timestamp}] ERROR: {message}\n";
                logMessage += $"Exception: {ex.GetType().Name}\n";
                logMessage += $"Message: {ex.Message}\n";
                logMessage += $"Stack Trace:\n{ex.StackTrace}\n";
                File.AppendAllText(_logFilePath, logMessage);
            }
            catch (Exception logEx)
            {
                // If we can't log, there's not much we can do
                System.Diagnostics.Debug.WriteLine($"Failed to log error: {logEx.Message}");
            }
        }
    }
} 