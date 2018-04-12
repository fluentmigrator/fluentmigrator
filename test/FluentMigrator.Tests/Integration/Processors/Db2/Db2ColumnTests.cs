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

using System.Diagnostics;

using FluentMigrator.Runner.Announcers;
using FluentMigrator.Runner.Generators.DB2;
using FluentMigrator.Runner.Processors;
using FluentMigrator.Runner.Processors.DB2;
using FluentMigrator.Tests.Helpers;

using NUnit.Framework;
using NUnit.Should;

namespace FluentMigrator.Tests.Integration.Processors.Db2
{
    [TestFixture]
    [Category("Db2")]
    public class Db2ColumnTests : BaseColumnTests
    {
        static Db2ColumnTests()
        {
            try { EnsureReference(); } catch { /* ignore */ }
        }

        #region Properties

        public System.Data.IDbConnection Connection
        {
            get;
            set;
        }

        public Db2DbFactory Factory
        {
            get;
            set;
        }

        public Db2Processor Processor
        {
            get;
            set;
        }

        public Db2Quoter Quoter
        {
            get;
            set;
        }

        #endregion Properties

        #region Methods

        [Test]
        public override void CallingColumnExistsCanAcceptColumnNameWithSingleQuote()
        {
            var columnName = Quoter.Quote("I'D") + " INT";
            using (var table = new Db2TestTable(Processor, null, columnName))
            {
                Processor.ColumnExists(null, table.Name, "I'D").ShouldBeTrue();
            }
        }

        [Test]
        public override void CallingColumnExistsCanAcceptTableNameWithSingleQuote()
        {
            using (var table = new Db2TestTable("Test'Table", Processor, null, "ID INT"))
            {
                Processor.ColumnExists(null, table.Name, "ID").ShouldBeTrue();
            }
        }

        [Test]
        public override void CallingColumnExistsReturnsFalseIfColumnDoesNotExist()
        {
            using (var table = new Db2TestTable(Processor, null, "ID INT"))
            {
                Processor.ColumnExists(null, table.Name, "DoesNotExist").ShouldBeFalse();
            }
        }

        [Test]
        public override void CallingColumnExistsReturnsFalseIfColumnDoesNotExistWithSchema()
        {
            using (var table = new Db2TestTable(Processor, "TstSchma", "ID INT"))
            {
                Processor.ColumnExists("TstSchma", table.Name, "DoesNotExist").ShouldBeFalse();
            }
        }

        [Test]
        public override void CallingColumnExistsReturnsFalseIfTableDoesNotExist()
        {
            Processor.ColumnExists(null, "DoesNotExist", "DoesNotExist").ShouldBeFalse();
        }

        [Test]
        public override void CallingColumnExistsReturnsFalseIfTableDoesNotExistWithSchema()
        {
            Processor.ColumnExists("TstSchma", "DoesNotExist", "DoesNotExist").ShouldBeFalse();
        }

        [Test]
        public override void CallingColumnExistsReturnsTrueIfColumnExists()
        {
            using (var table = new Db2TestTable(Processor, null, "ID INT"))
            {
                Processor.ColumnExists(null, table.Name, "ID").ShouldBeTrue();
            }
        }

        [Test]
        public override void CallingColumnExistsReturnsTrueIfColumnExistsWithSchema()
        {
            using (var table = new Db2TestTable(Processor, "TstSchma", "ID INT"))
            {
                Processor.ColumnExists("TstSchma", table.Name, "ID").ShouldBeTrue();
            }
        }

        [SetUp]
        public void SetUp()
        {
            if (!IntegrationTestOptions.Db2.IsEnabled)
                Assert.Ignore();
            Factory = new Db2DbFactory();
            Connection = Factory.CreateConnection(IntegrationTestOptions.Db2.ConnectionString);
            Quoter = new Db2Quoter();
            Processor = new Db2Processor(Connection, new Db2Generator(Quoter), new TextWriterAnnouncer(TestContext.Out), new ProcessorOptions(), Factory);
            Connection.Open();
        }

        [TearDown]
        public void TearDown()
        {
            Processor?.Dispose();
        }

        private static void EnsureReference()
        {
            // This is here to avoid the removal of the referenced assembly
            Debug.WriteLine(typeof(IBM.Data.DB2.DB2Factory));
        }

        #endregion Methods
    }
}
