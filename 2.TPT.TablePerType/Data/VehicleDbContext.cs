using Microsoft.EntityFrameworkCore;
using EF.TPT.Models;

namespace EF.TPT.Data;

/// <summary>
/// Database context for Table Per Type (TPT) inheritance demonstration.
/// TPT creates a separate table for EACH type in the hierarchy (base and derived).
/// </summary>
/// <remarks>
/// TPT Key Points:
/// 1. Each class gets its OWN table (Vehicles, Cars, Motorcycles, Trucks)
/// 2. Base table contains only base properties
/// 3. Derived tables contain only type-specific properties
/// 4. Derived tables have FK to base table (Id → Id)
/// 5. NO discriminator column (type determined by which child table has data)
/// 6. Requires explicit configuration with UseTptMappingStrategy()
///
/// Database Structure:
/// - Vehicles: Id (PK), Brand, Model, Year, Price
/// - Cars: Id (PK, FK→Vehicles), NumberOfDoors, FuelType
/// - Motorcycles: Id (PK, FK→Vehicles), HasSidecar, EngineCC
/// - Trucks: Id (PK, FK→Vehicles), LoadCapacity, NumberOfAxles
/// </remarks>
public class VehicleDbContext : DbContext
{
    private readonly string _connectionString;

    /// <summary>
    /// Constructor that accepts a connection string.
    /// In production, you'd typically use dependency injection and configuration.
    /// </summary>
    public VehicleDbContext(string connectionString)
    {
        _connectionString = connectionString;
    }

    /// <summary>
    /// DbSet for the base Vehicle class.
    /// In TPT, this represents the base Vehicles table AND can query across all derived types.
    /// </summary>
    public DbSet<Vehicle> Vehicles { get; set; } = null!;

    /// <summary>
    /// Optional: You can also add DbSets for derived types for direct access.
    /// These are not required but can make code more explicit.
    /// </summary>
    public DbSet<Car> Cars { get; set; } = null!;
    public DbSet<Motorcycle> Motorcycles { get; set; } = null!;
    public DbSet<Truck> Trucks { get; set; } = null!;

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlite(_connectionString);
        optionsBuilder.EnableSensitiveDataLogging();
        optionsBuilder.LogTo(Console.WriteLine, Microsoft.Extensions.Logging.LogLevel.Information);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // CRITICAL: Must explicitly configure TPT strategy
        // Without this, EF Core will use the default TPH strategy
        modelBuilder.Entity<Vehicle>()
            .UseTptMappingStrategy(); // This line enables TPT!

        // ============================================================
        // WHAT HAPPENS BEHIND THE SCENES:
        // ============================================================
        // EF Core creates these tables:
        //
        // Vehicles (base table)
        //   Id (PK) | Brand | Model | Year | Price
        //   --------|-------|-------|------|-------
        //   1       | ...   | ...   | ...  | ...
        //
        // Cars (derived table)
        //   Id (PK, FK→Vehicles.Id) | NumberOfDoors | FuelType
        //   ------------------------|---------------|----------
        //   1                       | 4             | Hybrid
        //
        // Similar structure for Motorcycles and Trucks
        // ============================================================

        modelBuilder.Entity<Vehicle>().ToTable("Vehicles");
        modelBuilder.Entity<Car>().ToTable("Cars");
        modelBuilder.Entity<Motorcycle>().ToTable("Motorcycles");
        modelBuilder.Entity<Truck>().ToTable("Trucks");

        modelBuilder.Entity<Vehicle>(entity =>
        {
            entity.HasKey(v => v.Id);

            entity.Property(v => v.Brand)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(v => v.Model)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(v => v.Year)
                .IsRequired();

            entity.Property(v => v.Price)
                .IsRequired()
                .HasPrecision(18, 2);
        });

        modelBuilder.Entity<Car>(entity =>
        {
            entity.Property(c => c.NumberOfDoors)
                .IsRequired(); // Enforced at DB level!

            entity.Property(c => c.FuelType)
                .IsRequired()
                .HasMaxLength(50);
        });

        modelBuilder.Entity<Motorcycle>(entity =>
        {
            entity.Property(m => m.HasSidecar)
                .IsRequired(); // Always has a value (not nullable)

            entity.Property(m => m.EngineCC)
                .IsRequired();
        });

        // Configure Truck-specific properties
        modelBuilder.Entity<Truck>(entity =>
        {
            entity.Property(t => t.LoadCapacity)
                .IsRequired()
                .HasPrecision(10, 2);

            entity.Property(t => t.NumberOfAxles)
                .IsRequired();
        });

        SeedData(modelBuilder);
    }

    private void SeedData(ModelBuilder modelBuilder)
    {
        // Seed Cars - creates rows in BOTH Vehicles and Cars tables
        modelBuilder.Entity<Car>().HasData(
            new Car
            {
                Id = 1,
                Brand = "Volkswagen",
                Model = "Golf",
                Year = 2024,
                Price = 28000,
                NumberOfDoors = 5,
                FuelType = "Hybrid"
            },
            new Car
            {
                Id = 2,
                Brand = "Peugeot",
                Model = "3008",
                Year = 2024,
                Price = 35000,
                NumberOfDoors = 5,
                FuelType = "Electric"
            }
        );

        // Seed Motorcycles - creates rows in BOTH Vehicles and Motorcycles tables
        modelBuilder.Entity<Motorcycle>().HasData(
            new Motorcycle
            {
                Id = 3,
                Brand = "BMW",
                Model = "R 1250 GS",
                Year = 2023,
                Price = 18000,
                HasSidecar = false,
                EngineCC = 1254
            },
            new Motorcycle
            {
                Id = 4,
                Brand = "Ducati",
                Model = "Monster",
                Year = 2024,
                Price = 12000,
                HasSidecar = false,
                EngineCC = 937
            }
        );

        // Seed Trucks - creates rows in BOTH Vehicles and Trucks tables
        modelBuilder.Entity<Truck>().HasData(
            new Truck
            {
                Id = 5,
                Brand = "Mercedes-Benz",
                Model = "Actros",
                Year = 2024,
                Price = 95000,
                LoadCapacity = 12.0m,
                NumberOfAxles = 2
            },
            new Truck
            {
                Id = 6,
                Brand = "Scania",
                Model = "R 500",
                Year = 2023,
                Price = 125000,
                LoadCapacity = 18.0m,
                NumberOfAxles = 3
            }
        );
    }
}
