#region License
// Copyright (c) 2018, Fluent Migrator Project
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
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Reflection;

using Autofac.Core;

using FluentMigrator.Runner.Initialization;
using FluentMigrator.Runner.Processors;
using FluentMigrator.Runner.Processors.DB2;
using FluentMigrator.Runner.Processors.DB2.iSeries;
using FluentMigrator.Runner.Processors.DotConnectOracle;
using FluentMigrator.Runner.Processors.Firebird;
using FluentMigrator.Runner.Processors.Hana;
using FluentMigrator.Runner.Processors.Jet;
using FluentMigrator.Runner.Processors.MySql;
using FluentMigrator.Runner.Processors.Oracle;
using FluentMigrator.Runner.Processors.Postgres;
using FluentMigrator.Runner.Processors.Postgres92;
using FluentMigrator.Runner.Processors.Redshift;
using FluentMigrator.Runner.Processors.Snowflake;
using FluentMigrator.Runner.Processors.SQLite;
using FluentMigrator.Runner.Processors.SqlServer;

using Microsoft.Extensions.DependencyInjection;

using NUnit.Framework;

namespace FluentMigrator.Tests.Unit.Processors
{
    [TestFixture]
    [Category("ConnectionFactory")]
    public sealed class ProcessorConstructorSelectionTests
    {
        public static IEnumerable<Type> ProcessorTypes()
        {
            yield return typeof(Db2ISeriesProcessor);
            yield return typeof(Db2Processor);
            yield return typeof(FirebirdProcessor);
            yield return typeof(HanaProcessor);
            yield return typeof(JetProcessor);
            yield return typeof(MySql4Processor);
            yield return typeof(MySql5Processor);
            yield return typeof(MySql8Processor);
            yield return typeof(DotConnectOracle12CProcessor);
            yield return typeof(DotConnectOracleProcessor);
            yield return typeof(Oracle12CManagedProcessor);
            yield return typeof(Oracle12CProcessor);
            yield return typeof(OracleManagedProcessor);
            yield return typeof(OracleProcessor);
            yield return typeof(Postgres10_0Processor);
            yield return typeof(Postgres11_0Processor);
            yield return typeof(Postgres15_0Processor);
            yield return typeof(PostgresProcessor);
            yield return typeof(Postgres92Processor);
            yield return typeof(RedshiftProcessor);
            yield return typeof(SnowflakeProcessor);
            yield return typeof(SQLiteProcessor);
            yield return typeof(SqlServer2000Processor);
            yield return typeof(SqlServer2005Processor);
            yield return typeof(SqlServer2008Processor);
            yield return typeof(SqlServer2012Processor);
            yield return typeof(SqlServer2014Processor);
            yield return typeof(SqlServer2016Processor);
        }

        [Test]
        [TestCaseSource(nameof(ProcessorTypes))]
        public void MigrationConnectionFactoryConstructorIsPreferredByActivatorUtilities(Type processorType)
        {
            var publicConstructors = processorType.GetConstructors();

            var oldConstructor = publicConstructors.SingleOrDefault(
                ctor => ctor.GetParameters().Any(
                    p => p.ParameterType == typeof(IConnectionStringAccessor)));

            var newConstructor = publicConstructors.SingleOrDefault(
                ctor => ctor.GetParameters().Any(
                    p => p.ParameterType == typeof(IMigrationConnectionFactory)));

            using (Assert.EnterMultipleScope())
            {
                Assert.That(
                    oldConstructor,
                    Is.Not.Null,
                    $"{processorType.FullName} should keep the old {nameof(IConnectionStringAccessor)} constructor for backward compatibility.");

                Assert.That(
                    newConstructor,
                    Is.Not.Null,
                    $"{processorType.FullName} should have a new {nameof(IMigrationConnectionFactory)} constructor.");

                Assert.That(
                    oldConstructor.GetCustomAttribute<ObsoleteAttribute>(),
                    Is.Not.Null,
                    $"{processorType.FullName} old {nameof(IConnectionStringAccessor)} constructor should be obsolete.");

                Assert.That(
                    newConstructor.GetCustomAttribute<ActivatorUtilitiesConstructorAttribute>(),
                    Is.Not.Null,
                    $"{processorType.FullName} new {nameof(IMigrationConnectionFactory)} constructor should be marked with {nameof(ActivatorUtilitiesConstructorAttribute)}.");
            }
        }

        [Test]
        public void ActivatorUtilitiesThrowsWhenConnectionConstructorsAreAmbiguous()
        {
            var services = new ServiceCollection();

            services.AddScoped<IConnectionStringAccessor, TestConnectionStringAccessor>();
            services.AddScoped<IMigrationConnectionFactory, TestMigrationConnectionFactory>();

            using (var serviceProvider = services.BuildServiceProvider())
            {
                var ex = Assert.Throws<InvalidOperationException>(
                    () => ActivatorUtilities.CreateInstance<AmbiguousProcessor>(serviceProvider));

                Assert.That(
                    ex.Message,
                    Does.Contain("ambiguous").IgnoreCase);
            }
        }

        [Test]
        public void ActivatorUtilitiesUsesMigrationConnectionFactoryConstructorWhenMarked()
        {
            var services = new ServiceCollection();

            services.AddScoped<IConnectionStringAccessor, TestConnectionStringAccessor>();
            services.AddScoped<IMigrationConnectionFactory, TestMigrationConnectionFactory>();

            using (var serviceProvider = services.BuildServiceProvider())
            {
                var processor = ActivatorUtilities.CreateInstance<PreferredProcessor>(serviceProvider);

                Assert.That(
                    processor.ConstructorUsed,
                    Is.EqualTo(ConstructorUsed.MigrationConnectionFactory));
            }
        }

        [Test]
        public void ServiceProviderResolutionThrowsEvenWhenMigrationConnectionFactoryConstructorIsMarked()
        {
            var services = new ServiceCollection();

            services.AddScoped<IConnectionStringAccessor, TestConnectionStringAccessor>();
            services.AddScoped<IMigrationConnectionFactory, TestMigrationConnectionFactory>();

            services.AddScoped<PreferredProcessor>();

            using (var serviceProvider = services.BuildServiceProvider())
            {
                var ex = Assert.Throws<InvalidOperationException>(
                    () => serviceProvider.GetRequiredService<PreferredProcessor>());

                Assert.That(
                    ex.Message,
                    Does.Contain("ambiguous").IgnoreCase);
            }
        }

        [Test]
        public void ServiceProviderResolutionThrowsWhenConnectionConstructorsAreAmbiguous()
        {
            var services = new ServiceCollection();

            services.AddScoped<IConnectionStringAccessor, TestConnectionStringAccessor>();
            services.AddScoped<IMigrationConnectionFactory, TestMigrationConnectionFactory>();

            services.AddScoped<AmbiguousProcessor>();

            using (var serviceProvider = services.BuildServiceProvider())
            {
                var ex = Assert.Throws<InvalidOperationException>(
                    () => serviceProvider.GetRequiredService<AmbiguousProcessor>());

                Assert.That(
                    ex.Message,
                    Does.Contain("ambiguous").IgnoreCase);
            }
        }

        private enum ConstructorUsed
        {
            ConnectionStringAccessor,
            MigrationConnectionFactory
        }

        private sealed class AmbiguousProcessor
        {
            public AmbiguousProcessor(IConnectionStringAccessor connectionStringAccessor)
            {
                ConstructorUsed = ConstructorUsed.ConnectionStringAccessor;
            }

            public AmbiguousProcessor(IMigrationConnectionFactory migrationConnectionFactory)
            {
                ConstructorUsed = ConstructorUsed.MigrationConnectionFactory;
            }

            public ConstructorUsed ConstructorUsed { get; }
        }

        private sealed class PreferredProcessor
        {
            public PreferredProcessor(IConnectionStringAccessor connectionStringAccessor)
            {
                ConstructorUsed = ConstructorUsed.ConnectionStringAccessor;
            }

            [ActivatorUtilitiesConstructor]
            public PreferredProcessor(IMigrationConnectionFactory migrationConnectionFactory)
            {
                ConstructorUsed = ConstructorUsed.MigrationConnectionFactory;
            }

            public ConstructorUsed ConstructorUsed { get; }
        }

        private sealed class TestMigrationConnectionFactory : IMigrationConnectionFactory
        {
            public bool HasConnection => true;

            public IDbConnection CreateConnection(DbProviderFactory providerFactory)
            {
                throw new NotSupportedException();
            }
        }

        private sealed class TestConnectionStringAccessor : IConnectionStringAccessor
        {
            public string ConnectionString => "Data Source=test.db";
        }
    }
}
