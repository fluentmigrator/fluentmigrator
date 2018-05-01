---
uid: raw-sql
title: Raw SQL helper
---

# Raw SQL helper

When using the `Insert.IntoTable` expression, or when setting the default column value, all row data that is a string is quoted and saved in the database. If you want to use an sql expression instead then the [RawSql](xref:FluentMigrator.RawSql) helper class is what you need.

For example, if I want to use a Microsoft SQL Server function like `CURRENT_USER` and try to insert like this:

```c#
Insert.IntoTable("Users").Row(new { Username = "CURRENT_USER" });
```

The result will be that the Username column will get the value `CURRENT_USER` as a string. To execute the function you can use the [RawSql.Insert](xref:FluentMigrator.RawSql.Insert(System.String)) method like this:

```c#
Insert.IntoTable("User").Row(new { Username = RawSql.Insert("CURRENT_USER") });
```

This will result in the Username column being set to the current username. Be aware that by using an sql server specific function like `CURRENT_USER` that this expression is not portable anymore and will not work against another database (like PostgreSQL).
