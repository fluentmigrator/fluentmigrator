# FluentMigrator.Runner.Host — a general command-hosting framework

## Philosophy

**Work Where You Work Best** is a broad, environment-agnostic philosophy. It allows you to freely choose between FluentMigrator runners, depending on your daily tasks.

## Design goals

Ten consumers (DotNet.Cli, Console, MSBuild, TUI, Aspire, CopilotCanvas, wasm-based REPL, MCP plug-in, GitHub Action, EF Core migration generator) all need to do the same underlying thing — configure FluentMigrator, resolve which migration operation to run, run it, and report the outcome — but each arrives at "which operation" through a completely different front door: a command line, an MSBuild task's properties, an Aspire dashboard button, an MCP tool call, a browser's JS interop bridge. Building ten independent implementations of "run migrations up" means ten independent places for behavior to drift.

The host needs three things, cleanly separated:

1. A **fluent builder** that configures an `IServiceCollection` before anything runs, independent of how the host is eventually invoked. FluentMigrator already covers most of this via `IServiceCollection.AddFluentMigratorCore()` and friends.
2. A **discoverable, composable unit of work** — a `Command` — enumerable and invocable by the host, so new operations show up in every consumer without each consumer being modified by hand.
3. A terminal entry point that turns whatever the caller provides — `string[] argv`, MSBuild task properties, an Aspire command payload, a Canvas/MCP request, a JS interop call from a browser — into "run this command with these arguments," executes setup → command → teardown, and reports a result in whatever shape that consumer needs. This can't assume `string[] argv` — more than half of these consumers never see a command line at all.

The fix that makes all three of these clean: split "what a migration operation is" (an `IMigrationCommand` with typed, self-describing arguments) from "how it's invoked" (argv parsing is just *one* adapter over that abstraction). Everything else — MSBuild task properties, Aspire `WithCommand` metadata, a Copilot Canvas JSON schema, Spectre.Console prompts, WebAssembly browser navigation/prompts, and MCP request/response handling — becomes another adapter over the same commands, instead of ten reimplementations of "run migrations up."

## Package layout

```
FluentMigrator.Runner.Host        <- new. MigrationHost, IMigrationCommand, argument metadata, discovery, DI glue
FluentMigrator.Runner.Commands    <- new. Built-in commands: MigrateUp/Down, Rollback, ListMigrations, Validate, GenerateScript
FluentMigrator.DotNet.Cli         <- thin: argv adapter over MigrationHost
FluentMigrator.Console            <- thin: legacy argv adapter over MigrationHost (back-compat mapping)
FluentMigrator.MSBuild            <- thin: ITaskItem[] -> structured invocation over MigrationHost
FluentMigrator.TUI                <- thin: Spectre.Console adapter, built from command/argument metadata
FluentMigrator.Aspire             <- thin: exposes each IMigrationCommand as an Aspire resource Command
FluentMigrator.CopilotCanvas      <- thin: emits a JSON schema of commands/arguments; executes via structured invocation
FluentMigrator.Repl               <- thin: provides a C# IDE like dotnetfiddle to run migrations against wasm-embedded databases like pglite and sqlite
FluentMigrator.Mcp                <- thin: hosts an MCP server exposing Discover()/RunAsync as MCP tools, plus lightweight client contracts for talking to it
FluentMigrator.GitHubAction       <- thin: maps workflow with: inputs to a structured invocation; previews rather than applies
FluentMigrator.EFCoreMigrationGenerator <- thin: contributes a command that writes migrations from an EF Core model snapshot

FluentMigrator.Net.Sdk            <- MSBuild project SDK: a migrations project's source-controlled database model
FluentMigrator.Net.Sdk.Host       <- MSBuild project SDK: the composition root; selects hosting contexts and runners
```

## Build-time composition: `FluentMigrator.Net.Sdk.Host`

Everything above is about *runtime* dispatch — one command set, eight front
doors. There is a symmetric question at build time that the eight consumers
would otherwise each answer privately: given several migration assemblies,
which ones apply to which database, in what order, against which runner, and
where are their artifacts? Aspire's AppHost, the MSBuild task, and `dotnet fm`
all need that answer, and none of them can derive it from a connection string.

`FluentMigrator.Net.Sdk.Host` answers it once, at build time, and serializes the
result to `$(MSBuildProjectName).host.json`. It is the build-time counterpart of
`MigrationHost`, not a competitor to it: the SDK decides *what is to be hosted*,
`MigrationHost` decides *what is invoked*. Design rationale for the underlying
source model is in [`SourceControlledDatabaseModel.md`](SourceControlledDatabaseModel.md).

```xml
<Project Sdk="FluentMigrator.Net.Sdk.Host">
  <PropertyGroup>
    <HostContexts>dotnet-cli;aspire</HostContexts>
  </PropertyGroup>
  <ItemGroup>
    <ProjectReference Include="../Core/Core.csproj" />
    <ProjectReference Include="../Sales/Sales.csproj" />
  </ItemGroup>
  <ItemGroup>
    <DeploymentTarget Include="primary">
      <ConnectionStringName>Primary</ConnectionStringName>
      <Dialect>sqlserver</Dialect>
    </DeploymentTarget>
  </ItemGroup>
</Project>
```

The manifest records **contexts × targets**, and the two axes are independent:

- `hostContexts` — the front doors this host offers, one entry per consumer
  above, each carrying its adapter package, `referenceKind` (`package` for
  something the host references, `tool` for something invoked out of process),
  `invocation` shape, and whether it renders `Discover()` metadata. A context
  the ADR describes but that has not shipped yet is recorded as
  `availability: "planned"` — legal to select, so tooling can be built against
  it, and contributing no package reference until it exists.
- `targets` — each deployment target's ordered module plan, per-module
  VersionInfo table, source-model manifest and assembly, plus the
  `FluentMigrator.Runner.*` package resolved from that target's dialect. One
  host project can therefore drive a `sqlserver` target and a `postgres` target
  through the same `aspire` context.

Two consequences worth stating explicitly, because they close gaps the
consumer sections above would otherwise each have to fill:

**`Discover()` gains a build-time sibling.** The TUI, Aspire and Canvas
integrations are cheap because they ask the host what *commands* exist. They are
cheap for the same reason about what *databases and modules* exist — the
manifest is the discovery surface, and no consumer hardcodes a module list any
more than it hardcodes a command list.

**Declared order is the contract; the reference graph is the linter.**
`DeploymentTarget` document order wins unconditionally. MSBuild *build* order
stays graph-driven; *deploy* order stays declared. When they disagree the SDK
raises `FMSDK201` rather than silently reordering. This is the build-time
analogue of the Choice operator's rule above: deterministic, inspectable
precedence, never an implicit resolution.

The reserved `$(GenerateMigrationHost)` property is where the two halves meet —
generating an `OutputType=Exe` entry point over `MigrationHost` from the
manifest, so a host project's `Program.cs` is one more thing nobody writes by
hand. It is deliberately unimplemented until `FluentMigrator.Runner.Host` exists.

## Core abstractions

### The command

```csharp
namespace FluentMigrator.Runner.Host;

public interface IMigrationCommand
{
    string Name { get; }                 // "migrate:up", "migrate:down", "list", "validate", "rollback"
    string Description { get; }
    IReadOnlyList<MigrationArgument> Arguments { get; }

    // Returns a CommandOutcome, not a thrown exception — see Error presentation
    Task<CommandOutcome> ExecuteAsync(MigrationExecutionContext context, CancellationToken cancellationToken);
}
```

### Self-describing arguments

This is the load-bearing abstraction across every non-CLI consumer: instead of each host reinventing "what parameters does migrate:up take," every command declares them once, typed, with validation and defaults, and every adapter renders that metadata natively.

```csharp
public sealed record MigrationArgument(
    string Name,                       // "TargetVersion"
    string CliAlias,                   // "--target-version" / "-t"
    Type ValueType,                    // typeof(long?)
    string Description,
    bool Required = false,
    object? DefaultValue = null,
    Func<object?, ValidationResult>? Validate = null);
```

A concrete command:

```csharp
public sealed class MigrateUpCommand : IMigrationCommand
{
    public string Name => "migrate:up";
    public string Description => "Applies all pending migrations, or up to a target version.";

    public IReadOnlyList<MigrationArgument> Arguments { get; } =
    [
        new("TargetVersion", "--target-version", typeof(long?),
            "Migrate up to (and including) this version. Omit to migrate to latest."),
        new("Timeout", "--timeout", typeof(int?),
            "Command timeout in seconds.", DefaultValue: 60),
    ];

    public async Task<CommandOutcome> ExecuteAsync(MigrationExecutionContext ctx, CancellationToken ct)
    {
        var runner = ctx.ServiceProvider.GetRequiredService<IMigrationRunner>();
        var target = ctx.Arguments.Get<long?>("TargetVersion");

        if (target is { } version) await runner.MigrateUpAsync(version, ct);
        else await runner.MigrateUpAsync(ct);

        return CommandOutcome.Ok();
    }
}
```

`ctx.Arguments` is a small typed bag (`MigrationArgument` name → boxed value) populated by whichever adapter is driving — argv parser, MSBuild properties, Aspire command payload, Copilot Canvas request. The command itself never parses a string.

### The execution context

```csharp
public sealed class MigrationExecutionContext
{
    public required IServiceProvider ServiceProvider { get; init; }
    public required MigrationArgumentBag Arguments { get; init; }
    public required IMigrationRunnerConventions Conventions { get; init; }
    public DirectoryPath WorkingDirectory { get; init; } = DirectoryPath.Current;
}
```

### Startup / lifetime hooks

```csharp
public interface IMigrationStartup
{
    void ConfigureServices(IServiceCollection services);
}

public interface IMigrationLifetime
{
    void Setup(MigrationExecutionContext context);
    void Teardown(MigrationExecutionContext context, int exitCode);
}
```

### The host itself

```csharp
public sealed class MigrationHost
{
    private readonly IServiceCollection _services = new ServiceCollection();
    private readonly List<Action<IServiceCollection>> _configure = [];
    private readonly List<CommandFactoryRegistration> _factories = [];
    private readonly MutableCommandFactory _adHocCommands = new();

    public MigrationHost() => _factories.Add(new(_adHocCommands, CommandConflictPolicy.ComposeThrowOnConflict));

    public MigrationHost UseStartup<TStartup>() where TStartup : IMigrationStartup, new()
    {
        _configure.Add(s => new TStartup().ConfigureServices(s));
        return this;
    }

    public MigrationHost UseLifetime<TLifetime>() where TLifetime : IMigrationLifetime
    {
        _services.AddSingleton(typeof(IMigrationLifetime), typeof(TLifetime));
        return this;
    }

    /// Registers a command factory. See Command discovery for the default
    /// (compile-time, generated) factory and how sidecar-loaded factories compose.
    public MigrationHost UseCommandFactory(
        ICommandFactory factory,
        CommandConflictPolicy onConflict = CommandConflictPolicy.ComposeThrowOnConflict)
    {
        if (onConflict == CommandConflictPolicy.Replace)
            _factories.Clear();
        _factories.Add(new(factory, onConflict));
        return this;
    }

    /// Manual escape hatch — used by generated registration call sites, and as the
    /// Post-MVP fallback if cross-assembly interceptors don't fire reliably for
    /// Aspire/CopilotCanvas (see Post-MVP).
    public MigrationHost UseCommand<TCommand>() where TCommand : class, IMigrationCommand, new()
    {
        _adHocCommands.Add(new TCommand());
        return this;
    }

    // --- Terminal methods ---

    /// For DotNet.Cli / Console / TUI: resolve argv to a command + MigrationArgumentBag,
    /// then run it through the same structured path every other consumer uses.
    /// This method does no parsing itself — see Parsing for what actually does.
    public async Task<int> RunAsync(string[] args, CancellationToken ct = default)
    {
        var (commandName, argBag) = ResolveArgv(args);
        return await RunAsync(commandName, argBag, ct);
    }

    /// For MSBuild / Aspire / Copilot Canvas: invoke directly with structured arguments.
    /// No string parsing anywhere in this path — the argv path above funnels into it too.
    public async Task<int> RunAsync(string commandName, MigrationArgumentBag arguments, CancellationToken ct = default)
    {
        foreach (var c in _configure) c(_services);
        var provider = _services.BuildServiceProvider();

        var command = ResolveCommands().SingleOrDefault(c => c.Name == commandName)
            ?? throw new MigrationCommandNotFoundException(commandName);

        var context = new MigrationExecutionContext
        {
            ServiceProvider = provider,
            Arguments = arguments,
            Conventions = provider.GetRequiredService<IMigrationRunnerConventions>(),
        };

        var lifetime = provider.GetService<IMigrationLifetime>();
        lifetime?.Setup(context);
        var outcome = await command.ExecuteAsync(context, ct); // CommandOutcome — see Error presentation
        lifetime?.Teardown(context, outcome.ExitCode);
        return outcome.ExitCode;
    }

    /// For TUI / Aspire / Copilot Canvas: introspect what's available, without running anything.
    public IReadOnlyList<IMigrationCommand> Discover() => ResolveCommands();

    // Merges every registered factory's commands, applying each registration's
    // conflict policy — see Command discovery.
    private IReadOnlyList<IMigrationCommand> ResolveCommands() { /* ... */ throw null!; }

    // Delegates to the composed factories' parsers — see Parsing.
    private (string CommandName, MigrationArgumentBag Arguments) ResolveArgv(string[] args) { /* ... */ throw null!; }
}
```

`Discover()` is the piece that makes the TUI, Aspire, and Copilot Canvas integrations cheap: they never hardcode a list of "migrate up / migrate down / rollback," they just ask the host what commands exist and render the metadata.

## Command discovery: mechanism, policy, and sidecar loading

Two questions were bundled together under "command discovery" and are worth separating cleanly, because they have different answers:

- **Mechanism** — how does a command declared in source become something `MigrationHost` knows about, without runtime reflection? Answered by attribute + source-generator + interceptor: `[Command]`-attributed partial classes, discovered at compile time, registered via a generated interceptor that rewrites the host's entry-point call — no `Assembly.GetTypes()`, fully trim/AOT-safe. This is the *default* factory, and it's what every consumer gets with zero configuration.
- **Policy** — for `FluentMigrator.DotNet.Cli` specifically, should a user be able to add or replace commands at run time, without recompiling FluentMigrator itself? Yes — this is the sidecar-loading capability, deliberately scoped to the one consumer where it makes sense (a JIT-compiled, framework-dependent CLI), consistent with the Native AOT tradeoff already noted in Post-MVP: a Native AOT-published `dotnet-fm` cannot support this, because Native AOT cannot load an assembly it didn't know about at publish time. `--command-factory` is either unavailable in that build, or errors clearly if passed.

### `ICommandFactory` — the seam between "compiled in" and "loaded at run time"

```csharp
public interface ICommandFactory
{
    /// Commands this factory contributes. Used for Discover(), for composition
    /// with other factories, and — for factories that also parse — for building
    /// whatever the factory's own argument syntax needs.
    IReadOnlyList<IMigrationCommand> Commands { get; }
}

/// Optional: a factory that wants to own its own argv parsing (rather than
/// delegating to the default System.CommandLine-based resolver) implements this.
/// A factory that only implements ICommandFactory gets its commands parsed by
/// whatever the default resolver is doing — see Parsing.
public interface ICommandArgvParser
{
    bool TryResolve(IReadOnlyList<string> args, out string commandName, out MigrationArgumentBag arguments);
}
```

The generated default factory implements `ICommandFactory` only — its commands are parsed by the shared System.CommandLine-based resolver:

```csharp
// Generator output — compiled into the same assembly, no reflection anywhere.
public sealed class DefaultCommandFactory : ICommandFactory
{
    public static readonly DefaultCommandFactory Instance = new();

    public IReadOnlyList<IMigrationCommand> Commands { get; } =
    [
        new MigrateUpCommand(),
        new MigrateDownCommand(),
        new RollbackCommand(),
        // one entry per [Command]-attributed class found in this compilation
    ];
}
```

A sidecar factory that wants to "ditch the default argument syntax tree and replace it with their own" implements `ICommandArgvParser` too, and owns parsing for the commands it contributes.

### Composition and conflicts

```csharp
public enum CommandConflictPolicy
{
    Replace,                 // clears every previously registered factory
    ComposeThrowOnConflict,  // default — a duplicate command Name across factories fails fast at host construction
    ComposeFirstWins,
    ComposeLastWins,
}
```

`Replace` and `ComposeThrowOnConflict` cover "either replace the default factory or compose with it" directly. The two `*Wins` policies exist for the rarer case where a sidecar deliberately intends to override one specific built-in command (say, a custom `migrate:up` with extra validation) without disabling everything else the default factory provides — an explicit choice at registration time, never a silent default.

### Sidecar loading for `dotnet-fm`

```
dotnet fm --command-factory ./MyCompany.Migrations.Extensions.dll migrate up
```

`FluentMigrator.DotNet.Cli`'s `Program.Main` has to pre-scan `args` for `--command-factory` *before* the normal parse — the same two-phase problem `dotnet` itself has for options that change what the rest of the parser even knows about:

```csharp
public static class Program
{
    public static async Task<int> Main(string[] args)
    {
        var host = new MigrationHost().UseCommandFactory(DefaultCommandFactory.Instance);

        if (SidecarArgs.TryExtractCommandFactoryPath(args, out var path, out var remainingArgs))
        {
            var factory = SidecarCommandFactoryLoader.Load(path); // isolated, collectible AssemblyLoadContext
            host.UseCommandFactory(factory, factory.PreferredConflictPolicy);
            args = remainingArgs;
        }

        return await host.UseStartup<CliStartup>().RunAsync(args);
    }
}
```

`SidecarCommandFactoryLoader` loads the named assembly into its own collectible `AssemblyLoadContext` — not `Assembly.LoadFrom` into the default context — for isolation and to avoid the sidecar's own dependencies colliding with the host's. Locating the factory type inside that one assembly uses ordinary reflection: this is fine, because it's a single assembly the user explicitly named at the command line, not a broad scan — the AOT/trimming objection to `Assembly.GetTypes()` was about scanning *unknown* assemblies for discovery; here the assembly identity is a runtime input by design, so this code path is inherently JIT-only and is never compiled into a Native AOT build in the first place. Resolve the factory type by an explicit convention — a single public parameterless-constructible `ICommandFactory` implementation in the assembly, or an unambiguous `[assembly: CommandFactory(typeof(MyFactory))]` marker — and fail loudly if that's not satisfiable rather than silently picking one.

### The Choice operator — disambiguating composed commands

Once more than one factory can contribute commands, you need an answer for what happens when two factories' commands could plausibly both match a given input — the "word soup" problem. `[Choice]`/`ICommandChoice`-style compile-time expressibility is the right instinct: it makes ordered choice discoverable in source rather than buried in registration-order side effects, the same reason PEG's `/` operator or Maude's rewrite-rule priorities exist — deterministic, inspectable precedence instead of implicit backtracking.

Where it's worth narrowing the scope of the mechanism: full PEG-style backtracking composition — try factory A's whole grammar, fall through to factory B's whole grammar on failure — is a lot of machinery for a problem that, in practice, doesn't need it. Every multi-command CLI that already handles this well (`git`, `dotnet`, `kubectl`) resolves composition at exactly one point: the **leading verb** selects which sub-parser owns everything after it, and nothing below that point ever needs to merge grammars from two different sources. `git`'s own external-command mechanism is the cleanest existing example — a `git-foo` executable on `PATH` is invoked as `git foo` with zero ambiguity, because dispatch happens purely on the name, before `git-foo` ever sees its own arguments. FluentMigrator's version of that is command *names* being the choice space, not full argument grammars:

```csharp
public interface ICommandNameResolver
{
    /// Given every command contributed by every composed factory, and the
    /// leading tokens of argv, resolve exactly one command — or throw a
    /// clear ambiguity/conflict diagnostic. Never silently picks one.
    IMigrationCommand Resolve(IReadOnlyList<IMigrationCommand> candidates, IReadOnlyList<string> leadingTokens);
}
```

This reframes `[Choice]` from "a marker that something participates in disambiguation" into something with an actual algorithm behind it: an ordered list of factories (registration order, or an explicit `Priority` if that turns out to matter in practice) plus the `CommandConflictPolicy` from registration, applied once, at the command-*name* level. Once a name resolves to exactly one `IMigrationCommand`, that command's own `Arguments` — and, if it came from a factory implementing `ICommandArgvParser`, its own parser — owns everything after the name. Nothing has to merge two independently authored argument grammars, because nothing below the name-resolution boundary is shared. That keeps composition to "a lookup with an explicit conflict policy," not a parser-combinator problem, while still giving you the compile-time-expressible, extensible ordering `[Choice]` was reaching for.

## Parsing: from argv to a `MigrationArgumentBag`

Worth being precise about what `FluentMigratorCommandBuilder` (from PR #2284) actually does, since "builds the command tree" and "parses input" are easy to conflate — System.CommandLine keeps them deliberately separate:

1. **`FluentMigratorCommandBuilder.Build()`** constructs the `RootCommand` — the tree of `Command`/`Option`/`Argument` objects for every command the default factory contributes (migrate up/down, rollback, validate, list). This step does no parsing; it's pure tree construction, and for the generated commands it's driven by the same `[Command]`/`[Option]` attributes the interceptor-based registration uses, so the CLI's argument shape and the generated `MigrationArgument` metadata can't drift apart.
2. **`RootCommand.Parse(args, parserConfiguration)`** is where argv actually gets tokenized and bound — this is `Command.Parse`, part of System.CommandLine itself, not something FluentMigrator reimplements. It returns a `ParseResult`.
3. Each leaf `Command` has a bound action (System.CommandLine 2.0's `SetAction`) that reads the typed values off the `ParseResult`, builds a `MigrationArgumentBag` from them, and calls `MigrationHost.RunAsync(commandName, bag, ct)` — the same structured entry point MSBuild, Aspire, and Canvas all use. `RunAsync(string[] args, ct)` isn't a separate execution path; it's argv → System.CommandLine parse → `MigrationArgumentBag` → the one real terminal method.

For composed sidecar commands that stick with the default parser (implement `ICommandFactory` only, not `ICommandArgvParser`), their `[Command]`-declared shape gets folded into the same `RootCommand` tree at step 1 before `Parse` ever runs — which is exactly why name resolution has to happen at registration/tree-construction time (Command discovery), not after parsing has already committed to a tree. A factory that implements `ICommandArgvParser` bypasses this tree entirely for its own commands and is invoked directly once its command name is matched, per its own parsing logic.

`FluentMigrator.Console`'s legacy shim (`NormalizeArgs`, rewriting `/option` to `--option`) runs *before* step 2 — it's a text-to-text rewrite of `argv` itself, not a second parser, so it composes with everything above for free.

## Consumer 1: — FluentMigrator.DotNet.Cli (dogfooding)

```csharp
public static class Program
{
    public static Task<int> Main(string[] args) =>
        new MigrationHost()
            .UseCommandFactory(DefaultCommandFactory.Instance)
            .UseStartup<CliStartup>()
            .RunAsync(args);
}

public sealed class CliStartup : IMigrationStartup
{
    public void ConfigureServices(IServiceCollection services) =>
        services.AddFluentMigratorCore(); // existing extension, DB provider selected via config
}
```

This becomes the reference implementation everything else is checked against — if `dotnet fm migrate up --target-version 5` works, the Aspire command and the MSBuild task calling the same `migrate:up` command with the same argument bag are exercising identical code. The full version with `--command-factory` sidecar loading is in Command discovery; this is the plain form with only the compiled-in default.

## Consumer 2: — FluentMigrator.Console

Same shape, plus a compatibility shim mapping legacy flags (`/db`, `/connection`, `/target` etc.) onto the new `MigrationArgumentBag` before calling `RunAsync(commandName, bag)` directly — bypassing the new argv parser entirely so old scripts don't need to change:

```csharp
var bag = LegacyArgMapper.Map(args, out var commandName);
return await host.RunAsync(commandName, bag);
```

## Consumer 3: — FluentMigrator.MSBuild

MSBuild gives you `ITaskItem`/`string` properties, not argv — this is exactly why the structured `RunAsync(commandName, MigrationArgumentBag)` overload exists.

```csharp
public sealed class MigrateTask : Microsoft.Build.Utilities.Task
{
    public string Command { get; set; } = "migrate:up";
    public string? TargetVersion { get; set; }
    public string Connection { get; set; } = "";

    public override bool Execute()
    {
        // SetRaw goes through the shared coercion path (Argument coercion) —
        // no direct long.Parse here, and "" is treated as absent, not invalid.
        var bag = new MigrationArgumentBag()
            .SetRaw("TargetVersion", TargetVersion, typeof(long?))
            .Set("ConnectionString", Connection);

        var host = new MigrationHost()
            .UseCommandFactory(DefaultCommandFactory.Instance)
            .UseStartup<MsBuildStartup>();

        var exitCode = host.RunAsync(Command, bag).GetAwaiter().GetResult();
        return exitCode == 0;
    }
}
```

## Consumer 4: — FluentMigrator.TUI (Spectre.Console)

Built entirely from `Discover()` — no hand-maintained menu:

```csharp
var host = new MigrationHost().UseCommandFactory(DefaultCommandFactory.Instance).UseStartup<CliStartup>();
var commands = host.Discover();

var chosen = AnsiConsole.Prompt(
    new SelectionPrompt<IMigrationCommand>()
        .Title("Select a migration operation")
        .UseConverter(c => $"{c.Name} — {c.Description}")
        .AddChoices(commands));

var bag = new MigrationArgumentBag();
foreach (var arg in chosen.Arguments)
{
    var value = AnsiConsole.Ask<string>($"{arg.Description} [{arg.Name}]:", arg.DefaultValue?.ToString() ?? "");
    bag.SetRaw(arg.Name, value, arg.ValueType);
}

await host.RunAsync(chosen.Name, bag);
```

Adding a new built-in command later (say `preview:sql`) means the TUI menu grows automatically — nothing in `FluentMigrator.TUI` needs to change.

## Consumer 5: — FluentMigrator.Aspire

This is the one directly modeled on EF Core's Aspire integration (`WithReference` + migration-on-startup hooks), except you're going one step further and exposing each command as an interactive Aspire dashboard **Command** (the same mechanism Aspire uses for things like "Restart," "Clear Cache" on resources), rather than only running migrations at startup.

```csharp
public static class FluentMigratorResourceBuilderExtensions
{
    public static IResourceBuilder<T> WithFluentMigratorCommands<T>(
        this IResourceBuilder<T> builder, MigrationHost host)
        where T : IResourceWithConnectionString
    {
        foreach (var command in host.Discover())
        {
            builder.WithCommand(
                name: command.Name,
                displayName: command.Description,
                executeCommand: async ctx =>
                {
                    var bag = MigrationArgumentBag.FromAspireCommandParameters(ctx.Parameters);
                    var exitCode = await host.RunAsync(command.Name, bag, ctx.CancellationToken);
                    return exitCode == 0
                        ? CommandResults.Success()
                        : CommandResults.Failure($"Exit code {exitCode}");
                },
                commandOptions: new CommandOptions
                {
                    // arg metadata -> Aspire dashboard's parameter prompt
                    Parameters = command.Arguments.Select(a => new CommandParameter(a.Name, a.Description, a.Required)).ToArray()
                });
        }
        return builder;
    }
}
```

Since `MigrationArgument` already carries `Name`, `Description`, `Required`, `ValueType`, mapping it to whatever shape Aspire's `WithCommand` parameters API wants is mechanical — that mapping is the *only* Aspire-specific code you write.

## Consumer 6: — FluentMigrator.CopilotCanvas

Copilot Canvas needs a declarative description of "what can I show the user" plus a way to invoke it — no argv, no MSBuild, just structured requests (likely over MCP, given Copilot's agentic tooling model). Same pattern again:

```csharp
// Schema generation for Canvas's dashboard UI
app.MapGet("/fluentmigrator/commands", (MigrationHost host) =>
    host.Discover().Select(c => new
    {
        c.Name,
        c.Description,
        Arguments = c.Arguments.Select(a => new
        {
            a.Name, a.CliAlias, a.Description, a.Required,
            Type = JsonSchemaType(a.ValueType),
            a.DefaultValue
        })
    }));

// Invocation
app.MapPost("/fluentmigrator/commands/{name}", async (string name, MigrationCommandRequest req, MigrationHost host) =>
{
    var bag = MigrationArgumentBag.FromJson(req.Arguments);
    var exitCode = await host.RunAsync(name, bag);
    return Results.Ok(new { exitCode });
});
```

If you're leaning into MCP specifically (fits nicely with "agentic dev environment"), the same `Discover()` output maps directly onto MCP tool definitions — each `IMigrationCommand` becomes an MCP tool, each `MigrationArgument` becomes a tool input schema property. This is exactly what Consumer 8 below builds as its own package — worth revisiting this section once that exists, since Canvas's data/execution layer becoming an MCP client of `FluentMigrator.Mcp` removes the need to maintain this bespoke HTTP schema at all.

## Consumer 7: — FluentMigrator.Repl

Modeled on [PhenX's FluentMigrator.Repl.Poc](https://github.com/PhenX/FluentMigrator.Repl.Poc): a Blazor WebAssembly *headless* backend (no rendered UI of its own) paired with a Vue.js + Monaco Editor frontend, talking to each other over a JS interop bridge — Vue calls `migrationInterop.invokeMethodAsync('ExecuteMigrationAsync', code)`, Blazor runs real `FluentMigrator.Runner` code against an in-browser database, and results flow back the same way. The package layout entry generalizes the PoC's SQLite-only proof (`Microsoft.Data.Sqlite` + the `wasm-tools` workload) to also target pglite for in-browser Postgres.

This consumer looks the least like the other seven at first glance — no argv, no request/response cycle, a long-lived SPA instead of a process-per-invocation model — but it still fits the same three pieces from Design goals, just with an unusual adapter:

**The host is long-lived, not process-per-invocation.** `MigrationHost` gets registered once in Blazor's DI container and reused across every "Run" click in the session, unlike every other consumer here which constructs one, runs one command, and exits. This is exactly the DI-scope point from Async execution: each individual run still needs its own scope for a fresh, disposed-on-completion `DbContext`/connection, even though the host object itself outlives any single run.

**Two distinct execution modes map onto the same terminal method.** The "batteries included" mode is identical to the TUI: `host.Discover()` populates a picker of known commands (`migrate:up`, `list`, etc.), a form-like widget collects arguments into a `MigrationArgumentBag`, and `host.RunAsync(name, bag, ct)` runs it — the JS interop bridge is just another thin adapter here, the same role MSBuild's `ITask` or Aspire's `WithCommand` play elsewhere. The genuinely new mode is Monaco's free-form editor, where the user types an entire `Migration` subclass rather than filling in a known command's arguments. Modeling that as its own command keeps it inside the same pipeline instead of forking a separate execution path:

```csharp
public sealed class ExecuteSourceCommand : IMigrationCommand
{
    public string Name => "execute:source";
    public string Description => "Compiles and runs a Migration class from C# source.";

    public IReadOnlyList<MigrationArgument> Arguments { get; } =
    [
        new("Source", "--source", typeof(string),
            "C# source defining a class deriving from Migration.", Required: true),
    ];

    public async Task<CommandOutcome> ExecuteAsync(MigrationExecutionContext ctx, CancellationToken ct)
    {
        var source = ctx.Arguments.Get<string>("Source")!;
        var compiled = await MigrationScriptCompiler.CompileAsync(source, ct); // Roslyn scripting
        if (compiled is not { } migration)
            return CommandOutcome.Fail("Compilation failed — see diagnostics.");

        var runner = ctx.ServiceProvider.GetRequiredService<IMigrationRunner>();
        await runner.MigrateUpAsync(migration, ct); // hypothetical overload taking a Migration instance directly

        return CommandOutcome.Ok();
    }
}
```

Vue's "Run" button ends up calling `host.RunAsync("execute:source", MigrationArgumentBag.FromJson(jsonArgs), ct)` on the Blazor side — the same structured entry point every other consumer uses, with a single string-valued argument carrying the user's source instead of a typed value.

**The AOT/JIT tension resurfaces here, and it's the same constraint as sidecar loading.** Compiling and loading arbitrary user-typed source at run time needs Roslyn's scripting APIs plus dynamic assembly loading, which works today because Blazor WebAssembly runs on the Mono interpreter by default. If the REPL is ever published with `<RunAOTCompilation>true</RunAOTCompilation>` for load-time performance, `execute:source` specifically stops working — the identical "framework-dependent/JIT gets the dynamic capability, AOT doesn't" tradeoff already documented for `--command-factory` in Command discovery, just inside the browser instead of on the CLI. Worth documenting as a known limitation up front rather than discovering it the day someone flips that switch.

**Data access is two different problems wearing one name.** SQLite works today, per the PoC, via `Microsoft.Data.Sqlite` and the `wasm-tools` workload. pglite (Postgres compiled to WASM) is materially different: it's a JS/WASM module, not something a normal ADO.NET driver opens a socket to, so an `IMigrationProcessor`/`DbConnection` implementation for it has to marshal every command through JS interop rather than a wire protocol. There's no existing FluentMigrator-compatible provider that does this — it's real, unscoped engineering, and belongs in Post-MVP as its own spike rather than being assumed to "just work" alongside the SQLite path.

**Output can actually use `ILogger`, unlike MSBuild.** Blazor supports a custom `ILoggerProvider` that forwards to the JS interop bridge instead of the console — so `CommandOutcome` → `ILogger` → JS interop → Vue's output panel reuses the same `ILogger`-based adapter DotNet.Cli/Console/Aspire already have, with only the sink swapped out.

**Cancellation needs to be more cooperative than any server-side host.** WASM runs cooperatively scheduled on the UI thread by default — a "Cancel" button sets a `CancellationToken`, but a long migration loop has to check it and `await Task.Yield()` periodically, or the browser tab visibly freezes. That's a stricter requirement than any of the other seven consumers have, worth calling out explicitly as a REPL-specific implementation constraint layered on top of the general async design.

## Consumer 8: — FluentMigrator.Mcp

MCP servers in .NET are built on the official `ModelContextProtocol` SDK: `[McpServerToolType]`/`[McpServerTool]`-attributed methods, registered via `AddMcpServer().WithToolsFromAssembly()`, exposed over either a `stdio` transport (a local subprocess — the natural fit for Claude Code, Copilot's CLI agents, or any locally-run agentic tool) or an HTTP/streamable transport via `ModelContextProtocol.AspNetCore` (`WithHttpTransport()` + `app.MapMcp()` — the natural fit for a shared, remote server, including being hosted as another resource inside an Aspire AppHost).

Worth flagging immediately: the SDK's own default registration path, `WithToolsFromAssembly()`, is exactly the reflection-based assembly scan the rest of this document has been steering away from for AOT/trimming reasons. `FluentMigrator.Mcp` shouldn't use it — the SDK also supports registering tools programmatically (confirm the exact API surface at implementation time), which is the right fit anyway, since FluentMigrator's tools aren't compile-time-attributed methods on a fixed class — they're `Discover()`'s runtime list of `IMigrationCommand`s, which can include sidecar-loaded ones a static scan would never see. Every registered command becomes one MCP tool, built the same way Consumer 6's `/fluentmigrator/commands` schema already does:

```csharp
public static class McpToolRegistration
{
    public static void RegisterTools(IMcpServerBuilder builder, MigrationHost host)
    {
        foreach (var command in host.Discover())
        {
            builder.WithTool(
                name: McpToolName.From(command.Name), // "migrate:up" -> "migrate_up" — see naming note below
                description: command.Description,
                inputSchema: BuildJsonSchema(command.Arguments), // same JsonSchemaType(a.ValueType) helper as Consumer 6
                handler: async (arguments, ct) =>
                {
                    var bag = MigrationArgumentBag.FromJson(arguments);
                    var outcome = await host.RunAsync(command.Name, bag, ct); // CommandOutcome — see Error presentation
                    return McpToolResult.From(outcome); // { success, exitCode, message } — same shape as the CopilotCanvas row
                });
        }
    }
}
```

That handler body is close to identical to Consumer 6's `MapPost` handler — worth acting on rather than leaving as a coincidence. **Revise Consumer 6 to consume this package** rather than maintain a parallel bespoke HTTP schema: `FluentMigrator.CopilotCanvas` becomes the dashboard UI specifically for GitHub Copilot's agentic environment, and its data/execution layer is just an MCP client talking to `FluentMigrator.Mcp` — one real implementation of "expose commands over a protocol," reused instead of duplicated. If Canvas needs presentation details MCP's tool schema doesn't carry (icons, grouping, layout hints), those become Canvas-specific annotations layered on top of the MCP tool list, not a second source of truth for what commands and arguments exist.

A few things specific to the MCP surface:

- **Tool naming needs an explicit, documented mapping.** MCP tool names are typically more restrictive than FluentMigrator's `Name` values — `migrate:up` isn't necessarily a valid tool identifier. `migrate:up` → `migrate_up` (or similar) has to be a documented, stable convention an agent reading tool names can rely on, not an implicit one that could change between releases.
- **Schema versioning ties directly into Schema versioning, applied.** The MCP tool list is another rendering of the same `schema/v1/commands.json` manifest — generate the tool registration from that manifest rather than from an independent call to `Discover()`, so the two can't drift apart. MCP's own `initialize` handshake carries server info (name/version); surface the schema version there too, so a client can detect a mismatch the same way it would against the self-hosted release index.
- **Cancellation and logging are closer to first-class here than for most consumers.** MCP has an explicit cancellation notification (maps directly onto `CancellationToken`) and a logging capability (structured log messages sent to the client) — meaning `FluentMigrator.Mcp` can plausibly get the richest `CommandOutcome` translation of any consumer: not just a final `{ success, message }`, but progress notifications streamed while a long migration runs, which is a genuinely good fit for an agent that's expected to narrate what it's doing.
- **Tools aren't the only MCP primitive.** Resources (readable data) and Prompts (reusable templates) are natural Post-MVP extensions once Tools are solid — a "pending migrations" Resource an agent can read without invoking a command, or a Prompt template for "help me write a migration that does X" leaning on FluentMigrator's own conventions. Worth keeping in view given this package's origin in a broader AI-assistance conversation, but scoping the first cut to Tools, where the direct `Discover()`/`RunAsync` mapping already does most of the work.

## Consumer 9: — FluentMigrator.GitHubAction

The front door here is a workflow step, and its inputs arrive as `with:` entries that the Actions runner hands over as environment variables — structured key/value pairs, never argv. That makes it the same adapter shape as MSBuild: map the inputs onto a `MigrationArgumentBag` and call `RunAsync(commandName, bag, ct)`.

What makes it worth calling out separately is that it is deliberately *thin in scope as well as in code*: its job is **preview**, not application. A pull request wants to see the SQL a merge would run, and it wants to see it without anything being granted permission to run it. No new command is needed — preview is an argument on the commands that already exist — so the whole package is an input mapping plus a renderer:

```yaml
- uses: fluentmigrator/preview-action@v1
  with:
    command: migrate:up
    assembly: ./artifacts/MyApp.Migrations.dll
    dialect: sqlserver
```

The sharp edge worth designing for is credentials. FluentMigrator's connectionless mode already generates a script with no connection string at all, so the preview path should default to it: a pull request from a fork then produces a reviewable script without a database secret ever entering CI. Preview *with* a connection remains available for the cases that need the live schema to generate faithfully, but it should be the opt-in, not the default — the reverse of that arrangement is how preview jobs quietly become the most over-privileged step in a pipeline.

Output is where this consumer differs most from the CLI. GitHub gives two distinct surfaces and `CommandOutcome` maps onto both: the generated script belongs in `$GITHUB_STEP_SUMMARY` as a fenced block, where a reviewer reads it as part of the check, while failures additionally emit `::error::` workflow commands so they surface as annotations against the diff. The exit code still decides whether the step passes, so the existing `ExitCode` contract needs no special casing.

## Consumer 10: — FluentMigrator.EFCoreMigrationGenerator

Every consumer above *runs* a migration. This one **writes** one: it reads an EF Core model snapshot and emits FluentMigrator migration source, so a team already doing code-first modelling keeps `dotnet ef migrations add` as the authoring gesture while FluentMigrator owns application, ordering, and the version table.

```csharp
public sealed class GenerateFromModelCommand : IMigrationCommand
{
    public string Name => "generate:migration";
    public string Description => "Emits a FluentMigrator migration from an EF Core model snapshot.";
    // arguments: ModelAssembly, ContextType, OutputDirectory, MigrationName, Version
}
```

This is the consumer that stretches the abstraction, and that is precisely why it belongs in the document rather than being discovered later. Three consequences:

**A command must be able to run without a database.** `MigrationExecutionContext` gives a command an `IServiceProvider` and `IMigrationRunnerConventions`; a generator needs the conventions — that is where migration naming and version-number policy live, and generated files must obey the same ones a hand-written migration does — but it has no use for an `IMigrationProcessor` and no connection to open. So resolving a processor has to remain the *command's* choice, not something the host does eagerly on every invocation. A host configured with no provider at all should still be able to run `generate:migration`.

**The outcome is files, not statements.** `CommandOutcome` currently answers "did it work and what do I print". A generator's useful result is the set of paths it wrote, which a CLI prints, a GitHub Action turns into a diff, and an agent reads before deciding what to do next. That argues for the outcome carrying produced artifacts rather than each consumer scraping them out of log lines.

**Generation is idempotent in a way execution is not.** Re-running the generator against an unchanged model should produce no new migration, which makes it safe to put in a pre-commit hook or a CI check that fails when the model and the migrations have drifted apart. That check is the real prize here — model-versus-migration drift is otherwise found at deploy time.

It also closes a loop with the source-control model: generated migrations land in the `Migrations/` pivot, so the next build classifies and checksums them exactly like hand-written ones, and nothing downstream needs to know which of the two a given migration was.

## Why this holds up as FluentMigrator evolves

- **New built-in command → zero consumer changes.** Add `GenerateSqlScriptCommand` to `FluentMigrator.Runner.Commands`, and DotNet.Cli, TUI, Aspire, Copilot Canvas, the REPL, and the MCP server all pick it up via `Discover()`. Only Console/MSBuild/GitHubAction need explicit opt-in if you want it exposed there (reasonable, since those are argument-shaped tools already). The EF Core generator is that same property running backwards: it *contributes* a command, and every discovery-driven consumer gains it without that package knowing they exist.
- **Argument metadata is the single source of truth for validation.** Put `Validate` funcs on `MigrationArgument` once; argv parser, MSBuild task, Aspire dashboard, and Canvas all reject bad input the same way, with the same message.
- **Testing surface shrinks.** You test `IMigrationCommand` implementations once, against `MigrationExecutionContext`, independent of any host. Consumer-specific tests reduce to "does argv/ITaskItem/Aspire params map to the right `MigrationArgumentBag`."
- **Third-party commands become a real extension point** — a plugin package can ship an `ICommandFactory`, and `UseCommandFactory` composes it into any host, including ones you didn't write (community TUI themes, a hypothetical VS Code extension, etc.), with sidecar loading at run time available specifically where it makes sense (Command discovery).

## Post-MVP

Items deliberately deferred past the initial `MigrationHost` implementation — real, but not blocking for a first cut.

### Cross-assembly interceptors for `FluentMigrator.Aspire` / `FluentMigrator.CopilotCanvas`

This is specifically about the *compile-time generated default factory* (Command discovery); it's unrelated to the sidecar `ICommandFactory` loading mechanism, which is runtime/reflection-based by design and unaffected by anything below.

The command-discovery design leans on C# interceptors to rewrite a call like `new MigrationHost().Run(args)` into a fully specialized, statically-bound dispatch, with the source generator running in the same compilation as the attributed `[Command]` classes and the call site it rewrites.

That's a safe assumption for `FluentMigrator.DotNet.Cli`, `FluentMigrator.Console`, and `FluentMigrator.TUI` — the commands and the entry point live in the same project. It stops being a safe assumption for `FluentMigrator.Aspire` and `FluentMigrator.CopilotCanvas`, where the natural shape is: FluentMigrator ships the generator and the attributes, and a *consumer's* project (their AppHost, their Canvas-integrated repo) declares `[Command]`-attributed types and writes the call the generator is supposed to intercept. That's interception across an assembly boundary — the generator runs when the consumer's project builds, not when `FluentMigrator.Aspire` itself builds, and the call site being rewritten lives in code we don't control and can't see at the time our package is built.

This needs to be prototyped early, not discovered late, because it changes where the generator ships from (as an analyzer/generator package referenced by the consumer, not bundled invisibly inside a runtime library) and may constrain what the consumer is allowed to write for the interception to line up. Concretely, before committing the Aspire/Canvas packages to this pattern:

- Spike a minimal repro: generator + attributes in package A, `[Command]`-attributed class and the `Run(args)` call in package B referencing A, confirm the interceptor actually fires across that boundary under a Native AOT publish.
- If it doesn't fire reliably, the fallback isn't a redesign — it's dropping to the plain `UseCommand<T>()` / `UseGeneratedCommands()` explicit-registration path for just these two consumers, while DotNet.Cli/Console/TUI keep the fully-intercepted zero-boilerplate path. That's an acceptable asymmetry; it shouldn't be an accidental one discovered during the Aspire package's first release.
- Track this against the Roslyn interceptors feature maturing through .NET SDK releases — cross-assembly support is exactly the kind of gap likely to close over time rather than a permanent limitation, so this is worth re-testing each SDK cycle rather than designing around once and forgetting.

### A pglite (wasm-embedded Postgres) provider for `FluentMigrator.Repl`

Real, unscoped engineering: a JS/WASM-hosted Postgres has no socket for a conventional ADO.NET driver to open, so an `IMigrationProcessor`/`DbConnection` for it has to marshal commands through JS interop instead. Needs its own spike before it's promised alongside the already-proven SQLite path — see Consumer 7.

### Consolidating `FluentMigrator.CopilotCanvas` onto `FluentMigrator.Mcp`

Once `FluentMigrator.Mcp` exists, Canvas's bespoke `/fluentmigrator/commands` HTTP schema (Consumer 6) is redundant with the MCP tool list (Consumer 8) — same underlying `Discover()`/`RunAsync` mapping, done twice. Worth deciding explicitly whether Canvas becomes an MCP client of the Mcp package rather than maintaining a second schema, and if so, sequencing which package ships first.

## Resolved design decisions

The three open questions below on async execution, argument coercion, and schema versioning have moved from "open" to "decided, with follow-up work spelled out." Command discovery (see Design questions — resolution log below) is now resolved as well.

### Async execution: DI scope and `Execute.WithConnectionAsync`

Pushing async into the runner isn't only a processor-layer concern (see the Post-MVP-adjacent decision on `IAsyncMigrationProcessor`) — `Execute.WithConnectionAsync` needs to support migrations that inject real application services, not just raw ADO.NET calls. The concrete example that drove this: a data migration composing an EF Core async repository to backfill or transform rows, which is precisely the kind of thing `Execute.WithConnection` exists for today, just without an async-friendly path.

Two consequences for the design:

- **Connection/transaction sharing.** A migration that constructs an EF Core `DbContext` inside `Execute.WithConnectionAsync` must have that context join the *same* connection and transaction the runner already opened (`UseSqlServer(existingConnection)` + `Database.UseTransaction(existingTransaction)`), not open a second one. The delegate's connection/transaction parameters need to be usable this way, not assumed to be consumed only via raw `SqlCommand`.
- **DI scope, not just a delegate.** For a migration to resolve `IWidgetRepository` (or any other injected service) with its own dependencies wired correctly, the delegate needs access to the same `IServiceProvider` the runner itself came from:

```csharp
Execute.WithConnectionAsync(async (connection, transaction, services, ct) =>
{
    var repo = services.GetRequiredService<IWidgetRepository>(); // EF Core-backed, async
    await repo.BackfillLegacyStatusAsync(ct);
});
```

This means the DI container backing a migration run needs to be a proper **scope** — a scoped `DbContext` per migration run, not a singleton one that outlives its connection — rather than the flatter singleton-heavy container implied by the original sketch. Treat this as a concrete DI-lifetime decision to make explicitly when `MigrationHost`'s container setup is implemented, not something to discover once a migration author hits a disposed-context exception in production.

### Argument coercion, worked through scenario by scenario

The single shared `MigrationArgumentBag.SetRaw(name, rawValue, type)` conversion policy needs to cover four concrete failure modes, not just "be consistent":

**Culture.** Standardize on `CultureInfo.InvariantCulture` everywhere a value is parsed, full stop. This is a **breaking change** for anyone currently running in a non-invariant locale where `long.Parse`/`int.Parse` happened to behave differently (thousands separators, decimal points) — a previously-working invocation could start failing, or vice versa. Document it explicitly in the migration guide with a before/after example, not just a changelog line, since this is the kind of breaking change that's silent until someone's CI runs on a differently-configured agent.

**Missing vs. empty — mapping the MSBuild task as a thin scaffold onto the shared `Command` class.** The instinct is right and generalizes the "one coercion path" principle: MSBuild `ITask` properties get mapped onto the same `FluentMigrator.Runner.Commands` `Command`/`MigrationArgument` types the CLI uses, and the source generator scaffolds only the mapping glue, not any parsing logic. What that scaffold has to bridge, because MSBuild's type system is narrower than `MigrationArgument.ValueType`:

- **No nullable numerics.** MSBuild task properties don't support `long?` directly — only `string`, `bool`, `ITaskItem`, `ITaskItem[]`, and a handful of others. The scaffold exposes `string TargetVersion { get; set; }` on the `ITask` and performs the empty-string-means-absent translation *before* handing off to the shared coercion function — which is the right place for that translation to live regardless.
- **MSBuild bools are `"True"`/`"False"`**, not `"true"`/`"false"`. `bool.TryParse` accepts both case-insensitively by default, but this deserves an explicit test case rather than relying on that being remembered.
- **Connection strings and semicolons.** ADO.NET connection strings routinely contain `;`, and MSBuild uses `;` as an item-list separator in several contexts. A plain property assignment is usually safe, but the string needs `%3B` escaping the moment it flows through anything that treats it as an item list. This is the single most likely "worked on the CLI, broke in the build file" bug report this design will get — worth an explicit regression test in the MSBuild package.
- **`[Required]` bypasses the centralized error path entirely.** If a required argument's corresponding MSBuild property is marked `[Required]`, the MSBuild *engine* fails the build before `Execute()` runs — before the shared `CommandOutcome`/validation path ever sees it (see "Error presentation" below). That's not necessarily wrong, but it's a real per-argument choice between engine-level required-ness (terser, bypasses your error formatting) and validation-time required-ness (consistent message, fails later) — the scaffold generator needs to make that choice per argument, not by convention.
- **Multi-valued arguments** map to `ITaskItem[]`, which carries metadata beyond a raw value — something `MigrationArgument` has no slot for today. Only matters if/when a command needs "value plus structured metadata" (e.g., an assembly path plus a tag filter scoped to it); flagging it now so it's a deliberate future extension rather than a surprise.
- **Incremental build caching.** A migration task must never declare `Inputs`/`Outputs` — MSBuild tasks without them always run, which is the correct behavior here. State this explicitly in the scaffold so nobody "helpfully" adds incremental support later and silently causes migrations to be skipped.

**Format edge cases — audit plus a standing guardrail, not a one-time fix.** The audit target is narrower than "every `Parse` call in the codebase": migration version numbers embedded via `[Migration(20240101120000)]` are compile-time constants, never parsed from a string, and are out of scope. In scope is anywhere a string that *originated from user input* (CLI args, MSBuild properties, an Aspire command payload, a Canvas request body) gets converted to a typed value — `--target-version`, `--steps`, `--timeout`, and any future `MigrationArgument`-typed option. The durable fix isn't the one-time grep — it's a Roslyn analyzer banning direct `long.Parse`/`int.Parse`/`Convert.ToInt64`/etc. outside the coercion module, enforced in CI, so a future contributor can't reintroduce a bypass without realizing there's a shared function they were supposed to use.

**Error presentation — a `CommandOutcome` result type instead of try/catch, translated per host.**

```csharp
public readonly struct CommandOutcome
{
    public bool Success { get; }
    public int ExitCode { get; }
    public string? Message { get; }          // user-facing, already formatted
    public Exception? Exception { get; }      // present only for unexpected failures, not validation failures

    public static CommandOutcome Ok(int exitCode = 0) => new(true, exitCode, null, null);
    public static CommandOutcome Fail(string message, int exitCode = 1) => new(false, exitCode, message, null);
    public static CommandOutcome Unexpected(Exception ex) => new(false, 1, ex.Message, ex);
}

public interface IMigrationCommand
{
    // ...
    Task<CommandOutcome> ExecuteAsync(MigrationExecutionContext context, CancellationToken ct);
}
```

This is the right call specifically because several heterogeneous consumers each need to render the same failure differently — a thrown exception forces every layer above the throw site to catch-and-translate, which is exactly the kind of per-adapter divergence the argument-coercion problem already illustrated.

On resurrecting something like the old `IAnnouncer`: the lesson from its collapse into `ILogger` isn't "never build a parallel abstraction again" — `IAnnouncer` folding into `ILogger` was reasonable when there was one output surface. What's different now is that one consumer (MSBuild) has no `ILogger` path at all, and several more (Aspire, Canvas, MCP) have a native output shape that's structured data, not log lines. The fix is putting `CommandOutcome` (structured, not narrated text) above `ILogger`, with one adapter per host translating it — `ILogger` stays exactly where it already fits, this adds a translation layer above it rather than replacing it.

| Consumer | Native output mechanism | `CommandOutcome.Success` → | `CommandOutcome.Fail`/`Unexpected` → | Notes |
|---|---|---|---|---|
| **DotNet.Cli** | `ILogger` (console formatter) + process exit code | Log at `Information`, exit `0` | Log at `Error`, exit `ExitCode` | Baseline adapter; everything else is compared against this |
| **Console** | `ILogger`, legacy text format | Same as DotNet.Cli, reformatted for back-compat | Same | Shares the `ILogger`-based adapter with DotNet.Cli; only the formatter differs |
| **MSBuild** | `TaskLoggingHelper` (`Log.LogMessage`/`LogWarning`/`LogError`) — no `ILogger` | `Log.LogMessage(MessageImportance.Normal, message)`, `Execute()` returns `true` | `Log.LogError(message)`, `Execute()` returns `false` | No first-party `ILogger` bridge exists; write a thin `ILoggerAdapter : ILogger` wrapping `TaskLoggingHelper` so command code stays `ILogger`-shaped and only this adapter knows about `TaskLoggingHelper` |
| **TUI (Spectre.Console)** | `AnsiConsole` renderables | `AnsiConsole.MarkupLine` success panel | Styled error panel; use `CommandOutcome.Exception` for Spectre's exception renderer when present | Richer diagnostic when `Exception` is set, plain message otherwise |
| **Aspire** | `CommandResults.Success()`/`Failure(message)` + OpenTelemetry logs to the dashboard | `CommandResults.Success()`, plus an OTel log event | `CommandResults.Failure(message)`, plus an OTel error event | Aspire's own logging is already OTel/`ILogger`-based via `ServiceDefaults`, so this reuses the `ILogger` adapter and additionally maps to `CommandResults` |
| **CopilotCanvas** | Structured JSON / MCP tool result content | `{ "success": true, "exitCode": 0 }` | `{ "success": false, "message": ..., "exitCode": ... }`, `isError: true` if MCP | Exception details go in an optional `debug` field, not the primary message — Canvas is agent-facing, the message should be directly actionable; see Post-MVP on consolidating onto `FluentMigrator.Mcp` |
| **Repl** | `ILogger` via a custom provider forwarding to the JS interop bridge | `ILogger.LogInformation`, forwarded to Vue's output panel | `ILogger.LogError`, forwarded the same way, with `CommandOutcome.Exception` available for a stack-trace-style detail view | Reuses the `ILogger`-based adapter with only the sink swapped — see Consumer 7 |
| **Mcp** | MCP tool result content + MCP's logging/progress notifications | `{ success: true, exitCode: 0 }`, optionally preceded by progress notifications during a long run | `{ success: false, message: ..., exitCode: ... }`, `isError: true` | Closest to first-class of any consumer, since MCP has explicit cancellation and logging capabilities — see Consumer 8 |
| **GitHubAction** | `$GITHUB_STEP_SUMMARY` markdown + `::error::` workflow commands + the step exit code | Script rendered into the job summary, step exits `0` | `::error::` annotation against the diff, step exits `ExitCode` | Preview-only by design, and connectionless by default, so no database secret need enter CI — see Consumer 9 |
| **EFCoreMigrationGenerator** | `ILogger`, plus the generated files themselves | Paths written, logged at `Information`, exit `0` | Log at `Error`, exit `ExitCode`, leaving nothing partially written | The only consumer whose result is artifacts rather than statements — see Consumer 10 |

The row that matters most architecturally is MSBuild's — it's the only consumer with no `ILogger` path at all. Every other host can share one `ILogger`-based adapter and differ only in formatting or in also producing a secondary structured result (`CommandResults`, JSON). That argues for building exactly one non-`ILogger` adapter (`TaskLoggingHelper`), not a bespoke logging abstraction spanning every consumer.

### Schema versioning, applied

**Version the schema, not the package — using Package Validation for the compiled-API half of the problem.** `EnablePackageValidation` (backed by `Microsoft.DotNet.ApiCompat`) diffs your compiled assembly's public API against a baseline version and fails the build on incompatible changes. Because the source generator makes each `[Option]` a real public C# property on a generated partial command class, ApiCompat genuinely catches a removed or retyped option — a nice emergent benefit of the attribute-driven design. But it validates the *compiled .NET API surface* only; it has no visibility into the serialized JSON manifest or MCP tool schema handed to Aspire and Canvas, which are cross-process, often non-.NET consumers reading a document rather than referencing the assembly. A C# rename could be ApiCompat-breaking while the JSON manifest (keyed off `[Option("--target-version")]`'s CLI alias rather than the property name) doesn't move, and vice versa — a manifest-only change with no corresponding C# API change. **Use both**: Package Validation for the compiled-API population (plugin authors, MSBuild task writers referencing `FluentMigrator.Runner.Commands.dll` directly), and a separate manifest diff for the serialized-schema population (Aspire, Canvas, MCP clients, anything reading the JSON over the wire).

**AI skill files as a companion build artifact.** Once the manifest is a build artifact (below), generating a `SKILL.md`-shaped companion from the same source of truth is close to free — same generator, same trigger, different output format. Version it alongside the JSON rather than as a separately-lifecycled artifact: `schema/v1/commands.json` + `schema/v1/SKILL.md`, so an agent reading the skill file and a tool reading the JSON are always describing the same generation.

**CI enforcing the version bump — partially covered by Package Validation, with a gap worth naming explicitly.** Package Validation covers the compiled-API population; it won't catch a manifest-only regression — a `Description` string removed, an argument reordered in a way a naive client depends on positionally, a JSON field renamed with no backing C# property. Close that gap by reusing ApiCompat's actual mechanism (compare current against a stored baseline, fail on incompatible removal) applied to the JSON manifest as a second, narrower CI gate, rather than building a separate system from scratch.

**Release index with dates and support phases, modeled closely on .NET's own `releases-index.json`:**

```json
{
  "schema-releases-index": [
    {
      "schema-version": "v1",
      "release-date": "2026-08-01",
      "support-phase": "active",
      "eol-date": "2028-08-01",
      "schema-url": "https://fluentmigrator.github.io/schema/v1/commands.json",
      "skill-url": "https://fluentmigrator.github.io/schema/v1/SKILL.md"
    }
  ]
}
```

Host on `fluentmigrator.github.io` for cost and for the "official schema store" framing — and additionally submit the JSON schema to **schemastore.org**, the same registry `package.json`, `tsconfig.json`, and GitHub Actions workflow files publish to. That gets IDE-level validation (VS Code, Rider, etc. auto-associate schemas from that store) for anyone hand-authoring a manifest or config file against the schema, essentially for free, layered on top of the self-hosted index that drives Aspire/Canvas's own version negotiation.

## Open design questions

No open design questions remain; Post-MVP covers work that's deliberately deferred rather than undecided.