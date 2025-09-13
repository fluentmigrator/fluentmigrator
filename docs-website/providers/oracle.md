# Oracle

Oracle Database is a powerful enterprise relational database management system. FluentMigrator provides comprehensive support for Oracle, including Oracle-specific data types, features, and optimizations.

## Getting Started with Oracle

### Installation

Install the Oracle provider package:

```bash
# For .NET CLI
dotnet add package FluentMigrator.Runner.Oracle

# For Package Manager Console
Install-Package FluentMigrator.Runner.Oracle
```

### Basic Configuration

```csharp
services.AddFluentMigratorCore()
    .ConfigureRunner(rb => rb
        .AddOracle()
        .WithGlobalConnectionString(connectionString)
        .ScanIn(Assembly.GetExecutingAssembly()).For.Migrations())
    .AddLogging(lb => lb.AddFluentMigratorConsole());
```

## Oracle Data Types

Column types are specified in the [DBMS specific type map class](https://github.com/fluentmigrator/fluentmigrator/blob/main/src/FluentMigrator.Runner.Oracle/Generators/Oracle/OracleTypeMap.cs).

## Troubleshooting Oracle Issues

### Common Oracle Migration Issues

```csharp
public class OracleTroubleshooting : Migration
{
    public override void Up()
    {
        // Issue 1: Long identifier names (Oracle 12.1 allows 128 chars, earlier versions 30)
        var longTableName = "VERY_LONG_TABLE_NAME_THAT_MIGHT_EXCEED_LIMITS";
        if (longTableName.Length > 30)
        {
            longTableName = "LONG_TABLE_NAME"; // Truncate for older Oracle versions
        }

        Create.Table(longTableName)
            .WithColumn("ID").AsInt32().NotNullable().PrimaryKey()
            .WithColumn("DESCRIPTION").AsString(4000).Nullable();

        // Issue 2: Data type precision and scale
        Create.Table("PRECISION_TEST")
            .WithColumn("ID").AsInt32().NotNullable().PrimaryKey()
            .WithColumn("DECIMAL_VALUE").AsDecimal(38, 4).NotNullable() // Oracle max precision is 38
            .WithColumn("NUMBER_VALUE").AsCustom("NUMBER(10,2)").NotNullable()
            .WithColumn("FLOAT_VALUE").AsCustom("FLOAT(126)").Nullable(); // Oracle FLOAT precision

        // Issue 3: Character set and national character set
        Create.Table("CHARACTER_SET_TEST")
            .WithColumn("ID").AsInt32().NotNullable().PrimaryKey()
            .WithColumn("VARCHAR2_COL").AsString(4000).NotNullable() // Max 4000 bytes
            .WithColumn("NVARCHAR2_COL").AsCustom("NVARCHAR2(2000)").Nullable() // Max 2000 chars
            .WithColumn("CHAR_COL").AsFixedLengthString(2000).Nullable() // Max 2000 bytes
            .WithColumn("NCHAR_COL").AsCustom("NCHAR(1000)").Nullable(); // Max 1000 chars

        // Issue 4: Date handling and time zones
        Create.Table("DATE_TEST")
            .WithColumn("ID").AsInt32().NotNullable().PrimaryKey()
            .WithColumn("DATE_COL").AsDateTime().NotNullable()
            .WithColumn("TIMESTAMP_COL").AsCustom("TIMESTAMP").NotNullable()
            .WithColumn("TIMESTAMP_TZ_COL").AsCustom("TIMESTAMP WITH TIME ZONE").Nullable()
            .WithColumn("TIMESTAMP_LTZ_COL").AsCustom("TIMESTAMP WITH LOCAL TIME ZONE").Nullable();

        // Issue 5: Handling NULL vs empty strings (Oracle treats empty string as NULL)
        Insert.IntoTable("CHARACTER_SET_TEST")
            .Row(new
            {
                ID = 1,
                VARCHAR2_COL = "Test Value", // Don't use empty string - Oracle converts to NULL
                NVARCHAR2_COL = "Unicode Test: 测试",
                CHAR_COL = "Fixed Length",
                NCHAR_COL = "Unicode Fixed"
            });

        // Issue 6: Foreign key constraint issues
        Execute.Sql("ALTER SESSION SET FOREIGN_KEY_CHECKS = TRUE"); // Oracle equivalent concept

        // Check for orphaned records before creating foreign keys
        Execute.Sql(@"
            SELECT COUNT(*) FROM
            (SELECT 1 FROM DUAL WHERE 1=0) -- Placeholder query
            ");

        // Issue 7: Sequence and trigger issues
        Execute.Sql("CREATE SEQUENCE SEQ_PRECISION_TEST START WITH 1 INCREMENT BY 1 NOCACHE");

        Execute.Sql(@"
            CREATE OR REPLACE TRIGGER TR_PRECISION_TEST_BI
            BEFORE INSERT ON PRECISION_TEST
            FOR EACH ROW
            WHEN (NEW.ID IS NULL)
            BEGIN
                SELECT SEQ_PRECISION_TEST.NEXTVAL INTO :NEW.ID FROM DUAL;
            EXCEPTION
                WHEN OTHERS THEN
                    RAISE_APPLICATION_ERROR(-20001, 'Error in trigger: ' || SQLERRM);
            END;");
    }

    public override void Down()
    {
        Execute.Sql("DROP TRIGGER TR_PRECISION_TEST_BI");
        Execute.Sql("DROP SEQUENCE SEQ_PRECISION_TEST");
        Delete.Table("DATE_TEST");
        Delete.Table("CHARACTER_SET_TEST");
        Delete.Table("PRECISION_TEST");
        Delete.Table("LONG_TABLE_NAME");
    }
}
```

## Best Practices for Oracle

### Use Sequences for Primary Keys
Oracle sequences provide better performance than identity columns for high-volume applications:

```csharp
// Create sequence first
Create.Sequence("SEQ_ORDERS")
    .StartWith(1000)
    .IncrementBy(1)
    .Cache(100);

// Use sequence in table
Create.Table("ORDERS")
    .WithColumn("ORDER_ID").AsInt32().NotNullable().PrimaryKey()
        .WithDefaultValue(RawSql.Insert("SEQ_ORDERS.NEXTVAL"))
    .WithColumn("ORDER_DATE").AsDateTime().NotNullable();
```

### Handle Oracle Naming Conventions
Oracle converts unquoted identifiers to uppercase:

```csharp
// Will be converted to USERS, USER_ID, USER_NAME
Create.Table("Users")
    .WithColumn("UserId").AsInt32().NotNullable().PrimaryKey()
    .WithColumn("UserName").AsString(100).NotNullable();

// Use quoted identifiers to preserve case (not recommended)
Execute.Sql("CREATE TABLE \"Users\" (\"UserId\" NUMBER NOT NULL)");
```
