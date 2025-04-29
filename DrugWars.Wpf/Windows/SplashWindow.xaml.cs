using System.ComponentModel;
using System.Windows;
using DrugWars.Core.Models;
using System.Windows.Input;

namespace DrugWars.Wpf.Windows
{
    /// <summary>
    /// Interaction logic for SplashWindow.xaml
    /// </summary>
    public partial class SplashWindow : GameWindowBase
    {
        public GameExpansion? SelectedExpansion { get; private set; }
        public event EventHandler<GameExpansion>? ExpansionSelected;

        public SplashWindow()
        {
            InitializeComponent();

            if (DesignerProperties.GetIsInDesignMode(this))
            {
                // Design-time initialization
                return;
            }

            CommandBindings.Add(new CommandBinding(System.Windows.Input.ApplicationCommands.Close, (s, e) => Application.Current.Shutdown()));
        }

        private void OnOriginalClick(object sender, RoutedEventArgs e)
        {
            ExpansionSelected?.Invoke(this, GameExpansion.Original);
        }

        private void OnAustraliaClick(object sender, RoutedEventArgs e)
        {
            ExpansionSelected?.Invoke(this, GameExpansion.Australia);
        }

        private void OnUKClick(object sender, RoutedEventArgs e)
        {
            ExpansionSelected?.Invoke(this, GameExpansion.UK);
        }

        private void OnMedellinClick(object sender, RoutedEventArgs e)
        {
            ExpansionSelected?.Invoke(this, GameExpansion.Medellin);
        }

        private void OnExitClick(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
    }
} 