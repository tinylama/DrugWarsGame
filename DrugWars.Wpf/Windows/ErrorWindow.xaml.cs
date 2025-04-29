using System.Windows;

namespace DrugWars.Wpf.Windows
{
    public partial class ErrorWindow : GameWindowBase
    {
        public ErrorWindow(string message)
        {
            InitializeComponent();
            MessageText.Text = message;
        }

        private void OnOkClick(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
} 