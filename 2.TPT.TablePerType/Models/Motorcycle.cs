namespace EF.TPT.Models;

/// <summary>
/// Motorcycle entity - represents motorcycles in the vehicle hierarchy.
/// In TPT, Motorcycle gets its OWN table called "Motorcycles" containing ONLY motorcycle-specific properties.
/// </summary>
/// <remarks>
/// TPT Storage for Motorcycle:
/// 1. Base properties (Id, Brand, Model, Year, Price) → Vehicles table
/// 2. Motorcycle-specific properties (HasSidecar, EngineCC) → Motorcycles table
/// 3. Motorcycles.Id is both PK and FK to Vehicles.Id
///
/// Example data flow:
/// INSERT a Motorcycle:
///   - First: INSERT INTO Vehicles (Brand, Model, Year, Price) VALUES (...)
///   - Then: INSERT INTO Motorcycles (Id, HasSidecar, EngineCC) VALUES (...)
///
/// SELECT a Motorcycle:
///   - SELECT v.*, m.* FROM Vehicles v INNER JOIN Motorcycles m ON v.Id = m.Id
///
/// Benefits vs TPH:
/// - No wasted columns for car or truck properties
/// - Motorcycles table is clean and focused
/// - Can enforce business rules at database level (e.g., EngineCC > 0)
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
