# Well-Known Token Variables for `Execute.Sql`, `Execute.Script`, `Execute.EmbeddedScript`

* Status: proposed
* Related issue: [#2300](https://github.com/fluentmigrator/fluentmigrator/issues/2300)

## Context and Problem Statement

`Execute.Sql`, `Execute.Script`, and `Execute.EmbeddedScript` already support token
substitution via `SqlScriptTokenReplacer` (see
`src/FluentMigrator.Abstractions/SqlScriptTokenReplacer.cs`). Two token styles are
supported:

* `$(name)` &mdash; replaced with the raw, unmodified value (for identifiers/SQL fragments).
* `$[name]` &mdash; replaced with a safely quoted SQL string literal.

Both styles are driven exclusively by the `Parameters` dictionary
(`IDictionary<string, string>`) that a caller supplies explicitly on
`ExecuteSqlScriptExpressionBase.Parameters`. There is currently no way for
FluentMigrator itself to contribute tokens whose values are derived from the
runner's own state, such as the schema name resolved by the currently configured
`IDefaultSchemaNameConvention` (see
`src/FluentMigrator.Runner.Core/Conventions/IDefaultSchemaNameConvention.cs` and
`DefaultSchemaConvention` in `IConventionSet`).

This forces multi-tenant/multi-schema users (see the use case raised by
@jhardin-accumula) to manually thread the default schema name (or any other
runner-derived value) into every `Execute.Sql`/`Execute.Script` call:

```csharp
Execute.Sql(
    "ALTER TABLE $(DefaultSchema).mumble ADD ...",
    new Dictionary<string, string> { ["DefaultSchema"] = /* looked up manually */ });
```

We want a mechanism that lets FluentMigrator (and users, via DI) automatically
expose a set of "well-known" tokens &mdash; e.g. `DefaultSchema` &mdash; without
requiring every migration author to look up and pass the value by hand.

## Decision Drivers

* Should be opt-in/overridable: users must be able to add, remove, or replace any
  well-known token (including `DefaultSchema`) without forking FluentMigrator.
* Should not break the existing `Parameters` token-replacement contract or its
  escaping rules (`$$((name))`, `$$[[name]]`).
* Should be resolvable through the existing Microsoft.Extensions.DependencyInjection
  container that the runner already uses, consistent with other extensibility
  points (`IConventionSet`, `IMigrationRunnerConventions`, etc. &mdash; see the
  `DependencyInjection.md` ADR).
* Should have a clear precedence rule when a well-known token name collides with a
  user-supplied `Parameters` entry.

## Considered Options

1. **Do nothing** &mdash; keep requiring callers to pass every token explicitly.
2. **Hard-code `DefaultSchema` substitution** directly inside `SqlScriptTokenReplacer`
   or the `Execute*` expressions.
3. **Introduce an injectable `IWellKnownTokenMapProvider`** service that supplies a
   `IDictionary<string, string>` of well-known tokens, merged with the
   caller-supplied `Parameters` at execution time.
4. **Adopt a general-purpose templating engine** (e.g. Jinja2.NET) to replace the
   current token syntax entirely.

## Decision Outcome

Chosen option: **3, introduce an injectable `IWellKnownTokenMapProvider`.**

### Rationale

* Option 1 does not address the issue.
* Option 2 hard-codes a single, non-extensible token and would need to be repeated
  for every future well-known token; it also can't be disabled/overridden by users.
* Option 4 is a much larger undertaking. FluentMigrator's SQL scripts are meant to
  stay close to plain SQL; adopting a full templating language with control-flow
  constructs (`{% ... %}`) reintroduces the expressiveness problems discussed in
  [#1263](https://github.com/fluentmigrator/fluentmigrator/issues/1263), and is out
  of scope for solving the immediate token-substitution need. It remains a
  candidate for a future, separate ADR.
* Option 3 fits the existing extensibility model. It keeps the current, simple
  `$(name)` / `$[name]` syntax, is a small, additive change, and is naturally
  DI-friendly:

```csharp
public interface IWellKnownTokenMapProvider
{
    IDictionary<string, string> GetWellKnownTokenMap();
}

public class DefaultWellKnownTokenMapProvider : IWellKnownTokenMapProvider
{
    private readonly IConventionSetAccessor _conventionSetAccessor;

    public DefaultWellKnownTokenMapProvider(IConventionSetAccessor conventionSetAccessor)
    {
        _conventionSetAccessor = conventionSetAccessor;
    }

    public IDictionary<string, string> GetWellKnownTokenMap()
    {
        var conventionSet = _conventionSetAccessor.GetConventionSet();
        return new Dictionary<string, string>
        {
            ["DefaultSchema"] = conventionSet?.SchemaConvention?.SchemaNameConvention.GetSchemaName(null),
        };
    }
}
```

`ExecuteSqlScriptExpressionBase` (or its `IMigrationProcessor`-facing execution
path) resolves `IWellKnownTokenMapProvider` from DI, merges its map with the
user-supplied `Parameters`, and passes the merged dictionary to
`SqlScriptTokenReplacer.ReplaceSqlScriptTokens`. User-supplied `Parameters` values
take precedence over well-known tokens of the same name, so users can always
override or shadow a well-known token (including effectively "removing"
`DefaultSchema` by mapping it to an empty/other value, or by replacing the whole
`IWellKnownTokenMapProvider` registration with one that omits it).

Multiple providers can be registered (`IEnumerable<IWellKnownTokenMapProvider>`)
and merged in registration order, so users can add their own well-known tokens
(e.g. tenant id, root path) alongside or instead of the built-in ones, following
the same "type getter / type instantiator" DI pattern already used elsewhere in
the runner (see `DependencyInjection.md`).

## Design Questions (open for future ADRs)

1. **Namespacing of reserved tokens.** We will not namespace built-in tokens (e.g.
   `$[FM-DefaultSchema]`); plain names like `$[DefaultSchema]` are used, since the
   token is opt-in/overridable rather than reserved. A Roslyn analyzer that flags
   compile-time-constant dictionaries passed to `Execute.Sql`/`Execute.Script`/
   `Execute.EmbeddedScript` (to nudge users toward the well-known token provider
   instead of ad hoc dictionaries) is a possible follow-up, tracked separately.
2. **Templating engines** (Jinja2.NET or similar) remain a candidate for a future,
   larger ADR if simple token substitution proves insufficient, but are out of
   scope here because of the control-flow expressiveness gap described in
   [#1263](https://github.com/fluentmigrator/fluentmigrator/issues/1263).

   **Not a blocker for this MVP.** [#1263](https://github.com/fluentmigrator/fluentmigrator/issues/1263)
   is fundamentally about *when* a conditional is evaluated relative to a
   migration's two-phase plan/execute model: `Schema.Table(...).Exists()`
   queries live database state at the moment the `Up()`/`Down()` method runs,
   so the same migration can branch differently depending on run order,
   transaction isolation level, or whether it is being previewed vs. actually
   executed. That is a *control-flow* problem &mdash; it only arises once a
   templating layer offers `{% if %}` / `{% for %}` constructs that need to
   decide, ahead of or during SQL generation, whether to include a block of
   SQL at all.

   The `IWellKnownTokenMapProvider` mechanism proposed here does not introduce
   any control flow. It is a straight, non-branching string substitution:
   every well-known token is resolved to a single string value once, at the
   moment `Execute.Sql`/`Execute.Script`/`Execute.EmbeddedScript` runs, using
   the same `SqlScriptTokenReplacer.ReplaceSqlScriptTokens` code path that
   already handles user-supplied `Parameters` today. There is no notion of a
   token being conditionally present, no loop/repetition, and no dependency on
   evaluating live database state ahead of execution &mdash; the exact
   ambiguity #1263 raises simply does not apply to `$(name)`/`$[name]`
   substitution. Consequently, design question 2 (whether/how to adopt a full
   templating engine with control flow) can be deferred indefinitely without
   blocking or reworking the MVP described in this ADR; if a templating engine
   is adopted later, it can layer `{% if %}`/`{% for %}` support on top of (or
   independently of) the token map without changing the
   `IWellKnownTokenMapProvider` contract.

## Consequences

* Additive change: no existing `Execute.Sql`/`Execute.Script`/`Execute.EmbeddedScript`
  call sites break, since `IWellKnownTokenMapProvider` contributes additional
  tokens on top of (and overridable by) the existing `Parameters` dictionary.
* Users gain a supported, DI-based extension point for adding their own
  well-known tokens or removing/replacing the built-in `DefaultSchema` token.
* Future well-known tokens (e.g. derived from `IMigrationRunnerConventions` or a
  root path convention) can be added by registering additional
  `IWellKnownTokenMapProvider` implementations, without further changes to
  `SqlScriptTokenReplacer`.
