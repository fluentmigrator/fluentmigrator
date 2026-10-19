# Async All The Way

* Status: proposed (replaces the placeholder that previously occupied this file)
* Related issue: [#854](https://github.com/fluentmigrator/fluentmigrator/issues/854) &mdash; "async/await all the way"
* Related ADRs:
  * [ConnectionManagement.md](ConnectionManagement.md) &mdash; `IMigrationConnectionFactory`, `DbDataSource`, the Firebird ownership limitation
  * [UnitOfWork.md](UnitOfWork.md) &mdash; ownership of the version-table transaction
  * [TransactionalDataDefinitionLanguage.md](TransactionalDataDefinitionLanguage.md) &mdash; which DDL can be rolled back
  * [DependencyInjection.md](DependencyInjection.md) &mdash; rows 9 (Journaling) and 13 (Connections)
* Code references are to `main` at `6e137b6` (2026-10-16). Driver references are to the driver
  versions pinned in `Directory.Packages.props` at that commit, unless a different version is named.

## Context and Problem Statement

FluentMigrator executes everything synchronously. Shipped `src/` contains no `async`, `await`,
`CancellationToken`, `Console.CancelKeyPress`, `ICancelableTask` or `IAsyncDisposable`. The only
matches are Roslyn plumbing in `FluentMigrator.Analyzers`.

The discussion on #854 comes down to two use cases that a sequential, single-connection migration
runner actually benefits from. Throughput is not one of them: FluentMigrator opens one connection
and never runs SQL in parallel, by design.

1. **Composing with async code.** `Execute.WithConnection` takes an
   `Action<IDbConnection, IDbTransaction>`. Anyone who wants to reuse an async data-access layer
   inside a migration has to block on it today. The same applies to applications that host the
   runner in-process (multi-tenant upgrade-on-demand, an upgrade wizard with a Cancel button, as
   described by @snakex64). There, `MigrateUp()` ties up a thread-pool thread for the whole run and
   cannot be composed with the application's own cancellation.
2. **Stopping a runaway migration.** Pressing Ctrl+C in `Migrate.exe`, `dotnet fm` or an MSBuild
   build today kills the process without cancelling the in-flight command. The server keeps running
   the statement, and possibly holding locks, until it finishes or someone runs `KILL <spid>` by
   hand. MSBuild cannot cancel the `Migrate` task at all because it does not implement
   `ICancelableTask` (MSBuild reports
   [MSB4220](https://learn.microsoft.com/visualstudio/msbuild/errors/msb4220) and waits).

The main result of the analysis below is that **these are two different features**. The second
does not need the first:

* Cancelling a running statement is `DbCommand.Cancel()`. It is designed to be called from a
  different thread than the one blocked in `ExecuteNonQuery()`. `Console.CancelKeyPress` handlers
  and `ICancelableTask.Cancel()` already run on a different thread. Ctrl+C support therefore works
  on the existing synchronous pipeline and with drivers that have no async support.
* Async/await adds non-blocking threads and the ability to compose with async user code. It does
  not add the ability to cancel; that comes from `CancellationToken` plumbing plus `Cancel()`.

The .NET base class library makes the same split. On .NET Framework, the default
`DbCommand.ExecuteNonQueryAsync(CancellationToken)` registers `Cancel` on the token and then calls
the synchronous `ExecuteNonQuery()`
([referencesource `DBCommand.cs` L208–226](https://github.com/microsoft/referencesource/blob/master/System.Data/System/Data/Common/DBCommand.cs)).
In other words, it is "a separate party that calls `DbCommand.Cancel()` when the token fires", as
@jzabroski described the requirement in #854.

This ADR does four things:

1. Audits every database driver FluentMigrator supports for async and cancellation support,
   including what is available to a .NET Framework 4.6.1 application.
2. Decides how FluentMigrator behaves with drivers that have no real async support.
3. Defines cancellation semantics for transactions and the journal (VersionInfo), and the test
   matrix that pins them down.
4. Defines what an *asynchronous migration constraint* means.

## Decision Drivers

* **No breaking changes for existing migrations.** `Migration.Up()`/`Down()` stay `void`.
  `IMigrationProcessor`, `IMigrationRunner` and `IVersionLoader` keep their members, because custom
  processors and runners exist in the wild.
* **Every supported asset.** The libraries ship `net48;netstandard2.0;net8.0`, and
  `FluentMigrator.Runner.SqlServer` ships `net48;net8.0;net9.0;net10.0`. Default interface methods
  are not available on `net48`/`netstandard2.0`, and neither are `DbTransaction.CommitAsync`,
  `DbConnection.BeginTransactionAsync`/`CloseAsync` or `DisposeAsync`.
* **Every supported provider**, including providers loaded reflectively through
  `ReflectionBasedDbFactory` that have no async support at all.
* **The journal must not lie.** After a cancellation, VersionInfo must describe exactly the
  migrations whose effects are durable. The only exception is a provider or migration that
  FluentMigrator cannot make atomic, and that exception must be reported loudly.
* **No sync-over-async and no async-over-sync inside the library.** No `GetAwaiter().GetResult()`
  on genuinely asynchronous work (deadlocks under a `SynchronizationContext`, thread-pool
  starvation), and no `Task.Run` to make synchronous drivers look asynchronous.
* **One implementation of the transaction and journal logic**, so that the sync and async paths
  cannot drift apart where correctness matters most.

## Driver audit

Method: the open-source drivers were audited from source at the tag FluentMigrator pins. For each
one we checked overrides of `ExecuteNonQueryAsync`, `ExecuteDbDataReaderAsync`,
`ExecuteScalarAsync`, `OpenAsync`, `CommitAsync`/`RollbackAsync`, and `Cancel()`, and whether the
async override performs real asynchronous I/O. The closed-source drivers (ODP.NET, IBM Db2, SAP
HANA) were audited from vendor documentation. Package metadata could not be inspected directly
because nuget.org was unreachable from the audit environment, so target frameworks come from the
drivers' project files. Rows marked *unverified* need a runtime probe (see
[Test plan](#test-plan)) before we rely on them.

**.NET Framework 4.6.1 baseline.** 4.6.1 reached end of support on 2022-04-26, and Microsoft's
guidance is to move to at least 4.6.2
([announcement](https://devblogs.microsoft.com/dotnet/net-framework-4-5-2-4-6-4-6-1-will-reach-end-of-support-on-april-26-2022/)).
FluentMigrator has no `net461` asset. A 4.6.1 application can only consume the `netstandard2.0`
assets, and that path is already unsupported today:

* `FluentMigrator.Runner.SqlServer` ships no `netstandard2.0` asset, so SQL Server cannot be used
  from 4.6.1 at all.
* The `Microsoft.Extensions.*` `[8.0.0,)` dependency floors ship a `buildTransitive` warning:
  "*doesn't support net461 and has not been tested with it. Consider upgrading your
  TargetFramework to net462 or later*" (dotnet/runtime `eng/packaging.targets`, the
  `NETStandardCompatError` item for `net461`).

The "4.6.1" column below therefore answers a narrower question: *if* someone runs FluentMigrator's
`netstandard2.0` asset on 4.6.1, which build of the driver can they get, and does that build do real
async?

| Provider | Package (FM pin) | Driver TFMs | What a 4.6.1 app gets | Real async I/O | `Cancel()` on an in-flight command | DDL transactional? |
| --- | --- | --- | --- | --- | --- | --- |
| SQL Server | `Microsoft.Data.SqlClient` 6.0.1 | `net462`, `net8.0`, `net9.0` (.NET Standard removed in 6.0) | No 6.x asset. MDS **4.x** was the last line with `net461` (dropped in 5.0, [#1574](https://github.com/dotnet/SqlClient/pull/1574)). FM's SqlServer runner can't be consumed anyway. | **Yes**, on netfx too (`SqlCommand`/`SqlConnection`/`SqlDataReader` override every async method) | **Yes** (TDS attention). The transaction is left **open** unless `XACT_ABORT ON`, so FM must roll back explicitly. | Mostly |
| PostgreSQL, Redshift | `Npgsql` 8.0.7 | `netstandard2.0`, `netstandard2.1`, `net6.0`–`net8.0` | `netstandard2.0` asset | **Yes** | **Yes** (CancelRequest). If the server does not answer within `Cancellation Timeout` (default 2000 ms), Npgsql **breaks the connection**. | Postgres: yes, with exceptions (`CREATE INDEX CONCURRENTLY`, ...). Redshift: some statements can't run in a transaction block. |
| MySQL | `MySql.Data` 9.3.0 | `net462`, `net48`, `netstandard2.0/2.1`, `net8.0`–`net10.0` | `netstandard2.0` asset | **Yes since 8.0.33** ("Re-implemented the asynchronous methods for the Classic protocol (WL15484)"). **Earlier versions' `*Async` methods ran synchronously**, which is what #854's 2018 table observed. | **Yes** (`KILL QUERY` from a second connection) | **No** (implicit commit) |
| MySQL | `MySqlConnector` 2.4.0 | `net462`, `net471`, `net48`, `netstandard2.0/2.1`, `net6.0`, `net8.0`, `net9.0` | `netstandard2.0` asset | **Yes** | **Yes** (`KILL QUERY`; `Cancellation Timeout` defaults to 2 s) | **No** |
| Firebird | `FirebirdSql.Data.FirebirdClient` 10.3.3 | `net48`, `netstandard2.0/2.1`, `net5.0`–`net8.0` | `netstandard2.0` asset | **Yes for the managed wire protocol** (since 8.0). The native/embedded client wraps synchronous P/Invoke (`isc_dsql_execute2`) inside its `*Async` methods. | **Yes** (`fb_cancel_operation`; server 2.5+) | Yes, but **FM's Firebird processor commits mid-migration** (`CommitRetaining`), so FM migrations are not atomic on Firebird. |
| Oracle (managed) | `Oracle.ManagedDataAccess` 23.8.0 | **`net472` only** (23.x) | **Nothing from 23.x.** Only ≤ 21.x, whose `*Async` methods are the `DbCommand` base implementation, i.e. synchronous. | **Yes since 23** ("Managed ODP.NET and ODP.NET Core support TAP starting with version 23", [docs](https://docs.oracle.com/en/database/oracle/oracle-database/26/odpnt/featAsyncPipelining.html)). No `CommitAsync`/`RollbackAsync`. | **Yes**, but not when pipelining is enabled | **No** (implicit commit) |
| Oracle (Core) | `Oracle.ManagedDataAccess.Core` 23.8.0 | `net8.0` | n/a | **Yes** (23+) | Yes | **No** |
| Oracle (unmanaged) | `Oracle.DataAccess.x86.4` 4.112.3 | netfx | x86 only | **No** | Yes (documented); *unverified in FM* | **No** |
| Oracle (dotConnect) | Devart `Devart.Data.Oracle` | commercial | *not audited* | *unverified* | *unverified* | **No** |
| Db2 LUW / iSeries | `Net.IBM.Data.Db2` 9.0.0.300 (.NET); `IBM.Data.DB2` (netfx, from the client install) | .NET (Core) / netfx | netfx client install | **No documented TAP support.** Treat as synchronous. | *unverified* | LUW: yes |
| SAP HANA | `Sap.Data.Hana.*` | per-TFM packages | `Sap.Data.Hana.v4.5` | **No documented TAP support.** The 2018 audit on #854 found `*Async` "partially" present but executing synchronously. | *unverified* | DDL auto-commits by default |
| SQLite | `Microsoft.Data.Sqlite` `[8.0.0,)` | `netstandard2.0`, `net6.0`, `net8.0` | `netstandard2.0` asset | **No**: "Async ADO.NET methods will execute synchronously" ([docs](https://learn.microsoft.com/dotnet/standard/data/sqlite/async)) | **No**: `SqliteCommand.Cancel()` is documented as "Does nothing". `sqlite3_interrupt` is reachable through `SqliteConnection.Handle`. | Yes |
| SQLite | `System.Data.SQLite` / `Mono.Data.Sqlite` | netfx / Mono | netfx | **No** (no async overrides) | *unverified* | Yes |
| Snowflake | `Snowflake.Data` 5.7.0 | `netstandard2.0`, `net481`, `net8.0`–`net10.0` | `netstandard2.0` asset | **Yes** (HTTP) | **Yes** (server-side cancel through `SFStatement.Cancel`) | **No** (DDL auto-commits) |

Three columns hold for every row:

* **Async transaction control on FM's `net48`/`netstandard2.0` assets: none.**
  `DbTransaction.CommitAsync`/`RollbackAsync` don't exist before `netstandard2.1`/.NET Core 3.0.
  Some drivers (MySqlConnector, Firebird) expose their own `CommitAsync` on those TFMs, but only as
  provider-specific methods. FM's `net8.0` asset gets the real virtuals.
* **`DbConnection.OpenAsync` on sync-only drivers** is the base implementation: it calls `Open()`
  and only honours a token that is already cancelled.
* **Fake async on .NET Framework leaks registrations.** When the base `ExecuteNonQueryAsync`
  succeeds, the netfx implementation never disposes its `CancellationTokenRegistration` (referencesource
  L213–224, disposed only in the `catch`). In .NET, `System.Data.Common` disposes it in a `finally`.
  A long migration on a sync-only driver would accumulate one registration per statement on the
  run's token. When the token fires, `Cancel()` would then be invoked on every command ever
  executed.

### What the audit means

1. Real async exists for SQL Server, PostgreSQL/Redshift, MySQL (with a version gate), Firebird
   (managed), Oracle 23+ and Snowflake. SQLite, Db2, HANA, unmanaged Oracle and pre-23 managed
   Oracle are synchronous, whatever their method names suggest.
2. On 4.6.1, the async story collapses further: no SQL Server runner and no ODP.NET with TAP.
   Supporting 4.6.1 is not a meaningful goal. **This ADR targets FM's existing assets and treats
   `netstandard2.0` on 4.6.1 as "must not break, not supported".**
3. In-flight cancellation exists for every audited driver except Microsoft.Data.Sqlite, which can
   be closed with `sqlite3_interrupt`. It works **regardless of async support**, so Ctrl+C can
   ship first, on the synchronous pipeline.
4. The rollback guarantees offered after a cancellation depend on transactional DDL, not on async.
   MySQL, Oracle, HANA and Snowflake can be left with a partially applied migration, exactly as
   they can after any exception today.

## Decision

### D1. Ship cancellation before async, on the existing synchronous pipeline

**Phase 1** delivers Ctrl+C support without adding a single async API.

* A new scoped service in Runner.Core, `IMigrationExecutionContext`, exposes
  `CancellationToken CancellationToken` (default `CancellationToken.None`). Hosts and
  `TaskExecutor.Execute(CancellationToken)` populate it.
  * It is scoped, not passed as a parameter, so the 40-odd `IMigrationProcessor` members and
    third-party processors keep their signatures.
  * The phase 2 async interfaces take explicit tokens. Phase 2 implementations bridge the explicit
    token into this context, so a token passed to `MigrateUpAsync` reaches the same checkpoints
    and command registrations described below.
* `GenericProcessorBase` gains one execution helper, used by every processor instead of calling
  `IDbCommand.ExecuteNonQuery/ExecuteReader/ExecuteScalar` directly. There are about 15 call sites
  (SqlServer `:280/:297/:304`, Postgres `:148/:160/:180`, ...). The helper:
  1. throws `OperationCanceledException` if the token is already cancelled,
  2. registers `command.Cancel()` on the token for the duration of the call, and **disposes the
     registration** (fixing the netfx leak described above),
  3. translates the provider's "cancelled" error into `OperationCanceledException`, using a small
     per-processor classifier (SQL Server error 0 with "Operation cancelled by user", Postgres
     `57014`, MySQL `1317`, Oracle `ORA-01013`, Firebird `335544794`, ...).
* `MigrationRunner` checks the token at **checkpoints**: before each maintenance stage, before each
  migration, and before each expression in `ExecuteExpressions`.
* Hosts (see [D6](#d6-hosts)) wire Ctrl+C and SIGTERM to the token.

This works for every provider and every asset. Two drivers only get the checkpoints:

* **Microsoft.Data.Sqlite**, whose `Cancel()` does nothing. The SQLite processor additionally
  registers `SQLitePCL.raw.sqlite3_interrupt(connection.Handle)`, which is documented as safe to
  call from another thread. `System.Data.SQLite` is used if its own `Cancel()` is verified to
  interrupt.
* **Custom `IMigrationProcessor` implementations** that don't derive from `GenericProcessorBase`.

### D2. Cancellation semantics: what is rolled back and what is journaled

The current algorithm (`MigrationRunner.ApplyMigrationUp`, `:553–583`) executes the migration's
expressions, inserts the VersionInfo row, and only then calls `scope.Complete()`. In transactional
mode, the schema change and its journal row commit together. The rules below keep that property
under cancellation and close the gaps that exist today.

* **R1. Cancellation is an exception, and nothing swallows it.** `OperationCanceledException` must
  propagate unchanged through all of these:
  * `ProcessorBase.ReThrowWithSql`, which today rewraps everything in `System.Exception`
  * `SnowflakeException` and SQLite `DbException` wrapping
  * `ExecuteExpressions` with `SilentlyFail`
  * the catch-alls in `SqlServerProcessor.TableExists`, `MySqlProcessor.Exists`,
    `SQLiteProcessor.Exists` and `VersionLoader`
  * `ValidateConnection`'s `UndeterminableConnectionException`

  Each of these gets a `catch (OperationCanceledException) { throw; }` or an exception filter.
* **R2. Cancellable until the last expression completes; after that, the migration finishes.**
  Once a migration's last expression has succeeded, writing the VersionInfo row and committing run
  with `CancellationToken.None`, bounded only by the command timeout. Cancellation is then observed
  at the next checkpoint. Without this rule:
  * a cancel during `Commit()` would leave the outcome unknowable, and
  * a cancel between the DDL and the VersionInfo insert under `TransactionBehavior.None` would leave
    an applied but unrecorded migration.
* **R3. Rollback after cancellation is not cancellable.** `RollbackTransaction` runs with
  `CancellationToken.None`. If it throws, the processor disposes the connection, and the server
  rolls back on disconnect. Both errors are reported (the original cancellation plus the rollback
  failure). This matters in practice:
  * Npgsql breaks the connection when the cancel request times out.
  * SQL Server leaves the transaction open after an attention signal.
* **R4. Transaction per session** (`TransactionPerSession = true`): a cancellation rolls back the
  whole session, including every VersionInfo row written in it. The journal and schema both return
  to their state before the session.
* **R5. Transaction per migration** (the default): migrations 1..N−1 are committed together with
  their VersionInfo rows, and migration N is rolled back. The journal records exactly the progress
  made. This is the "log the progress so far" requirement.
* **R6. No transaction** (`TransactionBehavior.None`, non-transactional DDL, Firebird
  `CommitRetaining`): FluentMigrator cannot make the interrupted migration atomic. It:
  * does **not** write the VersionInfo row for it (the same as after an exception today),
  * logs a warning such as `Migration 2026101901 (AddOrders) was cancelled after 3 of 7 expressions; the provider does not roll back DDL, so the database may be partially migrated.`, and
  * includes the version in the `MigrationCancelledException` it throws.
* **R7. Distinct outcome.** The runner throws `MigrationCancelledException : OperationCanceledException`,
  which carries the last committed version and the interrupted version, if any. Hosts map it to
  exit code **130** (128 + SIGINT) on every OS.

### D3. Async API: additive interfaces, explicit tokens

**Phase 2** adds async counterparts next to the existing interfaces. It does not replace them.

| New type | Assembly | Shape |
| --- | --- | --- |
| `IAsyncMigrationProcessor` | Abstractions | `ExecuteAsync(string sql, CancellationToken)`; `ReadAsync`/`ReadTableDataAsync` → `DataSet`; `ExistsAsync`; async counterparts of `IQuerySchema`'s seven `*Exists`; `BeginTransactionAsync`, `CommitTransactionAsync`, `RollbackTransactionAsync`; and a **single** `ProcessAsync(IMigrationExpression, CancellationToken)` entry point instead of 25 overloads. All return `ValueTask`/`ValueTask<T>`. |
| `IAsyncMigrationExpression` | Abstractions | `ValueTask ExecuteWithAsync(IAsyncMigrationProcessor, CancellationToken)`. `MigrationExpressionBase` implements it by calling `processor.ProcessAsync(this, ct)`. |
| `IAsyncMigration` | Abstractions | `ValueTask GetUpExpressionsAsync(IMigrationContext, CancellationToken)` and the `Down` equivalent. |
| `AsyncMigration` | FluentMigrator | Base class with `abstract Task UpAsync(CancellationToken)` / `DownAsync`, for migrations that must await at *collection* time, e.g. `await Schema.Table("x").ExistsAsync(ct)`. |
| `Execute.WithConnectionAsync(Func<DbConnection, DbTransaction, CancellationToken, Task>)` | FluentMigrator | Use case 1. Adds an `AsyncOperation` to `PerformDBOperationExpression`. |
| `IAsyncMigrationRunner` | Runner.Core | `MigrateUpAsync`, `MigrateDownAsync`, `RollbackAsync`, `RollbackToVersionAsync`, `ValidateVersionOrderAsync`, `ListMigrationsAsync`, `HasMigrationsToApply*Async`, all with a `CancellationToken`. |
| `IAsyncVersionLoader` | Runner.Core | `LoadVersionInfoAsync`, `UpdateVersionInfoAsync`, `DeleteVersionAsync`, `RemoveVersionTableAsync`. |
| `IAsyncMigrationScope : IMigrationScope, IAsyncDisposable` | Runner.Core | `CompleteAsync`, `CancelAsync`. Created by a factory method, because a constructor cannot await `BeginTransactionAsync`. |
| `TaskExecutor.ExecuteAsync(CancellationToken)` | Runner.Core | — |

`MigrationRunner`, `VersionLoader` and `GenericProcessorBase` implement both the sync and the async
interfaces.

Prerequisite refactors (**Phase 0**, which are behaviour-preserving and land first):

* Move I/O out of constructors and lazy getters:
  * `VersionLoader` calls `LoadVersionInfo()` from its constructor (`:88`).
  * `ConnectionlessVersionLoader` does the same (`:75`).
  * `TransactionalMigrationScope` begins a transaction in its constructor (`:37`).
  * `GenericProcessorBase` opens the connection inside a `Lazy` getter (`:92–93`).
* Change the event-driven batch parsers (SqlServer, SqlServer2000, SQLite and Snowflake run
  `ExecuteNonQuery` inside synchronous `SpecialToken` handlers) to **parse into a list, then
  execute**. Events cannot be awaited, and the `GO n` repeat loop needs a checkpoint per iteration.
* Type the processor's connection, transaction and command as `DbConnection`, `DbTransaction` and
  `DbCommand` internally. The public `IDbConnection`-typed properties stay. A factory that returns
  a non-`DbConnection` gets the synchronous path.

### D4. One implementation, two entry points (no sync-over-async)

The sync API is **not** implemented as `XxxAsync().GetAwaiter().GetResult()`. Instead, the runner,
version loader and processor share private core methods that take a `bool async` flag. This is the
pattern MySqlConnector uses: `ExecuteNonQuery() => ExecuteNonQueryAsync(IOBehavior.Synchronous, CancellationToken.None).Result`
(`MySqlCommand.cs:108`). MySql.Data uses the same pattern (`execAsync`).

* With `async: false`, every awaited operation is a synchronous ADO.NET call wrapped in a completed
  `ValueTask`. The method therefore never yields, and blocking on its result is safe under any
  `SynchronizationContext`. A `Debug.Assert(task.IsCompleted)` at each sync entry point, together
  with a test running under a context that throws on `Post` (see the test plan), enforces this.
* With `async: true`, the core calls `DbCommand.*Async(ct)`, except where [D5](#d5-drivers-without-async-support)
  says otherwise.

The one unavoidable exception is **user-supplied async code on the sync path**: an
`Execute.WithConnectionAsync` delegate or an `AsyncMigration` run through `MigrateUp()`. That work
is blocked on with `Task.Run(() => op(...)).GetAwaiter().GetResult()`, which escapes any captured
`SynchronizationContext`. This is documented, and analyzer FM0004 flags it.

### D5. Drivers without async support

Each processor declares a `ProcessorAsyncCapabilities` value (flags):

* `NativeAsyncIo` &mdash; the driver's `*Async` methods perform real asynchronous I/O.
* `CancelsInFlightCommand` &mdash; `DbCommand.Cancel()` interrupts a running statement.
* `TransactionalDdl` &mdash; used for R6 warnings and test expectations, not for execution.

Capabilities are **declared by the processor, not probed**. Reflection can show that a type
overrides `ExecuteNonQueryAsync`, but Microsoft.Data.Sqlite overrides `ExecuteDbDataReaderAsync`
and still runs synchronously. A Debug-level log line reports the probe result, as a diagnostic only.
Where the reflectively loaded assembly's version decides the answer, the processor checks it once
when the factory is created:

* MySql.Data < 8.0.33 is treated as synchronous.
* Oracle.ManagedDataAccess < 23 is treated as synchronous.
* The version source (assembly, file, or informational version) is pinned per driver by the
  capability tests.

Execution policy on the async path:

| Capability | What `ExecuteAsync` does |
| --- | --- |
| `NativeAsyncIo` | `await command.ExecuteNonQueryAsync(ct)`. The driver owns the token. |
| Sync-only, `CancelsInFlightCommand` | Calls the **synchronous** `ExecuteNonQuery()` inline, with FM's own disposed `ct.Register(command.Cancel)`, and returns a completed `ValueTask`. FM deliberately bypasses the base `*Async` so that the netfx registration leak can't happen and error translation is uniform. |
| Sync-only, no in-flight cancel (Microsoft.Data.Sqlite) | Same, plus `sqlite3_interrupt` registration ([D1](#d1-ship-cancellation-before-async-on-the-existing-synchronous-pipeline)). Otherwise checkpoint-only, which is documented. |
| Third-party `IMigrationProcessor` that does not implement `IAsyncMigrationProcessor` | Wrapped in `SyncMigrationProcessorAdapter : IAsyncMigrationProcessor`, which returns completed `ValueTask`s. Only checkpoint cancellation applies, because the adapter cannot reach the commands. |
| Transactions on `net48`/`netstandard2.0` | `Commit()`/`Rollback()` called synchronously, which is acceptable because they are short. `#if NETCOREAPP3_0_OR_GREATER` uses `CommitAsync(CancellationToken.None)` (R2/R3). |

The following was considered and **rejected**: offloading sync-only drivers with `Task.Run`, by
default or behind an option.

* It does not make the work asynchronous. It moves the blocking to another pool thread, which is
  precisely the pool-starvation problem in-process hosts complain about.
* It isn't needed for Ctrl+C, because the cancel handler already runs on its own thread.
* A UI host that wants responsiveness can write `await Task.Run(() => runner.MigrateUpAsync(ct))`
  itself. That is the host's decision to make, not the library's.

### D6. Hosts

* **FluentMigrator.Console**
  * `Main` becomes `async Task<int>`.
  * It handles `Console.CancelKeyPress`. On the first press it sets `e.Cancel = true` and cancels
    the token. On the second press it lets the process die; the server rolls back on disconnect.
  * On `net8.0` it also uses `PosixSignalRegistration` for `SIGTERM`, which is what containers and
    Kubernetes send.
  * `StopOnErrorLoggerProvider` must not block on `Console.ReadLine()` once cancellation has been
    requested.
* **FluentMigrator.DotNet.Cli**
  * Move to `CommandLineApplication.ExecuteAsync<Root>` and `OnExecuteAsync(CancellationToken)`.
  * Note that McMaster 4.1.1's own handler cancels the token but **does not set
    `e.Cancel = true`** (`CommandLineApplication.cs:873–881`), so the process would still be
    killed. dotnet-fm must install its own handler that does.
* **FluentMigrator.MSBuild**
  * Implement `ICancelableTask`. `Cancel()` cancels a `CancellationTokenSource`, and `Execute()`
    passes the token to `TaskExecutor.Execute(CancellationToken)` on the sync path. MSBuild tasks
    are synchronous, so this needs no async at all.
  * Under `AppDomainIsolatedTask`, `Cancel()` arrives cross-AppDomain, which works because the task
    is a `MarshalByRefObject`.
* All hosts map `MigrationCancelledException` to exit code 130 and log the last committed version.

### D7. Asynchronous migration constraints

**How constraints work today.** These details decide what "async" can mean.

* `MigrationConstraintAttribute.ShouldRun(MigrationConstraintContext)` is a synchronous predicate
  (`Constraints/MigrationConstraintAttribute.cs:42`).
* The context exposes only `RunnerOptions` and the in-memory `IVersionInfo`. There is **no
  processor, connection or service provider**, so a constraint cannot query the database today
  except through ambient state.
* Constraints are evaluated in `IsMigrationStepNeededForUpMigration` (`MigrationRunner.cs:397`).
  The caller, `GetUpMigrationsToApply`, is a **deferred LINQ query**, so each constraint runs
  *just in time*: as `foreach` advances, after migrations 1..N−1 have executed and before migration
  N's `BeforeEach` maintenance.
* The `VersionInfo` they see is the snapshot reloaded after `BeforeAll` (`:355`). It is not updated
  in memory as migrations apply.
* Constraints apply to up-migrations only. `HasMigrationsToApplyUp()` without a version bypasses
  them (`:490`).

**Decision.** An async constraint is a *gate*: the runner awaits a definite answer before it
starts anything belonging to that migration.

1. **API.** `AsyncMigrationConstraintAttribute` declares
   `ValueTask<bool> ShouldRunAsync(AsyncMigrationConstraintContext context, CancellationToken ct)`.
   The context adds read-only database access (`IAsyncQuerySchema`, plus `ReadAsync`/`ExistsAsync`
   on the runner's processor) and the scoped `IServiceProvider`. Existing sync constraints run
   unchanged through an adapter.
2. **Just-in-time, sequential, exactly once.**
   * The async runner keeps today's interleaving by evaluating constraint N immediately before
     migration N, in an explicit loop. Materializing the list up front would silently change
     behaviour.
   * Constraints are never evaluated concurrently. There is one connection and no MARS.
   * Each migration's result is computed once per run and logged
     (`Migration 2026101901 skipped: constraint RequiresFeatureFlag returned false`).
3. **Same connection, same transaction.**
   * Under a session transaction, the constraint sees the uncommitted effects of earlier migrations
     in the run, which is what the migration itself will see.
   * Under transaction-per-migration, it runs between transactions.
   * Opening a *second* connection inside a constraint is unsupported, and the documentation says
     so. On SQL Server it can block on locks held by the runner's own session transaction and
     self-deadlock.
4. **Three outcomes, never conflated.**
   * `true`: the migration runs.
   * `false`: the migration is skipped.
   * An exception fails the run. A constraint error is *not* treated as `false`, unlike the
     catch-alls in the `*Exists` helpers.
   * `OperationCanceledException` cancels the run under D2. It is never a skip.
5. **Bounded.** The constraint receives the run's token. Its queries go through the processor
   helper, so they get the command timeout and in-flight cancellation.
6. **Preview and connectionless mode.** `ConnectionlessProcessor` cannot read. A constraint that
   declares `RequiresConnection` makes `--preview` fail fast and name the constraint, unless the
   user passes an explicit assumption (see Open questions). It does not silently include or exclude
   the migration from the script handed to a DBA.
7. **Consistency.** `HasMigrationsToApplyUp[Async]()` without a version will evaluate constraints
   like the versioned overload does. With async constraints, that can touch the database.

### Answers to the placeholder's questions

1. **`IAsyncDisposable` placement.** Put it on the same class as `IDisposable`, with no partial-class
   split. Use `#if` only where a BCL API is missing (`DbConnection.DisposeAsync` before
   netstandard2.1). The type itself comes from `Microsoft.Bcl.AsyncInterfaces` on
   `net48`/`netstandard2.0`. The `Microsoft.Extensions.*` 8.x floors very likely bring it in
   already; add an explicit `PackageVersion` at `8.0.0` so that the dependency is deliberate.
2. **`netstandard2.1`?** No. `await using` is a language feature (C# 8, and `LangVersion` is
   `latest`) that needs only the `IAsyncDisposable` type. The `net8.0` asset already gets the real
   `CommitAsync`/`DisposeAsync`. `netstandard2.1` would only serve .NET Core 3.x–7, all of which
   are out of support.
3. **Which drivers support async?** See the [Driver audit](#driver-audit). Publish it on the docs
   site and keep it current through the capability tests.
4. **`Task.CompletedTask` for sync-only drivers?** Yes, in the refined form of
   [D5](#d5-drivers-without-async-support): completed `ValueTask`s from inline synchronous calls,
   *with* FM-owned cancellation registration. Not `Task.Run`.

## Test plan

These are new requirements. None of the following is covered today: the existing runner tests
assert `WasCommitted`, rollback-on-exception and VersionInfo deletion, but nothing involving
cancellation.

**Unit tests** (`FluentMigrator.Tests/Unit`, NUnit and Moq, with no database). A fake processor
records Begin/Commit/Rollback, `UpdateVersionInfo` and per-command `Cancel()` calls, and can block
inside an expression until signalled.

| # | Scenario | Assertion |
| --- | --- | --- |
| U1 | Token cancelled before `MigrateUp` | No `BeginTransaction`, no VersionInfo write, `MigrationCancelledException` |
| U2 | Cancel during migration N, transaction per migration | 1..N−1 committed with their VersionInfo rows; N rolled back with no row; nothing after N starts (R5) |
| U3 | Cancel during migration N, `TransactionPerSession` | Exactly one rollback, zero commits, no VersionInfo rows persisted (R4) |
| U4 | Cancel after N's last expression, before commit | N commits with its row; cancellation surfaces before N+1 (R2) |
| U5 | Cancel while `Commit()` is blocking | Commit is not interrupted; outcome is "committed" (R2) |
| U6 | Cancel inside a `TransactionBehavior.None` migration | No row for N; R6 warning logged with the version |
| U7 | Rollback throws after cancel | Connection disposed; both errors reported; exit code 130 (R3) |
| U8 | `SilentlyFail = true` plus cancel | Cancellation still propagates (R1) |
| U9 | Cancel during `TableExists`/`Exists`/`ValidateConnection` | `OperationCanceledException`, not `false` or `UndeterminableConnectionException` (R1) |
| U10 | 10,000 statements, then cancel | `Cancel()` invoked at most on the in-flight command (no registration leak) |
| U11 | Sync API run under a `SynchronizationContext` whose `Post` throws | Never throws, which proves the sync path never yields (D4) |
| U12 | Async constraint returns false / throws / is cancelled | Skip and log / fail / cancel. Never conflated (D7.4) |
| U13 | Constraint evaluation order | Constraint N observes migration N−1's effects; evaluated once; never concurrently (D7.2). **Write this test against the current sync runner first**, to pin today's deferred-LINQ behaviour before refactoring. |
| U14 | `HasMigrationsToApplyUp()` with a false constraint | Consistent with the versioned overload (D7.7) |
| U15 | Cancel during the `BeforeAll` maintenance stage | No migrations attempted; maintenance scope rolled back |

**Integration tests** (Testcontainers: SQL Server, PostgreSQL, MySQL with both drivers, Oracle,
Firebird, Db2; file-based SQLite). Each provider runs a migration containing a long statement and
the test cancels it after one second.

* Long statements: `WAITFOR DELAY '00:01:00'`, `SELECT pg_sleep(60)`, `SELECT SLEEP(60)`,
  `DBMS_SESSION.SLEEP(60)`; for Firebird, Db2 and SQLite, a recursive CTE large enough to run for
  a minute.
* Assertions:
  * **I1.** The call returns within `Cancellation Timeout` plus a margin, with `MigrationCancelledException`.
  * **I2.** No server-side request survives. Check `sys.dm_exec_requests`, `pg_stat_activity`,
    `information_schema.processlist`, `v$session`, `MON$STATEMENTS`, or the Db2 equivalent.
  * **I3.** On transactional-DDL providers, the cancelled migration's objects don't exist and its
    VersionInfo row is absent.
  * **I4.** On MySQL and Oracle, objects created before the cancelled statement *do* exist, the row
    is absent, and the R6 warning was logged. This pins the documented limitation.
  * **I5.** In transaction-per-migration mode, the rows for the earlier migrations are present and
    their objects exist.
  * **I6.** The connection is still usable, or was disposed, never left half-open in the pool.
* **Capability probes** per driver: does `ExecuteNonQueryAsync` return an incomplete task for
  `SELECT pg_sleep(1)` (`NativeAsyncIo`)? Does `Cancel()` interrupt (`CancelsInFlightCommand`)?
  These turn the *unverified* cells in the audit into facts and fail if a driver upgrade changes
  behaviour.
* **GH2220 extension**: cancel a runner while it waits on `sp_getapplock` in `BeforeAll`. The lock
  wait ends, no migrations run, and the other runner proceeds.

**Host tests**

* Start `Migrate` and `dotnet-fm` as child processes and send `SIGINT` (Linux CI) after the first
  migration commits. Assert exit code 130, the logged last-committed version, and the I2 check.
* Send a second `SIGINT` and assert the process exits immediately.
* MSBuild: call `ICancelableTask.Cancel()` on a running `Migrate` task. `Execute()` must return
  `false` promptly and log the cancellation.

## Considered Options

* **Implement the sync API as sync-over-async of a single async implementation.** Rejected. This
  deadlocks under WinForms, WPF and classic ASP.NET `SynchronizationContext`s, which run migrations
  at startup, and it consumes extra threads.
* **Async-over-sync with `Task.Run` around today's code.** Rejected. It is not asynchronous, and it
  makes pool starvation worse ([D5](#d5-drivers-without-async-support)).
* **Two hand-written code paths.** Rejected. The transaction and journal logic (D2) would be
  duplicated, and the two copies would diverge exactly where correctness matters.
* **Default interface methods on the existing interfaces.** Not available on `net48`/`netstandard2.0`.
* **Making async the only API in a major version.** Deferred. Revisit once the async path has
  shipped and the analyzers have moved users over. Breaking every custom processor buys nothing
  that the adapters don't already give.
* **Ship async first, cancellation second**, as originally suggested on #854. Inverted. Cancellation
  delivers the most-requested behaviour with no public API break and works on every driver. The
  async work then reuses its token plumbing, R1–R7 and the test suite.

## Consequences

* Positive: Ctrl+C, SIGTERM and MSBuild cancellation in phase 1, for every provider and asset, with
  no API break.
* Positive: the journal invariants under cancellation (R2–R6) are written down and tested. R2 also
  closes an existing window under `TransactionBehavior.None` between the DDL and the VersionInfo
  insert.
* Positive: `Execute.WithConnectionAsync` and `IAsyncMigrationRunner` let in-process hosts stop
  blocking pool threads on drivers with real async.
* Negative: the public surface roughly doubles (sync plus async interfaces), and the documentation
  has to explain which drivers are "async in name only".
* Negative: phase 0 touches every processor (the execution helper, parse-then-execute batch
  parsers). The unit-test mocks that set up `cmd.ExecuteNonQuery()` (`ProcessorBatchParserTestsBase`)
  have to change.
* Negative: on MySQL, Oracle, HANA and Snowflake, cancellation can leave a partially applied
  migration. This is no worse than an exception today, but users will now trigger it on purpose
  with Ctrl+C.
* Neutral: Firebird's mid-migration `CommitRetaining` already makes migrations non-atomic. The ADR
  documents it and does not change it.

## Analyzers

* **FM0004** (Warning): sync-over-async inside `Up()`/`Down()`, `WithConnection` lambdas or
  constraint predicates (`.Result`, `.Wait()`, `.GetAwaiter().GetResult()`). The code fix
  converts the code to `AsyncMigration`/`WithConnectionAsync`.
* **FM0005** (Info): `Execute.WithConnection` whose lambda calls `*Async` ADO.NET methods. Suggests
  `WithConnectionAsync`.
* Both build on `FluentMigratorAnalyzer` and `FluentMigratorContext`, adding a `MigrationBase`
  symbol to `Constants`.

## Open questions

1. **Preview with DB-reading constraints.** Should preview fail (as proposed in D7.6), or should
   `--assume-constraints=true|false` let the DBA hand-off workflow proceed with an explicit,
   printed assumption?
2. **Exit code on Windows.** 130 is the POSIX convention. A Ctrl+C-killed process on Windows exits
   with `0xC000013A`. Should Windows use 130 too, for script portability?
3. **Down and Rollback constraints.** Constraints don't apply to down-migrations today. Should
   async constraints change that, or keep parity?
4. **Firebird.** Should `CancelsInFlightCommand` force `FirebirdTransactionModel.None` semantics
   for the cancelled migration, or should the R6 warning stay the only treatment?

## References

* Driver sources (tags pinned by FM):
  * [dotnet/SqlClient v6.0.1](https://github.com/dotnet/SqlClient/tree/v6.0.1)
  * [npgsql/npgsql v8.0.7](https://github.com/npgsql/npgsql/tree/v8.0.7)
  * [mysql-net/MySqlConnector 2.4.0](https://github.com/mysql-net/MySqlConnector/tree/2.4.0)
  * [mysql/mysql-connector-net 9.3.0](https://github.com/mysql/mysql-connector-net/tree/9.3.0) (`CHANGES`, 8.0.33 entry)
  * [FirebirdSQL/NETProvider FirebirdClient-10.3.3.0](https://github.com/FirebirdSQL/NETProvider/tree/FirebirdClient-10.3.3.0)
  * [snowflakedb/snowflake-connector-net v5.7.0](https://github.com/snowflakedb/snowflake-connector-net/tree/v5.7.0)
  * [dotnet/efcore v8.0.0 `Microsoft.Data.Sqlite.Core`](https://github.com/dotnet/efcore/tree/v8.0.0/src/Microsoft.Data.Sqlite.Core)
* `DbCommand` base behaviour:
  [.NET Framework reference source](https://github.com/microsoft/referencesource/blob/master/System.Data/System/Data/Common/DBCommand.cs) and
  [dotnet/runtime `DbCommand.cs`](https://github.com/dotnet/runtime/blob/main/src/libraries/System.Data.Common/src/System/Data/Common/DbCommand.cs)
* [ODP.NET: Asynchronous Programming and Pipelining](https://docs.oracle.com/en/database/oracle/oracle-database/26/odpnt/featAsyncPipelining.html);
  [Oracle.ManagedDataAccess on NuGet](https://www.nuget.org/packages/Oracle.ManagedDataAccess/23.6.0) (net472)
* [Microsoft.Data.Sqlite async limitations](https://learn.microsoft.com/dotnet/standard/data/sqlite/async)
* [SqlClient 5.0.0 release notes: dropped .NET Framework 4.6.1](https://github.com/dotnet/SqlClient/blob/main/release-notes/5.0/5.0.0.md);
  [6.0.0: removed .NET Standard](https://github.com/dotnet/SqlClient/blob/main/release-notes/6.0/6.0.0.md)
* [.NET Framework 4.5.2, 4.6, 4.6.1 end of support](https://devblogs.microsoft.com/dotnet/net-framework-4-5-2-4-6-4-6-1-will-reach-end-of-support-on-april-26-2022/)
* [MSB4220 / `ICancelableTask`](https://learn.microsoft.com/visualstudio/msbuild/errors/msb4220)
* [natemcmaster/CommandLineUtils v4.1.1 `CommandLineApplication.ExecuteAsync`](https://github.com/natemcmaster/CommandLineUtils/blob/v4.1.1/src/CommandLineUtils/CommandLineApplication.cs)
* [Dan Guzman: query timeouts leave transactions open without `XACT_ABORT`](https://weblogs.sqlteam.com/dang/2007/10/20/use-caution-with-explicit-transactions-in-stored-procedures)

## Credits

The use cases come from the #854 discussion: [@jzabroski](https://github.com/jzabroski) (use cases
1 and 2, and the `DbCommand.Cancel()` requirement), [@snakex64](https://github.com/snakex64)
(multi-tenant and upgrade-UI scenarios), [@fairking](https://github.com/fairking),
[@eaglestorm](https://github.com/eaglestorm), [@PhenX](https://github.com/PhenX) and
[@Pretasoc](https://github.com/Pretasoc). The 2018 provider table in the issue is the baseline that
the [Driver audit](#driver-audit) updates.
