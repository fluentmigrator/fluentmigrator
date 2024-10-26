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
    public class SnowflakeProcessorTests
    {
        private ServiceProvider ServiceProvider { get; set; }
        private IServiceScope ServiceScope { get; set; }
        private SnowflakeProcessor Processor { get; set; }
        private SnowflakeQuoter Quoter { get; set; }
        private const string TestSchema1 = "TestSchema1";
        private const string TestSchema2 = "TestSchema2";

        private readonly bool _quotingEnabled;

        public SnowflakeProcessorTests(bool quotingEnabled)
        {
            _quotingEnabled = quotingEnabled;
            try { EnsureReference(); } catch { /* ignore */ }
        }

        [Test]
        public void CallingColumnExistsReturnsFalseIfColumnExistsInDifferentSchema()
        {
            using (var table = new SnowflakeTestTable(Processor, "TestSchema1", $"{Quoter.Quote("id")} int"))
            {
                Processor.ColumnExists(TestSchema1, table.Name, "id").ShouldBeTrue();
                Processor.ColumnExists(TestSchema2, table.Name, "id").ShouldBeFalse();
            }
        }

        [Test]
        public void CallingConstraintExistsReturnsFalseIfConstraintExistsInDifferentSchema()
        {
            using (var table = new SnowflakeTestTable(
                Processor,
                TestSchema1,
                $"{Quoter.Quote("id")} int",
                $"{Quoter.Quote("wibble")} int CONSTRAINT {Quoter.Quote("c1")} PRIMARY KEY"))
            {
                Processor.ConstraintExists(TestSchema1, table.Name, "c1").ShouldBeTrue();
                Processor.ConstraintExists(TestSchema2, table.Name, "c1").ShouldBeFalse();
            }
        }

        [Test]
        public void CallingTableExistsReturnsFalseIfTableExistsInDifferentSchema()
        {
            using (var table = new SnowflakeTestTable(Processor, TestSchema1, $"{Quoter.Quote("id")} int"))
            {
                Processor.TableExists(TestSchema1, table.Name).ShouldBeTrue();
                Processor.TableExists(TestSchema2, table.Name).ShouldBeFalse();
            }
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
            Processor.Execute($"drop schema if exists {Quoter.Quote(TestSchema1)}");
            Processor.Execute($"drop schema if exists {Quoter.Quote(TestSchema2)}");
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
