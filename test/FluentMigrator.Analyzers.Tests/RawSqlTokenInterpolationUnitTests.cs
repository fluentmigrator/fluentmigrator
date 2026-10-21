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

        /// <summary>
        /// An adjacent identifier that is not a literal prefix belongs to the surrounding SQL, so only
        /// the quotes are removed and the keyword survives.
        /// </summary>
        [Test]
        public async Task Fix_Keeps_Adjacent_Keyword_When_It_Touches_The_Literal()
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
            new FakeExecuteExpressionRoot().Sql(""SELECT * FROM Foo WHERE Name LIKE'$(name)'"");
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
            new FakeExecuteExpressionRoot().Sql(""SELECT * FROM Foo WHERE Name LIKE$[name]"");
        }
    }
}";

            var expected = Verify.Diagnostic(RawSqlTokenInterpolationAnalyzer.DiagnosticId)
                .WithSpan(10, 84, 10, 91)
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
        /// A recognised prefix is part of the literal and goes with the quotes.
        /// </summary>
        [Test]
        public async Task Fix_Strips_National_Character_Prefix()
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
            new FakeExecuteExpressionRoot().Sql(""UPDATE Foo SET Name = N'$(name)'"");
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
                .WithSpan(10, 74, 10, 81)
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
        /// Prefix recognition is case-insensitive.
        /// </summary>
        [Test]
        public async Task Fix_Strips_Lowercase_National_Character_Prefix()
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
            new FakeExecuteExpressionRoot().Sql(""UPDATE Foo SET Name = n'$(name)'"");
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
                .WithSpan(10, 74, 10, 81)
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
        /// MySQL charset introducers are literal prefixes.
        /// </summary>
        [Test]
        public async Task Fix_Strips_Charset_Introducer_Prefix()
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
            new FakeExecuteExpressionRoot().Sql(""UPDATE Foo SET Name = _utf8mb4'$(name)'"");
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
                .WithSpan(10, 81, 10, 88)
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
        /// The Unicode-escape prefix ends in an ampersand, which is not an identifier character, so it
        /// is matched separately.
        /// </summary>
        [Test]
        public async Task Fix_Strips_Unicode_Escape_Prefix()
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
            new FakeExecuteExpressionRoot().Sql(""UPDATE Foo SET Name = U&'$(name)'"");
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
                .WithSpan(10, 75, 10, 82)
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
        /// An ampersand after an identifier is an operator, not the tail of a Unicode-escape prefix,
        /// so the identifier and the ampersand stay in place.
        /// </summary>
        [Test]
        public async Task Fix_Keeps_Identifier_Followed_By_Ampersand()
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
            new FakeExecuteExpressionRoot().Sql(""UPDATE Foo SET Name = FOO_U&'$(name)'"");
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
            new FakeExecuteExpressionRoot().Sql(""UPDATE Foo SET Name = FOO_U&$[name]"");
        }
    }
}";

            var expected = Verify.Diagnostic(RawSqlTokenInterpolationAnalyzer.DiagnosticId)
                .WithSpan(10, 79, 10, 86)
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
        /// The whole adjacent run is tested, so an identifier merely ending in a prefix letter is not
        /// mistaken for one.
        /// </summary>
        [Test]
        public async Task Fix_Keeps_Identifier_That_Ends_In_A_Prefix_Letter()
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
            new FakeExecuteExpressionRoot().Sql(""UPDATE Foo SET Name = ColumnN'$(name)'"");
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
            new FakeExecuteExpressionRoot().Sql(""UPDATE Foo SET Name = ColumnN$[name]"");
        }
    }
}";

            var expected = Verify.Diagnostic(RawSqlTokenInterpolationAnalyzer.DiagnosticId)
                .WithSpan(10, 80, 10, 87)
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
        /// The PostgreSQL escape-string prefix changes how backslashes in the value are read, so it
        /// must go with the quotes: E$[name] would re-escape the expanded value.
        /// </summary>
        [Test]
        public async Task Fix_Strips_Escape_String_Prefix()
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
            new FakeExecuteExpressionRoot().Sql(""UPDATE Foo SET Name = E'$(name)'"");
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
                .WithSpan(10, 74, 10, 81)
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
        /// A Unicode-escape literal at the very start of the SQL text has nothing before its prefix,
        /// which exercises the boundary check's start-of-input branch.
        /// </summary>
        [Test]
        public async Task Fix_Strips_Unicode_Escape_Prefix_At_Start_Of_Sql()
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
            new FakeExecuteExpressionRoot().Sql(""U&'$(name)'"");
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
            new FakeExecuteExpressionRoot().Sql(""$[name]"");
        }
    }
}";

            var expected = Verify.Diagnostic(RawSqlTokenInterpolationAnalyzer.DiagnosticId)
                .WithSpan(10, 53, 10, 60)
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
        /// A charset introducer is only a prefix when it starts the run: an identifier that merely
        /// ends in one is kept.
        /// </summary>
        [Test]
        public async Task Fix_Keeps_Identifier_That_Ends_In_A_Charset_Introducer()
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
            new FakeExecuteExpressionRoot().Sql(""UPDATE Foo SET Name = Col_utf8mb4'$(name)'"");
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
            new FakeExecuteExpressionRoot().Sql(""UPDATE Foo SET Name = Col_utf8mb4$[name]"");
        }
    }
}";

            var expected = Verify.Diagnostic(RawSqlTokenInterpolationAnalyzer.DiagnosticId)
                .WithSpan(10, 84, 10, 91)
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
    }
}
