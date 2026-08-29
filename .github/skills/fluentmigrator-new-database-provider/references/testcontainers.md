# TestContainers for FluentMigrator integration tests

FluentMigrator's integration suite starts every enabled server database once per test
run through `test/FluentMigrator.Tests/Containers/*Container.cs` (all extend
`ContainerBase`) and `Integration/IntegrationTestsSetup.cs` (an NUnit `[SetUpFixture]`
that `Task.WhenAll`s the `Start()` calls). `ContainerBase.Start()` returns immediately
unless both `IsEnabled` and `ContainerEnabled` are true for that engine, then starts the
container and rewrites the configured connection string, replacing the default port
with the mapped public port. So the connection string in `appsettings.json` must
contain the engine's **default port as a literal** (e.g. `Port=5432`) for the
substitution to work.

## Decision tree: how to get a container

### 1. Official Testcontainers module exists (`Testcontainers.<Db>` on NuGet)

Check https://dotnet.testcontainers.org/modules/ and search NuGet for
`Testcontainers.<Db>`. Currently used in the repo: `Testcontainers.MsSql`, `.MySql`,
`.PostgreSql`, `.Oracle`, `.FirebirdSql`, `.Db2`. Other modules that exist and are
plausible FluentMigrator targets include `Testcontainers.ClickHouse`,
`Testcontainers.CockroachDb`, `Testcontainers.MariaDb`, `Testcontainers.Cassandra`,
`Testcontainers.Couchbase`, `Testcontainers.InfluxDb`, `Testcontainers.Neo4j`,
`Testcontainers.Redis`, `Testcontainers.Elasticsearch`, `Testcontainers.MongoDb`,
`Testcontainers.Milvus`, `Testcontainers.Qdrant`, `Testcontainers.Weaviate`. Verify
the current list — modules are added regularly.

A module gives you a builder with sane defaults (image, port, credentials, wait
strategy). Use it:

```csharp
using DotNet.Testcontainers.Containers;
using Testcontainers.CockroachDb;

namespace FluentMigrator.Tests.Containers;

public class CockroachDbContainer : ContainerBase
{
    protected override IntegrationTestOptions.DatabaseServerOptions ServerOptions => IntegrationTestOptions.CockroachDb;
    protected override int Port => 26257;
    protected override DockerContainer Build() => new CockroachDbBuilder().WithReuse(true).Build();
}
```

Add `<PackageVersion Include="Testcontainers.<Db>" Version="4.x.y" />` to
`Directory.Packages.props` — use the **same version** as the other `Testcontainers.*`
entries (they are released in lockstep; mixing versions causes binding failures) — and
`<PackageReference Include="Testcontainers.<Db>" />` to `FluentMigrator.Tests.csproj`.

Override defaults only when the processor needs them (e.g. `OracleContainer` sets
`.WithUsername("TestSchema")` because the Oracle tests assume that schema;
`Db2Container` sets `.WithAcceptLicenseAgreement(true)`). Whatever you set must match
the credentials in `appsettings.json`.

### 2. No module, but an official Docker image exists

Use the generic `ContainerBuilder` from the base `Testcontainers` package. You are
now responsible for everything the module would have given you:

```csharp
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;

namespace FluentMigrator.Tests.Containers;

public class ExampleDbContainer : ContainerBase
{
    protected override IntegrationTestOptions.DatabaseServerOptions ServerOptions => IntegrationTestOptions.ExampleDb;
    protected override int Port => 12345;

    protected override DockerContainer Build() => new ContainerBuilder()
        .WithImage("vendor/exampledb:1.2.3")            // pin a tag, never :latest
        .WithPortBinding(12345, assignRandomHostPort: true)
        .WithEnvironment("EXAMPLEDB_PASSWORD", "test")
        .WithEnvironment("EXAMPLEDB_DATABASE", "test")
        .WithEnvironment("ACCEPT_EULA", "Y")            // if the image needs it
        .WithWaitStrategy(Wait.ForUnixContainer()
            .UntilPortIsAvailable(12345)
            .UntilMessageIsLogged("ready to accept connections"))  // engine-specific
        .WithReuse(true)
        .Build();
}
```

Wait strategies: `UntilPortIsAvailable` alone is usually insufficient — most engines
open the port before they can accept a session. Prefer `UntilMessageIsLogged` with the
line the engine prints when fully up, or `UntilCommandIsCompleted("<cli> -c 'SELECT 1'")`
using the engine's CLI inside the image. Read the image's README for the canonical
readiness line. Set a generous timeout for heavy engines (`.WithStartupCallback` or
`.WithWaitStrategy(Wait.ForUnixContainer().UntilXxx().WithTimeout(...))` on newer
versions).

### 3. No official image

Options, in order of preference:

1. A vendor-blessed community image (check the vendor's docs for "Docker" first —
   many vendors publish to a private registry rather than Docker Hub: Oracle uses
   `container-registry.oracle.com`, IBM uses `icr.io`, Microsoft uses `mcr.microsoft.com`).
2. Build a small image in-repo from a `Dockerfile` under
   `test/FluentMigrator.Tests/Containers/<Db>/` and use `ImageFromDockerfileBuilder`.
   Keep it minimal and document how long the build takes; CI pays that cost on every
   run unless the image is pushed to GHCR.
3. Skip the container and let the integration tests stay `IsEnabled: false` by default,
   with documented instructions for running against a developer-provided instance
   (this is how Hana and Snowflake work today — cloud-only engines have no container).

## Things that go wrong (check each before opening the PR)

- **Architecture.** Many commercial images are amd64-only (Db2, Oracle XE, SQL
  Server). If so, add `"SupportedPlatforms": "x64"` to the `appsettings.json` block
  and guard the `IntegrationTestOptions` property with `Environment.Is64BitProcess`
  (see `Db2` / `Hana`). Apple-silicon developers will need Rosetta/emulation — say so
  in the docs.
- **Licence acceptance.** Images that require `ACCEPT_EULA`/`LICENSE=accept` must set
  it in `Build()`; the CI runner cannot answer a prompt. Confirm the licence permits
  automated CI use.
- **Startup time.** Oracle and Db2 take 1–3 minutes. `IntegrationTestsSetup` starts
  all containers in parallel so one slow engine does not serialise the others, but NUnit
  still waits. `.WithReuse(true)` keeps the container alive between local runs — always
  set it so developers don't pay the startup cost per `dotnet test`.
- **Credentials in two places.** The container's env vars/builder options and the
  `appsettings.json` connection string must agree. The only thing `ContainerBase`
  rewrites is the port.
- **Default port literal.** `ContainerBase.Start()` does
  `ConnectionString.Replace(Port.ToString(), publicPort.ToString())`. If the default
  port number appears elsewhere in the connection string (e.g. as part of a password
  or timeout), the replace will corrupt it. Choose test credentials without that
  substring.
- **Pre-created objects.** If the tests assume a schema/user/database exists (Oracle's
  `TestSchema`), create it via the builder (`WithUsername`, `WithDatabase`) or via a
  `WithResourceMapping` init script executed by the image's entrypoint hook
  (`/docker-entrypoint-initdb.d/` for Postgres/MySQL-style images).
- **Windows CI.** Containers only run on the `ubuntu-latest` matrix leg. The
  `pull-requests.yaml` env vars use `${{ matrix.os == 'ubuntu-latest' }}` for exactly
  this reason; copy that pattern.
- **Docker socket in CI.** GitHub-hosted Ubuntu runners have Docker preinstalled;
  Testcontainers auto-detects it. Nothing else is needed. If a self-hosted runner is
  used, `TESTCONTAINERS_HOST_OVERRIDE` / `DOCKER_HOST` may be required.

## Enabling the container

`appsettings.json` (defaults off so `dotnet test` works anywhere):

```json
"<Db>": {
    "ConnectionString": "Server=localhost;Port=<defaultPort>;Database=test;User Id=test;Password=test",
    "IsEnabled": false,
    "ContainerEnabled": false
}
```

Local override (environment variables, prefix `FM_`, `__` separator):

```bash
export FM_TestConnectionStrings__<Db>__IsEnabled=true
export FM_TestConnectionStrings__<Db>__ContainerEnabled=true
dotnet test test/FluentMigrator.Tests --framework net8.0 --filter "Category=<Db>"
```

Or via `dotnet user-secrets set "TestConnectionStrings:<Db>:IsEnabled" true --id FluentMigrator.Tests`
for a persistent local setting.

CI (`.github/workflows/pull-requests.yaml`, in the `Restore, Build, Test` step `env:`):

```yaml
FM_TestConnectionStrings__<Db>__ContainerEnabled: ${{ matrix.os == 'ubuntu-latest' }}
FM_TestConnectionStrings__<Db>__IsEnabled: ${{ matrix.os == 'ubuntu-latest' }}
```

Also check `azure-pipelines*.yml` if the engine should run there too.

## Embedded engines (no container)

In-process databases (SQLite, DuckDB, H2-style engines with a .NET port) do not need
any of the above. Follow the SQLite pattern: `IsEnabled: true` in `appsettings.json`,
an in-memory or `|DataDirectory|`-relative file connection string, and
`IntegrationTestOptions.<Db>` returning
`GetOptions(ProcessorIdConstants.<Db>).ReplaceConnectionStringDataDirectory()` when a
file path is used so each test run gets a fresh temp directory (see
`IntegrationTestBase.SetUpDataDirectory`/`TearDownDataDirectory`).
