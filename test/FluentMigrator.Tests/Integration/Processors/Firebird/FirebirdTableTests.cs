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

using FluentMigrator.Expressions;
using FluentMigrator.Model;
using FluentMigrator.Runner;
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
    public class FirebirdTableTests : BaseTableTests
    {
        private readonly FirebirdLibraryProber _prober = new FirebirdLibraryProber();
        private TemporaryDatabase _temporaryDatabase;

        private ServiceProvider ServiceProvider { get; set; }
        private IServiceScope ServiceScope { get; set; }
        private FirebirdProcessor Processor { get; set; }

        [Test]
        public override void CallingTableExistsCanAcceptTableNameWithSingleQuote()
        {
            using (var table = new FirebirdTestTable("\"Test'Table\"", Processor, "id int"))
                Processor.TableExists(null, table.Name).ShouldBeTrue();
        }

        [Test]
        public override void CallingTableExistsReturnsFalseIfTableDoesNotExist()
        {
            Processor.TableExists(null, "DoesNotExist").ShouldBeFalse();
        }

        [Test]
        public override void CallingTableExistsReturnsFalseIfTableDoesNotExistWithSchema()
        {
            Processor.TableExists("TestSchema", "DoesNotExist").ShouldBeFalse();
        }

        [Test]
        public override void CallingTableExistsReturnsTrueIfTableExists()
        {
            using (var table = new FirebirdTestTable(Processor, "id int"))
                Processor.TableExists(null, table.Name).ShouldBeTrue();
        }

        [Test]
        public override void CallingTableExistsReturnsTrueIfTableExistsWithSchema()
        {
            using (var table = new FirebirdTestTable(Processor, "id int"))
                Processor.TableExists("TestSchema", table.Name).ShouldBeTrue();
        }

        [Test]
        public void CreateTableIfNotExistsCreatesAQuotedTableWhenOnlyTheUnquotedOneExists()
        {
            // FMCASE and "FmCase" are different tables: a case-insensitive check would take one for the other and skip
            // the CREATE TABLE.
            Processor.Execute("CREATE TABLE FmCase (Id INTEGER)");
            var createTable = new CreateTableExpression { TableName = "\"FmCase\"", IfNotExists = true };
            createTable.Columns.Add(new ColumnDefinition { Name = "Id", Type = DbType.Int32, TableName = createTable.TableName });

            Processor.Process(createTable);

            Processor.Exists("select 1 from rdb$relations where rdb$relation_name = 'FMCASE'").ShouldBeTrue();
            Processor.Exists("select 1 from rdb$relations where rdb$relation_name = 'FmCase'").ShouldBeTrue();
        }

        [Test]
        public void CreateTableIfNotExistsKeepsAnExistingTableOfTheSameStoredName()
        {
            Processor.Execute("CREATE TABLE FmKept (Id INTEGER, Marker INTEGER)");
            var createTable = new CreateTableExpression { TableName = "FmKept", IfNotExists = true };
            createTable.Columns.Add(new ColumnDefinition { Name = "Id", Type = DbType.Int32, TableName = createTable.TableName });

            Processor.Process(createTable);

            Processor.ColumnExists(null, "FmKept", "Marker").ShouldBeTrue();
        }

        [SetUp]
        public void SetUp()
        {
            IntegrationTestOptions.Firebird.IgnoreIfNotEnabled();

            var services = FbDatabase.CreateFirebirdServices(_prober, out _temporaryDatabase);

            ServiceProvider = services.BuildServiceProvider();
            ServiceScope = ServiceProvider.CreateScope();
            Processor = ServiceScope.ServiceProvider.GetRequiredService<FirebirdProcessor>();
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
