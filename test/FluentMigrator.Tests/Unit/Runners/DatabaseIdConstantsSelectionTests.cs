#region License
//
// Copyright (c) 2026, Fluent Migrator Project
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//   http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using FluentMigrator.Runner;
using FluentMigrator.Runner.Generators;
using FluentMigrator.Runner.Processors;

using Microsoft.Extensions.DependencyInjection;

using NUnit.Framework;

namespace FluentMigrator.Tests.Unit.Runners
{
    /// <summary>
    /// Verifies that <b>every</b> constant published in <see cref="ProcessorIdConstants"/> and
    /// <see cref="GeneratorIdConstants"/> can actually be selected via
    /// <c>SelectingProcessorAccessor</c> / <c>SelectingGeneratorAccessor</c> when all providers
    /// are registered.
    /// </summary>
    /// <remarks>
    /// Regression coverage for issue #2305: after #2018, no registered
    /// <c>IMigrationProcessor</c> carried the ID <c>Postgres</c> anymore, so
    /// <c>dotnet fm migrate -p Postgres</c> failed with
    /// <c>ProcessorFactoryNotFoundException</c> even though
    /// <c>ProcessorIdConstants.Postgres</c> exists and the documentation advertises it.
    /// <para>
    /// Unlike the hand-maintained test case lists in <see cref="DatabaseIdentifierTests"/>,
    /// these tests enumerate the constants classes via reflection, so any constant added in the
    /// future is covered automatically; a constant that is published but not selectable is a
    /// bug in either the constant or the alias lists. Reflection also sidesteps the
    /// <see cref="ObsoleteAttribute"/> on the Hana constants.
    /// </para>
    /// </remarks>
    [TestFixture]
    [Category("Runners")]
    public class DatabaseIdConstantsSelectionTests
    {
        private static IEnumerable<TestCaseData> GetConstants(Type constantsClass)
        {
            var fields = constantsClass
                .GetFields(BindingFlags.Public | BindingFlags.Static)
                .Where(f => f.IsLiteral && f.FieldType == typeof(string))
                .OrderBy(f => f.Name, StringComparer.Ordinal);

            foreach (var field in fields)
            {
#if !NETFRAMEWORK
                // The Jet provider is only functional on the .NET Framework; its test cases in
                // DatabaseIdentifierTests are guarded the same way.
                if (field.Name == "Jet")
                {
                    continue;
                }
#endif
                var value = (string)field.GetRawConstantValue();
                yield return new TestCaseData(value).SetArgDisplayNames($"{field.Name} (\"{value}\")");
            }
        }

        private static IEnumerable<TestCaseData> ProcessorIdConstantValues()
            => GetConstants(typeof(ProcessorIdConstants));

        private static IEnumerable<TestCaseData> GeneratorIdConstantValues()
            => GetConstants(typeof(GeneratorIdConstants));

        [TestCaseSource(nameof(ProcessorIdConstantValues))]
        public void EveryProcessorIdConstantIsSelectable(string processorId)
        {
            var serviceProvider = GetServiceProvider(processorId, generatorId: null);

            var accessor = serviceProvider.GetRequiredService<IProcessorAccessor>();

            Assert.That(accessor.Processor, Is.Not.Null);

            var matchesId =
                string.Equals(accessor.Processor.DatabaseType, processorId, StringComparison.OrdinalIgnoreCase)
             || accessor.Processor.DatabaseTypeAliases.Any(
                    alias => string.Equals(alias, processorId, StringComparison.OrdinalIgnoreCase));
            Assert.That(
                matchesId,
                Is.True,
                $"Processor {accessor.Processor.GetType().Name} was selected for ID \"{processorId}\" but neither its DatabaseType (\"{accessor.Processor.DatabaseType}\") nor its aliases match.");
        }

        [TestCaseSource(nameof(GeneratorIdConstantValues))]
        public void EveryGeneratorIdConstantIsSelectable(string generatorId)
        {
            var serviceProvider = GetServiceProvider(processorId: null, generatorId);

            var accessor = serviceProvider.GetRequiredService<IGeneratorAccessor>();

            Assert.That(accessor.Generator, Is.Not.Null);

            // Mirrors the three match strategies of SelectingGeneratorAccessor.FindGenerator:
            // GeneratorId, GeneratorIdAliases, and the type name without the "Generator" suffix.
            var matchesId =
                string.Equals(accessor.Generator.GeneratorId, generatorId, StringComparison.OrdinalIgnoreCase)
             || accessor.Generator.GeneratorIdAliases.Any(
                    alias => string.Equals(alias, generatorId, StringComparison.OrdinalIgnoreCase))
             || string.Equals(
                    accessor.Generator.GetType().Name.Replace("Generator", string.Empty),
                    generatorId,
                    StringComparison.OrdinalIgnoreCase);
            Assert.That(
                matchesId,
                Is.True,
                $"Generator {accessor.Generator.GetType().Name} was selected for ID \"{generatorId}\" but neither its GeneratorId (\"{accessor.Generator.GeneratorId}\") nor its aliases match.");
        }

        private static IServiceProvider GetServiceProvider(string processorId, string generatorId)
        {
            // Keep the provider list in sync with DatabaseIdentifierTests.GetServiceProvider.
            var serviceProvider = new ServiceCollection()
                .AddFluentMigratorCore()
                .ConfigureRunner(
                    builder => builder
                        .AddDb2()
                        .AddDb2ISeries()
                        .AddDotConnectOracle()
                        .AddDotConnectOracle12C()
                        .AddFirebird()
                        .AddHana()
                        .AddJet()
                        .AddMySql()
                        .AddMySql4()
                        .AddMySql5()
                        .AddMySql8()
                        .AddOracle()
                        .AddOracle12C()
                        .AddOracleManaged()
                        .AddOracle12CManaged()
                        .AddPostgres()
                        .AddPostgres92()
                        .AddPostgres10_0()
                        .AddPostgres11_0()
                        .AddPostgres15_0()
                        .AddRedshift()
                        .AddSnowflake()
                        .AddSQLite()
                        .AddSqlServer()
                        .AddSqlServer2000()
                        .AddSqlServer2005()
                        .AddSqlServer2008()
                        .AddSqlServer2012()
                        .AddSqlServer2014()
                        .AddSqlServer2016()
                )
                .Configure<SelectingProcessorAccessorOptions>(opt => opt.ProcessorId = processorId)
                .Configure<SelectingGeneratorAccessorOptions>(opt => opt.GeneratorId = generatorId)
                .BuildServiceProvider();
            return serviceProvider;
        }
    }
}
