using Microsoft.EntityFrameworkCore;
using EF.TPC.Models;

namespace EF.TPC.Data;

/// <summary>
/// Database context for Table Per Concrete Type (TPC) inheritance demonstration.
/// TPC creates tables ONLY for concrete classes - the abstract base class has NO table.
/// </summary>
/// <remarks>
/// TPC Key Points:
/// 1. NO Vehicles table (abstract base class)
/// 2. Only concrete classes get tables: Cars, Motorcycles, Trucks
/// 3. Each table contains ALL properties (base + derived)
/// 4. No foreign key relationships between tables
/// 5. No discriminator column (type = which table contains the data)
/// 6. Requires explicit configuration with UseTpcMappingStrategy()
///
/// Database Structure:
/// ❌ Vehicles table - DOES NOT EXIST
/// ✅ Cars: Id, Brand, Model, Year, Price, NumberOfDoors, FuelType
/// ✅ Motorcycles: Id, Brand, Model, Year, Price, HasSidecar, EngineCC
/// ✅ Trucks: Id, Brand, Model, Year, Price, LoadCapacity, NumberOfAxles
///
/// Key Difference from TPT:
/// - TPT: Vehicles table exists, derived tables have FK to it
/// - TPC: No Vehicles table, all properties duplicated in each concrete table
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
    /// In TPC, querying this DbSet executes a UNION ALL across all concrete tables.
    /// Even though there's no Vehicles table, you can still query polymorphically!
    /// </summary>
    public DbSet<Vehicle> Vehicles { get; set; } = null!;

    /// <summary>
    /// DbSets for concrete types - these map directly to database tables.
    /// Querying these is FAST - simple SELECT from a single table.
    /// </summary>
    public DbSet<Car> Cars { get; set; } = null!;
    public DbSet<Motorcycle> Motorcycles { get; set; } = null!;
    public DbSet<Truck> Trucks { get; set; } = null!;

    /// <summary>
    /// Configure the SQLite connection.
    /// </summary>
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        // Use SQLite with the provided connection string
        optionsBuilder.UseSqlite(_connectionString);

        // Enable sensitive data logging for demo purposes (disable in production)
        optionsBuilder.EnableSensitiveDataLogging();

        // Log the generated SQL to see UNION ALL queries for polymorphic access
        optionsBuilder.LogTo(Console.WriteLine, Microsoft.Extensions.Logging.LogLevel.Information);
    }

    /// <summary>
    /// Configure the model and TPC inheritance strategy.
    /// </summary>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // ============================================================
        // TPC CONFIGURATION (REQUIRED!)
        // ============================================================

        // CRITICAL: Must explicitly configure TPC strategy
        // Without this, EF Core will use the default TPH strategy
        modelBuilder.Entity<Vehicle>()
            .UseTpcMappingStrategy(); // This line enables TPC!

        // ============================================================
        // WHAT HAPPENS BEHIND THE SCENES:
        // ============================================================
        // 1. NO Vehicles table is created
        // 2. Cars table gets: Id, Brand, Model, Year, Price, NumberOfDoors, FuelType
        // 3. Motorcycles table gets: Id, Brand, Model, Year, Price, HasSidecar, EngineCC
        // 4. Trucks table gets: Id, Brand, Model, Year, Price, LoadCapacity, NumberOfAxles
        //
        // Notice: Brand, Model, Year, Price are DUPLICATED in each table!
        // ============================================================

        // Configure table names (optional - defaults to entity names)
        // Note: No .ToTable() for Vehicle because it doesn't get a table!
        modelBuilder.Entity<Car>().ToTable("Cars");
        modelBuilder.Entity<Motorcycle>().ToTable("Motorcycles");
        modelBuilder.Entity<Truck>().ToTable("Trucks");

        // ============================================================
        // Configure Base Vehicle Properties
        // ============================================================
        // These configurations apply to ALL concrete tables
        // Each table will have these columns with these constraints
        modelBuilder.Entity<Vehicle>(entity =>
        {
            entity.HasKey(v => v.Id);

            // Base properties - will be DUPLICATED in Cars, Motorcycles, Trucks
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

        // ============================================================
        // ID Management in TPC
        // ============================================================
        // IMPORTANT: IDs must be unique across ALL concrete tables!
        // EF Core handles this automatically using:
        // - PostgreSQL: A shared sequence across all tables
        // - SQL Server: HiLo algorithm
        //
        // Manual alternative (commented out):
        // modelBuilder.HasSequence<int>("VehicleIdSequence")
        //     .StartsAt(1)
        //     .IncrementsBy(1);
        //
        // modelBuilder.Entity<Car>()
        //     .Property(c => c.Id)
        //     .HasDefaultValueSql("nextval('\"VehicleIdSequence\"')");
        // (Repeat for Motorcycle and Truck)
        // ============================================================

        // Configure Car-specific properties
        modelBuilder.Entity<Car>(entity =>
        {
            // Car properties - can be NOT NULL at database level
            entity.Property(c => c.NumberOfDoors)
                .IsRequired();

            entity.Property(c => c.FuelType)
                .IsRequired()
                .HasMaxLength(50);
        });

        // Configure Motorcycle-specific properties
        modelBuilder.Entity<Motorcycle>(entity =>
        {
            entity.Property(m => m.HasSidecar)
                .IsRequired();

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
        // Seed Cars - creates rows ONLY in Cars table
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

        // Seed Motorcycles - creates rows ONLY in Motorcycles table
        modelBuilder.Entity<Motorcycle>().HasData(
            new Motorcycle
            {
                Id = 3, // Note: IDs must be unique across ALL tables!
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

        // Seed Trucks - creates rows ONLY in Trucks table
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
