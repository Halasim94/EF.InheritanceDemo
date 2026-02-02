namespace EF.TPC.Models;

/// <summary>
/// Truck entity - represents commercial trucks in the vehicle hierarchy.
/// In TPC, Truck is a CONCRETE class and gets its own complete table.
/// </summary>
/// <remarks>
/// TPC Storage for Truck:
/// - Creates a "Trucks" table with ALL properties (base + truck-specific)
/// - NO relationship to a Vehicles table (doesn't exist!)
/// - Table is completely independent
///
/// Trucks Table Structure:
/// ┌────┬───────┬───────┬──────┬───────┬──────────────┬──────────────┐
/// │ Id │ Brand │ Model │ Year │ Price │ LoadCapacity │ NumberOfAxles│
/// └────┴───────┴───────┴──────┴───────┴──────────────┴──────────────┘
///  ↑                                    ↑
///  Base properties (duplicated)        Truck-specific properties
///
/// TPC Advantages for Trucks:
/// 1. Fast Queries: SELECT * FROM Trucks WHERE LoadCapacity > 10
///    → No JOINs, direct table access
///
/// 2. Independence: Trucks table evolution doesn't affect Cars or Motorcycles
///    → Can add truck-specific indexes, constraints, partitions
///
/// 3. Complete Data: All truck information in one row
///    → Easy to export, backup, or analyze truck data independently
///
/// TPC Disadvantages for Trucks:
/// 1. Duplication: Brand, Model, Year, Price also in Cars and Motorcycles tables
///
/// 2. Schema Changes: Adding a base property (e.g., "Color") requires:
///    → ALTER TABLE Cars ADD COLUMN Color
///    → ALTER TABLE Trucks ADD COLUMN Color
///    → ALTER TABLE Motorcycles ADD COLUMN Color
///
/// 3. Polymorphic Queries: "All vehicles over $100k" requires:
///    → SELECT ... FROM Cars WHERE Price > 100000
///    → UNION ALL
///    → SELECT ... FROM Trucks WHERE Price > 100000
///    → UNION ALL
///    → SELECT ... FROM Motorcycles WHERE Price > 100000
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
