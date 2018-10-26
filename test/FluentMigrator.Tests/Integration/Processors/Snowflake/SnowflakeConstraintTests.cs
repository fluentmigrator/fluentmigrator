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
    public class SnowflakeConstraintTests : BaseConstraintTests
    {
        private ServiceProvider ServiceProvider { get; set; }
        private IServiceScope ServiceScope { get; set; }
        private SnowflakeProcessor Processor { get; set; }
        private SnowflakeQuoter Quoter { get; set; }
        private const string TestSchema = "test_schema";

        private readonly bool _quotingEnabled;

        public SnowflakeConstraintTests(bool quotingEnabled)
        {
            _quotingEnabled = quotingEnabled;
            try { EnsureReference(); } catch { /* ignore */ }
        }

        [Test]
        public override void CallingConstraintExistsCanAcceptConstraintNameWithSingleQuote()
        {
            var constraintName = Quoter.QuoteConstraintName("c'1");
            SnowflakeTestTable TableCreationFunc() => new SnowflakeTestTable(Processor, TestSchema, $"{Quoter.Quote("id")} int",
                $"{Quoter.Quote("wibble")} int CONSTRAINT {constraintName} PRIMARY KEY");

            if (_quotingEnabled)
            {
                // Snowflake does not support check constraints, trying with PK instead.
                using (var table = TableCreationFunc())
                    Processor.ConstraintExists(TestSchema, table.Name, "c'1").ShouldBeTrue();
            }
            else
            {
                // Creating a table with single quote is a bad idea if identifier quoting is disabled.
                Assert.Throws<InvalidOperationException>(() => TableCreationFunc());
            }
        }

        [Test]
        public override void CallingConstraintExistsCanAcceptTableNameWithSingleQuote()
        {
            SnowflakeTestTable TableCreationFunc() => new SnowflakeTestTable("Test'Table", Processor, TestSchema, $"{Quoter.Quote("id")} int", $"{Quoter.Quote("wibble")} int CONSTRAINT {Quoter.Quote("c1")} PRIMARY KEY");

            if (_quotingEnabled)
            {
                // Snowflake does not support check constraints, trying with PK instead.
                using (var table = TableCreationFunc())
                    Processor.ConstraintExists(TestSchema, table.Name, "c1").ShouldBeTrue();
            }
            else
            {
                // Creating a table with single quote is a bad idea if identifier quoting is disabled.
                Assert.Throws<InvalidOperationException>(() => TableCreationFunc());
            }
        }

        [Test]
        public override void CallingConstraintExistsReturnsFalseIfConstraintDoesNotExist()
        {
            Assert.Ignore("No default schema support.");
        }

        [Test]
        public override void CallingConstraintExistsReturnsFalseIfConstraintDoesNotExistWithSchema()
        {
            using (var table = new SnowflakeTestTable(Processor, TestSchema, $"{Quoter.Quote("id")} int"))
                Processor.ConstraintExists(TestSchema, table.Name, "DoesNotExist").ShouldBeFalse();
        }

        [Test]
        public override void CallingConstraintExistsReturnsFalseIfTableDoesNotExist()
        {
            Assert.Ignore("No default schema support.");
        }

        [Test]
        public override void CallingConstraintExistsReturnsFalseIfTableDoesNotExistWithSchema()
        {
            Processor.ConstraintExists(TestSchema, "DoesNotExist", "DoesNotExist").ShouldBeFalse();
        }

        [Test]
        public override void CallingConstraintExistsReturnsTrueIfConstraintExists()
        {
            Assert.Ignore("No default schema support.");
        }

        [Test]
        public override void CallingConstraintExistsReturnsTrueIfConstraintExistsWithSchema()
        {
            // Snowflake does not support check constraints, trying with PK instead.
            using (var table = new SnowflakeTestTable(Processor, TestSchema, $"{Quoter.Quote("id")} int", $"{Quoter.Quote("wibble")} int CONSTRAINT {Quoter.Quote("c1")} PRIMARY KEY"))
                Processor.ConstraintExists(TestSchema, table.Name, "c1").ShouldBeTrue();
        }

        [OneTimeSetUp]
        public void ClassSetUp()
        {
            if (!IntegrationTestOptions.Snowflake.IsEnabled)
                Assert.Ignore();

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
            Processor.Execute($"drop schema if exists {Quoter.Quote("TestSchema")}");
        }

        [TearDown]
        public void TearDown()
        {
            ServiceScope?.Dispose();
        }

        private static void EnsureReference()
        {
            // This is here to avoid the removal of the referenced assembly
            Debug.WriteLine(typeof(SnowflakeDbFactory));
        }
    }
}
