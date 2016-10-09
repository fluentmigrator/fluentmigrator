using System.Data;
using System.Linq;
using System.Collections.Generic;
using System;

using FluentMigrator.Runner.Announcers;
using FluentMigrator.Runner.Generators;
using FluentMigrator.Runner.Generators.Oracle;
using FluentMigrator.Runner.Processors;
using FluentMigrator.Runner.Processors.Oracle;
using FluentMigrator.Tests.Helpers;

using Xunit;

namespace FluentMigrator.Tests.Integration.Processors.Oracle {
	[Trait("Category", "Integration")]
	public abstract class OracleProcessorTestsBase
	{
		private const string SchemaName = "test";
		private IDbConnection Connection { get; set; }
		private OracleProcessor Processor { get; set; }
		private IDbFactory Factory { get; set; }
		private IQuoter Quoter { get { return this.Processor.Quoter; } }

		protected void SetUp(IDbFactory dbFactory)
		{
			this.Factory = dbFactory;
			this.Connection = this.Factory.CreateConnection(IntegrationTestOptions.Oracle.ConnectionString);
			this.Processor = new OracleProcessor(this.Connection, new OracleGenerator(), new TextWriterAnnouncer(System.Console.Out), new ProcessorOptions(), this.Factory);
			this.Connection.Open();
		}

		[TearDown]
		public void TearDown()
		{
			this.Processor.Dispose();
		}

		[Fact]
		public void CallingColumnExistsReturnsFalseIfColumnExistsInDifferentSchema()
		{
			using (var table = new OracleTestTable(this.Connection, SchemaName, this.Factory, "id int"))
				this.Processor.ColumnExists("testschema", table.Name, "ID").ShouldBeFalse();
		}

		[Fact]
		public void CallingConstraintExistsReturnsFalseIfConstraintExistsInDifferentSchema()
		{
			using (var table = new OracleTestTable(this.Connection, SchemaName, this.Factory, "id int"))
			{
				table.WithUniqueConstraintOn("ID");
				this.Processor.ConstraintExists("testschema", table.Name, "UC_id").ShouldBeFalse();
			}
		}

		[Fact]
		public void CallingTableExistsReturnsFalseIfTableExistsInDifferentSchema()
		{
			using (var table = new OracleTestTable(this.Connection, SchemaName, this.Factory, "id int"))
				this.Processor.TableExists("testschema", table.Name).ShouldBeFalse();
		}

		[Fact]
		public void CallingColumnExistsWithIncorrectCaseReturnsTrueIfColumnExists()
		{
			//the ColumnExisits() function is'nt case sensitive
			using (var table = new OracleTestTable(this.Connection, null, this.Factory, "id int"))
				this.Processor.ColumnExists(null, table.Name, "Id").ShouldBeTrue();
		}

		[Fact]
		public void CallingConstraintExistsWithIncorrectCaseReturnsTrueIfConstraintExists()
		{
			//the ConstraintExists() function is'nt case sensitive
			using (var table = new OracleTestTable(this.Connection, null, this.Factory, "id int"))
			{
				table.WithUniqueConstraintOn("ID", "uc_id");
				this.Processor.ConstraintExists(null, table.Name, "Uc_Id").ShouldBeTrue();
			}
		}

		[Fact]
		public void CallingIndexExistsWithIncorrectCaseReturnsFalseIfIndexExist()
		{
			//the IndexExists() function is'nt case sensitive
			using (var table = new OracleTestTable(this.Connection, null, this.Factory, "id int"))
			{
				table.WithIndexOn("ID", "ui_id");
				this.Processor.IndexExists(null, table.Name, "Ui_Id").ShouldBeTrue();
			}
		}

		[Fact]
		public void TestQuery()
		{
			string sql = "SELECT SYSDATE FROM " + this.Quoter.QuoteTableName("DUAL");
			var ds = new DataSet();
			using (var command = this.Factory.CreateCommand(sql, this.Connection))
			{
				var adapter = this.Factory.CreateDataAdapter(command);
				adapter.Fill(ds);
			}

			Assert.Greater(ds.Tables.Count, 0);
			Assert.Greater(ds.Tables[0].Columns.Count, 0);
		}
	}
}