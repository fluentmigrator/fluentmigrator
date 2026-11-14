#region License
// Copyright (c) 2007-2024, Fluent Migrator Project
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

#if !NETFRAMEWORK
using FluentMigrator.DotNet.Cli;
using FluentMigrator.DotNet.Cli.Commands;
using FluentMigrator.Runner.Generators;

using McMaster.Extensions.CommandLineUtils;

using Microsoft.Extensions.DependencyInjection;

using NUnit.Framework;

using Shouldly;

namespace FluentMigrator.Tests.Unit.Commands
{
    [TestFixture]
    public class GeneratorSelectionTests
    {
        [TestCase("Oracle12cManaged", null, "Oracle12C")]
        [TestCase("Oracle12cManaged", "Oracle", "Oracle")]
        [TestCase("Oracle12cManaged", "Oracle12c", "Oracle12C")]
        [TestCase("Oracle12c", null, "Oracle12C")]
        [TestCase("OracleManaged", null, "OracleManaged")]
        [TestCase("SQLite", null, "SQLite")]
        [TestCase("SQLite", "SqlServer2016", "SqlServer2016")]
        public void SelectsTheExpectedGenerator(string processor, string generator, string expectedGeneratorType)
        {
            var command = new Migrate { ProcessorType = processor, GeneratorType = generator, TargetAssemblies = new string[0] };
            var options = MigratorOptions.CreateMigrateUp(command);

            using var serviceProvider = Setup.BuildServiceProvider(options, PhysicalConsole.Singleton);
            var selected = serviceProvider.GetRequiredService<IGeneratorAccessor>().Generator;

            selected.GetType().Name.ShouldBe(expectedGeneratorType + "Generator");
        }
    }
}
#endif
