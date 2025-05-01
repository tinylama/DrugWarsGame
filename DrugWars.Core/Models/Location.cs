namespace DrugWars.Core.Models;

public class Location
{
    public string Name { get; set; } = string.Empty;
    public decimal TravelCost { get; set; }
    public int CopPresence { get; set; }
    public decimal PriceMultiplier { get; set; } = 1.0m;
}