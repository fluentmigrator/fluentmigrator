# MySQL

MySQL is a popular open-source relational database management system. FluentMigrator provides comprehensive support for MySQL, including MySQL-specific data types, storage engines, and features.

## Getting Started with MySQL

### Installation

Install the MySQL provider package:

```bash
# For .NET CLI
dotnet add package FluentMigrator.Runner.MySql

# For Package Manager Console
Install-Package FluentMigrator.Runner.MySql
```

### Basic Configuration

```csharp
services.AddFluentMigratorCore()
    .ConfigureRunner(rb => rb
        .AddMySql()
        .WithGlobalConnectionString(connectionString)
        .ScanIn(Assembly.GetExecutingAssembly()).For.Migrations())
    .AddLogging(lb => lb.AddFluentMigratorConsole());
```

## MySQL-Specific Data Types

Column types are specified in the DBMS specific type map classes :

* [MySQL 4](https://github.com/fluentmigrator/fluentmigrator/blob/main/src/FluentMigrator.Runner.MySql/Generators/MySql/MySql4TypeMap.cs)
* [MySQL 5](https://github.com/fluentmigrator/fluentmigrator/blob/main/src/FluentMigrator.Runner.MySql/Generators/MySql/MySql5TypeMap.cs)
* [MySQL 8+](https://github.com/fluentmigrator/fluentmigrator/blob/main/src/FluentMigrator.Runner.MySql/Generators/MySql/MySql8TypeMap.cs)

### MySQL Enum and Set Types

```csharp
public class MySqlEnumSetTypes : Migration
{
    public override void Up()
    {
        Create.Table("ProductCatalog")
            .WithColumn("Id").AsInt32().NotNullable().PrimaryKey().Identity()
            .WithColumn("Name").AsString(100).NotNullable()

            // ENUM type for single selection
            .WithColumn("Size").AsCustom("ENUM('XS','S','M','L','XL','XXL')").NotNullable()
            .WithColumn("Status").AsCustom("ENUM('draft','published','archived')").NotNullable()
                .WithDefaultValue(RawSql.Insert("'draft'"))
            .WithColumn("Priority").AsCustom("ENUM('low','medium','high','urgent')").NotNullable()
                .WithDefaultValue(RawSql.Insert("'medium'"))

            // SET type for multiple selections
            .WithColumn("Features").AsCustom("SET('waterproof','breathable','insulated','reflective')").Nullable()
            .WithColumn("Colors").AsCustom("SET('red','blue','green','black','white','yellow')").Nullable()
            .WithColumn("Categories").AsCustom("SET('outdoor','sports','casual','formal','work')").Nullable()

            .WithColumn("CreatedAt").AsDateTime().NotNullable()
                .WithDefaultValue(SystemMethods.CurrentDateTime);
    }

    public override void Down()
    {
        Delete.Table("ProductCatalog");
    }
}
```

## MySQL Storage Engines

### Specifying Storage Engines

```csharp
public class MySqlStorageEngines : Migration
{
    public override void Up()
    {
        // InnoDB table (default, ACID compliant, supports foreign keys)
        Create.Table("Orders")
            .WithColumn("Id").AsInt32().NotNullable().PrimaryKey().Identity()
            .WithColumn("CustomerId").AsInt32().NotNullable()
            .WithColumn("OrderDate").AsDateTime().NotNullable()
            .WithColumn("TotalAmount").AsDecimal(10, 2).NotNullable();

        Execute.Sql("ALTER TABLE Orders ENGINE = InnoDB");

        // MyISAM table (fast for read-heavy operations, no foreign keys)
        Create.Table("SearchIndex")
            .WithColumn("Id").AsInt32().NotNullable().PrimaryKey().Identity()
            .WithColumn("DocumentId").AsInt32().NotNullable()
            .WithColumn("Keywords").AsCustom("TEXT").NotNullable()
            .WithColumn("RelevanceScore").AsFloat().NotNullable();

        Execute.Sql("ALTER TABLE SearchIndex ENGINE = MyISAM");

        // Memory table (stored in RAM, very fast but volatile)
        Create.Table("SessionData")
            .WithColumn("SessionId").AsString(128).NotNullable().PrimaryKey()
            .WithColumn("UserId").AsInt32().Nullable()
            .WithColumn("Data").AsCustom("TEXT").Nullable()
            .WithColumn("LastAccessed").AsCustom("TIMESTAMP").NotNullable()
                .WithDefaultValue(SystemMethods.CurrentDateTime);

        Execute.Sql("ALTER TABLE SessionData ENGINE = MEMORY");

        // Archive table (compressed storage for historical data)
        Create.Table("AuditLogArchive")
            .WithColumn("Id").AsInt32().NotNullable().PrimaryKey().Identity()
            .WithColumn("TableName").AsString(100).NotNullable()
            .WithColumn("Action").AsString(20).NotNullable()
            .WithColumn("RecordId").AsInt32().NotNullable()
            .WithColumn("ChangedAt").AsDateTime().NotNullable();

        Execute.Sql("ALTER TABLE AuditLogArchive ENGINE = ARCHIVE");
    }

    public override void Down()
    {
        Delete.Table("AuditLogArchive");
        Delete.Table("SessionData");
        Delete.Table("SearchIndex");
        Delete.Table("Orders");
    }
}
```

## Common Issues and Solutions

### Issue: Character Set and Collation
Always specify UTF-8 character set for international applications:

```csharp
// Set table character set and collation
Execute.Sql("ALTER TABLE Users CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci");

// Create table with specific character set
Create.Table("LocalizedContent")
    .WithColumn("Id").AsInt32().NotNullable().PrimaryKey().Identity()
    .WithColumn("Title").AsString(255).NotNullable()
    .WithColumn("Content").AsCustom("TEXT").NotNullable();

Execute.Sql("ALTER TABLE LocalizedContent ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci");
```

### Issue: Case Sensitivity
MySQL column names are case-insensitive, but be consistent with naming:

```csharp
// Good - consistent naming
Create.Table("users")
    .WithColumn("user_id").AsInt32().NotNullable().PrimaryKey()
    .WithColumn("username").AsString(50).NotNullable();
```
