# FluentMigrator analyzers

Roslyn analyzers for FluentMigrator migrations.

| Rule | Severity | Summary |
|---|---|---|
| `FM0001` | Warning | Migration version should be unique |
| `FM0002` | Warning | Prefer `$[name]` over `$(name)` for SQL token substitution |
| `FM0003` | Warning | Automatically quoted token is inside a string literal |
| `FMUP0001` | Warning | Prefer `IDictionary<string, object>` over `IDictionary<string, string>` for `Execute` parameters |

## Token substitution rules

The two token styles are complements, not alternatives:

- `$(name)` substitutes the value **verbatim**. It supplies content; anything around it in the script
  supplies the syntax. `WHERE name = '$(userName)'` is the idiomatic use.
- `$[name]` substitutes a **complete SQL literal**, already quoted: `'O''Brien'`, `NULL`, `1`,
  `0xdead`. It stands alone: `WHERE name = $[userName]`.

`FM0002` steers you from the first to the second, and `FM0003` catches the failure mode of applying
that advice mechanically: `'$[userName]'` quotes the value twice. Only the string case fails loudly —
numbers silently become string literals, `null` silently becomes the four-character text `'NULL'`,
and a `RawSql` value silently becomes text so its SQL never runs.

`FM0002` does not report the escaped form `$$((name))`, which performs no substitution and so carries
no injection risk, nor tokens inside comments, which are never executed.

## Configuring the SQL dialect

Most of `FM0003` needs no dialect: every supported database opens a string literal with `'`, so a
token wrapped in single quotes is wrong everywhere. A few cases do depend on the dialect, and stay
silent unless it is known:

| Construct | Needs a dialect because |
|---|---|
| `"$[name]"` | A string literal in MySQL without `ANSI_QUOTES`, a quoted identifier elsewhere |
| `$tag$ ... $tag$` | PostgreSQL dollar quoting; the body is scanned as SQL rather than as data |
| `'a\'$[name]'` | A backslash escapes the quote in MySQL, so the literal has not ended |
| `q'[$[name]]'` | Oracle's alternative quoting |

Two signals resolve the dialect, in precedence order.

**`IfDatabase(...)`** — the most specific, and needs no configuration:

```csharp
IfDatabase("MySql").Execute.Sql("SELECT \"$[name]\"");   // FM0003 reports
```

Naming several databases only resolves when they share a family: `IfDatabase("MySql5", "MySql8")`
resolves, `IfDatabase("MySql", "Postgres")` does not.

**`.editorconfig`** — applies to a file, directory or project:

```ini
[*.cs]
fluent_migrator_sql_dialect = postgres
```

Accepted values are the `ProcessorIdConstants` spellings, matched by family and case-insensitively:
`sqlserver`, `postgres`, `redshift`, `mysql`, `mariadb`, `oracle`, `sqlite`, `firebird`, `db2`,
`hana`, `snowflake`. Version suffixes are accepted and ignored, since no supported version changes
how a string literal is delimited.

Referenced runner packages are deliberately not used as a signal — projects routinely reference
several providers, so the inference would be wrong more often than useful.

## Limitations

Session-level settings that change literal lexing are not visible at compile time:
`NO_BACKSLASH_ESCAPES`, `ANSI_QUOTES`, `QUOTED_IDENTIFIER OFF`, `standard_conforming_strings`,
`SQLITE_DQS`. Each dialect's **default** is assumed, and any diagnostic that would depend on a
flipped mode is not reported. SQLite double quotes are never reported for this reason: whether
`"foo"` is an identifier or a string depends on the schema, not on the text.

Only string *literal* arguments to `Execute.Sql(...)` are analyzed. SQL built at runtime, read from a
file, or passed through `Execute.Script`/`Execute.EmbeddedScript` is out of reach.
