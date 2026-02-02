namespace EF.TPT.Models;

/// <summary>
/// Car entity - represents passenger cars in the vehicle hierarchy.
/// In TPT, Car gets its OWN table called "Cars" containing ONLY car-specific properties.
/// </summary>
/// <remarks>
/// TPT Storage for Car:
/// 1. Base properties (Id, Brand, Model, Year, Price) → Vehicles table
/// 2. Car-specific properties (NumberOfDoors, FuelType) → Cars table
/// 3. Cars.Id is both PK and FK to Vehicles.Id
///
/// Example data flow:
/// INSERT a Car:
///   - First: INSERT INTO Vehicles (Brand, Model, Year, Price) VALUES (...)
///   - Then: INSERT INTO Cars (Id, NumberOfDoors, FuelType) VALUES (...)
///
/// SELECT a Car:
///   - SELECT v.*, c.* FROM Vehicles v INNER JOIN Cars c ON v.Id = c.Id
///
/// Benefits vs TPH:
/// - No NULL values in Cars table (always has NumberOfDoors and FuelType)
/// - Can enforce NOT NULL constraints on car-specific properties
/// - Cars table is narrow and contains only relevant data
/// </remarks>
public class Car : Vehicle
{
    public int NumberOfDoors { get; set; }

    public string FuelType { get; set; } = string.Empty;

    public override string GetDescription()
    {
        return $"{base.GetDescription()} | {NumberOfDoors}-door {FuelType} Car";
    }
}
