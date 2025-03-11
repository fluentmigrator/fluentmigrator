#region License
//
// Copyright (c) 2018, Fluent Migrator Project
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
using System.Diagnostics;

using FluentMigrator.Runner;
using FluentMigrator.Runner.Generators.Snowflake;
using FluentMigrator.Runner.Initialization;
using FluentMigrator.Runner.Processors.Snowflake;
using FluentMigrator.Tests.Helpers;

using Microsoft.Extensions.DependencyInjection;

using NUnit.Framework;

using Shouldly;

using SnowflakeDbFactory = Snowflake.Data.Client.SnowflakeDbFactory;

namespace FluentMigrator.Tests.Integration.Processors.Snowflake
{
    [TestFixture(true)]
    [TestFixture(false)]
    [Category("Integration")]
    [Category("Snowflake")]
    public class SnowflakeSchemaExtensionsTests : BaseSchemaExtensionsTests
    {
        private ServiceProvider ServiceProvider { get; set; }
        private IServiceScope ServiceScope { get; set; }
        private SnowflakeProcessor Processor { get; set; }
        private SnowflakeQuoter Quoter { get; set; }
        private const string TestSchemaSingleQuote = "test'schema";

        private readonly bool _quotingEnabled;

        public SnowflakeSchemaExtensionsTests(bool quotingEnabled)
        {
            _quotingEnabled = quotingEnabled;
            try { EnsureReference(); } catch { /* ignore */ }
        }

        [Test]
        public override void CallingColumnExistsCanAcceptSchemaNameWithSingleQuote()
        {
            SnowflakeTestTable TableCreationFunc() => new SnowflakeTestTable(Processor, TestSchemaSingleQuote, $"{Quoter.Quote("id")} int");

            if (_quotingEnabled)
            {
                using (var table = TableCreationFunc())
                    Processor.ColumnExists(TestSchemaSingleQuote, table.Name, "id").ShouldBeTrue();
            }
            else
            {
                // Creating a table with single quote in schema name is a bad idea if identifier quoting is disabled.
                var ex = Assert.Throws<SnowflakeException>(() => TableCreationFunc());
                Assert.That(ex.Message, Does.StartWith("An error occurred executing the following sql"));
            }
        }

        [Test]
        public override void CallingConstraintExistsCanAcceptSchemaNameWithSingleQuote()
        {
            SnowflakeTestTable TableCreationFunc() => new SnowflakeTestTable(Processor, TestSchemaSingleQuote, $"{Quoter.Quote("id")} int", $"{Quoter.Quote("wibble")} int CONSTRAINT {Quoter.Quote("c1")} PRIMARY KEY");

            if (_quotingEnabled)
            {
                using (var table = TableCreationFunc())
                    Processor.ConstraintExists(TestSchemaSingleQuote, table.Name, "c1").ShouldBeTrue();
            }
            else
            {
                // Creating a table with single quote in schema name is a bad idea if identifier quoting is disabled.
                var ex = Assert.Throws<SnowflakeException>(() => TableCreationFunc());
                Assert.That(ex.Message, Does.StartWith("An error occurred executing the following sql"));
            }
        }

        [Test]
        public override void CallingIndexExistsCanAcceptSchemaNameWithSingleQuote()
        {
            Assert.Ignore("No index support.");
        }

        [Test]
        public override void CallingSchemaExistsCanAcceptSchemaNameWithSingleQuote()
        {
            SnowflakeTestTable TableCreationFunc() => new SnowflakeTestTable(Processor, TestSchemaSingleQuote, $"{Quoter.Quote("id")} int");

            if (_quotingEnabled)
            {
                using (TableCreationFunc())
                    Processor.SchemaExists(TestSchemaSingleQuote).ShouldBeTrue();
            }
            else
            {
                // Creating a table with single quote in schema name is a bad idea if identifier quoting is disabled.
                var ex = Assert.Throws<SnowflakeException>(() => TableCreationFunc());
                Assert.That(ex.Message, Does.StartWith("An error occurred executing the following sql"));
            }
        }

        [Test]
        public override void CallingTableExistsCanAcceptSchemaNameWithSingleQuote()
        {
            SnowflakeTestTable TableCreationFunc() => new SnowflakeTestTable(Processor, TestSchemaSingleQuote, $"{Quoter.Quote("id")} int");

            if (_quotingEnabled)
            {
                using (var table = TableCreationFunc())
                    Processor.TableExists(TestSchemaSingleQuote, table.Name).ShouldBeTrue();
            }
            else
            {
                // Creating a table with single quote in schema name is a bad idea if identifier quoting is disabled.
                var ex = Assert.Throws<SnowflakeException>(() => TableCreationFunc());
                Assert.That(ex.Message, Does.StartWith("An error occurred executing the following sql"));
            }
        }

        [Test]
        public void CallingDefaultValueExistsCanAcceptSchemaNameWithSingleQuote()
        {
            SnowflakeTestTable TableCreationFunc() => new SnowflakeTestTable(Processor, TestSchemaSingleQuote, $"{Quoter.Quote("id")} int DEFAULT 1");

            if (_quotingEnabled)
            {
                using (var table = TableCreationFunc())
                {
                    Processor.DefaultValueExists(TestSchemaSingleQuote, table.Name, "id", 1).ShouldBeTrue();
                    Processor.DefaultValueExists(TestSchemaSingleQuote, table.Name, "id", 2).ShouldBeFalse();
                }
            }
            else
            {
                // Creating a table with single quote in schema name is a bad idea if identifier quoting is disabled.
                var ex = Assert.Throws<SnowflakeException>(() => TableCreationFunc());
                Assert.That(ex.Message, Does.StartWith("An error occurred executing the following sql"));
            }
        }

        [OneTimeSetUp]
        public void ClassSetUp()
        {
            IntegrationTestOptions.Snowflake.IgnoreIfNotEnabled();

            var services = ServiceCollectionExtensions.CreateServices()
                .ConfigureRunner(r => r.AddSnowflake())
                .AddScoped<IConnectionStringReader>(
                    _ => new PassThroughConnectionStringReader(IntegrationTestOptions.Snowflake.ConnectionString))
                .AddScoped(_ => _quotingEnabled ? SnowflakeOptions.QuotingEnabled() : SnowflakeOptions.QuotingDisabled());
            ServiceProvider = services.BuildServiceProvider();
        }

        [OneTimeTearDown]
        public void ClassTearDown()
        {
            ServiceProvider?.Dispose();
        }

        [SetUp]
        public void SetUp()
        {
            ServiceScope = ServiceProvider.CreateScope();
            Processor = ServiceScope.ServiceProvider.GetRequiredService<SnowflakeProcessor>();
            Quoter = ServiceScope.ServiceProvider.GetRequiredService<SnowflakeQuoter>();
            if (_quotingEnabled) Processor.Execute($"drop schema if exists {Quoter.Quote(TestSchemaSingleQuote)}");
        }

        [TearDown]
        public void TearDown()
        {
            ServiceScope?.Dispose();
            Processor?.Dispose();
        }

        private static void EnsureReference()
        {
            // This is here to avoid the removal of the referenced assembly
            Debug.WriteLine(typeof(SnowflakeDbFactory));
        }
    }
}
