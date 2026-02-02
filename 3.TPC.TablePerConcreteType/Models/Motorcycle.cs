namespace EF.TPC.Models;

/// <summary>
/// Motorcycle entity - represents motorcycles in the vehicle hierarchy.
/// In TPC, Motorcycle is a CONCRETE class and gets its own complete table.
/// </summary>
/// <remarks>
/// TPC Storage for Motorcycle:
/// - Creates a "Motorcycles" table with ALL properties (base + motorcycle-specific)
/// - NO relationship to a Vehicles table (doesn't exist!)
/// - Table is completely independent
///
/// Motorcycles Table Structure:
/// ┌────┬───────┬───────┬──────┬───────┬───────────┬──────────┐
/// │ Id │ Brand │ Model │ Year │ Price │ HasSidecar│ EngineCC │
/// └────┴───────┴───────┴──────┴───────┴───────────┴──────────┘
///  ↑                                    ↑
///  Base properties (duplicated)        Motorcycle-specific properties
///
/// Query Performance:
/// - SELECT FROM Motorcycles WHERE ... → Very fast! Single table, no JOINs
/// - Ideal when you know you're querying motorcycles specifically
///
/// Example Queries:
/// Good (TPC Advantage):
///   context.Motorcycles.Where(m => m.EngineCC > 1000)
///   → SELECT * FROM Motorcycles WHERE EngineCC > 1000 (fast!)
///
/// Acceptable:
///   context.Vehicles.OfType<Motorcycle>().Where(m => m.EngineCC > 1000)
///   → Same query, slightly more verbose in code
///
/// Expensive (TPC Disadvantage):
///   context.Vehicles.Where(v => v.Price > 20000)
///   → UNION ALL across Cars, Trucks, Motorcycles (slower)
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
