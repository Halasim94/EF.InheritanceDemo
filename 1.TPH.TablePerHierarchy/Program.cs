using EF.TPH.Data;
using EF.TPH.Models;
using Microsoft.EntityFrameworkCore;

// ============================================================
// TABLE PER HIERARCHY (TPH) DEMONSTRATION
// ============================================================
// This demo shows how TPH inheritance mapping works in EF Core.
// All entities are stored in a SINGLE table with a discriminator column.
// ============================================================

Console.WriteLine("╔════════════════════════════════════════════════════════════════╗");
Console.WriteLine("║   Entity Framework Core - Table Per Hierarchy (TPH) Demo      ║");
Console.WriteLine("╚════════════════════════════════════════════════════════════════╝");
Console.WriteLine();

// ============================================================
// CONFIGURATION - SQLite database file
// ============================================================
const string connectionString = "Data Source=EFInheritance_TPH.db";
Console.WriteLine("📊 Connection: EFInheritance_TPH.db (SQLite)");
Console.WriteLine();

// ============================================================
// INITIALIZE DATABASE
// ============================================================
Console.WriteLine("🔧 Initializing database...");
using (var context = new VehicleDbContext(connectionString))
{
    // Ensure database is created with the single Vehicles table
    await context.Database.EnsureCreatedAsync();
    Console.WriteLine("✅ Database created/verified");
    Console.WriteLine("   Table created: Vehicles (contains ALL vehicle types)");
    Console.WriteLine();
}

// ============================================================
// DEMONSTRATION 1: View Seeded Data
// ============================================================
Console.WriteLine("═══════════════════════════════════════════════════════════════");
Console.WriteLine("📋 DEMONSTRATION 1: Querying ALL Vehicles (Polymorphic Query)");
Console.WriteLine("═══════════════════════════════════════════════════════════════");
Console.WriteLine("TPH Advantage: Single table query, no JOINs needed!");
Console.WriteLine();

using (var context = new VehicleDbContext(connectionString))
{
    // Query all vehicles - returns all types from the single Vehicles table
    // EF Core adds a SELECT for all columns including the Discriminator
    var allVehicles = await context.Vehicles.ToListAsync();

    Console.WriteLine($"Found {allVehicles.Count} vehicles in the Vehicles table:");
    Console.WriteLine();

    foreach (var vehicle in allVehicles)
    {
        // Demonstrates runtime polymorphism - calls the appropriate GetDescription() override
        Console.WriteLine($"  [{vehicle.GetType().Name}] {vehicle.GetDescription()}");
    }
}

Console.WriteLine();
Console.WriteLine("💡 Notice: All vehicle types retrieved in a single query!");
Console.WriteLine("   SQL: SELECT * FROM Vehicles (no JOINs)");
Console.WriteLine();

// ============================================================
// DEMONSTRATION 2: Query Specific Type
// ============================================================
Console.WriteLine("═══════════════════════════════════════════════════════════════");
Console.WriteLine("🚗 DEMONSTRATION 2: Querying Specific Type (Cars Only)");
Console.WriteLine("═══════════════════════════════════════════════════════════════");
Console.WriteLine("Uses WHERE clause on Discriminator column");
Console.WriteLine();

using (var context = new VehicleDbContext(connectionString))
{
    // Query only cars using OfType<T>()
    // Generates: SELECT * FROM Vehicles WHERE Discriminator = 'Car'
    var cars = await context.Vehicles.OfType<Car>().ToListAsync();

    Console.WriteLine($"Found {cars.Count} cars:");
    foreach (var car in cars)
    {
        Console.WriteLine($"  • {car.Brand} {car.Model} ({car.Year})");
        Console.WriteLine($"    Doors: {car.NumberOfDoors}, Fuel: {car.FuelType}, Price: ${car.Price:N2}");
    }
}

Console.WriteLine();

// ============================================================
// DEMONSTRATION 3: Query by Base Class Property
// ============================================================
Console.WriteLine("═══════════════════════════════════════════════════════════════");
Console.WriteLine("💰 DEMONSTRATION 3: Filter by Base Property (Expensive Vehicles)");
Console.WriteLine("═══════════════════════════════════════════════════════════════");
Console.WriteLine("TPH Advantage: Fast filtering on shared properties!");
Console.WriteLine();

using (var context = new VehicleDbContext(connectionString))
{
    // Query vehicles by base class property - works across all types
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

// ============================================================
// DEMONSTRATION 4: Type-Specific Queries
// ============================================================
Console.WriteLine("═══════════════════════════════════════════════════════════════");
Console.WriteLine("🏍️  DEMONSTRATION 4: Type-Specific Property Query");
Console.WriteLine("═══════════════════════════════════════════════════════════════");
Console.WriteLine("Query motorcycles by engine size");
Console.WriteLine();

using (var context = new VehicleDbContext(connectionString))
{
    // Query by derived class property
    // Generates: SELECT * FROM Vehicles WHERE Discriminator = 'Motorcycle' AND EngineCC > 1000
    var bigBikes = await context.Vehicles
        .OfType<Motorcycle>()
        .Where(m => m.EngineCC > 1000)
        .ToListAsync();

    Console.WriteLine($"Found {bigBikes.Count} motorcycles over 1000cc:");
    foreach (var bike in bigBikes)
    {
        Console.WriteLine($"  • {bike.Brand} {bike.Model} - {bike.EngineCC}cc");
    }
}

Console.WriteLine();

// ============================================================
// DEMONSTRATION 5: Insert New Entities
// ============================================================
Console.WriteLine("═══════════════════════════════════════════════════════════════");
Console.WriteLine("➕ DEMONSTRATION 5: Adding New Vehicles");
Console.WriteLine("═══════════════════════════════════════════════════════════════");
Console.WriteLine();

using (var context = new VehicleDbContext(connectionString))
{
    // Add a new car
    var newCar = new Car
    {
        Brand = "Renault",
        Model = "Clio",
        Year = 2024,
        Price = 22000,
        NumberOfDoors = 5,
        FuelType = "Diesel"
    };

    // Add a new truck
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

    Console.WriteLine("✅ Added new vehicles:");
    Console.WriteLine($"  • {newCar.GetDescription()}");
    Console.WriteLine($"  • {newTruck.GetDescription()}");
}

Console.WriteLine();

// ============================================================
// DEMONSTRATION 6: Update Operation
// ============================================================
Console.WriteLine("═══════════════════════════════════════════════════════════════");
Console.WriteLine("✏️  DEMONSTRATION 6: Updating a Vehicle");
Console.WriteLine("═══════════════════════════════════════════════════════════════");
Console.WriteLine();

using (var context = new VehicleDbContext(connectionString))
{
    // Find and update a car
    var peugeot = await context.Vehicles
        .OfType<Car>()
        .FirstOrDefaultAsync(c => c.Brand == "Peugeot");

    if (peugeot != null)
    {
        Console.WriteLine($"Before: {peugeot.GetDescription()}");

        peugeot.Price = 32500; // Price reduction
        peugeot.Year = 2025;   // Model year update

        await context.SaveChangesAsync();

        Console.WriteLine($"After:  {peugeot.GetDescription()}");
        Console.WriteLine("✅ Update successful (single table UPDATE)");
    }
}

Console.WriteLine();

// ============================================================
// DEMONSTRATION 7: Delete Operation
// ============================================================
Console.WriteLine("═══════════════════════════════════════════════════════════════");
Console.WriteLine("🗑️  DEMONSTRATION 7: Deleting a Vehicle");
Console.WriteLine("═══════════════════════════════════════════════════════════════");
Console.WriteLine();

using (var context = new VehicleDbContext(connectionString))
{
    // Find a motorcycle to delete
    var ducati = await context.Vehicles
        .OfType<Motorcycle>()
        .FirstOrDefaultAsync(m => m.Brand == "Ducati");

    if (ducati != null)
    {
        Console.WriteLine($"Deleting: {ducati.GetDescription()}");

        context.Vehicles.Remove(ducati);
        await context.SaveChangesAsync();

        Console.WriteLine("✅ Delete successful (single table DELETE)");
    }
}

Console.WriteLine();

// ============================================================
// DEMONSTRATION 8: Count by Type
// ============================================================
Console.WriteLine("═══════════════════════════════════════════════════════════════");
Console.WriteLine("📊 DEMONSTRATION 8: Statistics by Type");
Console.WriteLine("═══════════════════════════════════════════════════════════════");
Console.WriteLine("Demonstrating discriminator-based grouping");
Console.WriteLine();

using (var context = new VehicleDbContext(connectionString))
{
    var carCount = await context.Vehicles.OfType<Car>().CountAsync();
    var motorcycleCount = await context.Vehicles.OfType<Motorcycle>().CountAsync();
    var truckCount = await context.Vehicles.OfType<Truck>().CountAsync();
    var total = await context.Vehicles.CountAsync();

    Console.WriteLine("Vehicle Inventory:");
    Console.WriteLine($"  🚗 Cars:        {carCount}");
    Console.WriteLine($"  🏍️  Motorcycles: {motorcycleCount}");
    Console.WriteLine($"  🚚 Trucks:      {truckCount}");
    Console.WriteLine($"  ──────────────");
    Console.WriteLine($"  📦 Total:       {total}");
    Console.WriteLine();
    Console.WriteLine("💡 All counts from the single Vehicles table using Discriminator!");
}

Console.WriteLine();

// ============================================================
// SUMMARY
// ============================================================
Console.WriteLine("═══════════════════════════════════════════════════════════════");
Console.WriteLine("📚 TPH STRATEGY SUMMARY");
Console.WriteLine("═══════════════════════════════════════════════════════════════");
Console.WriteLine();
Console.WriteLine("✅ Advantages:");
Console.WriteLine("  • Simple schema: One table for entire hierarchy");
Console.WriteLine("  • Fast queries: No JOINs needed");
Console.WriteLine("  • Easy polymorphic queries: All data in one place");
Console.WriteLine("  • Default strategy: Minimal configuration");
Console.WriteLine();
Console.WriteLine("⚠️  Trade-offs:");
Console.WriteLine("  • Sparse data: Many NULL values for type-specific properties");
Console.WriteLine("  • Wide table: More columns as you add derived types");
Console.WriteLine("  • Cannot enforce NOT NULL on type-specific columns");
Console.WriteLine();
Console.WriteLine("🎯 Best for:");
Console.WriteLine("  • Simple hierarchies with few types");
Console.WriteLine("  • Frequent polymorphic queries");
Console.WriteLine("  • Performance-critical applications");
Console.WriteLine();
Console.WriteLine("═══════════════════════════════════════════════════════════════");
Console.WriteLine();