# Table Per Hierarchy (TPH) - EF Core Inheritance Strategy

## Overview

Table Per Hierarchy (TPH) is the **default inheritance mapping strategy** in Entity Framework Core. In this approach, all classes in an inheritance hierarchy are mapped to a **single database table**.

## How It Works

### Key Characteristics

1. **Single Table**: All entities (base and derived) share one table
2. **Discriminator Column**: Special column identifies the type of each row
3. **Nullable Columns**: Type-specific properties are nullable for other types
4. **No JOINs**: Queries are fast since everything is in one table

### Database Schema

```
Table: Vehicles
┌────────────────┬─────────────────┬───────────────┬────────────┐──────────────────┐
│ Discriminator  │ Base Props      │ Car Props     │ Truck Props│ Motorcycle Props |
├────────────────┼─────────────────┼───────────────┼────────────┤──────────────────┤
│ "Car"          │ VW Golf, 2024   │ Doors, Fuel   │ NULL       │ NULL             |
│ "Truck"        │ MB Actros, 2024 │ NULL          │ Load, Axles│ NULL             |
│ "Motorcycle"   │ BMW R1250, 2023 │ NULL          │ NULL       │ Sidecar, EngineCC|
└────────────────┴─────────────────┴───────────────┴────────────┘──────────────────┘
```

**Actual Table Structure:**
```
Vehicles
├── Id (int, PK)
├── Discriminator (string) ← Identifies the type
├── Brand (string)
├── Model (string)
├── Year (int)
├── Price (decimal)
├── NumberOfDoors (int, nullable) ← Car only
├── FuelType (string, nullable) ← Car only
├── LoadCapacity (decimal, nullable) ← Truck only
├── NumberOfAxles (int, nullable) ← Truck only
├── HasSidecar (bool, nullable) ← Motorcycle only
└── EngineCC (int, nullable) ← Motorcycle only
```

## Configuration

TPH is the **default behavior** in EF Core. Minimal configuration needed:

```csharp
public class VehicleDbContext : DbContext
{
    public DbSet<Vehicle> Vehicles { get; set; }
    // Derived types are automatically included

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Optional: Customize discriminator
        modelBuilder.Entity<Vehicle>()
            .HasDiscriminator<string>("Discriminator")
            .HasValue<Car>("Car")
            .HasValue<Truck>("Truck")
            .HasValue<Motorcycle>("Motorcycle");
    }
}
```

## Advantages ✅

1. **Simple Structure**: Easy to understand and implement
2. **Fast Queries**: No JOINs required for polymorphic queries
3. **Easy to Maintain**: Single table to manage
4. **Default Strategy**: Works out of the box with zero configuration
5. **Excellent for Polymorphic Queries**: Retrieving all types is very fast

## Disadvantages ❌

1. **Sparse Data**: Many NULL values when hierarchy has many type-specific properties
2. **Storage Waste**: Nullable columns consume space even when NULL
3. **Table Width**: Can become very wide with many derived types
4. **Constraint Limitations**: Cannot enforce NOT NULL on type-specific columns
5. **Schema Visibility**: All properties visible even if not applicable

## Example Queries and Generated SQL

### Query All Vehicles

```csharp
var allVehicles = context.Vehicles.ToList();
```

**Generated SQL:**
```sql
SELECT "v"."Id", "v"."Brand", "v"."Discriminator", "v"."Model",
       "v"."Price", "v"."Year", "v"."FuelType", "v"."NumberOfDoors",
       "v"."EngineCC", "v"."HasSidecar", "v"."LoadCapacity", "v"."NumberOfAxles"
FROM "Vehicles" AS "v"
```

### Query Specific Type (Cars Only)

```csharp
var cars = context.Vehicles.OfType<Car>().ToList();
```

**Generated SQL:**
```sql
SELECT "v"."Id", "v"."Brand", "v"."Model", "v"."Price", "v"."Year",
       "v"."FuelType", "v"."NumberOfDoors"
FROM "Vehicles" AS "v"
WHERE "v"."Discriminator" = 'Car'
```

### Polymorphic Query with Filter

```csharp
var expensiveVehicles = context.Vehicles
    .Where(v => v.Price > 50000)
    .ToList();
```

**Generated SQL:**
```sql
SELECT "v"."Id", "v"."Brand", "v"."Discriminator", ...
FROM "Vehicles" AS "v"
WHERE "v"."Price" > 50000
```

## When to Use TPH

### ✅ Good Use Cases

- **Simple hierarchies** with 2-4 derived types
- **Few type-specific properties** per derived class
- **Frequent polymorphic queries** (querying all types together)
- **Performance-critical reads** where JOINs should be avoided
- **Shared property queries** (e.g., "all vehicles over $50k")

### ❌ Avoid When

- **Many derived types** (creates very wide table)
- **Many type-specific properties** (lots of NULL values)
- **Storage efficiency is critical**
- **Need NOT NULL constraints** on type-specific properties
- **Compliance requires strict data normalization**

## Real-World Scenarios

### Perfect Fit ✅
- **Payment Methods**: CreditCard, BankTransfer, PayPal (2-3 extra fields each)
- **Notifications**: Email, SMS, Push (similar structure, few differences)
- **Vehicle Fleet Management**: Cars, Vans, Trucks (similar base properties)

### Poor Fit ❌
- **Complex Product Catalog**: Electronics, Clothing, Food, etc. (too many specific properties)
- **Extensive User Types**: Admin, Customer, Vendor, Partner (many role-specific fields)
- **Multi-level Hierarchies**: Deep inheritance trees create extremely wide tables

## Running This Demo

### 1. Run the Application

No configuration needed! The SQLite database file is created automatically.

```bash
dotnet restore
dotnet run
```

### 2. Observe the Output

The demo will:
1. Create the SQLite database and single `Vehicles` table
2. Insert sample data (cars, trucks, motorcycles)
3. Query all vehicles polymorphically
4. Query specific types using `OfType<T>()`
5. Demonstrate updates and deletes
6. Show the discriminator column in action

### 3. Inspect the Database

Use DB Browser for SQLite or the command line:

```bash
# Using sqlite3 command line
sqlite3 EFInheritance_TPH.db

# View table structure
.schema Vehicles

# See discriminator values
SELECT Discriminator, COUNT(*)
FROM Vehicles
GROUP BY Discriminator;

# View all data
SELECT * FROM Vehicles;
```

## Key Takeaways

1. TPH is the **default and simplest** inheritance strategy
2. **Best performance** for queries that return multiple types
3. **Trade-off**: Simplicity and speed vs. storage efficiency and normalization
4. **Ideal for**: Simple hierarchies with shared querying patterns
5. **Default choice**: Start with TPH unless you have specific reasons to use TPT or TPC

## Performance Characteristics

| Operation | Performance | Notes |
|-----------|-------------|-------|
| Insert | ⭐⭐⭐⭐⭐ | Single table, very fast |
| Query All Types | ⭐⭐⭐⭐⭐ | No JOINs, excellent |
| Query Specific Type | ⭐⭐⭐⭐⭐ | Simple WHERE clause |
| Update | ⭐⭐⭐⭐⭐ | Single table update |
| Delete | ⭐⭐⭐⭐⭐ | Single table delete |
| Storage Efficiency | ⭐⭐⭐ | NULL values waste space |

## Next Steps

After understanding TPH, explore:
- **TPT** (`2.TPT.TablePerType`) to see normalized table structure
- **TPC** (`3.TPC.TablePerConcreteType`) to see concrete-only tables

Compare query performance and database schemas across all three strategies!
