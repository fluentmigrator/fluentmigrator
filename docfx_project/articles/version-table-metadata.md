---
uid: version-table-metadata
title: Custom metadata for the VersionInfo table
---

# Custom metadata for the VersionInfo table

By implementing the [IVersionTableMetaData](xref:FluentMigrator.Runner.VersionTableInfo.IVersionTableMetaData) interface you can change the defaults for the VersionInfo table. The interface exposes six properties:

Property | Default value | Description
---------|---------------|-------------
[SchemaName](xref:FluentMigrator.Runner.VersionTableInfo.IVersionTableMetaData.SchemaName) | (empty) | The schema where the version table is stored
[TableName](xref:FluentMigrator.Runner.VersionTableInfo.IVersionTableMetaData.TableName) | `"VersionInfo"` | The table where the version information is stored
[ColumnName](xref:FluentMigrator.Runner.VersionTableInfo.IVersionTableMetaData.ColumnName) | `"Version"` | The name of the column where the version numbers are stored
[DescriptionColumnName](xref:FluentMigrator.Runner.VersionTableInfo.IVersionTableMetaData.DescriptionColumnName) | `"Description"` | The name of the last migration applied
[AppliedOnColumnName](xref:FluentMigrator.Runner.VersionTableInfo.IVersionTableMetaData.AppliedOnColumnName) | `"AppliedOn"` | The datetime of when the last migration was applied
[UniqueIndexName](xref:FluentMigrator.Runner.VersionTableInfo.IVersionTableMetaData.UniqueIndexName) | `"UC_Version"` | The name of the unique constraint for the version column

In the same assembly that your migrations are located, create a new class (it has to be public) that implements the [IVersionTableMetaData](xref:FluentMigrator.Runner.VersionTableInfo.IVersionTableMetaData) interface and decorate the class with the [VersionTableMetaDataAttribute](xref:FluentMigrator.Runner.VersionTableInfo.VersionTableMetaDataAttribute). FluentMigrator will automatically find this and use it instead of the default settings.

> [!NOTE]
> The custom [IVersionTableMetaData](xref:FluentMigrator.Runner.VersionTableInfo.IVersionTableMetaData) is filtered by the values in the [TypeFilterOptions](xref:FluentMigrator.Runner.Initialization.TypeFilterOptions). This allows different [IVersionTableMetaData](xref:FluentMigrator.Runner.VersionTableInfo.IVersionTableMetaData) for every database.

A common use case is changing the default schema so that you can have a migration assembly per schema. 

```c#
using FluentMigrator.VersionTableInfo;

namespace Migrations
{
    [VersionTableMetaData]
    public class VersionTable : IVersionTableMetaData
    {
        public string ColumnName
        {
            get { return "Version"; }
        }

        public string SchemaName
        {
            get { return ""; }
        }

        public string TableName
        {
            get { return "Version2"; }
        }

        public string UniqueIndexName
        {
            get { return "UC_Version"; }
        }

        public virtual string AppliedOnColumnName
        {
            get { return "AppliedOn"; }
        }

        public virtual string DescriptionColumnName
        {
            get { return "Description"; }
        }
    }
}
```

# Overriding the DefaultVersionTableMetaData class

If you want to keep most of the default values and just change one or two of the properties. Then you can create a class that inherits from DefaultVersionTableMetaData and override the property to be changed. Don't forget to add the VersionTableMetaData attribute to the class.

```c#
[VersionTableMetaData]
public class VersionTable : DefaultVersionTableMetaData
{
    public override string ColumnName
    {
        get { return "Version"; }
    }
}
```
