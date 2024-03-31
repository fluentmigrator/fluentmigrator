# FluentMigrator

FluentMigrator is a an open-source .NET library that allows you to manage and version database schema changes using a code-first approach. With FluentMigrator, you can define database migrations as code rather than maintaining SQL scripts or using other tools.

Some key features of FluentMigrator include:

- Code-based Migrations: You define database migrations as C# or VB.NET classes, making it easier to version control and maintain your schema changes.
- Fluent Interface: FluentMigrator provides a fluent interface for defining database objects like tables, columns, indexes, and constraints, making the code more readable and expressive.
- Cross-platform: FluentMigrator supports multiple databases, including SQL Server, PostgreSQL, MySQL, Oracle, and SQLite.
- Rollback Support: FluentMigrator allows you to roll back migrations, making it easier to undo changes or revert to a previous database state.
- Extensibility: You can create your own custom migrations, conventions, and processors to extend FluentMigrator's functionality.

### Getting Started

For a brief overview on getting started with FluentMigrator, please see the documentation links here:

- [FluentMigrator Docs](https://fluentmigrator.github.io/index.html)
- [FluentMigrator Introduction](https://fluentmigrator.github.io/articles/intro.html)

### Installation

You can install FluentMigrator via [NuGet](https://www.nuget.org/packages/FluentMigrator):

```
Install-Package FluentMigrator
```

### Usage

FluentMigrator example migration and usage:

```csharp
[Migration(202401011200)]
public class CreatePersonTable : Migration
{
    public override void Up()
    {
        Create.Table("People")
            .WithColumn("Id").AsGuid().PrimaryKey()
            .WithColumn("Name").AsString(100).NotNullable()
            .WithColumn("Email").AsString(200).Nullable();
    }

    public override void Down()
    {
        Delete.Table("People");
    }
}
```

For more detailed documentation and examples, please refer to [link to your comprehensive documentation].

## Feedback and Contributing

We welcome your feedback, bug reports, and contributions to FluentMigrator.

- To report a bug or request a feature, please [open an issue](https://github.com/fluentmigrator/fluentmigrator/issues) on our GitHub repository.

If you'd like to contribute to the project, please follow our [contributing guidelines](https://fluentmigrator.github.io/articles/guides/contribution.html).

## License

FluentMigrator is released under the Apache license. See the [LICENSE](https://github.com/fluentmigrator/fluentmigrator/blob/main/LICENSE.txt) file for more details.

