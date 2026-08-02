#region License
// Copyright (c) 2018, Fluent Migrator Project
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
#endregion

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using FluentMigrator.Generation;
using FluentMigrator.Runner;
using FluentMigrator.Runner.Generators;
using FluentMigrator.Runner.Initialization;
using FluentMigrator.Runner.Processors;
using FluentMigrator.Runner.Processors.Postgres;
using FluentMigrator.Runner.Processors.Snowflake;

using Microsoft.Extensions.DependencyInjection;

using NUnit.Framework;

using Shouldly;

using VerifyNUnit;

// FluentMigrator.Runner.Generators also declares an IQuoter; the token replacer takes the
// FluentMigrator.Generation one.
using IQuoter = FluentMigrator.Generation.IQuoter;

namespace FluentMigrator.Tests.Unit.TokenSubstitution
{
    /// <summary>
    /// Path coverage for <see cref="SqlScriptTokenReplacer"/> across the full
    /// <c>IQuoter</c> / <c>ITypeMap</c> / <c>IMigrationGenerator</c> product.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <strong>Read <c>TokenSubstitutionMatrix.md</c> (beside this file) first.</strong> It documents
    /// every axis, why it exists, and what each assertion claims. This file is the executable form of
    /// that document.
    /// </para>
    /// <para>
    /// The axes multiply out to roughly 35,000 combinations, so the configuration axes (dialect,
    /// quoter switch, typemap switch, generator switch) are multiplexed into <see cref="TestCaseSource"/>
    /// while the value x token-style grid becomes a snapshot payload. A regression surfaces as a
    /// one-cell diff in a markdown table rather than one of 35,000 opaque case names.
    /// </para>
    /// <para>Origin: https://github.com/fluentmigrator/fluentmigrator/issues/2336</para>
    /// </remarks>
    [TestFixture]
    [Category("SqlScriptTokenReplacer")]
    [Category("TokenSubstitutionMatrix")]
    public class TokenSubstitutionMatrixTests
    {
        private const string RawToken = "$(x)";
        private const string SafeToken = "$[x]";

        // ------------------------------------------------------------------ axes 4-7: configuration

        /// <summary>
        /// One fully-resolvable DI configuration.
        /// </summary>
        public sealed class DialectCase
        {
            public DialectCase(string id, string family, Action<IMigrationRunnerBuilder> configure, string typeMapVariantOf = null)
            {
                Id = id;
                Family = family;
                Configure = configure;
                TypeMapVariantOf = typeMapVariantOf;
            }

            public string Id { get; }

            public string Family { get; }

            public Action<IMigrationRunnerBuilder> Configure { get; }

            /// <summary>
            /// Gets the id of the sibling case this one differs from by an <c>ITypeMap</c> switch only.
            /// Drives <see cref="TypeMapSwitch_DoesNotAffect_TokenExpansion"/>.
            /// </summary>
            public string TypeMapVariantOf { get; }

            public override string ToString() => Id;
        }

        private static IEnumerable<DialectCase> DialectCases()
        {
            // --- SQL Server -------------------------------------------------------------------
            // The unversioned entry point is a separate registration block, not a delegation to the
            // versioned one - AddSqlServer() wires SqlServer2016Processor/Generator and
            // SqlServer2008Quoter in its own body. It is also the form most user code calls, so it
            // is pinned here rather than assumed to match AddSqlServer2016().
            yield return new DialectCase("SqlServer(unversioned)", "SqlServer", b => b.AddSqlServer());
            yield return new DialectCase("SqlServer2000", "SqlServer", b => b.AddSqlServer2000());
            yield return new DialectCase("SqlServer2005", "SqlServer", b => b.AddSqlServer2005());
            yield return new DialectCase("SqlServer2008", "SqlServer", b => b.AddSqlServer2008());
            yield return new DialectCase("SqlServer2012", "SqlServer", b => b.AddSqlServer2012());
            yield return new DialectCase("SqlServer2014", "SqlServer", b => b.AddSqlServer2014());
            yield return new DialectCase("SqlServer2016", "SqlServer", b => b.AddSqlServer2016());

            // --- MySQL: MySql4/5/8 differ only by ITypeMap, so their matrices must be identical.
            // Likewise its own block, wiring MySql8Processor/Generator.
            yield return new DialectCase("MySql(unversioned)", "MySql", b => b.AddMySql());
            yield return new DialectCase("MySql4", "MySql", b => b.AddMySql4());
            yield return new DialectCase("MySql5", "MySql", b => b.AddMySql5(), typeMapVariantOf: "MySql4");
            yield return new DialectCase("MySql8", "MySql", b => b.AddMySql8(), typeMapVariantOf: "MySql4");

            // --- Postgres: version lineage x ForceQuote (an IQuoter switch).
            //     PostgresOptions is resolved as a plain scoped service and the provider registers it
            //     with TryAdd semantics, so pre-registering here wins.
            foreach (var forceQuote in new[] { true, false })
            {
                var fq = forceQuote;

                Action<IMigrationRunnerBuilder> WithOptions(Action<IMigrationRunnerBuilder> add) =>
                    b =>
                    {
                        b.Services.AddScoped(_ => new PostgresOptions { ForceQuote = fq });
                        add(b);
                    };

                yield return new DialectCase($"Postgres(ForceQuote={fq})", "Postgres", WithOptions(x => x.AddPostgres()));
                yield return new DialectCase($"Postgres92(ForceQuote={fq})", "Postgres", WithOptions(x => x.AddPostgres92()));
                yield return new DialectCase($"Postgres10_0(ForceQuote={fq})", "Postgres", WithOptions(x => x.AddPostgres10_0()));
                yield return new DialectCase($"Postgres11_0(ForceQuote={fq})", "Postgres", WithOptions(x => x.AddPostgres11_0()));
                yield return new DialectCase($"Postgres15_0(ForceQuote={fq})", "Postgres", WithOptions(x => x.AddPostgres15_0()));
            }

            // --- SQLite: the only provider exposing all three switches independently.
            //     binaryGuid   -> IQuoter
            //     strictTables -> ITypeMap    (must NOT move any cell)
            //     compatMode   -> IMigrationGenerator
            foreach (var binaryGuid in new[] { false, true })
            {
                foreach (var strictTables in new[] { false, true })
                {
                    foreach (var compat in new CompatibilityMode?[] { null, CompatibilityMode.STRICT, CompatibilityMode.LOOSE })
                    {
                        var bg = binaryGuid;
                        var st = strictTables;
                        var cm = compat;

                        var compatLabel = cm?.ToString() ?? "null";
                        var id = $"SQLite(guid={bg},strict={st},compat={compatLabel})";
                        var baseline = $"SQLite(guid={bg},strict=False,compat={compatLabel})";

                        yield return new DialectCase(
                            id,
                            "SQLite",
                            b => b.AddSQLite(binaryGuid: bg, useStrictTables: st, compatibilityMode: cm),
                            typeMapVariantOf: st ? baseline : null);
                    }
                }
            }

            // --- Snowflake: QuoteIdentifiers is an IQuoter switch.
            foreach (var quoteIdentifiers in new[] { true, false })
            {
                var qi = quoteIdentifiers;
                yield return new DialectCase(
                    $"Snowflake(QuoteIdentifiers={qi})",
                    "Snowflake",
                    b => b.AddSnowflake(new SnowflakeOptions { QuoteIdentifiers = qi }));
            }

            // --- Oracle: OracleQuoter vs OracleQuoterQuotedIdentifier are distinct registrations.
            yield return new DialectCase("Oracle", "Oracle", b => b.AddOracle());
            yield return new DialectCase("OracleManaged", "Oracle", b => b.AddOracleManaged());
            yield return new DialectCase("DotConnectOracle", "Oracle", b => b.AddDotConnectOracle());
            yield return new DialectCase("Oracle12C", "Oracle", b => b.AddOracle12C());
            yield return new DialectCase("Oracle12CManaged", "Oracle", b => b.AddOracle12CManaged());
            yield return new DialectCase("DotConnectOracle12C", "Oracle", b => b.AddDotConnectOracle12C());

            // --- remainder --------------------------------------------------------------------
            yield return new DialectCase("Db2", "Other", b => b.AddDb2());
            yield return new DialectCase("Db2ISeries", "Other", b => b.AddDb2ISeries());
            yield return new DialectCase("Firebird", "Other", b => b.AddFirebird());
            yield return new DialectCase("Hana(unversioned)", "Other", b => b.AddHana());
            yield return new DialectCase("Hana8", "Other", b => b.AddHana8());
            yield return new DialectCase("Hana10", "Other", b => b.AddHana10());
            yield return new DialectCase("Redshift", "Other", b => b.AddRedshift());
        }

        private static IEnumerable<string> Families() =>
            DialectCases().Select(d => d.Family).Distinct();

        // ------------------------------------------------------------------ axis 3: values

        public sealed class ValueCase
        {
            public ValueCase(string label, object value)
            {
                Label = label;
                Value = value;
            }

            public string Label { get; }

            public object Value { get; }

            public override string ToString() => Label;
        }

        /// <summary>
        /// One entry per branch of <c>GenericQuoter.QuoteValue</c>, in source order, plus the
        /// uncased integral types that fall through to <c>value.ToString()</c>, plus the RawSql forms.
        /// See the "Value cases" table in <c>TokenSubstitutionMatrix.md</c>.
        /// </summary>
        private static IEnumerable<ValueCase> ValueCases()
        {
            yield return new ValueCase("null", null);
            yield return new ValueCase("DBNull", DBNull.Value);
            yield return new ValueCase("string", "O'Brien");
            yield return new ValueCase("NonUnicodeString", new NonUnicodeString("ansi"));
            yield return new ValueCase("char", 'x');
            yield return new ValueCase("bool", true);
            yield return new ValueCase("Guid", new Guid("11111111-2222-3333-4444-555555555555"));
            yield return new ValueCase("DateTime", new DateTime(2026, 7, 28, 13, 45, 6));
            yield return new ValueCase("DateTimeOffset", new DateTimeOffset(2026, 7, 28, 13, 45, 6, TimeSpan.FromHours(-4)));
            yield return new ValueCase("double", 1234.5678d);
            yield return new ValueCase("float", 1234.5f);
            yield return new ValueCase("decimal", 1234.5678m);
            yield return new ValueCase("int", -1234);

            // Not cased in GenericQuoter.QuoteValue: these reach value.ToString() and are therefore
            // culture-sensitive on the DML path. Pinned deliberately so a change is visible.
            yield return new ValueCase("long(uncased)", -1234L);
            yield return new ValueCase("short(uncased)", (short)-12);
            yield return new ValueCase("byte(uncased)", (byte)7);

            yield return new ValueCase("SystemMethods", SystemMethods.CurrentDateTime);
            yield return new ValueCase("Enum", DayOfWeek.Tuesday);
            yield return new ValueCase("byte[]", new byte[] { 0xDE, 0xAD });
            yield return new ValueCase("TimeSpan", new TimeSpan(1, 2, 3));

            // The subjects of #2332 and #2335.
            yield return new ValueCase("RawSql", RawSql.Insert("CURRENT_TIMESTAMP"));
            yield return new ValueCase("RawSql+backslash", RawSql.Insert(@"c REGEXP '\d+'"));
            yield return new ValueCase("RawSql+quote", RawSql.Insert("name = 'O''Brien'"));
        }

        // ------------------------------------------------------------------ container plumbing

        private sealed class Resolved : IDisposable
        {
            private readonly ServiceProvider _root;
            private readonly IServiceScope _scope;

            public Resolved(DialectCase dialect)
            {
                var services = new ServiceCollection()
                    .AddLogging()
                    .AddFluentMigratorCore()
                    .ConfigureRunner(rb =>
                    {
                        dialect.Configure(rb);
                        rb.WithGlobalConnectionString("Data Source=:memory:");
                    });

                _root = services.BuildServiceProvider(validateScopes: false);
                _scope = _root.CreateScope();

                Generator = _scope.ServiceProvider.GetRequiredService<IMigrationGenerator>();
                GeneratorQuoter = Generator.Quoter;

                // Deliberately resolved through IMigrationProcessor, because that is what
                // ExecuteSqlStatementExpression.ExecuteWith hands to the token replacer. Some
                // processors shadow ProcessorBase.Quoter with `public new IQuoter Quoter`.
                ProcessorQuoter = _scope.ServiceProvider.GetRequiredService<IMigrationProcessor>().Quoter;
            }

            public IMigrationGenerator Generator { get; }

            public IQuoter GeneratorQuoter { get; }

            public IQuoter ProcessorQuoter { get; }

            public void Dispose()
            {
                _scope?.Dispose();
                _root?.Dispose();
            }
        }

        /// <summary>
        /// Providers whose processor cannot be constructed without a native driver assembly are
        /// reported Inconclusive rather than silently skipped.
        /// </summary>
        private static Resolved Resolve(DialectCase dialect)
        {
            try
            {
                return new Resolved(dialect);
            }
            catch (Exception ex)
            {
                Assert.Inconclusive($"{dialect.Id}: not constructible in this environment - {ex.GetType().Name}: {ex.Message}");
                throw;
            }
        }

        // ------------------------------------------------------------------ rendering

        private static IDictionary<string, object> Params(object value) =>
            new Dictionary<string, object> { ["x"] = value };

        private static string Expand(string template, object value, IQuoter quoter)
        {
            try
            {
                return SqlScriptTokenReplacer.ReplaceSqlScriptTokens(template, Params(value), quoter) ?? "<null>";
            }
            catch (Exception ex)
            {
                return $"<throws {ex.GetType().Name}>";
            }
        }

        private static string QuoteValueOrThrow(IQuoter quoter, object value)
        {
            try
            {
                return quoter.QuoteValue(value) ?? "<null>";
            }
            catch (Exception ex)
            {
                return $"<throws {ex.GetType().Name}>";
            }
        }

        /// <summary>
        /// Renders the value x token-style grid for one resolved configuration as a markdown table.
        /// This is the snapshot payload.
        /// </summary>
        private static string RenderMatrix(DialectCase dialect, Resolved r)
        {
            var sb = new StringBuilder();

            sb.AppendLine($"### {dialect.Id}");
            sb.AppendLine();
            sb.AppendLine($"- generator: `{r.Generator.GetType().Name}`");
            sb.AppendLine($"- generator.Quoter: `{r.GeneratorQuoter.GetType().Name}`");
            sb.AppendLine($"- processor.Quoter: `{r.ProcessorQuoter.GetType().Name}`");
            sb.AppendLine();
            // There was once a "no quoter" column here, recording what the private QuoteSqlLiteral
            // fallback rendered. That fallback is gone: a null quoter now throws, identically for
            // every value and every dialect, so the column carried one fact repeated 161 times.
            // The contract it documented is covered once, in SqlScriptTokenReplacerTests, by
            // ThrowsWhenSafeTokenIsExpandedWithoutAQuoter and its two companions.
            sb.AppendLine("| value | `$(x)` | `$[x]` w/ quoter | QuoteValue (DML) |");
            sb.AppendLine("|---|---|---|---|");

            foreach (var v in ValueCases())
            {
                sb.AppendLine(string.Join(
                    " | ",
                    "| " + v.Label,
                    Cell(Expand(RawToken, v.Value, r.ProcessorQuoter)),
                    Cell(Expand(SafeToken, v.Value, r.ProcessorQuoter)),
                    Cell(QuoteValueOrThrow(r.GeneratorQuoter, v.Value)) + " |"));
            }

            sb.AppendLine();
            return sb.ToString();
        }

        /// <summary>
        /// Escapes a rendered SQL fragment for display inside a markdown table cell.
        /// </summary>
        private static string Cell(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return "*(empty)*";
            }

            return "`" + value.Replace("|", "\\|") + "`";
        }

        // ------------------------------------------------------------------ tests

        /// <summary>
        /// Golden matrix per dialect family. The approved snapshot is a readable markdown document,
        /// so a behavioural change is a one-line diff naming the dialect, the value and the token style.
        /// </summary>
        [TestCaseSource(nameof(Families))]
        public Task TokenMatrix_IsStable(string family)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"# Token substitution matrix - {family}");
            sb.AppendLine();
            sb.AppendLine("See `TokenSubstitutionMatrix.md` for what each column means.");
            sb.AppendLine();

            foreach (var dialect in DialectCases().Where(d => d.Family == family))
            {
                using var r = Resolve(dialect);
                sb.Append(RenderMatrix(dialect, r));
            }

            return Verifier.Verify(sb.ToString(), "md").UseParameters(family);
        }

        /// <summary>
        /// <c>RawSql</c> is an opt-in escape hatch: the caller has taken responsibility for the SQL,
        /// so it must reach the database byte-for-byte through every quoter.
        /// </summary>
        /// <remarks>
        /// This is the assertion that catches #2335 (<c>MySqlQuoter</c> double-escaping backslashes
        /// in the <c>RawSql</c> passthrough). Note that
        /// <see cref="SafeToken_Agrees_With_QuoteValue"/> cannot catch it: both sides of that
        /// comparison run through the same quoter and therefore agree on the corrupted value.
        /// </remarks>
        [TestCaseSource(nameof(DialectCases))]
        public void RawSql_RoundTripsVerbatim(DialectCase dialect)
        {
            using var r = Resolve(dialect);

            var rawSqlCases = new[]
            {
                "CURRENT_TIMESTAMP",
                @"c REGEXP '\d+'",
                "name = 'O''Brien'",
                @"path LIKE 'C:\Temp\%'",
                @"json_col->'$.a\b'",
            };

            Assert.Multiple(() =>
            {
                foreach (var sql in rawSqlCases)
                {
                    var value = RawSql.Insert(sql);

                    QuoteValueOrThrow(r.GeneratorQuoter, value).ShouldBe(
                        sql,
                        $"{dialect.Id}: QuoteValue mangled RawSql instead of passing it through verbatim.");

                    Expand(SafeToken, value, r.ProcessorQuoter).ShouldBe(
                        sql,
                        $"{dialect.Id}: $[x] mangled RawSql instead of passing it through verbatim.");

                    Expand(RawToken, value, r.ProcessorQuoter).ShouldBe(
                        sql,
                        $"{dialect.Id}: $(x) mangled RawSql instead of passing it through verbatim.");
                }
            });
        }

        /// <summary>
        /// <c>$[x]</c> and <c>Insert</c>/<c>Update</c>/<c>Delete</c> must render an identical literal
        /// for the same value, because both mean "this value as SQL data" and both terminate in
        /// <see cref="IQuoter.QuoteValue"/>.
        /// </summary>
        /// <remarks>
        /// Guards against the two paths drifting apart, e.g. if the token path were changed to use a
        /// different quoter or to pre-format values. It cannot catch a defect *inside* a shared
        /// quoter - see <see cref="RawSql_RoundTripsVerbatim"/> for that.
        /// </remarks>
        [TestCaseSource(nameof(DialectCases))]
        public void SafeToken_Agrees_With_QuoteValue(DialectCase dialect)
        {
            using var r = Resolve(dialect);

            Assert.Multiple(() =>
            {
                foreach (var v in ValueCases())
                {
                    Expand(SafeToken, v.Value, r.ProcessorQuoter).ShouldBe(
                        QuoteValueOrThrow(r.GeneratorQuoter, v.Value),
                        $"{dialect.Id} / {v.Label}: $[x] and Insert/Update/Delete disagree.");
                }
            });
        }

        /// <summary>
        /// Token substitution reads <c>processor.Quoter</c> while DML generation reads
        /// <c>generator.Quoter</c>. If a processor shadows <c>ProcessorBase.Quoter</c> with a
        /// separately registered instance, the two paths diverge silently.
        /// </summary>
        [TestCaseSource(nameof(DialectCases))]
        public void ProcessorQuoter_Subsumes_GeneratorQuoter(DialectCase dialect)
        {
            using var r = Resolve(dialect);

            r.ProcessorQuoter.GetType().ShouldBe(
                r.GeneratorQuoter.GetType(),
                $"{dialect.Id}: token substitution uses processor.Quoter but DML uses generator.Quoter; " +
                "these must agree or $[x] and Insert/Update/Delete will render the same value differently.");

            // Type equality is necessary but not sufficient: two same-typed quoters can carry
            // different switch state, e.g. SQLiteQuoter(binaryGuid: true) vs (false).
            Assert.Multiple(() =>
            {
                foreach (var v in ValueCases())
                {
                    QuoteValueOrThrow(r.ProcessorQuoter, v.Value).ShouldBe(
                        QuoteValueOrThrow(r.GeneratorQuoter, v.Value),
                        $"{dialect.Id} / {v.Label}: processor and generator quoters disagree.");
                }
            });
        }

        /// <summary>
        /// <c>ITypeMap</c> is orthogonal to token expansion by construction - the replacer never
        /// receives one. Pinned so a refactor cannot quietly couple them.
        /// </summary>
        [TestCaseSource(nameof(TypeMapVariantCases))]
        public void TypeMapSwitch_DoesNotAffect_TokenExpansion(DialectCase variant, DialectCase baseline)
        {
            using var a = Resolve(variant);
            using var b = Resolve(baseline);

            RenderBody(variant, a).ShouldBe(
                RenderBody(baseline, b),
                $"{variant.Id} differs from {baseline.Id} only by an ITypeMap switch, " +
                "which must not influence token expansion.");
        }

        /// <summary>
        /// The matrix rows without the dialect-specific heading, for comparing two configurations.
        /// </summary>
        private static string RenderBody(DialectCase dialect, Resolved r)
        {
            var full = RenderMatrix(dialect, r);
            var marker = "| value |";
            var index = full.IndexOf(marker, StringComparison.Ordinal);
            return index < 0 ? full : full.Substring(index);
        }

        private static IEnumerable<TestCaseData> TypeMapVariantCases()
        {
            var all = DialectCases().ToDictionary(d => d.Id);

            foreach (var d in all.Values.Where(d => d.TypeMapVariantOf != null))
            {
                if (all.TryGetValue(d.TypeMapVariantOf, out var baseline))
                {
                    yield return new TestCaseData(d, baseline).SetName($"TypeMapSwitch_{d.Id}_vs_{d.TypeMapVariantOf}");
                }
            }
        }

        /// <summary>
        /// Neither token style may depend on <see cref="CultureInfo.CurrentCulture"/>. #2324 fixed
        /// <c>$(x)</c> via <c>IFormattable</c> + <c>InvariantCulture</c>; the <c>$[x]</c> no-quoter
        /// fallback still calls plain <c>ToString()</c>.
        /// </summary>
        [TestCaseSource(nameof(DialectCases))]
        public void TokenMatrix_IsCultureInvariant(DialectCase dialect)
        {
            using var r = Resolve(dialect);

            var original = Thread.CurrentThread.CurrentCulture;
            try
            {
                Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");
                var enUs = RenderBody(dialect, r);

                Thread.CurrentThread.CurrentCulture = new CultureInfo("de-DE");
                var deDe = RenderBody(dialect, r);

                deDe.ShouldBe(enUs, $"{dialect.Id}: token expansion is culture-sensitive.");
            }
            finally
            {
                Thread.CurrentThread.CurrentCulture = original;
            }
        }

        /// <summary>
        /// Escape and miss handling is orthogonal to both the value and the dialect axes, so it gets
        /// one small fixture rather than a full cross-product.
        /// </summary>
        [TestCase("$$((x))", "$(x)")]
        [TestCase("$$[[x]]", "$[x]")]
        [TestCase("$(unknown)", "$(unknown)")]
        [TestCase("$[unknown]", "$[unknown]")]
        public void EscapeAndMissForms_AreDialectIndependent(string template, string expected)
        {
            var parameters = Params(RawSql.Insert("CURRENT_TIMESTAMP"));

            Assert.Multiple(() =>
            {
                foreach (var dialect in DialectCases())
                {
                    using var r = Resolve(dialect);

                    SqlScriptTokenReplacer.ReplaceSqlScriptTokens(template, parameters, r.ProcessorQuoter)
                        .ShouldBe(expected, $"{dialect.Id}: escape/miss handling should not vary by dialect.");
                }
            });
        }
    }
}
