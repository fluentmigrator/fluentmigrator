---
uid: migration-example.md
title: Migrations
---

# Migrations

The basic unit of FM is the Migration abstract class. Your migrations will derive from this class and implement two methods. You also need to define the Migration attribute with a unique identifier.

Commonly, this identifier is just an incrementing value, although the attribute accepts a Int64, some people use a numbered date format such as yyyyMMdd. The significance of this number is for the ordering of your migrations. Lower numbers execute first and then larger. It also provides a way to target a specific migration. Your migration class that you create is where you define the migrations to execute.

Migrations can be run out of sequence if they are checked in out of sequence. For example you may checkin migration 1, 3 and 5 in one build, and migration 2 in a later build. Migration 2 will still get run even though it is not later than the most recent migration because it has not been run before.

Since you define both a UP and DOWN for each migration, it's possible to move forward and backwards in migrations at any point in time. The only caveat is that migrating down is destructive as tables and/or rows are being deleted in the process. Be sure to have a backup first if you want to keep the data

```cs
[Migration(1)]
public class CreateUserTable : Migration
{
    public override void Up()
    {
        Create.Table("Users");
    }

    public override void Down()
    {
        Delete.Table("Users");
    }
}
```

When you are migrating TO version 1 (if the database is empty for instance), then the migration code in the Up() method will be called creating a table called "Users" in the database.

When you are migrating to a version BEFORE version 1 (for instance version 0), then the migration code in the Down() method will be called deleting the "Users" table from the database.

Here's another sample migration that goes a little more in depth

```cs
[Migration(1)]
public class TestCreateAndDropTableMigration: Migration
{
	public override void Up()
	{
		Create.Table("TestTable")
			.WithColumn("Id").AsInt32().NotNullable().PrimaryKey().Identity()
			.WithColumn("Name").AsString(255).NotNullable().WithDefaultValue("Anonymous");

		Create.Table("TestTable2")
			.WithColumn("Id").AsInt32().NotNullable().PrimaryKey().Identity()
			.WithColumn("Name").AsString(255).Nullable()
			.WithColumn("TestTableId").AsInt32().NotNullable();

		Create.Index("ix_Name").OnTable("TestTable2").OnColumn("Name").Ascending()
			.WithOptions().NonClustered();

		Create.Column("Name2").OnTable("TestTable2").AsBoolean().Nullable();

		Create.ForeignKey("fk_TestTable2_TestTableId_TestTable_Id")
			.FromTable("TestTable2").ForeignColumn("TestTableId")
			.ToTable("TestTable").PrimaryColumn("Id");

		Insert.IntoTable("TestTable").Row(new { Name = "Test" });
	}

	public override void Down()
	{
		Delete.Table("TestTable2");
		Delete.Table("TestTable");
	}
}

```

Note that if you use SQL Server identity columns with [WithIdentityInsert](xref:sql-server-extensions.md#withidentityinsert)


Diving further into FM, we look at the [Fluent Interface](xref:fluent-interface.md) to make more useful migrations.
