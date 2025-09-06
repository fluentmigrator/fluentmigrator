# Multi-Column Foreign Keys for SQLite

This document explains how to use FluentMigrator's multi-column foreign key support for SQLite databases.

## Background

SQLite requires foreign keys to be defined as part of the `CREATE TABLE` statement. Unlike other databases that support `ALTER TABLE ADD CONSTRAINT FOREIGN KEY`, SQLite foreign keys must be created when the table is initially created.

Previously, FluentMigrator's fluent syntax only supported single-column foreign keys in CREATE TABLE statements. This limitation made it impossible to create composite foreign keys (referencing multiple columns) using the fluent API.

## New Syntax

FluentMigrator now supports multi-column foreign keys through new overloads of the `ForeignKey` method:

```csharp
// Basic multi-column foreign key
.ForeignKey(string[] foreignColumns, string primaryTableName, string[] primaryColumns)

// Named multi-column foreign key
.ForeignKey(string foreignKeyName, string[] foreignColumns, string primaryTableName, string[] primaryColumns)

// Multi-column foreign key with schema
.ForeignKey(string foreignKeyName, string[] foreignColumns, string primaryTableSchema, string primaryTableName, string[] primaryColumns)
```

## Examples

### Example 1: Basic Multi-Column Foreign Key

```csharp
[Migration(20241213001)]
public class CreateAreaTables : Migration
{
    public override void Up()
    {
        // Create the referenced table first
        Create.Table("AreaGroup")
            .WithColumn("ArticleId").AsString().NotNullable()
            .WithColumn("Index").AsInt32().NotNullable()
            .WithColumn("Name").AsString().Nullable();

        // Create table with multi-column foreign key
        Create.Table("Area")
            .WithColumn("ArticleId").AsString().NotNullable()
            .WithColumn("AreaGroupIndex").AsInt32().NotNullable()
            .WithColumn("Index").AsInt32().NotNullable()
            .ForeignKey(["ArticleId", "AreaGroupIndex"], "AreaGroup", ["ArticleId", "Index"])
            .OnDelete(Rule.Cascade);
    }

    public override void Down()
    {
        Delete.Table("Area");
        Delete.Table("AreaGroup");
    }
}
```

This generates the following SQL:

```sql
CREATE TABLE "AreaGroup" ("ArticleId" TEXT NOT NULL, "Index" INTEGER NOT NULL, "Name" TEXT);

CREATE TABLE "Area" (
    "ArticleId" TEXT NOT NULL, 
    "AreaGroupIndex" INTEGER NOT NULL, 
    "Index" INTEGER NOT NULL, 
    CONSTRAINT "FK_Area_AreaGroup" FOREIGN KEY ("ArticleId", "AreaGroupIndex") 
    REFERENCES "AreaGroup" ("ArticleId", "Index") ON DELETE CASCADE
);
```

### Example 2: Named Multi-Column Foreign Key

```csharp
Create.Table("Area")
    .WithColumn("ArticleId").AsString().NotNullable()
    .WithColumn("AreaGroupIndex").AsInt32().NotNullable()
    .WithColumn("Index").AsInt32().NotNullable()
    .ForeignKey("FK_Area_References_AreaGroup", 
                ["ArticleId", "AreaGroupIndex"], 
                "AreaGroup", 
                ["ArticleId", "Index"])
    .OnDelete(Rule.Cascade);
```

### Example 3: Multi-Column Foreign Key with Schema

```csharp
Create.Table("Area").InSchema("content")
    .WithColumn("ArticleId").AsString().NotNullable()
    .WithColumn("AreaGroupIndex").AsInt32().NotNullable()
    .WithColumn("Index").AsInt32().NotNullable()
    .ForeignKey("FK_Area_AreaGroup", 
                ["ArticleId", "AreaGroupIndex"], 
                "content",  // primary table schema
                "AreaGroup", 
                ["ArticleId", "Index"])
    .OnDelete(Rule.Cascade);
```

## Usage Notes

1. **Column Order Matters**: The order of columns in the `foreignColumns` array must match the order in the `primaryColumns` array.

2. **Referenced Table Must Exist**: The referenced table (primary table) must be created before the table with the foreign key.

3. **Primary Key Not Required**: The referenced columns don't have to be a primary key, but they should have a unique constraint for proper foreign key behavior.

4. **Cascade Rules**: You can still use `.OnDelete()` and `.OnUpdate()` after defining the foreign key to specify cascade behavior.

5. **Works with All Column Types**: The feature works with any column types, not just the string/integer example shown.

## Migration from Execute.Sql

If you were previously using `Execute.Sql` for multi-column foreign keys, you can now use the fluent syntax:

### Before (Execute.Sql)
```csharp
Execute.Sql(@"
    CREATE TABLE Area(
        ArticleId TEXT NOT NULL,
        AreaGroupIndex INTEGER NOT NULL,
        ""Index"" INTEGER NOT NULL,
        PRIMARY KEY(ArticleId, AreaGroupIndex, ""Index""),
        FOREIGN KEY(ArticleId, AreaGroupIndex) REFERENCES AreaGroup(ArticleId, ""Index"") ON DELETE CASCADE
    )
");
```

### After (Fluent Syntax)
```csharp
Create.Table("Area")
    .WithColumn("ArticleId").AsString().NotNullable()
    .WithColumn("AreaGroupIndex").AsInt32().NotNullable()
    .WithColumn("Index").AsInt32().NotNullable()
    .ForeignKey(["ArticleId", "AreaGroupIndex"], "AreaGroup", ["ArticleId", "Index"])
    .OnDelete(Rule.Cascade);

Create.PrimaryKey("PK_Area")
    .OnTable("Area")
    .Columns("ArticleId", "AreaGroupIndex", "Index");
```

## Benefits

- **Type Safety**: Fluent syntax provides compile-time checking of column names and types
- **Database Portability**: The same fluent code works across different database providers
- **Code Reusability**: You can reuse table/column definitions across migrations
- **IntelliSense Support**: IDEs provide better autocomplete and navigation support
- **Consistency**: Maintains the same fluent style as other FluentMigrator operations

## Database Support

This feature is specifically designed for SQLite, where foreign keys must be created as part of the CREATE TABLE statement. For other databases, continue to use the existing `Create.ForeignKey()` syntax for standalone foreign key creation.