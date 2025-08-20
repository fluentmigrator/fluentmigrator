#region License
//
// Copyright (c) 2007-2024, Fluent Migrator Project
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

using FluentMigrator.Runner;
using FluentMigrator.Runner.Generators;
using FluentMigrator.Runner.Generators.DB2;
using FluentMigrator.Runner.Generators.DB2.iSeries;
using FluentMigrator.Runner.Generators.Firebird;
using FluentMigrator.Runner.Generators.Hana;
#if NETFRAMEWORK
using FluentMigrator.Runner.Generators.Jet;
#endif
using FluentMigrator.Runner.Generators.MySql;
using FluentMigrator.Runner.Generators.Oracle;
using FluentMigrator.Runner.Generators.Postgres;
using FluentMigrator.Runner.Generators.Postgres92;
using FluentMigrator.Runner.Generators.Redshift;
using FluentMigrator.Runner.Generators.Snowflake;
using FluentMigrator.Runner.Generators.SQLite;
using FluentMigrator.Runner.Generators.SqlServer;
using FluentMigrator.Runner.Processors;
using FluentMigrator.Runner.Processors.DB2;
using FluentMigrator.Runner.Processors.DB2.iSeries;
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

namespace FluentMigrator.Tests.Unit.Runners
{
    [TestFixture]
    [Category("Runners")]
    public class DatabaseIdentifierTests
    {

        [Test]
        [TestCase(typeof(Db2Processor), typeof(Db2Generator), ProcessorIdConstants.DB2, GeneratorIdConstants.DB2)]
        [TestCase(typeof(Db2ISeriesProcessor), typeof(Db2ISeriesGenerator), ProcessorIdConstants.Db2ISeries, GeneratorIdConstants.Db2ISeries)]
        [TestCase(typeof(FirebirdProcessor), typeof(FirebirdGenerator), ProcessorIdConstants.Firebird, GeneratorIdConstants.Firebird)]
        [TestCase(typeof(HanaProcessor), typeof(HanaGenerator), ProcessorIdConstants.Hana, GeneratorIdConstants.Hana)]
#if NETFRAMEWORK
        [TestCase(typeof(JetProcessor), typeof(JetGenerator), ProcessorIdConstants.Jet, GeneratorIdConstants.Jet)]
#endif
        [TestCase(typeof(MySql4Processor), typeof(MySql4Generator), ProcessorIdConstants.MySql4, GeneratorIdConstants.MySql4)]
        [TestCase(typeof(MySql5Processor), typeof(MySql5Generator), ProcessorIdConstants.MySql5, GeneratorIdConstants.MySql5)]
        [TestCase(typeof(MySql8Processor), typeof(MySql8Generator), ProcessorIdConstants.MySql8, GeneratorIdConstants.MySql8)]
        [TestCase(typeof(OracleProcessor), typeof(IOracleGenerator), ProcessorIdConstants.Oracle, GeneratorIdConstants.Oracle)]
        [TestCase(typeof(Oracle12CProcessor), typeof(IOracle12CGenerator), ProcessorIdConstants.Oracle12c, GeneratorIdConstants.Oracle12c)]
        [TestCase(typeof(Postgres92Processor), typeof(Postgres92Generator), ProcessorIdConstants.Postgres92, GeneratorIdConstants.Postgres92)]
        [TestCase(typeof(Postgres10_0Processor), typeof(Postgres10_0Generator), ProcessorIdConstants.PostgreSQL10_0, GeneratorIdConstants.PostgreSQL10_0)]
        [TestCase(typeof(Postgres11_0Processor), typeof(Postgres11_0Generator), ProcessorIdConstants.PostgreSQL11_0, GeneratorIdConstants.PostgreSQL11_0)]
        [TestCase(typeof(Postgres15_0Processor), typeof(Postgres15_0Generator), ProcessorIdConstants.PostgreSQL15_0, GeneratorIdConstants.PostgreSQL15_0)]
        [TestCase(typeof(RedshiftProcessor), typeof(RedshiftGenerator), ProcessorIdConstants.Redshift, GeneratorIdConstants.Redshift)]
        [TestCase(typeof(SnowflakeProcessor), typeof(SnowflakeGenerator), ProcessorIdConstants.Snowflake, GeneratorIdConstants.Snowflake)]
        [TestCase(typeof(SQLiteProcessor), typeof(SQLiteGenerator), ProcessorIdConstants.SQLite, GeneratorIdConstants.SQLite)]
        [TestCase(typeof(SqlServer2000Processor), typeof(SqlServer2000Generator), ProcessorIdConstants.SqlServer2000, GeneratorIdConstants.SqlServer2000)]
        [TestCase(typeof(SqlServer2005Processor), typeof(SqlServer2005Generator), ProcessorIdConstants.SqlServer2005, GeneratorIdConstants.SqlServer2005)]
        [TestCase(typeof(SqlServer2008Processor), typeof(SqlServer2008Generator), ProcessorIdConstants.SqlServer2008, GeneratorIdConstants.SqlServer2008)]
        [TestCase(typeof(SqlServer2012Processor), typeof(SqlServer2012Generator), ProcessorIdConstants.SqlServer2012, GeneratorIdConstants.SqlServer2012)]
        [TestCase(typeof(SqlServer2016Processor), typeof(SqlServer2016Generator), ProcessorIdConstants.SqlServer2016, GeneratorIdConstants.SqlServer2016)]
        public void ProcessorIdMatchesGeneratorId(Type processor, Type generator, string processorId, string generatorId)
        {
            var serviceProvider = GetServiceProvider(processorId, generatorId);

            var migrationProcessor = ((IQuerySchema)serviceProvider.GetRequiredService(processor));
            var migrationGenerator = (IMigrationGenerator)serviceProvider.GetRequiredService(generator);
            var migrationProcessorAccessor = serviceProvider.GetRequiredService<IProcessorAccessor>();
            var migrationGeneratorAccessor = serviceProvider.GetRequiredService<IGeneratorAccessor>();
            
            Assert.That(migrationProcessor.DatabaseType, Is.EqualTo(processorId));
            Assert.That(migrationGenerator.GeneratorId, Is.EqualTo(generatorId));
            Assert.That(migrationProcessor.DatabaseType, Is.EqualTo(
                migrationGenerator.GeneratorId));
            Assert.That(migrationProcessorAccessor.Processor.DatabaseType, Is.EqualTo(processorId));
            Assert.That(migrationGeneratorAccessor.Generator.GeneratorId, Is.EqualTo(generatorId));
        }

        // MySQL
        [TestCase(typeof(MySql4Processor), typeof(MySql4Generator), ProcessorIdConstants.MySql4, GeneratorIdConstants.MySql4, ProcessorIdConstants.MySql, GeneratorIdConstants.MySql, false)]
        [TestCase(typeof(MySql5Processor), typeof(MySql5Generator), ProcessorIdConstants.MySql5, GeneratorIdConstants.MySql5, ProcessorIdConstants.MySql, GeneratorIdConstants.MySql, false)]
        [TestCase(typeof(MySql8Processor), typeof(MySql8Generator), ProcessorIdConstants.MySql8, GeneratorIdConstants.MySql8, ProcessorIdConstants.MySql, GeneratorIdConstants.MySql, true)]
        // PostgreSQL
        [TestCase(typeof(Postgres92Processor), typeof(Postgres92Generator), ProcessorIdConstants.Postgres92, GeneratorIdConstants.Postgres92, ProcessorIdConstants.PostgreSQL, GeneratorIdConstants.PostgreSQL, false)]
        [TestCase(typeof(Postgres10_0Processor), typeof(Postgres10_0Generator), ProcessorIdConstants.PostgreSQL10_0, GeneratorIdConstants.PostgreSQL10_0, ProcessorIdConstants.PostgreSQL, GeneratorIdConstants.PostgreSQL, false)]
        [TestCase(typeof(Postgres11_0Processor), typeof(Postgres11_0Generator), ProcessorIdConstants.PostgreSQL11_0, GeneratorIdConstants.PostgreSQL11_0, ProcessorIdConstants.PostgreSQL, GeneratorIdConstants.PostgreSQL, false)]
        [TestCase(typeof(Postgres15_0Processor), typeof(Postgres15_0Generator), ProcessorIdConstants.PostgreSQL15_0, GeneratorIdConstants.PostgreSQL15_0, ProcessorIdConstants.PostgreSQL, GeneratorIdConstants.PostgreSQL, true)]
        // SQL Server
        [TestCase(typeof(SqlServer2000Processor), typeof(SqlServer2000Generator), ProcessorIdConstants.SqlServer2000, GeneratorIdConstants.SqlServer2000, ProcessorIdConstants.SqlServer, GeneratorIdConstants.SqlServer,  false)]
        [TestCase(typeof(SqlServer2005Processor), typeof(SqlServer2005Generator), ProcessorIdConstants.SqlServer2005, GeneratorIdConstants.SqlServer2005, ProcessorIdConstants.SqlServer, GeneratorIdConstants.SqlServer, false)]
        [TestCase(typeof(SqlServer2008Processor), typeof(SqlServer2008Generator), ProcessorIdConstants.SqlServer2008, GeneratorIdConstants.SqlServer2008, ProcessorIdConstants.SqlServer, GeneratorIdConstants.SqlServer, false)]
        [TestCase(typeof(SqlServer2012Processor), typeof(SqlServer2012Generator), ProcessorIdConstants.SqlServer2012, GeneratorIdConstants.SqlServer2012, ProcessorIdConstants.SqlServer, GeneratorIdConstants.SqlServer, false)]
        [TestCase(typeof(SqlServer2016Processor), typeof(SqlServer2016Generator), ProcessorIdConstants.SqlServer2016, GeneratorIdConstants.SqlServer2016, ProcessorIdConstants.SqlServer, GeneratorIdConstants.SqlServer, true)]
        public void EnsureAliasMatchesLatestVersion(Type processor, Type generator, string processorId, string generatorId, string processorIdAlias, string generatorIdAlias, bool isLatestVersion)
        {
            var serviceProvider = GetServiceProvider(processorIdAlias, generatorIdAlias);

            var migrationProcessorAccessor = serviceProvider.GetRequiredService<IProcessorAccessor>();
            var migrationGeneratorAccessor = serviceProvider.GetRequiredService<IGeneratorAccessor>();

            var processorMatcher = isLatestVersion ? Is.EqualTo(processorId) : Is.Not.EqualTo(processorId);
            var generatorMatcher = isLatestVersion ? Is.EqualTo(generatorId) : Is.Not.EqualTo(generatorId);
            Assert.That(migrationProcessorAccessor.Processor.DatabaseType, processorMatcher);
            Assert.That(migrationGeneratorAccessor.Generator.GeneratorId, generatorMatcher);
        }
        
        private IServiceProvider GetServiceProvider(string processorId, string generatorId)
        {
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
