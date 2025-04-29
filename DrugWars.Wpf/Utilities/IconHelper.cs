using System;
using System.IO;
using System.Windows;
using System.Windows.Media.Imaging;

namespace DrugWars.Wpf.Utilities
{
    public static class IconHelper
    {
        private static BitmapImage? _iconImage;

        public static BitmapImage? GetApplicationIcon()
        {
            if (_iconImage != null)
                return _iconImage;

            try
            {
                string iconPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "images", "DrugWars.ico");
                if (File.Exists(iconPath))
                {
                    _iconImage = new BitmapImage();
                    _iconImage.BeginInit();
                    _iconImage.UriSource = new Uri(iconPath);
                    _iconImage.CacheOption = BitmapCacheOption.OnLoad;
                    _iconImage.EndInit();
                    _iconImage.Freeze(); // Important for cross-thread usage
                    return _iconImage;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading icon: {ex.Message}");
            }

            return null;
        }

        public static void SetWindowIcon(Window window)
        {
            var icon = GetApplicationIcon();
            if (icon != null)
            {
                window.Icon = icon;
            }
        }
    }
} 