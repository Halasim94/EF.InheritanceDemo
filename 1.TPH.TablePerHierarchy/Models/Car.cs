namespace EF.TPH.Models;

/// <summary>
/// Car entity - represents passenger cars in the vehicle hierarchy.
/// In TPH, Car-specific properties (NumberOfDoors, FuelType) are added as
/// NULLABLE columns in the single Vehicles table.
/// </summary>
/// <remarks>
/// TPH Storage for Car:
/// - Discriminator column will contain "Car"
/// - NumberOfDoors and FuelType will have values
/// - Truck properties (LoadCapacity, NumberOfAxles) will be NULL
/// - Motorcycle properties (HasSidecar, EngineCC) will be NULL
///
/// This demonstrates the "sparse data" characteristic of TPH - many NULL values
/// when a table contains multiple types with different properties.
/// </remarks>
public class Car : Vehicle
{
    /// <summary>
    /// Number of doors (typically 2, 4, or 5)
    /// In the database: NULLABLE integer column in Vehicles table
    /// Only populated when Discriminator = "Car"
    /// </summary>
    public int NumberOfDoors { get; set; }

    /// <summary>
    /// Fuel type (e.g., "Gasoline", "Diesel", "Electric", "Hybrid")
    /// In the database: NULLABLE string column in Vehicles table
    /// Only populated when Discriminator = "Car"
    /// </summary>
    public string FuelType { get; set; } = string.Empty;

    /// <summary>
    /// Overrides base method to include car-specific information
    /// Demonstrates polymorphic behavior in TPH
    /// </summary>
    public override string GetDescription()
    {
        return $"{base.GetDescription()} | {NumberOfDoors}-door {FuelType} Car";
    }
}
