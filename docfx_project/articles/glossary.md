---
uid: glossary
title: Glossary
---

# Expression

See: [Migration Expression](#migration-expression)

# Maintenance

Those are migrations that should always be executed at a specified stage.

> [!WARNING]
> Maintenance migrations are always run at the given stage when selected.

> [!NOTE]
> A migration must have:
> * `public` visibility
> * [IMigration](xref:FluentMigrator.IMigration) as implemented interface (The [Migration class](xref:FluentMigrator.Migration) already implements it)
> * [MaintenanceAttribute](xref:FluentMigrator.MaintenanceAttribute)

# Migration

A migration is a database modification usually applied within its own transaction.

> [!NOTE]
> A migration must have:
> * `public` visibility
> * [IMigration](xref:FluentMigrator.IMigration) as implemented interface (The [Migration class](xref:FluentMigrator.Migration) already implements it)
> * [MigrationAttribute](xref:FluentMigrator.MigrationAttribute)

# Migration Expression

An expression created from within a [migration](#migration) that gets used to
produce an SQL statement (using a [migration generator](#migration-generator))
that gets executed by a [migration processor](#migration-processor).

A migration expression always implements [IMigrationExpression](xref:FluentMigrator.Expressions.IMigrationExpression).

# Migration Generator

Translates an [expression](#migration-expression) created from within a [migration](#migration)
to one or more SQL statements.

A migration generator always implements [IMigrationGenerator](xref:FluentMigrator.IMigrationGenerator).

# Migration Processor

Passes the [expressions](#migration-expression) to a [migration generator](#migration-generator)
and then executes the resulting SQL statements.

A migration processor always implements [IMigrationProcessor](xref:FluentMigrator.IMigrationProcessor).

# Profile

A profile is used to selectively apply migrations.

> [!WARNING]
> Profiles are always run when selected.

> [!NOTE]
> A profile must have:
> * `public` visibility
> * [IMigration](xref:FluentMigrator.IMigration) as implemented interface (The [Migration class](xref:FluentMigrator.Migration) already implements it)
> * [ProfileAttribute](xref:FluentMigrator.ProfileAttribute)

