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

using System.Threading.Tasks;

using Microsoft.CodeAnalysis.Testing;

using NUnit.Framework;

using Verify = Microsoft.CodeAnalysis.CSharp.Testing.CSharpCodeFixVerifier<
    FluentMigrator.Analyzers.RawSqlTokenInterpolationAnalyzer,
    FluentMigrator.Analyzers.RawSqlTokenInterpolationCodeFixProvider,
    Microsoft.CodeAnalysis.Testing.DefaultVerifier
>;
using VerifyTest = Microsoft.CodeAnalysis.CSharp.Testing.CSharpCodeFixTest<
    FluentMigrator.Analyzers.RawSqlTokenInterpolationAnalyzer,
    FluentMigrator.Analyzers.RawSqlTokenInterpolationCodeFixProvider,
    Microsoft.CodeAnalysis.Testing.DefaultVerifier
>;

namespace FluentMigrator.Analyzers.Tests
{
    public class RawSqlTokenInterpolationUnitTests
    {
        // Note: the test sources below intentionally contain the literal "$(" / "$$((" sequences under
        // test. Since "$$" is reserved by the Roslyn testing framework's markup syntax to denote a caret
        // position, markup handling is disabled for every source in this fixture to prevent it from being
        // misinterpreted/stripped.
        private const string StubDefinitions = @"
    namespace FluentMigrator.Builders.Execute
    {
        public interface IExecuteExpressionRoot
        {
            void Sql(string sqlStatement);
        }
    }

    namespace ConsoleApplication1
    {
        using FluentMigrator.Builders.Execute;

        public class FakeExecuteExpressionRoot : IExecuteExpressionRoot
        {
            public void Sql(string sqlStatement)
            {
            }
        }
    }
";

        private static string Source(string sql) => @"
    using ConsoleApplication1;

    namespace ConsoleApplication1
    {
        public class TypeName
        {
            public void Up()
            {
                new FakeExecuteExpressionRoot().Sql(""" + sql + @""");
            }
        }
    }";

        private static async Task VerifyCodeFixAsync(string sql, string fixedSql)
        {
            var source = Source(sql);
            var fixedSource = Source(fixedSql);
            var sourceLine = "                new FakeExecuteExpressionRoot().Sql(\"" + sql + "\");";
            var tokenColumn = sourceLine.IndexOf("$(name)") + 1;
            var expected = Verify.Diagnostic(RawSqlTokenInterpolationAnalyzer.DiagnosticId)
                .WithSpan(10, tokenColumn, 10, tokenColumn + "$(name)".Length)
                .WithArguments("$(name)", "$[name]");

            var ut = new VerifyTest
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

            ut.TestState.ExpectedDiagnostics.Add(expected);
#if NET
            ut.ReferenceAssemblies = ReferenceAssemblies.Net.Net80;
#endif

            await ut.RunAsync();
        }

        [Test]
        public async Task Doesnt_Warn_On_Quoted_Token()
        {
            //language=csharp
            const string source = @"
    using ConsoleApplication1;

    namespace ConsoleApplication1
    {
        public class TypeName
        {
            public void Up()
            {
                new FakeExecuteExpressionRoot().Sql(""SELECT * FROM Foo WHERE Name = $[name]"");
            }
        }
    }";

            var ut = new VerifyTest
            {
                TestState =
                {
                    Sources = { source, StubDefinitions },
                    MarkupHandling = MarkupMode.None,
                    ExpectedDiagnostics =
                    {
                        Capacity = 0
                    },
                },
            };

#if NET
            ut.ReferenceAssemblies = ReferenceAssemblies.Net.Net80;
#endif

            await ut.RunAsync();
        }

        [Test]
        public async Task Warns_On_Raw_Token()
        {
            //language=csharp
            const string source = @"
    using ConsoleApplication1;

    namespace ConsoleApplication1
    {
        public class TypeName
        {
            public void Up()
            {
                new FakeExecuteExpressionRoot().Sql(""SELECT * FROM Foo WHERE Name = $(name)"");
            }
        }
    }";

            //language=csharp
            const string fixedSource = @"
    using ConsoleApplication1;

    namespace ConsoleApplication1
    {
        public class TypeName
        {
            public void Up()
            {
                new FakeExecuteExpressionRoot().Sql(""SELECT * FROM Foo WHERE Name = $[name]"");
            }
        }
    }";

            var expected = Verify.Diagnostic(RawSqlTokenInterpolationAnalyzer.DiagnosticId)
                .WithSpan(10, 85, 10, 92)
                .WithArguments("$(name)", "$[name]");

            var ut = new VerifyTest
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

            ut.TestState.ExpectedDiagnostics.Add(expected);
#if NET
            ut.ReferenceAssemblies = ReferenceAssemblies.Net.Net80;
#endif

            await ut.RunAsync();
        }

        /// <summary>
        /// The escaped form performs no substitution - it renders as the literal text
        /// <c>$(name)</c> - so it carries no injection risk. Reporting it also invited a code fix
        /// that changed the emitted SQL from <c>$(name)</c> to <c>$[name]</c>, corrupting output for
        /// anyone deliberately emitting token syntax.
        /// </summary>
        [Test]
        public async Task Doesnt_Warn_On_Escaped_Raw_Token()
        {
            //language=csharp
            const string source = @"
    using ConsoleApplication1;

    namespace ConsoleApplication1
    {
        public class TypeName
        {
            public void Up()
            {
                new FakeExecuteExpressionRoot().Sql(""SELECT '$$((name))' AS Literal"");
            }
        }
    }";

            var ut = new VerifyTest
            {
                TestState =
                {
                    Sources = { source, StubDefinitions },
                    MarkupHandling = MarkupMode.None,
                    ExpectedDiagnostics =
                    {
                        Capacity = 0
                    },
                },
            };

#if NET
            ut.ReferenceAssemblies = ReferenceAssemblies.Net.Net80;
#endif

            await ut.RunAsync();
        }

        /// <summary>
        /// The code fix has to remove the surrounding quotes: <c>$[name]</c> already expands to a
        /// complete, quoted literal, so rewriting the token alone produced <c>'$[name]'</c> and
        /// quoted the value twice.
        /// </summary>
        [Test]
        public async Task Fix_Removes_Enclosing_Quotes_When_Token_Is_The_Whole_Literal()
        {
            //language=csharp
            const string source = @"
    using ConsoleApplication1;

    namespace ConsoleApplication1
    {
        public class TypeName
        {
            public void Up()
            {
                new FakeExecuteExpressionRoot().Sql(""UPDATE Foo SET Name = '$(name)'"");
            }
        }
    }";

            //language=csharp
            const string fixedSource = @"
    using ConsoleApplication1;

    namespace ConsoleApplication1
    {
        public class TypeName
        {
            public void Up()
            {
                new FakeExecuteExpressionRoot().Sql(""UPDATE Foo SET Name = $[name]"");
            }
        }
    }";

            var expected = Verify.Diagnostic(RawSqlTokenInterpolationAnalyzer.DiagnosticId)
                .WithSpan(10, 77, 10, 84)
                .WithArguments("$(name)", "$[name]");

            var ut = new VerifyTest
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

            ut.TestState.ExpectedDiagnostics.Add(expected);
#if NET
            ut.ReferenceAssemblies = ReferenceAssemblies.Net.Net80;
#endif

            await ut.RunAsync();
        }

        /// <summary>
        /// SQL keywords and identifiers may be adjacent to a string literal. The code fix must
        /// remove only the literal delimiter, without consuming the preceding SQL text as a prefix.
        /// </summary>
        [Test]
        public async Task Fix_Preserves_Sql_Text_Adjacent_To_A_Literal()
        {
            await VerifyCodeFixAsync(
                "SELECT * FROM Foo WHERE Name LIKE'$(name)'",
                "SELECT * FROM Foo WHERE Name LIKE$[name]");
        }

        [TestCase("N")]
        [TestCase("n")]
        [TestCase("E")]
        [TestCase("e")]
        [TestCase("B")]
        [TestCase("b")]
        [TestCase("X")]
        [TestCase("x")]
        [TestCase("_utf8mb4")]
        [TestCase("_custom_charset")]
        [TestCase("U&")]
        [TestCase("u&")]
        public async Task Fix_Removes_Known_Literal_Prefix(string prefix)
        {
            await VerifyCodeFixAsync("SELECT " + prefix + "'$(name)'", "SELECT $[name]");
        }

        [TestCase("1&N", "1&")]
        [TestCase("1&U&", "1&")]
        public async Task Fix_Removes_Literal_Prefix_After_NonIdentifier_Boundary(
            string sqlBeforeQuote,
            string fixedSqlBeforeToken)
        {
            await VerifyCodeFixAsync(
                "SELECT " + sqlBeforeQuote + "'$(name)'",
                "SELECT " + fixedSqlBeforeToken + "$[name]");
        }

        [TestCase("columnN")]
        [TestCase("columnU&")]
        [TestCase("column_utf8mb4")]
        public async Task Fix_Preserves_Identifier_Text_Adjacent_To_A_Literal(string adjacentText)
        {
            await VerifyCodeFixAsync(
                "SELECT " + adjacentText + "'$(name)'",
                "SELECT " + adjacentText + "$[name]");
        }

        /// <summary>
        /// When the token is only part of a literal there is no mechanical rewrite, so the
        /// diagnostic is still reported but no code fix is offered.
        /// </summary>
        [Test]
        public async Task Offers_No_Fix_When_Token_Is_Embedded_In_A_Literal()
        {
            //language=csharp
            const string source = @"
    using ConsoleApplication1;

    namespace ConsoleApplication1
    {
        public class TypeName
        {
            public void Up()
            {
                new FakeExecuteExpressionRoot().Sql(""SELECT * FROM Foo WHERE Name LIKE '%$(name)%'"");
            }
        }
    }";

            var expected = Verify.Diagnostic(RawSqlTokenInterpolationAnalyzer.DiagnosticId)
                .WithSpan(10, 90, 10, 97)
                .WithArguments("$(name)", "$[name]");

            var ut = new VerifyTest
            {
                TestState =
                {
                    Sources = { source, StubDefinitions },
                    MarkupHandling = MarkupMode.None,
                },
                FixedState =
                {
                    // Unchanged: the analyzer reports, but the fix provider declines to rewrite.
                    Sources = { source, StubDefinitions },
                    MarkupHandling = MarkupMode.None,
                },
            };

            ut.TestState.ExpectedDiagnostics.Add(expected);
            ut.FixedState.ExpectedDiagnostics.Add(expected);
#if NET
            ut.ReferenceAssemblies = ReferenceAssemblies.Net.Net80;
#endif

            await ut.RunAsync();
        }

        /// <summary>
        /// A token in a comment is never executed, so there is nothing to warn about.
        /// </summary>
        [Test]
        public async Task Doesnt_Warn_Inside_A_Comment()
        {
            //language=csharp
            const string source = @"
    using ConsoleApplication1;

    namespace ConsoleApplication1
    {
        public class TypeName
        {
            public void Up()
            {
                new FakeExecuteExpressionRoot().Sql(""SELECT 1 -- $(name)"");
            }
        }
    }";

            var ut = new VerifyTest
            {
                TestState =
                {
                    Sources = { source, StubDefinitions },
                    MarkupHandling = MarkupMode.None,
                    ExpectedDiagnostics =
                    {
                        Capacity = 0
                    },
                },
            };

#if NET
            ut.ReferenceAssemblies = ReferenceAssemblies.Net.Net80;
#endif

            await ut.RunAsync();
        }

        [Test]
        public async Task Doesnt_Warn_On_Non_Execute_Sql_Call()
        {
            //language=csharp
            const string source = @"
    namespace ConsoleApplication1
    {
        public class TypeName
        {
            public void Sql(string sqlStatement)
            {
            }

            public void Up()
            {
                Sql(""SELECT * FROM Foo WHERE Name = $(name)"");
            }
        }
    }";

            var ut = new VerifyTest
            {
                TestState =
                {
                    Sources = { source, StubDefinitions },
                    MarkupHandling = MarkupMode.None,
                    ExpectedDiagnostics =
                    {
                        Capacity = 0
                    },
                },
            };

#if NET
            ut.ReferenceAssemblies = ReferenceAssemblies.Net.Net80;
#endif

            await ut.RunAsync();
        }
    }
}
