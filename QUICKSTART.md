# Quick Start Guide - EF Core Inheritance Demo

## Prerequisites

Before running this demo, ensure you have:

1. **.NET 10 SDK** installed
   ```bash
   dotnet --version  # Should show 10.x.x
   ```

2. **IDE** (optional but recommended)
   - Visual Studio 2022+
   - VS Code with C# extension
   - JetBrains Rider

**That's it!** This demo uses SQLite - no database server installation required!

## Step 1: Build the Solution

**No configuration needed!** Each project uses SQLite and will automatically create its database file:
- `EFInheritance_TPH.db`
- `EFInheritance_TPT.db`
- `EFInheritance_TPC.db`

```bash
# Navigate to the solution directory
cd EF.InheritanceDemo

# Build all projects
dotnet build
```

You should see:
```
Build succeeded.
    0 Warning(s)
    0 Error(s)
```

## Step 2: Run Each Project

### Option A: Run from Command Line

```bash
# TPH (Table Per Hierarchy)
cd 1.TPH.TablePerHierarchy
dotnet run

# TPT (Table Per Type)
cd ../2.TPT.TablePerType
dotnet run

# TPC (Table Per Concrete Type)
cd ../3.TPC.TablePerConcreteType
dotnet run
```

### Option B: Run from Visual Studio

1. Open `EF.InheritanceDemo.sln` in Visual Studio
2. Right-click on `EF.TPH` project → Set as Startup Project
3. Press F5 or click Run
4. Repeat for `EF.TPT` and `EF.TPC` projects

### Option C: Run from VS Code

1. Open the folder in VS Code
2. Press Ctrl+Shift+P (or Cmd+Shift+P on Mac)
3. Type "Terminal: Create New Terminal"
4. Navigate to each project and run `dotnet run`

## Step 3: Verify Database Creation

After running each project, you'll see `.db` files created in each project folder:
- `1.TPH.TablePerHierarchy/EFInheritance_TPH.db`
- `2.TPT.TablePerType/EFInheritance_TPT.db`
- `3.TPC.TablePerConcreteType/EFInheritance_TPC.db`

You can open these with:
- **DB Browser for SQLite** (free tool)
- **VS Code SQLite extensions**
- **Command line:** `sqlite3 EFInheritance_TPH.db`

## Step 4: Explore the Data

### TPH (Single Table)
```bash
sqlite3 1.TPH.TablePerHierarchy/EFInheritance_TPH.db
```
```sql
-- See all vehicles with discriminator
SELECT Discriminator, Brand, Model, Price
FROM Vehicles
ORDER BY Discriminator, Id;

-- Notice: All types in one table with many NULL values
SELECT * FROM Vehicles;
```

### TPT (Normalized Tables)
```bash
sqlite3 2.TPT.TablePerType/EFInheritance_TPT.db
```
```sql
-- See base data
SELECT * FROM Vehicles;

-- See joined data
SELECT v.Brand, v.Model, c.FuelType, c.NumberOfDoors
FROM Vehicles v
INNER JOIN Cars c ON v.Id = c.Id;
```

### TPC (Concrete Tables Only)
```bash
sqlite3 3.TPC.TablePerConcreteType/EFInheritance_TPC.db
```
```sql
-- Each table has ALL properties
SELECT * FROM Cars;
SELECT * FROM Motorcycles;
SELECT * FROM Trucks;

-- UNION ALL to see all vehicles
SELECT 'Car' as Type, Brand, Model, Price FROM Cars
UNION ALL
SELECT 'Motorcycle', Brand, Model, Price FROM Motorcycles
UNION ALL
SELECT 'Truck', Brand, Model, Price FROM Trucks
ORDER BY Price DESC;
```

## Understanding the Output

Each project displays:

1. **Database initialization** confirmation
2. **Demonstration 1-9** showing different query patterns:
   - Polymorphic queries (all types)
   - Specific type queries
   - Filtering by base/derived properties
   - Insert operations
   - Update operations
   - Delete operations
   - Statistics and counts

3. **SQL output** in the console (EF Core logging enabled)
   - Watch for JOINs in TPT
   - Watch for UNION ALL in TPC polymorphic queries
   - Notice simple queries in TPH

4. **Strategy summary** explaining pros/cons

## Troubleshooting

### Issue: Database File Locked
**Error:** `SQLite Error: database is locked`

**Solutions:**
1. Close any SQLite viewers that have the database open
2. Stop the application and try again
3. Delete the `.db` file and run again (will recreate with seed data)

### Issue: Build Failed
**Error:** `The SDK 'Microsoft.NET.Sdk' specified could not be found`

**Solution:** Install .NET 10 SDK from https://dotnet.microsoft.com/download

### Issue: Package Restore Failed
**Error:** `Package 'Microsoft.EntityFrameworkCore' is not found`

**Solution:**
```bash
dotnet restore
# or
dotnet nuget locals all --clear
dotnet restore
```

### Issue: .NET 10 Not Available
If .NET 10 is not released yet, downgrade to .NET 8:

1. Edit all `.csproj` files
2. Change `<TargetFramework>net10.0</TargetFramework>`
3. To `<TargetFramework>net8.0</TargetFramework>`
4. Change package versions to `8.0.0`

## Next Steps

1. **Read the READMEs** - Each project has detailed documentation:
   - `README.md` (solution overview - comparison table)
   - `1.TPH.TablePerHierarchy/README.md` (TPH details)
   - `2.TPT.TablePerType/README.md` (TPT details)
   - `3.TPC.TablePerConcreteType/README.md` (TPC details)

2. **Review the Code** - Extensively commented:
   - Model classes explain property mapping
   - DbContext classes explain configuration
   - Program.cs files demonstrate usage patterns

3. **Experiment** - Modify and test:
   - Add new vehicle types
   - Add new properties
   - Try different queries
   - Generate migrations: `dotnet ef migrations add InitialCreate`

4. **Compare** - Run all three projects and compare:
   - Database schemas (use `\d table_name` in psql)
   - Generated SQL (in console output)
   - Performance implications

## Learning Path

**Recommended order:**

1. **Start with TPH** (`1.TPH.TablePerHierarchy`)
   - Simplest to understand
   - Default EF Core behavior
   - See single-table approach

2. **Move to TPT** (`2.TPT.TablePerType`)
   - Understand normalization
   - See JOINs in action
   - Compare with TPH schema

3. **Finish with TPC** (`3.TPC.TablePerConcreteType`)
   - Most complex trade-offs
   - See UNION ALL queries
   - Understand when to use it

## Additional Resources

- [EF Core Documentation](https://learn.microsoft.com/en-us/ef/core/)
- [EF Core Inheritance](https://learn.microsoft.com/en-us/ef/core/modeling/inheritance)
- [EF Core SQLite Provider](https://learn.microsoft.com/en-us/ef/core/providers/sqlite/)
- [DB Browser for SQLite](https://sqlitebrowser.org/) - Free SQLite database viewer

## Support

For issues with this demo:
1. Check the project-specific README.md
2. Review code comments
3. Ensure .NET 10 SDK is installed
4. Delete .db files and run again if you encounter database issues

Happy learning! 🚀
