using EF.TPC.Data;
using EF.TPC.Models;
using Microsoft.EntityFrameworkCore;

// ============================================================
// TABLE PER CONCRETE TYPE (TPC) DEMONSTRATION
// ============================================================
// This demo shows how TPC inheritance mapping works in EF Core.
// Only concrete classes get tables - no base table, no foreign keys.
// ============================================================

Console.WriteLine("╔════════════════════════════════════════════════════════════════╗");
Console.WriteLine("║  Entity Framework Core - Table Per Concrete Type (TPC) Demo   ║");
Console.WriteLine("╚════════════════════════════════════════════════════════════════╝");
Console.WriteLine();

// ============================================================
// CONFIGURATION - SQLite database file
// ============================================================
const string connectionString = "Data Source=EFInheritance_TPC.db";
Console.WriteLine("📊 Connection: EFInheritance_TPC.db (SQLite)");
Console.WriteLine();

// ============================================================
// INITIALIZE DATABASE
// ============================================================
Console.WriteLine("🔧 Initializing database...");
using (var context = new VehicleDbContext(connectionString))
{
    // Ensure database is created with only concrete tables
    await context.Database.EnsureCreatedAsync();
    Console.WriteLine("✅ Database created/verified");
    Console.WriteLine("   Tables created:");
    Console.WriteLine("   ❌ Vehicles (does NOT exist - no base table!)");
    Console.WriteLine("   ✅ Cars (complete data: base + car properties)");
    Console.WriteLine("   ✅ Motorcycles (complete data: base + motorcycle properties)");
    Console.WriteLine("   ✅ Trucks (complete data: base + truck properties)");
    Console.WriteLine();
    Console.WriteLine("   Key: Each table is independent with ALL properties!");
    Console.WriteLine();
}

// ============================================================
// DEMONSTRATION 1: View Seeded Data (Polymorphic Query)
// ============================================================
Console.WriteLine("═══════════════════════════════════════════════════════════════");
Console.WriteLine("📋 DEMONSTRATION 1: Querying ALL Vehicles (Polymorphic Query)");
Console.WriteLine("═══════════════════════════════════════════════════════════════");
Console.WriteLine("TPC Challenge: Requires UNION ALL across all concrete tables!");
Console.WriteLine();

using (var context = new VehicleDbContext(connectionString))
{
    // Query all vehicles - EF Core generates UNION ALL across Cars, Motorcycles, Trucks
    // This is the SLOWEST operation in TPC
    var allVehicles = await context.Vehicles.ToListAsync();

    Console.WriteLine($"Found {allVehicles.Count} vehicles across all tables:");
    Console.WriteLine();

    foreach (var vehicle in allVehicles)
    {
        Console.WriteLine($"  [{vehicle.GetType().Name}] {vehicle.GetDescription()}");
    }
}

Console.WriteLine();
Console.WriteLine("💡 SQL: UNION ALL query!");
Console.WriteLine("   (SELECT * FROM Cars) UNION ALL");
Console.WriteLine("   (SELECT * FROM Motorcycles) UNION ALL");
Console.WriteLine("   (SELECT * FROM Trucks)");
Console.WriteLine();

// ============================================================
// DEMONSTRATION 2: Query Specific Type (TPC ADVANTAGE!)
// ============================================================
Console.WriteLine("═══════════════════════════════════════════════════════════════");
Console.WriteLine("🚗 DEMONSTRATION 2: Querying Specific Type (Cars Only)");
Console.WriteLine("═══════════════════════════════════════════════════════════════");
Console.WriteLine("TPC Advantage: Simple SELECT from single table - VERY FAST!");
Console.WriteLine();

using (var context = new VehicleDbContext(connectionString))
{
    // Query only cars - generates simple SELECT from Cars table
    // This is where TPC SHINES - no JOINs, no UNION, just a simple query
    var cars = await context.Vehicles.OfType<Car>().ToListAsync();

    Console.WriteLine($"Found {cars.Count} cars:");
    foreach (var car in cars)
    {
        Console.WriteLine($"  • {car.Brand} {car.Model} ({car.Year})");
        Console.WriteLine($"    Doors: {car.NumberOfDoors}, Fuel: {car.FuelType}, Price: ${car.Price:N2}");
    }
}

Console.WriteLine();
Console.WriteLine("💡 SQL: SELECT * FROM Cars (simple, fast!)");
Console.WriteLine("   No JOINs, no UNION - direct table access");
Console.WriteLine();

// ============================================================
// DEMONSTRATION 3: Direct Table Access
// ============================================================
Console.WriteLine("═══════════════════════════════════════════════════════════════");
Console.WriteLine("🏍️  DEMONSTRATION 3: Direct Access via DbSet<Motorcycle>");
Console.WriteLine("═══════════════════════════════════════════════════════════════");
Console.WriteLine("Best practice in TPC: Query concrete types directly");
Console.WriteLine();

using (var context = new VehicleDbContext(connectionString))
{
    // Direct access to Motorcycles table - optimal for TPC
    var motorcycles = await context.Motorcycles
        .Where(m => m.EngineCC > 900)
        .ToListAsync();

    Console.WriteLine($"Found {motorcycles.Count} motorcycle(s) over 900cc:");
    foreach (var bike in motorcycles)
    {
        Console.WriteLine($"  • {bike.Brand} {bike.Model} - {bike.EngineCC}cc");
        Console.WriteLine($"    Sidecar: {(bike.HasSidecar ? "Yes" : "No")}, Price: ${bike.Price:N2}");
    }
}

Console.WriteLine();
Console.WriteLine("💡 SQL: SELECT * FROM Motorcycles WHERE EngineCC > 900");
Console.WriteLine();

// ============================================================
// DEMONSTRATION 4: Filter by Base Property
// ============================================================
Console.WriteLine("═══════════════════════════════════════════════════════════════");
Console.WriteLine("💰 DEMONSTRATION 4: Filter by Base Property (Expensive Vehicles)");
Console.WriteLine("═══════════════════════════════════════════════════════════════");
Console.WriteLine("TPC: Filters applied to each table in UNION ALL");
Console.WriteLine();

using (var context = new VehicleDbContext(connectionString))
{
    // Query vehicles by base property - requires UNION ALL
    // Filter is pushed down to each subquery for efficiency
    var expensiveVehicles = await context.Vehicles
        .Where(v => v.Price > 40000)
        .OrderByDescending(v => v.Price)
        .ToListAsync();

    Console.WriteLine($"Found {expensiveVehicles.Count} vehicles over $40,000:");
    foreach (var vehicle in expensiveVehicles)
    {
        Console.WriteLine($"  • [{vehicle.GetType().Name}] {vehicle.Brand} {vehicle.Model} - ${vehicle.Price:N2}");
    }
}

Console.WriteLine();
Console.WriteLine("💡 SQL:");
Console.WriteLine("   (SELECT * FROM Cars WHERE Price > 40000) UNION ALL");
Console.WriteLine("   (SELECT * FROM Motorcycles WHERE Price > 40000) UNION ALL");
Console.WriteLine("   (SELECT * FROM Trucks WHERE Price > 40000)");
Console.WriteLine();

// ============================================================
// DEMONSTRATION 5: Type-Specific Query (Optimal Pattern)
// ============================================================
Console.WriteLine("═══════════════════════════════════════════════════════════════");
Console.WriteLine("🚚 DEMONSTRATION 5: Type-Specific Query (Best Practice)");
Console.WriteLine("═══════════════════════════════════════════════════════════════");
Console.WriteLine("When you know the type, query the concrete table directly");
Console.WriteLine();

using (var context = new VehicleDbContext(connectionString))
{
    // BEST PRACTICE for TPC: Query concrete type when known
    var heavyTrucks = await context.Trucks
        .Where(t => t.LoadCapacity > 10)
        .OrderByDescending(t => t.LoadCapacity)
        .ToListAsync();

    Console.WriteLine($"Found {heavyTrucks.Count} heavy truck(s):");
    foreach (var truck in heavyTrucks)
    {
        Console.WriteLine($"  • {truck.Brand} {truck.Model}");
        Console.WriteLine($"    Capacity: {truck.LoadCapacity}t, Axles: {truck.NumberOfAxles}, Price: ${truck.Price:N2}");
    }
}

Console.WriteLine();
Console.WriteLine("💡 SQL: SELECT * FROM Trucks WHERE LoadCapacity > 10");
Console.WriteLine("   Optimal query pattern for TPC!");
Console.WriteLine();

// ============================================================
// DEMONSTRATION 6: Insert New Entities
// ============================================================
Console.WriteLine("═══════════════════════════════════════════════════════════════");
Console.WriteLine("➕ DEMONSTRATION 6: Adding New Vehicles");
Console.WriteLine("═══════════════════════════════════════════════════════════════");
Console.WriteLine("TPC: Inserts only into the specific concrete table");
Console.WriteLine();

using (var context = new VehicleDbContext(connectionString))
{
    // Add a new car - inserts ONLY into Cars table
    var newCar = new Car
    {
        Brand = "Renault",
        Model = "Clio",
        Year = 2024,
        Price = 22000,
        NumberOfDoors = 5,
        FuelType = "Diesel"
    };

    // Add a new truck - inserts ONLY into Trucks table
    var newTruck = new Truck
    {
        Brand = "DAF",
        Model = "XF",
        Year = 2024,
        Price = 110000,
        LoadCapacity = 15.0m,
        NumberOfAxles = 3
    };

    context.Vehicles.AddRange(newCar, newTruck);
    await context.SaveChangesAsync();

    Console.WriteLine("✅ Added new vehicles (single table per entity):");
    Console.WriteLine($"  • {newCar.GetDescription()}");
    Console.WriteLine($"    → INSERT INTO Cars (Id, Brand, Model, Year, Price, NumberOfDoors, FuelType)");
    Console.WriteLine();
    Console.WriteLine($"  • {newTruck.GetDescription()}");
    Console.WriteLine($"    → INSERT INTO Trucks (Id, Brand, Model, Year, Price, LoadCapacity, NumberOfAxles)");
}

Console.WriteLine();
Console.WriteLine("💡 Notice: ALL properties (base + derived) in single INSERT");
Console.WriteLine();

// ============================================================
// DEMONSTRATION 7: Update Operation
// ============================================================
Console.WriteLine("═══════════════════════════════════════════════════════════════");
Console.WriteLine("✏️  DEMONSTRATION 7: Updating a Vehicle");
Console.WriteLine("═══════════════════════════════════════════════════════════════");
Console.WriteLine("TPC Advantage: Updates only one table");
Console.WriteLine();

using (var context = new VehicleDbContext(connectionString))
{
    // Find and update a car
    var peugeot = await context.Cars
        .FirstOrDefaultAsync(c => c.Brand == "Peugeot");

    if (peugeot != null)
    {
        Console.WriteLine($"Before: {peugeot.GetDescription()}");

        // Update both base and derived properties
        peugeot.Price = 32500;    // Base property
        peugeot.Year = 2025;      // Base property
        peugeot.FuelType = "Plug-in Hybrid"; // Derived property

        await context.SaveChangesAsync();

        Console.WriteLine($"After:  {peugeot.GetDescription()}");
        Console.WriteLine();
        Console.WriteLine("✅ Update executed:");
        Console.WriteLine("   → UPDATE Cars SET Price = ..., Year = ..., FuelType = ... WHERE Id = ...");
        Console.WriteLine("   (Single table update - fast!)");
    }
}

Console.WriteLine();

// ============================================================
// DEMONSTRATION 8: Delete Operation
// ============================================================
Console.WriteLine("═══════════════════════════════════════════════════════════════");
Console.WriteLine("🗑️  DEMONSTRATION 8: Deleting a Vehicle");
Console.WriteLine("═══════════════════════════════════════════════════════════════");
Console.WriteLine("TPC Advantage: Deletes from only one table");
Console.WriteLine();

using (var context = new VehicleDbContext(connectionString))
{
    // Find a motorcycle to delete
    var ducati = await context.Motorcycles
        .FirstOrDefaultAsync(m => m.Brand == "Ducati");

    if (ducati != null)
    {
        Console.WriteLine($"Deleting: {ducati.GetDescription()}");

        context.Motorcycles.Remove(ducati);
        await context.SaveChangesAsync();

        Console.WriteLine("✅ Delete executed:");
        Console.WriteLine("   → DELETE FROM Motorcycles WHERE Id = ...");
        Console.WriteLine("   (Single table delete - no cascades needed!)");
    }
}

Console.WriteLine();

// ============================================================
// DEMONSTRATION 9: Count by Type
// ============================================================
Console.WriteLine("═══════════════════════════════════════════════════════════════");
Console.WriteLine("📊 DEMONSTRATION 9: Statistics by Type");
Console.WriteLine("═══════════════════════════════════════════════════════════════");
Console.WriteLine("Direct table counts - very efficient in TPC");
Console.WriteLine();

using (var context = new VehicleDbContext(connectionString))
{
    // Direct COUNT on each table - very fast
    var carCount = await context.Cars.CountAsync();
    var motorcycleCount = await context.Motorcycles.CountAsync();
    var truckCount = await context.Trucks.CountAsync();
    var total = carCount + motorcycleCount + truckCount;

    Console.WriteLine("Vehicle Inventory:");
    Console.WriteLine($"  🚗 Cars:        {carCount} (SELECT COUNT(*) FROM Cars)");
    Console.WriteLine($"  🏍️  Motorcycles: {motorcycleCount} (SELECT COUNT(*) FROM Motorcycles)");
    Console.WriteLine($"  🚚 Trucks:      {truckCount} (SELECT COUNT(*) FROM Trucks)");
    Console.WriteLine($"  ──────────────");
    Console.WriteLine($"  📦 Total:       {total}");
    Console.WriteLine();
    Console.WriteLine("💡 Three separate COUNT queries - fast and simple!");
}

Console.WriteLine();

// ============================================================
// SUMMARY
// ============================================================
Console.WriteLine("═══════════════════════════════════════════════════════════════");
Console.WriteLine("📚 TPC STRATEGY SUMMARY");
Console.WriteLine("═══════════════════════════════════════════════════════════════");
Console.WriteLine();
Console.WriteLine("✅ Advantages:");
Console.WriteLine("  • Fastest specific type queries: No JOINs needed");
Console.WriteLine("  • Simple updates/deletes: Single table operations");
Console.WriteLine("  • Complete data: All properties in one row");
Console.WriteLine("  • Table independence: Each type isolated");
Console.WriteLine();
Console.WriteLine("⚠️  Trade-offs:");
Console.WriteLine("  • Data duplication: Base properties in every table");
Console.WriteLine("  • Slow polymorphic queries: UNION ALL across all tables");
Console.WriteLine("  • Schema changes: Must update ALL concrete tables");
Console.WriteLine("  • No base table: Cannot query abstract Vehicle table directly");
Console.WriteLine();
Console.WriteLine("🎯 Best for:");
Console.WriteLine("  • Type-specific queries dominate your workload");
Console.WriteLine("  • Rarely query across all types polymorphically");
Console.WriteLine("  • Each type is largely independent");
Console.WriteLine("  • Performance for specific types is critical");
Console.WriteLine();
Console.WriteLine("⚠️  Avoid when:");
Console.WriteLine("  • Frequent polymorphic queries across all types");
Console.WriteLine("  • Base class properties change often");
Console.WriteLine("  • Many shared properties (high duplication cost)");
Console.WriteLine();
Console.WriteLine("═══════════════════════════════════════════════════════════════");
Console.WriteLine();
Console.WriteLine("✨ Demo completed! Verify in database that there's NO Vehicles table.");
Console.WriteLine();
Console.WriteLine("   Run these SQL queries:");
Console.WriteLine("   SELECT * FROM \"Cars\";");
Console.WriteLine("   SELECT * FROM \"Motorcycles\";");
Console.WriteLine("   SELECT * FROM \"Trucks\";");
Console.WriteLine();
Console.WriteLine("   Try this to see UNION ALL:");
Console.WriteLine("   SELECT 'Car' as Type, * FROM \"Cars\"");
Console.WriteLine("   UNION ALL");
Console.WriteLine("   SELECT 'Motorcycle', * FROM \"Motorcycles\"");
Console.WriteLine("   UNION ALL");
Console.WriteLine("   SELECT 'Truck', * FROM \"Trucks\";");
Console.WriteLine();
