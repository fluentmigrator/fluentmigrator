# Custom Extensions

You can create custom extension methods to encapsulate common patterns and reduce code duplication in your migrations. This approach helps maintain consistency across your migration scripts and makes complex operations reusable.

## Extension Methods

```csharp
public static class MigrationExtensions
{
    public static ICreateTableColumnOptionOrWithColumnSyntax WithIdColumn(
        this ICreateTableWithColumnSyntax tableWithColumnSyntax)
    {
        return tableWithColumnSyntax
            .WithColumn("Id")
            .AsInt32()
            .NotNullable()
            .PrimaryKey()
            .Identity();
    }

    public static ICreateTableColumnOptionOrWithColumnSyntax WithTimeStamps(
        this ICreateTableColumnOptionOrWithColumnSyntax tableWithColumnSyntax)
    {
        return tableWithColumnSyntax
            .WithColumn("CreatedAt")
                .AsDateTime()
                .NotNullable()
                .WithDefaultValue(SystemMethods.CurrentDateTime)
            .WithColumn("UpdatedAt")
                .AsDateTime()
                .Nullable();
    }

    public static ICreateTableColumnOptionOrWithColumnSyntax WithAuditFields(
        this ICreateTableColumnOptionOrWithColumnSyntax tableWithColumnSyntax)
    {
        return tableWithColumnSyntax
            .WithTimeStamps()
            .WithColumn("CreatedBy")
                .AsString(100)
                .Nullable()
            .WithColumn("UpdatedBy")
                .AsString(100)
                .Nullable();
    }
}
```

## Usage Examples

**Basic Usage:**
```csharp
Create.Table("Products")
    .WithIdColumn()           // Extension method
    .WithColumn("Name").AsString(200).NotNullable()
    .WithColumn("Price").AsDecimal(10, 2).NotNullable()
    .WithAuditFields();       // Extension method
```

**Advanced Pattern - User Management Table:**
```csharp
Create.Table("Users")
    .WithIdColumn()
    .WithColumn("Username").AsString(50).NotNullable().Unique()
    .WithColumn("Email").AsString(255).NotNullable().Unique()
    .WithColumn("PasswordHash").AsString(255).NotNullable()
    .WithAuditFields();
```

## Advanced Extension Patterns

### Database-Specific Extensions
```csharp
public static class SqlServerExtensions
{
    public static ICreateTableColumnOptionOrWithColumnSyntax WithRowVersion(
        this ICreateTableColumnOptionOrWithColumnSyntax tableWithColumnSyntax)
    {
        return tableWithColumnSyntax
            .WithColumn("RowVersion")
            .AsCustom("ROWVERSION")
            .NotNullable();
    }

    public static ICreateTableColumnOptionOrWithColumnSyntax WithJsonColumn(
        this ICreateTableColumnOptionOrWithColumnSyntax tableWithColumnSyntax,
        string columnName)
    {
        return tableWithColumnSyntax
            .WithColumn(columnName)
            .AsCustom("NVARCHAR(MAX)")
            .Nullable();
    }
}
```

### Multi-Provider Extensions
```csharp
public static class CrossProviderExtensions
{
    public static ICreateTableColumnOptionOrWithColumnSyntax WithJsonColumn(
        this ICreateTableColumnOptionOrWithColumnSyntax tableWithColumnSyntax,
        string columnName,
        IMigrationContext context)
    {
        if (context.QuerySchema.DatabaseType == "SqlServer2016" || 
            context.QuerySchema.DatabaseType == "SqlServer")
        {
            return tableWithColumnSyntax
                .WithColumn(columnName)
                .AsCustom("NVARCHAR(MAX)")
                .Nullable();
        }
        else if (context.QuerySchema.DatabaseType == "Postgres")
        {
            return tableWithColumnSyntax
                .WithColumn(columnName)
                .AsCustom("JSONB")
                .Nullable();
        }
        else if (context.QuerySchema.DatabaseType == "MySql")
        {
            return tableWithColumnSyntax
                .WithColumn(columnName)
                .AsCustom("JSON")
                .Nullable();
        }
        else
        {
            // Fallback to TEXT for other providers
            return tableWithColumnSyntax
                .WithColumn(columnName)
                .AsString()
                .Nullable();
        }
    }
}
```

### Domain-Specific Extensions
```csharp
public static class ECommerceExtensions
{
    public static ICreateTableColumnOptionOrWithColumnSyntax WithProductColumns(
        this ICreateTableColumnOptionOrWithColumnSyntax tableWithColumnSyntax)
    {
        return tableWithColumnSyntax
            .WithColumn("SKU").AsString(50).NotNullable().Unique()
            .WithColumn("Name").AsString(200).NotNullable()
            .WithColumn("Description").AsString().Nullable()
            .WithColumn("Price").AsDecimal(10, 2).NotNullable()
            .WithColumn("IsActive").AsBoolean().NotNullable().WithDefaultValue(true)
            .WithAuditFields();
    }

    public static ICreateTableColumnOptionOrWithColumnSyntax WithOrderColumns(
        this ICreateTableColumnOptionOrWithColumnSyntax tableWithColumnSyntax)
    {
        return tableWithColumnSyntax
            .WithColumn("OrderNumber").AsString(20).NotNullable().Unique()
            .WithColumn("Status").AsString(20).NotNullable().WithDefaultValue("Pending")
            .WithColumn("TotalAmount").AsDecimal(10, 2).NotNullable()
            .WithColumn("OrderDate").AsDateTime().NotNullable()
                .WithDefaultValue(SystemMethods.CurrentDateTime)
            .WithAuditFields();
    }
}
```

## Best Practices

### Extension Method Design
- **Keep extensions focused**: Each extension should have a single, clear purpose
- **Use descriptive names**: `WithIdColumn()` is clearer than `WithId()`
- **Return appropriate interfaces**: Maintain fluent API chain compatibility
- **Handle nullability carefully**: Provide sensible defaults for common scenarios

### Organization
- **Group by domain**: Create separate extension classes for different business domains
- **Database-specific extensions**: Use separate classes for provider-specific features
- **Common patterns first**: Start with most frequently used patterns
- **Document thoroughly**: Include XML documentation for all extension methods

### Compatibility
- **Test across providers**: Ensure extensions work with all target databases
- **Use IfDatabase() when needed**: Handle provider-specific requirements
- **Graceful fallbacks**: Provide reasonable defaults for unsupported features
- **Version compatibility**: Consider FluentMigrator version requirements

### Usage Guidelines
- **Consistent application**: Use extensions consistently across all migrations
- **Team standards**: Establish team conventions for extension usage
- **Migration testing**: Test migrations with extensions thoroughly
- **Documentation**: Document custom extensions for team members

## Example: Complete Extension Library

```csharp
namespace YourProject.Migrations.Extensions
{
    public static class StandardExtensions
    {
        // Standard ID column
        public static ICreateTableColumnOptionOrWithColumnSyntax WithIdColumn(
            this ICreateTableWithColumnSyntax syntax) =>
            syntax.WithColumn("Id").AsInt32().NotNullable().PrimaryKey().Identity();

        // Audit fields
        public static ICreateTableColumnOptionOrWithColumnSyntax WithAuditFields(
            this ICreateTableColumnOptionOrWithColumnSyntax syntax) =>
            syntax
                .WithColumn("CreatedAt").AsDateTime().NotNullable()
                    .WithDefaultValue(SystemMethods.CurrentDateTime)
                .WithColumn("CreatedBy").AsString(100).Nullable()
                .WithColumn("UpdatedAt").AsDateTime().Nullable()
                .WithColumn("UpdatedBy").AsString(100).Nullable();

        // Soft delete
        public static ICreateTableColumnOptionOrWithColumnSyntax WithSoftDelete(
            this ICreateTableColumnOptionOrWithColumnSyntax syntax) =>
            syntax
                .WithColumn("IsDeleted").AsBoolean().NotNullable().WithDefaultValue(false)
                .WithColumn("DeletedAt").AsDateTime().Nullable()
                .WithColumn("DeletedBy").AsString(100).Nullable();
    }
}
```

Custom extensions are a powerful way to make your migrations more maintainable and reduce duplication while ensuring consistency across your database schema evolution.