namespace EF.TPC.Models;

/// <summary>
/// Base class for all vehicles in the hierarchy.
/// In TPC (Table Per Concrete Type), this ABSTRACT class does NOT get a database table!
/// </summary>
/// <remarks>
/// Key TPC Concepts for Base Class:
/// - NO "Vehicles" table is created
/// - Base properties (Id, Brand, Model, Year, Price) are DUPLICATED in each concrete table
/// - Only concrete derived classes (Car, Truck, Motorcycle) get tables
/// - Each concrete table contains ALL properties (base + derived)
/// - No discriminator column needed (type determined by which table contains the row)
/// - No foreign key relationships between tables
///
/// TPC Database Structure:
/// ❌ Vehicles table - DOES NOT EXIST
/// ✅ Cars table - Contains: Id, Brand, Model, Year, Price, NumberOfDoors, FuelType
/// ✅ Trucks table - Contains: Id, Brand, Model, Year, Price, LoadCapacity, NumberOfAxles
/// ✅ Motorcycles table - Contains: Id, Brand, Model, Year, Price, HasSidecar, EngineCC
///
/// Notice: Brand, Model, Year, Price appear in EVERY concrete table (data duplication)
/// </remarks>
public abstract class Vehicle
{
    /// <summary>
    /// Primary key - DUPLICATED in each concrete table.
    /// EF Core ensures IDs are unique across all tables using a shared sequence.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Vehicle brand (e.g., "Volkswagen", "BMW", "Mercedes-Benz").
    /// DUPLICATED in Cars, Trucks, and Motorcycles tables.
    /// </summary>
    public string Brand { get; set; } = string.Empty;

    /// <summary>
    /// Vehicle model (e.g., "Golf", "R 1250 GS", "Actros").
    /// DUPLICATED in Cars, Trucks, and Motorcycles tables.
    /// </summary>
    public string Model { get; set; } = string.Empty;

    /// <summary>
    /// Manufacturing year.
    /// DUPLICATED in Cars, Trucks, and Motorcycles tables.
    /// </summary>
    public int Year { get; set; }

    /// <summary>
    /// Price in dollars.
    /// DUPLICATED in Cars, Trucks, and Motorcycles tables.
    /// </summary>
    public decimal Price { get; set; }

    /// <summary>
    /// Virtual method that can be overridden by derived classes.
    /// Demonstrates polymorphic behavior in TPC.
    /// </summary>
    public virtual string GetDescription()
    {
        return $"{Year} {Brand} {Model} - ${Price:N2}";
    }
}
