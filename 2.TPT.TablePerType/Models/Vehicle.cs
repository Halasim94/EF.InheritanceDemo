namespace EF.TPT.Models;

/// <summary>
/// Base class for all vehicles in the hierarchy.
/// In TPT (Table Per Type), this class gets its OWN table called "Vehicles"
/// containing ONLY the base properties (Id, Brand, Model, Year, Price).
/// </summary>
/// <remarks>
/// Key TPT Concepts for Base Class:
/// - Creates a "Vehicles" table with ONLY base properties
/// - Derived classes have their own tables with their specific properties
/// - Derived tables have a FK relationship to this base table
/// - No discriminator column (type determined by which child table has a row)
/// - All base properties are NOT NULL (no sparse data)
/// </remarks>
public abstract class Vehicle
{
    public int Id { get; set; }

    public string Brand { get; set; } = string.Empty;

    public string Model { get; set; } = string.Empty;

    public int Year { get; set; }

    public decimal Price { get; set; }

    public virtual string GetDescription()
    {
        return $"{Year} {Brand} {Model} - ${Price:N2}";
    }
}
