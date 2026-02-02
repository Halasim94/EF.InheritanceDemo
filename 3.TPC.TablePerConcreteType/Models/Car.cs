namespace EF.TPC.Models;

/// <summary>
/// Car entity - represents passenger cars in the vehicle hierarchy.
/// In TPC, Car is a CONCRETE class and gets its own complete table.
/// </summary>
/// <remarks>
/// TPC Storage for Car:
/// - Creates a "Cars" table with ALL properties (base + car-specific)
/// - NO relationship to a Vehicles table (doesn't exist!)
/// - Table is completely independent
///
/// Cars Table Structure:
/// ┌────┬───────┬───────┬──────┬───────┬──────────────┬──────────┐
/// │ Id │ Brand │ Model │ Year │ Price │ NumberOfDoors│ FuelType │
/// └────┴───────┴───────┴──────┴───────┴──────────────┴──────────┘
///  ↑                                    ↑
///  Base properties (duplicated)        Car-specific properties
///
/// Query Performance:
/// - SELECT FROM Cars WHERE ... → Very fast! Single table, no JOINs
/// - No overhead of joining to a base table
///
/// Trade-offs:
/// ✅ Pros:
///   - Fast queries (no JOINs)
///   - Complete data in one place
///   - Independent from other vehicle types
///
/// ❌ Cons:
///   - Base properties (Brand, Model, Year, Price) duplicated across Cars, Trucks, Motorcycles
///   - Changing a base property requires migration of ALL concrete tables
///   - Polymorphic queries require UNION ALL across all tables
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
