# Analyzers

::: info Available Since
FluentMigrator 7.2.0
:::

FluentMigrator provides Roslyn-based code analyzers to help catch common mistakes and enforce best practices at compile time. These analyzers are distributed as a separate NuGet package and integrate directly into your IDE and build process.

## Installation

The analyzers are available as a NuGet package that can be added to your migration projects:

```bash
dotnet add package FluentMigrator.Analyzers
```

Or via Package Manager Console:

```powershell
Install-Package FluentMigrator.Analyzers
```

Once installed, the analyzers will automatically run during builds and provide real-time feedback in your IDE.

## Available Analyzers

### FM0001: Migration Version Should Be Unique

**Category**: FluentMigrator
**Severity**: Warning

This analyzer ensures that each migration in your project has a unique version number. Duplicate version numbers can cause unpredictable behavior during migration execution.

#### Example Problem

```csharp
[Migration(20240101120000)]
public class CreateUsersTable : Migration
{
    public override void Up()
    {
        Create.Table("Users")
            .WithColumn("Id").AsInt32().PrimaryKey().Identity()
            .WithColumn("Name").AsString(100);
    }

    public override void Down()
    {
        Delete.Table("Users");
    }
}

// ❌ This will trigger FM0001
[Migration(20240101120000)]  // Same version as above!
public class CreateProductsTable : Migration
{
    public override void Up()
    {
        Create.Table("Products")
            .WithColumn("Id").AsInt32().PrimaryKey().Identity()
            .WithColumn("Name").AsString(100);
    }

    public override void Down()
    {
        Delete.Table("Products");
    }
}
```

**Diagnostic Message**:
```
FM0001: Migration attributes on CreateUsersTable, CreateProductsTable have conflicting version 20240101120000
```

#### Solution

Ensure each migration has a unique version number. The recommended format is a timestamp: `yyyyMMddHHmmss`

```csharp
[Migration(20240101120000)]
public class CreateUsersTable : Migration
{
    // ... migration code ...
}

// ✅ Use a unique version
[Migration(20240101120100)]  // One minute later
public class CreateProductsTable : Migration
{
    // ... migration code ...
}
```

#### Best Practices for Version Numbers

1. **Use Timestamps**: Format `yyyyMMddHHmmss` ensures chronological ordering
   ```csharp
   [Migration(20240322143000)]  // March 22, 2024 at 14:30:00
   ```

2. **Team Coordination**: In team environments, coordinate version numbers to avoid conflicts
   - Consider using Git commit timestamps
   - Use a convention like developer initials in the seconds portion
   - Resolve conflicts during merge

3. **Sequential Ordering**: Ensure migrations run in the intended order
   ```csharp
   [Migration(20240322140000)]  // Runs first
   [Migration(20240322140100)]  // Runs second
   [Migration(20240322140200)]  // Runs third
   ```

### FM0002: Prefer $[name] Over $(name) For SQL Token Substitution

**Category**: FluentMigrator
**Severity**: Warning

`Execute.Sql(...)` supports token substitution via [`SqlScriptTokenReplacer`](https://github.com/fluentmigrator/fluentmigrator/blob/main/src/FluentMigrator.Abstractions/SqlScriptTokenReplacer.cs),
which offers two token styles:

- `$(name)` substitutes the parameter value **verbatim/unquoted**. Since the value is inserted as-is,
  it can be a SQL injection risk if the value originates from outside the migration itself (for
  example, user input, environment variables, or other external/untrusted sources).
- `$[name]` substitutes the parameter value as a **safely quoted SQL string literal** (wrapped in
  single quotes, with embedded single quotes escaped by doubling them), rendered via `IQuoter`.

This analyzer flags uses of the raw `$(name)` token style (and its escaped form, `$$((name))`,
which renders the literal text `$(name)` without substitution) and recommends switching to the
automatically quoted `$[name]`/`$$[[name]]` forms instead, to reduce the software supply-chain risk
of accidentally interpolating untrusted data unquoted into SQL.

#### Example Problem

```csharp
[Migration(20240101120000)]
public class UpdateUserEmail : Migration
{
    public override void Up()
    {
        // ❌ This will trigger FM0002: the value is substituted unquoted.
        Execute.Sql(
            "UPDATE Users SET Email = $(email) WHERE Id = $(id)",
            new Dictionary<string, string> { ["email"] = email, ["id"] = id });
    }

    public override void Down()
    {
    }
}
```

**Diagnostic Message**:
```
FM0002: Replace raw variable interpolation token '$(email)' with the automatically quoted '$[email]' token, unless the raw substitution is intentional
```

#### Solution

Use the `$[name]` token style so the value is safely quoted for you:

```csharp
// ✅ The value is safely quoted as a SQL string literal.
Execute.Sql(
    "UPDATE Users SET Email = $[email] WHERE Id = $[id]",
    new Dictionary<string, string> { ["email"] = email, ["id"] = id });
```

A code fix is available (in supporting IDEs) to automatically replace `$(name)` with `$[name]`
(and `$$((name))` with `$$[[name]]`).

#### When Raw Substitution Is Legitimate

The `$(name)` token style still has legitimate uses, since it is not always desirable (or valid
SQL) to quote the substituted value &mdash; for example, when substituting an identifier, a SQL
keyword/fragment, or when concatenating two similar queries that differ only in a clause, using
`UNION`/`UNION ALL`:

```csharp
// $(whereClause) intentionally substitutes a raw SQL fragment, not string data.
Execute.Sql(
    "SELECT Id, Name FROM Users WHERE $(whereClause1) " +
    "UNION ALL " +
    "SELECT Id, Name FROM Users WHERE $(whereClause2)",
    new Dictionary<string, string>
    {
        ["whereClause1"] = "IsActive = 1",
        ["whereClause2"] = "IsActive = 0 AND LastLoginDate > '2024-01-01'",
    });
```

In such cases, since the value being substituted is trusted, fixed, developer-authored SQL (not
external/untrusted data), the FM0002 warning can be suppressed for that call &mdash; see
[Suppressing Analyzer Warnings](#suppressing-analyzer-warnings) below.

### FM0007: Date/Time Value Substituted Through a Raw $(name) Token

**Category**: FluentMigrator
**Severity**: Warning
**Available since**: 9.0.0

`$(name)` inserts the value's invariant `ToString()` result. For a `DateTime` that is
`07/28/2026 13:45:06`, and for a `DateTimeOffset` it is `07/28/2026 13:45:06 -04:00`. Databases read
that form differently:

- **SQL Server:** the result depends on the session's `DATEFORMAT` and language, so a day of the month
  up to 12 can silently become a different date on another server.
- **PostgreSQL:** depends on `DateStyle`.
- **MySQL and Oracle:** usually reject it.
- **SQLite:** stores it as text, so date comparisons stop working.

`$[name]` formats the value through the provider's quoter instead:

| Provider | `DateTime` | `DateTimeOffset` |
|---|---|---|
| SQL Server, PostgreSQL, MySQL, SQLite, Redshift, SAP HANA | `'2026-07-28T13:45:06'` (ISO 8601) | `'2026-07-28T13:45:06-04:00'` (ISO 8601) |
| Oracle | `to_date('2026-07-28 13:45:06', 'yyyy-mm-dd hh24:mi:ss')` | `'2026-07-28T13:45:06-04:00'`, with no conversion function |
| Db2 | `'2026-07-28-13.45.06'` | `'2026-07-28T13:45:06-04:00'` |
| Firebird | `'2026-07-28 13:45:06'` | `'2026-07-28T13:45:06-04:00'` |
| Snowflake | `'2026-07-28 13:45:06'` | `'2026-07-28 13:45:06 -04:00'` |

::: warning DateTimeOffset on Oracle
The quoter has no explicit conversion for `DateTimeOffset` on Oracle, so the value still reaches the
server as a string, and Oracle reads it according to the session's `NLS_TIMESTAMP_TZ_FORMAT`. `$[name]`
is still clearer than `07/28/2026 13:45:06 -04:00`, but it is not session-independent. Wrap the token in
`TO_TIMESTAMP_TZ($[name], 'YYYY-MM-DD"T"HH24:MI:SSTZH:TZM')`, or pass a `DateTime`.
:::

FluentMigrator deliberately does **not** change what `$(name)` emits, because that would silently
change the SQL of existing migrations. FM0007 points at the affected calls instead. It is separate from
FM0002, so it stays on in projects that suppress FM0002 because they use raw tokens on purpose.

#### Example Problem

```csharp
Execute.Sql("DELETE FROM Audit WHERE CreatedAt < '$(cutoff)'", new Dictionary<string, object>
{
    ["cutoff"] = new DateTime(2026, 7, 28, 13, 45, 6),   // FM0007
});
```

#### Solution

The code fix replaces the token with `$[cutoff]`. When the token was the whole SQL string literal, as
above, it also removes the surrounding quotes, because `$[name]` supplies its own:

```csharp
Execute.Sql("DELETE FROM Audit WHERE CreatedAt < $[cutoff]", new Dictionary<string, object>
{
    ["cutoff"] = new DateTime(2026, 7, 28, 13, 45, 6),
});
```

When the token is only part of a literal, for example `'$(day) 00:00'`, there is no mechanical
rewrite, so FM0007 reports without offering a fix.

#### What FM0007 Can See

The analyzer sees the SQL and the parameters separately, and its fix rewrites SQL. So it reports only
when it knows exactly what the token will receive:

- the call is `Execute.Sql` with a SQL string literal;
- the parameters are a `Dictionary<string, ...>` created in the call, or a local variable initialized
  with one that is never reassigned, modified (indexer writes, `Add`, `Remove`, ...) or passed anywhere
  except `Execute.Sql`;
- every key in the initializer is a constant string; and
- the comparer is the default, `StringComparer.Ordinal` or `StringComparer.OrdinalIgnoreCase`.
  Culture-sensitive comparers such as `CurrentCultureIgnoreCase` match keys differently depending on the
  culture the migration runs under, so they are not analyzed.

Anything else is not reported, and neither is `Execute.Script`/`EmbeddedScript`, whose SQL is not in
your source. See also [Date and time values](../operations/execute-sql.md#date-and-time-values).

## IDE Integration

The analyzers work seamlessly with popular IDEs:

### Visual Studio

Warnings appear in:
- Error List window during builds
- Inline in the code editor with squiggly underlines
- IntelliSense with detailed diagnostic messages

### Visual Studio Code

When using the C# extension:
- Warnings appear in the Problems panel
- Inline diagnostics in the editor
- Quick fixes when available

### Rider

- Warnings shown in the Solution-wide Analysis window
- Inline highlighting in the editor
- Integration with code inspections

## Build Integration

Analyzers run automatically during build and can be configured to treat warnings as errors:

### MSBuild Configuration

```xml
<PropertyGroup>
  <!-- Treat all warnings as errors -->
  <TreatWarningsAsErrors>true</TreatWarningsAsErrors>

  <!-- Or treat specific analyzer warnings as errors -->
  <WarningsAsErrors>FM0001</WarningsAsErrors>
</PropertyGroup>
```

### CI/CD Integration

Since analyzers run during compilation, they automatically integrate with CI/CD pipelines:

```bash
# Analyzers will report warnings/errors during build
dotnet build
```

## Suppressing Analyzer Warnings

In rare cases where you need to suppress an analyzer warning:

### Using Pragma Directives

```csharp
#pragma warning disable FM0001
[Migration(20240101120000)]
public class SpecialMigration : Migration
{
    // ... migration code ...
}
#pragma warning restore FM0001
```

### Using Attributes

```csharp
[Migration(20240101120000)]
[System.Diagnostics.CodeAnalysis.SuppressMessage("FluentMigrator", "FM0001:Migration version isn't unique")]
public class SpecialMigration : Migration
{
    // ... migration code ...
}
```

### Using .editorconfig

Create or modify `.editorconfig` in your project root:

```ini
[*.cs]
# Disable FM0001 for all files
dotnet_diagnostic.FM0001.severity = none

# Or reduce severity to suggestion
dotnet_diagnostic.FM0001.severity = suggestion
```

::: warning
Suppressing analyzer warnings should be done sparingly and only when you have a specific reason. Duplicate migration versions can cause serious issues in production environments.
:::

## Troubleshooting

### Analyzers Not Running

If analyzers aren't providing diagnostics:

1. **Verify Package Installation**
   ```bash
   dotnet list package | grep FluentMigrator.Analyzers
   ```

2. **Clean and Rebuild**
   ```bash
   dotnet clean
   dotnet build
   ```

3. **Check IDE Analyzer Settings**
   - Visual Studio: Tools → Options → Text Editor → C# → Advanced → Enable "Run code analysis in background"
   - VS Code: Ensure Roslyn analyzers are enabled in the C# extension settings

4. **Verify SDK Version**
   - Analyzers require .NET SDK 5.0 or later
   ```bash
   dotnet --version
   ```

### Performance Concerns

Analyzers run during compilation and design-time builds. If you experience performance issues:

1. **Disable Background Analysis** (temporary, for large solutions)
2. **Use Solution Filters** to work on subsets of large solutions
3. **Adjust Analysis Scope** in IDE settings

## Future Analyzers

The FluentMigrator team is actively developing additional analyzers to help catch more issues at compile time. Check the [GitHub releases](https://github.com/fluentmigrator/fluentmigrator/releases) for announcements of new analyzers.

Potential future analyzers may include:
- Migration method implementation validation
- Transaction behavior best practices
- Database-specific syntax validation
- Performance anti-patterns detection

## Contributing

Interested in contributing new analyzers? Check out the [FluentMigrator contribution guide](https://fluentmigrator.github.io/intro/contributing.html) and the existing analyzer implementation in the `src/FluentMigrator.Analyzers` directory.
