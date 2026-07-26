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

using FluentMigrator.Runner;
using FluentMigrator.Runner.Generators.Oracle;
using FluentMigrator.Runner.Initialization;
using FluentMigrator.Runner.Processors.Oracle;

using Microsoft.Extensions.DependencyInjection;

using NUnit.Framework;

namespace FluentMigrator.Tests.Unit.Runners
{
    /// <summary>
    /// Verifies that each Oracle runner registration method produces a service collection
    /// from which the processor can be activated <b>in isolation</b>.
    /// </summary>
    /// <remarks>
    /// Regression coverage for the DI break introduced by PR #2269 and fixed by PR #2313:
    /// <c>AddOracleManaged()</c> stopped registering <c>IOracleGenerator</c>, but
    /// <c>OracleManagedProcessor</c>'s constructor still requires it, so every managed Oracle
    /// integration test failed at setup with "Unable to resolve service for type
    /// 'IOracleGenerator' while attempting to activate 'OracleManagedProcessor'".
    /// <para>
    /// <see cref="DatabaseIdentifierTests"/> did not catch this because it registers
    /// <b>all</b> providers in one container: <c>AddOracle()</c> runs before
    /// <c>AddOracleManaged()</c> and registers <c>IOracleGenerator</c> as a side effect.
    /// These tests therefore build one container per registration method, mirroring how the
    /// integration test fixtures (and real applications) configure a single provider.
    /// </para>
    /// </remarks>
    [TestFixture]
    [Category("Runners")]
    public class OracleRunnerBuilderExtensionsTests
    {
        private static ServiceProvider CreateServiceProvider(Action<IMigrationRunnerBuilder> registerOracleServices)
        {
            return new ServiceCollection()
                .AddFluentMigratorCore()
                .ConfigureRunner(registerOracleServices)
                // No connection is ever opened: GenericProcessorBase reads the connection
                // string eagerly but opens the connection lazily.
                .AddScoped<IConnectionStringReader>(
                    _ => new PassThroughConnectionStringReader("Data Source=fake;User Id=fake;Password=fake"))
                .BuildServiceProvider(validateScopes: true);
        }

        private static IEnumerable<TestCaseData> IsolatedRegistrationTestCases()
        {
            yield return new TestCaseData(
                    (Action<IMigrationRunnerBuilder>)(r => r.AddOracle()),
                    typeof(OracleProcessor),
                    ProcessorIdConstants.Oracle,
                    GeneratorIdConstants.Oracle)
                .SetArgDisplayNames(nameof(OracleRunnerBuilderExtensions.AddOracle));

            yield return new TestCaseData(
                    (Action<IMigrationRunnerBuilder>)(r => r.AddOracleManaged()),
                    typeof(OracleManagedProcessor),
                    ProcessorIdConstants.OracleManaged,
                    GeneratorIdConstants.OracleManaged)
                .SetArgDisplayNames(nameof(OracleRunnerBuilderExtensions.AddOracleManaged));

            yield return new TestCaseData(
                    (Action<IMigrationRunnerBuilder>)(r => r.AddOracle12C()),
                    typeof(Oracle12CProcessor),
                    ProcessorIdConstants.Oracle12c,
                    GeneratorIdConstants.Oracle12c)
                .SetArgDisplayNames(nameof(OracleRunnerBuilderExtensions.AddOracle12C));

            yield return new TestCaseData(
                    (Action<IMigrationRunnerBuilder>)(r => r.AddOracle12CManaged()),
                    typeof(Oracle12CManagedProcessor),
                    ProcessorIdConstants.Oracle12cManaged,
                    GeneratorIdConstants.Oracle12c)
                .SetArgDisplayNames(nameof(OracleRunnerBuilderExtensions.AddOracle12CManaged));
        }

        [TestCaseSource(nameof(IsolatedRegistrationTestCases))]
        public void IsolatedRegistrationResolvesProcessorAndGenerator(
            Action<IMigrationRunnerBuilder> registerOracleServices,
            Type expectedProcessorType,
            string expectedDatabaseType,
            string expectedGeneratorId)
        {
            using (var serviceProvider = CreateServiceProvider(registerOracleServices))
            using (var scope = serviceProvider.CreateScope())
            {
                // Mirrors OracleProcessorTestsBase.SetUp()
                var processor = scope.ServiceProvider.GetRequiredService<OracleProcessorBase>();
                var migrationProcessor = scope.ServiceProvider.GetRequiredService<IMigrationProcessor>();
                var generator = scope.ServiceProvider.GetRequiredService<IMigrationGenerator>();

                Assert.Multiple(() =>
                {
                    Assert.That(processor, Is.TypeOf(expectedProcessorType));
                    Assert.That(migrationProcessor, Is.SameAs(processor));
                    Assert.That(processor.DatabaseType, Is.EqualTo(expectedDatabaseType));
                    Assert.That(generator.GeneratorId, Is.EqualTo(expectedGeneratorId));
                });
            }
        }

        [Test]
        public void AddOracleManagedMapsOracleGeneratorOntoManagedGenerator()
        {
            // PR #2313: IOracleGenerator must resolve on the managed path (the processor's
            // constructor requires it) and must be the same instance as IOracleManagedGenerator
            // so the processor and the runner use one generator with GeneratorId "OracleManaged".
            using (var serviceProvider = CreateServiceProvider(r => r.AddOracleManaged()))
            using (var scope = serviceProvider.CreateScope())
            {
                var oracleGenerator = scope.ServiceProvider.GetRequiredService<IOracleGenerator>();
                var managedGenerator = scope.ServiceProvider.GetRequiredService<IOracleManagedGenerator>();

                Assert.Multiple(() =>
                {
                    Assert.That(oracleGenerator, Is.SameAs(managedGenerator));
                    Assert.That(oracleGenerator.GeneratorId, Is.EqualTo(GeneratorIdConstants.OracleManaged));
                });
            }
        }

        [Test]
        public void OracleAndOracleManagedResolveWhenRegisteredTogether([Values] bool oracleFirst)
        {
            // IOracleGenerator uses TryAdd semantics, so registration order determines which
            // implementation backs it when both providers are registered. Both processors must
            // remain activatable in either order.
            Action<IMigrationRunnerBuilder> register = oracleFirst
                ? r => r.AddOracle().AddOracleManaged()
                : r => r.AddOracleManaged().AddOracle();

            using (var serviceProvider = CreateServiceProvider(register))
            using (var scope = serviceProvider.CreateScope())
            {
                var oracleProcessor = scope.ServiceProvider.GetRequiredService<OracleProcessor>();
                var oracleManagedProcessor = scope.ServiceProvider.GetRequiredService<OracleManagedProcessor>();

                Assert.Multiple(() =>
                {
                    Assert.That(oracleProcessor.DatabaseType, Is.EqualTo(ProcessorIdConstants.Oracle));
                    Assert.That(oracleManagedProcessor.DatabaseType, Is.EqualTo(ProcessorIdConstants.OracleManaged));
                });
            }
        }
    }
}
