using System.Windows;
using System.Windows.Controls;

namespace DrugWars.Wpf.Windows
{
    public partial class PlayerChoiceDialog : Window
    {
        public string SelectedOption { get; private set; } = string.Empty;
        public PlayerChoiceDialog(string message, string[] options)
        {
            InitializeComponent();
            MessageText.Text = message;
            foreach (var option in options)
            {
                var btn = new Button
                {
                    Content = option,
                    Margin = new Thickness(10, 0, 10, 0),
                    Padding = new Thickness(16, 8, 16, 8),
                    FontSize = 16,
                    FontFamily = MessageText.FontFamily,
                    Foreground = MessageText.Foreground,
                    Background = MessageText.Background,
                    BorderBrush = MessageText.Foreground
                };
                btn.Click += (s, e) =>
                {
                    SelectedOption = option;
                    DialogResult = true;
                    Close();
                };
                ButtonPanel.Children.Add(btn);
            }
        }
    }
} 