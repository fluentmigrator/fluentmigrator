# SQL Server Provider

FluentMigrator provides comprehensive support for Microsoft SQL Server, including specific features and optimizations for different versions.

## Supported Versions

FluentMigrator supports:
- **SQL Server 2019** ✅ (Recommended)
- **SQL Server 2017** ✅
- **SQL Server 2016** ✅
- **SQL Server 2014** ✅
- **SQL Server 2012** ✅
- **SQL Server 2008/R2** ⚠️ (Limited support)
- **Azure SQL Database** ✅
- **Azure SQL Managed Instance** ✅

## Installation

Install the SQL Server provider package:

```bash
# For .NET CLI
dotnet add package FluentMigrator.Runner.SqlServer

# For Package Manager Console
Install-Package FluentMigrator.Runner.SqlServer
```

## Configuration

### Basic Configuration
```csharp
services.AddFluentMigratorCore()
    .ConfigureRunner(rb => rb
        .AddSqlServer()
        .WithGlobalConnectionString("Server=.;Database=MyApp;Trusted_Connection=true;")
        .ScanIn(typeof(MyMigration).Assembly).For.Migrations());
```
## SQL Server Extensions Package

For advanced SQL Server features, install the extensions package:

```xml
<PackageReference Include="FluentMigrator.Extensions.SqlServer" Version="7.2.0" />
```

## SQL Server Specific Features

### Identity Columns

#### Custom Identity Seed and Increment
```csharp
Create.Table("Orders")
    .WithColumn("Id").AsInt32().NotNullable().PrimaryKey().Identity(1000, 5) // Start at 1000, increment by 5
    .WithColumn("OrderNumber").AsString(20).NotNullable();

// Advanced identity options
Create.Table("Invoices")
    .WithColumn("Id").AsInt32().NotNullable().PrimaryKey()
        .Identity(1000, 5) // Start at 1000, increment by 5
    .WithColumn("InvoiceNumber").AsString(20).NotNullable();
```

### Data Types

Column types are specified in the DBMS specific type map classes :

* [SQL Server 2000](https://github.com/fluentmigrator/fluentmigrator/blob/main/src/FluentMigrator.Runner.SqlServer/Generators/SqlServer/SqlServer2000TypeMap.cs)
* [SQL Server 2005](https://github.com/fluentmigrator/fluentmigrator/blob/main/src/FluentMigrator.Runner.SqlServer/Generators/SqlServer/SqlServer2005TypeMap.cs)
* [SQL Server 2008+](https://github.com/fluentmigrator/fluentmigrator/blob/main/src/FluentMigrator.Runner.SqlServer/Generators/SqlServer/SqlServer2008TypeMap.cs)

### Indexes and Index Extensions

#### Clustered and Non-Clustered Indexes
```csharp
Create.Table("Users")
    .WithColumn("Id").AsInt32().NotNullable().PrimaryKey().Identity()
    .WithColumn("Username").AsString(50).NotNullable()
    .WithColumn("Email").AsString(255).NotNullable()
    .WithColumn("LastName").AsString(100).NotNullable()
    .WithColumn("FirstName").AsString(100).NotNullable();

// Clustered index (primary key is clustered by default)
Create.Index("IX_Users_Username").OnTable("Users")
    .OnColumn("Username").Ascending()
    .WithOptions()
        .Clustered();

// Non-clustered index
Create.Index("IX_Users_Email").OnTable("Users")
    .OnColumn("Email").Ascending()
    .WithOptions()
        .NonClustered();
```

#### Included Columns (Covering Indexes)
```csharp
using FluentMigrator.SqlServer;

Create.Index("IX_Users_LastName_Covering").OnTable("Users")
    .OnColumn("LastName").Ascending()
    .WithOptions()
        .NonClustered()
        .Include("FirstName")
        .Include("Email")
        .Include("PhoneNumber");

// Complex covering index
Create.Index("IX_Orders_Covering").OnTable("Orders")
    .OnColumn("CustomerId").Ascending()
    .OnColumn("OrderDate").Descending()
    .WithOptions()
        .NonClustered()
        .Include("TotalAmount")
        .Include("Status")
        .Include("ShippingAddress");
```

#### Filtered Indexes
```csharp
Create.Index("IX_Users_Active").OnTable("Users")
    .OnColumn("LastName").Ascending()
    .WithOptions()
        .NonClustered()
        .Filter("[IsActive] = 1");
```

### Sequences

#### Creating Sequences
```csharp
Create.Sequence("OrderNumberSeq")
    .InSchema("dbo")
    .StartWith(1000)
    .IncrementBy(1)
    .MinValue(1000)
    .MaxValue(999999)
    .Cache(50);
```

#### Using Sequences in Tables
```csharp
Create.Table("Orders")
    .WithColumn("Id").AsInt32().NotNullable().PrimaryKey().Identity()
    .WithColumn("OrderNumber").AsInt32().NotNullable()
        .WithDefaultValue(RawSql.Insert("NEXT VALUE FOR OrderNumberSeq"));
```

### Identity Insert Operations

#### WithIdentityInsert Extension
For inserting explicit values into identity columns, use the `WithIdentityInsert()` extension method:

```csharp
using FluentMigrator.SqlServer;

[Migration(1)]
public class InsertExplicitIdentityValues : Migration
{
    public override void Up()
    {
        Create.Table("Users")
            .WithColumn("Id").AsInt32().NotNullable().PrimaryKey().Identity()
            .WithColumn("Name").AsString(100).NotNullable();

        // Insert explicit identity values using WithIdentityInsert()
        Insert.IntoTable("Users")
            .WithIdentityInsert()
            .Row(new { Id = 100, Name = "Admin" })
            .Row(new { Id = 200, Name = "Manager" });

        // Regular insert without explicit identity values
        Insert.IntoTable("Users")
            .Row(new { Name = "Regular User" }); // Id will be auto-generated
    }

    public override void Down()
    {
        Delete.Table("Users");
    }
}
```

#### Batch Identity Inserts
```csharp
[Migration(2)]
public class BatchIdentityInserts : Migration
{
    public override void Up()
    {
        // Insert multiple rows with explicit identity values
        Insert.IntoTable("Users")
            .WithIdentityInsert()
            .Row(new { Id = 1, Name = "System Admin" })
            .Row(new { Id = 2, Name = "Database Admin" })
            .Row(new { Id = 1000, Name = "Special User" });
    }

    public override void Down()
    {
        Delete.FromTable("Users").AllRows();
    }
}
```

#### Important Notes
- `WithIdentityInsert()` automatically handles the SQL Server `IDENTITY_INSERT` setting
- You can mix explicit identity values with regular inserts in the same migration
- The identity seed will continue from the highest inserted value
- Use with caution in production environments to avoid primary key conflicts

## Azure SQL Database Considerations

### Differences from On-Premises SQL Server
- No physical file operations
- Limited administrative commands
- Different pricing tiers affect performance
- Automatic backup and recovery
