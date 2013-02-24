using System.Data;
using FluentMigrator.Runner.Processors.Oracle;
using FluentMigrator.Runner.Announcers;
using FluentMigrator.Runner.Generators.Oracle;
using FluentMigrator.Tests.Helpers;
using FluentMigrator.Runner.Processors;
using FluentMigrator.Runner.Generators;
using NUnit.Framework;
using NUnit.Should;

namespace FluentMigrator.Tests.Integration.Processors
{
    [TestFixture]
    [Category("Integration")]
    public class OracleProcessorTests
    {
        private const string SchemaName = "test";
        public IDbConnection Connection { get; set; }
        public OracleProcessor Processor { get; set; }
        public OracleDbFactory Factory { get; set; }
        public IQuoter Quoter { get; set; }

        [SetUp]
        public void SetUp()
        {
            Factory = new OracleDbFactory();
            Connection = Factory.CreateConnection(IntegrationTestOptions.Oracle.ConnectionString);
            Quoter = new OracleQuoter();
            Processor = new OracleProcessor(Connection, new OracleGenerator(), new TextWriterAnnouncer(System.Console.Out), new ProcessorOptions(), Factory);
            Connection.Open();
        }

        [TearDown]
        public void TearDown()
        {
            Processor.Dispose();
        }

        #region When working with Table

        [Test]
        public void CallingTableExistsReturnsTrueIfTableExists()
        {
            using (var table = new OracleTestTable(Connection, Quoter, null, Quoter.QuoteColumnName("id") + " int"))
                Processor.TableExists(null, table.Name).ShouldBeTrue();
        }

        [Test]
        public void CallingTableExistsReturnsFalseIfTableDoesNotExist()
        {
            Processor.TableExists(null, "DoesNotExist").ShouldBeFalse();
        }

        [Test]
        public void CallingTableExistsReturnsTrueIfTableExistsWithSchema()
        {
            using (var table = new OracleTestTable(Connection, Quoter, SchemaName, Quoter.QuoteColumnName("id") + " int"))
                Processor.TableExists(SchemaName, table.Name).ShouldBeTrue();
        }

        [Test]
        public void CallingTableExistsReturnsFalseIfTableDoesNotExistWithSchema()
        {
            Processor.TableExists(SchemaName, "DoesNotExist").ShouldBeFalse();
        }

        #endregion

        #region When working with Column

        [Test]
        public void CallingColumnExistsReturnsTrueIfColumnExists()
        {
            using (var table = new OracleTestTable(Connection, Quoter, null, Quoter.QuoteColumnName("id") + " int"))
                Processor.ColumnExists(null, table.Name, "id").ShouldBeTrue();
        }

        [Test]
        public void CallingColumnExistsWithIncorrectCaseReturnsFalseIfColumnExists()
        {
            using (var table = new OracleTestTable(Connection, Quoter, null, Quoter.QuoteColumnName("id") + " int"))
                Processor.ColumnExists(null, table.Name, "ID").ShouldBeFalse();
        }

        [Test]
        public void CallingColumnExistsReturnsFalseIfTableDoesNotExist()
        {
            Processor.ColumnExists(null, "DoesNotExist", "DoesNotExist").ShouldBeFalse();
        }

        [Test]
        public void CallingColumnExistsReturnsFalseIfColumnDoesNotExist()
        {
            using (var table = new OracleTestTable(Connection, Quoter, null, Quoter.QuoteColumnName("id") + " int"))
                Processor.ColumnExists(null, table.Name, "DoesNotExist").ShouldBeFalse();
        }

        [Test]
        public void CallingColumnExistsReturnsTrueIfColumnExistsWithSchema()
        {
            using (var table = new OracleTestTable(Connection, Quoter, SchemaName, Quoter.QuoteColumnName("id") + " int"))
                Processor.ColumnExists(SchemaName, table.Name, "id").ShouldBeTrue();
        }

        [Test]
        public void CallingColumnExistsReturnsFalseIfTableDoesNotExistWithSchema()
        {
            Processor.ColumnExists(SchemaName, "DoesNotExist", "DoesNotExist").ShouldBeFalse();
        }

        [Test]
        public void CallingColumnExistsReturnsFalseIfColumnDoesNotExistWithSchema()
        {
            using (var table = new OracleTestTable(Connection, Quoter, SchemaName, Quoter.QuoteColumnName("id") + " int"))
                Processor.ColumnExists(SchemaName, table.Name, "DoesNotExist").ShouldBeFalse();
        }

        #endregion

        #region When working with Constraints

        [Test]
        public void CallingConstraintExistsReturnsTrueIfConstraintExist()
        {
            using (var table = new OracleTestTable(Connection, Quoter, null, Quoter.QuoteColumnName("id") + " int"))
            {
                table.WithUniqueConstraintOn("id");
                Processor.ConstraintExists(null, table.Name, "UC_id").ShouldBeTrue();
            }
        }

        [Test]
        public void CallingConstraintExistsReturnsFalseIfConstraintDoesNotExist()
        {
            using (var table = new OracleTestTable(Connection, Quoter, null, Quoter.QuoteColumnName("id") + " int"))
            {
                table.WithUniqueConstraintOn("id");
                Processor.ConstraintExists(null, table.Name, "DoesNotExist").ShouldBeFalse();
            }
        }

        [Test]
        public void CallingConstraintExistsWithIncorrectCaseReturnsFalseIfConstraintExists()
        {
            using (var table = new OracleTestTable(Connection, Quoter, null, Quoter.QuoteColumnName("id") + " int"))
            {
                table.WithUniqueConstraintOn("id");
                Processor.ConstraintExists(null, table.Name, "UC_ID").ShouldBeFalse();
            }
        }

        [Test]
        public void CallingConstraintExistsReturnsTrueIfConstraintExistWithSchema()
        {
            using (var table = new OracleTestTable(Connection, Quoter, SchemaName, Quoter.QuoteColumnName("id") + " int"))
            {
                table.WithUniqueConstraintOn("id");
                Processor.ConstraintExists(SchemaName, table.Name, "UC_id").ShouldBeTrue();
            }
        }

        [Test]
        public void CallingConstraintExistsReturnsFalseIfConstraintDoesNotExistWithSchema()
        {
            using (var table = new OracleTestTable(Connection, Quoter, SchemaName, Quoter.QuoteColumnName("id") + " int"))
            {
                table.WithUniqueConstraintOn("id");
                Processor.ConstraintExists(SchemaName, table.Name, "DoesNotExist").ShouldBeFalse();
            }
        }

        #endregion

        #region When working with Indexes

        [Test]
        public void CallingIndexExistsReturnsTrueIfIndexExist()
        {
            using (var table = new OracleTestTable(Connection, Quoter, null, Quoter.QuoteColumnName("id") + " int"))
            {
                table.WithIndexOn("id");
                Processor.IndexExists(null, table.Name, "UI_id").ShouldBeTrue();
            }
        }

        [Test]
        public void CallingIndexExistsReturnsFalseIfIndexDoesNotExist()
        {
            using (var table = new OracleTestTable(Connection, Quoter, null, Quoter.QuoteColumnName("id") + " int"))
            {
                table.WithIndexOn("id");
                Processor.IndexExists(null, table.Name, "DoesNotExist").ShouldBeFalse();
            }
        }

        [Test]
        public void CallingIndexExistsWithIncorrectCaseReturnsFalseIfIndexExist()
        {
            using (var table = new OracleTestTable(Connection, Quoter, null, Quoter.QuoteColumnName("id") + " int"))
            {
                table.WithIndexOn("id");
                Processor.IndexExists(null, table.Name, "UI_ID").ShouldBeFalse();
            }
        }

        [Test]
        public void CallingIndexExistsReturnsTrueIfIndexExistWithSchema()
        {
            using (var table = new OracleTestTable(Connection, Quoter, SchemaName, Quoter.QuoteColumnName("id") + " int"))
            {
                table.WithIndexOn("id");
                Processor.IndexExists(SchemaName, table.Name, "UI_id").ShouldBeTrue();
            }
        }

        [Test]
        public void CallingIndexExistsReturnsFalseIfIndexDoesNotExistWithSchema()
        {
            using (var table = new OracleTestTable(Connection, Quoter, SchemaName, Quoter.QuoteColumnName("id") + " int"))
            {
                table.WithIndexOn("id");
                Processor.IndexExists(SchemaName, table.Name, "DoesNotExist").ShouldBeFalse();
            }
        }

        #endregion

        #region Schema

        [Test]
        public void CallingSchemaExistsReturnsTrueIfSchemaExist()
        {
            Processor.SchemaExists(SchemaName).ShouldBeTrue();
        }

        [Test]
        public void CallingSchemaExistsReturnsFalseIfSchemaDoesNotExis()
        {
            Processor.SchemaExists("DoesNotExist").ShouldBeFalse();
        }

        #endregion

        [Test]
        public void TestQuery()
        {
            string sql = "SELECT SYSDATE FROM " + Quoter.QuoteTableName("DUAL");
            DataSet ds = new DataSet();
            using (var command = Factory.CreateCommand(sql, Connection))
            {
                var adapter = Factory.CreateDataAdapter(command);
                adapter.Fill(ds);
            }

            Assert.Greater(ds.Tables.Count, 0);
            Assert.Greater(ds.Tables[0].Columns.Count, 0);
        }
    }
}
