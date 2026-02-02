# Entity Framework Core Inheritance Mapping Strategies Demo

This solution demonstrates the three inheritance mapping strategies available in Entity Framework Core 7+ using a Vehicle domain model with SQLite.

## Overview

Entity Framework Core supports three strategies for mapping inheritance hierarchies to relational databases:

1. **Table Per Hierarchy (TPH)** - Default strategy, all types in one table
2. **Table Per Type (TPT)** - Separate table for each type with foreign keys
3. **Table Per Concrete Type (TPC)** - Separate table for each concrete class only

## Domain Model

All three projects use the same Vehicle hierarchy for easy comparison:

```
Vehicle (Base Class)
├── Car
├── Motorcycle
└── Truck
```

**Properties:**
- **Vehicle**: Id, Brand, Model, Year, Price
- **Car**: NumberOfDoors, FuelType (e.g., Volkswagen Golf, Peugeot 3008)
- **Motorcycle**: HasSidecar, EngineCC (e.g., BMW R 1250 GS, Ducati Monster)
- **Truck**: LoadCapacity, NumberOfAxles (e.g., Mercedes-Benz Actros, Scania R 500)

## Strategy Comparison

| Feature | TPH | TPT | TPC |
|---------|-----|-----|-----|
| **Tables** | 1 table for all types | 1 table per type | 1 table per concrete type |
| **Discriminator** | Required | Not used | Not used |
| **Queries** | Fast (no JOINs) | Slower (requires JOINs) | Fast for specific types |
| **Polymorphic Queries** | Excellent | Good (uses JOINs) | Complex (uses UNION ALL) |
| **Data Normalization** | Poor (sparse columns) | Excellent | Poor (column duplication) |
| **Storage** | Less efficient (NULL values) | Efficient | Medium (duplicate base columns) |
| **Schema Changes** | Easy | Medium (multiple tables) | Medium (multiple tables) |
| **Use Case** | Default choice, simple hierarchies | Normalized data, reporting | Performance-critical, rarely polymorphic |

## Prerequisites

- .NET 10 SDK
- Visual Studio 2022+ or VS Code with C# extension
- Basic knowledge of Entity Framework Core

**Note:** This demo uses SQLite - no database server installation required!

## Getting Started

### 1. Clone or Download

Place this solution in your desired directory.

### 2. Database Files

Each project uses a separate SQLite database file (automatically created):
- `EFInheritance_TPH.db`
- `EFInheritance_TPT.db`
- `EFInheritance_TPC.db`

No configuration needed - just run the projects!

### 3. Run Each Project

Navigate to each project folder and run:

```bash
# Navigate to project directory
cd 1.TPH.TablePerHierarchy

# Restore packages
dotnet restore

# Run the demo
dotnet run
```

Repeat for each project to see the differences in action.

### 4. View Generated Database Schema

Use a SQLite viewer (DB Browser for SQLite, VS Code extensions, or command line):

```bash
# Using sqlite3 command line
sqlite3 EFInheritance_TPH.db ".tables"
sqlite3 EFInheritance_TPH.db "SELECT * FROM Vehicles;"

# TPH: Single table
# TPT: Multiple related tables (Vehicles, Cars, Motorcycles, Trucks)
# TPC: Only concrete tables (Cars, Motorcycles, Trucks - no Vehicles)
```

## Project Structure

```
EF.InheritanceDemo/
├── EF.InheritanceDemo.sln          # Solution file
├── README.md                        # This file
├── 1.TPH.TablePerHierarchy/        # Table Per Hierarchy demo
│   ├── EF.TPH.csproj
│   ├── README.md                    # TPH-specific documentation
│   ├── Program.cs                   # Demo application
│   ├── Models/                      # Domain models
│   └── Data/                        # DbContext
├── 2.TPT.TablePerType/             # Table Per Type demo
│   ├── EF.TPT.csproj
│   ├── README.md                    # TPT-specific documentation
│   ├── Program.cs
│   ├── Models/
│   └── Data/
└── 3.TPC.TablePerConcreteType/     # Table Per Concrete Type demo
    ├── EF.TPC.csproj
    ├── README.md                    # TPC-specific documentation
    ├── Program.cs
    ├── Models/
    └── Data/
```

## When to Use Each Strategy

### Table Per Hierarchy (TPH) ✅ Default Choice
**Use when:**
- You have a simple hierarchy with few derived types
- Query performance is critical
- Null columns are acceptable
- You need simple polymorphic queries

**Avoid when:**
- You have many type-specific properties (creates many nullable columns)
- Data normalization is critical
- Storage efficiency is a concern

### Table Per Type (TPT) 📊 Normalized
**Use when:**
- Data normalization is important
- You have many type-specific properties
- Storage efficiency matters
- You need complex reporting across types

**Avoid when:**
- Query performance is critical (JOINs add overhead)
- You have deep inheritance hierarchies (many JOINs)
- You primarily query specific types

### Table Per Concrete Type (TPC) 🚀 Performance
**Use when:**
- Query performance for specific types is critical
- You rarely perform polymorphic queries
- Each type is largely independent
- You want to avoid JOINs

**Avoid when:**
- You frequently query across all types polymorphically
- You want to avoid data duplication
- Base class properties change frequently

## Learning Path

1. **Start with TPH** (`1.TPH.TablePerHierarchy`) - Understand the default behavior
2. **Move to TPT** (`2.TPT.TablePerType`) - See how normalization works
3. **Explore TPC** (`3.TPC.TablePerConcreteType`) - Understand the performance trade-offs

Each project includes:
- Extensive code comments explaining concepts
- Seed data for testing
- CRUD operation examples
- Query demonstrations
- Generated SQL output

## Additional Resources

- [EF Core Inheritance Documentation](https://learn.microsoft.com/en-us/ef/core/modeling/inheritance)
- [EF Core SQLite Provider](https://learn.microsoft.com/en-us/ef/core/providers/sqlite/)
- [EF Core Performance](https://learn.microsoft.com/en-us/ef/core/performance/)
- [DB Browser for SQLite](https://sqlitebrowser.org/) - Free database viewer

## Support

For issues or questions:
- Check individual project README.md files
- Review code comments in each project
- Consult official EF Core documentation

## License

This demo is provided as-is for educational purposes.
