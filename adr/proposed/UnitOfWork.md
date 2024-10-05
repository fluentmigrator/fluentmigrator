# Overview

Currently, FluentMigrator has minimal support for:
1. Conditionals
    1. e.g. testing if an object exists before creating or altering the object
2. Previewing a migration script
    2. e.g. for hand-off to a DBA to peer review and then run on a production machine
3. Persistence ignorance
    1. e.g., ability to preview a migration script is really just a form of "persistence ignorance", since we're persisting the migrations to a .sql file instead of to a `System.Data.IDbConnection`.

The purpose of this document is to address the following user stories and bugs/limitations:

## User Stories
1. As an Engineer working in a regulated industry where engineers are not allowed access to production databases, I want to Preview the Up migration script, so that a Database Adminstrator can peer review it and run it for me against the production database.
2. As an Engineer working in an industry where downtime can cost millions of dollars, I want to Preview the Rollback migration script, so that a Database Adminstrator can peer review it and its consistency with the Up migration script, and run it in case of emergency.

## Bugs / Limitations
1. #1047 : Output generated with connection is different from one using --no-connection
2. #978 : Improve VersionInfo dependency injection
  1. Separate VersionInfo tables for different schemas (e.g., for MySQL, where schema is a synonym for database within a MySQL server instance).
  2. Alternatively, extend the main VersionInfo table with extra columns (e.g., Table Per Hierarchy pattern, where each database is a unique Discriminator value)
3. #708 : Support for disconnected scripting
  1. See also: #1263 : if_exists works in reverted way on Oracle
4. #1036 : Write example docs for ISupportAdditionalFeatures (extend GeneratorBase / GenericProcessorBase sub-classes)
  1. `IMigrationContext` is not exposed to `ISupportAdditionalFeatures`
  2. Cannot mock an `Up` expression if the `Up` expression contains an extension method which depends on the `IMigrationContext`
     1. Note, the extension method doesn't have to be an `ISupportAdditionalFeatures`.  It can be any extension method that is a functor on `IMigration`, like so:
	     ```c#
		 using FluentMigrator.Builders.Create.Table;
         namespace Database.Extensions
		 {
		    public static class CreateTableSyntaxExtensions
		    {
			    public static ICreateTableWithColumnSyntax WithColumnsForLookupTable(this ICreateTableWithColumnSyntax syntax, string redundantTableName)
			    {
				    return syntax
					  .WithColumn($"{redundantTableName}ID").AsInt64().NotNullable().PrimaryKey($"PK_{redundantTableName}")
					  .WithColumn("Name").AsString(50).NotNullable().Unique($"UQ_{redundantTableName}_Name")
					  .WithColumn("Description").AsString(100).NotNullable();
			    }
		   }
		 }
		 ```

# Architecture

The proposal here is to abstract away the `System.Data` layer, as much as possible.  To do that, we will use a Unit of Work pattern.  Good surveys of the Unit of Work pattern are available in:

1. [Survey of Entity Framework Unit of Work Patterns](https://lostechies.com/derekgreer/2015/11/01/survey-of-entity-framework-unit-of-work-patterns/)
2. [Applying Domain-Driven Design and Patterns: With Examples in C# and .NET, by Jimmy Nilsson](https://www.amazon.com/Applying-Domain-Driven-Design-Patterns-Examples/dp/0321268202/)
    1. See Page 200.  Jimmy Nilsson describes a different approach to the Unit of Work, and that is to let the repositories delegate all of their work to a Unit of Work,
	   and then the Unit of Work then makes all necessary database calls (or other types of resource calls) on behalf of the repositories.
	   1. This approach hides a lot of plumbing calls inside of domain objects or from application-level code.
    1. Uses NHibernate, but contains good Unit Of Work example that is different from the classical Object/Caller registration styles.
	2. Excellent book on how to use Test-Driven Development to grow a domain model.
3. [.NET Domain-Driven Design with C#: Problem – Design – Solution, by Tim McCarthy](https://www.amazon.com/gp/product/0470147563/)
4. Personal experience

The Unit of Work pattern, as defined by Martin Fowler (Fowler, Patterns of Enterprise Application Architecture , 184).
According to Martin, the Unit of Work:

> maintains a list of objects affected by a business transaction and coordinates the writing out of changes and the resolution of concurrency problems.

The Unit of Work needs to know what objects it should keep track of.  Martin describes two ways this can be accomplished:

1. Caller registration — The user of the object has to remember to register with the Unit of Work.
2. Object registration — The objects register themselves with the Unit of Work.

However, a third possibility is that the objects don't know anything about the Unit of Work.

```
.------------------------.
|                        |
| IUnitOfWork            |
|                        |
'------------------------'
             |
.------------------------.
|                        |
| IUnitOfWorkParticipant |
|                        |
'------------------------'
```

```c#
public interface IUnitOfWorkParticipant
{
  
}

public interface IMigration : IUnitOfWorkParticipant
{
  /* Pre-existing IMigration interface details elided */
}

public interface IUnitOfWork
{
  void Enlist(IUnitOfWorkParticipant participant);
  void Commit();
}
```
