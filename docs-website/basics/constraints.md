# Constraints

Database constraints are essential for maintaining data integrity and enforcing business rules at the database level.
FluentMigrator provides comprehensive support for creating and managing unique constraints across different database providers.

## Unique Constraints

Unique constraints ensure column values are unique across the table.

### Single Column Unique Constraints

```csharp
public class SingleColumnUnique : Migration
{
    public override void Up()
    {
        Create.Table("Users")
            .WithColumn("Id").AsInt32().NotNullable().PrimaryKey().Identity()
            .WithColumn("Username").AsString(50).NotNullable()
            .WithColumn("Email").AsString(255).NotNullable();

        // Username must be unique
        Create.UniqueConstraint("UQ_Users_Username")
            .OnTable("Users")
            .Column("Username");

        // Email must be unique
        Create.UniqueConstraint("UQ_Users_Email")
            .OnTable("Users")
            .Column("Email");
    }

    public override void Down()
    {
        Delete.UniqueConstraint("UQ_Users_Username").FromTable("Users");
        Delete.UniqueConstraint("UQ_Users_Email").FromTable("Users");
        Delete.Table("Users");
    }
}
```

### Composite Unique Constraints

```csharp
public class CompositeUnique : Migration
{
    public override void Up()
    {
        Create.Table("OrderItems")
            .WithColumn("OrderId").AsInt32().NotNullable()
            .WithColumn("ProductId").AsInt32().NotNullable()
            .WithColumn("Quantity").AsInt32().NotNullable()
            .WithColumn("UnitPrice").AsDecimal(10, 2).NotNullable();

        // Composite unique constraint - one product per order
        Create.UniqueConstraint("UQ_OrderItems_OrderId_ProductId")
            .OnTable("OrderItems")
            .Columns("OrderId", "ProductId");
    }

    public override void Down()
    {
        Delete.UniqueConstraint("UQ_OrderItems_OrderId_ProductId").FromTable("OrderItems");
        Delete.Table("OrderItems");
    }
}
```

## Default Constraints

Default constraints provide automatic values when no value is specified.

### Basic Default Values

```csharp
public class DefaultConstraints : Migration
{
    public override void Up()
    {
        Create.Table("Products")
            .WithColumn("Id").AsInt32().NotNullable().PrimaryKey().Identity()
            .WithColumn("Name").AsString(100).NotNullable()
            .WithColumn("IsActive").AsBoolean().NotNullable().WithDefaultValue(true)
            .WithColumn("CreatedAt").AsDateTime().NotNullable().WithDefaultValue(SystemMethods.CurrentDateTime)
            .WithColumn("Quantity").AsInt32().NotNullable().WithDefaultValue(0);
    }

    public override void Down()
    {
        Delete.Table("Products");
    }
}
```

## Managing Existing Constraints

### Adding Constraints to Existing Tables

```csharp
public class AddConstraintsToExistingTable : Migration
{
    public override void Up()
    {
        // Add unique constraint
        Create.UniqueConstraint("UQ_Users_Email")
            .OnTable("Users")
            .Column("Email");
    }

    public override void Down()
    {
        Delete.UniqueConstraint("UQ_Users_Email").FromTable("Users");
    }
}
```

### Removing Constraints

```csharp
public class RemoveConstraints : Migration
{
    public override void Up()
    {
        Delete.UniqueConstraint("UQ_Users_Email").FromTable("Users");
    }

    public override void Down()
    {
        Create.UniqueConstraint("UQ_Users_Email")
            .OnTable("Users")
            .Column("Email");
    }
}
```

## Database-Specific Constraint Features

Different database providers offer specialized constraint options and advanced features:

### SQL Server Specific
- **Clustered vs Non-Clustered**: Primary key clustering options - [SQL Server Provider](../providers/sql-server.md#sql-server-specific-features)
- **Filtered Unique Constraints**: Conditional unique constraints with WHERE clauses - [SQL Server Provider](../providers/sql-server.md)
- **Include Columns**: Covering constraint optimizations - [SQL Server Provider](../providers/sql-server.md)

## Troubleshooting

### Common Issues
1. **Constraint Violation Errors**: Review existing data before adding constraints
2. **Performance Impact**: Monitor constraint evaluation performance
3. **Cross-Database Syntax**: Use `IfDatabase()` for provider-specific syntax

## See Also
- [Foreign Keys](./foreign-keys.md) - Referential integrity constraints
- [Indexes](./indexes.md) - Index-backed constraint performance
- [Execute SQL](../operations/execute-sql.md) - Custom constraint implementation
