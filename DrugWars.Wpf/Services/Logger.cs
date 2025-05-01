using System;
using System.Diagnostics;
using System.Windows;
using DrugWars.Wpf.Windows;

namespace DrugWars.Wpf.Services
{
    public static class Logger
    {
        public static void Initialize()
        {
            Debug.WriteLine($"DrugWars Log - {DateTime.Now}");
        }

        public static void Log(string message)
        {
            string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            Debug.WriteLine($"[{timestamp}] {message}");
        }

        public static void LogError(string message, Exception ex)
        {
            string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            Debug.WriteLine($"[{timestamp}] ERROR: {message}");
            Debug.WriteLine($"Exception: {ex.GetType().Name}");
            Debug.WriteLine($"Message: {ex.Message}");
            Debug.WriteLine($"Stack Trace:\n{ex.StackTrace}");

            // Show error window on UI thread
            Application.Current.Dispatcher.Invoke(() =>
            {
                var errorWindow = new ErrorWindow($"{message}\n\nError: {ex.Message}");
                errorWindow.ShowDialog();
            });
        }
    }
}