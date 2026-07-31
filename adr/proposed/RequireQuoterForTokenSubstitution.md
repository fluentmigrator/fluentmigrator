# ADR: Require an IQuoter for SQL Script Token Substitution

## Status
Proposed

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
Removes the duplication with no new type. Rejected: `GenericQuoter` carries generator-facing
concerns and a large public surface, and relocating it across assemblies is a far larger
break than the problem justifies.

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
public static string ReplaceSqlScriptTokens(string sqlText, IDictionary<string, object> parameters, IQuoter quoter)
{
    if (quoter is null) throw new ArgumentNullException(nameof(quoter));
    ...
}

[Obsolete("Use the IDictionary<string, object> overload instead...")]
public static string ReplaceSqlScriptTokens(string sqlText, IDictionary<string, string> parameters)
{
    return ReplaceSqlScriptTokensCore(sqlText, parameters, StandardSqlQuoter.Instance);
}
```

The legacy overload names the quoter it uses, instead of passing null and a flag meaning
"you know the one". No null branch, no boolean, no `QuoteSqlLiteral`, one implementation of
value rendering.

This is Option C with its blocker removed. The obsolete overload never lacked a *quoter*; it
lacked a quoter it could *name*. Option D supplies one, and the fact that the signature is
still unreleased supplies the rest.

## Decision

**Adopt Option G.**

1. Add `StandardSqlQuoter` to `FluentMigrator.Abstractions`, implementing `IQuoter.QuoteValue`
   with the current `QuoteSqlLiteral` semantics and throwing `NotSupportedException` from the
   identifier-quoting members, with a message directing callers to the processor's quoter.
2. Make `quoter` required and guarded on the `IDictionary<string, object>` overload; drop the
   `= null` default.
3. Route the obsolete string-dictionary overload through `StandardSqlQuoter.Instance`
   explicitly.
4. Delete `QuoteSqlLiteral`.
5. Update the 23 two-arg call sites in the test suite.
6. Leave `processor?.Quoter` alone. It guards third-party processors and is a separate
   question from this parameter.

8.0.1 users are unaffected: their entry point is the string-dictionary overload, which keeps
working — now with behaviour that is named rather than implied.

## Consequences

**Positive.** One implementation of value rendering instead of two. No null-as-mode. The
break is taken in the only release where it is free. The compatibility path keeps working and
its behaviour becomes explicit at the call site.

**Negative.** A new public type in Abstractions whose `IQuoter` implementation is deliberately
partial — the identifier-quoting members exist only to satisfy the interface. Reviewers should
weigh that against the current wart of a private method shadowing `GenericQuoter` from another
assembly. `ReplaceSqlScriptTokens(sqlText, parameters)` stops compiling: free externally,
23 call sites internally.

**Neutral.** `SystemMethods` continues to throw from this path. Its SQL spelling is
dialect-specific and a standard-SQL quoter has no more basis to render it than the current
fallback does.

## Out of Scope

- **Whether `processor?.Quoter` should become non-nullable.** Gated on `IMigrationProcessor.Quoter`
  adoption by third-party processors, which is a separate compatibility question.
- **`$(name)` DateTime portability** ([#2354](https://github.com/fluentmigrator/fluentmigrator/issues/2354)). Concerns the raw token style and is unaffected
  either way.
- **Stubbing `Quoter` across mock-based tests.** Those tests pass a mock processor whose
  `Quoter` is null; the call sites still use `processor?.Quoter`, so they are unaffected by
  this change.

## Validation Plan

- The token substitution matrix should produce **byte-identical snapshots** before and after.
  Any moved cell means `QuoteSqlLiteral` and `GenericQuoter.QuoteValue` had already drifted,
  and the diff identifies exactly where.
- A test that the obsolete string-dictionary overload still round-trips, since it is the
  reason a named default is needed at all.
- A test that `ReplaceSqlScriptTokens(sql, parameters, null)` throws `ArgumentNullException`.
- A test that `StandardSqlQuoter`'s identifier-quoting members throw with an actionable
  message.

## References

- `cdf0ff74` ([#2312](https://github.com/fluentmigrator/fluentmigrator/pull/2312)) — introduced the optional parameter, the
  `IMigrationProcessor.Quoter` member and the compatibility suppressions
- [#2322](https://github.com/fluentmigrator/fluentmigrator/pull/2322) — restored the obsolete string-dictionary overload
- [#2343](https://github.com/fluentmigrator/fluentmigrator/pull/2343) — refactored the body into `ReplaceSqlScriptTokensCore<TValue>`
- [#2336](https://github.com/fluentmigrator/fluentmigrator/issues/2336) — coverage work that deferred this decision
- `test/FluentMigrator.Tests/Unit/TokenSubstitution/BASELINE-FAILURES.md` — records the deferral
