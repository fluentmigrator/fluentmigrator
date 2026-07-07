# Tutorial: Extending the Generator Pipeline with `ISupportAdditionalFeatures`

This tutorial walks you through adding a database-specific option to the FluentMigrator fluent API — from defining the feature key right through to writing the unit tests — using the `ISupportAdditionalFeatures` mechanism.

**What you will build:** A custom PostgreSQL `CREATE SCHEMA … AUTHORIZATION owner` extension that integrates cleanly with the fluent API, works in strict mode, and is fully unit-tested.

**Prerequisites:**

- Familiarity with FluentMigrator migrations and the fluent `Create.`/`Alter.` API
- A .NET project that already uses FluentMigrator with a PostgreSQL provider

---

## Background: Why `ISupportAdditionalFeatures`?

FluentMigrator uses an *expression/generator* pipeline:

1. A **builder** (e.g. `CreateSchemaExpressionBuilder`) accumulates options into an **expression** (e.g. `CreateSchemaExpression`).
2. The **expression** is handed to a **generator** (e.g. `Postgres15_0Generator`) which produces a SQL string.
3. The **processor** executes the SQL string against the database.

When you write a custom fluent extension, you need a way to store your option at step 1 and read it again at step 2. `ISupportAdditionalFeatures` is exactly that bridge: it exposes a `Dictionary<string, object>` on the expression (or model) so that extension methods can store arbitrary data that the generator can later retrieve.

This pattern is already used throughout FluentMigrator:

- `SqlServerExtensions.Authorization` stores the schema owner name on `CreateSchemaExpression`
- `PostgresExtensions.UsingBTree` stores an index algorithm on `CreateIndexExpression`

You can follow the same pattern for your own database-specific options.

---

## Part 1 — Scaffold the Project

For this tutorial assume you have a class library project `MyDatabase.Migrations` that references:

```xml
<!-- Use the latest stable versions from NuGet -->
<PackageReference Include="FluentMigrator" Version="6.*" />
<PackageReference Include="FluentMigrator.Runner.Postgres" Version="6.*" />
<PackageReference Include="FluentMigrator.Extensions.Postgres" Version="6.*" />
```

You will create three files:

| File | Purpose |
|---|---|
| `MyDatabaseExtensions.cs` | Feature keys and fluent extension methods |
| `MyCustomPostgresGenerator.cs` | Generator subclass that produces the extra SQL |
| `ServiceCollectionExtensions.cs` | DI wiring |

---

## Part 2 — Define the Feature Key

Every additional feature needs a **unique string key** that acts as its dictionary key inside the expression. Use a namespaced prefix so your keys never collide with FluentMigrator's own keys or those of other extension packages.

Create `MyDatabaseExtensions.cs`:

```csharp
using System;
using FluentMigrator.Builders.Create.Schema;
using FluentMigrator.Infrastructure;
using FluentMigrator.Infrastructure.Extensions;

namespace MyDatabase.Migrations.Extensions
{
    /// <summary>
    /// Fluent API extensions and feature keys for MyDatabase-specific features.
    /// </summary>
    public static partial class MyDatabaseExtensions
    {
        // ---------------------------------------------------------------
        // Feature keys — unique, namespaced strings stored in expressions
        // ---------------------------------------------------------------

        /// <summary>
        /// Key for the schema owner/authorization option on CREATE SCHEMA.
        /// </summary>
        public const string SchemaOwner = "MyDatabase:SchemaOwner";

        // ---------------------------------------------------------------
        // Private helper shared across all extension methods in this class
        // ---------------------------------------------------------------

        private static string UnsupportedMethodMessage(string methodName)
            => $"The '{methodName}' extension method requires the expression to implement "
             + $"{nameof(ISupportAdditionalFeatures)}. "
             + $"Make sure you are using a FluentMigrator builder that supports additional features.";
    }
}
```

> **Why `partial`?**
> Declaring the class as `partial` lets you add more feature keys and extension methods in
> separate files (e.g. `MyDatabaseExtensions.Index.cs`) as the library grows, while still
> sharing the private `UnsupportedMethodMessage` helper.

---

## Part 3 — Write the Extension Method

Still in `MyDatabaseExtensions.cs` (or a companion file), add the fluent extension method.
The method:
1. Casts the builder to `ISupportAdditionalFeatures` — if the cast fails the expression type
   doesn't support additional features, so a helpful exception is thrown immediately.
2. Stores the value under the key defined above.
3. Returns the *original* interface type so the fluent chain can continue.

```csharp
// In MyDatabase.Migrations.Extensions — same partial class
public static partial class MyDatabaseExtensions
{
    /// <summary>
    /// Sets the authorization (owner) role for the schema being created.
    /// Generates: <c>CREATE SCHEMA "name" AUTHORIZATION "role";</c>
    /// </summary>
    /// <param name="expression">The schema creation syntax builder.</param>
    /// <param name="ownerRole">The database role that will own the schema.</param>
    /// <returns>The same builder so calls can be chained.</returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown when <paramref name="expression"/> does not implement
    /// <see cref="ISupportAdditionalFeatures"/>.
    /// </exception>
    public static ICreateSchemaOptionsSyntax OwnedBy(
        this ICreateSchemaOptionsSyntax expression,
        string ownerRole)
    {
        var additionalFeatures = expression as ISupportAdditionalFeatures
            ?? throw new InvalidOperationException(
                UnsupportedMethodMessage(nameof(OwnedBy)));

        additionalFeatures.SetAdditionalFeature(SchemaOwner, ownerRole);
        return expression;
    }
}
```

> **Tip:** `SetAdditionalFeature<T>` and `GetAdditionalFeature<T>` / `TryGetAdditionalFeature<T>`
> live in the `FluentMigrator.Infrastructure.Extensions` namespace.
> `SetAdditionalFeature` is type-safe and more readable than writing directly to the
> `AdditionalFeatures` dictionary.

---

## Part 4 — Create the Custom Generator

The generator is where the stored value is read back and incorporated into the SQL string.
Subclass the generator for your target database version and override the `Generate` overload
that handles `CreateSchemaExpression`.

Create `MyCustomPostgresGenerator.cs`:

```csharp
using FluentMigrator.Expressions;
using FluentMigrator.Infrastructure.Extensions;
using FluentMigrator.Runner.Generators;
using FluentMigrator.Runner.Generators.Postgres;
using Microsoft.Extensions.Options;

using MyDatabase.Migrations.Extensions;

namespace MyDatabase.Migrations.Generators
{
    /// <summary>
    /// Extends the standard PostgreSQL 15.0 generator with MyDatabase-specific features.
    /// </summary>
    public class MyCustomPostgresGenerator : Postgres15_0Generator
    {
        public MyCustomPostgresGenerator(
            PostgresQuoter quoter,
            IOptions<GeneratorOptions> generatorOptions,
            IPostgresTypeMap typeMap)
            : base(quoter, generatorOptions, typeMap)
        {
        }

        // ------------------------------------------------------------------
        // Override Generate to append AUTHORIZATION when SchemaOwner is set
        // ------------------------------------------------------------------

        /// <inheritdoc />
        public override string Generate(CreateSchemaExpression expression)
        {
            // Delegate to the base generator first so we always produce valid SQL.
            var sql = base.Generate(expression);

            // TryGetAdditionalFeature returns false when the key is absent,
            // keeping this generator fully backward-compatible.
            if (expression.TryGetAdditionalFeature<string>(
                    MyDatabaseExtensions.SchemaOwner, out var owner)
                && !string.IsNullOrEmpty(owner))
            {
                // Quoter.QuoteSchemaName properly quotes the role identifier.
                sql = sql.TrimEnd(';').TrimEnd()
                    + $" AUTHORIZATION {Quoter.QuoteSchemaName(owner)};";
            }

            return sql;
        }

        // ------------------------------------------------------------------
        // Declare supported features for strict-mode compatibility checks
        // ------------------------------------------------------------------

        /// <inheritdoc />
        public override bool IsAdditionalFeatureSupported(string feature)
            => feature == MyDatabaseExtensions.SchemaOwner
            || base.IsAdditionalFeatureSupported(feature);
    }
}
```

> **Why override `IsAdditionalFeatureSupported`?**
> FluentMigrator's `CompatibilityMode.STRICT` validates that every `AdditionalFeature` key
> on an expression is known to the active generator. Without this override, a migration that
> calls `.OwnedBy(…)` would throw a "feature not supported in strict mode" error. Returning
> `true` for your own key — and delegating everything else to `base` — prevents that.

---

## Part 5 — Wire Up Dependency Injection

FluentMigrator registers generators as scoped services. After calling `AddPostgres()`,
replace the `Postgres15_0Generator` registration so the processor uses your subclass.

Create `ServiceCollectionExtensions.cs`:

```csharp
using FluentMigrator.Runner.Generators;
using FluentMigrator.Runner.Generators.Postgres;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

using MyDatabase.Migrations.Generators;

namespace MyDatabase.Migrations
{
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Registers MyDatabase's custom PostgreSQL generator,
        /// replacing the default <see cref="Postgres15_0Generator"/>.
        /// Call this <em>after</em> <c>.AddPostgres()</c>.
        /// </summary>
        public static IServiceCollection AddMyDatabaseGeneratorExtensions(
            this IServiceCollection services)
        {
            services.Replace(
                ServiceDescriptor.Scoped<Postgres15_0Generator>(sp =>
                    new MyCustomPostgresGenerator(
                        sp.GetRequiredService<PostgresQuoter>(),
                        sp.GetRequiredService<IOptions<GeneratorOptions>>(),
                        sp.GetRequiredService<IPostgresTypeMap>())));

            return services;
        }
    }
}
```

Register it alongside the standard FluentMigrator setup in your application:

```csharp
services.AddFluentMigratorCore()
    .ConfigureRunner(rb => rb
        .AddPostgres()
        .WithGlobalConnectionString("Host=localhost;Port=5432;Database=myapp;Username=myuser;Password=mypassword")
        .ScanIn(typeof(MyFirstMigration).Assembly).For.Migrations())
    .AddMyDatabaseGeneratorExtensions();  // ← register the custom generator
```

---

## Part 6 — Use the Extension in a Migration

With the DI wiring in place, any migration can call `.OwnedBy(…)`:

```csharp
using FluentMigrator;
using MyDatabase.Migrations.Extensions;

namespace MyDatabase.Migrations
{
    [Migration(20240101120000)]
    public class CreateReportingSchema : Migration
    {
        public override void Up()
        {
            Create.Schema("reporting")
                .OwnedBy("reporting_role");  // custom extension — stored in AdditionalFeatures
        }

        public override void Down()
        {
            Delete.Schema("reporting");
        }
    }
}
```

When FluentMigrator runs `Up()`, the generator produces:

```sql
CREATE SCHEMA "reporting" AUTHORIZATION "reporting_role";
```

---

## Part 7 — Write Unit Tests

Tests are split across three independent layers so failures are easy to diagnose.

### 7.1 Test the Extension Method in Isolation

These tests verify that calling `.OwnedBy(…)` correctly stores the value in
`AdditionalFeatures`. No generator, no database, no DI required.

```csharp
using System;
using FluentMigrator.Builders.Create.Schema;
using FluentMigrator.Expressions;
using FluentMigrator.Infrastructure.Extensions;
using MyDatabase.Migrations.Extensions;
using NUnit.Framework;
using Shouldly;

namespace MyDatabase.Migrations.Tests
{
    [TestFixture]
    [Category("Unit")]
    public class MyDatabaseExtensionsTests
    {
        [Test]
        public void OwnedBy_StoresOwnerRoleInAdditionalFeatures()
        {
            var expression = new CreateSchemaExpression { SchemaName = "reporting" };
            var builder = new CreateSchemaExpressionBuilder(expression);

            builder.OwnedBy("reporting_role");

            expression.TryGetAdditionalFeature<string>(
                MyDatabaseExtensions.SchemaOwner, out var storedValue).ShouldBeTrue();
            storedValue.ShouldBe("reporting_role");
        }

        [Test]
        public void OwnedBy_ThrowsInvalidOperationException_WhenBuilderDoesNotSupportAdditionalFeatures()
        {
            var unsupportedBuilder = new UnsupportedSchemaSyntax();

            Should.Throw<InvalidOperationException>(() =>
                unsupportedBuilder.OwnedBy("reporting_role"));
        }

        // Minimal stub: implements ICreateSchemaOptionsSyntax but NOT ISupportAdditionalFeatures
        private sealed class UnsupportedSchemaSyntax : ICreateSchemaOptionsSyntax { }
    }
}
```

### 7.2 Test the Generator in Isolation

These tests verify that the generator produces the correct SQL. No migration, no DI required.

```csharp
using FluentMigrator.Expressions;
using FluentMigrator.Infrastructure.Extensions;
using FluentMigrator.Runner.Generators.Postgres;
using Microsoft.Extensions.Options;
using MyDatabase.Migrations.Extensions;
using MyDatabase.Migrations.Generators;
using NUnit.Framework;
using Shouldly;

namespace MyDatabase.Migrations.Tests
{
    [TestFixture]
    [Category("Unit")]
    public class MyCustomPostgresGeneratorTests
    {
        private MyCustomPostgresGenerator _generator;

        [SetUp]
        public void SetUp()
        {
            _generator = new MyCustomPostgresGenerator(
                new PostgresQuoter(),
                new OptionsWrapper<GeneratorOptions>(new GeneratorOptions()),
                new PostgresTypeMap());
        }

        [Test]
        public void Generate_CreateSchema_WithOwner_EmitsAuthorizationClause()
        {
            var expression = new CreateSchemaExpression { SchemaName = "reporting" };
            expression.SetAdditionalFeature(MyDatabaseExtensions.SchemaOwner, "reporting_role");

            var sql = _generator.Generate(expression);

            sql.ShouldBe("CREATE SCHEMA \"reporting\" AUTHORIZATION \"reporting_role\";");
        }

        [Test]
        public void Generate_CreateSchema_WithoutOwner_OmitsAuthorizationClause()
        {
            var expression = new CreateSchemaExpression { SchemaName = "reporting" };

            var sql = _generator.Generate(expression);

            sql.ShouldBe("CREATE SCHEMA \"reporting\";");
        }

        [Test]
        public void IsAdditionalFeatureSupported_ReturnsTrue_ForSchemaOwnerKey()
        {
            _generator.IsAdditionalFeatureSupported(MyDatabaseExtensions.SchemaOwner)
                .ShouldBeTrue();
        }

        [Test]
        public void IsAdditionalFeatureSupported_ReturnsFalse_ForUnknownKey()
        {
            _generator.IsAdditionalFeatureSupported("UnknownKey:ShouldNotExist")
                .ShouldBeFalse();
        }
    }
}
```

### 7.3 Test the Full Migration Pipeline

This end-to-end test verifies that the migration class produces the right expression when
`Up()` is called, using a mocked `IMigrationContext`. It bridges the extension method and
the expression together without needing a live database.

```csharp
using System.Collections.Generic;
using System.Linq;
using FluentMigrator.Expressions;
using FluentMigrator.Infrastructure;
using FluentMigrator.Infrastructure.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using MyDatabase.Migrations.Extensions;
using NUnit.Framework;
using Shouldly;

namespace MyDatabase.Migrations.Tests
{
    [TestFixture]
    [Category("Unit")]
    public class CreateReportingSchemaMigrationTests
    {
        [Test]
        public void Up_ProducesCreateSchemaExpression_WithCorrectSchemaNameAndOwner()
        {
            // Arrange — build a mock context that captures expressions
            var expressions = new List<IMigrationExpression>();

            var contextMock = new Mock<IMigrationContext>();
            contextMock.SetupGet(x => x.Expressions).Returns(expressions);
            contextMock.SetupGet(x => x.ServiceProvider)
                .Returns(new ServiceCollection().BuildServiceProvider());

            // Act
            var migration = new CreateReportingSchema();
            migration.GetUpExpressions(contextMock.Object);

            // Assert — exactly one CreateSchemaExpression was added
            var schemaExpr = expressions
                .OfType<CreateSchemaExpression>()
                .ShouldHaveSingleItem();

            schemaExpr.SchemaName.ShouldBe("reporting");

            // Assert — SchemaOwner additional feature is set
            schemaExpr.TryGetAdditionalFeature<string>(
                MyDatabaseExtensions.SchemaOwner, out var owner).ShouldBeTrue();
            owner.ShouldBe("reporting_role");
        }
    }
}
```

> **Tip:** Keep these three layers separate. Extension method tests catch problems with how
> the option is stored; generator tests catch problems with the SQL; pipeline tests catch
> wiring problems. A failure in any one layer pinpoints the problem immediately.

---

## Summary

You have learned how to:

1. **Define a feature key** — a namespaced string constant that identifies your option.
2. **Write an extension method** — casts to `ISupportAdditionalFeatures`, stores the value with `SetAdditionalFeature`, returns the builder for chaining.
3. **Subclass the generator** — overrides `Generate`, reads the value with `TryGetAdditionalFeature`, and overrides `IsAdditionalFeatureSupported` for strict-mode compatibility.
4. **Register the generator** — replaces the default scoped registration after `AddPostgres()`.
5. **Use the extension** — call it just like any other FluentMigrator fluent method.
6. **Test all three layers** — extension method, generator, and pipeline tests independently.

This same pattern applies to any `ISupportAdditionalFeatures`-implementing expression or model: `CreateIndexExpression`, `IndexDefinition`, `ColumnDefinition`, `ConstraintDefinition`, and others. You can extend them all in the same way.
