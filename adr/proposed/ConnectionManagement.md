# Connection Management

* Status: proposed (the `IMigrationConnectionFactory` part is implemented)
* Related issue: [#1981](https://github.com/fluentmigrator/fluentmigrator/issues/1981)
* Related pull requests:
  * [#2002](https://github.com/fluentmigrator/fluentmigrator/pull/2002) by [@Merlin-Taylor](https://github.com/Merlin-Taylor) &mdash; custom `DbProviderFactory` for PostgreSQL
  * [#2268](https://github.com/fluentmigrator/fluentmigrator/pull/2268) by [@Sherif-Ahmed](https://github.com/Sherif-Ahmed) &mdash; provider-neutral `IMigrationConnectionFactory`
* Related ADR: [DependencyInjection.md](DependencyInjection.md) (row 13, "Connections")

## Context and Problem Statement

Historically the only supported way to tell FluentMigrator how to reach a database was a
connection string, resolved through `IConnectionStringReader`/`IConnectionStringAccessor`
and handed to `GenericProcessorBase`, which then created a connection from the provider's
`DbProviderFactory`:

```text
connection string -> DbProviderFactory.CreateConnection() -> IDbConnection
```

That pipeline breaks down whenever the connection cannot be fully described by a
connection string:

* Azure SQL / Azure Entra ID access tokens (`SqlConnection.AccessToken`).
* AWS RDS IAM authentication tokens, HashiCorp Vault, or any other short-lived secret.
* PostgreSQL dynamic passwords (`NpgsqlDataSourceBuilder.UsePasswordProvider`), which is
  the use case raised in issue #1981.
* Provider callbacks that only exist on the concrete connection or data source type
  (client certificates, `NpgsqlDataSourceBuilder.MapEnum`, connection interceptors, ...).
* Multi-tenant applications that resolve the target database at runtime.
* Applications that already own a connection (or an ambient unit of work) and want the
  migration runner to enlist in it rather than open a second one.

Two community proposals attacked this problem from different angles:

* @Merlin-Taylor's PR #2002 allowed a caller-supplied `DbProviderFactory` to be passed to
  `AddPostgres*`. Minimal and effective, but provider-specific, and it exposes
  `DbProviderFactory` &mdash; a *provider* abstraction &mdash; as the public extension
  point for what is really a *data source* concern. @Merlin-Taylor said so himself in the
  PR description, which is why it stayed in draft.
* @Sherif-Ahmed's PR #2268 introduced a provider-neutral `IMigrationConnectionFactory`
  that hands the runner a fully configured `IDbConnection`, regardless of how it was
  produced.

During the review of #2268 we also discussed whether FluentMigrator should instead reify
[`DbDataSource`](https://learn.microsoft.com/dotnet/api/system.data.common.dbdatasource)
(see [dotnet/runtime#64812](https://github.com/dotnet/runtime/issues/64812)) as its
primary connection abstraction. This ADR records the decision that was taken and sketches
what that `DbDataSource` design would look like, so the decision can be revisited when the
supported target frameworks make it viable.

## Decision Drivers

* Must work on every supported target framework. The runner multi-targets
  `net48;netstandard2.0;net8.0`, and `DbDataSource` exists only on .NET 7 and later, so it
  is unavailable on two of the three assets.
* Must work for every provider FluentMigrator supports, including those whose ADO.NET
  provider has no `DbDataSource` implementation (SQLite, Firebird, Oracle, DB2, HANA,
  Snowflake, ...), and for the provider assemblies that are loaded reflectively
  (`ReflectionBasedDbFactory`), which cannot take a compile-time dependency on the
  provider's types.
* Must not break `WithGlobalConnectionString(...)`, `IConnectionStringAccessor`, or the
  existing processor constructors; the connection-string path is the overwhelmingly common
  case and stays the default.
* Must respect connection lifetime: a connection that the application owns must never be
  closed or disposed by the runner.
* The extension point must be resolvable from Microsoft.Extensions.DependencyInjection,
  consistent with the rest of the runner (see `DependencyInjection.md`).
* Credentials that rotate must be observed per migration scope, so the abstraction must be
  consulted per connection rather than cached forever in a singleton.

## Decision

Reify the **connection** (`IDbConnection`), not the data source, as the core abstraction,
and treat `DbDataSource` as an *adapter* over it on the target frameworks that have it.

### The abstraction

`FluentMigrator.Runner.Initialization.IMigrationConnectionFactory` has three members:

| Member | Purpose |
| --- | --- |
| `bool HasConnection` | Whether a connection can be created at all. Replaces the "is the connection string empty?" test that previously decided whether the runner is connectionless (preview/`--no-connection` mode) and which `IVersionLoader` to use. |
| `bool OwnsConnection` | Whether the processor may close and dispose the connection it is given. |
| `IDbConnection CreateConnection(DbProviderFactory providerFactory)` | Produces the connection. The provider factory selected for the processor is passed in so that the default implementation, and any implementation that only wants to customise the connection string, does not have to know the concrete provider type. |

The default implementation, `ConnectionStringMigrationConnectionFactory`, wraps
`IConnectionStringAccessor` and reproduces the historical behaviour exactly. It is
registered in the container, so `WithGlobalConnectionString(...)` and the new APIs are
orthogonal: the connection string keeps flowing into `ProcessorOptions`, and a custom
factory simply replaces the *creation* step. The last configured factory wins, regardless
of the order relative to `WithGlobalConnectionString(...)`.

### The registrations

```csharp
// Anything you can build yourself, including rotating credentials.
.WithConnectionFactory(sp => new SqlConnection(connectionString)
{
    AccessToken = sp.GetRequiredService<ITokenProvider>().GetToken()
})

// The application keeps ownership; the runner will not close or dispose it.
.WithConnectionFactory(sp => sp.GetRequiredService<MyUnitOfWork>().Connection,
                       ownsConnection: false)

// .NET 7+ only: adapt a provider data source.
.WithDataSource(sp => sp.GetRequiredService<NpgsqlDataSource>())
```

`WithDataSource(...)` is a thin adapter that calls `DbDataSource.CreateConnection()` from
inside an `IMigrationConnectionFactory`. It is compiled only under `NET7_0_OR_GREATER`, so in
practice it ships in the `net8.0` asset and the `net48`/`netstandard2.0` assets keep
building.

### Lifetime and ownership

The factory is registered **scoped**, and `GenericProcessorBase` resolves the connection
lazily, once per processor instance. That means:

* One connection per migration scope, not one per command, so pooling and transaction
  semantics are unchanged.
* A rotating credential is observed at the start of each scope, which is the granularity
  users actually need (a token must be valid when the connection is opened; it does not
  need to be re-issued per statement).
* When `OwnsConnection` is `false`, `EnsureConnectionIsClosed()` and `Dispose(bool)` leave
  the connection alone. The processor still rolls back a transaction it started itself,
  because that transaction belongs to the processor and not to the caller.

Because a custom factory may return a connection from a *different* ADO.NET provider than
the one selected for the processor, `CreateCommandCore` falls back to
`IDbConnection.CreateCommand()` when assigning the connection to a provider-created command
throws, instead of surfacing an obscure `InvalidCastException`.

## Considered Alternative: reifying `DbDataSource`

`DbDataSource` is the more idiomatic modern ADO.NET abstraction. It represents a
*configured source of connections* &mdash; connection string plus provider-specific
configuration plus pooling &mdash; which is conceptually exactly what FluentMigrator needs,
and it is what a provider hands you when the configuration cannot be expressed in a
connection string (`NpgsqlDataSourceBuilder`, `SqlClientFactory.CreateDataSource`). It also
exposes `CreateBatch()`, which is the natural hook if we ever want to reduce round-trips by
batching migration statements (`DbConnection.CreateBatch()` exists too on new targets, but
the data source is the object that knows whether batching is supported).

A future design that reifies it would look roughly like this:

1. Add `IMigrationDataSourceAccessor` (or `Func<IServiceProvider, DbDataSource>` bound
   through options) as the primary configuration surface for .NET 8+ assets.
2. Give `GenericProcessorBase` a `DbDataSource`-typed path: create the connection with
   `dataSource.CreateConnection()`, and later optionally create batches with
   `dataSource.CreateBatch()` for multi-statement expressions.
3. Provide a `ConnectionStringDataSource : DbDataSource` shim that wraps
   `DbProviderFactory` + connection string, so providers without a real `DbDataSource`
   implementation (and reflectively loaded providers) still work.
4. Keep `IMigrationConnectionFactory` as the lower-level escape hatch, since a data source
   cannot express "use *this* connection instance that I already own".
5. Keep the `net48`/`netstandard2.0` assets on the connection-factory path only.

This was **not** chosen now because:

* `DbDataSource` does not exist on the `net48` and `netstandard2.0` assets that
  FluentMigrator still ships, so it can never be the *only* abstraction; we would need both
  paths anyway.
* Most providers FluentMigrator supports still have no `DbDataSource` implementation, so
  the shim in step 3 would be doing the real work for the majority of providers, and a
  `DbDataSource` that only wraps a connection string adds a layer without adding power.
* The immediate user need (issue #1981) is "let me configure the connection", which the
  connection factory solves for every provider and every target framework today.
* `WithDataSource(...)` already gives users the idiomatic entry point, so nothing is lost
  by deferring; the internal representation can change later without changing that API.

The batching argument is the one that may eventually force the change. If FluentMigrator
wants to send migration statements as a `DbBatch` to cut round-trips, the data source is
where that capability should be discovered, and the processor pipeline
(`ProcessorBase.Process(...)`, `Execute.Sql`, statement splitting) would need to be taught
to group statements. That is a separate ADR.

## Consequences

* Positive: connection creation is configurable for every provider and every target
  framework, without a provider-specific API surface (#2002's limitation) and without
  exposing `DbProviderFactory` as the extension point.
* Positive: externally owned connections are expressible, which is a first step toward the
  unit-of-work scenarios described in `UnitOfWork.md`.
* Positive: preview/connectionless detection and version-loader selection are now driven by
  `HasConnection` instead of string inspection.
* Negative: two configuration entry points (`WithConnectionFactory` and `WithDataSource`)
  plus `WithGlobalConnectionString` means the documentation has to be explicit about
  precedence; the docs and the `in-process` runner guide now recommend `WithDataSource` for
  .NET 8+.
* Negative: `GenericProcessorBase` must tolerate connections that do not match its
  `DbProviderFactory`.
* Neutral: processor constructors taking `IConnectionStringAccessor` are obsolete but kept,
  so no consumer breaks.

## Credits

The design in this ADR is the result of the discussion on PR #2268 between
[@Sherif-Ahmed](https://github.com/Sherif-Ahmed), [@fubar-coder](https://github.com/fubar-coder)
and [@jzabroski](https://github.com/jzabroski), building on the earlier prototype in PR #2002
by [@Merlin-Taylor](https://github.com/Merlin-Taylor). The `WithDataSource(...)` adapter and
the analysis of `DbDataSource` versus `DbProviderFactory` come directly from that thread.
