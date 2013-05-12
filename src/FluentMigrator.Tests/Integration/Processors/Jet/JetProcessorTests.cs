#region License
// Copyright (c) 2007-2009, Sean Chambers <schambers80@gmail.com>
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

using System.Data;
using System.Data.OleDb;
using FluentMigrator.Runner.Announcers;
using FluentMigrator.Runner.Generators.Jet;
using FluentMigrator.Runner.Processors;
using FluentMigrator.Runner.Processors.Jet;
using FluentMigrator.Tests.Helpers;
using NUnit.Framework;
using NUnit.Should;

namespace FluentMigrator.Tests.Integration.Processors.Jet
{
    [TestFixture]
    [Category("Integration")]
    public class JetProcessorTests
    {
        public OleDbConnection Connection { get; set; }
        public JetProcessor Processor { get; set; }
        [SetUp]
        public void SetUp()
        {
            Connection = new OleDbConnection(IntegrationTestOptions.Jet.ConnectionString);
            Processor = new JetProcessor(Connection, new JetGenerator(), new TextWriterAnnouncer(System.Console.Out), new ProcessorOptions());
            Connection.Open();
        }

        [Test]
        public void CallingTableExistsReturnsFalseIfTableDoesNotExist()
        {
            Processor.TableExists(null, "DoesNotExist").ShouldBeFalse();
        }

        [Test]
        public void CallingTableExistsReturnsTrueIfTableExists()
        {
            using (var table = new JetTestTable(Processor, "id int"))
                Processor.TableExists(null, table.Name).ShouldBeTrue();
        }

        [Test]
        public void CallingColumnExistsReturnsTrueIfColumnExists()
        {
            using (var table = new JetTestTable(Processor, "id int"))
                Processor.ColumnExists(null, table.Name, "id").ShouldBeTrue();
        }

        [Test]
        public void CallingColumnExistsReturnsFalseIfTableDoesNotExist()
        {
            Processor.ColumnExists(null, "DoesNotExist", "DoesNotExist").ShouldBeFalse();
        }

        [Test]
        public void CallingColumnExistsReturnsFalseIfColumnDoesNotExist()
        {
            using (var table = new JetTestTable(Processor, "id int"))
                Processor.ColumnExists(null, table.Name, "DoesNotExist").ShouldBeFalse();
        }

        [Test]
        public void CanReadData()
        {
            using (var table = new JetTestTable(Processor, "id int"))
            {
                AddTestData(table);

                DataSet ds = Processor.Read("SELECT * FROM {0}", table.Name);

                ds.ShouldNotBeNull();
                ds.Tables.Count.ShouldBe(1);
                ds.Tables[0].Rows.Count.ShouldBe(3);
                ds.Tables[0].Rows[2][0].ShouldBe(2);
            }
        }

        [Test]
        public void CanReadTableData()
        {
            using (var table = new JetTestTable(Processor, "id int"))
            {
                AddTestData(table);

                DataSet ds = Processor.ReadTableData(null, table.Name);

                ds.ShouldNotBeNull();
                ds.Tables.Count.ShouldBe(1);
                ds.Tables[0].Rows.Count.ShouldBe(3);
                ds.Tables[0].Rows[2][0].ShouldBe(2);
            }
        }

        private void AddTestData(JetTestTable table)
        {
            for (int i = 0; i < 3; i++)
            {
                var cmd = table.Connection.CreateCommand();
                cmd.Transaction = table.Transaction;
                cmd.CommandText = string.Format("INSERT INTO {0} (id) VALUES ({1})", table.Name, i);
                cmd.ExecuteNonQuery();
            }
        }

        [TearDown]
        public void TearDown()
        {
            Processor.CommitTransaction();
            Processor.Dispose();
        }
    }
}