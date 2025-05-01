using System.Windows;
using DrugWars.Core.Models;

namespace DrugWars.Wpf.Windows
{
    public partial class LoanSharkIntroWindow : GameWindowBase
    {
        public GameExpansion Expansion { get; }

        public LoanSharkIntroWindow(GameExpansion expansion)
        {
            Expansion = expansion;
            InitializeComponent();
            SetIntroText();
        }

        private void OnOkClick(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }

        private void SetIntroText()
        {
            string text = Expansion switch
            {
                GameExpansion.Original => "Listen up, rookie. The streets are rough, and the fuzz are everywhere. Don't get pinched, and don't stiff me on my cash.",
                GameExpansion.Australia => "Oi, mate! Welcome to the Aussie underworld. Watch out for the coppers and keep your stash hidden, yeah? No funny business with my dosh.",
                GameExpansion.UK => "Alright, geezer. London's a jungle. Mind the bobbies and pay your debts, or you'll be in a right mess.",
                GameExpansion.Medellin => "Bienvenido, amigo. In Medellín, you play with fire. The cartel doesn't forgive, and neither do I. Paga tu plata, rápido.",
                _ => "Welcome to the game. Don't mess up."
            };
            // Assume there is a TextBlock named IntroTextBlock in the XAML
            IntroTextBlock.Text = text;
        }
    }
}