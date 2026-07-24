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

        [Test]
        public async Task Warns_On_Escaped_Raw_Token()
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

            //language=csharp
            const string fixedSource = @"
    using ConsoleApplication1;

    namespace ConsoleApplication1
    {
        public class TypeName
        {
            public void Up()
            {
                new FakeExecuteExpressionRoot().Sql(""SELECT '$$[[name]]' AS Literal"");
            }
        }
    }";

            var expected = Verify.Diagnostic(RawSqlTokenInterpolationAnalyzer.DiagnosticId)
                .WithSpan(10, 62, 10, 72)
                .WithArguments("$$((name))", "$$[[name]]");

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
