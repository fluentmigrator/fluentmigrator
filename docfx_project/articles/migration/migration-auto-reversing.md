---
uid: migration-auto-reversing
title: Auto-reversing migrations
---

# Auth-reversing migrations

Auto reversing migrations are migrations that only contain an up command and FluentMigrator figures out the down command. Create a migration class that inherits from [AutoReversingMigration](xref:FluentMigrator.AutoReversingMigration) (instead of [Migration](xref:FluentMigrator.Migration)) to use this feature.

```cs
[Migration(201207080104)]
public class RenameTableWithSchema : AutoReversingMigration
{
    public override void Up()
    {
        Rename.Table("TestTable2").InSchema("TestSchema").To("TestTable'3");
    }
}
```

FluentMigrator can  automatically figure out the down command and revert the rename. However, not all expressions are supported for auto reversing. For example, using the Execute expression to execute an sql script is impossible to reverse. Expressions that are currently supported for auto reversing are:

* Create.Table
* Create.Column
* Create.Index
* Create.ForeignKey
* Create.Schema
* Delete.ForeignKey
* Rename.Table
* Rename.Column
