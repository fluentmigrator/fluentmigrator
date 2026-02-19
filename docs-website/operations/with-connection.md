# Advanced Logic on Connection

The `Execute.WithConnection` method provides direct access to the database connection and transaction when FluentMigrator's standard fluent API isn't sufficient for complex operations.

## Overview

`Execute.WithConnection` allows you to execute custom logic that requires direct access to the `IDbConnection` and `IDbTransaction` objects. This is useful for advanced scenarios that can't be easily expressed through FluentMigrator's fluent interface.

## Method Signature

```csharp
public void WithConnection(Action<IDbConnection, IDbTransaction> operation)
```

## When to Use

Use `Execute.WithConnection` when you need to:

- Execute complex stored procedures with multiple result sets
- Perform bulk operations requiring fine-grained control
- Execute multiple related commands in a specific sequence
- Access database-specific features not covered by the fluent API
- Perform complex data transformations during migration
- Execute operations that require custom error handling

## Basic Example

```csharp
[Migration(20240101001)]
public class ExampleMigration : Migration
{
    public override void Up()
    {
        Execute.WithConnection((connection, transaction) =>
        {
            var command = connection.CreateCommand();
            command.Transaction = transaction;
            command.CommandText = "SELECT COUNT(*) FROM Users WHERE Active = 1";
            
            var activeUserCount = (int)command.ExecuteScalar();
            
            // Use the result for conditional logic
            if (activeUserCount > 1000)
            {
                var updateCommand = connection.CreateCommand();
                updateCommand.Transaction = transaction;
                updateCommand.CommandText = "UPDATE Settings SET MaxUsers = @count";
                
                var parameter = updateCommand.CreateParameter();
                parameter.ParameterName = "@count";
                parameter.Value = activeUserCount * 2;
                updateCommand.Parameters.Add(parameter);
                
                updateCommand.ExecuteNonQuery();
            }
        });
    }

    public override void Down()
    {
        // Reverse the operation if needed
        Execute.WithConnection((connection, transaction) =>
        {
            var command = connection.CreateCommand();
            command.Transaction = transaction;
            command.CommandText = "UPDATE Settings SET MaxUsers = 1000";
            command.ExecuteNonQuery();
        });
    }
}
```

## Advanced Examples

### Bulk Data Operations

```csharp
[Migration(20240101002)]
public class BulkDataMigration : Migration
{
    public override void Up()
    {
        Execute.WithConnection((connection, transaction) =>
        {
            // Read data from source table
            var selectCommand = connection.CreateCommand();
            selectCommand.Transaction = transaction;
            selectCommand.CommandText = "SELECT Id, Name, Email FROM OldUsers";
            
            var insertCommand = connection.CreateCommand();
            insertCommand.Transaction = transaction;
            insertCommand.CommandText = @"
                INSERT INTO NewUsers (LegacyId, FullName, EmailAddress, CreatedDate) 
                VALUES (@id, @name, @email, @created)";
            
            var idParam = insertCommand.CreateParameter();
            idParam.ParameterName = "@id";
            insertCommand.Parameters.Add(idParam);
            
            var nameParam = insertCommand.CreateParameter();
            nameParam.ParameterName = "@name";
            insertCommand.Parameters.Add(nameParam);
            
            var emailParam = insertCommand.CreateParameter();
            emailParam.ParameterName = "@email";
            insertCommand.Parameters.Add(emailParam);
            
            var createdParam = insertCommand.CreateParameter();
            createdParam.ParameterName = "@created";
            insertCommand.Parameters.Add(createdParam);
            
            using (var reader = selectCommand.ExecuteReader())
            {
                while (reader.Read())
                {
                    idParam.Value = reader["Id"];
                    nameParam.Value = reader["Name"];
                    emailParam.Value = reader["Email"];
                    createdParam.Value = DateTime.UtcNow;
                    
                    insertCommand.ExecuteNonQuery();
                }
            }
        });
    }

    public override void Down()
    {
        Delete.Table("NewUsers");
    }
}
```

### Complex Stored Procedure Execution

```csharp
[Migration(20240101003)]
public class StoredProcedureMigration : Migration
{
    public override void Up()
    {
        // Create the stored procedure first
        Execute.Sql(@"
            CREATE PROCEDURE ComplexDataMigration
                @BatchSize INT = 1000,
                @ProcessedCount INT OUTPUT
            AS
            BEGIN
                -- Complex stored procedure logic here
                SET @ProcessedCount = @@ROWCOUNT;
            END
        ");

        // Execute the stored procedure with output parameters
        Execute.WithConnection((connection, transaction) =>
        {
            var command = connection.CreateCommand();
            command.Transaction = transaction;
            command.CommandText = "ComplexDataMigration";
            command.CommandType = System.Data.CommandType.StoredProcedure;
            
            var batchSizeParam = command.CreateParameter();
            batchSizeParam.ParameterName = "@BatchSize";
            batchSizeParam.Value = 500;
            command.Parameters.Add(batchSizeParam);
            
            var processedCountParam = command.CreateParameter();
            processedCountParam.ParameterName = "@ProcessedCount";
            processedCountParam.Direction = System.Data.ParameterDirection.Output;
            processedCountParam.DbType = System.Data.DbType.Int32;
            command.Parameters.Add(processedCountParam);
            
            command.ExecuteNonQuery();
            
            var processedCount = (int)processedCountParam.Value;
            Console.WriteLine($"Processed {processedCount} records");
        });
    }

    public override void Down()
    {
        Execute.Sql("DROP PROCEDURE ComplexDataMigration");
    }
}
```

### Database-Specific Operations

```csharp
[Migration(20240101004)]
public class DatabaseSpecificMigration : Migration
{
    public override void Up()
    {
        IfDatabase(ProcessorIdConstants.SqlServer).Execute.WithConnection((connection, transaction) =>
        {
            // SQL Server specific operations
            var command = connection.CreateCommand();
            command.Transaction = transaction;
            command.CommandText = @"
                ALTER DATABASE CURRENT 
                SET ENABLE_BROKER WITH ROLLBACK IMMEDIATE";
            command.ExecuteNonQuery();
        });

        IfDatabase(ProcessorIdConstants.PostgreSql).Execute.WithConnection((connection, transaction) =>
        {
            // PostgreSQL specific operations
            var command = connection.CreateCommand();
            command.Transaction = transaction;
            command.CommandText = "CREATE EXTENSION IF NOT EXISTS \"uuid-ossp\"";
            command.ExecuteNonQuery();
        });
    }

    public override void Down()
    {
        // Reverse operations if necessary
    }
}
```

### Error Handling and Validation

```csharp
[Migration(20240101005)]
public class ValidatedMigration : Migration
{
    public override void Up()
    {
        Execute.WithConnection((connection, transaction) =>
        {
            try
            {
                // Check if data meets requirements
                var validationCommand = connection.CreateCommand();
                validationCommand.Transaction = transaction;
                validationCommand.CommandText = @"
                    SELECT COUNT(*) 
                    FROM Users 
                    WHERE Email IS NULL OR Email = ''";
                
                var invalidCount = (int)validationCommand.ExecuteScalar();
                
                if (invalidCount > 0)
                {
                    throw new InvalidOperationException(
                        $"Cannot proceed with migration: {invalidCount} users have invalid emails");
                }
                
                // Proceed with the actual migration
                var migrationCommand = connection.CreateCommand();
                migrationCommand.Transaction = transaction;
                migrationCommand.CommandText = @"
                    UPDATE Users 
                    SET EmailVerified = 0 
                    WHERE EmailVerified IS NULL";
                
                migrationCommand.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                // Log the error or perform cleanup
                Console.WriteLine($"Migration failed: {ex.Message}");
                throw; // Re-throw to rollback the transaction
            }
        });
    }

    public override void Down()
    {
        Execute.WithConnection((connection, transaction) =>
        {
            var command = connection.CreateCommand();
            command.Transaction = transaction;
            command.CommandText = "UPDATE Users SET EmailVerified = NULL";
            command.ExecuteNonQuery();
        });
    }
}
```

## Important Considerations

### Transaction Management

- The connection is already open and a transaction is active
- **Always** assign the transaction to your commands: `command.Transaction = transaction`
- Do not create or commit transactions manually
- Exceptions will cause the entire migration to be rolled back

### Connection State

- The connection is already open and ready for use
- Do not close the connection
- Do not dispose of the connection or transaction
- The connection lifetime is managed by FluentMigrator

### Performance Considerations

```csharp
// Good: Reuse command objects when possible
Execute.WithConnection((connection, transaction) =>
{
    var command = connection.CreateCommand();
    command.Transaction = transaction;
    command.CommandText = "INSERT INTO Table (Column) VALUES (@value)";
    
    var parameter = command.CreateParameter();
    parameter.ParameterName = "@value";
    command.Parameters.Add(parameter);
    
    // Prepare once, execute many times
    command.Prepare();
    
    foreach (var value in values)
    {
        parameter.Value = value;
        command.ExecuteNonQuery();
    }
});
```

### Database Portability

When using `Execute.WithConnection`, consider database portability:

```csharp
Execute.WithConnection((connection, transaction) =>
{
    var sql = "SELECT 1"; // Standard SQL
    
    // Avoid database-specific syntax unless necessary
    if (connection.GetType().Name.Contains("SqlConnection"))
    {
        sql = "SELECT @@VERSION"; // SQL Server specific
    }
    else if (connection.GetType().Name.Contains("NpgsqlConnection"))
    {
        sql = "SELECT version()"; // PostgreSQL specific
    }
    
    var command = connection.CreateCommand();
    command.Transaction = transaction;
    command.CommandText = sql;
    
    var result = command.ExecuteScalar();
});
```

## Best Practices

1. **Use Sparingly**: Only use `Execute.WithConnection` when the fluent API is insufficient
2. **Assign Transactions**: Always set `command.Transaction = transaction`
3. **Handle Exceptions**: Implement proper error handling and validation
4. **Consider Portability**: Be mindful of database-specific code
5. **Document Complex Logic**: Add comments explaining complex operations
6. **Test Thoroughly**: Test rollback scenarios and error conditions
7. **Use Parameters**: Always use parameterized queries to prevent SQL injection

## Alternative Approaches

Before using `Execute.WithConnection`, consider if these alternatives might work:

- **Execute.Sql**: For simple SQL statements
- **Data Operations**: For standard CRUD operations
- **Custom Extensions**: For reusable database operations
- **Multiple Migrations**: Break complex operations into smaller migrations

## Debugging Tips

- Enable SQL logging to see generated SQL statements
- Use try-catch blocks to capture and log detailed error information
- Test with small data sets first
- Consider adding progress logging for long-running operations

```csharp
Execute.WithConnection((connection, transaction) =>
{
    Console.WriteLine("Starting complex migration...");
    
    try
    {
        // Your complex logic here
        Console.WriteLine("Migration completed successfully");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Migration failed: {ex.Message}");
        Console.WriteLine($"Stack trace: {ex.StackTrace}");
        throw;
    }
});
```