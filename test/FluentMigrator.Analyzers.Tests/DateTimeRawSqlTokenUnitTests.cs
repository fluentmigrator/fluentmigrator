#region License
// Copyright (c) 2024, Fluent Migrator Project
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
using System.Threading.Tasks;

using Microsoft.CodeAnalysis.Testing;

using NUnit.Framework;

using Verify = Microsoft.CodeAnalysis.CSharp.Testing.CSharpCodeFixVerifier<
    FluentMigrator.Analyzers.DateTimeRawSqlTokenAnalyzer,
    FluentMigrator.Analyzers.DateTimeRawSqlTokenCodeFixProvider,
    Microsoft.CodeAnalysis.Testing.DefaultVerifier
>;
using VerifyTest = Microsoft.CodeAnalysis.CSharp.Testing.CSharpCodeFixTest<
    FluentMigrator.Analyzers.DateTimeRawSqlTokenAnalyzer,
    FluentMigrator.Analyzers.DateTimeRawSqlTokenCodeFixProvider,
    Microsoft.CodeAnalysis.Testing.DefaultVerifier
>;

namespace FluentMigrator.Analyzers.Tests
{
    /// <summary>
    /// FM0007: a <c>DateTime</c>/<c>DateTimeOffset</c> value substituted through a raw <c>$(name)</c> token (#2354).
    /// </summary>
    public class DateTimeRawSqlTokenUnitTests
    {
        private const string StubDefinitions = @"
    namespace FluentMigrator.Builders.Execute
    {
        using System.Collections.Generic;

        public interface IExecuteExpressionRoot
        {
            void Sql(string sqlStatement);
            void Sql(string sqlStatement, IDictionary<string, object> parameters);
        }
    }

    namespace ConsoleApplication1
    {
        using System.Collections.Generic;
        using FluentMigrator.Builders.Execute;

        public class FakeExecuteExpressionRoot : IExecuteExpressionRoot
        {
            public void Sql(string sqlStatement)
            {
            }

            public void Sql(string sqlStatement, IDictionary<string, object> parameters)
            {
            }
        }
    }
";

        [Test]
        public async Task Fix_Replaces_Quoted_Raw_Token_With_Quoted_Token()
        {
            // The example from #2354.
            var source = Migration(@"
                new FakeExecuteExpressionRoot().Sql(""DELETE FROM Audit WHERE CreatedAt < '$(cutoff)'"", new Dictionary<string, object>
                {
                    [""cutoff""] = new DateTime(2026, 7, 28, 13, 45, 6)
                });");
            var fixedSource = Migration(@"
                new FakeExecuteExpressionRoot().Sql(""DELETE FROM Audit WHERE CreatedAt < $[cutoff]"", new Dictionary<string, object>
                {
                    [""cutoff""] = new DateTime(2026, 7, 28, 13, 45, 6)
                });");

            await RunAsync(source, fixedSource, Expect(source, "$(cutoff)", "DateTime"));
        }

        [Test]
        public async Task Warns_On_DateTimeOffset_In_Collection_Initializer()
        {
            var source = Migration(@"
                new FakeExecuteExpressionRoot().Sql(""SELECT * FROM Audit WHERE CreatedAt < $(cutoff)"", new Dictionary<string, object>
                {
                    { ""cutoff"", DateTimeOffset.UtcNow },
                });");
            var fixedSource = Migration(@"
                new FakeExecuteExpressionRoot().Sql(""SELECT * FROM Audit WHERE CreatedAt < $[cutoff]"", new Dictionary<string, object>
                {
                    { ""cutoff"", DateTimeOffset.UtcNow },
                });");

            await RunAsync(source, fixedSource, Expect(source, "$(cutoff)", "DateTimeOffset"));
        }

        [Test]
        public async Task Warns_On_Nullable_DateTime_From_A_Local_Dictionary()
        {
            var source = Migration(@"
                DateTime? cutoff = DateTime.UtcNow;
                var parameters = new Dictionary<string, object> { [""cutoff""] = cutoff };
                new FakeExecuteExpressionRoot().Sql(""DELETE FROM Audit WHERE CreatedAt < '$(cutoff)'"", parameters);");
            var fixedSource = Migration(@"
                DateTime? cutoff = DateTime.UtcNow;
                var parameters = new Dictionary<string, object> { [""cutoff""] = cutoff };
                new FakeExecuteExpressionRoot().Sql(""DELETE FROM Audit WHERE CreatedAt < $[cutoff]"", parameters);");

            await RunAsync(source, fixedSource, Expect(source, "$(cutoff)", "DateTime"));
        }

        [Test]
        public async Task Reports_Only_The_Date_Token_Among_Several()
        {
            var source = Migration(@"
                new FakeExecuteExpressionRoot().Sql(""DELETE FROM $(table) WHERE CreatedAt < $(cutoff) AND Id > $(id)"", new Dictionary<string, object>
                {
                    [""table""] = ""Audit"",
                    [""cutoff""] = DateTime.UtcNow,
                    [""id""] = 42,
                });");
            var fixedSource = Migration(@"
                new FakeExecuteExpressionRoot().Sql(""DELETE FROM $(table) WHERE CreatedAt < $[cutoff] AND Id > $(id)"", new Dictionary<string, object>
                {
                    [""table""] = ""Audit"",
                    [""cutoff""] = DateTime.UtcNow,
                    [""id""] = 42,
                });");

            await RunAsync(source, fixedSource, Expect(source, "$(cutoff)", "DateTime"));
        }

        [Test]
        public async Task Offers_No_Fix_When_Token_Is_Embedded_In_A_Literal()
        {
            var source = Migration(@"
                new FakeExecuteExpressionRoot().Sql(""DELETE FROM Audit WHERE CreatedAt < '$(day) 00:00'"", new Dictionary<string, object>
                {
                    [""day""] = DateTime.Today,
                });");

            await RunAsync(source, source, Expect(source, "$(day)", "DateTime"));
        }

        [Test]
        public async Task Honours_A_Case_Insensitive_Dictionary()
        {
            var source = Migration(@"
                new FakeExecuteExpressionRoot().Sql(""DELETE FROM Audit WHERE CreatedAt < $(Cutoff)"", new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase)
                {
                    [""cutoff""] = DateTime.UtcNow,
                });");
            var fixedSource = Migration(@"
                new FakeExecuteExpressionRoot().Sql(""DELETE FROM Audit WHERE CreatedAt < $[Cutoff]"", new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase)
                {
                    [""cutoff""] = DateTime.UtcNow,
                });");

            await RunAsync(source, fixedSource, Expect(source, "$(Cutoff)", "DateTime"));
        }

        [Test]
        public async Task Doesnt_Warn_When_Key_Case_Differs_In_An_Ordinal_Dictionary()
        {
            // The token is not substituted at all at runtime, which is a different problem.
            var source = Migration(@"
                new FakeExecuteExpressionRoot().Sql(""DELETE FROM Audit WHERE CreatedAt < $(Cutoff)"", new Dictionary<string, object>
                {
                    [""cutoff""] = DateTime.UtcNow,
                });");

            await RunAsync(source, source);
        }

        [Test]
        public async Task Doesnt_Warn_On_Non_Date_Values()
        {
            var source = Migration(@"
                new FakeExecuteExpressionRoot().Sql(""SELECT $(a), $(b), $(c)"", new Dictionary<string, object>
                {
                    [""a""] = 1,
                    [""b""] = ""text"",
                    [""c""] = DateTime.UtcNow.ToString(""yyyy-MM-dd""),
                });");

            await RunAsync(source, source);
        }

        [Test]
        public async Task Doesnt_Warn_On_Quoted_Token()
        {
            var source = Migration(@"
                new FakeExecuteExpressionRoot().Sql(""DELETE FROM Audit WHERE CreatedAt < $[cutoff]"", new Dictionary<string, object>
                {
                    [""cutoff""] = DateTime.UtcNow,
                });");

            await RunAsync(source, source);
        }

        [Test]
        public async Task Doesnt_Warn_Inside_A_Comment()
        {
            var source = Migration(@"
                new FakeExecuteExpressionRoot().Sql(""-- was: CreatedAt < $(cutoff)\nDELETE FROM Audit"", new Dictionary<string, object>
                {
                    [""cutoff""] = DateTime.UtcNow,
                });");

            await RunAsync(source, source);
        }

        [Test]
        public async Task Doesnt_Warn_When_The_Parameters_Come_From_Elsewhere()
        {
            // Nothing can be correlated: the dictionary's contents are unknown here.
            var source = Migration(@"
                Run(new Dictionary<string, object> { [""cutoff""] = DateTime.UtcNow });
            }

            private void Run(IDictionary<string, object> parameters)
            {
                new FakeExecuteExpressionRoot().Sql(""DELETE FROM Audit WHERE CreatedAt < '$(cutoff)'"", parameters);");

            await RunAsync(source, source);
        }

        private static string Migration(string body)
        {
            return @"
    using System;
    using System.Collections.Generic;
    using ConsoleApplication1;

    namespace ConsoleApplication1
    {
        public class TypeName
        {
            public void Up()
            {" + body + @"
            }
        }
    }";
        }

        /// <summary>
        /// Builds the expected diagnostic for the first occurrence of <paramref name="token"/> inside a
        /// <c>Sql(</c> call in <paramref name="source"/>.
        /// </summary>
        private static DiagnosticResult Expect(string source, string token, string typeName)
        {
            var index = source.IndexOf(token, source.IndexOf(".Sql(", StringComparison.Ordinal), StringComparison.Ordinal);
            var lineStart = source.LastIndexOf('\n', index) + 1;
            var line = 1;
            for (var i = 0; i < index; i++)
            {
                if (source[i] == '\n')
                {
                    line++;
                }
            }

            var column = index - lineStart + 1;
            var quoted = token.Replace('(', '[').Replace(')', ']');
            return Verify.Diagnostic(DateTimeRawSqlTokenAnalyzer.DiagnosticId)
                .WithSpan(line, column, line, column + token.Length)
                .WithArguments(token, typeName, quoted);
        }

        private static async Task RunAsync(string source, string fixedSource, params DiagnosticResult[] expected)
        {
            var test = new VerifyTest
            {
                TestState =
                {
                    Sources = { source, StubDefinitions },
                    MarkupHandling = MarkupMode.None,
                },
                FixedState =
                {
                    Sources = { fixedSource, StubDefinitions },
                    MarkupHandling = MarkupMode.None,
                },
            };

            test.TestState.ExpectedDiagnostics.AddRange(expected);
            if (source == fixedSource)
            {
                // No fix applies, so the diagnostics remain.
                test.FixedState.ExpectedDiagnostics.AddRange(expected);
            }

#if NET
            test.ReferenceAssemblies = ReferenceAssemblies.Net.Net80;
#endif

            await test.RunAsync();
        }
    }
}
