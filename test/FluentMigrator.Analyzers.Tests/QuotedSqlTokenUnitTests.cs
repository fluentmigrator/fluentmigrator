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

using VerifyTest = Microsoft.CodeAnalysis.CSharp.Testing.CSharpCodeFixTest<
    FluentMigrator.Analyzers.QuotedSqlTokenAnalyzer,
    FluentMigrator.Analyzers.QuotedSqlTokenCodeFixProvider,
    Microsoft.CodeAnalysis.Testing.DefaultVerifier
>;

namespace FluentMigrator.Analyzers.Tests
{
    /// <summary>
    /// Coverage for FM0003, which reports a <c>$[name]</c> token sitting inside a SQL string literal.
    /// </summary>
    /// <remarks>
    /// The dialect axis is exercised through <c>IfDatabase("...")</c> rather than
    /// <c>.editorconfig</c>, because it scopes to a single statement and keeps each case readable.
    /// <see cref="Dialect_Comes_From_EditorConfig"/> covers the other signal.
    /// </remarks>
    public class QuotedSqlTokenUnitTests
    {
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

        public class FakeIfDatabase
        {
            public IExecuteExpressionRoot Execute { get { return null; } }
        }

        public class FakeMigration
        {
            public FakeIfDatabase IfDatabase(params string[] databaseType) { return null; }
        }
    }
";

        private static string Source(string body) => @"
    using ConsoleApplication1;

    namespace ConsoleApplication1
    {
        public class TypeName
        {
            public void Up()
            {
" + body + @"
            }
        }
    }";

        private static VerifyTest CreateTest(string source, string fixedSource = null)
        {
            var test = new VerifyTest
            {
                TestState =
                {
                    Sources = { source, StubDefinitions },
                },
            };

            if (fixedSource != null)
            {
                test.FixedState.Sources.Add(fixedSource);
                test.FixedState.Sources.Add(StubDefinitions);
            }

#if NET
            test.ReferenceAssemblies = ReferenceAssemblies.Net.Net80;
#endif

            return test;
        }

        // ---------------------------------------------------------------- dialect-independent

        [Test]
        public async Task Warns_When_Token_Is_Wrapped_In_Single_Quotes()
        {
            var source = Source(@"                new FakeExecuteExpressionRoot().Sql(""SELECT * FROM Foo WHERE Name = '{|FM0003:$[name]|}'"");");
            var fixedSource = Source(@"                new FakeExecuteExpressionRoot().Sql(""SELECT * FROM Foo WHERE Name = '$(name)'"");");

            await CreateTest(source, fixedSource).RunAsync();
        }

        [Test]
        public async Task Warns_When_Token_Is_Embedded_In_A_Larger_Literal()
        {
            var source = Source(@"                new FakeExecuteExpressionRoot().Sql(""SELECT * FROM Foo WHERE Name LIKE '%{|FM0003:$[name]|}%'"");");
            var fixedSource = Source(@"                new FakeExecuteExpressionRoot().Sql(""SELECT * FROM Foo WHERE Name LIKE '%$(name)%'"");");

            await CreateTest(source, fixedSource).RunAsync();
        }

        [TestCase("N")]
        [TestCase("E")]
        [TestCase("B")]
        [TestCase("X")]
        [TestCase("_utf8mb4")]
        public async Task Warns_For_Prefixed_String_Literals(string prefix)
        {
            var source = Source(@"                new FakeExecuteExpressionRoot().Sql(""SELECT " + prefix + @"'{|FM0003:$[name]|}'"");");
            var fixedSource = Source(@"                new FakeExecuteExpressionRoot().Sql(""SELECT " + prefix + @"'$(name)'"");");

            await CreateTest(source, fixedSource).RunAsync();
        }

        [Test]
        public async Task Warns_For_Unicode_Escape_Literals()
        {
            var source = Source(@"                new FakeExecuteExpressionRoot().Sql(""SELECT U&'{|FM0003:$[name]|}'"");");
            var fixedSource = Source(@"                new FakeExecuteExpressionRoot().Sql(""SELECT U&'$(name)'"");");

            await CreateTest(source, fixedSource).RunAsync();
        }

        [Test]
        public async Task Doesnt_Warn_When_Token_Is_Not_In_A_Literal()
        {
            var source = Source(@"                new FakeExecuteExpressionRoot().Sql(""SELECT * FROM Foo WHERE Name = $[name]"");");

            await CreateTest(source).RunAsync();
        }

        [Test]
        public async Task Doesnt_Warn_For_Raw_Token_In_A_Literal()
        {
            // '$(name)' is the correct way to substitute a value into a string literal.
            var source = Source(@"                new FakeExecuteExpressionRoot().Sql(""SELECT * FROM Foo WHERE Name = '$(name)'"");");

            await CreateTest(source).RunAsync();
        }

        [Test]
        public async Task Doesnt_Warn_Inside_A_Line_Comment()
        {
            var source = Source(@"                new FakeExecuteExpressionRoot().Sql(""SELECT 1 -- '$[name]'"");");

            await CreateTest(source).RunAsync();
        }

        [Test]
        public async Task Doesnt_Warn_Inside_A_Block_Comment()
        {
            var source = Source(@"                new FakeExecuteExpressionRoot().Sql(""SELECT /* '$[name]' */ 1"");");

            await CreateTest(source).RunAsync();
        }

        [Test]
        public async Task Doesnt_Warn_When_A_Preceding_Literal_Is_Already_Closed()
        {
            // The token follows a complete literal, so it is in code, not inside it.
            var source = Source(@"                new FakeExecuteExpressionRoot().Sql(""SELECT 'a', $[name]"");");

            await CreateTest(source).RunAsync();
        }

        [Test]
        public async Task Doesnt_Warn_When_Doubled_Quotes_Close_The_Literal()
        {
            // 'it''s' is a complete literal; the token after it is in code.
            var source = Source(@"                new FakeExecuteExpressionRoot().Sql(""SELECT 'it''''s', $[name]"");");

            await CreateTest(source).RunAsync();
        }

        // ---------------------------------------------------------------- dialect-gated

        [Test]
        public async Task Warns_For_Double_Quoted_Literal_On_MySql()
        {
            // Without ANSI_QUOTES a double-quoted run is a string literal in MySQL.
            var source = Source(@"                new FakeMigration().IfDatabase(""MySql"").Execute.Sql(""SELECT \""{|FM0003:$[name]|}\"""");");
            var fixedSource = Source(@"                new FakeMigration().IfDatabase(""MySql"").Execute.Sql(""SELECT \""$(name)\"""");");

            await CreateTest(source, fixedSource).RunAsync();
        }

        [Test]
        public async Task Doesnt_Warn_For_Double_Quoted_Identifier_On_SqlServer()
        {
            var source = Source(@"                new FakeMigration().IfDatabase(""SqlServer2016"").Execute.Sql(""SELECT \""$[name]\"""");");

            await CreateTest(source).RunAsync();
        }

        [Test]
        public async Task Doesnt_Warn_For_Double_Quoted_Run_When_Dialect_Is_Unknown()
        {
            var source = Source(@"                new FakeExecuteExpressionRoot().Sql(""SELECT \""$[name]\"""");");

            await CreateTest(source).RunAsync();
        }

        [Test]
        public async Task Doesnt_Warn_For_Double_Quoted_Run_On_SQLite()
        {
            // SQLite resolves "x" to an identifier or a string depending on the schema, so the
            // outcome is not knowable at compile time.
            var source = Source(@"                new FakeMigration().IfDatabase(""SQLite"").Execute.Sql(""SELECT \""$[name]\"""");");

            await CreateTest(source).RunAsync();
        }

        [Test]
        public async Task Doesnt_Warn_Inside_Postgres_Dollar_Quoting()
        {
            // A dollar-quoted block holds SQL source, so a quoted literal inside it is expected.
            var source = Source(@"                new FakeMigration().IfDatabase(""Postgres"").Execute.Sql(""CREATE FUNCTION f() AS $body$ SELECT $[name]; $body$"");");

            await CreateTest(source).RunAsync();
        }

        [Test]
        public async Task Warns_Inside_A_Literal_Nested_In_Postgres_Dollar_Quoting()
        {
            var source = Source(@"                new FakeMigration().IfDatabase(""Postgres"").Execute.Sql(""CREATE FUNCTION f() AS $body$ SELECT '{|FM0003:$[name]|}'; $body$"");");
            var fixedSource = Source(@"                new FakeMigration().IfDatabase(""Postgres"").Execute.Sql(""CREATE FUNCTION f() AS $body$ SELECT '$(name)'; $body$"");");

            await CreateTest(source, fixedSource).RunAsync();
        }

        [Test]
        public async Task Warns_For_Oracle_Alternative_Quoting()
        {
            var source = Source(@"                new FakeMigration().IfDatabase(""Oracle"").Execute.Sql(""SELECT q'[{|FM0003:$[name]|}]' FROM dual"");");
            var fixedSource = Source(@"                new FakeMigration().IfDatabase(""Oracle"").Execute.Sql(""SELECT q'[$(name)]' FROM dual"");");

            await CreateTest(source, fixedSource).RunAsync();
        }

        [Test]
        public async Task Backslash_Does_Not_Close_A_Literal_On_MySql()
        {
            // In MySQL's default mode '\'' is a complete literal, so the token is still inside it.
            var source = Source(@"                new FakeMigration().IfDatabase(""MySql"").Execute.Sql(""SELECT 'a\\'{|FM0003:$[name]|}'"");");
            var fixedSource = Source(@"                new FakeMigration().IfDatabase(""MySql"").Execute.Sql(""SELECT 'a\\'$(name)'"");");

            await CreateTest(source, fixedSource).RunAsync();
        }

        [Test]
        public async Task Backslash_Closes_A_Literal_On_Postgres()
        {
            // With standard_conforming_strings the backslash is literal, so 'a\' is complete and
            // the token that follows is in code.
            var source = Source(@"                new FakeMigration().IfDatabase(""Postgres"").Execute.Sql(""SELECT 'a\\', $[name]"");");

            await CreateTest(source).RunAsync();
        }

        [Test]
        public async Task Doesnt_Resolve_Dialect_When_IfDatabase_Names_Several_Families()
        {
            // Guarded for two dialects at once: nothing dialect-specific is safe, so the
            // double-quote case stays silent.
            var source = Source(@"                new FakeMigration().IfDatabase(""MySql"", ""Postgres"").Execute.Sql(""SELECT \""$[name]\"""");");

            await CreateTest(source).RunAsync();
        }

        [Test]
        public async Task Resolves_Dialect_When_IfDatabase_Names_One_Family()
        {
            var source = Source(@"                new FakeMigration().IfDatabase(""MySql5"", ""MySql8"").Execute.Sql(""SELECT \""{|FM0003:$[name]|}\"""");");
            var fixedSource = Source(@"                new FakeMigration().IfDatabase(""MySql5"", ""MySql8"").Execute.Sql(""SELECT \""$(name)\"""");");

            await CreateTest(source, fixedSource).RunAsync();
        }

        [Test]
        public async Task Dialect_Comes_From_EditorConfig()
        {
            var source = Source(@"                new FakeExecuteExpressionRoot().Sql(""SELECT \""{|FM0003:$[name]|}\"""");");
            var fixedSource = Source(@"                new FakeExecuteExpressionRoot().Sql(""SELECT \""$(name)\"""");");

            var test = CreateTest(source, fixedSource);
            test.TestState.AnalyzerConfigFiles.Add(
                ("/.editorconfig", "root = true\n\n[*.cs]\nfluent_migrator_sql_dialect = mysql\n"));
            test.FixedState.AnalyzerConfigFiles.Add(
                ("/.editorconfig", "root = true\n\n[*.cs]\nfluent_migrator_sql_dialect = mysql\n"));

            await test.RunAsync();
        }
    }
}
