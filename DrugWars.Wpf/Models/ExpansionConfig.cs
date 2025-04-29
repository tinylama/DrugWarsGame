using System.Collections.Generic;
using DrugWars.Core.Models;

namespace DrugWars.Wpf.Models
{
    public static class ExpansionConfig
    {
        public static List<Location> GetLocations(GameExpansion expansion)
        {
            return expansion switch
            {
                GameExpansion.Original => new List<Location>
                {
                    new Location { Name = "Bronx", TravelCost = 100, PriceMultiplier = 1.0m, CopPresence = 3 },
                    new Location { Name = "Brooklyn", TravelCost = 150, PriceMultiplier = 1.2m, CopPresence = 2 },
                    new Location { Name = "Manhattan", TravelCost = 200, PriceMultiplier = 1.5m, CopPresence = 4 },
                    new Location { Name = "Staten Island", TravelCost = 125, PriceMultiplier = 0.8m, CopPresence = 1 },
                    new Location { Name = "Queens", TravelCost = 175, PriceMultiplier = 1.1m, CopPresence = 2 },
                    new Location { Name = "Central Park", TravelCost = 250, PriceMultiplier = 1.3m, CopPresence = 5 }
                },
                GameExpansion.Australia => new List<Location>
                {
                    new Location { Name = "Kings Cross", TravelCost = 100, PriceMultiplier = 1.0m, CopPresence = 3 },
                    new Location { Name = "Bondi Beach", TravelCost = 150, PriceMultiplier = 1.2m, CopPresence = 2 },
                    new Location { Name = "Melbourne CBD", TravelCost = 200, PriceMultiplier = 1.5m, CopPresence = 4 },
                    new Location { Name = "Gold Coast", TravelCost = 125, PriceMultiplier = 0.8m, CopPresence = 1 },
                    new Location { Name = "Perth", TravelCost = 175, PriceMultiplier = 1.1m, CopPresence = 2 },
                    new Location { Name = "Darwin", TravelCost = 250, PriceMultiplier = 1.3m, CopPresence = 5 }
                },
                GameExpansion.UK => new List<Location>
                {
                    new Location { Name = "Brixton", TravelCost = 100, PriceMultiplier = 1.0m, CopPresence = 3 },
                    new Location { Name = "Manchester", TravelCost = 150, PriceMultiplier = 1.2m, CopPresence = 2 },
                    new Location { Name = "Glasgow", TravelCost = 200, PriceMultiplier = 1.5m, CopPresence = 4 },
                    new Location { Name = "Liverpool", TravelCost = 125, PriceMultiplier = 0.8m, CopPresence = 1 },
                    new Location { Name = "Birmingham", TravelCost = 175, PriceMultiplier = 1.1m, CopPresence = 2 },
                    new Location { Name = "Bristol", TravelCost = 250, PriceMultiplier = 1.3m, CopPresence = 5 }
                },
                GameExpansion.Medellin => new List<Location>
                {
                    new Location { Name = "El Centro", TravelCost = 100, PriceMultiplier = 1.0m, CopPresence = 3 },
                    new Location { Name = "El Poblado", TravelCost = 150, PriceMultiplier = 1.2m, CopPresence = 2 },
                    new Location { Name = "Comuna 13", TravelCost = 200, PriceMultiplier = 1.5m, CopPresence = 4 },
                    new Location { Name = "Laureles", TravelCost = 125, PriceMultiplier = 0.8m, CopPresence = 1 },
                    new Location { Name = "Envigado", TravelCost = 175, PriceMultiplier = 1.1m, CopPresence = 2 },
                    new Location { Name = "Parque Lleras", TravelCost = 250, PriceMultiplier = 1.3m, CopPresence = 5 }
                },
                _ => new List<Location>()
            };
        }

        public static List<Drug> GetDrugs(GameExpansion expansion)
        {
            var drugs = expansion switch
            {
                GameExpansion.Original => new List<Drug>
                {
                    new Drug { Name = "Cocaine", CurrentPrice = 15000, MinPrice = 8000, MaxPrice = 30000 },
                    new Drug { Name = "Heroin", CurrentPrice = 12000, MinPrice = 6000, MaxPrice = 25000 },
                    new Drug { Name = "Acid", CurrentPrice = 1000, MinPrice = 500, MaxPrice = 4000 },
                    new Drug { Name = "Weed", CurrentPrice = 300, MinPrice = 100, MaxPrice = 1000 },
                    new Drug { Name = "Speed", CurrentPrice = 100, MinPrice = 50, MaxPrice = 500 },
                    new Drug { Name = "Ludes", CurrentPrice = 50, MinPrice = 10, MaxPrice = 300 }
                },
                GameExpansion.Australia => new List<Drug>
                {
                    new Drug { Name = "Ice", CurrentPrice = 15000, MinPrice = 8000, MaxPrice = 30000 },
                    new Drug { Name = "Pingas", CurrentPrice = 1000, MinPrice = 500, MaxPrice = 4000 },
                    new Drug { Name = "Rack", CurrentPrice = 12000, MinPrice = 6000, MaxPrice = 25000 },
                    new Drug { Name = "Buds", CurrentPrice = 300, MinPrice = 100, MaxPrice = 1000 },
                    new Drug { Name = "Gear", CurrentPrice = 12000, MinPrice = 6000, MaxPrice = 25000 },
                    new Drug { Name = "Benzos", CurrentPrice = 100, MinPrice = 50, MaxPrice = 500 }
                },
                GameExpansion.UK => new List<Drug>
                {
                    new Drug { Name = "Charlie", CurrentPrice = 15000, MinPrice = 8000, MaxPrice = 30000 },
                    new Drug { Name = "Mandy", CurrentPrice = 1500, MinPrice = 800, MaxPrice = 5000 },
                    new Drug { Name = "Brown", CurrentPrice = 12000, MinPrice = 6000, MaxPrice = 25000 },
                    new Drug { Name = "Draw", CurrentPrice = 300, MinPrice = 100, MaxPrice = 1000 },
                    new Drug { Name = "Pills", CurrentPrice = 1000, MinPrice = 500, MaxPrice = 4000 },
                    new Drug { Name = "Ket", CurrentPrice = 2000, MinPrice = 1000, MaxPrice = 6000 }
                },
                GameExpansion.Medellin => new List<Drug>
                {
                    new Drug { Name = "Perico", CurrentPrice = 15000, MinPrice = 8000, MaxPrice = 30000 },
                    new Drug { Name = "Cripy", CurrentPrice = 500, MinPrice = 200, MaxPrice = 1500 },
                    new Drug { Name = "Tussi", CurrentPrice = 2000, MinPrice = 1000, MaxPrice = 6000 },
                    new Drug { Name = "Morado", CurrentPrice = 1000, MinPrice = 500, MaxPrice = 4000 },
                    new Drug { Name = "Pepas", CurrentPrice = 1000, MinPrice = 500, MaxPrice = 4000 },
                    new Drug { Name = "Basuco", CurrentPrice = 100, MinPrice = 50, MaxPrice = 500 }
                },
                _ => new List<Drug>()
            };
            
            // Initialize price history for all drugs
            foreach (var drug in drugs)
            {
                drug.InitializeHistory();
                
                // Force the price history to contain the initial price
                if (!drug.PriceHistory.Any())
                {
                    // We need to use reflection because AddPriceToHistory is private
                    var priceHistoryMethod = drug.GetType().GetMethod(
                        "AddPriceToHistory", 
                        System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    
                    if (priceHistoryMethod != null)
                    {
                        priceHistoryMethod.Invoke(drug, new object[] { drug.CurrentPrice });
                        System.Diagnostics.Debug.WriteLine($"Force-initialized price history for {drug.Name}");
                    }
                }
            }
            
            return drugs;
        }

        public static string GetWelcomeMessage(GameExpansion expansion)
        {
            return expansion switch
            {
                GameExpansion.Original => "Welcome to Drug Wars! You have 30 days to make as much money as possible.",
                GameExpansion.Australia => "G'day and welcome to Drug Wars Down Under! You've got 30 days to make a motza, mate!",
                GameExpansion.UK => "Oi mate! Welcome to Drug Wars UK! You've got 30 days to become proper minted!",
                GameExpansion.Medellin => "¡Bienvenidos a Drug Wars! You have 30 days to become the king of El Dorado!",
                _ => string.Empty
            };
        }

        public static string GetCopMessage(GameExpansion expansion)
        {
            return expansion switch
            {
                GameExpansion.Original => "You got caught by the cops!",
                GameExpansion.Australia => "Crikey! The coppers nabbed ya!",
                GameExpansion.UK => "Bloody hell! The Old Bill's nicked you!",
                GameExpansion.Medellin => "¡La policía! The cops caught you, parcero!",
                _ => string.Empty
            };
        }

        public static string GetNoMoneyMessage(GameExpansion expansion)
        {
            return expansion switch
            {
                GameExpansion.Original => "You don't have enough cash!",
                GameExpansion.Australia => "You're a bit short on the dosh there, mate!",
                GameExpansion.UK => "You're skint mate! No dosh!",
                GameExpansion.Medellin => "¡No hay plata! You're out of cash, parcero!",
                _ => string.Empty
            };
        }

        public static string GetNoSpaceMessage(GameExpansion expansion)
        {
            return expansion switch
            {
                GameExpansion.Original => "Not enough space in inventory!",
                GameExpansion.Australia => "Your stash spot's chockers, mate!",
                GameExpansion.UK => "Your stash is packed mate, no room!",
                GameExpansion.Medellin => "¡No hay espacio! Your caleta is full!",
                _ => string.Empty
            };
        }
    }
} 