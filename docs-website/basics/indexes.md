# Indexes

Indexes are crucial for database performance optimization. FluentMigrator provides comprehensive support for creating, modifying, and managing database indexes across different database providers.

## Basic Index Operations

### Creating Simple Indexes

```csharp
public class BasicIndexes : Migration
{
    public override void Up()
    {
        Create.Table("Users")
            .WithColumn("Id").AsInt32().NotNullable().PrimaryKey().Identity()
            .WithColumn("FirstName").AsString(50).NotNullable()
            .WithColumn("LastName").AsString(50).NotNullable()
            .WithColumn("Email").AsString(255).NotNullable()
            .WithColumn("CreatedAt").AsDateTime().NotNullable();

        // Single column index
        Create.Index("IX_Users_LastName")
            .OnTable("Users")
            .OnColumn("LastName");

        // Multi-column index
        Create.Index("IX_Users_FirstName_LastName")
            .OnTable("Users")
            .OnColumn("FirstName")
            .OnColumn("LastName");

        // Index with sort order
        Create.Index("IX_Users_CreatedAt_Desc")
            .OnTable("Users")
            .OnColumn("CreatedAt").Descending();
    }

    public override void Down()
    {
        Delete.Index("IX_Users_LastName").OnTable("Users");
        Delete.Index("IX_Users_FirstName_LastName").OnTable("Users");
        Delete.Index("IX_Users_CreatedAt_Desc").OnTable("Users");
        Delete.Table("Users");
    }
}
```

### Unique Indexes

```csharp
public class UniqueIndexes : Migration
{
    public override void Up()
    {
        Create.Table("Products")
            .WithColumn("Id").AsInt32().NotNullable().PrimaryKey().Identity()
            .WithColumn("Name").AsString(100).NotNullable()
            .WithColumn("SKU").AsString(50).NotNullable()
            .WithColumn("CompanyId").AsInt32().NotNullable()
            .WithColumn("InternalCode").AsString(20).NotNullable();

        // Simple unique index
        Create.Index("UQ_Products_SKU")
            .OnTable("Products")
            .OnColumn("SKU")
            .Unique();

        // Composite unique index
        Create.Index("UQ_Products_CompanyId_InternalCode")
            .OnTable("Products")
            .OnColumn("CompanyId")
            .OnColumn("InternalCode")
            .Unique();
    }

    public override void Down()
    {
        Delete.Index("UQ_Products_SKU").OnTable("Products");
        Delete.Index("UQ_Products_CompanyId_InternalCode").OnTable("Products");
        Delete.Table("Products");
    }
}
```

### Filtered Indexes

```csharp
public class FilteredIndexes : Migration
{
    public override void Up()
    {
        Create.Table("Orders")
            .WithColumn("Id").AsInt32().NotNullable().PrimaryKey().Identity()
            .WithColumn("CustomerId").AsInt32().NotNullable()
            .WithColumn("Status").AsString(20).NotNullable()
            .WithColumn("OrderDate").AsDateTime().NotNullable()
            .WithColumn("CompletedDate").AsDateTime().Nullable();

        // Filtered index for SQL Server, or PostgreSQL
        IfDatabase(ProcessorIdConstants.SqlServer, ProcessorIdConstants.Postgres).Delegate(() =>
        {
            Create.Index("IX_Orders_CustomerId_Active")
                .OnTable("Orders")
                .OnColumn("CustomerId")
                .WithOptions()
                .Filter("Status = 'Active'");

            // Filtered index excluding NULL values
            Create.Index("IX_Orders_CompletedDate")
                .OnTable("Orders")
                .OnColumn("CompletedDate")
                .WithOptions()
                .Filter("CompletedDate IS NOT NULL");
        });
    }

    public override void Down()
    {
        IfDatabase(ProcessorIdConstants.SqlServer, ProcessorIdConstants.Postgres).Delegate(() =>
        {
            Delete.Index("IX_Orders_CustomerId_Active").OnTable("Orders");
            Delete.Index("IX_Orders_CompletedDate").OnTable("Orders");
        });
        Delete.Table("Orders");
    }
}
```

## Database-Specific Index Features

Different database providers offer specialized indexing capabilities and advanced options:

### SQL Server Specific
- **Clustered vs Non-Clustered**: Advanced index clustering options - [SQL Server Provider](../providers/sql-server.md#clustered-and-non-clustered-indexes)
- **Covering Indexes**: Include columns for covering index optimization - [SQL Server Provider](../providers/sql-server.md#included-columns-covering-indexes)
- **Filtered Indexes**: WHERE clause conditions for selective indexing - [SQL Server Provider](../providers/sql-server.md#filtered-indexes)

### PostgreSQL Specific
- **Algorithm-Specific Indexes**: B-tree, Hash, GIN, GiST algorithms - [PostgreSQL Provider](../providers/postgresql.md#algorithm-specific-indexes)

## See Also
- [Execute SQL](../operations/execute-sql.md) - When you need to create indexes with provider-specific SQL
- [Columns](./columns.md) - Column indexing strategies and patterns
- [Constraints](./constraints.md) - Index-backed constraint types
