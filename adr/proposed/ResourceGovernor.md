# ADR: Top-Level Create Syntax for Resource Governors

## Status
Proposed

## Context
FluentMigrator does not currently provide any top-level fluent syntax for database resource governance. Resource governance is the mechanism by which a database engine classifies incoming sessions/queries and enforces hard caps on CPU, memory, I/O, and degree of parallelism for each classification, so that a small number of runaway queries cannot starve the rest of the workload.

SQL Server calls this feature **Resource Governor**. Most other enterprise-grade RDBMS provide a direct (if differently named) equivalent:

- **Oracle Database** — Database Resource Manager (Resource Consumer Groups, Resource Plans, Resource Plan Directives).
- **PostgreSQL** — no first-class SQL DDL equivalent in vanilla PostgreSQL; governance is achieved through role-level `SET` parameters (e.g., `work_mem`, `statement_timeout`), the `pg_resqueue`/`pg_wlm` extensions or, in some forks (Greenplum, EDB Postgres Advanced Server), a native `RESOURCE GROUP` / `RESOURCE QUEUE` DDL, and OS-level cgroups mapping.
- **MySQL (Enterprise/Community 8.0+) and Percona** — `RESOURCE GROUP` DDL that pins connection threads to CPU cores and thread priorities.
- **Other providers** (MariaDB, Aurora MySQL/PostgreSQL, SQLite, Firebird, DB2 LUW/iSeries, Snowflake, SAP HANA, Redshift) have partial or no equivalent, as detailed below.

Without native FluentMigrator support, users must currently fall back to `Execute.Sql()` with hand-written, provider-specific DDL, which bypasses FluentMigrator's abstraction and compatibility layers, and offers no `Down()` support for reversible migrations.

This ADR proposes a top-level `Create`/`Delete`/`Alter` syntax for resource governance objects and documents how it maps (or fails to map) across the providers FluentMigrator supports.

## Research Findings

### Terminology

| Concept | Description |
|---|---|
| **Resource Pool / Resource Consumer Group / Resource Group** | The physical container that defines hard limits (CPU %, memory %, max degree of parallelism, IOPS). |
| **Workload Group / Consumer Group Mapping** | A logical group of sessions that share settings (priority, max requests) and are bound to a single resource pool. |
| **Classifier Function / Mapping Rule** | The logic that inspects an incoming session (login, app name, host, module) and assigns it to a workload/consumer group. |
| **Resource Plan** | (Oracle-specific) A named collection of consumer group directives that can be activated/deactivated as a unit. |

### Provider Resource Governance Support Matrix

| Feature | SQL Server | Oracle | PostgreSQL (vanilla) | Greenplum / EDB Postgres | MySQL 8 Enterprise / Percona | MariaDB | Aurora MySQL | Aurora PostgreSQL | Snowflake | SAP HANA | DB2 LUW | Redshift | SQLite | Firebird |
|---|---|---|---|---|---|---|---|---|---|---|---|---|---|---|
| **Native DDL for resource limits** | ✅ `CREATE RESOURCE POOL` | ✅ `DBMS_RESOURCE_MANAGER` PL/SQL API (no plain DDL) | ❌ | ✅ `CREATE RESOURCE GROUP` / `CREATE RESOURCE QUEUE` | ✅ `CREATE RESOURCE GROUP` | ❌ | ⚠️ Aurora Serverless v2 ACUs only, no session-level DDL | ❌ | ✅ Warehouses act as the isolation unit; no session-level resource group DDL | ✅ `CREATE WORKLOAD CLASS` / `CREATE WORKLOAD MAPPING` | ✅ Workload Management (WLM) via `db2 create workload` | ✅ WLM `CREATE WLM_QUERY_GROUP`-style config tables | ❌ | ❌ |
| **Logical routing group** | ✅ `CREATE WORKLOAD GROUP` | ✅ Consumer groups | ❌ (role-level `SET`) | ✅ Consumer groups map roles to resource groups | ✅ Thread assigned via `SET RESOURCE GROUP` | ❌ | ❌ | ❌ | ✅ Resource monitors / warehouses | ✅ Workload classes/mappings | ✅ Workload classes | ✅ Query groups / WLM queues | ❌ | ❌ |
| **Classifier function** | ✅ `CREATE FUNCTION ... WITH SCHEMABINDING` registered via `sp_configure` | ✅ Mapping rules via `DBMS_RESOURCE_MANAGER.SET_CONSUMER_GROUP_MAPPING` | ❌ | ✅ Role-based binding | ⚠️ Session-level `SET RESOURCE GROUP` (app must opt-in) | ❌ | ❌ | ❌ | ✅ Resource monitors triggered by warehouse/role | ✅ Mapping rules (user, app, statement) | ✅ WLM mapping rules | ✅ WLM query group assignment rules | ❌ | ❌ |
| **CPU cap** | ✅ | ✅ | ⚠️ via cgroups only | ✅ | ✅ VCPU pinning | ❌ | ❌ | ❌ | ⚠️ Indirect (warehouse size) | ✅ | ✅ | ✅ | ❌ | ❌ |
| **Memory cap** | ✅ | ✅ | ⚠️ via role `work_mem` only | ✅ | ❌ (CPU/thread only) | ❌ | ❌ | ❌ | ❌ | ✅ | ✅ | ✅ | ❌ | ❌ |
| **Kill/Downgrade long-running queries** | ⚠️ Limited (max requests only) | ✅ Automatic switch/kill on CPU threshold | ❌ (manual `statement_timeout`) | ✅ | ❌ | ❌ | ❌ | ❌ | ✅ (statement/warehouse timeout) | ✅ | ✅ | ✅ (WLM timeout) | ❌ | ❌ |

> **Legend:** ✅ Supported natively · ⚠️ Partial/indirect support · ❌ Not supported

### Detailed Provider Analysis

#### SQL Server
- `CREATE RESOURCE POOL poolname WITH (MAX_CPU_PERCENT = 50, MIN_MEMORY_PERCENT = 0, MAX_MEMORY_PERCENT = 50)`
- `CREATE WORKLOAD GROUP groupname USING poolname`
- `CREATE FUNCTION classifier_function() RETURNS sysname ...` then `ALTER RESOURCE GOVERNOR WITH (CLASSIFIER_FUNCTION = dbo.classifier_function)`
- Requires `ALTER RESOURCE GOVERNOR RECONFIGURE` to activate changes.
- This is the reference implementation for this ADR's proposed syntax.

#### Oracle Database
- No plain SQL DDL; configuration is performed exclusively through the `DBMS_RESOURCE_MANAGER` PL/SQL package (`CREATE_CONSUMER_GROUP`, `CREATE_PLAN`, `CREATE_PLAN_DIRECTIVE`), wrapped in a pending area (`CREATE_PENDING_AREA` / `SUBMIT_PENDING_AREA`).
- Mapping rules are configured via `DBMS_RESOURCE_MANAGER.SET_CONSUMER_GROUP_MAPPING`.
- FluentMigrator would need to emit `Execute.Sql()`-style PL/SQL blocks rather than declarative DDL, since there is no `CREATE RESOURCE POOL`-equivalent statement.

#### PostgreSQL (vanilla)
- No built-in resource-group DDL. Governance is done at the role level:
  ```sql
  ALTER ROLE reporting_user SET work_mem = '64MB';
  ALTER ROLE reporting_user SET statement_timeout = '30s';
  ```
- OS-level cgroups mapping is outside the scope of SQL DDL entirely.
- Enterprise forks (Greenplum, EDB Postgres Advanced Server) add `CREATE RESOURCE GROUP` / `CREATE RESOURCE QUEUE`, but this is not part of vanilla PostgreSQL and would need to be a provider-specific extension.

#### MySQL 8 (Enterprise/Community) / Percona / MariaDB
- `CREATE RESOURCE GROUP ReportingGroup TYPE = USER VCPU = 0-2 THREAD_PRIORITY = 5;`
- `ALTER RESOURCE GROUP ReportingGroup VCPU = 0-2;`
- `SET RESOURCE GROUP ReportingGroup [FOR thread_id]` — routes an existing session/thread into the group.
- MariaDB does not implement `CREATE RESOURCE GROUP` as of this writing.
- Aurora MySQL follows MySQL 8 semantics only when running in compatible engine versions; Aurora Serverless v2 uses ACU-based auto-scaling instead of session-level resource groups.

#### Snowflake
- No `CREATE RESOURCE POOL`, but **warehouses** are the physical resource-isolation unit (`CREATE WAREHOUSE ... WAREHOUSE_SIZE = 'MEDIUM'`), and **resource monitors** cap credit consumption (`CREATE RESOURCE MONITOR ... CREDIT_QUOTA = 100`).
- Because warehouses conflate compute provisioning with resource governance, they are architecturally different enough from SQL Server pools that a 1:1 mapping is not accurate; this should be treated as a provider-specific extension rather than the core API.

#### SAP HANA
- Workload Management: `CREATE WORKLOAD CLASS classname SET 'PRIORITY' = '5', 'TOTAL STATEMENT MEMORY LIMIT' = '512'`
- `CREATE WORKLOAD MAPPING mappingname WORKLOAD CLASS classname SET 'USER NAME' = 'reporting_user'`
- Closest 1:1 analog to SQL Server's pool + workload group + classifier model among non-SQL Server providers.

#### DB2 LUW / DB2 iSeries
- Workload Management (WLM): `db2 "CREATE WORKLOAD reporting_wl SESSION_USER('reporting_user') MAXIMUM CONCURRENT ACTIVITIES 5 ENABLE"`
- Service classes and thresholds provide CPU/memory shaping, roughly analogous to resource pools.

#### Redshift
- WLM (Workload Management) is configured via **parameter groups and queues**, not classic DDL statements, though newer versions support `CREATE WORKLOAD` style configuration through system tables and console/API rather than SQL text executed per-session.

#### SQLite / Firebird
- No resource governance concept exists; these are typically single-connection or lightly-concurrent embedded/small-footprint engines. Any resource governance expression should raise `DatabaseOperationNotSupportedException` per `CompatibilityMode.HandleCompatibility`.

### Summary Statistics

| Category | Providers |
|---|---|
| **Native declarative DDL resembling SQL Server pools/groups** | SQL Server, MySQL 8/Percona, SAP HANA, DB2 LUW/iSeries, Greenplum/EDB Postgres (fork-specific) |
| **API/procedural configuration only (no DDL)** | Oracle (PL/SQL package), Redshift (parameter groups) |
| **Role/session parameter workaround only** | Vanilla PostgreSQL, Aurora PostgreSQL |
| **Conceptually different isolation unit (warehouse/ACU)** | Snowflake, Aurora Serverless v2 |
| **No support** | SQLite, Firebird, MariaDB |

---

## Proposed Top-Level Syntax

The syntax mirrors FluentMigrator's existing `Create`/`Delete`/`Alter` root conventions, treating a resource pool as a first-class schema object with an optional nested workload group, consistent with how `Create.Table(...).WithColumn(...)` composes related objects in a single fluent chain.

### Core API

```csharp
// Create a resource pool with CPU/memory caps
Create.ResourcePool("ReportingPool")
    .WithMaxCpuPercent(50)
    .WithMaxMemoryPercent(50);

// Create a workload group bound to a pool
Create.WorkloadGroup("ReportingGroup")
    .UsingResourcePool("ReportingPool")
    .WithImportance(WorkloadImportance.Low);

// Register/replace the classifier function used to route sessions
Create.Classifier("dbo.fn_ClassifyReportingSessions");

// Remove objects (mirrors Delete.Table, Delete.Index, etc.)
Delete.WorkloadGroup("ReportingGroup");
Delete.ResourcePool("ReportingPool");

// Provider-specific escape hatch for engines without declarative DDL (e.g. Oracle)
IfDatabase(ProcessorIdConstants.Oracle)
    .Execute.Sql(@"
        BEGIN
          DBMS_RESOURCE_MANAGER.CREATE_PENDING_AREA();
          DBMS_RESOURCE_MANAGER.CREATE_CONSUMER_GROUP('REPORTING_GP', 'Reporting workload');
          DBMS_RESOURCE_MANAGER.SUBMIT_PENDING_AREA();
        END;");
```

### Proposed Expression Classes

```
Create.ResourcePool(name)        → ICreateResourcePoolExpressionBuilder
  .WithMaxCpuPercent(int)        → ICreateResourcePoolExpressionBuilder
  .WithMinCpuPercent(int)        → ICreateResourcePoolExpressionBuilder
  .WithMaxMemoryPercent(int)     → ICreateResourcePoolExpressionBuilder
  .WithMinMemoryPercent(int)     → ICreateResourcePoolExpressionBuilder
  .WithMaxIops(int)              → ICreateResourcePoolExpressionBuilder (SQL Server only, ISupportAdditionalFeatures)

Create.WorkloadGroup(name)              → ICreateWorkloadGroupExpressionBuilder
  .UsingResourcePool(poolName)          → ICreateWorkloadGroupExpressionBuilder
  .WithImportance(WorkloadImportance)   → ICreateWorkloadGroupExpressionBuilder
  .WithMaxRequests(int)                 → ICreateWorkloadGroupExpressionBuilder

Create.Classifier(functionName)  (terminates chain — registers/activates the classifier)

Delete.ResourcePool(name)        (terminates chain)
Delete.WorkloadGroup(name)       (terminates chain)
```

### Builder Interface Definitions

```csharp
/// <summary>Fluent entry point for CREATE RESOURCE POOL-equivalent statements.</summary>
public interface ICreateResourcePoolExpressionBuilder : IFluentSyntax
{
    /// <summary>Sets the maximum percentage of CPU the pool may consume.</summary>
    ICreateResourcePoolExpressionBuilder WithMaxCpuPercent(int percent);

    /// <summary>Sets the minimum guaranteed percentage of CPU for the pool.</summary>
    ICreateResourcePoolExpressionBuilder WithMinCpuPercent(int percent);

    /// <summary>Sets the maximum percentage of memory the pool may consume.</summary>
    ICreateResourcePoolExpressionBuilder WithMaxMemoryPercent(int percent);

    /// <summary>Sets the minimum guaranteed percentage of memory for the pool.</summary>
    ICreateResourcePoolExpressionBuilder WithMinMemoryPercent(int percent);
}

/// <summary>Fluent entry point for CREATE WORKLOAD GROUP-equivalent statements.</summary>
public interface ICreateWorkloadGroupExpressionBuilder : IFluentSyntax
{
    /// <summary>Binds the workload group to a previously created resource pool.</summary>
    ICreateWorkloadGroupExpressionBuilder UsingResourcePool(string poolName);

    /// <summary>Sets the relative scheduling importance of sessions in this group.</summary>
    ICreateWorkloadGroupExpressionBuilder WithImportance(WorkloadImportance importance);

    /// <summary>Limits the maximum number of concurrent requests for this group.</summary>
    ICreateWorkloadGroupExpressionBuilder WithMaxRequests(int maxRequests);
}
```

### ISupportAdditionalFeatures Candidates

| Feature | Recommended Approach | Reason |
|---|---|---|
| **Classifier function registration** | Core API (`Create.Classifier(...)`), generator translates to provider-specific activation call | Concept exists (in some form) on SQL Server, Oracle, SAP HANA, DB2 |
| **IOPS limits** | `ISupportAdditionalFeatures` / extension method | SQL Server-specific option not shared by other providers |
| **PostgreSQL role-level `work_mem`/`statement_timeout`** | Provider extension mapped onto `ALTER ROLE ... SET` rather than the core pool/group API | Conceptually a role setting, not a first-class DDL object |
| **Snowflake warehouses / resource monitors** | Separate provider-specific extension (`Create.Warehouse(...)`) rather than reusing `Create.ResourcePool` | Different isolation unit (compute cluster) with billing implications |
| **Oracle Resource Manager (plans, consumer groups, directives)** | `IfDatabase(ProcessorIdConstants.Oracle)` + `Execute.Sql()` PL/SQL block, or a dedicated Oracle extension API in a follow-up ADR | No native DDL; requires the `DBMS_RESOURCE_MANAGER` package and pending-area transaction model, which doesn't map onto simple `CREATE`/`ALTER` statements |
| **Greenplum/EDB Postgres `CREATE RESOURCE GROUP`** | Provider-specific extension (`FluentMigrator.Extensions.Postgres`-style package) | Not part of vanilla PostgreSQL; would mislead users of standard PostgreSQL |

### Handling Unsupported Providers (SQLite, Firebird, MariaDB, vanilla PostgreSQL, Redshift, Aurora)

For providers with no native resource-governance DDL, FluentMigrator should throw a `DatabaseOperationNotSupportedException` when a `Create.ResourcePool`/`Create.WorkloadGroup`/`Create.Classifier` expression is executed, consistent with `CompatibilityMode.HandleCompatibility` and `GenericGenerator.NotSupported()`.

---

## Recommendations

1. **Implement the core `Create.ResourcePool` / `Create.WorkloadGroup` / `Delete.ResourcePool` / `Delete.WorkloadGroup` API first**, targeting SQL Server as the primary implementation since it has the most direct, fully declarative DDL surface.

2. **Treat the classifier function as a registration step** (`Create.Classifier(functionName)`) rather than trying to model the classifier logic itself in the fluent API — the classifier body is arbitrary user-defined-function logic that FluentMigrator should not attempt to abstract.

3. **Implement SAP HANA and DB2 LUW/iSeries next**, since their workload class/mapping model is the closest conceptual analog to SQL Server's pool + group + classifier model.

4. **Do not attempt to unify Oracle Resource Manager, Snowflake warehouses, or Greenplum/EDB Postgres resource groups under the same core API.** These should be deferred to provider-specific extensions or follow-up ADRs, since forcing them into the SQL Server-shaped API would misrepresent their semantics (procedural pending-area model for Oracle, compute-cluster billing model for Snowflake, fork-specific DDL for Greenplum/EDB).

5. **Throw `DatabaseOperationNotSupportedException`** for SQLite, Firebird, MariaDB, vanilla PostgreSQL, Redshift, and Aurora (both engines), since none provide session-level resource-governance DDL equivalent to the proposed core API.

6. **Expose IOPS limits and other SQL-Server-only knobs via `ISupportAdditionalFeatures`**, keeping the cross-database core surface limited to CPU percent, memory percent, importance, and max requests, which have the broadest conceptual support.

---

## Decision
**To be decided by maintainers**

This ADR provides the research necessary to make an informed decision about implementing resource governance support in FluentMigrator. The key tradeoffs are:

**Pros**:
- Enables database-agnostic migrations that provision workload isolation as part of environment setup, rather than requiring manual, out-of-band DBA scripts.
- Consistent API reduces provider-specific `Execute.Sql()` calls for the providers that do have declarative DDL (SQL Server, MySQL 8, SAP HANA, DB2).
- Establishes a pattern (`Create.<Concept>` + `ISupportAdditionalFeatures` + `IfDatabase()` escape hatch) that can be reused for future cross-database governance features.

**Cons**:
- Resource governance concepts diverge more sharply between providers than most other FluentMigrator features (DDL vs. procedural API vs. role-level settings vs. compute-cluster sizing), making a truly "database-agnostic" API difficult to achieve without leaky abstractions.
- Oracle, Snowflake, and Greenplum/EDB Postgres cannot be meaningfully expressed through the proposed core API without either oversimplifying their models or requiring `Execute.Sql()`/provider-specific extensions anyway.
- The majority of providers (SQLite, Firebird, MariaDB, vanilla PostgreSQL, Redshift, Aurora) have no equivalent at all, so the value of a top-level abstraction is concentrated in a minority of providers (SQL Server, MySQL 8/Percona, SAP HANA, DB2).
- Resource Governor changes typically require a server-wide `RECONFIGURE`-style activation step (SQL Server's `ALTER RESOURCE GOVERNOR RECONFIGURE`) that has different transactional semantics than ordinary schema DDL, and FluentMigrator's migration/transaction model would need to account for this.

---

## References

- **SQL Server**: [CREATE RESOURCE POOL (Transact-SQL)](https://learn.microsoft.com/en-us/sql/t-sql/statements/create-resource-pool-transact-sql), [CREATE WORKLOAD GROUP (Transact-SQL)](https://learn.microsoft.com/en-us/sql/t-sql/statements/create-workload-group-transact-sql), [Resource Governor](https://learn.microsoft.com/en-us/sql/relational-databases/resource-governor/resource-governor)
- **Oracle**: [Managing Resources with Oracle Database Resource Manager (19c)](https://docs.oracle.com/en/database/oracle/oracle-database/19/admin/managing-resources-with-oracle-database-resource-manager.html), [Managing Resources with Oracle Database Resource Manager (12.1)](https://docs.oracle.com/database/121/ADMIN/dbrm.htm)
- **PostgreSQL**: [Resource Consumption (Runtime Config)](https://www.postgresql.org/docs/current/runtime-config-resource.html), [How to Apply Data Governance for PostgreSQL — DataSunrise](https://www.datasunrise.com/knowledge-center/how-to-apply-data-governance-for-postgresql/)
- **cgroups**: [Deep Dive: Linux cgroups Resource Management — CloudAstra Technologies](https://www.linkedin.com/pulse/deep-dive-linux-cgroups-resource-management-cloudastra-technologies-08vpc)
- **MySQL / Percona**: [MySQL 8 Load Fine-Tuning with Resource Groups — Percona Blog](https://www.percona.com/blog/mysql-8-load-fine-tuning-with-resource-groups/), [MySQL Resource Groups](https://dev.mysql.com/doc/refman/8.0/en/resource-groups.html)
- **Snowflake**: [Warehouses](https://docs.snowflake.com/en/user-guide/warehouses-overview), [Resource Monitors](https://docs.snowflake.com/en/user-guide/resource-monitors)
- **SAP HANA**: [Workload Management](https://help.sap.com/docs/SAP_HANA_PLATFORM/6b94445c94ae495c83a19646e7c3fd56/f1f27c31ceb44346a72db8ffd0985617.html)
- **DB2 LUW**: [Workload Management](https://www.ibm.com/docs/en/db2/11.5?topic=management-workload)
- **Redshift**: [Workload Management (WLM)](https://docs.aws.amazon.com/redshift/latest/dg/c_workload_mngmt_classification.html)
- **Original request**: [SQLServerCentral — Taming Resource Hogs: Using SQL Server Resource Governor to Restrict User Group Consumption](https://www.sqlservercentral.com/articles/taming-resource-hogs-using-sql-server-resource-governor-to-restrict-user-group-consumption)

## Conclusion

SQL Server's Resource Governor has direct or partial equivalents in Oracle, MySQL 8/Percona, SAP HANA, and DB2 LUW/iSeries, but the underlying models (declarative DDL vs. procedural API vs. role-level settings vs. compute-cluster sizing) diverge enough that a single unified core API can only cleanly cover a subset of providers. This ADR proposes a `Create.ResourcePool` / `Create.WorkloadGroup` / `Create.Classifier` core API modeled closely on SQL Server's syntax, with `ISupportAdditionalFeatures` and `IfDatabase()` escape hatches for provider-specific concepts (Oracle's procedural resource plans, Snowflake's warehouses, Greenplum/EDB's fork-specific resource groups), and `DatabaseOperationNotSupportedException` for providers with no equivalent (SQLite, Firebird, MariaDB, vanilla PostgreSQL, Redshift, Aurora). Maintainers should decide whether the value delivered for the small set of fully-compatible providers justifies the added API surface, given that most FluentMigrator-supported providers cannot participate in this feature at all.
