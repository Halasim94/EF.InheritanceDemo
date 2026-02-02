# Table Per Concrete Type (TPC) - EF Core Inheritance Strategy

## Overview

Table Per Concrete Type (TPC) is an inheritance mapping strategy where **only concrete (non-abstract) classes get database tables**. The base class does NOT have a table, and each concrete class has a table with ALL properties (both inherited and type-specific).

## How It Works

### Key Characteristics

1. **No Base Table**: Abstract base class does NOT get a table
2. **Concrete Tables Only**: Only Car, Truck, Motorcycle get tables (not Vehicle)
3. **Complete Data**: Each table contains ALL properties (base + derived)
4. **No Foreign Keys**: Tables are independent (no FK relationships)
5. **No Discriminator**: Type is determined by which table the data is in
6. **Property Duplication**: Base properties are duplicated in each concrete table

### Database Schema

```
NO Vehicle table! ❌

Instead, three independent tables:

Cars Table
┌────┬───────┬───────┬──────┬───────┬──────┬──────────┐
│ Id │ Brand │ Model │ Year │ Price │ Doors│ FuelType │
└────┴───────┴───────┴──────┴───────┴──────┴──────────┘
(All base properties + car properties)

Trucks Table
┌────┬───────┬───────┬──────┬───────┬──────────┬───────┐
│ Id │ Brand │ Model │ Year │ Price │ Capacity │ Axles │
└────┴───────┴───────┴──────┴───────┴──────────┴───────┘
(All base properties + truck properties)

Motorcycles Table
┌────┬───────┬───────┬──────┬───────┬─────────┬──────┐
│ Id │ Brand │ Model │ Year │ Price │ Sidecar │ EngineCC │
└────┴───────┴───────┴──────┴───────┴─────────┴──────┘
(All base properties + motorcycle properties)
```

**Key Observation:**
- Brand, Model, Year, Price appear in ALL three tables
- No relationships between tables
- Each table is completely independent

## Configuration

TPC requires explicit configuration using `UseTpcMappingStrategy()`:

```csharp
public class VehicleDbContext : DbContext
{
    public DbSet<Vehicle> Vehicles { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // REQUIRED: Explicitly enable TPC strategy
        modelBuilder.Entity<Vehicle>()
            .UseTpcMappingStrategy();

        // Base class (Vehicle) will NOT have a table
        // Only concrete classes (Car, Truck, Motorcycle) get tables
    }
}
```

## Advantages ✅

1. **Fast Specific Queries**: Querying Cars is a simple SELECT from Cars table
2. **No JOINs**: Each table has all needed data, no relationships to navigate
3. **Independent Tables**: Each type is completely isolated
4. **Simple Updates**: Updating a car only touches the Cars table
5. **Type-Specific Constraints**: Can enforce constraints on all properties
6. **No Sparse Data**: Each table contains only relevant columns

## Disadvantages ❌

1. **Data Duplication**: Base properties (Brand, Model, etc.) duplicated in every table
2. **Complex Polymorphic Queries**: Requires UNION ALL across all concrete tables
3. **Schema Changes**: Changing base class requires updating ALL concrete tables
4. **ID Management**: Must ensure IDs are unique across all tables
5. **No Base Table**: Cannot query the base Vehicle table directly
6. **Maintenance**: More tables to maintain and keep in sync

## Example Queries and Generated SQL

### Query All Vehicles (Polymorphic)

```csharp
var allVehicles = context.Vehicles.ToList();
```

**Generated SQL:**
```sql
-- UNION ALL across all concrete tables
SELECT "c"."Id", "c"."Brand", "c"."Model", "c"."Price", "c"."Year",
       "c"."FuelType", "c"."NumberOfDoors",
       NULL AS "EngineCC", NULL AS "HasSidecar",
       NULL AS "LoadCapacity", NULL AS "NumberOfAxles"
FROM "Cars" AS "c"

UNION ALL

SELECT "m"."Id", "m"."Brand", "m"."Model", "m"."Price", "m"."Year",
       NULL, NULL,
       "m"."EngineCC", "m"."HasSidecar",
       NULL, NULL
FROM "Motorcycles" AS "m"

UNION ALL

SELECT "t"."Id", "t"."Brand", "t"."Model", "t"."Price", "t"."Year",
       NULL, NULL, NULL, NULL,
       "t"."LoadCapacity", "t"."NumberOfAxles"
FROM "Trucks" AS "t"
```

### Query Specific Type (Cars Only)

```csharp
var cars = context.Vehicles.OfType<Car>().ToList();
```

**Generated SQL:**
```sql
-- Simple, fast query - TPC advantage!
SELECT "c"."Id", "c"."Brand", "c"."Model", "c"."Price", "c"."Year",
       "c"."FuelType", "c"."NumberOfDoors"
FROM "Cars" AS "c"
```

### Query with Filter on Base Property

```csharp
var expensiveVehicles = context.Vehicles
    .Where(v => v.Price > 50000)
    .ToList();
```

**Generated SQL:**
```sql
-- UNION ALL with WHERE in each subquery
SELECT ... FROM "Cars" WHERE "Price" > 50000
UNION ALL
SELECT ... FROM "Motorcycles" WHERE "Price" > 50000
UNION ALL
SELECT ... FROM "Trucks" WHERE "Price" > 50000
```

## When to Use TPC

### ✅ Good Use Cases

- **Performance for specific types** is critical
- **Rarely query polymorphically** (across all types)
- **Each type is largely independent** and self-contained
- **Want to avoid JOINs** entirely
- **Type-specific queries dominate** your workload
- **Each concrete type has distinct lifecycle** or access patterns

### ❌ Avoid When

- **Frequent polymorphic queries** (e.g., "all vehicles over $50k")
- **Base class properties change frequently** (requires updating all tables)
- **Need referential integrity** between base and derived
- **Many shared properties** (high duplication cost)
- **ID uniqueness is complex** to manage

## Real-World Scenarios

### Perfect Fit ✅
- **Multi-Tenant Systems**: Each tenant type has its own table, rarely queried together
- **Historical Data**: Current/Archived records in separate tables
- **Geographic Partitioning**: US/EU/Asia customers in separate tables
- **Log Types**: ErrorLogs, AuditLogs, PerformanceLogs (queried independently)

### Poor Fit ❌
- **Product Catalog**: Frequent cross-category searches
- **User Management**: Admin/Customer/Vendor often queried together
- **Content Management**: Pages/Posts/Articles frequently searched across types
- **E-commerce Orders**: Different order types often queried together

## Running This Demo

### 1. Run the Application

No configuration needed! The SQLite database file is created automatically.

```bash
dotnet restore
dotnet run
```

### 2. Observe the Output

The demo will:
1. Create three tables: Cars, Trucks, Motorcycles (NO Vehicles table!)
2. Insert data into independent tables
3. Query specific types (fast, no JOINs)
4. Query all types polymorphically (UNION ALL)
5. Demonstrate updates and deletes

### 3. Inspect the Database

Use DB Browser for SQLite or the command line:

```bash
# Using sqlite3 command line
sqlite3 EFInheritance_TPC.db

# List all tables - note NO Vehicles table!
.tables
# Should show: Cars  Motorcycles  Trucks

# View concrete table structures
.schema Cars
.schema Trucks
.schema Motorcycles

# Notice: Each table has ALL properties (base + derived)
PRAGMA table_info(Cars);

# Query all vehicles with UNION ALL
SELECT 'Car' as Type, Brand, Model, Price FROM Cars
UNION ALL
SELECT 'Truck', Brand, Model, Price FROM Trucks
UNION ALL
SELECT 'Motorcycle', Brand, Model, Price FROM Motorcycles
ORDER BY Price DESC;
```

## Performance Characteristics

| Operation | Performance | Notes |
|-----------|-------------|-------|
| Insert | ⭐⭐⭐⭐⭐ | Single table, very fast |
| Query All Types | ⭐⭐ | Requires UNION ALL (slower) |
| Query Specific Type | ⭐⭐⭐⭐⭐ | No JOINs, excellent |
| Update | ⭐⭐⭐⭐⭐ | Single table update |
| Delete | ⭐⭐⭐⭐⭐ | Single table delete |
| Storage Efficiency | ⭐⭐⭐ | Duplicates base properties |
| Schema Changes | ⭐⭐ | Must update all concrete tables |

## Key Takeaways

1. TPC is **optimized for type-specific queries**
2. **No base table** - only concrete classes have tables
3. **Best performance** for querying specific types
4. **Worst performance** for polymorphic queries (UNION ALL)
5. **Trade-off**: Query speed for specific types vs. data duplication
6. **Use when**: You know the specific type at query time

## Comparison with Other Strategies

| Aspect | TPH | TPT | TPC |
|--------|-----|-----|-----|
| Base Table | Yes (all data) | Yes (base props) | No |
| Concrete Tables | No | Yes (derived props) | Yes (all props) |
| JOINs | Never | Always | Only polymorphic |
| Duplication | No | No | Yes |
| Specific Type Query | Fast | Medium | Fastest |
| Polymorphic Query | Fastest | Medium | Slowest |

## ID Management

**IMPORTANT**: With TPC, you must ensure IDs are unique across ALL concrete tables.

EF Core handles this by default using:
- **AUTOINCREMENT** (SQLite)
- **Identity columns** with shared sequences (SQL Server, PostgreSQL)
- **Manual ID generation** in code

SQLite uses AUTOINCREMENT to ensure unique IDs across all tables automatically.

## Next Steps

After understanding TPC, compare with:
- **TPH** (`1.TPH.TablePerHierarchy`) to see single-table simplicity
- **TPT** (`2.TPT.TablePerType`) to see normalized structure

Run the same queries in all three projects and compare:
- Database schema (table count and structure)
- Generated SQL (JOINs vs UNION ALL vs simple SELECT)
- Performance implications
- Code complexity

## Summary

TPC is a **specialized strategy** that trades data duplication for query performance on specific types. It works best when:
- You mostly query specific types (not polymorphically)
- Each type is largely independent
- You want to avoid JOINs
- Performance for type-specific queries is critical

For most applications, **TPH (default)** or **TPT (normalized)** are better choices!
