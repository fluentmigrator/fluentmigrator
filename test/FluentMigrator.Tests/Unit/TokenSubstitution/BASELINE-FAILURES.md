# Baseline: token substitution matrix against `main`

Snapshot of the matrix as it stands **before** the fixes in this branch. Committed deliberately so
the next commit's diff shows exactly which cells the fixes move.

Run against `e25bf5ef` ("Fix RawSql script token replacement (#2333)") on `net48`, `en-US` host.

```
Failed!  - Failed: 48, Passed: 151, Skipped: 0, Total: 199 - FluentMigrator.Tests.dll (net48)
```

| Failing test | Count | Defect |
|---|---|---|
| `TokenMatrix_IsCultureInvariant` | 45 | `$[name]` no-quoter fallback calls plain `ToString()` |
| `RawSql_RoundTripsVerbatim` | 3 | `MySqlQuoter` double-escapes backslashes in the `RawSql` passthrough (#2335) |

45 is every dialect case; 3 is `MySql4`, `MySql5`, `MySql8`.

## `TokenMatrix_IsCultureInvariant`

#2324 routed `$(name)` through `IFormattable` + `InvariantCulture`, but the `$[name]` no-quoter
fallback still calls plain `.ToString()`. Under `de-DE`:

| Value | `$[x]` no quoter, `en-US` | `$[x]` no quoter, `de-DE` |
|---|---|---|
| `1234.5678d` | `'1234.5678'` | `'1234,5678'` |
| `DateTime(2026,7,28,13,45,6)` | `'7/28/2026 1:45:06 PM'` | `'28.07.2026 13:45:06'` |

Fails for every dialect because the fallback never reaches the provider quoter.

## `RawSql_RoundTripsVerbatim`

```
RawSql_RoundTripsVerbatim(MySql8)
  QuoteValueOrThrow(r.GeneratorQuoter, value)
    should be  "c REGEXP '\d+'"
    but was    "c REGEXP '\\d+'"
```

`MySqlQuoter.QuoteValue` applies string-literal backslash escaping to the value returned by
`GenericQuoter.QuoteValue`, which for `RawSql` is a verbatim passthrough. Tracked as #2335.

## What the snapshots already record but no assertion fails on yet

The `*.verified.md` files in this directory capture the `$[name]` no-quoter fallback rendering
non-string values as string literals:

| Value | `$[x]` no quoter | `$[x]` with quoter |
|---|---|---|
| `1234.5678d` | `'1234.5678'` | `1234.5678` |
| `-1234` | `'-1234'` | `-1234` |
| `true` | `'True'` | `1` |
| `new byte[]{0xDE,0xAD}` | `'System.Byte[]'` | `0xdead` |
| `DBNull.Value` | `''` | `NULL` |
| `RawSql.Insert("CURRENT_TIMESTAMP")` | `'FluentMigrator.RawSql'` | `CURRENT_TIMESTAMP` |

These are visible in the golden tables rather than asserted individually, because the correct
behaviour of the fallback is an API decision (see #2336) rather than an unambiguous bug.

## Two claims this run disproved

Recorded because both were asserted in the analysis on #2332 before being measured.

1. **`SqlServerProcessor` / `Db2Processor` were expected to fail `ProcessorQuoter_Subsumes_GeneratorQuoter`.**
   They do not. Both declare `public new IQuoter Quoter`, but the shadowing property and the
   generator resolve the same scoped instance from the container, so the two paths agree across
   every registration. The test passes and is kept as a regression guard.

2. **`SafeToken_Agrees_With_QuoteValue` was expected to catch #2335.** It does not, and cannot:
   both sides of the comparison run through the same `MySqlQuoter` and therefore agree on the
   corrupted value. Catching a defect inside a shared component needs an absolute assertion, which
   is why `RawSql_RoundTripsVerbatim` was added.
