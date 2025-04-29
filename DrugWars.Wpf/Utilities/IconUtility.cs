using System;
using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace DrugWars.Wpf.Utilities
{
    public static class IconUtility
    {
        public static void SaveAsIcon(DrawingImage drawingImage, string filePath, int size = 256)
        {
            try
            {
                // Create a drawing visual
                DrawingVisual drawingVisual = new DrawingVisual();
                
                // Get the drawing context
                using (DrawingContext drawingContext = drawingVisual.RenderOpen())
                {
                    drawingContext.DrawImage(drawingImage, new Rect(0, 0, size, size));
                }
                
                // Render the visual to a bitmap
                RenderTargetBitmap bmp = new RenderTargetBitmap(size, size, 96, 96, PixelFormats.Pbgra32);
                bmp.Render(drawingVisual);
                
                // Encode the bitmap as PNG
                PngBitmapEncoder encoder = new PngBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(bmp));
                
                // Save the PNG
                using (FileStream fs = new FileStream(filePath, FileMode.Create))
                {
                    encoder.Save(fs);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving icon: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        
        public static BitmapImage XamlToBitmapImage(DrawingImage drawingImage, int size = 256)
        {
            try
            {
                // Create a drawing visual
                DrawingVisual drawingVisual = new DrawingVisual();
                
                // Get the drawing context
                using (DrawingContext drawingContext = drawingVisual.RenderOpen())
                {
                    drawingContext.DrawImage(drawingImage, new Rect(0, 0, size, size));
                }
                
                // Render the visual to a bitmap
                RenderTargetBitmap bmp = new RenderTargetBitmap(size, size, 96, 96, PixelFormats.Pbgra32);
                bmp.Render(drawingVisual);
                
                // Convert to BitmapImage
                BitmapImage bitmapImage = new BitmapImage();
                PngBitmapEncoder encoder = new PngBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(bmp));
                
                using (MemoryStream ms = new MemoryStream())
                {
                    encoder.Save(ms);
                    ms.Position = 0;
                    
                    bitmapImage.BeginInit();
                    bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                    bitmapImage.StreamSource = ms;
                    bitmapImage.EndInit();
                    bitmapImage.Freeze(); // Important for cross-thread access
                }
                
                return bitmapImage;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error converting icon: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return new BitmapImage();
            }
        }
    }
} 