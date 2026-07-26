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
    FluentMigrator.Analyzers.LegacyStringDictionaryParameterAnalyzer,
    FluentMigrator.Analyzers.LegacyStringDictionaryParameterCodeFixProvider,
    Microsoft.CodeAnalysis.Testing.DefaultVerifier
>;

namespace FluentMigrator.Analyzers.Tests
{
    public class LegacyStringDictionaryParameterUnitTests
    {
        private const string StubDefinitions = @"
    namespace FluentMigrator.Builders.Execute
    {
        using System.Collections.Generic;

        public interface IExecuteExpressionRoot
        {
            void Sql(string sqlStatement);
            void Sql(string sqlStatement, IDictionary<string, object> parameters);
            void Sql(string sqlStatement, IDictionary<string, string> parameters);
            void Script(string pathToSqlScript, IDictionary<string, object> parameters);
            void Script(string pathToSqlScript, IDictionary<string, string> parameters);
            void EmbeddedScript(string embeddedSqlScriptName, IDictionary<string, object> parameters);
            void EmbeddedScript(string embeddedSqlScriptName, IDictionary<string, string> parameters);
        }
    }

    namespace ConsoleApplication1
    {
        using System.Collections.Generic;
        using FluentMigrator.Builders.Execute;

        public class FakeExecuteExpressionRoot : IExecuteExpressionRoot
        {
            public void Sql(string sqlStatement) { }
            public void Sql(string sqlStatement, IDictionary<string, object> parameters) { }
            public void Sql(string sqlStatement, IDictionary<string, string> parameters) { }
            public void Script(string pathToSqlScript, IDictionary<string, object> parameters) { }
            public void Script(string pathToSqlScript, IDictionary<string, string> parameters) { }
            public void EmbeddedScript(string embeddedSqlScriptName, IDictionary<string, object> parameters) { }
            public void EmbeddedScript(string embeddedSqlScriptName, IDictionary<string, string> parameters) { }
        }
    }
";

        private static VerifyTest CreateTest(string source, string fixedSource)
        {
            var ut = new VerifyTest
            {
                TestState =
                {
                    Sources = { source, StubDefinitions },
                },
                FixedState =
                {
                    Sources = { fixedSource, StubDefinitions },
                },
            };

#if NET
            ut.ReferenceAssemblies = ReferenceAssemblies.Net.Net80;
#endif

            return ut;
        }

        [Test]
        public async Task Warns_And_Fixes_Inline_String_Dictionary()
        {
            //language=csharp
            const string source = @"
    using System.Collections.Generic;
    using ConsoleApplication1;

    namespace ConsoleApplication1
    {
        public class TypeName
        {
            public void Up()
            {
                new FakeExecuteExpressionRoot().Sql(""SELECT * FROM Foo WHERE Name = $[name]"", {|FMUP0001:new Dictionary<string, string> { { ""name"", ""value"" } }|});
            }
        }
    }";

            //language=csharp
            const string fixedSource = @"
    using System.Collections.Generic;
    using ConsoleApplication1;

    namespace ConsoleApplication1
    {
        public class TypeName
        {
            public void Up()
            {
                new FakeExecuteExpressionRoot().Sql(""SELECT * FROM Foo WHERE Name = $[name]"", new Dictionary<string, object> { { ""name"", ""value"" } });
            }
        }
    }";

            await CreateTest(source, fixedSource).RunAsync();
        }

        [Test]
        public async Task Warns_And_Fixes_Local_Variable_String_Dictionary()
        {
            //language=csharp
            const string source = @"
    using System.Collections.Generic;
    using ConsoleApplication1;

    namespace ConsoleApplication1
    {
        public class TypeName
        {
            public void Up()
            {
                IDictionary<string, string> parameters = new Dictionary<string, string> { { ""name"", ""value"" } };
                new FakeExecuteExpressionRoot().Sql(""SELECT * FROM Foo WHERE Name = $[name]"", {|FMUP0001:parameters|});
            }
        }
    }";

            //language=csharp
            const string fixedSource = @"
    using System.Collections.Generic;
    using ConsoleApplication1;

    namespace ConsoleApplication1
    {
        public class TypeName
        {
            public void Up()
            {
                IDictionary<string, object> parameters = new Dictionary<string, object> { { ""name"", ""value"" } };
                new FakeExecuteExpressionRoot().Sql(""SELECT * FROM Foo WHERE Name = $[name]"", parameters);
            }
        }
    }";

            await CreateTest(source, fixedSource).RunAsync();
        }

        [Test]
        public async Task Warns_And_Fixes_Var_Local_Variable_String_Dictionary()
        {
            //language=csharp
            const string source = @"
    using System.Collections.Generic;
    using ConsoleApplication1;

    namespace ConsoleApplication1
    {
        public class TypeName
        {
            public void Up()
            {
                var parameters = new Dictionary<string, string> { { ""name"", ""value"" } };
                new FakeExecuteExpressionRoot().Script(""script.sql"", {|FMUP0001:parameters|});
            }
        }
    }";

            //language=csharp
            const string fixedSource = @"
    using System.Collections.Generic;
    using ConsoleApplication1;

    namespace ConsoleApplication1
    {
        public class TypeName
        {
            public void Up()
            {
                var parameters = new Dictionary<string, object> { { ""name"", ""value"" } };
                new FakeExecuteExpressionRoot().Script(""script.sql"", parameters);
            }
        }
    }";

            await CreateTest(source, fixedSource).RunAsync();
        }

        [Test]
        public async Task Doesnt_Warn_On_Object_Dictionary()
        {
            //language=csharp
            const string source = @"
    using System.Collections.Generic;
    using ConsoleApplication1;

    namespace ConsoleApplication1
    {
        public class TypeName
        {
            public void Up()
            {
                new FakeExecuteExpressionRoot().Sql(""SELECT * FROM Foo WHERE Name = $[name]"", new Dictionary<string, object> { { ""name"", ""value"" } });
            }
        }
    }";

            await CreateTest(source, source).RunAsync();
        }

        [Test]
        public async Task Doesnt_Warn_On_Non_Execute_Method()
        {
            //language=csharp
            const string source = @"
    using System.Collections.Generic;

    namespace ConsoleApplication1
    {
        public class TypeName
        {
            public void Sql(string sqlStatement, IDictionary<string, string> parameters) { }

            public void Up()
            {
                Sql(""SELECT 1"", new Dictionary<string, string>());
            }
        }
    }";

            await CreateTest(source, source).RunAsync();
        }
    }
}
