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

using System.Collections.Generic;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis.Testing;

using NUnit.Framework;
using Verify = Microsoft.CodeAnalysis.CSharp.Testing.CSharpAnalyzerVerifier<
    FluentMigrator.Analyzers.MigrationAttributeVersionShouldBeUniqueAnalyzer,
    Microsoft.CodeAnalysis.Testing.DefaultVerifier
>;
using VerifyTest = Microsoft.CodeAnalysis.CSharp.Testing.CSharpAnalyzerTest<
    FluentMigrator.Analyzers.MigrationAttributeVersionShouldBeUniqueAnalyzer,
    Microsoft.CodeAnalysis.Testing.DefaultVerifier
>;

namespace FluentMigrator.Analyzers.Tests
{
    public class MigrationAttributeVersionShouldBeUniqueUnitTests
    {
        private const string StubDefinitions = @"
    namespace FluentMigrator
    {
        [System.AttributeUsage(System.AttributeTargets.Class, Inherited = false)]
        public class MigrationAttribute : System.Attribute
        {
            public MigrationAttribute(long version, string description) { Version = version; }
            public MigrationAttribute(long version, TransactionBehavior transactionBehavior = TransactionBehavior.Default, string description = null) { Version = version; }
            public long Version { get; }
            public TransactionBehavior TransactionBehavior { get; }
            public string Description { get; }
        }

        public enum TransactionBehavior { Default, None }
    }
";

        [Test]
        public async Task Doesnt_Warns_On_Unique_Migration_Version()
        {
            //language=csharp
            const string source = @"
    using FluentMigrator;

    namespace ConsoleApplication1
    {
        [Migration(1)]
        public class TypeName
        {
        }
    }";

            var ut = new VerifyTest
            {
                TestState =
                {
                    Sources = { source, StubDefinitions },
                    ExpectedDiagnostics =
                    {
                        Capacity = 0
                    },
                },
            };

            ut.TestState.AdditionalReferences.Add(typeof(IMigration).Assembly);
#if NET
            // The referenced FluentMigrator assembly is loaded against the modern runtime, so align the
            // compilation reference set to match and avoid CS1705 assembly-version conflicts.
            ut.ReferenceAssemblies = ReferenceAssemblies.Net.Net80;
#endif

            await ut.RunAsync();
        }

        [Test]
        public async Task Warns_On_Duplicate_Migration_Version()
        {
            //language=csharp
            const string source = @"
    using FluentMigrator;

    namespace ConsoleApplication1
    {
        [Migration(1)]
        public class TypeName
        {
        }

        [Migration(1)]
        public class TypeName2
        {
        }
    }";

            var expected = new List<DiagnosticResult>
            {
                Verify.Diagnostic(MigrationAttributeVersionShouldBeUniqueAnalyzer.DiagnosticId).WithLocation(7, 22).WithArguments("TypeName, TypeName2", "1"),
                Verify.Diagnostic(MigrationAttributeVersionShouldBeUniqueAnalyzer.DiagnosticId).WithLocation(12, 22).WithArguments("TypeName, TypeName2", "1")
            };

            var ut = new VerifyTest
            {
                TestState =
                {
                    Sources = { source, StubDefinitions },
                },
            };

            ut.TestState.ExpectedDiagnostics.AddRange(expected);
            ut.TestState.AdditionalReferences.Add(typeof(IMigration).Assembly);
#if NET
            // The referenced FluentMigrator assembly is loaded against the modern runtime, so align the
            // compilation reference set to match and avoid CS1705 assembly-version conflicts.
            ut.ReferenceAssemblies = ReferenceAssemblies.Net.Net80;
#endif

            await ut.RunAsync();
        }
    }
}
