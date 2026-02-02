namespace EF.TPH.Models;

/// <summary>
/// Motorcycle entity - represents motorcycles in the vehicle hierarchy.
/// In TPH, Motorcycle-specific properties (HasSidecar, EngineCC) are added as
/// NULLABLE columns in the single Vehicles table.
/// </summary>
/// <remarks>
/// TPH Storage for Motorcycle:
/// - Discriminator column will contain "Motorcycle"
/// - HasSidecar and EngineCC will have values
/// - Car properties (NumberOfDoors, FuelType) will be NULL
/// - Truck properties (LoadCapacity, NumberOfAxles) will be NULL
/// </remarks>
public class Motorcycle : Vehicle
{
    public bool HasSidecar { get; set; }

    public int EngineCC { get; set; }

    public override string GetDescription()
    {
        var sidecar = HasSidecar ? "with sidecar" : "no sidecar";
        return $"{base.GetDescription()} | {EngineCC}cc Motorcycle ({sidecar})";
    }
}
