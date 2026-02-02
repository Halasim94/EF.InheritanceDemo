namespace EF.TPH.Models;

/// <summary>
/// Truck entity - represents commercial trucks in the vehicle hierarchy.
/// In TPH, Truck-specific properties (LoadCapacity, NumberOfAxles) are added as
/// NULLABLE columns in the single Vehicles table.
/// </summary>
/// <remarks>
/// TPH Storage for Truck:
/// - Discriminator column will contain "Truck"
/// - LoadCapacity and NumberOfAxles will have values
/// - Car properties (NumberOfDoors, FuelType) will be NULL
/// - Motorcycle properties (HasSidecar, EngineCC) will be NULL
/// </remarks>
public class Truck : Vehicle
{
    public decimal LoadCapacity { get; set; }

    public int NumberOfAxles { get; set; }

    public override string GetDescription()
    {
        return $"{base.GetDescription()} | {LoadCapacity}t capacity, {NumberOfAxles}-axle Truck";
    }
}
