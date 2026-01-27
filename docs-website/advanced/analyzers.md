# Analyzers

::: warning Coming Soon
The FluentMigrator.Analyzers package will be available with FluentMigrator 7.2.0 release. The feature is currently in the main branch and will be published to NuGet.org with the next release.

To use the analyzers before the official release, you can build the package from source or use pre-release versions from the [Azure Artifacts feed](https://dev.azure.com/fluentmigrator/fluentmigrator/_packaging?_a=feed&feed=fluentmigrator).
:::

FluentMigrator provides Roslyn-based code analyzers to help catch common mistakes and enforce best practices at compile time. These analyzers are distributed as a separate NuGet package and integrate directly into your IDE and build process.

## Installation

Once released, the analyzers will be available as a NuGet package that can be added to your migration projects:

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
