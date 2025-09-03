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

using System.Data;

using FluentMigrator.Runner;
using FluentMigrator.Generation;
using FluentMigrator.Runner.Generators.Firebird;
using FluentMigrator.Runner.Initialization;
using FluentMigrator.Runner.Processors.Firebird;
using FluentMigrator.Tests.Helpers;

using Microsoft.Extensions.DependencyInjection;

using NUnit.Framework;

using Shouldly;

namespace FluentMigrator.Tests.Integration.Processors.Firebird
{
    [TestFixture]
    [Category("Integration")]
    [Category("Firebird")]
    public class FirebirdIndexTests : BaseIndexTests
    {
        private readonly FirebirdLibraryProber _prober = new FirebirdLibraryProber();
        private TemporaryDatabase _temporaryDatabase;

        private ServiceProvider ServiceProvider { get; set; }
        private IServiceScope ServiceScope { get; set; }
        private FirebirdProcessor Processor { get; set; }
        private IQuoter Quoter { get; set; }

        [Test]
        public override void CallingIndexExistsCanAcceptIndexNameWithSingleQuote()
        {
            using (var table = new FirebirdTestTable(Processor, "id int"))
            {
                Processor.CheckTable(table.Name);
                Processor.LockTable(table.Name);
                var idxName = $"\"id'x_{table.Name}\"";

                if (table.Connection.State != ConnectionState.Open)
                    table.Connection.Open();

                using (var cmd = table.Connection.CreateCommand())
                {
                    cmd.Transaction = table.Transaction;
                    cmd.CommandText = $"CREATE INDEX {idxName} ON {table.Name} (id)";
                    cmd.ExecuteNonQuery();
                }

                Processor.AutoCommit();

                Processor.IndexExists(null, table.Name, idxName).ShouldBeTrue();
            }
        }

        [Test]
        public override void CallingIndexExistsCanAcceptTableNameWithSingleQuote()
        {
            using (var table = new FirebirdTestTable("\"Test'Table\"", Processor, "id int"))
            {
                Processor.CheckTable(table.Name);
                Processor.LockTable(table.Name);
                var idxName = "\"idx_Test'Table\"";

                if (table.Connection.State != ConnectionState.Open)
                    table.Connection.Open();

                using (var cmd = table.Connection.CreateCommand())
                {
                    cmd.Transaction = table.Transaction;
                    cmd.CommandText = $"CREATE INDEX {idxName} ON {table.Name} (id)";
                    cmd.ExecuteNonQuery();
                }

                Processor.AutoCommit();

                Processor.IndexExists(null, table.Name, idxName).ShouldBeTrue();
            }
        }

        [Test]
        public override void CallingIndexExistsReturnsFalseIfIndexDoesNotExist()
        {
            using (var table = new FirebirdTestTable(Processor, "id int"))
                Processor.IndexExists(null, table.Name, "DoesNotExist").ShouldBeFalse();
        }

        [Test]
        public override void CallingIndexExistsReturnsFalseIfIndexDoesNotExistWithSchema()
        {
            using (var table = new FirebirdTestTable(Processor, "id int"))
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
            using (var table = new FirebirdTestTable(Processor, "id int"))
            {
                Processor.CheckTable(table.Name);
                Processor.LockTable(table.Name);
                var idxName = $"idx_{table.Name}";

                if (table.Connection.State != ConnectionState.Open)
                    table.Connection.Open();

                using (var cmd = table.Connection.CreateCommand())
                {
                    cmd.Transaction = table.Transaction;
                    cmd.CommandText = $"CREATE INDEX {Quoter.QuoteIndexName(idxName)} ON {Quoter.QuoteTableName(table.Name)} (id)";
                    cmd.ExecuteNonQuery();
                }

                Processor.AutoCommit();

                Processor.IndexExists(null, table.Name, idxName).ShouldBeTrue();
            }
        }

        [Test]
        public override void CallingIndexExistsReturnsTrueIfIndexExistsWithSchema()
        {
            using (var table = new FirebirdTestTable(Processor, "id int"))
            {
                Processor.CheckTable(table.Name);
                Processor.LockTable(table.Name);
                var idxName = $"idx_{table.Name}";

                if (table.Connection.State != ConnectionState.Open)
                    table.Connection.Open();

                using (var cmd = table.Connection.CreateCommand())
                {
                    cmd.Transaction = table.Transaction;
                    cmd.CommandText = $"CREATE INDEX {Quoter.QuoteIndexName(idxName)} ON {Quoter.QuoteTableName(table.Name)} (id)";
                    cmd.ExecuteNonQuery();
                }

                Processor.AutoCommit();

                Processor.IndexExists("TestSchema", table.Name, idxName).ShouldBeTrue();
            }
        }

        [SetUp]
        public void SetUp()
        {
            IntegrationTestOptions.Firebird.IgnoreIfNotEnabled();

            var services = FbDatabase.CreateFirebirdServices(_prober, out _temporaryDatabase);

            ServiceProvider = services.BuildServiceProvider();
            ServiceScope = ServiceProvider.CreateScope();
            Processor = ServiceScope.ServiceProvider.GetRequiredService<FirebirdProcessor>();
            Quoter = ServiceScope.ServiceProvider.GetRequiredService<FirebirdQuoter>();
        }

        [TearDown]
        public void TearDown()
        {
            ServiceScope?.Dispose();
            ServiceProvider?.Dispose();
            Processor?.Dispose();
            _temporaryDatabase?.Dispose();
        }
    }
}
