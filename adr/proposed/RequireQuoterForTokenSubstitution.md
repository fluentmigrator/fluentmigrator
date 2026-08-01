# ADR: Require an IQuoter for SQL Script Token Substitution

## Status
Proposed — **amended**. The original decision (Option G) was superseded during implementation; see
[Amendment](#amendment-option-g-superseded-by-option-i) and the revised [Decision](#decision).

Follows the coverage work in [#2336](https://github.com/fluentmigrator/fluentmigrator/issues/2336), which recorded the correct behaviour of the no-quoter fallback as "an API decision" rather than resolving it.

## Context

`SqlScriptTokenReplacer.ReplaceSqlScriptTokens` takes an optional quoter:

```csharp
public static string ReplaceSqlScriptTokens(
    [StringSyntax("sql")] string sqlText,
    IDictionary<string, object> parameters,
    IQuoter quoter = null)
```

When `quoter` is null, `$[name]` falls back to `QuoteSqlLiteral`, a private method in
`FluentMigrator.Abstractions` that reimplements the default rendering of
`GenericQuoter.QuoteValue`. The two must agree, and are held together only by a test —
`TokenSubstitutionMatrixTests.SafeToken_Agrees_With_QuoteValue`.

That duplication is the visible problem. The null is the underlying one.

### How it got there

`git blame` places the parameter on `cdf0ff74` ([#2312](https://github.com/fluentmigrator/fluentmigrator/pull/2312), 2026-07-24), the commit
that introduced the `$[name]` syntax. It was optional from birth; there is no earlier
stricter form that was later relaxed.

That same commit did three things that shape this decision:

1. It **replaced** the public `ReplaceSqlScriptTokens(string, IDictionary<string, string>)`
   overload rather than adding alongside it, requiring `CompatibilitySuppressions.xml`
   entries for `IExecuteExpressionRoot.Sql`, `.Script` and `.EmbeddedScript`.
2. It **added `IQuoter Quoter { get; }` to `IMigrationProcessor`**, itself a breaking
   interface change.
3. It introduced the `quoter != null` branch and the fallback.

[#2322](https://github.com/fluentmigrator/fluentmigrator/pull/2322) subsequently restored the string-dictionary overload as `[Obsolete]`.
[#2343](https://github.com/fluentmigrator/fluentmigrator/pull/2343) refactored the body into a generic `ReplaceSqlScriptTokensCore<TValue>`.

### Why the parameter is nullable today

**The obsolete overload has no quoter to pass.** This is the constraint that has kept it
optional:

```csharp
[Obsolete("Use the IDictionary<string, object> overload instead...")]
public static string ReplaceSqlScriptTokens(string sqlText, IDictionary<string, string> parameters)
{
    return ReplaceSqlScriptTokensCore(sqlText, parameters, null);
}
```

It is a public, supported, backwards-compatibility entry point that structurally cannot
supply one. It exists specifically so 8.0.1 callers keep working.

**`IMigrationProcessor.Quoter` is new.** Third-party processors written against 8.0.1 do not
implement it, and implementations may return null. This is what `processor?.Quoter` tolerates
at the two call sites — note that `ExecuteSqlStatementExpression.ExecuteWith` dereferences
`processor` unconditionally on the next line, so the `?.` is guarding a null **Quoter**, not
a null **processor**.

**Tests exercise the null path.** `ExecuteSqlScriptExpressionTests` and
`ExecuteEmbeddedSqlExpressionTests` construct `new Mock<IMigrationProcessor>()` and stub only
`Execute`; Moq returns null for `Quoter`. Exactly one test stubs it deliberately.

### What is *not* a reason

No production path in this repository reaches the fallback. `Db2Processor`,
`Db2ISeriesProcessor` and `SqlServerProcessor` assign `Quoter` from `[NotNull]` constructor
parameters; every other processor delegates to `Generator.Quoter`. The fallback is reachable
only from the obsolete overload, from third-party processors, and from tests.

### The decisive fact: none of this has shipped

8.0.1 exposes exactly one overload — `ReplaceSqlScriptTokens(string, IDictionary<string, string>)` —
confirmed by reflecting over the released `FluentMigrator.Abstractions` package.
`IMigrationProcessor.Quoter` is likewise absent from 8.0.1.

So the entire quoter-threading design is unreleased. Changing its signature costs shipped
users nothing, and this is the last release in which that is true.

### The other half: the string-dictionary overload is genuinely old (amendment)

The converse is equally load-bearing and was not stated originally.
`ReplaceSqlScriptTokens(string, IDictionary<string, string>)` dates from `ce4d9428`, 2018-04-04,
and appears in tags from `v2.0.0` onward. It is the only public overload in 8.0.1.

The git history disguises this: #2312 *replaced* it rather than adding beside it, and #2322 then
*restored* it, so it reads as recently added when it is recently re-added.

Two consequences:

1. Its behaviour is a compatibility surface eight years wide. It cannot simply be deleted to make
   the quoter question go away.
2. **In 8.0.1, `$[name]` was not a token at all.** Running the released package on
   `"SELECT $[CurrentTimestamp]"` returns it unchanged. Today's `main` routes that overload through
   the shared core, so the same input is now *substituted* — an unannounced behaviour change on the
   oldest API in the file.

The cheapest correct behaviour for that overload is therefore not "find it a quoter" but "restore
what it did in 8.0.1": expand `$(name)`, leave `$[name]` alone, and need no quoter at all.

## Options Considered

### Option A — status quo
Optional parameter, duplicated fallback. Costs nothing now; keeps two implementations of
value rendering that must be manually kept in agreement, in two assemblies, indefinitely.

### Option B — remove `= null` only
Callers must write the argument, but may still write `null`. No behavioural change, no
guard, the fallback stays. Named explicitly so it is not chosen by accident: this is the
version of "required" that looks like progress and delivers none.

### Option C — require, and throw on null
Deletes the fallback and the duplication outright. Rejected **as originally framed**, because
it had no answer for the obsolete overload except to break it. See Option G, which is this
option with that block removed.

### Option D — default to a real quoter that lives in Abstractions
Introduce a standard-SQL `IQuoter` implementation in `FluentMigrator.Abstractions` and use it
wherever the fallback is used now, so the null becomes an ordinary default rather than a
semantic mode. Removes the duplication without changing the signature.

The replacer only calls `QuoteValue`, so the new type needs one real member; the
identifier-quoting members of `IQuoter` throw `NotSupportedException`. That asymmetry is the
honest cost.

### Option E — move `GenericQuoter` into Abstractions
Removes the duplication with no new type.

> **Correction (amendment).** This was originally rejected on the grounds that `GenericQuoter`
> "carries generator-facing concerns and a large public surface". That was asserted without
> checking. Its entire dependency set is `System.Globalization`, `JetBrains.Annotations`, and
> `IQuoter` / `SystemMethods` / `RawSql` / `NonUnicodeString` — all already in Abstractions. It
> *could* move. The real objections are that `GenericQuoter` has shipped publicly from
> `Runner.Core` since 2018, so relocating it is a binary break needing `[TypeForwardedTo]`, and
> that moving a concrete generator into Abstractions sets a precedent for putting implementation
> in the contract assembly. Both are judgement calls rather than the mechanical impossibility
> originally implied.

### Option F — keep the null path for 9.0, but deprecate it
Keep the null path working so 8.0.1 users have a step-up rather than a wall, and mark it
obsolete now while doing so is free. The suggested mechanism is an explicit opt-in:

```csharp
ReplaceSqlScriptTokens(sqlText, parameters, quoter: null, allowNullQuoter: true)
```

**A C# constraint shapes this.** Keeping `IQuoter quoter = null` *and* marking it obsolete
cannot be expressed as written — `[Obsolete]` applies to methods, not to parameter defaults,
and there is only one overload to attach it to. Obsoleting that overload also deprecates the
correct, quoter-supplied call. Two overloads differentiated by the flag are required, which
means the `= null` default goes regardless and `(sqlText, parameters)` stops compiling.

Fixes the contract; leaves the duplication. Also, `allowNullQuoter: true` understates the
effect — it does not permit a null, it selects a second rendering implementation.

Retained as the fallback position if Option G's new type is judged too high a price.

### Option G — required quoter, with the fallback promoted to a named default
Options D and F are complementary rather than competing: F fixes the contract, D fixes the
duplication. Applying the unreleased-API fact to both collapses them, and the flag disappears:

```csharp
// no `= null` default: every caller states its quoter
public static string ReplaceSqlScriptTokens(string sqlText, IDictionary<string, object> parameters, IQuoter quoter)

// ... and inside the $[name] branch only:
if (quoter is null)
{
    throw new ArgumentNullException(
        nameof(quoter),
        $"Expanding the $[{key}] token requires an IQuoter. Pass the processor's Quoter, " +
        "or StandardSqlQuoter.Instance if no provider-specific quoter is available.");
}

[Obsolete("Use the IDictionary<string, object> overload instead...")]
public static string ReplaceSqlScriptTokens(string sqlText, IDictionary<string, string> parameters)
{
    return ReplaceSqlScriptTokensCore(sqlText, parameters, StandardSqlQuoter.Instance);
}
```

The legacy overload names the quoter it uses, instead of passing null and a flag meaning
"you know the one". No silent fallback, no boolean, no `QuoteSqlLiteral`, one implementation
of value rendering.

**The guard belongs at the point of use, not at method entry.** A script containing only
`$(name)` tokens never consults the quoter, so rejecting it on entry would fail callers that
have no need of one. That is not hypothetical: four of the five mock-based expression test
files use `$(name)` exclusively, and an entry guard would break all of them for a facility
they do not touch.

This makes `null` a legal argument meaning "I have no quoter", which fails only where it
actually matters. That is **not** Option B: B keeps the silent fallback, so a null still
renders — just differently and invisibly. Here a null never renders. It is either irrelevant
or fatal, and the message says which token forced the issue.

This is Option C with its blocker removed, and its guard narrowed. The obsolete overload never
lacked a *quoter*; it lacked a quoter it could *name*. Option D supplies one, and the fact
that the signature is still unreleased supplies the rest.

> **Superseded (amendment).** Option G does not deliver what this ADR claimed for it. It replaces
> a private duplicate with a *public* one: `StandardSqlQuoter.QuoteValue` in Abstractions and
> `GenericQuoter.QuoteValue` in `Runner.Core` still both exist, still must agree. The count of
> value-rendering implementations stays at two; only its visibility changes. The name was also
> wrong — see Option H.

### Option H — a shared static formatter in Abstractions (amendment)
Put the default rendering in a public static helper in Abstractions; `SqlScriptTokenReplacer` uses
it, and `GenericQuoter` delegates to it.

**`GenericQuoter.QuoteValue` cannot delegate wholesale.** It is a type-dispatch switch over
*virtual* `Format*` members, and providers override roughly thirty of them —
`FormatSystemMethods` ×11, `FormatDateTime` ×4, `FormatByteArray` ×3, `FormatNationalString` ×3,
`FormatBool` ×2, `FormatGuid` ×2, and more. Delegating `QuoteValue` to a static would bypass every
one, silently regressing each provider to the defaults.

Delegation therefore only works one level down, with each virtual's *default body* calling the
static. That shares the per-type rules but leaves the switch duplicated, and moves roughly fifteen
formatting methods into Abstractions — the same slippery slope as Option E, at a gentler gradient.

Rejected on that basis, but retained because it is the right shape *if* the legacy overload must
keep expanding `$[name]`.

### Option I — add nothing to Abstractions; restore 8.0.1 behaviour on the legacy overload (amendment)
Delete `QuoteSqlLiteral` outright. Require the quoter on the object overload, guarded inside the
`$[name]` branch as in Option G. Then let the string-dictionary overload do exactly what 8.0.1 did:
expand `$(name)`, leave `$[name]` untouched, consult no quoter.

```csharp
public static string ReplaceSqlScriptTokens(string sqlText, IDictionary<string, object> parameters, IQuoter quoter)

[Obsolete("Use the IDictionary<string, object> overload instead...")]
public static string ReplaceSqlScriptTokens(string sqlText, IDictionary<string, string> parameters)
{
    // 8.0.1 had no $[name] token and left such text alone. Preserved deliberately: this overload
    // has no quoter, and inventing one would change what eight years of scripts emit.
    return ReplaceSqlScriptTokensCore(sqlText, parameters, quoter: null, expandSafeTokens: false);
}
```

`expandSafeTokens` is a parameter on the **private** core, so no public surface grows.

The count of fallback implementations goes to **zero**, not one. `GenericQuoter` is untouched and
keeps its virtual dispatch. Abstractions gains nothing. And the legacy overload becomes *more*
faithful to 8.0.1 than `main` currently is.

The cost: `$[name]` is unavailable through the obsolete overload. It always was — that token did
not exist when the overload was the only API — and the obsolete message points at the overload that
supports it.

## Decision

**Adopt Option I.** This supersedes the original decision, Option G; see
[Amendment](#amendment-option-g-superseded-by-option-i) below for why.

1. Delete `QuoteSqlLiteral`. Add nothing to `FluentMigrator.Abstractions`.
2. Make `quoter` required on the `IDictionary<string, object>` overload by dropping the
   `= null` default, and guard it **inside the `$[name]` branch** rather than at method entry.
3. Give the private core an `expandSafeTokens` parameter. The string-dictionary overload passes
   `false`, restoring 8.0.1 behaviour: `$(name)` expands, `$[name]` passes through untouched, no
   quoter consulted. No public surface grows.
4. Update the 23 two-arg call sites in the test suite to state their quoter. Those expanding
   only `$(name)` may state `null`; the rest pass a real one.
5. Change the two production call sites from `processor?.Quoter` to `processor.Quoter`. The
   `?.` is vestigial — `ExecuteSqlStatementExpression.ExecuteWith` dereferences `processor`
   unconditionally on the very next line, so it never guarded a null processor. Audit confirms
   every in-tree `IMigrationProcessor` supplies a non-null quoter.
6. Add a `Quoter` stub to the mock-based tests that expand `$[name]`. Today exactly one of
   twenty-two mock-processor call sites stubs it, and Moq returns null for the rest.
7. `GenericQuoter` is untouched, keeping its virtual dispatch and its ~30 provider overrides.

8.0.1 users are unaffected — more precisely than under Option G. Their entry point is the
string-dictionary overload, which returns to doing exactly what it did in 8.0.1, including for
scripts whose text happens to contain `$[...]`.

### Amendment: Option G superseded by Option I

The original decision was **Option G**. Implementation surfaced two problems with it, both
recorded above rather than quietly corrected:

1. **It does not remove the duplication it was adopted for.** `StandardSqlQuoter.QuoteValue` and
   `GenericQuoter.QuoteValue` would both exist and both have to agree. The private duplicate
   becomes a public one; the count stays at two.
2. **The name was wrong.** `GenericQuoter` is not ANSI SQL — `FormatBool` emits `1`/`0` where the
   standard has `TRUE`/`FALSE`, `FormatByteArray` emits `0xdead` where the standard has `X'DEAD'`,
   and `FormatDateTime` emits `'2026-07-28T13:45:06'` where the standard has
   `TIMESTAMP '2026-07-28 13:45:06'`. Providers override precisely these to correct them. A type
   mirroring those defaults is the *FluentMigrator default*, not a standard-SQL quoter, and naming
   it `StandardSqlQuoter` would have claimed a conformance it does not have.

Both dissolve once the string-dictionary overload is recognised as eight years old rather than
newly restored, and once it is accepted that in 8.0.1 that overload never expanded `$[name]` at
all. It does not need a quoter. It needs its old behaviour back.

Option I above records what was adopted in its place.

### A processor returning a null quoter

With decision item 5, a third-party `IMigrationProcessor` whose `Quoter` returns null hands that
null straight to the replacer, where it is inert for `$(name)` and fatal for `$[name]`.

That is acceptable, and rests on the same fact as the rest of this decision: `IMigrationProcessor.Quoter`
does not exist in 8.0.1. No shipped implementation can be relying on returning null, because
no shipped implementation has the member at all. Any processor compiled against 9.0 must add
it, and returning null would be a deliberate choice to opt out of `$[name]` support.

The alternative — defaulting at the call site with `processor.Quoter ?? someFallbackQuoter` — was
rejected. It would silently substitute default rendering for a provider-specific quoter, which is
precisely the invisible behaviour this ADR exists to remove, merely relocated from the replacer to
the call site.

## Consequences

**Positive.** Value rendering has **one** implementation, `GenericQuoter`, reached through the
providers' own virtual dispatch. Nothing is added to `FluentMigrator.Abstractions`, so the contract
assembly does not acquire a concrete quoter or a set of formatting helpers. No null-as-mode: a null
quoter is either irrelevant or an error. The break is taken in the only release where it is free.

**Negative.** `$[name]` cannot be used through the obsolete string-dictionary overload. This is a
restoration rather than a regression — that token did not exist when the overload was the only API —
but it does mean a caller who adopted `$[name]` on the legacy overload against unreleased `main`
must move to the object overload. `ReplaceSqlScriptTokens(sqlText, parameters)` stops compiling:
free externally, 23 call sites internally.

**Neutral.** `SystemMethods` continues to throw from `GenericQuoter.FormatSystemMethods` by
default, as it always has. Its SQL spelling is dialect-specific, and providers override it.

## Out of Scope

- **Whether `processor?.Quoter` should become non-nullable.** Gated on `IMigrationProcessor.Quoter`
  adoption by third-party processors, which is a separate compatibility question.
- **`$(name)` DateTime portability** ([#2354](https://github.com/fluentmigrator/fluentmigrator/issues/2354)). Concerns the raw token style and is unaffected
  either way.
- **Making `IMigrationProcessor.Quoter` non-nullable by contract.** Decision item 5 stops the
  call sites from tolerating a null, but does not stop a processor from returning one. Whether
  the interface should forbid it — by documentation, by analyzer, or by a default interface
  member — is a separate question.
- **Restoring `$[name]` to the string-dictionary overload.** If that is ever wanted, Option H is
  the right shape, and it should be decided on its own merits rather than smuggled in here.

## Validation Plan

- The token substitution matrix should produce **byte-identical snapshots** before and after.
  Any moved cell means `QuoteSqlLiteral` and `GenericQuoter.QuoteValue` had already drifted,
  and the diff identifies exactly where.
- A test that the string-dictionary overload expands `$(name)` and leaves `$[name]` **verbatim**,
  matching 8.0.1. This is the behaviour the amendment restores, and the reason no fallback quoter
  is needed.
- A test that `ReplaceSqlScriptTokens(sql, parameters, quoter: null)` throws
  `ArgumentNullException` **when the script contains `$[name]`**, and the message names the
  offending token.
- A test that the same call **succeeds** when the script contains only `$(name)`. This is the
  narrowed guard's whole point and the easiest thing to regress by "tidying" the check up to
  method entry.

## References

- `ce4d9428`, 2018-04-04 — introduced the string-dictionary overload, present from `v2.0.0`
- `cdf0ff74` ([#2312](https://github.com/fluentmigrator/fluentmigrator/pull/2312)) — introduced the optional parameter, the
  `IMigrationProcessor.Quoter` member and the compatibility suppressions
- [#2322](https://github.com/fluentmigrator/fluentmigrator/pull/2322) — restored the obsolete string-dictionary overload
- [#2343](https://github.com/fluentmigrator/fluentmigrator/pull/2343) — refactored the body into `ReplaceSqlScriptTokensCore<TValue>`
- [#2336](https://github.com/fluentmigrator/fluentmigrator/issues/2336) — coverage work that deferred this decision
- [#2355](https://github.com/fluentmigrator/fluentmigrator/pull/2355) — the original ADR, adopting Option G
- [#2362](https://github.com/fluentmigrator/fluentmigrator/issues/2362) — the implementation task, which this amendment rescopes
- `test/FluentMigrator.Tests/Unit/TokenSubstitution/BASELINE-FAILURES.md` — records the deferral
