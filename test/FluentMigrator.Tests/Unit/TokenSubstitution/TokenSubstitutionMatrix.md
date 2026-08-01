# Token substitution matrix

Reference document for `TokenSubstitutionMatrixTests`. Describes *what* the matrix covers and *why*
each axis exists. The measured output lives in the `*.verified.md` snapshots beside this file.

Origin: [#2336](https://github.com/fluentmigrator/fluentmigrator/issues/2336), the coverage request this fixture answers. Design decisions since then live in `adr/proposed/RequireQuoterForTokenSubstitution.md`.

## Why a matrix

`SqlScriptTokenReplacer` is reached from `Execute.Sql`, `Execute.Script` and `Execute.EmbeddedScript`
(`ExecuteSqlStatementExpression.ExecuteWith` and `ExecuteSqlScriptExpressionBase.ExecuteWith` are its
only callers in `src/`). Those call sites pass `processor.Quoter`, so token output depends on the
whole DI graph, not just on the replacer.

The axes multiply out to roughly 35,000 combinations. Enumerating each as an NUnit case would be
unreadable, so the fixture multiplexes the **configuration** axes into `TestCaseSource` and treats the
**value x token-style** grid as a snapshot payload. A regression is then a one-cell diff in a
markdown table rather than one of 35,000 opaque case names.

## Axes

| # | Axis | Values | Why it matters |
|---|---|---|---|
| 1 | Token style | `$(name)` raw, `$[name]` safe-quoted | Different code paths: raw bypasses the quoter entirely |
| 2 | Quoter presence | supplied only | A `null` quoter no longer renders anything - it throws when `$[name]` is expanded, identically for every value and dialect. Covered once in `SqlScriptTokenReplacerTests` rather than as a matrix column; see `adr/proposed/RequireQuoterForTokenSubstitution.md` |
| 3 | Value type | every branch of `GenericQuoter.QuoteValue`, plus uncased fall-through, plus `RawSql` | The reported defects are all value-type specific |
| 4 | Dialect | ~30 `Add*()` registrations on `IMigrationRunnerBuilder` | Each wires a different quoter/typemap/generator triple |
| 5 | `IQuoter` switch | `binaryGuid`, `QuoteIdentifiers`, `ForceQuote`, quoted-identifier variants | Orthogonal to dialect; changes value rendering |
| 6 | `ITypeMap` switch | `useStrictTables`, version lineage | Orthogonal to dialect; must *not* change value rendering |
| 7 | `IGenerator` switch | `GeneratorOptions.CompatibilityMode` | Orthogonal to both |
| 8 | `CurrentCulture` | `en-US`, `de-DE` | Invariant-culture regressions are invisible on an en-US agent |

Axes 4-7 become test cases. Axes 1-3 become the snapshot payload. Axis 8 is a dedicated invariance
test rather than a doubling of the snapshot.

## Value cases

One entry per branch of `GenericQuoter.QuoteValue`, in source order, so a new `case` added to the
quoter without a matching row here is visible as a gap.

| Value case | Cased in `GenericQuoter.QuoteValue`? | Notes |
|---|---|---|
| `null` | yes | `FormatNull()` |
| `DBNull.Value` | yes | shares the `FormatNull()` branch |
| `string` | yes | `FormatNationalString`; uses `O'Brien` to exercise quote doubling |
| `NonUnicodeString` | yes | `FormatAnsiString` |
| `char` | yes | |
| `bool` | yes | |
| `Guid` | yes | |
| `DateTime` | yes | |
| `DateTimeOffset` | yes | |
| `double` | yes | culture-sensitive if mishandled |
| `float` | yes | culture-sensitive if mishandled |
| `decimal` | yes | culture-sensitive if mishandled |
| `int` | yes | |
| `long` | **no** | falls through to `value.ToString()` |
| `short` | **no** | falls through to `value.ToString()` |
| `byte` | **no** | falls through to `value.ToString()` |
| `SystemMethods` | yes | `GenericQuoter.FormatSystemMethods` throws; providers override |
| `Enum` | yes | |
| `byte[]` | yes | |
| `TimeSpan` | yes | |
| `RawSql` (plain) | yes | verbatim passthrough — subject of #2332 |
| `RawSql` (backslash) | yes | verbatim passthrough — subject of #2335 |
| `RawSql` (embedded quote) | yes | verbatim passthrough |

`long`, `short` and `byte` are deliberately included *because* they are uncased. They render
correctly by accident via `ToString()`, which is culture-sensitive. Pinning them means a future
change to the fall-through is visible.

## Assertions

| Test | Claim |
|---|---|
| `TokenMatrix_IsStable` | Golden markdown table per dialect family |
| `RawSql_RoundTripsVerbatim` | `RawSql` reaches the database byte-for-byte through every quoter and both token styles |
| `SafeToken_Agrees_With_QuoteValue` | `$[x]` and `Insert`/`Update`/`Delete` render an identical literal for the same value, since both terminate in `IQuoter.QuoteValue` |
| `ProcessorQuoter_Subsumes_GeneratorQuoter` | `processor.Quoter` and `generator.Quoter` agree |
| `TypeMapSwitch_DoesNotAffect_TokenExpansion` | Flipping only an `ITypeMap` switch moves no cell |
| `TokenMatrix_IsCultureInvariant` | The matrix is identical under `en-US` and `de-DE` |
| `EscapeAndMissForms_AreDialectIndependent` | `$$((x))`, `$$[[x]]` and unmatched tokens do not vary by dialect |

### On `RawSql_RoundTripsVerbatim`

`RawSql` is an opt-in escape hatch: the caller has accepted responsibility for the SQL, so it must
arrive unmodified. This is the assertion that catches #2335.

It is worth being precise about why `SafeToken_Agrees_With_QuoteValue` does **not** catch #2335:
both sides of that comparison run through the same `MySqlQuoter` instance, so they agree on the
corrupted value. Agreement between two paths is only useful when the paths can differ; a defect
inside the shared component needs an absolute assertion, not a relative one.

### On `ProcessorQuoter_Subsumes_GeneratorQuoter`

`IGenerator` is supposed to subsume `IQuoter`, and `ProcessorBase.Quoter => Generator.Quoter` honours
that. `SqlServerProcessor` and `Db2Processor` additionally declare `public new IQuoter Quoter`, which
shadows it - but in practice both resolve the same scoped instance from the container, so the paths
agree today across every registration. This test passes as written; it exists to keep that true,
because token substitution reads `processor.Quoter` while DML generation reads `generator.Quoter`.

### On `TypeMapSwitch_DoesNotAffect_TokenExpansion`

`ReplaceSqlScriptTokens` never receives an `ITypeMap` — that interface maps CLR/`DbType` to column
type strings for DDL. The orthogonality is therefore true by construction today, and this test exists
so that a future refactor cannot quietly couple them.

## What this fixture does *not* cover

The matrix calls `SqlScriptTokenReplacer` directly with a quoter resolved from a real container. It
therefore proves what the replacer does with a given quoter, but not that the production call sites
hand it the right one. That is covered separately, and deliberately, because the two call sites are
distinct pieces of code:

| Entry point | Call site | Covered by |
|---|---|---|
| `Execute.Sql` | `ExecuteSqlStatementExpression.ExecuteWith` | `ExecuteSqlStatementExpressionTests` |
| `Execute.Script` | `ExecuteSqlScriptExpressionBase.ExecuteWith` | `ExecuteSqlScriptExpressionTests` |
| `Execute.EmbeddedScript` | same base | `ExecuteEmbeddedSqlExpressionTests` |

Each has a test that expands a `$[name]` token through a mock processor with a stubbed `Quoter`, so
reverting a call site to `null` fails rather than silently degrading. Before those existed, only
`Execute.Sql` was pinned - the script paths used `$(name)` exclusively and would have stayed green.

## Known-wrong cells

The snapshot is a record of *current* behaviour, not of *desired* behaviour; the assertion tests
above encode desired behaviour and are the ones that fail while a defect is open.

One cell is knowingly wrong today: `$(x)` renders a `DateTime` as `07/28/2026 13:45:06`, which is
invariant but not a portable SQL datetime literal. Tracked in
[#2354](https://github.com/fluentmigrator/fluentmigrator/issues/2354). The defects the matrix was
built to find - [#2332](https://github.com/fluentmigrator/fluentmigrator/issues/2332),
[#2335](https://github.com/fluentmigrator/fluentmigrator/issues/2335) and the culture and
fallback bugs - are fixed, so no other cell is pending.
