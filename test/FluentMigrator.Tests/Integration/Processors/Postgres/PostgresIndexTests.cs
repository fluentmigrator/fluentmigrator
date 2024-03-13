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

using FluentMigrator.Runner;
using FluentMigrator.Runner.Generators.Postgres;
using FluentMigrator.Runner.Initialization;
using FluentMigrator.Runner.Processors.Postgres;
using FluentMigrator.Tests.Helpers;

using Microsoft.Extensions.DependencyInjection;

using NUnit.Framework;

using Shouldly;

namespace FluentMigrator.Tests.Integration.Processors.Postgres
{
    [TestFixture]
    [Category("Integration")]
    [Category("Postgres")]
    public class PostgresIndexTests : BaseIndexTests
    {
        private ServiceProvider ServiceProvider { get; set; }
        private IServiceScope ServiceScope { get; set; }
        private PostgresProcessor Processor { get; set; }
        private PostgresQuoter Quoter { get; set; }

        [Test]
        public override void CallingIndexExistsCanAcceptIndexNameWithSingleQuote()
        {
            using (var table = new PostgresTestTable(Processor, null, "id int"))
            {
                var idxName = $"\"id'x_{Quoter.UnQuote(table.Name)}\"";

                var cmd = table.Connection.CreateCommand();
                cmd.Transaction = table.Transaction;
                cmd.CommandText = $"CREATE INDEX {idxName} ON \"{table.Name}\" (id)";
                cmd.ExecuteNonQuery();

                Processor.IndexExists(null, table.Name, idxName).ShouldBeTrue();
            }
        }

        [Test]
        public override void CallingIndexExistsCanAcceptTableNameWithSingleQuote()
        {
            using (var table = new PostgresTestTable("Test'Table", Processor, null, "id int"))
            {
                var idxName = $"\"idx_{Quoter.UnQuote(table.Name)}\"";

                var cmd = table.Connection.CreateCommand();
                cmd.Transaction = table.Transaction;
                cmd.CommandText = $"CREATE INDEX {idxName} ON \"{table.Name}\" (id)";
                cmd.ExecuteNonQuery();

                Processor.IndexExists(null, table.Name, idxName).ShouldBeTrue();
            }
        }

        [Test]
        public override void CallingIndexExistsReturnsFalseIfIndexDoesNotExist()
        {
            using (var table = new PostgresTestTable(Processor, null, "id int"))
                Processor.IndexExists(null, table.Name, "DoesNotExist").ShouldBeFalse();
        }

        [Test]
        public override void CallingIndexExistsReturnsFalseIfIndexDoesNotExistWithSchema()
        {
            using (var table = new PostgresTestTable(Processor, "TestSchema", "id int"))
                Processor.IndexExists("TestSchema", table.Name, "DoesNotExist").ShouldBeFalse();
        }

        [Test]
        public override void CallingIndexExistsReturnsFalseIfTableDoesNotExist()
        {
            Processor.IndexExists(null, "DoesNotExist", "DoesNotExist").ShouldBeFalse();
        }

        [Test]
        public override void CallingIndexExistsReturnsFalseIfTableDoesNotExistWithSchema()
        {
            Processor.IndexExists("TestSchema", "DoesNotExist", "DoesNotExist").ShouldBeFalse();
        }

        [Test]
        public override void CallingIndexExistsReturnsTrueIfIndexExists()
        {
            using (var table = new PostgresTestTable(Processor, null, "id int"))
            {
                var idxName = $"\"idx_{Quoter.UnQuote(table.Name)}\"";

                var cmd = table.Connection.CreateCommand();
                cmd.Transaction = table.Transaction;
                cmd.CommandText = $"CREATE INDEX {idxName} ON \"{table.Name}\" (id)";
                cmd.ExecuteNonQuery();

                Processor.IndexExists(null, table.Name, idxName).ShouldBeTrue();
            }
        }

        [Test]
        public override void CallingIndexExistsReturnsTrueIfIndexExistsWithSchema()
        {
            using (var table = new PostgresTestTable(Processor, "TestSchema", "id int"))
            {
                var idxName = $"\"idx_{Quoter.UnQuote(table.Name)}\"";

                var cmd = table.Connection.CreateCommand();
                cmd.Transaction = table.Transaction;
                cmd.CommandText = $"CREATE INDEX {idxName} ON {table.NameWithSchema} (id)";
                cmd.ExecuteNonQuery();

                Processor.IndexExists("TestSchema", table.Name, idxName).ShouldBeTrue();
            }
        }

        [OneTimeSetUp]
        public void ClassSetUp()
        {
            if (!IntegrationTestOptions.Postgres.IsEnabled)
                Assert.Ignore();

            var serivces = ServiceCollectionExtensions.CreateServices()
                .ConfigureRunner(r => r.AddPostgres())
                .AddScoped<IConnectionStringReader>(
                    _ => new PassThroughConnectionStringReader(IntegrationTestOptions.Postgres.ConnectionString));
            ServiceProvider = serivces.BuildServiceProvider();
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
            Processor = ServiceScope.ServiceProvider.GetRequiredService<PostgresProcessor>();
            Quoter = ServiceScope.ServiceProvider.GetRequiredService<PostgresQuoter>();
        }

        [TearDown]
        public void TearDown()
        {
            ServiceScope?.Dispose();
            Processor?.Dispose();
        }
    }
}
