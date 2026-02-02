namespace EF.TPT.Models;

/// <summary>
/// Truck entity - represents commercial trucks in the vehicle hierarchy.
/// In TPT, Truck gets its OWN table called "Trucks" containing ONLY truck-specific properties.
/// </summary>
/// <remarks>
/// TPT Storage for Truck:
/// 1. Base properties (Id, Brand, Model, Year, Price) → Vehicles table
/// 2. Truck-specific properties (LoadCapacity, NumberOfAxles) → Trucks table
/// 3. Trucks.Id is both PK and FK to Vehicles.Id
///
/// Example data flow:
/// INSERT a Truck:
///   - First: INSERT INTO Vehicles (Brand, Model, Year, Price) VALUES (...)
///   - Then: INSERT INTO Trucks (Id, LoadCapacity, NumberOfAxles) VALUES (...)
///
/// SELECT a Truck:
///   - SELECT v.*, t.* FROM Vehicles v INNER JOIN Trucks t ON v.Id = t.Id
///
/// UPDATE a Truck:
///   - May update both tables:
///     UPDATE Vehicles SET Price = ... WHERE Id = ...
///     UPDATE Trucks SET LoadCapacity = ... WHERE Id = ...
///
/// DELETE a Truck:
///   - Cascade delete from both tables (FK constraint handles this)
///
/// Benefits vs TPH:
/// - Trucks table contains only relevant truck data
/// - No NULL values for car or motorcycle properties
/// - Can enforce truck-specific constraints (e.g., LoadCapacity > 0)
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
