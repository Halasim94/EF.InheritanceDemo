namespace EF.TPH.Models;

/// <summary>
/// Base class for all vehicles in the hierarchy.
/// In TPH (Table Per Hierarchy), this class and all its derived classes
/// will be stored in a SINGLE database table called "Vehicles".
/// </summary>
/// <remarks>
/// Key TPH Concepts:
/// - All properties from this base class appear in the single table
/// - A "Discriminator" column is automatically added to identify the type
/// - All derived class properties are added as NULLABLE columns
/// - No configuration needed - TPH is the default EF Core strategy
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
