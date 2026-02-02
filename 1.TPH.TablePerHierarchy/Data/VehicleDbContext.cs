using Microsoft.EntityFrameworkCore;
using EF.TPH.Models;

namespace EF.TPH.Data;

/// <summary>
/// Database context for Table Per Hierarchy (TPH) inheritance demonstration.
/// TPH is the DEFAULT inheritance strategy in EF Core - requires minimal configuration.
/// </summary>
/// <remarks>
/// TPH Key Points:
/// 1. Only ONE DbSet is needed (the base class)
/// 2. EF Core automatically includes all derived types
/// 3. A "Discriminator" column is automatically created
/// 4. All properties from all types go into ONE table
/// 5. Type-specific properties become NULLABLE columns
/// </remarks>
public class VehicleDbContext : DbContext
{
    private readonly string _connectionString;

    public VehicleDbContext(string connectionString)
    {
        _connectionString = connectionString;
    }

    /// <summary>
    /// DbSet for the base Vehicle class.
    /// In TPH, this single DbSet represents ALL vehicle types (Car, Truck, Motorcycle).
    /// You can still query derived types using OfType<T>() or the Set<T>() method.
    /// </summary>
    public DbSet<Vehicle> Vehicles { get; set; } = null!;

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlite(_connectionString);
        optionsBuilder.EnableSensitiveDataLogging();
        optionsBuilder.LogTo(Console.WriteLine, Microsoft.Extensions.Logging.LogLevel.Information);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Vehicle>()
            .UseTphMappingStrategy()
            .HasDiscriminator<string>("Discriminator")
            .HasValue<Car>("Car")               // Car entity
            .HasValue<Motorcycle>("Motorcycle") // Motorcycle entity
            .HasValue<Truck>("Truck");          // Truck entity

        modelBuilder.Entity<Vehicle>(entity =>
        {
            entity.Property(v => v.Brand)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(v => v.Model)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(v => v.Price)
                .HasPrecision(18, 2);
        });

        modelBuilder.Entity<Car>(entity =>
        {
            entity.Property(c => c.FuelType)
                .HasMaxLength(50);
        });

        modelBuilder.Entity<Truck>(entity =>
        {
            entity.Property(t => t.LoadCapacity)
                .HasPrecision(10, 2);
        });

        SeedData(modelBuilder);
    }
    
    private void SeedData(ModelBuilder modelBuilder)
    {
        // Seed Cars - will have Discriminator = "Car"
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

        // Seed Motorcycles - will have Discriminator = "Motorcycle"
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

        // Seed Trucks - will have Discriminator = "Truck"
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
