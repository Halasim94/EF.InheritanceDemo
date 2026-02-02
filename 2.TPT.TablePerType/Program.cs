using EF.TPT.Data;
using EF.TPT.Models;
using Microsoft.EntityFrameworkCore;

// ============================================================
// TABLE PER TYPE (TPT) DEMONSTRATION
// ============================================================
// This demo shows how TPT inheritance mapping works in EF Core.
// Each type gets its own table with FK relationships to the base table.
// ============================================================

Console.WriteLine("╔════════════════════════════════════════════════════════════════╗");
Console.WriteLine("║    Entity Framework Core - Table Per Type (TPT) Demo           ║");
Console.WriteLine("╚════════════════════════════════════════════════════════════════╝");
Console.WriteLine();

// ============================================================
// CONFIGURATION - SQLite database file
// ============================================================
const string connectionString = "Data Source=EFInheritance_TPT.db";
Console.WriteLine("📊 Connection: EFInheritance_TPT.db (SQLite)");
Console.WriteLine();

// ============================================================
// INITIALIZE DATABASE
// ============================================================
Console.WriteLine("🔧 Initializing database...");
using (var context = new VehicleDbContext(connectionString))
{
    // Ensure database is created with all four tables
    await context.Database.EnsureCreatedAsync();
    Console.WriteLine("✅ Database created/verified");
    Console.WriteLine("   Tables created:");
    Console.WriteLine("   - Vehicles (base table)");
    Console.WriteLine("   - Cars (FK to Vehicles)");
    Console.WriteLine("   - Motorcycles (FK to Vehicles)");
    Console.WriteLine("   - Trucks (FK to Vehicles)");
    Console.WriteLine();
}

// ============================================================
// DEMONSTRATION 1: View Seeded Data
// ============================================================
Console.WriteLine("═══════════════════════════════════════════════════════════════");
Console.WriteLine("📋 DEMONSTRATION 1: Querying ALL Vehicles (Polymorphic Query)");
Console.WriteLine("═══════════════════════════════════════════════════════════════");
Console.WriteLine("TPT Challenge: Requires UNION ALL of base + all derived tables!");
Console.WriteLine();

using (var context = new VehicleDbContext(connectionString))
{
    // Query all vehicles - EF Core generates complex UNION ALL query
    // to combine data from Vehicles + Cars + Motorcycles + Trucks
    var allVehicles = await context.Vehicles.ToListAsync();

    Console.WriteLine($"Found {allVehicles.Count} vehicles across all tables:");
    Console.WriteLine();

    foreach (var vehicle in allVehicles)
    {
        Console.WriteLine($"  [{vehicle.GetType().Name}] {vehicle.GetDescription()}");
    }
}

Console.WriteLine();
Console.WriteLine("💡 SQL: UNION ALL query combining all tables!");
Console.WriteLine("   (Vehicles JOIN Cars) UNION ALL (Vehicles JOIN Motorcycles) UNION ALL...");
Console.WriteLine();

// ============================================================
// DEMONSTRATION 2: Query Specific Type
// ============================================================
Console.WriteLine("═══════════════════════════════════════════════════════════════");
Console.WriteLine("🚗 DEMONSTRATION 2: Querying Specific Type (Cars Only)");
Console.WriteLine("═══════════════════════════════════════════════════════════════");
Console.WriteLine("TPT Advantage: Simple JOIN of two tables");
Console.WriteLine();

using (var context = new VehicleDbContext(connectionString))
{
    // Query only cars - generates simple JOIN
    // SELECT v.*, c.* FROM Vehicles v INNER JOIN Cars c ON v.Id = c.Id
    var cars = await context.Vehicles.OfType<Car>().ToListAsync();

    Console.WriteLine($"Found {cars.Count} cars:");
    foreach (var car in cars)
    {
        Console.WriteLine($"  • {car.Brand} {car.Model} ({car.Year})");
        Console.WriteLine($"    Doors: {car.NumberOfDoors}, Fuel: {car.FuelType}, Price: ${car.Price:N2}");
    }
}

Console.WriteLine();
Console.WriteLine("💡 SQL: SELECT * FROM Vehicles v JOIN Cars c ON v.Id = c.Id");
Console.WriteLine();

// ============================================================
// DEMONSTRATION 3: Direct Table Access
// ============================================================
Console.WriteLine("═══════════════════════════════════════════════════════════════");
Console.WriteLine("📊 DEMONSTRATION 3: Direct Access via DbSet<Car>");
Console.WriteLine("═══════════════════════════════════════════════════════════════");
Console.WriteLine("Alternative query method using typed DbSet");
Console.WriteLine();

using (var context = new VehicleDbContext(connectionString))
{
    // Can query derived types directly using their DbSet
    var electricCars = await context.Cars
        .Where(c => c.FuelType == "Electric")
        .ToListAsync();

    Console.WriteLine($"Found {electricCars.Count} electric car(s):");
    foreach (var car in electricCars)
    {
        Console.WriteLine($"  • {car.Brand} {car.Model} - {car.FuelType}");
    }
}

Console.WriteLine();

// ============================================================
// DEMONSTRATION 4: Query by Base Property
// ============================================================
Console.WriteLine("═══════════════════════════════════════════════════════════════");
Console.WriteLine("💰 DEMONSTRATION 4: Filter by Base Property (Expensive Vehicles)");
Console.WriteLine("═══════════════════════════════════════════════════════════════");
Console.WriteLine("Filter on base table property across all types");
Console.WriteLine();

using (var context = new VehicleDbContext(connectionString))
{
    // Query vehicles by base property - filters in base table, then JOINs to derived
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
// DEMONSTRATION 5: Type-Specific Query
// ============================================================
Console.WriteLine("═══════════════════════════════════════════════════════════════");
Console.WriteLine("🚚 DEMONSTRATION 5: Type-Specific Property Query");
Console.WriteLine("═══════════════════════════════════════════════════════════════");
Console.WriteLine("Query trucks by load capacity");
Console.WriteLine();

using (var context = new VehicleDbContext(connectionString))
{
    // Query by derived class property
    // Filters in derived table: WHERE LoadCapacity > 10
    var heavyTrucks = await context.Vehicles
        .OfType<Truck>()
        .Where(t => t.LoadCapacity > 10)
        .ToListAsync();

    Console.WriteLine($"Found {heavyTrucks.Count} heavy truck(s):");
    foreach (var truck in heavyTrucks)
    {
        Console.WriteLine($"  • {truck.Brand} {truck.Model} - {truck.LoadCapacity}t capacity, {truck.NumberOfAxles} axles");
    }
}

Console.WriteLine();

// ============================================================
// DEMONSTRATION 6: Insert New Entities
// ============================================================
Console.WriteLine("═══════════════════════════════════════════════════════════════");
Console.WriteLine("➕ DEMONSTRATION 6: Adding New Vehicles");
Console.WriteLine("═══════════════════════════════════════════════════════════════");
Console.WriteLine("TPT: Inserts into BOTH base and derived tables");
Console.WriteLine();

using (var context = new VehicleDbContext(connectionString))
{
    // Add a new car - inserts into Vehicles AND Cars tables
    var newCar = new Car
    {
        Brand = "Renault",
        Model = "Clio",
        Year = 2024,
        Price = 22000,
        NumberOfDoors = 5,
        FuelType = "Diesel"
    };

    // Add a new motorcycle - inserts into Vehicles AND Motorcycles tables
    var newBike = new Motorcycle
    {
        Brand = "KTM",
        Model = "890 Duke",
        Year = 2024,
        Price = 11500,
        HasSidecar = false,
        EngineCC = 889
    };

    context.Vehicles.AddRange(newCar, newBike);
    await context.SaveChangesAsync();

    Console.WriteLine("✅ Added new vehicles (2 tables affected per vehicle):");
    Console.WriteLine($"  • {newCar.GetDescription()}");
    Console.WriteLine($"    → INSERT INTO Vehicles (Brand, Model, Year, Price)");
    Console.WriteLine($"    → INSERT INTO Cars (Id, NumberOfDoors, FuelType)");
    Console.WriteLine();
    Console.WriteLine($"  • {newBike.GetDescription()}");
    Console.WriteLine($"    → INSERT INTO Vehicles (Brand, Model, Year, Price)");
    Console.WriteLine($"    → INSERT INTO Motorcycles (Id, HasSidecar, EngineCC)");
}

Console.WriteLine();

// ============================================================
// DEMONSTRATION 7: Update Operation
// ============================================================
Console.WriteLine("═══════════════════════════════════════════════════════════════");
Console.WriteLine("✏️  DEMONSTRATION 7: Updating a Vehicle");
Console.WriteLine("═══════════════════════════════════════════════════════════════");
Console.WriteLine("May update multiple tables depending on which properties change");
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

        // Update base property (affects Vehicles table)
        peugeot.Price = 32500;
        peugeot.Year = 2025;

        // Update derived property (affects Cars table)
        peugeot.FuelType = "Plug-in Hybrid";

        await context.SaveChangesAsync();

        Console.WriteLine($"After:  {peugeot.GetDescription()}");
        Console.WriteLine();
        Console.WriteLine("✅ Update executed:");
        Console.WriteLine("   → UPDATE Vehicles SET Price = ..., Year = ... WHERE Id = ...");
        Console.WriteLine("   → UPDATE Cars SET FuelType = ... WHERE Id = ...");
    }
}

Console.WriteLine();

// ============================================================
// DEMONSTRATION 8: Delete Operation
// ============================================================
Console.WriteLine("═══════════════════════════════════════════════════════════════");
Console.WriteLine("🗑️  DEMONSTRATION 8: Deleting a Vehicle");
Console.WriteLine("═══════════════════════════════════════════════════════════════");
Console.WriteLine("Cascade delete from both base and derived tables");
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

        Console.WriteLine("✅ Delete executed (cascade):");
        Console.WriteLine("   → DELETE FROM Motorcycles WHERE Id = ...");
        Console.WriteLine("   → DELETE FROM Vehicles WHERE Id = ...");
        Console.WriteLine("   (FK constraint handles cascade)");
    }
}

Console.WriteLine();

// ============================================================
// DEMONSTRATION 9: Count by Type
// ============================================================
Console.WriteLine("═══════════════════════════════════════════════════════════════");
Console.WriteLine("📊 DEMONSTRATION 9: Statistics by Type");
Console.WriteLine("═══════════════════════════════════════════════════════════════");
Console.WriteLine("Count rows in each derived table");
Console.WriteLine();

using (var context = new VehicleDbContext(connectionString))
{
    var carCount = await context.Vehicles.OfType<Car>().CountAsync();
    var motorcycleCount = await context.Vehicles.OfType<Motorcycle>().CountAsync();
    var truckCount = await context.Vehicles.OfType<Truck>().CountAsync();
    var total = await context.Vehicles.CountAsync();

    Console.WriteLine("Vehicle Inventory:");
    Console.WriteLine($"  🚗 Cars:        {carCount} (rows in Cars table)");
    Console.WriteLine($"  🏍️  Motorcycles: {motorcycleCount} (rows in Motorcycles table)");
    Console.WriteLine($"  🚚 Trucks:      {truckCount} (rows in Trucks table)");
    Console.WriteLine($"  ──────────────");
    Console.WriteLine($"  📦 Total:       {total} (rows in base Vehicles table)");
}

Console.WriteLine();

// ============================================================
// SUMMARY
// ============================================================
Console.WriteLine("═══════════════════════════════════════════════════════════════");
Console.WriteLine("📚 TPT STRATEGY SUMMARY");
Console.WriteLine("═══════════════════════════════════════════════════════════════");
Console.WriteLine();
Console.WriteLine("✅ Advantages:");
Console.WriteLine("  • Fully normalized: No NULL values, each table has only relevant data");
Console.WriteLine("  • Type-specific constraints: Can enforce NOT NULL on derived properties");
Console.WriteLine("  • Clean separation: Each type has its own focused table");
Console.WriteLine("  • Storage efficient: No wasted space on NULL columns");
Console.WriteLine();
Console.WriteLine("⚠️  Trade-offs:");
Console.WriteLine("  • Performance cost: JOINs required for every query");
Console.WriteLine("  • Complex polymorphic queries: UNION ALL across all tables");
Console.WriteLine("  • Multiple writes: Inserts/updates/deletes touch multiple tables");
Console.WriteLine("  • Deep hierarchies: More levels = more JOINs = slower");
Console.WriteLine();
Console.WriteLine("🎯 Best for:");
Console.WriteLine("  • Normalized data requirements (reporting, compliance)");
Console.WriteLine("  • Many type-specific properties per derived type");
Console.WriteLine("  • Need strong database constraints");
Console.WriteLine("  • Infrequent polymorphic queries");
Console.WriteLine();
Console.WriteLine("═══════════════════════════════════════════════════════════════");