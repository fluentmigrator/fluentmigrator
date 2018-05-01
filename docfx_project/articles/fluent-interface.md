---
uid: fluent-interface.md
title: Fluent Interface
---

# Fluent Interface

The FM fluent api allows you to create tables, columns, indexes and (nearly) every construct you need to manipulate your database structure.

Behind the scenes, the fluent api populates a semantic model that FM uses to analyze and apply migrations in batch. The fluent api that is available in your Migration class starts with five main root expressions as follows:

# Create Expression

Allows you to create a table, column, index, foreign key and schema.

```cs
Create.Table("Users")
    .WithIdColumn() // WithIdColumn is an extension, see below link.
    .WithColumn("Name").AsString().NotNullable();
```

```cs
Create.ForeignKey() // You can give the FK a name or just let Fluent Migrator default to one
    .FromTable("Users").ForeignColumn("CompanyId")
    .ToTable("Company").PrimaryColumn("Id");
```

Example extensions can be found in the [example MigrationExtensions.cs](https://github.com/fluentmigrator/fluentmigrator/blob/master/samples/FluentMigrator.Example.Migrations/MigrationExtensions.cs).

Otherwise, you can replace WithIdColumn with
```cs
.WithColumn("Id").AsInt32().NotNullable().PrimaryKey().Identity();
```

# Alter Expression

Allows you to alter existing tables and columns.

```cs
Alter.Table("Bar")
    .AddColumn("SomeDate")
    .AsDateTime()
    .Nullable();
```
```cs
Alter.Table("Bar")
    .AlterColumn("SomeDate")
    .AsDateTime()
    .NotNullable();
```
```cs
Alter.Column("SomeDate")
    .OnTable("Bar")
    .AsDateTime()
    .NotNullable();
```

# Delete Expression

Allows you to delete a table, column, foreign key and schema.

```cs
Delete.Table("Users");
```

## Delete Multiple Columns

Delete multiple columns from a table using the syntax in this expression:
```cs
Delete.Column("AllowSubscription").Column("SubscriptionDate").FromTable("Users");
```

# Execute Expression

Allows you to execute a block of sql, or a script by name (ie. myscript.sql) or an embedded sql script. To embed an sql script, add the file to your migration project and change the Build Action property to Embedded Resource.

```cs
Execute.Script("myscript.sql");
Execute.EmbeddedScript("UpdateLegacySP.sql");
Execute.Sql("DELETE TABLE Users");
```

# Rename Expression

Allows you to rename a column or table.

```cs
Rename.Table("Users").To("UsersNew");
Rename.Column("LastName").OnTable("Users").To("Surname");
```

# Data Expressions

Allows you to insert a row into a table using an anonymous type for the rows contents

```cs
Insert.IntoTable("Users").Row(new { FirstName = "John", LastName = "Smith" });
```

```cs
Delete.FromTable("Users").AllRows(); // delete all rows
Delete.FromTable("Users").Row(new { FirstName = "John" }); // delete all rows with FirstName==John
Delete.FromTable("Users").IsNull("Username"); //Delete all rows where Username is null
```

```cs
Update.Table("Users").Set(new { Name = "John" }).Where(new { Name = "Johnanna" });
```

## Insert data as an non-Unicode string

If you want to insert a string as non-Unicode (ANSI) then use the NonUnicodeString class:

```cs
Insert.IntoTable("TestTable").Row(new { Name = new NonUnicodeString("ansi string") });
```

## AllRows Attribute

A common task is to add a non-nullable column without a default value. One way this can be achieved is with the 'AllRows' attribute, via these three steps:

1. Add new nullable column.
```cs
Alter.Table("Bar")
    .AddColumn("SomeDate")
    .AsDateTime()
    .Nullable();
```
2. Update all rows to an initial value using the AllRows attribute.
```cs
Update.Table("Bar")
    .Set(new { SomeDate = DateTime.Today })
    .AllRows();
```
3. Change the column to be non-nullable.
```cs
Alter.Table("Bar")
    .AlterColumn("SomeDate")
    .AsDateTime()
    .NotNullable();
```


As of version 1.3.0, this can be done with a single statement using the SetExistingRowsTo method.

```cs
Alter.Table("Bar")
    .AddColumn("SomeDate")
    .AsDateTime()
    .SetExistingRowsTo(DateTime.Today)
    .NotNullable();
```

# IfDatabase Expression

Allows for conditional expressions depending on database type. The current database types supported are:

[!include[Supported databases](../snippets/supported-databases.md)]

Multiple database types (as specified above) can be passed into the IfDatabase Expression (see [Dealing with multiple database types](multi-db-support.md) for more details).

```cs
IfDatabase("SqlServer", "Postgres")
    .Create.Table("Users")
    .WithIdColumn()
    .WithColumn("Name").AsString().NotNullable();

IfDatabase("Sqlite")
    .Create.Table("Users")
    .WithColumn("Id").AsInt16().PrimaryKey()
    .WithColumn("Name").AsString().NotNullable();
```

# Schema.Exists Expressions

You can write migrations conditional on the preexisting schema, which comes in handy for getting you out of certain jams. For instance, if you need to make a column but aren't sure if it already exists:

```cs
if (!Schema.Table("Users").Column("FirstName").Exists())
{
    this.Create.Column("FirstName").OnTable("Users").AsAnsiString(128).Nullable();
}
```

Next up, [Profiles](profiles.md) are migrations that if specified, will always run regardless of what other migrations run.
