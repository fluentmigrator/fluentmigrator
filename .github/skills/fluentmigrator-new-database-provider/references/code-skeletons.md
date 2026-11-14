# Code skeletons

All source files carry the Apache-2.0 `#region License` header used across the repo
(copy it from any existing file, update the year). Namespaces are
`FluentMigrator.Runner.Generators.<Db>` and `FluentMigrator.Runner.Processors.<Db>`;
the DI extension lives in `FluentMigrator.Runner`. Public members need XML doc
comments — `PackageLibrary.props` treats missing docs as warnings and CI is strict.

Table of contents

1. `<Db>DbFactory`
2. `I<Db>TypeMap` / `<Db>TypeMap`
3. `<Db>Quoter`
4. `<Db>Column`
5. `<Db>Generator`
6. `<Db>Processor`
7. `<Db>RunnerBuilderExtensions`
8. Unit test class
9. Integration test class + `<Db>TestTable` helper

---

## 1. `<Db>DbFactory`

```csharp
using System;

namespace FluentMigrator.Runner.Processors.<Db>
{
    public class <Db>DbFactory : ReflectionBasedDbFactory
    {
        // Each TestEntry = (assembly name, DbProviderFactory type name, Type resolver).
        // The third argument is REQUIRED: under NativeAOT, ReflectionBasedDbFactory only
        // tries TestEntry.TypeFactory and then stops (AotSupport.IsDynamicCodeSupported == false).
        // The string must be a compile-time constant so the trimmer can resolve it.
        // List alternatives in preference order; the first that resolves wins.
        private static readonly TestEntry[] _entries =
        {
            new TestEntry("<Driver.Assembly>", "<Namespace>.<FactoryType>",
                () => Type.GetType("<Namespace>.<FactoryType>, <Driver.Assembly>")),
        };

        public <Db>DbFactory(IServiceProvider serviceProvider)
            : base(serviceProvider, _entries)
        {
        }
    }
}
```

`TryCreateFromPreloadedType` looks for a static `Instance` member first, then a public
parameterless constructor. A driver factory with neither cannot be created on the AOT
path; flag it in the ADR as a blocker rather than adding a reflection workaround. Do not
add a `PackageReference` to the driver in the runner `.csproj` — the consuming app
references it, and that reference is what makes `Type.GetType` resolve.

## 2. TypeMap

```csharp
using System.Data;
using FluentMigrator.Runner.Generators.Base;

namespace FluentMigrator.Runner.Generators.<Db>
{
    public interface I<Db>TypeMap : ITypeMap { }

    public class <Db>TypeMap : TypeMapBase, I<Db>TypeMap
    {
        public const int AnsiStringCapacity = 65535;   // from the ADR type table
        public const int UnicodeStringCapacity = 65535;
        public const int DecimalCapacity = 38;
        public const int BinaryCapacity = int.MaxValue;

        public <Db>TypeMap() { SetupTypeMaps(); }   // some providers call this from the base ctor; check TypeMapBase

        protected override void SetupTypeMaps()
        {
            // Unsized default first, then sized variants in ascending size.
            SetTypeMap(DbType.AnsiStringFixedLength, "CHAR(255)");
            SetTypeMap(DbType.AnsiStringFixedLength, "CHAR($size)", AnsiStringCapacity);
            SetTypeMap(DbType.AnsiString, "VARCHAR(255)");
            SetTypeMap(DbType.AnsiString, "VARCHAR($size)", AnsiStringCapacity);
            SetTypeMap(DbType.AnsiString, "TEXT", int.MaxValue);          // overflow mapping
            SetTypeMap(DbType.Binary, "BLOB");
            SetTypeMap(DbType.Binary, "BLOB", BinaryCapacity);
            SetTypeMap(DbType.Boolean, "BOOLEAN");
            SetTypeMap(DbType.Byte, "SMALLINT");
            SetTypeMap(DbType.Currency, "DECIMAL(19,4)");
            SetTypeMap(DbType.Date, "DATE");
            SetTypeMap(DbType.DateTime, "TIMESTAMP");
            SetTypeMap(DbType.DateTime2, "TIMESTAMP");
            SetTypeMap(DbType.DateTimeOffset, "TIMESTAMP WITH TIME ZONE");
            SetTypeMap(DbType.Decimal, "DECIMAL(19,5)");
            SetTypeMap(DbType.Decimal, "DECIMAL($size,$precision)", DecimalCapacity);
            SetTypeMap(DbType.Double, "DOUBLE PRECISION");
            SetTypeMap(DbType.Guid, "UUID");
            SetTypeMap(DbType.Int16, "SMALLINT");
            SetTypeMap(DbType.Int32, "INTEGER");
            SetTypeMap(DbType.Int64, "BIGINT");
            SetTypeMap(DbType.Single, "REAL");
            SetTypeMap(DbType.StringFixedLength, "CHAR(255)");
            SetTypeMap(DbType.StringFixedLength, "CHAR($size)", UnicodeStringCapacity);
            SetTypeMap(DbType.String, "VARCHAR(255)");
            SetTypeMap(DbType.String, "VARCHAR($size)", UnicodeStringCapacity);
            SetTypeMap(DbType.String, "TEXT", int.MaxValue);
            SetTypeMap(DbType.Time, "TIME");
            SetTypeMap(DbType.Xml, "TEXT");
            // Also: SByte, UInt16/32/64, VarNumeric, Object — every DbType in the ADR table.
        }
    }
}
```

Note that in `SetTypeMap(DbType, template, maxSize)` the `$size` placeholder receives
the requested size and `$precision` the precision; `GetTypeMap` picks the smallest
registered `maxSize` >= requested size. Look at `PostgresTypeMap` for the canonical
example with overflow types.

## 3. Quoter

```csharp
using System;
using FluentMigrator.Runner.Generators.Generic;

namespace FluentMigrator.Runner.Generators.<Db>
{
    public class <Db>Quoter : GenericQuoter
    {
        // Only if not the ANSI double quote:
        // public override string OpenQuote => "`";
        // public override string CloseQuote => "`";
        // public override string OpenQuoteEscapeString => "``";
        // public override string CloseQuoteEscapeString => "``";

        // Engines without schemas: return empty so QuoteTableName omits the prefix.
        // public override string QuoteSchemaName(string schemaName) => string.Empty;

        public override string FormatSystemMethods(SystemMethods value)
        {
            switch (value)
            {
                case SystemMethods.NewGuid:            return "gen_random_uuid()";
                case SystemMethods.CurrentDateTime:    return "CURRENT_TIMESTAMP";
                case SystemMethods.CurrentUTCDateTime: return "(CURRENT_TIMESTAMP AT TIME ZONE 'UTC')";
                case SystemMethods.CurrentUser:        return "CURRENT_USER";
            }
            return base.FormatSystemMethods(value);
        }

        // Override literal formats only when the engine deviates:
        // public override string FormatBool(bool value) => value ? "1" : "0";
        // public override string FormatDateTime(DateTime value) => ...;
        // public override string FormatGuid(Guid value) => ...;
        // public override string FormatByteArray(byte[] value) => ...;
    }
}
```

## 4. Column

```csharp
using System;
using System.Collections.Generic;
using System.Linq;
using FluentMigrator.Model;
using FluentMigrator.Runner.Generators.Base;

namespace FluentMigrator.Runner.Generators.<Db>
{
    internal class <Db>Column : ColumnBase<I<Db>TypeMap>
    {
        public <Db>Column(I<Db>TypeMap typeMap, <Db>Quoter quoter)
            : base(typeMap, quoter)
        {
            // Reorder only if the engine requires a different clause order.
            // ClauseOrder = new List<Func<ColumnDefinition, string>> { FormatString, FormatType, FormatNullable, FormatDefaultValue, FormatIdentity, FormatPrimaryKey, FormatCollation };
        }

        protected override string FormatIdentity(ColumnDefinition column)
        {
            return column.IsIdentity ? "GENERATED BY DEFAULT AS IDENTITY" : string.Empty;
        }

        // Common overrides:
        // protected override string FormatNullable(ColumnDefinition column) — engines where NULL must be omitted.
        // protected override string FormatDefaultValue(ColumnDefinition column) — engines needing named defaults.
        // public override bool ShouldPrimaryKeysBeAddedSeparately(...) — true if PK must be a separate ALTER.
        // public override string FormatCascade(string onWhat, Rule rule) — drop SET DEFAULT if unsupported.

        // Needed by Generate(AlterColumnExpression) when the engine splits type/null/default:
        public string GenerateAlterClauses(ColumnDefinition column)
        {
            var clauses = new List<string> { $"ALTER COLUMN {Quoter.QuoteColumnName(column.Name)} TYPE {FormatType(column)}" };
            if (column.IsNullable.HasValue)
                clauses.Add($"ALTER COLUMN {Quoter.QuoteColumnName(column.Name)} {(column.IsNullable.Value ? "DROP NOT NULL" : "SET NOT NULL")}");
            return string.Join(", ", clauses);
        }
    }
}
```

## 5. Generator

```csharp
using System.Collections.Generic;
using FluentMigrator.Expressions;
using FluentMigrator.Runner.Generators.Generic;
using JetBrains.Annotations;
using Microsoft.Extensions.Options;

namespace FluentMigrator.Runner.Generators.<Db>
{
    public class <Db>Generator : GenericGenerator
    {
        // Prefer format-string overrides over Generate() overrides.
        public override string RenameTable => "ALTER TABLE {0} RENAME TO {1}";
        public override string AlterColumn => "ALTER TABLE {0} {1}";
        public override string DropIndex   => "DROP INDEX {0}";

        public <Db>Generator() : this(new <Db>Quoter()) { }

        public <Db>Generator([NotNull] <Db>Quoter quoter)
            : this(quoter, new OptionsWrapper<GeneratorOptions>(new GeneratorOptions())) { }

        public <Db>Generator([NotNull] <Db>Quoter quoter, [NotNull] IOptions<GeneratorOptions> generatorOptions)
            : this(quoter, new <Db>TypeMap(), generatorOptions) { }

        public <Db>Generator([NotNull] <Db>Quoter quoter, [NotNull] I<Db>TypeMap typeMap, [NotNull] IOptions<GeneratorOptions> generatorOptions)
            : base(new <Db>Column(typeMap, quoter), quoter, new EmptyDescriptionGenerator(), generatorOptions) { }

        public override string GeneratorId => GeneratorIdConstants.<Db>;
        public override List<string> GeneratorIdAliases => new List<string> { GeneratorIdConstants.<Db> };

        // ✅ rows with different shape:
        public override string Generate(CreateSchemaExpression expression)
            => FormatStatement(CreateSchema, Quoter.QuoteSchemaName(expression.SchemaName));

        public override string Generate(AlterColumnExpression expression)
        {
            var clauses = ((<Db>Column)Column).GenerateAlterClauses(expression.Column);
            return FormatStatement(AlterColumn, Quoter.QuoteTableName(expression.TableName, expression.SchemaName), clauses);
        }

        // ❌ rows:
        public override string Generate(CreateSequenceExpression expression)
            => CompatibilityMode.HandleCompatibility("Sequences are not supported");

        public override string Generate(DeleteSequenceExpression expression)
            => CompatibilityMode.HandleCompatibility("Sequences are not supported");

        // ⚠️ rows: validate then delegate.
        public override string Generate(CreateIndexExpression expression)
        {
            if (expression.Index.IsClustered)
                return CompatibilityMode.HandleCompatibility("Clustered indexes are not supported");
            return base.Generate(expression);
        }
    }
}
```

`FormatStatement` appends the statement terminator honoured by the unit-test expected
strings (`;` by default). Every unit-test expected string ends with it.

## 6. Processor

```csharp
using System;
using System.Collections.Generic;
using System.Data;
using FluentMigrator.Expressions;
using FluentMigrator.Runner.Generators.<Db>;
using FluentMigrator.Runner.Helpers;
using FluentMigrator.Runner.Initialization;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FluentMigrator.Runner.Processors.<Db>
{
    public class <Db>Processor : GenericProcessorBase
    {
        private readonly <Db>Quoter _quoter = new <Db>Quoter();

        public override string DatabaseType => ProcessorIdConstants.<Db>;
        public override IList<string> DatabaseTypeAliases { get; } = new List<string>();

        [Obsolete("Use the constructor that accepts IMigrationConnectionFactory instead.")]
        public <Db>Processor(
            [NotNull] <Db>DbFactory factory, [NotNull] <Db>Generator generator,
            [NotNull] ILogger<<Db>Processor> logger, [NotNull] IOptionsSnapshot<ProcessorOptions> options,
            [NotNull] IConnectionStringAccessor connectionStringAccessor)
            : base(() => factory.Factory, generator, logger, options.Value, connectionStringAccessor) { }

        [ActivatorUtilitiesConstructor]
        public <Db>Processor(
            [NotNull] <Db>DbFactory factory, [NotNull] <Db>Generator generator,
            [NotNull] ILogger<<Db>Processor> logger, [NotNull] IOptionsSnapshot<ProcessorOptions> options,
            [NotNull] IMigrationConnectionFactory connectionFactory)
            : base(() => factory.Factory, generator, logger, options.Value, connectionFactory) { }

        public override void Execute(string template, params object[] args) => Process(string.Format(template, args));

        public override bool SchemaExists(string schemaName)
            => Exists("SELECT 1 FROM information_schema.schemata WHERE schema_name = '{0}'", FormatToSafeSchemaName(schemaName));

        public override bool TableExists(string schemaName, string tableName)
            => Exists("SELECT 1 FROM information_schema.tables WHERE table_schema = '{0}' AND table_name = '{1}'",
                FormatToSafeSchemaName(schemaName), FormatToSafeName(tableName));

        public override bool ColumnExists(string schemaName, string tableName, string columnName) => /* ADR query */ false;
        public override bool ConstraintExists(string schemaName, string tableName, string constraintName) => /* ADR query */ false;
        public override bool IndexExists(string schemaName, string tableName, string indexName) => /* ADR query */ false;
        public override bool SequenceExists(string schemaName, string sequenceName) => /* ADR query, or false + note in ADR */ false;

        public override bool DefaultValueExists(string schemaName, string tableName, string columnName, object defaultValue)
        {
            var pattern = string.Format("%{0}%", FormatHelper.FormatSqlEscape(defaultValue.ToString()));
            return Exists("SELECT 1 FROM information_schema.columns WHERE table_schema = '{0}' AND table_name = '{1}' AND column_name = '{2}' AND column_default LIKE '{3}'",
                FormatToSafeSchemaName(schemaName), FormatToSafeName(tableName), FormatToSafeName(columnName), pattern);
        }

        public override DataSet ReadTableData(string schemaName, string tableName)
            => Read("SELECT * FROM {0}", _quoter.QuoteTableName(tableName, schemaName));

        public override DataSet Read(string template, params object[] args)
        {
            EnsureConnectionIsOpen();
            using (var command = CreateCommand(string.Format(template, args)))
            using (var reader = command.ExecuteReader())
                return reader.ReadDataSet();
        }

        public override bool Exists(string template, params object[] args)
        {
            EnsureConnectionIsOpen();
            using (var command = CreateCommand(string.Format(template, args)))
            using (var reader = command.ExecuteReader())
                return reader.Read();
        }

        protected override void Process(string sql)
        {
            Logger.LogSql(sql);
            if (Options.PreviewOnly || string.IsNullOrEmpty(sql)) return;
            EnsureConnectionIsOpen();
            using (var command = CreateCommand(sql))
            {
                try { command.ExecuteNonQuery(); }
                catch (Exception ex) { ReThrowWithSql(ex, sql); }
            }
        }

        public override void Process(PerformDBOperationExpression expression)
        {
            Logger.LogSay("Performing DB Operation");
            if (Options.PreviewOnly) return;
            EnsureConnectionIsOpen();
            expression.Operation?.Invoke(Connection, Transaction);
        }

        // Helpers used above (copy from Postgres/Redshift):
        private string FormatToSafeSchemaName(string schemaName) => FormatHelper.FormatSqlEscape(_quoter.UnQuoteSchemaName(schemaName));
        private string FormatToSafeName(string name) => FormatHelper.FormatSqlEscape(_quoter.UnQuote(name));
    }
}
```

Case-folding trap: if the engine upper-cases unquoted identifiers (Oracle, Db2,
Snowflake, Firebird) or lower-cases them (Postgres), `*Exists` must apply the same
folding to the input name, otherwise `Schema.Table("Foo").Exists()` returns false for
a table created as `FOO`. Look at `OracleProcessorBase` / `SnowflakeProcessor` for the
pattern.

## 7. DI registration

```csharp
using FluentMigrator.Runner.Generators.<Db>;
using FluentMigrator.Runner.Processors.<Db>;
using Microsoft.Extensions.DependencyInjection;

namespace FluentMigrator.Runner
{
    public static class <Db>RunnerBuilderExtensions
    {
        public static IMigrationRunnerBuilder Add<Db>(this IMigrationRunnerBuilder builder)
        {
            builder.Services
                .AddScoped<<Db>DbFactory>()
                .AddScoped<<Db>Processor>()
                .AddScoped<IMigrationProcessor>(sp => sp.GetRequiredService<<Db>Processor>())
                .AddScoped<<Db>Quoter>()
                .AddScoped<I<Db>TypeMap>(sp => new <Db>TypeMap())
                .AddScoped<<Db>Generator>()
                .AddScoped<IMigrationGenerator>(sp => sp.GetRequiredService<<Db>Generator>());
            return builder;
        }
    }
}
```

Both `IMigrationProcessor` and `IMigrationGenerator` are registered as *additional*
implementations; `SelectingProcessorAccessor` picks the one whose `DatabaseType` /
aliases match the configured processor id. Registering only the concrete type breaks
selection.

## 8. Unit test class

```csharp
using FluentMigrator.Exceptions;
using FluentMigrator.Runner;
using FluentMigrator.Runner.Generators.<Db>;
using NUnit.Framework;
using Shouldly;

namespace FluentMigrator.Tests.Unit.Generators.<Db>
{
    [TestFixture]
    public class <Db>SequenceTests : BaseSequenceTests
    {
        private <Db>Generator _generator;

        [SetUp]
        public void Setup() => _generator = new <Db>Generator();

        [Test]
        public override void CanCreateSequenceWithCustomSchema()
        {
            var expression = GeneratorTestHelper.GetCreateSequenceExpression();
            expression.Sequence.SchemaName = "TestSchema";

            var result = _generator.Generate(expression);
            result.ShouldBe("CREATE SEQUENCE \"TestSchema\".\"Sequence\" INCREMENT BY 2 MINVALUE 0 MAXVALUE 100 START WITH 2 CACHE 10 CYCLE;");
        }

        // For a ❌ row, assert both modes:
        [Test]
        public override void CanDropSequenceWithCustomSchema()
        {
            var expression = GeneratorTestHelper.GetDeleteSequenceExpression();
            expression.SchemaName = "TestSchema";

            _generator.CompatibilityMode = CompatibilityMode.STRICT;
            Should.Throw<DatabaseOperationNotSupportedException>(() => _generator.Generate(expression));

            _generator.CompatibilityMode = CompatibilityMode.LOOSE;
            _generator.Generate(expression).ShouldBeEmpty();
        }
    }
}
```

## 9. Integration test class and helper

```csharp
[TestFixture]
[Category("Integration")]
[Category("<Db>")]
public class <Db>TableTests : BaseTableTests
{
    private ServiceProvider ServiceProvider { get; set; }
    private IServiceScope ServiceScope { get; set; }
    private <Db>Processor Processor { get; set; }
    private <Db>Quoter Quoter { get; set; }

    [OneTimeSetUp]
    public void ClassSetUp()
    {
        IntegrationTestOptions.<Db>.IgnoreIfNotEnabled();
        var services = ServiceCollectionExtensions.CreateServices()
            .ConfigureRunner(r => r.Add<Db>())
            .AddScoped<IConnectionStringReader>(_ => new PassThroughConnectionStringReader(IntegrationTestOptions.<Db>.ConnectionString));
        ServiceProvider = services.BuildServiceProvider();
    }

    [OneTimeTearDown] public void ClassTearDown() => ServiceProvider?.Dispose();

    [SetUp]
    public void SetUp()
    {
        ServiceScope = ServiceProvider.CreateScope();
        Processor = ServiceScope.ServiceProvider.GetRequiredService<<Db>Processor>();
        Quoter = ServiceScope.ServiceProvider.GetRequiredService<<Db>Quoter>();
    }

    [TearDown]
    public void TearDown()
    {
        ServiceScope?.Dispose();
        Processor?.Dispose();
    }

    [Test]
    public override void CallingTableExistsReturnsTrueIfTableExists()
    {
        using (var table = new <Db>TestTable(Processor, null, $"{Quoter.Quote("id")} INTEGER"))
            Processor.TableExists(null, table.Name).ShouldBeTrue();
    }
    // ... remaining abstract tests
}
```

`Helpers/<Db>TestTable.cs`: copy `SnowflakeTestTable`/`PostgresTestTable` — constructor
takes `(processor, schema, params string[] columnDefinitions)`, generates a unique name
(`Table_` + `Guid.NewGuid().GetHashCode().ToString("X8")`), creates the schema if given,
creates the table, and `Dispose()` drops both. Add `WithIndexOn`, `WithUniqueConstraintOn`,
`WithDefaultValueOn` helpers as the constraint/index/column tests need them.

`<Db>SchemaExtensionsTests` covers the `Schema.Schema(...).Table(...).Column(...)`
expression-root API and needs no helper beyond the table fixture.
