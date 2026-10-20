# Parallel Execution of Migrations

* Status: proposed, **exploratory**. This is a steelman: it makes the strongest case for parallel
  execution and states the conditions under which that case is sound. It is not a commitment to
  build it.
* Depends on: [AsyncAllTheWay.md](AsyncAllTheWay.md) phases 0–2. That means explicit
  `CancellationToken`s, `IAsyncMigrationProcessor`, the D2 cancellation and journal rules, and
  I/O moved out of constructors.
* Motivating workloads:
  * **Seeding.** Bulk data loads through Profiles and Maintenance migrations.
  * **[#1221](https://github.com/fluentmigrator/fluentmigrator/issues/1221),
    `With.Databases` / `With.Tables`.** A fluent replacement for `sp_MSforeachdb` and
    `sp_MSforeachtable`.
* Code references are to `main` at `6e137b6`.

## 1. Context and Problem Statement

[AsyncAllTheWay.md](AsyncAllTheWay.md) deliberately keeps FluentMigrator on one connection and one
logical thread of control, and defines rollback and journal guarantees (its rules R1–R7) for that
model. Two families of work would benefit from running more than one thing at a time:

1. **Seeding.** Loading reference and test data into many independent tables. With
   `SqlBulkCopy`/`COPY`, this is I/O-bound per table and parallelises well across connections,
   provided that tables are loaded after the tables they reference.
2. **Fan-out (#1221).** `With.Databases("Tenant_*")` applies the same operation to N databases.
   Each database is a natural unit of isolation, with its own connection, transaction and failure,
   and the work is embarrassingly parallel.

The proposal under evaluation is:

* `MigrationAttribute` gains `ExecutionMode { Sequential, Parallel }`, defaulting to `Sequential`.
* `ExecutionDepth`, defaulting to 1, states how parallel tasks compose into a DAG.
* `MigrationContext` knows which *parallel scope* it belongs to.

The question is whether this can be a **sustainable foundation**. That means a precise correctness
criterion, a schedule model with known properties, and an honest account of the concurrency hazards
it introduces, in particular through services that users inject from their own DI containers.

## 2. Correctness Criterion

Everything below follows from one definition. If it is weakened, the rest does not hold.

> **Serial equivalence.** Let *L* be the *canonical linearization* of a run: the order in which
> FluentMigrator runs tasks today. That is version order for migrations, and stage then type name
> for maintenance migrations (`MaintenanceLoader.cs`, `OrderBy(m => m.GetType().Name)`). A parallel
> run is correct if its effect on every database, **and on the journal**, equals the effect of
> executing *L* serially.

This is conflict-serializability (Papadimitriou 1979; Bernstein, Hadzilacos & Goodman 1987), except
that the serial order is fixed in advance rather than chosen by the scheduler. Two consequences
follow.

**C1. `Parallel` is a permission, not a command.** The runner may always execute a `Parallel` task
sequentially, for example when the degree of parallelism (DOP) is 1, when the provider cannot run
concurrent sessions, or when footprints conflict (§3.2). An author who marks two tasks `Parallel`
therefore asserts that **every** interleaving of them is equivalent to *L*. Under DOP = 1, the
schedule *is* *L*. That is the backward-compatibility argument, and it holds by construction rather
than by testing.

**C2. The assertion is commutativity.** Two tasks may share a layer only if they commute (Weihl
1988). For opaque SQL, the usable sufficient condition is **Bernstein's conditions** (Bernstein
1966). With read set *R* and write set *W*, tasks *T₁* and *T₂* may run in parallel if:

```text
W(T1) ∩ W(T2) = ∅   and   R(T1) ∩ W(T2) = ∅   and   W(T1) ∩ R(T2) = ∅
```

"Sets" here are schema objects (tables, sequences, schemas, databases), plus the journal table as a
distinguished object.

FluentMigrator has an advantage most migration tools don't: `Up()` *collects a declarative
expression list* before anything executes. For `Create.Table`, `Insert.IntoTable`,
`Alter.Column`, `Create.ForeignKey` and similar, a **syntactic footprint** can be computed from the
expressions rather than declared. `Execute.Sql`, `Execute.Script`,
`Execute.WithConnection[Async]` and custom `IMigrationExpression`s are opaque. Their footprint is
⊤ ("touches everything") unless it is declared or extracted (§2.1).

> **Alpha prototype: T-SQL via ScriptDOM.** The first extractor targets `Execute.Sql` and
> `Execute.Script`/`EmbeddedScript` (the latter supplied as `AdditionalFiles`) whose dialect
> resolves to a `SqlServer*` processor id. It parses them with
> [`Microsoft.SqlServer.TransactSql.ScriptDom`](https://github.com/microsoft/SqlScriptDOM), using
> the version-specific parser that matches the id (for example `SqlServer2016` → `TSql130Parser`).
> A `TSqlFragmentVisitor` collects the target objects of DDL and DML statements, plus the
> `FROM`/`JOIN` sources as reads.
>
> Everything outside the prototype's coverage stays ⊤:
> * `EXEC`/`sp_executesql` and other dynamic SQL;
> * routine calls;
> * any statement type the visitor does not handle;
> * every non-T-SQL dialect;
> * `WithConnection` bodies and custom expressions.
>
> The prototype counts as successful only if it is **never less conservative** than ⊤. That is
> measured by differential tests (V1) on a corpus of real migrations, and by the share of
> statements it resolves to a non-⊤ footprint.
>
> *Licensing:* ScriptDOM is MIT-licensed. It is compatible with FluentMigrator's Apache-2.0 license
> and listed as Category A by the ASF. Analyzer packages must bundle their dependencies, so the
> `FluentMigrator.Analyzers` package would redistribute the ScriptDOM binary and must carry its
> copyright and permission notice, for example in a `THIRD-PARTY-NOTICES.txt` included in the
> `.nupkg`.

A syntactic footprint is an **under-approximation**, even for builder expressions.
`Insert.IntoTable("Orders")` writes whatever the triggers on `Orders` write. Bernstein's conditions
need an *over*-approximation: the check is sound only if it never misses a write. The syntactic
footprint is therefore the input to a run-time closure (§2.1), not the answer.

### 2.1 Footprint extraction and closure

Computing a footprint has two stages. A task's footprint is usable only when both succeed;
otherwise it is ⊤, and the runner declines parallelism for that task (C1).

1. **Compile time: syntactic footprint.** An incremental source generator, paired with an
   analyzer, parses **compile-time-constant** SQL and emits per-migration footprint metadata,
   keyed by processor id. Analyzers cannot add to a compilation, which is why the metadata comes
   from a generator.
   * **Scope.** This covers SQL passed to `[StringSyntax("sql")]` parameters and constants
     annotated with them. `Execute.Script` and embedded scripts are covered only when supplied as
     `AdditionalFiles`.
   * **Run-time values are opaque.** Run-time strings, `$(token)`/`$[token]` substitutions and
     `WithConnection` bodies are ⊤.
   * **Unrecognized statements are opaque.** The parser recognizes the statement forms it
     understands and maps every other statement to ⊤. This is an island grammar (Moonen 2001), and
     the fallback is what keeps it sound.
   * **Dialect resolution.** It extends the existing `SqlDialectResolver` chain, most specific
     first:
     1. `IfDatabase(...)` on the statement;
     2. a `[StringSyntax("sql", "<ProcessorId>")]` argument on a user-declared string;
     3. the nearest `[FluentMigratorSql("<ProcessorId>")]` on a method, class or assembly. This
        attribute is needed because `StringSyntaxAttribute` cannot target a class, method or
        assembly;
     4. the `fluent_migrator_sql_dialect` key in `.editorconfig`.

     Dialects are named by **`ProcessorId` constants** (`SqlServer2016`, `PostgreSQL15_0`,
     `MySql8`), which already encode versions. That allows version diagnostics such as "`MERGE`
     requires `PostgreSQL15_0`".
   * **Name comparison is conservative.** Default schema and `search_path` are resolved at run
     time. Case folding of unquoted identifiers differs by dialect (Postgres lowers; Oracle,
     Firebird and Db2 upper; SQL Server follows collation; MySQL follows `lower_case_table_names`).
     Two names that *might* denote the same object are treated as the same object.
   * **Parsers.**
     * For T-SQL, `Microsoft.SqlServer.TransactSql.ScriptDom` (MIT, netstandard2.0, no dependencies)
       has a parser per SQL Server version.
     * For Postgres, the authoritative parser (libpg_query) is native, so it cannot run inside an
       analyzer.
     * Every other dialect starts as ⊤ until a parser demonstrates its coverage.
2. **Run time: catalog closure.** Before a region starts, the coordinator closes each task's write
   set over the hidden writers recorded in the target database's catalog:
   * triggers (`sys.triggers`, `pg_trigger`, `information_schema.TRIGGERS`, `ALL_TRIGGERS`,
     `RDB$TRIGGERS`);
   * cascading foreign keys;
   * indexed or materialized views;
   * routine dependencies (`sys.sql_expression_dependencies`, `pg_depend`);
   * synonyms.

   It iterates to a fixed point. Dynamic SQL, routine calls that cannot be resolved, and anything
   else unresolved yields ⊤. The closure is necessary because hidden writers live in the *target
   database*, not the source code, and they can differ between environments.

   Seeding can **shrink** the closure instead of computing it, by guaranteeing that triggers do not
   *execute* during the load (§4.7).

## 3. Schedule Model

### 3.1 Restrict to series-parallel graphs

An arbitrary DAG (`DependsOn = typeof(X)`) is the most expressive option. It is also the wrong
foundation. The schedule should be restricted to **series-parallel (SP) graphs**, which are built
only by series composition (A then B) and parallel composition (A alongside B, then join)
(Valdes, Tarjan & Lawler 1982). SP graphs are what **structured concurrency** produces (Smith 2018;
Java JEP 453), and they come with properties that arbitrary DAGs do not have:

| Property | SP / fork-join | Arbitrary DAG |
| --- | --- | --- |
| Every task has one enclosing scope that outlives it | Yes. Failure, cancellation and errors propagate up a tree. | No. A task can be "owned" by several predecessors. |
| Determinacy-race detection | Efficient (SP-bags; Feng & Leiserson 1997) | Needs general happens-before analysis |
| Performance bound with *P* lanes | Greedy schedule: *T_P ≤ T₁/P + T_∞* (Brent 1974; Blumofe & Leiserson 1999) | Same bound, but optimal makespan is NP-hard in general (Ullman 1975) |
| Explaining it in a code review | "These run together, then those" | Requires drawing the graph |
| Failure semantics | Well-defined: the scope fails and its siblings are cancelled | Ambiguous: which successors still run? |

### 3.2 `ExecutionDepth` produces SP graphs by construction

The proposed `ExecutionDepth` is a **layering**:

* tasks with the same depth run concurrently,
* there is a barrier between depth *d* and *d+1*,
* each layer completes before the next starts.

This is bulk-synchronous parallelism (Valiant 1990): a *series* of *parallel* layers, which is SP.
Nested fan-out inside a task (§4.3) is a parallel composition nested inside a parallel node, which
is still SP.

The proposal is stronger than it may first appear, for three reasons.

1. **For seeding, depth is foreign-key depth, under two preconditions.** Ordering seed loads by
   *longest path from a root in the FK graph* is the standard way to load a schema with referential
   integrity enforced. `Countries` has depth 1, `Customers → Countries` depth 2,
   `Orders → Customers` depth 3. Authors already think in these terms. FluentMigrator could even
   *suggest* the depth from `Create.ForeignKey` expressions. FK depth is the right layering only
   when:
   * **(a) no hidden writer executes during the load.** A trigger on `Customers` that writes
     `CustomerStats`, or reads `Orders`, adds edges that the FK graph does not show, and
     the §2.1 footprint closure must then add them. §4.7 describes how seeding can *guarantee* this
     precondition instead of analysing around it.
   * **(b) FK constraints are enforced during the load.** If enforcement is suspended, ordering is
     not needed for the inserts to succeed, but integrity is no longer *checked*, and a post-load
     validation becomes mandatory (§4.7).

   Without (a), FK depth is necessary but not sufficient.
2. **Layers turn journal and failure reasoning into per-layer reasoning.** At every barrier, all
   lanes have committed or rolled back. The "frontier" of progress is a layer index, not an
   arbitrary antichain of a DAG.
3. **Its weakness is bounded.** Layering a DAG adds *false dependencies*: a task waits for unrelated
   tasks in the previous layer. That costs makespan, not correctness. It is also where Graham's
   timing anomalies live (Graham 1969: adding lanes can make a run *slower*). For the two motivating
   workloads, the regions are one layer deep (fan-out) or a few layers deep (seeding), so the loss
   is small.

**Defining a parallel region.** `ExecutionDepth` needs a precise scope; otherwise depth 2 in one
stage could be confused with depth 2 in another.

* A **parallel region** is a maximal run of consecutive tasks in *L* with
  `ExecutionMode = Parallel` *within the same stage* (`BeforeAll`, `BeforeEach`, the versioned
  migrations, `BeforeProfiles`, profiles, `AfterEach`, `AfterAll`).
* A `Sequential` task is a full barrier and ends the region.
* Depth is interpreted relative to its region. The scheduler runs depth 1, then a barrier, then
  depth 2, and so on, with at most DOP lanes active.

**Consequence: the schedule depends on the database's state.** The region is computed over tasks
*still to be applied*. Production (applied up to v3) and development (applied up to v5) can
therefore get different regions from the same assembly. Each schedule is still serially equivalent
to its own *L*, so correctness holds. Logs and previews must print the computed regions so that the
difference is visible.

**Load-time validation.** The runner rejects a region if two same-depth tasks have intersecting
footprints (C2) or an opaque, undeclared footprint, unless the author has explicitly acknowledged
the opaque footprint (`[Footprint(Opaque = true)]`). It also rejects a region in which depth isn't
monotone with *L*: a task at depth 1 must not come after a task at depth 2 in version order. If it
did, the serial order and the layer order would disagree, and C1 would fail at DOP = 1.

### 3.3 Alternatives considered for the composition model

| Option | Strongest case for | Why not as the foundation |
| --- | --- | --- |
| `ExecutionDepth` (proposed) | SP by construction; maps to FK depth; per-layer journal frontier | False dependencies. Acceptable for both workloads. |
| `DependsOn = typeof(X)` edges | Precise; no false dependencies | Leaves SP (the "N" graph *a→c, b→c, b→d* is not SP); needs cycle detection; failure semantics become ambiguous |
| `ParallelGroup = "seed"` string labels | Regions survive interleaved sequential migrations | Groups that are not contiguous in *L* break C1 unless every intervening task commutes with the group |
| Infer everything from footprints | No annotations at all | Opaque SQL makes inference unsound, and silent parallelism violates the "permission" principle C1 |

Recommendation: adopt `ExecutionMode` + `ExecutionDepth` with the region definition above. Keep
footprints as a *verifier* of the author's claim, not as a *source* of parallelism.

## 4. Proposed Design

### 4.1 Annotations

```csharp
public enum ExecutionMode { Sequential = 0, Parallel = 1 }

// Same named properties on MigrationAttribute, MaintenanceAttribute and ProfileAttribute.
public ExecutionMode ExecutionMode { get; set; } = ExecutionMode.Sequential;
public int ExecutionDepth { get; set; } = 1;   // >= 1; meaningful only when Parallel
```

Seeding by FK depth in a profile:

```csharp
[Profile("Development", ExecutionMode = ExecutionMode.Parallel, ExecutionDepth = 1)]
public class SeedCountries : Migration { /* Insert.IntoTable("Countries")... */ }

[Profile("Development", ExecutionMode = ExecutionMode.Parallel, ExecutionDepth = 1)]
public class SeedCurrencies : Migration { /* independent of Countries */ }

[Profile("Development", ExecutionMode = ExecutionMode.Parallel, ExecutionDepth = 2)]
public class SeedCustomers : Migration { /* FK -> Countries */ }
```

Parallelism is enabled by **two** parties:

* the author marks the task (C1), and
* the operator raises `RunnerOptions.MaxDegreeOfParallelism`, whose default is 1.

Either one can veto. An existing deployment that upgrades FluentMigrator behaves identically until
both act.

### 4.2 The parallel scope lives on `IMigrationContext`

The context is the right owner, as proposed. `MigrationContext` is already per-migration (it is
built in `ExecuteMigration`, `MigrationRunner.cs:779`, and registered `TryAddTransient`), and it
already carries the processor and the service provider. The new member is:

```csharp
public interface IExecutionScope
{
    Guid ScopeId { get; }
    IExecutionScope Parent { get; }          // null for the run's root scope
    bool IsParallel { get; }
    int Depth { get; }                       // layer within the enclosing region
    int LaneId { get; }                      // stable per run, for logs and diagnostics
    CancellationToken CancellationToken { get; } // linked: parent token + sibling-failure token
}

// IMigrationContext gains:  IExecutionScope Execution { get; }
```

The context is passed **explicitly**, not through `AsyncLocal`. Ambient state flows into child
tasks implicitly through `ExecutionContext`, and that is the mechanism behind several hazards in
§5. The explicit context makes lane identity visible in code and testable.

Structured-concurrency rules for every scope:

* **S1.** A scope does not complete until all of its children have completed: there is no
  fire-and-forget.
* **S2.** The first failure cancels the sibling lanes (fail-fast). Fan-out may choose
  `CompleteAll`, which runs every target and reports each one.
* **S3.** Errors are aggregated and reported **in *L* order**, not completion order, so failure
  reports are reproducible.
* **S4.** Cancellation of a scope implies cancellation of all its descendants. AsyncAllTheWay's
  D2 rules apply to each lane separately.

### 4.3 Intra-migration fan-out (#1221)

```csharp
With.Databases("Tenant_*")
    .AsParallel()
    .WithDegreeOfParallelism(8)
    .WithFailurePolicy(FanOutFailure.CompleteAll)
    .Idempotent()
    .Delegate(db => db.Alter.Table("Orders").AddColumn("Region").AsString(32).Nullable());
```

The names follow PLINQ (`AsParallel`, `WithDegreeOfParallelism`), and the semantics match PLINQ
where it counts. Under `ParallelExecutionMode.Default`, PLINQ may run a query sequentially, and
`AsParallel()` here is likewise a *permission* (C1), not a command. The effective degree of
parallelism is the minimum of three values: the requested degree,
`RunnerOptions.MaxDegreeOfParallelism`, and what the provider and connection pool allow (§4.6, §6).

Three differences are deliberate and are documented on the API:

* The builder does not implement `IEnumerable<T>`, so `System.Linq.ParallelEnumerable` never binds
  to it.
* There is no lazy enumeration and no merge-option surface.
* Failures are reported in *L* order (S3). That is the analogue of `AsOrdered()`, always on.

* The target list is enumerated once, on the coordinator connection, and frozen. That makes the
  set deterministic for the rest of the run.
* Each target becomes a child lane with its own connection. `IMigrationConnectionFactory`
  therefore needs a target-aware overload, e.g.
  `CreateConnection(DbProviderFactory, MigrationTarget target)`, because today it takes only the
  provider factory (`IMigrationConnectionFactory.cs:66`).
* The delegate is called **once per target, on the coordinator, before any lane starts**. It only
  *builds* expressions, so user code in the delegate never runs concurrently. Only the *execution*
  of each target's expression list is parallel. This keeps author code single-threaded (see H9).

`With.Tables` (one database, many tables) is supported by the same mechanism, but it is a **weak
candidate for parallelism** (§7).

### 4.4 Connections and transactions

* **One lane = one connection = one transaction.** ADO.NET connections are not thread-safe, and
  MARS interleaves commands on a connection rather than running them in parallel.
* **No atomic commit across lanes.** Making N lanes commit atomically is the atomic commitment
  problem. Its standard solution, two-phase commit (Gray 1978), blocks if the coordinator fails
  (Skeen 1981), needs MSDTC or XA, and on .NET (Core) distributed transactions are supported only on
  Windows from .NET 7. FluentMigrator does not do 2PC. Atomicity holds **per lane**.
* **TransactionPerSession and parallel regions are mutually exclusive**, checked at load time.
  The session transaction lives on one connection, and lanes are other sessions. Combining them
  would silently downgrade the session guarantee.
* **The coordinator holds no open transaction while a region runs** (precondition P1). Otherwise a
  lane that touches an object the coordinator has locked (VersionInfo, for one) blocks on the
  coordinator, while the coordinator waits for the lane at the barrier. That is a cross-resource
  deadlock that no detector can see (H4).

### 4.5 The journal

The work is phased by how hard it is to journal.

| Phase | Scope | Journal impact |
| --- | --- | --- |
| **A** | Parallel **Maintenance** and **Profile** migrations | **None.** These are not journaled today: `ApplyMigrationUp` writes VersionInfo only when `IsAttributed()` is true, and maintenance and profile migrations go through `NonAttributedMigrationToMigrationInfoAdapter` (`ExtensionsForIMigrationInfo.cs:29–32`). This is exactly the proposed first use case, and it is the cheapest to make correct. |
| **B** | Intra-migration fan-out (#1221) | One version, N targets. Needs a **step log** (below). |
| **C** | Parallel **versioned** migrations | Each lane writes its own VersionInfo row in its own transaction, so AsyncAllTheWay's R5 holds per lane. This introduces **holes**: if v11 fails while v10 and v12 succeed, the journal is {10, 12}. Today holes arise only from out-of-order application. `VersionInfo.Latest()` (max version) stops being a progress frontier, which breaks `CurrentVersionMigrationConstraintAttribute`'s meaning. Rollback order is also undefined. **Deferred**: phase C needs a frontier definition ("largest *v* such that every task ≤ *v* is applied"), reverse-layer rollback, and the model checking in §6 before it ships. |

**The step log (phase B).** A fan-out migration's VersionInfo row is written only after every
target lane has committed. That makes it a *dual write*: the target commit happens on the target's
connection and the journal row on the coordinator's. A crash between the two leaves a target
changed and not recorded. Exactly-once execution across two resource managers is not achievable
without atomic commit. The achievable guarantee is **effectively-once = at-least-once plus
idempotence** (Helland 2012). Therefore:

* a new table, `VersionInfoStep (Version, Target, AppliedOn, Outcome)`, records completion per
  target, written by the coordinator after each target commits;
* on re-run, targets that have a step row are skipped, and the rest are retried;
* fan-out delegates **must be idempotent**, because the crash window between a target commit and its
  step row still exists. FluentMigrator requires `With.Databases(...).AsParallel()` to be marked
  idempotent explicitly (`.Idempotent()`), and it generates `IF NOT EXISTS` guards where the
  expressions permit.

On partial failure, the default is **forward recovery**: keep the targets that succeeded and retry
the rest. The alternative is **backward recovery** through compensation, i.e. running `Down` on the
succeeded targets, a saga (Garcia-Molina & Salem 1987). It is offered only when the migration's
`Down` is a genuine compensation. With non-transactional DDL, it frequently is not.

### 4.6 Provider capability

Parallel regions run at DOP = 1 (the permission is declined, per C1, with a log line) unless the
processor declares `SupportsConcurrentSessions`:

| Provider | Concurrent sessions are useful? | Notes |
| --- | --- | --- |
| SQL Server | Yes across databases. Within one database, DDL contends on schema locks and DML on the single per-database log stream. | Adding an FK needs locks on both tables, and lanes that create FKs into the same parent serialise on it. Deadlock victim: error 1205. |
| PostgreSQL | Yes | Most DDL takes `ACCESS EXCLUSIVE`; `ADD FOREIGN KEY` takes `SHARE ROW EXCLUSIVE` on both tables. Deadlock: `40P01`. |
| MySQL / MariaDB | Yes | Metadata locks, and DDL commits implicitly. Deadlock: 1213. |
| Oracle | Yes | Implicit DDL commit, library cache locks. Deadlock: `ORA-00060`. |
| SQLite | **No** | One writer per database file; concurrent writers get `SQLITE_BUSY`. Also synchronous-only (AsyncAllTheWay audit). |
| Firebird | Doubtful | Metadata changes are blocked by "object in use"; FM already commits mid-migration. |
| Snowflake | Yes (the warehouse scales) | DDL auto-commits. |
| Db2, HANA | *Unverified* | Synchronous-only drivers, so lanes need dedicated threads (H7). |

### 4.7 Seeding: guaranteeing that no trigger executes

Precondition (a) of §3.2 concerns **execution during the load window**, not whether triggers
exist. A seed task declares a `TriggerPolicy`, whose default is `Fire`:

* **`Fire`.** Triggers run normally, and their targets enter the footprint through the §2.1
  closure. This is always correct, and often parallelizes poorly.
* **`RequireNone`.** At region start, the coordinator verifies that the catalog shows **zero
  enabled triggers** on the task's write set, and fails the region otherwise. This has no side
  effects and works on every provider. It is the strongest guarantee and the recommended default
  for seed regions.
* **`Suppress`.** FluentMigrator applies the narrowest-scoped suppression mechanism the provider
  offers (table below). If there is none, or if only a mechanism the policy rejects is available,
  the region **fails at load time**.

Suppression mechanisms, ordered by scope:

| Scope | Mechanism | Effect on other sessions | After a crash |
| --- | --- | --- | --- |
| Operation | SQL Server `SqlBulkCopy` without `FireTriggers` (the default); `BULK INSERT` without `FIRE_TRIGGERS` | None | Nothing to undo |
| Session | PostgreSQL `SET session_replication_role = replica`. Requires superuser, or `GRANT SET ON PARAMETER` on PG 15+. | None | Nothing to undo |
| Table, transactional | SQL Server `DISABLE TRIGGER ALL ON t`, PostgreSQL `ALTER TABLE t DISABLE TRIGGER USER`, both inside the lane's transaction | Other sessions **block** on the table until commit; they never write without triggers | Rolled back with the lane |
| Table, non-transactional | Oracle `ALTER TABLE t DISABLE ALL TRIGGERS` (implicit commit); Firebird `ALTER TRIGGER … INACTIVE` (FM's Firebird processor commits mid-migration) | **Concurrent writers skip triggers** during the window (audit gaps, for example) | **Triggers stay disabled** |
| None | MySQL/MariaDB have no disable statement; the alternatives are drop/recreate (global, implicit commit) or a cooperative flag checked in each trigger body. SQLite has no disable statement either. | n/a | n/a |

Snowflake has no triggers, so `RequireNone` is satisfied trivially.

**Rejected by default:** non-transactional table-level suppression. It changes behaviour for other
sessions, and a crash leaves the database in a state no migration describes. It is allowed only for
an explicitly exclusive run (`--exclusive`, which asserts that there are no other writers), and only
if the re-enable step is journaled, so that recovery re-applies it before doing anything else.

**Suppression is part of the reference semantics.** A suppressed seed produces rows *without*
trigger side effects. That state is not reachable by ordinary inserts, so the seed must supply
whatever those triggers would have maintained: audit rows, counters, denormalized columns. The
policy applies identically at DOP = 1, so *L* is "seed with suppression", and C1 still holds.

**Checked preconditions and postconditions, not trusted ones:**

* *Pre (per lane).* Confirm that the mechanism is actually in effect on the lane's session
  (`SHOW session_replication_role` returns `replica`; the bulk-copy options are as expected). Then
  re-run the closure over triggers that ignore the mechanism. Under `replica`, triggers set to
  `ENABLE ALWAYS` still fire, so check `pg_trigger.tgenabled = 'A'`. The result must be empty.
* *Post (when FK enforcement was suspended as a side effect).* Two mechanisms above suspend
  enforcement, which breaks precondition (b) of §3.2:
  * PostgreSQL implements FKs as internal triggers, so `replica` skips the checks.
  * `SqlBulkCopy` without `CheckConstraints` leaves FK and CHECK constraints marked
    `is_not_trusted = 1`.

  After the load, re-validate: on SQL Server with `ALTER TABLE t WITH CHECK CHECK CONSTRAINT ALL`
  (which also restores trust for the optimizer), and on PostgreSQL with an anti-join per FK.
  Validation failure fails and rolls back the lane.

**Deterministic keys.** Lanes that draw from a shared sequence or identity column receive
interleaved values, so the data differs from *L* even though the schema matches. Seed tasks in a
parallel region therefore:

* insert **explicit keys** (`SET IDENTITY_INSERT`, which is per session and therefore per lane;
  `OVERRIDING SYSTEM VALUE` on PostgreSQL), and
* reference parents by those keys, never by looking up generated ids.

After the region, the coordinator advances each sequence or identity past the largest explicit key.

## 5. Concurrency Hazards from Dependency Injection

This is the central risk. In a sequential run, every service is effectively used by one thread at a
time, so thread-unsafety, captive dependencies and hidden locks are **latent**: present but never
triggered. Parallel execution makes them **live**. The hazards below are numbered so that tests and
analyzers can refer to them. The first group already exists in FluentMigrator itself.

### 5.1 Hazards inside FluentMigrator today

* **H1. Scope-wrapping by a shared `MigrationScopeHandler`.** `MigrationScopeHandler` is
  registered scoped (`FluentMigratorServiceCollectionExtensions.cs:250`). It holds
  `CurrentScope` as a plain auto-property, and `CreateOrWrapMigrationScope` returns a
  **`NoOpMigrationScope` whenever any scope is active** (`MigrationScopeHandler.cs`, the
  `HasActiveMigrationScope` check).
  * Two lanes sharing one DI scope would behave like this: lane B sees lane A's transaction as "the
    current scope" and runs **with no transaction at all**, silently, while its statements execute
    on its own connection.
  * The result is a correctness failure that never throws and passes every single-threaded test.
* **H2. Non-thread-safe journal state.** `VersionInfo` keeps a `List<long>`
  (`Versioning/VersionInfo.cs:35`), and `MigrationRunner._caughtExceptions` is a `List<Exception>`.
  Concurrent `Add` corrupts a `List<T>`.
* **H3. Singleton maintenance migrations (a latent captive dependency).** `IMaintenanceLoader` is a
  **singleton** (`:138`) that instantiates every maintenance migration **once, from the root
  provider** in its constructor (`MaintenanceLoader.cs:158`).
  * Any scoped service a maintenance migration injects is captured for the life of the process.
    Mark Seemann calls this a *captive dependency* (Seemann & van Deursen 2019).
  * Under parallel execution, the same instance and its captured services would be shared by every
    lane.
  * Phase A therefore requires `MaintenanceLoader` to load **types** and activate them per lane.
    `MigrationSource` already does this per scope (scoped, with a per-scope `ConcurrentDictionary`
    cache, `MigrationSource.cs:46`).
* **H4. Per-instance collection state.** `MigrationBase` stores `_context` in a field and guards
  `GetUpExpressions` with `lock (_mutex)` (`MigrationBase.cs`).
  * Today the lock is harmless. An `AsyncMigration` cannot `await` inside a `lock`, and the
    natural replacement, `SemaphoreSlim`, is **not reentrant**.
  * A migration whose `UpAsync` triggers a nested collection on the same instance (fan-out delegate
    → helper → same instance) then deadlocks *itself*, with no second party involved.
  * Rule: one migration instance per lane, and collection never re-enters an instance.

### 5.2 Hazards from user-imported services

Parallel lanes execute user code in five places:

* migration constructors (constructor injection through `ActivatorUtilities`),
* `Up()`/`UpAsync()` bodies,
* `Execute.WithConnection[Async]` delegates,
* constraint predicates, and
* user-supplied `IMigrationConnectionFactory`s and logger providers.

FluentMigrator cannot prove anything about these services. What follows is a taxonomy with the
mechanism of each hang or corruption, because "hard to debug" usually means the mechanism wasn't
known in advance.

* **H5. Shared scoped instances.**
  * If lanes share an `IServiceScope`, a scoped `DbContext` or unit of work is used from two
    threads.
  * EF Core detects *some* of these cases ("A second operation was started on this context
    instance…") and silently misbehaves in others.
* **H6. Non-thread-safe singletons.**
  * A singleton cache built on `Dictionary<,>` and mutated by two lanes can corrupt its internal
    buckets.
  * On .NET Framework this has long been known to produce an **infinite loop** inside
    `FindEntry`/`Insert`, which presents as a hang at 100 % CPU. Engineers then misdiagnose it as a
    deadlock.
  * Modern .NET throws `InvalidOperationException` for *some* concurrent-modification patterns, but
    not all.
* **H7. Thread-pool starvation.**
  * A lane on a synchronous driver (SQLite, Db2, HANA, ODP.NET before 23) blocks a pool thread for
    the duration of each statement. A service that does sync-over-async (`.Result`, or a
    token provider's synchronous `GetToken()` inside a connection factory) blocks another.
  * With DOP = N, that is up to 2N blocked threads. The pool injects threads gradually beyond its
    minimum. .NET 6 added faster injection when blocking on tasks, which mitigates the problem but
    does not eliminate it.
  * Every continuation, including the coordinator's barrier, waits behind the blocked threads.
* **H8. Hidden client-side locks: a deadlock no detector can see.**
  * Lane A holds a database lock (it is mid-transaction) and waits on a `SemaphoreSlim` inside a
    user singleton. Lane B holds that semaphore and waits on A's database lock.
  * The wait-for graph (Holt 1972) has a cycle, but the cycle spans **two resource managers**: the
    database's lock manager sees only B → A, and .NET sees only A → B.
  * Neither detector fires. This is the distributed-deadlock problem (Chandy, Misra & Haas 1983),
    and it ends only when the command timeout fires, possibly hours later.
* **H9. Hidden second connections: connection-pool exhaustion.**
  * Each lane holds its connection. A service, or a constraint (AsyncAllTheWay D7.3 already forbids
    this for constraints), opens a *second* connection from the same pool.
  * With DOP = `Max Pool Size`, every lane waits for a pool slot that only another waiting lane can
    free. Coffman's conditions (Coffman, Elphick & Shoshani 1971) are all met: hold-and-wait,
    no preemption, circular wait.
  * The run fails only when the pool wait times out, with an error that looks like a capacity
    problem.
* **H10. Ambient state through `ExecutionContext`.**
  * `AsyncLocal<T>` values flow into child tasks. So does `Transaction.Current` under
    `TransactionScopeAsyncFlowOption.Enabled`.
  * If a host wraps the runner in a `TransactionScope`, all N lanes **enlist in the same ambient
    transaction**. Their connections are separate, so enlistment promotes to a distributed
    transaction. That throws `PlatformNotSupportedException` off Windows, and on Windows needs MSDTC.
  * The same mechanism leaks culture, logging scopes and tenant identifiers set by middleware into
    lanes that belong to *other* tenants. That is a correctness and security problem for
    `With.Databases`.
* **H11. Disposal races.** A service starts background work (fire-and-forget) that outlives its lane
  scope. Disposal of the scope then races with that work. Rule S1 prevents this for FluentMigrator's
  own tasks, but not for tasks user services start.

### 5.3 Rules

* **DI-1. Lane isolation.**
  * Every lane runs in its **own `IServiceScope`**, created from the root `IServiceScopeFactory`.
  * Processor, connection, `MigrationScopeHandler`, `MigrationSource` and migration instances are
    resolved per lane.
  * This removes H1, H4 (one instance per lane) and H5 for scoped services.
* **DI-2. Coordinator/lane split.**
  * Services shared across lanes are an explicit, short, audited list: the scheduler, the journal
    coordinator, the cancellation source and the logger factory.
  * They must be linearizable (Herlihy & Wing 1990). The journal coordinator owns an immutable
    `VersionInfo` snapshot plus a concurrent applied set, which removes H2.
  * Everything not on the list is lane-scoped. FluentMigrator's own registrations get an explicit
    `Shared` or `PerLane` classification, enforced by a unit test that walks the service collection.
* **DI-3. Activation per lane.** `MaintenanceLoader` and `ProfileLoader` return types, not
  instances. This removes H3, and fixes the latent captive dependency even for sequential runs.
* **DI-4. Scope validation on.** FluentMigrator's hosts build their providers with
  `ValidateScopes = true` and `ValidateOnBuild = true`. That catches a scoped service resolved from
  the root, but not a singleton that *captures* a scoped service through a factory, so analyzers
  and documentation still have to cover H3-style cases.
* **DI-5. All waits stay inside one resource manager.**
  * Code running in a lane must not block on a client-side primitive while it holds a database
    transaction.
  * Any coordination FluentMigrator offers uses **database-side** locks (`sp_getapplock`,
    `pg_advisory_xact_lock`, `GET_LOCK`, `DBMS_LOCK`). Then the database's deadlock detector sees
    the whole wait-for graph, and H8 becomes an ordinary, detected deadlock with a victim error.
  * This is the single most important rule for debuggability.
* **DI-6. Clean ambient context.**
  * At lane start, FluentMigrator throws if `Transaction.Current != null`. Parallel lanes are
    incompatible with ambient transactions (H10).
  * Lane identity is carried by `IExecutionScope` and an explicit `ILogger.BeginScope`, never by
    `AsyncLocal` that user code set.
* **DI-7. Analyzer FM0006** (Info) on any `Parallel` migration whose constructor injects types
  outside an allow-list:
  * the allow-list is FluentMigrator abstractions, `ILogger<T>`, `IOptions<T>`/`IOptionsSnapshot<T>`
    and `IConfiguration`;
  * the message is "verify this service is thread-safe or scoped; see H5–H9";
  * it can be suppressed per type with `[assembly: ConcurrencySafe(typeof(MyService))]`.

## 6. Liveness, Deadlock and Diagnosability

**Argument that FluentMigrator's own schedule cannot deadlock.**

* Lanes wait for one another only at barriers.
* A lane reaches a barrier only after its transaction has committed or rolled back, so it holds no
  database locks there. Its single connection goes back to the region's budget.
* The coordinator holds no transaction during a region (P1).
* Each lane acquires exactly one connection, before it starts, and never a second one
  (DOP ≤ `Max Pool Size − 1` is validated, leaving a slot for the coordinator).
* Hold-and-wait is therefore broken at every client-side wait that FluentMigrator introduces. Any
  remaining cycle lies entirely inside the database and is detected there.
* Under DI-5 this extends to user code. Without DI-5 it cannot, and H8 and H9 are the residue that
  the following measures address.

**Measures:**

* **Server-detected deadlocks** (1205, `40P01`, 1213, `ORA-00060`):
  * the victim lane's transaction has already been rolled back by the server;
  * the lane is retried with bounded exponential backoff, *only* if the task is marked idempotent
    or is wholly transactional on a transactional-DDL provider;
  * otherwise the lane fails, under S2.
* **Thread-pool starvation (H7).** Lanes on synchronous-only drivers run on **dedicated threads**
  (`TaskCreationOptions.LongRunning`), bounded by DOP, so that they never consume pool threads.
* **Watchdog and blocking diagnosis.**
  * Each lane records its server session id (`@@SPID`, `pg_backend_pid()`, `CONNECTION_ID()`,
    `SYS_CONTEXT('USERENV','SID')`) and its current state (`ExecutingSql`, `InUserCode`,
    `AtBarrier`, `AwaitingConnection`).
  * If no lane makes progress for a configurable interval, the coordinator queries the server's
    blocking views (`sys.dm_os_waiting_tasks`, `pg_blocking_pids()`,
    `performance_schema.data_lock_waits`, `v$session.blocking_session`) on its own connection. It
    then prints the wait-for graph mapped back to lanes, for example:
    `lane 3 (spid 57, Tenant_042, SeedCustomers) is blocked by spid 61 (lane 1, Tenant_007)`.
    A spid that belongs to no lane points at H9.
  * A lane in `InUserCode` that is blocked by nothing on the server points at H6, H7 or H8.
  * This turns "it hangs" into a diagnosis.
* **Command timeouts remain the last resort** for liveness. Every lane statement has one; DOP > 1
  with `Timeout = infinite` produces a warning.

## 7. Where Parallelism Does Not Pay

Amdahl's law (Amdahl 1967) bounds speedup by the serial fraction, and in migrations the serial
fraction is often the database itself:

* **`With.Tables` in one database.** Deletes and inserts in one SQL Server database share one log
  stream. DDL serialises on schema locks and metadata. Expect contention, not speedup, except for
  minimally logged bulk loads. **Sequential is the right default.**
* **DDL-heavy versioned migrations.** These are metadata-bound, take exclusive locks and are short.
  Phase C's benefit is the smallest and its journal cost the largest.
* **Fan-out across databases or servers, and bulk seeding of independent tables.** These scale until
  server I/O saturates. They are the two motivating workloads, which is why phases A and B come
  first.

## 8. Verification Strategy

A foundation is only as rigorous as the evidence behind it. The following are proposed as
**acceptance criteria**, not aspirations.

* **V1. Differential testing against the serial oracle.** For every parallel test scenario, run with
  DOP = 1 and with DOP = k. Assert equal schema snapshots (`INFORMATION_SCHEMA`/catalog dump), equal
  data checksums for seeded tables, and equal VersionInfo and step-log contents. This tests the §2
  criterion directly.
* **V2. Property-based tests of the scheduler** (FsCheck), over generated regions with random
  depths, DOPs, durations and failure points. Properties:
  * every task runs at most once;
  * no task at depth *d+1* starts before all depth-*d* tasks finish;
  * active lanes never exceed DOP;
  * DOP = 1 yields exactly *L*;
  * errors are reported in *L* order (S3);
  * cancellation reaches every descendant (S4).
* **V3. Systematic concurrency testing** of the coordinator, the journal coordinator and lane
  lifecycle, with fake processors, using controlled scheduling such as
  [Microsoft Coyote](https://github.com/microsoft/coyote). It explores task interleavings
  systematically and replays failing schedules deterministically, in the tradition of CHESS
  (Musuvathi et al. 2008). Stress tests find Heisenbugs by luck; controlled scheduling finds them by
  construction.
* **V4. Model checking.** Write a TLA+ specification (Lamport 2002) of the scheduler, per-lane
  commit, the step log and crash and restart. Check:
  * *journal soundness*: every journaled task's effects are durable;
  * *step-log completeness after recovery*: re-running converges;
  * *no barrier deadlock under P1*.

  This is the gate for phase C. The approach is established for exactly this class of protocol
  (Newcombe et al. 2015).
* **V5. Hazard reproduction tests**, one per hazard, each of which must fail before its rule is
  implemented and pass after:
  * H1: shared scope handler gives a lane no transaction;
  * H3: a maintenance migration captures a scoped service;
  * H8: a singleton semaphore plus row locks gives a hang, then the watchdog report;
  * H9: DOP = pool size plus a second connection gives pool exhaustion, then load-time validation;
  * H10: an ambient `TransactionScope` gives the DI-6 exception.
* **V6. Fault injection.** Kill a lane's connection at every step boundary (before execution, mid
  statement, before commit, after commit but before the step row), then re-run. Assert
  convergence, and assert that the step log never records an uncommitted target.

## 9. Decision (proposed)

1. Adopt **serial equivalence to *L*** (§2) as the correctness criterion, and **series-parallel
   schedules** built from `ExecutionMode` + `ExecutionDepth` with the region definition in §3.2.
2. Put the parallel scope on `IMigrationContext` as an explicit `IExecutionScope`, governed by
   structured-concurrency rules S1–S4.
3. Ship in phases:
   * **Phase A**: parallel Maintenance and Profile regions, which need no journal changes.
   * **Phase B**: `With.Databases` fan-out with the step log and mandatory idempotence.
   * **Phase C**: parallel versioned migrations, deferred until V4 passes and a frontier definition
     is accepted.
4. Make DI-1 through DI-6 **preconditions for phase A**. DI-3 (activation per lane) and the H1 fix
   are worth doing even if parallelism never ships, because they remove latent defects.
5. Leave `Parallel` disabled unless the provider declares `SupportsConcurrentSessions` and the
   operator sets DOP > 1.

## 10. Consequences

* Positive: the two workloads that benefit most (fan-out and seeding) get parallelism with a
  precise guarantee, and DOP = 1 is provably today's behaviour.
* Positive: the DI audit (H1–H4) and per-lane activation fix latent defects in sequential runs.
* Positive: the watchdog turns cross-resource hangs into named, actionable reports.
* Negative: a new public concept (regions, depth, scopes), plus a new journal table for fan-out.
* Negative: authors take on a commutativity obligation that FluentMigrator can verify only for
  non-opaque expressions.
* Negative: the verification bar (V1–V6) is substantial. It is also the difference between a
  feature and a foundation.

## 11. Open Questions

1. Should `ExecutionDepth` be **inferred** from `Create.ForeignKey` expressions within a region, so
   that the declared value is a check rather than an input?
2. Should depth be validated against FK depth for seeding, and if so as an error or a warning?
3. Fan-out failure policy default: `FailFast` (S2) or `CompleteAll`? Tenants are independent, which
   argues for `CompleteAll`; consistency across tenants argues for `FailFast`.
4. Should the step log live in the coordinator database, or in each target's own VersionInfo?
   Per-target journaling avoids the dual write when the target's journal and its schema change
   share a transaction, but it scatters the run's history across N databases.
5. Phase C frontier: is "largest *v* such that every task ≤ *v* is applied" the right definition of
   `Latest()`, and what does `Rollback(steps)` mean when holes exist?

## References

* Amdahl, G. M. (1967). Validity of the single processor approach to achieving large scale computing capabilities. *AFIPS Spring Joint Computer Conference*.
* Bernstein, A. J. (1966). Analysis of programs for parallel processing. *IEEE Transactions on Electronic Computers*, EC-15(5), 757–763.
* Bernstein, P. A., Hadzilacos, V., & Goodman, N. (1987). *Concurrency Control and Recovery in Database Systems*. Addison-Wesley.
* Blumofe, R. D., & Leiserson, C. E. (1999). Scheduling multithreaded computations by work stealing. *Journal of the ACM*, 46(5), 720–748.
* Brent, R. P. (1974). The parallel evaluation of general arithmetic expressions. *Journal of the ACM*, 21(2), 201–206.
* Chandy, K. M., Misra, J., & Haas, L. M. (1983). Distributed deadlock detection. *ACM Transactions on Computer Systems*, 1(2), 144–156.
* Coffman, E. G., Elphick, M., & Shoshani, A. (1971). System deadlocks. *ACM Computing Surveys*, 3(2), 67–78.
* Feng, M., & Leiserson, C. E. (1997). Efficient detection of determinacy races in Cilk programs. *ACM SPAA*.
* Garcia-Molina, H., & Salem, K. (1987). Sagas. *ACM SIGMOD*.
* Graham, R. L. (1969). Bounds on multiprocessing timing anomalies. *SIAM Journal on Applied Mathematics*, 17(2), 416–429.
* Gray, J. (1978). Notes on data base operating systems. In *Operating Systems: An Advanced Course*, LNCS 60. Springer.
* Gray, J., & Lamport, L. (2006). Consensus on transaction commit. *ACM Transactions on Database Systems*, 31(1), 133–160.
* Helland, P. (2012). Idempotence is not a medical condition. *ACM Queue*, 10(4).
* Herlihy, M. P., & Wing, J. M. (1990). Linearizability: A correctness condition for concurrent objects. *ACM TOPLAS*, 12(3), 463–492.
* Holt, R. C. (1972). Some deadlock properties of computer systems. *ACM Computing Surveys*, 4(3), 179–196.
* Lamport, L. (2002). *Specifying Systems: The TLA+ Language and Tools for Hardware and Software Engineers*. Addison-Wesley.
* Moonen, L. (2001). Generating robust parsers using island grammars. *Working Conference on Reverse Engineering (WCRE)*.
* Musuvathi, M., Qadeer, S., Ball, T., Basler, G., Nainar, P. A., & Neamtiu, I. (2008). Finding and reproducing Heisenbugs in concurrent programs. *USENIX OSDI*.
* Newcombe, C., Rath, T., Zhang, F., Munteanu, B., Brooker, M., & Deardeuff, M. (2015). How Amazon Web Services uses formal methods. *Communications of the ACM*, 58(4), 66–73.
* Papadimitriou, C. H. (1979). The serializability of concurrent database updates. *Journal of the ACM*, 26(4), 631–653.
* Seemann, M., & van Deursen, S. (2019). *Dependency Injection Principles, Practices, and Patterns*. Manning. (Captive dependencies.)
* Skeen, D. (1981). Nonblocking commit protocols. *ACM SIGMOD*.
* Smith, N. J. (2018). Notes on structured concurrency, or: Go statement considered harmful. https://vorpus.org/blog/notes-on-structured-concurrency-or-go-statement-considered-harmful/
* Ullman, J. D. (1975). NP-complete scheduling problems. *Journal of Computer and System Sciences*, 10(3), 384–393.
* Valdes, J., Tarjan, R. E., & Lawler, E. L. (1982). The recognition of series parallel digraphs. *SIAM Journal on Computing*, 11(2), 298–313.
* Valiant, L. G. (1990). A bridging model for parallel computation. *Communications of the ACM*, 33(8), 103–111.
* Weihl, W. E. (1988). Commutativity-based concurrency control for abstract data types. *IEEE Transactions on Computers*, 37(12), 1488–1505.
* OpenJDK. JEP 453: Structured Concurrency (Preview). https://openjdk.org/jeps/453
