# Constraints

Database constraints are essential for maintaining data integrity and enforcing business rules at the database level.
FluentMigrator can create named unique and CHECK constraints on an existing table.

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

## CHECK constraints

Use a named CHECK constraint to enforce a SQL predicate on a table. Supply the predicate in the database's SQL dialect, including any identifier quoting it requires. The provider quotes identifiers and passes the predicate through as written. The SAP HANA provider uses the connection's current schema even when a migration specifies one.

```csharp
public override void Up()
{
    Create.Check("ck_order_items_quantity")
        .OnTable("order_items").InSchema("sales")
        .Condition("quantity > 0");
}

public override void Down()
{
    Delete.Check("ck_order_items_quantity")
        .FromTable("order_items").InSchema("sales");
}
```

To use the default naming convention, supply the columns in the order they should appear in the name:

```csharp
Create.Check().OnTable("bookings").Columns("starts_at", "ends_at")
    .Condition("ends_at > starts_at");
// CK_bookings_starts_at_ends_at

Delete.Check().FromTable("bookings").Columns("starts_at", "ends_at");
```

Use `.Column("quantity")` for a single column. The columns supply the name only. Give `Alter.Check()` and `Delete.Check()` the same columns in the same order as the create operation. Two checks on the same columns receive the same default name, so give at least one an explicit name. You can override the [naming convention](../intro/configuration.md).

To enforce the rule on new writes while you correct existing rows, add it with `.WithNoCheck()`. Once the existing rows satisfy the condition, validate them with `.Enabled()`.

```csharp
Create.Check("ck_order_items_quantity").OnTable("order_items")
    .Condition("quantity > 0").WithNoCheck();

// In a later migration, after correcting existing rows
Alter.Check("ck_order_items_quantity").OnTable("order_items").Enabled();
```

To suspend enforcement on a provider that supports it, use `Alter.Check(...).OnTable(...).Disabled()`. To replace the predicate, use `Alter.Check(...).OnTable(...).Condition("quantity >= 0")`. Replacing a condition drops the old constraint and creates a new one with the options you specify. For example, replacing a disabled SQL Server check with `.Condition(...)` alone enables it and validates existing rows. Provider-specific settings such as `NOT FOR REPLICATION` are lost unless reapplied.

Check the existing data before replacing a predicate. Firebird accepts the new constraint even when existing rows violate it, and those rows stay in the table. On Oracle and on Firebird with its default per-command commit, a failed replacement can leave the old constraint absent. On SAP HANA, the runner generates a single anonymous block, but recovery after a failed replacement has not been verified. Do not rely on the block being atomic. On SQL Server and Db2, keep both commands in the migration transaction. MySQL 8 uses one atomic `ALTER TABLE` statement.

| Provider | Create, replace and drop | Skip existing rows on create | Change enforcement or validate |
| --- | --- | --- | --- |
| SQL Server, Oracle | Yes | Yes | Disable, enable and validate, or enable without validation |
| PostgreSQL | Yes | Yes, from PostgreSQL 9.2 | Enable validates. Enable without validation leaves the existing state in place. Disabling is unsupported. |
| MySQL 8 | Yes, use 8.0.19+ for all three operations | Unsupported | Disable or enable with validation |
| MariaDB through the MySQL 8 generator | Yes on MariaDB 10.11 | Unsupported | MariaDB rejects MySQL's enforcement switches |
| Firebird | Yes | Always skips existing rows | Unsupported |
| Db2 LUW | Yes | Unsupported | Disable or enable with validation |
| Db2 for i, SAP HANA | Yes | Unsupported | Unsupported |
| MySQL 4/5, SQLite, Snowflake, Redshift | Unsupported | Unsupported | Unsupported |

A Db2 LUW table already in the set-integrity-pending state still requires its usual `SET INTEGRITY` operation. MySQL versions before 8.0.16 do not enforce CHECK constraints. On Oracle, enabling with `.WithNoCheck()` marks a previously validated constraint as unvalidated. On SQL Server, enabling without validation preserves an already enabled, trusted constraint's trust state.

`AutoReversingMigration` reverses a create by dropping the named constraint. Supply an explicit `Down()` when altering or deleting a check, since the previous predicate and enforcement state are needed to restore it. If a provider supports CHECK constraints but cannot honour a requested create option, creation fails in both compatibility modes. For example, MySQL 8 cannot create an enforced check while skipping validation of existing rows. Use `IfDatabase(...)` for provider-specific options. Other unsupported CHECK operations follow the compatibility mode: strict mode throws and loose mode skips the operation.

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

If you use a filtered unique constraint only to allow any number of rows with NULL (`WHERE col IS NOT NULL`), use a unique index with the provider-agnostic `NullsDistinct()` instead. For details, see [NULL handling in unique indexes](./indexes.md#null-handling-in-unique-indexes). For arbitrary filters, you still need `Filter()`.

## Troubleshooting

### Common Issues
1. **Constraint Violation Errors**: Review existing data before adding constraints
2. **Performance Impact**: Monitor constraint evaluation performance
3. **Cross-Database Syntax**: Use `IfDatabase()` for provider-specific syntax

## See Also
- [Foreign Keys](./foreign-keys.md) - Referential integrity constraints
- [Indexes](./indexes.md) - Index-backed constraint performance
- [Execute SQL](../operations/execute-sql.md) - Custom constraint implementation
