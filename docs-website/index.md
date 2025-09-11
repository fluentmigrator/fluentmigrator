---
layout: home

hero:
  name: FluentMigrator
  text: Database Schema Management
  tagline: A migration framework for .NET much like Ruby on Rails Migrations
  image:
    src: /logo-big.svg
    alt: FluentMigrator Logo
  actions:
    - theme: brand
      text: Quick Start
      link: /intro/quick-start
    - theme: alt
      text: View on GitHub
      link: https://github.com/fluentmigrator/fluentmigrator

features:
  - icon: 🚀
    title: Easy to Use
    details: Write database migrations in C# using a fluent API that's easy to learn and understand.

  - icon: 🗃️
    title: Multi-Database Support
    details: Supports SQL Server, PostgreSQL, MySQL, SQLite, Oracle, Firebird, and more database providers.

  - icon: ⚡
    title: Version Control Friendly
    details: Migrations are code that can be checked into version control and shared across teams.

  - icon: 🔄
    title: Rollback Support
    details: Define both Up and Down methods to enable rolling back migrations when needed.

  - icon: 🎯
    title: Conditional Logic
    details: Use conditional logic to create database-specific migrations for different providers.

  - icon: 🛠️
    title: Extensible
    details: Extensible architecture with support for custom extensions and database-specific features.
---

## What is FluentMigrator?

FluentMigrator is a migration framework for .NET that allows you to manage database schema changes in a structured, version-controlled way. Instead of manually running SQL scripts, you write migrations as C# classes that can be executed automatically.

## The Problem

Traditional database development often involves:
- Manual SQL scripts that need to be run by each developer
- No clear versioning of database changes
- Risk of inconsistencies between development, testing, and production environments
- Difficulty tracking what changes have been applied
- Database-specific SQL that doesn't work across different providers

## The Solution

FluentMigrator solves these problems by:
- **Code-based migrations**: Database changes are written in C# using a fluent API
- **Version control**: Migrations are part of your codebase and can be checked into source control
- **Automatic tracking**: FluentMigrator keeps track of which migrations have been applied
- **Database agnostic**: Write once, run on multiple database providers
- **Rollback support**: Define both Up and Down methods for reversible migrations

```csharp
[Migration(20240101000000)]
public class CreateUsersTable : Migration
{
    public override void Up()
    {
        Create.Table("Users")
            .WithColumn("Id").AsInt32().NotNullable().PrimaryKey().Identity()
            .WithColumn("Username").AsString(50).NotNullable().Unique()
            .WithColumn("Email").AsString(255).NotNullable()
            .WithColumn("CreatedAt").AsDateTime().NotNullable().WithDefault(SystemMethods.CurrentDateTime);
    }

    public override void Down()
    {
        Delete.Table("Users");
    }
}
```

## Benefits

### Clear syntax
* FluentMigrator uses a fluent API that reads like natural language:

### Version Control Integration
- Migrations are C# code files that integrate naturally with version control
- Easy to see what changed and when
- Collaborative development without merge conflicts
- Branching and merging of schema changes

### Database Provider Independence
- SQL Server
- PostgreSQL
- MySQL / MariaDB
- SQLite
- Oracle
- Firebird
- And more...

Write your migrations once and run them on any supported database.

### Team Development
- All team members can apply the same schema changes
- New developers can get up to date with a single command
- No more "Did you remember to run this script?" conversations

### CI/CD Integration
- Migrations can be automatically applied as part of deployment
- Consistent schema updates across all environments
- Reduced deployment errors

## Get Started

Ready to start using FluentMigrator? Check out our [Quick Start Guide](/intro/quick-start.md) to create your first migration in just a few minutes.

## Community & Support

- **GitHub**: [fluentmigrator/fluentmigrator](https://github.com/fluentmigrator/fluentmigrator)
- **Stack Overflow**: Use the [`fluent-migrator`](https://stackoverflow.com/questions/tagged/fluent-migrator) tag
- **Discussions**: [GitHub Discussions](https://github.com/fluentmigrator/fluentmigrator/discussions)
- **Issues**: [Bug Reports & Feature Requests](https://github.com/fluentmigrator/fluentmigrator/issues)
