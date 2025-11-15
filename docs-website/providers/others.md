# Other Database Providers

FluentMigrator supports a wide range of database providers beyond the major ones (SQL Server, PostgreSQL, MySQL, SQLite, Oracle). This guide covers additional database providers and how to work with them.

## Supported Database Providers

### Firebird

Firebird is an open-source relational database management system.

#### Installation

```bash
# For .NET CLI
dotnet add package FluentMigrator.Runner.Firebird

# For Package Manager Console
Install-Package FluentMigrator.Runner.Firebird
```

#### Configuration

```csharp
services.AddFluentMigratorCore()
    .ConfigureRunner(rb => rb
        .AddFirebird()
        .WithGlobalConnectionString(connectionString)
        .ScanIn(Assembly.GetExecutingAssembly()).For.Migrations())
    .AddLogging(lb => lb.AddFluentMigratorConsole());
```

### IBM DB2

IBM DB2 is an enterprise-class database management system.

#### Installation

```bash
# For .NET CLI
dotnet add package FluentMigrator.Runner.DB2

# For Package Manager Console
Install-Package FluentMigrator.Runner.DB2
```

#### Configuration

```csharp
services.AddFluentMigratorCore()
    .ConfigureRunner(rb => rb
        .AddDb2()
        .WithGlobalConnectionString(connectionString)
        .ScanIn(Assembly.GetExecutingAssembly()).For.Migrations())
    .AddLogging(lb => lb.AddFluentMigratorConsole());
```

### SAP HANA

SAP HANA is an in-memory database platform.

#### Installation

```bash
# For .NET CLI
dotnet add package FluentMigrator.Runner.Hana

# For Package Manager Console
Install-Package FluentMigrator.Runner.Hana
```

#### Configuration

```csharp
services.AddFluentMigratorCore()
    .ConfigureRunner(rb => rb
        .AddHana()
        .WithGlobalConnectionString(connectionString)
        .ScanIn(Assembly.GetExecutingAssembly()).For.Migrations())
    .AddLogging(lb => lb.AddFluentMigratorConsole());
```

### Redshift

Amazon Redshift is a cloud-based data warehouse service.

#### Installation

```bash
# For .NET CLI
dotnet add package FluentMigrator.Runner.Redshift

# For Package Manager Console
Install-Package FluentMigrator.Runner.Redshift
```

#### Configuration

```csharp
services.AddFluentMigratorCore()
    .ConfigureRunner(rb => rb
        .AddRedshift()
        .WithGlobalConnectionString(connectionString)
        .ScanIn(Assembly.GetExecutingAssembly()).For.Migrations())
    .AddLogging(lb => lb.AddFluentMigratorConsole());
```

## Generic Database Provider Support

### Using ODBC Connections

For databases not directly supported, you can use ODBC connections:

```csharp
public class OdbcMigration : Migration
{
    public override void Up()
    {
        IfDatabase("Generic").Execute.Sql(@"
            CREATE TABLE generic_table (
                id INTEGER NOT NULL,
                name VARCHAR(100) NOT NULL,
                amount DECIMAL(10,2),
                created_date TIMESTAMP
            )");
    }

    public override void Down()
    {
        Execute.Sql("DROP TABLE generic_table");
    }
}
```

## Cross-Database Compatibility

### Writing Database-Agnostic Migrations

```csharp
public class CrossDatabaseMigration : Migration
{
    public override void Up()
    {
        // Create table using FluentMigrator abstractions
        Create.Table("CrossDbTable")
            .WithColumn("Id").AsInt32().NotNullable().PrimaryKey().Identity()
            .WithColumn("Name").AsString(100).NotNullable()
            .WithColumn("Email").AsString(255).NotNullable()
            .WithColumn("Amount").AsDecimal(10, 2).NotNullable()
            .WithColumn("IsActive").AsBoolean().NotNullable().WithDefaultValue(true)
            .WithColumn("CreatedAt").AsDateTime().NotNullable().WithDefaultValue(SystemMethods.CurrentDateTime);

        // Create indexes using FluentMigrator
        Create.Index("IX_CrossDbTable_Email")
            .OnTable("CrossDbTable")
            .OnColumn("Email")
            .Unique();

        Create.Index("IX_CrossDbTable_Name")
            .OnTable("CrossDbTable")
            .OnColumn("Name");

        // Database-specific optimizations
        IfDatabase(ProcessorIdConstants.SqlServer).Execute.Sql("CREATE NONCLUSTERED INDEX IX_CrossDbTable_Amount_Filtered ON CrossDbTable (Amount) WHERE Amount > 0");
        IfDatabase(ProcessorIdConstants.Postgres).Execute.Sql("CREATE INDEX IX_CrossDbTable_Amount_Partial ON CrossDbTable (Amount) WHERE Amount > 0");
        IfDatabase(ProcessorIdConstants.MySql).Execute.Sql("ALTER TABLE CrossDbTable ENGINE=InnoDB");
        IfDatabase(ProcessorIdConstants.Oracle).Execute.Sql("CREATE INDEX IX_CrossDbTable_Amount_Filtered ON CrossDbTable (Amount) WHERE Amount > 0");
    }

    public override void Down()
    {
        Delete.Table("CrossDbTable");
    }
}
```

## Best Practices for Multiple Database Support

### Configuration Management

```csharp
public static class DatabaseConfiguration
{
    public static void ConfigureForEnvironment(IServiceCollection services, string environment)
    {
        var connectionString = GetConnectionString(environment);
        var databaseType = GetDatabaseType(environment);

        var builder = services.AddFluentMigratorCore()
            .ConfigureRunner(rb => {
                switch (databaseType.ToLower())
                {
                    case "sqlserver":
                        rb.AddSqlServer();
                        break;
                    case "postgresql":
                        rb.AddPostgres();
                        break;
                    case "mysql":
                        rb.AddMySql();
                        break;
                    case "sqlite":
                        rb.AddSQLite();
                        break;
                    case "oracle":
                        rb.AddOracle();
                        break;
                    case "firebird":
                        rb.AddFirebird();
                        break;
                    case "db2":
                        rb.AddDb2();
                        break;
                    default:
                        throw new NotSupportedException($"Database type {databaseType} is not supported");
                }

                rb.WithGlobalConnectionString(connectionString)
                  .ScanIn(Assembly.GetExecutingAssembly()).For.Migrations();
            })
            .AddLogging(lb => lb.AddFluentMigratorConsole());
    }

    private static string GetConnectionString(string environment)
    {
        // Implementation to get connection string based on environment
        return Environment.GetEnvironmentVariable($"DATABASE_CONNECTION_{environment.ToUpper()}")
               ?? throw new InvalidOperationException($"Connection string for {environment} not found");
    }

    private static string GetDatabaseType(string environment)
    {
        // Implementation to get database type based on environment
        return Environment.GetEnvironmentVariable($"DATABASE_TYPE_{environment.ToUpper()}")
               ?? "SqlServer";
    }
}
```
