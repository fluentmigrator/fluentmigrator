# SQL Scripts

FluentMigrator provides the Execute.Sql family of methods for running custom SQL when the fluent API doesn't cover your specific needs. This guide covers all Execute.Sql methods and patterns for executing custom SQL scripts.

## Execute.Sql Methods

### Basic SQL Execution

::: info
Note that these examples are possible with FluentMigrator's fluent API as well (see [Operations > Data](./data.md))
:::

```csharp
// Execute a simple SQL statement
Execute.Sql("UPDATE Users SET IsActive = 1 WHERE CreatedAt < '2023-01-01'");

// Execute with parameter-like syntax (still basic string)
Execute.Sql("DELETE FROM TempData WHERE ProcessedAt < DATEADD(day, -7, GETDATE())");
```

### Multi-Statement SQL Blocks

```csharp
Execute.Sql(@"
    CREATE TABLE #TempResults (
        Id INT,
        TotalCount INT
    );

    INSERT INTO #TempResults
    SELECT UserId, COUNT(*)
    FROM Orders
    GROUP BY UserId;

    UPDATE Users
    SET OrderCount = t.TotalCount
    FROM Users u
    INNER JOIN #TempResults t ON u.Id = t.Id;

    DROP TABLE #TempResults;
");
```

## Using Parameters in SQL Scripts

FluentMigrator supports parameterized SQL scripts through the `parameters` argument available in `Execute.Sql`, `Execute.Script`, and `Execute.EmbeddedScript` methods. Parameters use token replacement with the `$(parameterName)` syntax.

### Basic Parameter Usage with Execute.Sql

```csharp
public override void Up()
{
    var parameters = new Dictionary<string, string>
    {
        ["TablePrefix"] = "App_",
        ["DefaultStatus"] = "Active",
        ["CurrentDate"] = "GETDATE()"
    };

    Execute.Sql(@"
        CREATE TABLE $(TablePrefix)Users (
            Id INT IDENTITY(1,1) PRIMARY KEY,
            Username NVARCHAR(50) NOT NULL,
            Status NVARCHAR(20) DEFAULT '$(DefaultStatus)',
            CreatedAt DATETIME DEFAULT $(CurrentDate)
        );

        INSERT INTO $(TablePrefix)Users (Username, Status, CreatedAt)
        VALUES ('admin', '$(DefaultStatus)', $(CurrentDate));
    ", parameters);
}
```

### Parameter Usage with Execute.Script

```csharp
public override void Up()
{
    var parameters = new Dictionary<string, string>
    {
        ["Environment"] = "Production",
        ["DatabaseName"] = "MyAppDB",
        ["BackupPath"] = "C:\\Backups\\MyAppDB.bak"
    };

    // Assuming you have a SQL script file: Scripts/CreateDatabase.sql
    Execute.Script("Scripts/CreateDatabase.sql", parameters);
}
```

**Example Scripts/CreateDatabase.sql:**
```sql
-- Create database for $(Environment) environment
CREATE DATABASE $(DatabaseName)_$(Environment);

-- Set backup path
BACKUP DATABASE $(DatabaseName)_$(Environment) 
TO DISK = '$(BackupPath)';
```

### Parameter Usage with Execute.EmbeddedScript

```csharp
public override void Up()
{
    var parameters = new Dictionary<string, string>
    {
        ["SchemaName"] = "Reporting",
        ["TableSpace"] = "USERS",
        ["IndexSpace"] = "INDX"
    };

    // Assuming you have an embedded resource: MyAssembly.Scripts.CreateReportingSchema.sql
    Execute.EmbeddedScript("CreateReportingSchema.sql", parameters);
}
```

### Parameter Overloads with Descriptions

```csharp
public override void Up()
{
    var parameters = new Dictionary<string, string>
    {
        ["BatchSize"] = "1000",
        ["TableName"] = "LargeDataTable"
    };

    Execute.Sql(@"
        DECLARE @BatchSize INT = $(BatchSize);
        
        WHILE EXISTS(SELECT 1 FROM $(TableName) WHERE ProcessedAt IS NULL)
        BEGIN
            UPDATE TOP (@BatchSize) $(TableName)
            SET ProcessedAt = GETDATE()
            WHERE ProcessedAt IS NULL;
        END
    ", "Batch process large data table with parameterized batch size", parameters);
}
```

### Database-Specific Parameters

```csharp
public override void Up()
{
    // SQL Server parameters
    var sqlServerParams = new Dictionary<string, string>
    {
        ["CurrentDateTime"] = "GETDATE()",
        ["StringConcat"] = "+",
        ["TopClause"] = "TOP (100)"
    };

    IfDatabase(ProcessorIdConstants.SqlServer).Execute.Sql(@"
        SELECT $(TopClause) * 
        FROM Users 
        WHERE CreatedAt > DATEADD(day, -30, $(CurrentDateTime))
        ORDER BY Username $(StringConcat) ' (' $(StringConcat) Email $(StringConcat) ')'
    ", sqlServerParams);

    // PostgreSQL parameters  
    var postgresParams = new Dictionary<string, string>
    {
        ["CurrentDateTime"] = "NOW()",
        ["StringConcat"] = "||",
        ["LimitClause"] = "LIMIT 100"
    };

    IfDatabase(ProcessorIdConstants.Postgres).Execute.Sql(@"
        SELECT * 
        FROM Users 
        WHERE CreatedAt > $(CurrentDateTime) - INTERVAL '30 days'
        ORDER BY Username $(StringConcat) ' (' $(StringConcat) Email $(StringConcat) ')'
        $(LimitClause)
    ", postgresParams);
}
```

### Parameter Escaping

When you need to include the literal text `$(parameterName)` in your SQL (not as a parameter), use double dollar signs and double parentheses:

```csharp
public override void Up()
{
    var parameters = new Dictionary<string, string>
    {
        ["ActualTableName"] = "Users",
        ["ActualColumnName"] = "Username"
    };

    Execute.Sql(@"
        -- This will be replaced: $(ActualTableName) becomes 'Users'
        SELECT * FROM $(ActualTableName);

        -- This will NOT be replaced: $$((ParameterName)) becomes '$(ParameterName)' literally
        INSERT INTO DocumentationTable (Description) 
        VALUES ('Use $$((ParameterName)) syntax to include literal parameter syntax');

        -- Mixed example:
        COMMENT ON COLUMN $(ActualTableName).$(ActualColumnName) 
        IS 'This column was created using parameter $$((ActualTableName))';
    ", parameters);
}
```

### Environment-Based Parameters

```csharp
public override void Up()
{
    // Determine environment-specific values
    var environment = Environment.GetEnvironmentVariable("ENVIRONMENT") ?? "Development";
    
    var parameters = new Dictionary<string, string>
    {
        ["Environment"] = environment,
        ["ConnectionTimeout"] = environment == "Production" ? "30" : "300",
        ["LogLevel"] = environment == "Production" ? "ERROR" : "DEBUG",
        ["MaxConnections"] = environment == "Production" ? "100" : "10"
    };

    Execute.Sql(@"
        -- Environment-specific configuration
        EXEC sp_configure 'remote query timeout', $(ConnectionTimeout);
        
        INSERT INTO AppSettings (SettingKey, SettingValue, Environment)
        VALUES 
            ('LogLevel', '$(LogLevel)', '$(Environment)'),
            ('MaxConnections', '$(MaxConnections)', '$(Environment)');
    ", $"Configure settings for {environment} environment", parameters);
}
```

### Parameters with File Scripts - Advanced Example

```csharp
[Migration(1)]
public class CreateMultiTenantSchema : Migration
{
    public override void Up()
    {
        var tenants = new[] { "TenantA", "TenantB", "TenantC" };

        foreach (var tenant in tenants)
        {
            var parameters = new Dictionary<string, string>
            {
                ["TenantName"] = tenant,
                ["SchemaName"] = $"Tenant_{tenant}",
                ["TablePrefix"] = $"{tenant}_",
                ["CreatedBy"] = "Migration System",
                ["CreatedDate"] = "GETDATE()"
            };

            Execute.Script("Scripts/CreateTenantSchema.sql", parameters);
        }
    }

    public override void Down()
    {
        var tenants = new[] { "TenantA", "TenantB", "TenantC" };
        
        foreach (var tenant in tenants)
        {
            var parameters = new Dictionary<string, string>
            {
                ["SchemaName"] = $"Tenant_{tenant}"
            };

            Execute.Script("Scripts/DropTenantSchema.sql", parameters);
        }
    }
}
```

### Best Practices for Parameters

**✅ Good practices:**
- Use descriptive parameter names that clearly indicate their purpose
- Validate parameter values before passing them to avoid SQL injection
- Use parameters for table names, column names, and configuration values that change between environments
- Keep parameter dictionaries organized and well-documented
- Use consistent naming conventions across your migrations

**❌ Avoid:**
- Using parameters for complex logic that should be in C# code instead
- Passing user input directly as parameters without validation
- Using parameters for values that never change (use constants in SQL instead)
- Creating overly complex parameter hierarchies that are hard to maintain

```csharp
public override void Up()
{
    // ✅ Good: Validated parameters with clear names
    var environment = ValidateEnvironment(GetCurrentEnvironment());
    var parameters = new Dictionary<string, string>
    {
        ["EnvironmentName"] = environment,
        ["DatabasePrefix"] = GetDatabasePrefix(environment),
        ["RetentionDays"] = GetRetentionDays(environment).ToString(),
        ["BackupEnabled"] = environment == "Production" ? "1" : "0"
    };

    Execute.Sql(@"
        CREATE TABLE $(DatabasePrefix)AuditLog (
            Id BIGINT IDENTITY(1,1) PRIMARY KEY,
            Action NVARCHAR(100) NOT NULL,
            CreatedAt DATETIME DEFAULT GETDATE(),
            RetentionDate DATETIME DEFAULT DATEADD(day, $(RetentionDays), GETDATE()),
            Environment NVARCHAR(50) DEFAULT '$(EnvironmentName)'
        );

        IF $(BackupEnabled) = 1
        BEGIN
            -- Setup backup job for production
            EXEC CreateBackupJob '$(DatabasePrefix)AuditLog';
        END
    ", "Create audit log table with environment-specific configuration", parameters);
}

private string ValidateEnvironment(string env)
{
    var validEnvironments = new[] { "Development", "Staging", "Production" };
    if (!validEnvironments.Contains(env))
        throw new ArgumentException($"Invalid environment: {env}");
    return env;
}
```

### Conditional SQL Execution

```csharp
public override void Up()
{
    IfDatabase(ProcessorIdConstants.SqlServer).Execute.Sql(@"
        UPDATE Users
        SET LastLogin = GETUTCDATE()
        WHERE Email LIKE '%@company.com'");

    IfDatabase(ProcessorIdConstants.Postgres).Execute.Sql(@"
        UPDATE Users
        SET LastLogin = NOW()
        WHERE Email LIKE '%@company.com'");

    IfDatabase(ProcessorIdConstants.MySql).Execute.Sql(@"
        UPDATE Users
        SET LastLogin = UTC_TIMESTAMP()
        WHERE Email LIKE '%@company.com'");
}
```

## Data Querying with Returns

While FluentMigrator is primarily for schema changes, sometimes you need to query data during migrations:

```csharp
[Migration(1)]
public class MigrateUserData : Migration
{
    public override void Up()
    {
        // Check if migration is needed
        Execute.Sql(@"
            IF EXISTS (SELECT 1 FROM Users WHERE Status IS NULL)
            BEGIN
                UPDATE Users
                SET Status = 'Active'
                WHERE Status IS NULL AND IsActive = 1

                UPDATE Users
                SET Status = 'Inactive'
                WHERE Status IS NULL AND IsActive = 0
            END
        ");
    }

    public override void Down()
    {
        Execute.Sql("UPDATE Users SET Status = NULL WHERE Status IN ('Active', 'Inactive')");
    }
}
```

### Retrieving Data for Migration Decisions

```csharp
[Migration(2)]
public class ConditionalDataMigration : Migration
{
    public override void Up()
    {
        // Use conditional logic based on existing data
        Execute.Sql(@"
            DECLARE @UserCount INT
            SELECT @UserCount = COUNT(*) FROM Users

            IF @UserCount > 10000
            BEGIN
                -- Large dataset: use batch processing
                WHILE @@ROWCOUNT > 0
                BEGIN
                    UPDATE TOP (1000) Users
                    SET UpdatedAt = GETDATE()
                    WHERE UpdatedAt IS NULL
                END
            END
            ELSE
            BEGIN
                -- Small dataset: simple update
                UPDATE Users SET UpdatedAt = GETDATE() WHERE UpdatedAt IS NULL
            END
        ");
    }

    public override void Down()
    {
        Execute.Sql("UPDATE Users SET UpdatedAt = NULL");
    }
}
```

## Advanced Execute.Sql Patterns

### Complex Migration Logic

```csharp
[Migration(3)]
public class ComplexDataMigration : Migration
{
    public override void Up()
    {
        // Use batch processing for large datasets
        IfDatabase(ProcessorIdConstants.SqlServer).Delegate(() =>
        {
            Execute.Sql(@"
                DECLARE @BatchSize INT = 5000;
                WHILE EXISTS (SELECT 1 FROM Users WHERE UpdatedAt IS NULL)
                BEGIN
                    UPDATE TOP (@BatchSize) Users
                    SET UpdatedAt = GETDATE()
                    WHERE UpdatedAt IS NULL;

                    WAITFOR DELAY '00:00:01'; -- Small delay to reduce lock pressure
                END
            ");
        });

        // Since IfDatabase doesn't have a "not" operator, we'll handle common alternatives explicitly
        IfDatabase(ProcessorIdConstants.Postgres, ProcessorIdConstants.MySql, ProcessorIdConstants.SQLite).Execute.Sql(@"
            UPDATE Users
            SET UpdatedAt = CURRENT_TIMESTAMP
            WHERE UpdatedAt IS NULL
        ");
    }

    public override void Down()
    {
        Execute.Sql("UPDATE Users SET UpdatedAt = NULL");
    }
}
```

### Error Handling in SQL

```csharp
[Migration(4)]
public class ErrorHandlingMigration : Migration
{
    public override void Up()
    {
        IfDatabase(ProcessorIdConstants.SqlServer).Execute.Sql(@"
            BEGIN TRY
                -- Risky operation
                ALTER TABLE Users ADD CONSTRAINT CK_Users_Email CHECK (Email LIKE '%@%')
            END TRY
            BEGIN CATCH
                -- Log error or handle gracefully
                PRINT 'Failed to add email constraint: ' + ERROR_MESSAGE()
            END CATCH
        ");
    }

    public override void Down()
    {
        Execute.Sql("ALTER TABLE Users DROP CONSTRAINT IF EXISTS CK_Users_Email");
    }
}
```

### Dynamic SQL Generation

```csharp
[Migration(5)]
public class DynamicSqlMigration : Migration
{
    public override void Up()
    {
        // Generate SQL based on schema inspection
        Execute.Sql(@"
            DECLARE @sql NVARCHAR(MAX) = ''

            SELECT @sql = @sql + 'ALTER TABLE [' + TABLE_SCHEMA + '].[' + TABLE_NAME + ']
                                 ADD CreatedBy NVARCHAR(100) NULL;' + CHAR(13)
            FROM INFORMATION_SCHEMA.TABLES t
            WHERE TABLE_TYPE = 'BASE TABLE'
              AND NOT EXISTS (
                  SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS c
                  WHERE c.TABLE_SCHEMA = t.TABLE_SCHEMA
                    AND c.TABLE_NAME = t.TABLE_NAME
                    AND c.COLUMN_NAME = 'CreatedBy'
              )

            EXEC sp_executesql @sql
        ");
    }

    public override void Down()
    {
        Execute.Sql(@"
            DECLARE @sql NVARCHAR(MAX) = ''

            SELECT @sql = @sql + 'ALTER TABLE [' + TABLE_SCHEMA + '].[' + TABLE_NAME + ']
                                 DROP COLUMN CreatedBy;' + CHAR(13)
            FROM INFORMATION_SCHEMA.COLUMNS
            WHERE COLUMN_NAME = 'CreatedBy'

            EXEC sp_executesql @sql
        ");
    }
}
```

## Database-Specific Operations

### SQL Server Specific Features

```csharp
public override void Up()
{
    IfDatabase(ProcessorIdConstants.SqlServer).Delegate(() =>
    {
        // Enable features
        Execute.Sql("ALTER DATABASE CURRENT SET ALLOW_SNAPSHOT_ISOLATION ON");
        Execute.Sql("ALTER DATABASE CURRENT SET READ_COMMITTED_SNAPSHOT ON");

        // Create full-text catalog
        Execute.Sql("CREATE FULLTEXT CATALOG DocumentCatalog AS DEFAULT");
    });
}
```

### PostgreSQL Specific Features

```csharp
public override void Up()
{
    IfDatabase(ProcessorIdConstants.Postgres).Delegate(() =>
    {
        // Create extension
        Execute.Sql("CREATE EXTENSION IF NOT EXISTS \"uuid-ossp\"");

        // Create custom type
        Execute.Sql("CREATE TYPE user_status AS ENUM ('active', 'inactive', 'suspended')");
    });
}
```

## Best Practices and Limitations

### What Execute.Sql Should Be Used For

**✅ Good uses:**
- Database-specific operations not supported by the fluent API
- Complex data transformations during migration
- Bulk operations on existing data
- Creating database-specific objects (stored procedures, functions, views)
- Performance-critical operations requiring custom SQL

**❌ Avoid for:**
- Simple schema changes (use fluent API instead)
- Cross-database compatible operations
- Operations that might be rolled back frequently

### Performance Considerations

```csharp
public override void Up()
{
    // ❌ Bad: Can cause locks and timeouts
    Execute.Sql("UPDATE Users SET IsActive = 1"); // Updates entire table at once

    // ✅ Good: Batch processing
    Execute.Sql(@"
        WHILE @@ROWCOUNT > 0
        BEGIN
            UPDATE TOP (1000) Users
            SET IsActive = 1
            WHERE IsActive <> 1 OR IsActive IS NULL

            WAITFOR DELAY '00:00:01'  -- Give other operations a chance
        END
    ");
}
```

### SQL Injection Prevention

```csharp
public override void Up()
{
    // ❌ Bad: Potential SQL injection if value comes from external source
    var tableName = GetTableNameFromSomewhere();
    Execute.Sql($"DROP TABLE {tableName}");

    // ✅ Good: Validate input or use safer approaches
    var allowedTables = new[] { "TempTable1", "TempTable2" };
    if (allowedTables.Contains(tableName))
    {
        Execute.Sql($"DROP TABLE {tableName}");
    }
}
```

## Integration with Migration Features

### Working with Tags

```csharp
[Migration(1)]
[Tags("DataMigration", "Production")]
public class TaggedSqlMigration : Migration
{
    public override void Up()
    {
        // This SQL will only run when appropriate tags are specified
        Execute.Sql(@"
            UPDATE UserSettings
            SET Theme = 'Dark'
            WHERE Theme IS NULL
        ");
    }

    public override void Down()
    {
        Execute.Sql("UPDATE UserSettings SET Theme = NULL WHERE Theme = 'Dark'");
    }
}
```

### Working with Profiles

```csharp
[Migration(1)]
[Profile("DataSeed")]
public class DataSeedingSql : Migration
{
    public override void Up()
    {
        Execute.Sql(@"
            INSERT INTO Users (Username, Email, IsActive, CreatedAt)
            VALUES
                ('admin', 'admin@company.com', 1, GETDATE()),
                ('testuser', 'test@company.com', 1, GETDATE())
        ");
    }

    public override void Down()
    {
        Execute.Sql("DELETE FROM Users WHERE Username IN ('admin', 'testuser')");
    }
}
```

### Maintenance Migrations with SQL

```csharp
[Migration(1)]
[MaintenanceMigration(TransactionBehavior.None)] // Some operations can't be in transactions
public class MaintenanceSql : Migration
{
    public override void Up()
    {
        IfDatabase(ProcessorIdConstants.SqlServer).Execute.Sql(@"
            -- Rebuild indexes
            DECLARE @sql NVARCHAR(MAX) = ''
            SELECT @sql = @sql + 'ALTER INDEX ALL ON [' + s.name + '].[' + t.name + '] REBUILD;' + CHAR(13)
            FROM sys.tables t
            INNER JOIN sys.schemas s ON t.schema_id = s.schema_id

            EXEC sp_executesql @sql
        ");
    }

    public override void Down()
    {
        // Maintenance operations typically don't have meaningful rollbacks
        Execute.Sql("-- Maintenance migration - no rollback needed");
    }
}
```

See also [Execute code on connection](../operations/with-connection.md) for more complex scenarios involving direct SQL execution.
