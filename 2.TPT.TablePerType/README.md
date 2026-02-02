# Table Per Type (TPT) - EF Core Inheritance Strategy

## Overview

Table Per Type (TPT) is an inheritance mapping strategy where **each class in the hierarchy gets its own table**. Tables for derived classes contain only type-specific properties and have a foreign key relationship to the base table.

## How It Works

### Key Characteristics

1. **One Table Per Type**: Every class (base and derived) has its own table
2. **Foreign Key Relationships**: Derived tables reference the base table via PK/FK
3. **No Discriminator**: Type is determined by which child table has a matching record
4. **Normalized Data**: No NULL values for type-specific properties
5. **JOINs Required**: Queries need to JOIN base and derived tables

### Database Schema

```
Table: Vehicles (Base)
┌────┬───────┬───────┬──────┬───────┐
│ Id │ Brand │ Model │ Year │ Price │
└────┴───────┴───────┴──────┴───────┘
        ↑           ↑           ↑
        │           │           │
   ┌────┴────┐ ┌────┴─────┐ ┌──┴──────┐
   │ Cars    │ │ Trucks   │ │ Motorcycles │
   ├────┬────┤ ├────┬─────┤ ├────┬─────┤
   │ Id │...││ │ Id │... ││ │ Id │...  │
   │(FK)│    │ │(FK)│     │ │(FK)│     │
   └────┴────┘ └────┴─────┘ └────┴─────┘
```

**Actual Tables:**

```sql
-- Base table: Common properties
Vehicles
├── Id (PK)
├── Brand
├── Model
├── Year
└── Price

-- Derived table: Car-specific properties only
Cars
├── Id (PK, FK → Vehicles.Id)
├── NumberOfDoors
└── FuelType

-- Derived table: Truck-specific properties only
Trucks
├── Id (PK, FK → Vehicles.Id)
├── LoadCapacity
└── NumberOfAxles

-- Derived table: Motorcycle-specific properties only
Motorcycles
├── Id (PK, FK → Vehicles.Id)
├── HasSidecar
└── EngineCC
```

## Configuration

TPT requires explicit configuration using `UseTptMappingStrategy()`:

```csharp
public class VehicleDbContext : DbContext
{
    public DbSet<Vehicle> Vehicles { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // REQUIRED: Explicitly enable TPT strategy
        modelBuilder.Entity<Vehicle>()
            .UseTptMappingStrategy();

        // Tables are automatically named after entity types:
        // - Vehicles (base)
        // - Cars, Trucks, Motorcycles (derived)
    }
}
```

## Advantages ✅

1. **Fully Normalized**: No NULL values, each table contains only relevant data
2. **Clean Schema**: Easy to understand table structure
3. **Type-Specific Constraints**: Can enforce NOT NULL on derived properties
4. **Storage Efficient**: No wasted space on NULL values
5. **Clear Separation**: Each type has its own table for better organization
6. **Easy Migrations**: Adding properties to derived types doesn't affect other tables

## Disadvantages ❌

1. **Performance Overhead**: Every query requires JOINs (base + derived table)
2. **Complex Queries**: Polymorphic queries JOIN multiple tables with UNION
3. **Multiple Writes**: Insert/Update/Delete operations touch multiple tables
4. **Deep Hierarchies**: More levels = more JOINs = worse performance
5. **Query Complexity**: Generated SQL is more complex than TPH

## Example Queries and Generated SQL

### Query All Vehicles (Polymorphic)

```csharp
var allVehicles = context.Vehicles.ToList();
```

**Generated SQL:**
```sql
-- Complex query with UNION ALL to combine all types
SELECT "v"."Id", "v"."Brand", "v"."Model", "v"."Price", "v"."Year",
       "c"."FuelType", "c"."NumberOfDoors",
       NULL AS "EngineCC", NULL AS "HasSidecar",
       NULL AS "LoadCapacity", NULL AS "NumberOfAxles",
       'Car' AS "Discriminator"
FROM "Vehicles" AS "v"
INNER JOIN "Cars" AS "c" ON "v"."Id" = "c"."Id"

UNION ALL

SELECT "v"."Id", "v"."Brand", "v"."Model", "v"."Price", "v"."Year",
       NULL, NULL,
       "m"."EngineCC", "m"."HasSidecar",
       NULL, NULL,
       'Motorcycle'
FROM "Vehicles" AS "v"
INNER JOIN "Motorcycles" AS "m" ON "v"."Id" = "m"."Id"

UNION ALL

SELECT "v"."Id", "v"."Brand", "v"."Model", "v"."Price", "v"."Year",
       NULL, NULL, NULL, NULL,
       "t"."LoadCapacity", "t"."NumberOfAxles",
       'Truck'
FROM "Vehicles" AS "v"
INNER JOIN "Trucks" AS "t" ON "v"."Id" = "t"."Id"
```

### Query Specific Type (Cars Only)

```csharp
var cars = context.Vehicles.OfType<Car>().ToList();
```

**Generated SQL:**
```sql
-- Simpler: Just JOIN two tables
SELECT "v"."Id", "v"."Brand", "v"."Model", "v"."Price", "v"."Year",
       "c"."FuelType", "c"."NumberOfDoors"
FROM "Vehicles" AS "v"
INNER JOIN "Cars" AS "c" ON "v"."Id" = "c"."Id"
```

### Query with Filter

```csharp
var expensiveCars = context.Vehicles
    .OfType<Car>()
    .Where(c => c.Price > 30000)
    .ToList();
```

**Generated SQL:**
```sql
SELECT "v"."Id", "v"."Brand", "v"."Model", "v"."Price", "v"."Year",
       "c"."FuelType", "c"."NumberOfDoors"
FROM "Vehicles" AS "v"
INNER JOIN "Cars" AS "c" ON "v"."Id" = "c"."Id"
WHERE "v"."Price" > 30000
```

## When to Use TPT

### ✅ Good Use Cases

- **Normalized data requirements** (reporting, compliance, analytics)
- **Many type-specific properties** per derived type
- **Storage efficiency matters** (no wasted NULL values)
- **Need strong constraints** on type-specific properties
- **Infrequent polymorphic queries** (mostly query specific types)
- **Complex reporting** that benefits from separate tables

### ❌ Avoid When

- **High-performance requirements** (JOINs add overhead)
- **Frequent polymorphic queries** (UNION ALL is complex)
- **Deep inheritance hierarchies** (many levels = many JOINs)
- **Simple hierarchies** (TPH would be simpler and faster)
- **Read-heavy applications** where query speed is critical

## Real-World Scenarios

### Perfect Fit ✅
- **Employee Management**: Different tables for Managers, Engineers, Sales (many role-specific fields)
- **Financial Instruments**: Stocks, Bonds, Options (complex, distinct properties)
- **Medical Records**: Different exam types with vastly different data structures
- **Product Catalog**: When categories have 10+ unique properties each

### Poor Fit ❌
- **Notifications**: Email, SMS, Push (TPH is faster and simpler)
- **Payment Methods**: Few properties, frequent polymorphic queries
- **Simple Content Types**: Blog posts, pages (minimal property differences)

## Running This Demo

### 1. Run the Application

No configuration needed! The SQLite database file is created automatically.

```bash
dotnet restore
dotnet run
```

### 2. Observe the Output

The demo will:
1. Create four tables: Vehicles, Cars, Trucks, Motorcycles
2. Insert data with foreign key relationships
3. Query specific types (simple JOINs)
4. Query all types polymorphically (UNION ALL)
5. Demonstrate updates and deletes across tables

### 3. Inspect the Database

Use DB Browser for SQLite or the command line:

```bash
# Using sqlite3 command line
sqlite3 EFInheritance_TPT.db

# View table structures
.schema Vehicles
.schema Cars
.schema Trucks
.schema Motorcycles

# List all tables
.tables

# Query with JOINs
SELECT v.*, c.*
FROM Vehicles v
INNER JOIN Cars c ON v.Id = c.Id;
```

## Performance Characteristics

| Operation | Performance | Notes |
|-----------|-------------|-------|
| Insert | ⭐⭐⭐ | Writes to 2 tables (base + derived) |
| Query All Types | ⭐⭐ | Requires UNION ALL across all tables |
| Query Specific Type | ⭐⭐⭐⭐ | JOIN base + one derived table |
| Update | ⭐⭐⭐ | May update multiple tables |
| Delete | ⭐⭐⭐ | Cascade delete across tables |
| Storage Efficiency | ⭐⭐⭐⭐⭐ | No NULL values, fully normalized |

## Key Takeaways

1. TPT is the **most normalized** inheritance strategy
2. **Best for data integrity** and complex type-specific properties
3. **Trade-off**: Normalization and constraints vs. query performance
4. **Use when**: Data structure and integrity > query speed
5. **Avoid when**: Performance is critical and polymorphic queries are frequent

## Comparison with Other Strategies

| Aspect | TPH | TPT | TPC |
|--------|-----|-----|-----|
| Tables | 1 | N (all types) | N-1 (concrete only) |
| JOINs | None | Always | Only for polymorphic |
| Normalization | Poor | Excellent | Medium |
| Query Speed | Fastest | Slowest | Fast for specific types |

## Next Steps

After understanding TPT, explore:
- **TPH** (`1.TPH.TablePerHierarchy`) to see the single-table approach
- **TPC** (`3.TPC.TablePerConcreteType`) to see concrete-only tables

Compare the generated SQL and database schemas to understand the trade-offs!
